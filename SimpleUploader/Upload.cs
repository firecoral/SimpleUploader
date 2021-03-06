using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using DigiProofs.JSONUploader;
using DigiProofs.Logger;


namespace UploadExpress {
    /// <summary>
    /// Summary description for Upload.
    /// </summary>
    public class Upload : System.Windows.Forms.Form {
        private ArrayList uploadSets;
        private Account account;
        private Queue work;
        private UploadSet curUploadSet = null;
        private LogList log;    // application logger
        private bool cancelling;

        // Keep count of the total errors encountered and the number of contiguous
        // errors.
        private int totalErrors;
        private int contiguousErrors;
        // We keep track of the number of files uploaded and the number that had errors
        // in order to report on them at the end of the upload.
        private int uploadedFiles;
        private int errorFiles;

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button CloseBtn;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.TextBox textBox1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public Upload(LogList log) {
            this.log = log;
            InitializeComponent();
        }

        public bool Setup(Account account) {
            this.account = account;
            work = new Queue();
            uploadSets = new ArrayList();
            long pendingSize = 0;
            totalErrors = 0;
            contiguousErrors = 0;
            uploadedFiles = 0;
            errorFiles = 0;
            foreach (UploadSet uploadSet in account.UploadSetList) {
                if (uploadSet.Status == UploadStatus.Ready) {
                    // XXX if an upload contains more than 4gb, we could have a problem here.
                    // At this point I'm focused on accuracy so don't want to round.
                    pendingSize += uploadSet.QueueWork(work);
                    if (uploadSet.pageList.Count == 0)      // Cleanup empty uploads (shouldn't happen)
                        uploadSet.Status = UploadStatus.Complete;
                }
            }
            if (work.Count == 0) {
                MessageBox.Show("There were no Upload Sets ready for uploading.  Please use the 'Ready' button to mark sets as ready for upload",
                    "UploadExpress",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
            progressBar1.Maximum = (int)Math.Round(pendingSize / 1024.0);
            progressBar1.Value = 0;
            textBox1.Clear();
            textBox1.AppendText("Upload Started:" + Environment.NewLine + "\t" + work.Count + " files remaining." + Environment.NewLine);
            cancelling = false;
            this.Show();
            return true;
        }

        private void Cleanup(string why, string why2) {
            LabelUpdate(why, why2, "");  // Clear labels
            SetCancelButton(false);         // Turn Cancel button off
            string endLog = why + ":" + Environment.NewLine +
                            "\t" + uploadedFiles + " Files uploaded." + Environment.NewLine +
                            "\t" + errorFiles + " Files with upload errors." + Environment.NewLine +
                            ((work.Count > 0) ? "\t" + work.Count + " Files remaining for upload." : "");
            UploadLog(endLog);      // Add reason to log
            cancelling = false;
            work.Clear();       // Allow to be restarted.
        }

        /// <summary>
	/// Loop through the uploadSet work queue, doing all the queue tasks.
	/// </summary>
        public async Task Process() {
            while (work.Count > 0) {
                // If cancelling, stop the upload.
                if (cancelling) {
                    if (curUploadSet != null)
                        curUploadSet.UpdateStatus();
                    Cleanup("Upload Cancelled", "");
                    return;
                }
                // If too many errors have occurred, stop the upload.
                if (contiguousErrors > 80 || totalErrors > 300) {
                    if (curUploadSet != null)
                        curUploadSet.UpdateStatus();
                    Cleanup("Upload Aborted - Too many errors.", "");
                    return;
                }
                await Next();
            }
            if (curUploadSet != null)
                curUploadSet.UpdateStatus();
            Cleanup("Upload Complete", "");
            return;
        }

        /// <summary>
        /// Upload the next image in the queue, creating pages as needed.
        /// </summary>
        public async Task Next() {
            SetCancelButton(true);          // Turn Cancel button on

            WorkUnit workUnit = (WorkUnit)work.Peek();
            Image image = workUnit.Image;
            Page page = workUnit.Page;
            UploadSet uploadSet = workUnit.UploadSet;
            if (uploadSet != curUploadSet) {
                if (curUploadSet != null)
                    curUploadSet.UpdateStatus();
                curUploadSet = uploadSet;
            }
            if (page.pageID == 0) {
                try {
                    page.pageID = await account.Session.NewPageAsync(uploadSet.eventID, page.title);
                    uploadSet.Serialize();
                    UploadLog(page.title + "- Page Created");
                }
                catch (SessionException ex) {
                    switch (ex.Error) {
                        case SessionError.NotLoggedIn:
                            string message = "You are not logged in.  Please refresh your login and restart your upload session.";
                            MessageBox.Show(message, "UploadExpress", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            contiguousErrors = 90;  // Force stop
                            break;
                        case SessionError.NetworkError:     // Retry
                        case SessionError.ServerError:
                        case SessionError.UnknownError:
                            // It's possible that in some of the above cases we want to either
                            // abort or ignore this page, but for now, we'll just retry.
                            totalErrors++;
                            contiguousErrors++;
                            await Delay();
                            break;
                    }
                }
                return;
            }
            LabelUpdate("Event: " + uploadSet.eventTitle, "Page: " + page.title, "Image: " + image.Title);
            UploadMetrics metrics = new UploadMetrics(uploadSet.eventTitle, uploadSet.eventID, page.title, page.pageID, image.Title, image.Path);
            try {
                Stopwatch stopWatch = new Stopwatch();
                using (Stream imageStream = new FileStream(image.Path, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                    metrics.FileSize = imageStream.Length;

                    // The following code will be used for Self Fulfilled pros, or other pros that we have overridden
                    // to scale down their images.  These ignore the compression settings (in order to scale) and always
                    // use a compression of 90.  (awei 11/2016)
                    if (account.Session.MaxSize > 0) {
                        ImageProcess imageProcess = new ImageProcess(imageStream, 90, account.Session.MaxSize);
                        using (Stream imageStreamCompress = imageProcess.GetImageStream()) {
                            metrics.CompressedSize = imageStreamCompress.Length;
                            stopWatch.Start();
                            image.ImageID = await account.Session.UploadAsync(page.pageID, image.Path, imageStreamCompress);
                        }
                        metrics.Compression = 90;
                        metrics.Scale = account.Session.MaxSize;
                    }
                    else if (account.UseCompression) {
                        ImageProcess imageProcess = new ImageProcess(imageStream, account.CompressionRate, 0);
                        using (Stream imageStreamCompress = imageProcess.GetImageStream()) {
                            metrics.CompressedSize = imageStreamCompress.Length;
                            stopWatch.Start();
                            image.ImageID = await account.Session.UploadAsync(page.pageID, image.Path, imageStreamCompress);
                        }
                        metrics.Compression = account.CompressionRate;
                    }
                    else {
                        stopWatch.Start();
                        image.ImageID = await account.Session.UploadAsync(page.pageID, image.Path, imageStream);
                    }
                    stopWatch.Stop();
                    metrics.Milliseconds = stopWatch.ElapsedMilliseconds;
                    metrics.ImageId = image.ImageID;
                    contiguousErrors = 0;
                    uploadedFiles++;
                    work.Dequeue(); // Can now safely remove from queue
                    image.Status = ImageStatus.Uploaded;
                    image.Uploading = false;
                    UploadLog(image.FileName + " - Uploaded");
                    uploadSet.Serialize();
                    uploadSet.AsyncUpdateNodes(false);
                    ProgressIncrement((int)image.Size);
                }
            }
            catch (SessionException ex) {
                log.Add(new LogEntry("Upload Error:", ex.Message));
                metrics.ErrorMessage = ex.Message;
                switch (ex.Error) {
                    case SessionError.NotLoggedIn:
                        string message = "You are not logged in.  Please refresh your login and restart your upload session.";
                        MessageBox.Show(message, "UploadExpress", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        contiguousErrors = 90;  // Force stop
                        break;
                    case SessionError.NetworkError:     // Retry
                    case SessionError.WebRequestTimeout:
                    case SessionError.ServerError:
                    case SessionError.UnknownError:
                        // It's possible that in some of the above cases we want to either
                        // abort or ignore this page, but for now, we'll just retry.
                        totalErrors++;
                        contiguousErrors++;
                        await Delay();
                        break;
                    case SessionError.PageFull:
                        // Page is full.  Create a new page for images remaining on this page.
                        Page newPage = new Page(page.title);
                        uploadSet.pageList.Add(newPage);
                        foreach (Image im in (Image[])work.ToArray()) {
                            Page oldPage = (Page)im.node.Parent.Tag;
                            if (oldPage.pageID == page.pageID) {
                                oldPage.imageList.Remove(im);
                                oldPage.node.Nodes.Remove(im.node);
                                newPage.imageList.Add(im);
                                newPage.node.Nodes.Add(im.node);
                            }
                        }
                        uploadSet.Serialize();
                        uploadSet.AsyncUpdateNodes(true);
                        break;
                    case SessionError.EventExpired:
                        // Event is expired.  Remove all uploads for this event from the queue.
                        RemoveUploads(uploadSet.eventID, " - not uploaded.  Event has expired");
                        return;
                    case SessionError.InvalidImage:
                        // Server didn't like the image.  Mark as error and move on.
                        totalErrors++;
                        contiguousErrors++;
                        errorFiles++;
                        work.Dequeue();
                        image.Status = ImageStatus.Error;   // Mark file in error
                        image.Uploading = false;
                        UploadLog(image.FileName + " - invalid image");
                        uploadSet.Serialize();
                        uploadSet.AsyncUpdateNodes(false);
                        ProgressIncrement((int)image.Size);
                        break;
                    default:
                        // The only exception we expect to see here is if there is some error with the file, so
                        // there is no point in delaying.
                        totalErrors++;
                        contiguousErrors++;
                        errorFiles++;
                        image.Status = ImageStatus.Error;       // Mark file in error
                        image.Uploading = false;
                        UploadLog(image.Path + " - File Error");
                        work.Dequeue();
                        uploadSet.Serialize();
                        uploadSet.AsyncUpdateNodes(false);
                        ProgressIncrement((int)image.Size);
                        break;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
            }
            log.Add(new LogEntry("Upload Metrics:", metrics.ToString()));
            return;
        }

        public bool Uploading() {
            return (work != null && work.Count > 0);
        }

        private async Task Delay() {
            int delay = 1 << contiguousErrors;
            if (delay > 600)
                delay = 600;        // Make sure maximum retry time is 10 minutes.
            while (delay > 0 && !cancelling) {
                LabelUpdate("Error", "Retrying in " + delay + " seconds", "");
                await Task.Delay(1000);
                delay--;
            }
        }

        // Called to remove uploads from the work queue when they can't be completed for some
        // major reason.  (Event full or expired).
        private void RemoveUploads(int eventID, string reason) {
            object[] workUnits = work.ToArray();
            work.Clear();
            foreach (WorkUnit workUnit in workUnits) {
                UploadSet uploadSet = workUnit.UploadSet;
                if (uploadSet.eventID != eventID)
                    work.Enqueue(workUnit);
                else {
                    workUnit.Image.Uploading = false;
                    errorFiles++;
                    UploadLog(workUnit.Image.FileName + reason);
                    ProgressIncrement((int)workUnit.Image.Size);
                }
            }
        }

        private void ProgressIncrement(int size) {
            progressBar1.Increment((int)Math.Round(size / 1024.0));
        }

        // Update the tree labels in the upload.
        private void LabelUpdate(string eventTitle, string pageTitle, string imageTitle) {
            label1.Text = eventTitle;
            label2.Text = pageTitle;
            label3.Text = imageTitle;
        }

        // Add text to the upload log
        private void UploadLog(string text) {
            textBox1.AppendText(text + Environment.NewLine);
        }

        private void SetCancelButton(bool enabled) {
            cancelButton.Enabled = enabled;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing) {
            if (disposing) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.CloseBtn = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // CloseBtn
            // 
            this.CloseBtn.Location = new System.Drawing.Point(208, 416);
            this.CloseBtn.Name = "CloseBtn";
            this.CloseBtn.TabIndex = 3;
            this.CloseBtn.Text = "Close";
            this.CloseBtn.Click += new System.EventHandler(this.CloseBtn_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(80, 416);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.TabIndex = 2;
            this.cancelButton.Text = "Cancel";
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(24, 96);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(336, 23);
            this.progressBar1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(24, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(336, 23);
            this.label1.TabIndex = 0;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(24, 32);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(336, 23);
            this.label2.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(24, 56);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(336, 23);
            this.label3.TabIndex = 5;
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(24, 136);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(336, 264);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "";
            // 
            // Upload
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(384, 470);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.CloseBtn);
            this.Name = "Upload";
            this.Text = "Upload";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Upload_Closing);
            this.ResumeLayout(false);

        }
        #endregion

        private void CloseBtn_Click(object sender, System.EventArgs e) {
            this.Hide();
        }

        private void Upload_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            e.Cancel = true;
            this.Hide();
        }

        private void cancelButton_Click(object sender, System.EventArgs e) {
            cancelling = true;
            SetCancelButton(false);         // Turn Cancel button off
            LabelUpdate("Canceling.  Please Wait", "", "");
        }
    }

    public class WorkUnit {
        public UploadSet UploadSet {
            get { return uploadSet; }
        }
        private UploadSet uploadSet;

        public Page Page {
            get { return page; }
        }
        private Page page;

        public Image Image {
            get { return image; }
        }
        private Image image;

        public WorkUnit(UploadSet uploadSet, Page page, Image image) {
            this.uploadSet = uploadSet;
            this.page = page;
            this.image = image;
        }
    }

    // <summary>
    // A class to store metrics about an upload.  It's single method returns
    // a string suitable for logging.
    // </summary>
    public class UploadMetrics {
        string event_title;
        int event_id;
        string page_title;
        int page_id;
        string image_title;
        string image_path;
        public long FileSize {
            set { file_size = value; }
        }
        long file_size = 0;
        public long CompressedSize {
            set { compressed_size = value; }
        }
        long compressed_size = 0;
        public int Scale {
            set { scale = value; }
        }
        int scale = 0;
        public int Compression {
            set { compression = value; }
        }
        int compression = 0;
        public string ImageId {
            set { image_id = value; }
        }
        string image_id = null;
        public string ErrorMessage {
            set { error_message = value; }
        }
        string error_message = null;
        public long Milliseconds {
            set { milliseconds = value; }
        }
        long milliseconds = 0;

        public UploadMetrics(string event_title, int event_id, string page_title, int page_id, string image_title, string image_path) {
            this.event_title = event_title;
            this.event_id = event_id;
            this.page_title = page_title;
            this.page_id = page_id;
            this.image_title = image_title;
            this.image_path = image_path;
        }

        public override string ToString() {
            String res = String.Format("    {0}", image_title);
            res += Environment.NewLine;
            res += String.Format("    Event ({0}): {1}", event_id, event_title);
            res += Environment.NewLine;
            res += String.Format("    Page ({0}): {1}", page_id, page_title);
            res += Environment.NewLine;
            res += String.Format("    Path: {0}", image_path);
            res += Environment.NewLine;
            res += String.Format("    File Size: {0}", file_size);
            res += Environment.NewLine;
            if (compression > 0) {
                res += String.Format("    Compression: {0}", compression);
                res += Environment.NewLine;
                if (scale > 0) {
                    res += String.Format("    Scale: {0}", scale);
                    res += Environment.NewLine;
                }
                res += String.Format("    Compressed Size: {0}", compressed_size);
                res += Environment.NewLine;
            }
            if (milliseconds > 0 && file_size > 0) {
                double bandwidth = (double)file_size * 8.0 / milliseconds / 1000.0;  // Mbit/sec
                if (compressed_size > 0)
                    bandwidth = (double)compressed_size * 8.0 / milliseconds / 1000.0;  // Mbit/sec
                res += String.Format("    Upload Time (ms): {0}", milliseconds);
                res += Environment.NewLine;
                res += String.Format("    Bandwidth: {0:0.0000} Mbit/sec", bandwidth);
                res += Environment.NewLine;
            }
            if (image_id != null) {
                res += String.Format("    Image Id: {0}", image_id);
                res += Environment.NewLine;
            }
            if (error_message != null) {
                res += String.Format("    Error: {0}", error_message);
                res += Environment.NewLine;
            }
            res += Environment.NewLine;
            return res;
        }
    }
}

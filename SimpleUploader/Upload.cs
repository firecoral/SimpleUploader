using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Threading;
using System.Windows.Forms;
using DigiProofs.SoapUpload;


namespace UploadExpress {
    /// <summary>
    /// Summary description for Upload.
    /// </summary>
    public class Upload : System.Windows.Forms.Form {
	private ArrayList uploadSets;
	private Account account;
	private Queue work;
	private UploadSet curUploadSet = null;
	private bool cancelling;
	
	// Keep count of the total errors encountered and the number of contiguous
	// errors.
	private int totalErrors;
	private int contiguousErrors;
	// We keep track of the number of files uploaded and the number that had errors
	// in order to report on them at the end of the upload.
	private int uploadedFiles;
	private int errorFiles;

	public delegate void ProgressIncrementDelegate(int size);
	public ProgressIncrementDelegate progressIncrementDelegate;
	public delegate void UploadStatusDelegate(string eventTitle, string pageTitle, string imageTitle);
	public UploadStatusDelegate uploadStatusDelegate;
	public delegate void UploadLogDelegate(string text);
	public UploadLogDelegate uploadLogDelegate;
	public delegate void CancelButtonDelegate(bool enabled);
	public CancelButtonDelegate cancelButtonDelegate;

	public delegate void RemoveUploadsDelegate(int eventID, string reason);
	public RemoveUploadsDelegate removeUploadsDelegate;

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

	public Upload()	{
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
		    if (uploadSet.pageList.Count == 0)	    // Cleanup empty uploads (shouldn't happen)
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
	    progressIncrementDelegate = new ProgressIncrementDelegate(ProgressIncrement);
	    uploadStatusDelegate = new UploadStatusDelegate(LabelUpdate);
	    uploadLogDelegate = new UploadLogDelegate(UploadLog);
	    cancelButtonDelegate = new CancelButtonDelegate(SetCancelButton);
	    textBox1.Clear();
	    textBox1.AppendText("Upload Started:" + Environment.NewLine + "\t" + work.Count + " files remaining." + Environment.NewLine);
	    removeUploadsDelegate = new RemoveUploadsDelegate(RemoveUploads);
	    cancelling = false;
	    this.Show();
	    return true;
	}

	private void Cleanup(string why, string why2) {
	    BeginInvoke(uploadStatusDelegate, new object[] {why, why2, ""});  // Clear labels
	    BeginInvoke(cancelButtonDelegate, new object[] {false});	    // Turn Cancel button off
	    string endLog = why + ":" + Environment.NewLine +
			    "\t" + uploadedFiles + " Files uploaded." + Environment.NewLine +
			    "\t" + errorFiles + " Files with upload errors." + Environment.NewLine +
			    ((work.Count > 0) ? "\t" + work.Count + " Files remaining for upload." : "");
	    BeginInvoke(uploadLogDelegate, new object[] {endLog});	    // Add reason to log
	    cancelling = false;
	    work.Clear();	// Allow to be restarted.
	}

	//
	//
	public void Next() {
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
	    // Although we usually come into this method logged in, if there was a session error followed
	    // by a failed relogin attempt, we should try to log in again here.
	    if (!account.Session.LoggedIn) {
		BeginInvoke(uploadStatusDelegate, new object[] {"Retrying Login", "", ""});
		try {
		    account.Session.Login();
		    contiguousErrors = 0;
		}
		catch {
		    totalErrors++;
		    contiguousErrors++;
		    Delay();
		}
	    }
	    BeginInvoke(cancelButtonDelegate, new object[] {true});	    // Turn Cancel button on

	    // See if any work remains on the queue.
	    if (work.Count == 0) {
		if (curUploadSet != null)
		    curUploadSet.UpdateStatus();
		Cleanup("Upload Complete", "");
		return;
	    }
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
		account.Session.NewPage(uploadSet.eventID, page.title, "", new DPDoneHandler(this.PageDone), workUnit);
	    }
	    else {
		int compression = -1;
		if (account.UseCompression)
		    compression = account.CompressionRate;
		BeginInvoke(uploadStatusDelegate, new object[] {
				    "Event: " + uploadSet.eventTitle,
				    "Page: " + page.title,
				    "Image: " + image.Title
				});
		try {
		    account.Session.Upload(page.pageID, image.Path, compression, new DPDoneHandler(this.UploadDone), workUnit);
		}
		catch {
		    // The only exception we expect to see here is if there is some error with the file, so
		    // there is no point in delaying.  In addition, we may be in the main thread here so
		    // Thread.Sleep() would be a mistake.
		    totalErrors++;
		    contiguousErrors++;
		    errorFiles++;
		    image.Status = ImageStatus.Error;	// Mark file in error
		    image.Uploading = false;
		    BeginInvoke(uploadLogDelegate, new object[] {image.Path + " - File Error"});
		    work.Dequeue();
		    uploadSet.Serialize();
		    uploadSet.AsyncUpdateNodes(false);
		    BeginInvoke(progressIncrementDelegate, new object[] {(int)image.Size});
		    Next();
		}
	    }
	}

	public bool Uploading() {
	    return (work != null && work.Count > 0);
	}

	public void PageDone(object result, object state, SessionException e) {
	    WorkUnit workUnit = (WorkUnit)state;
	    Page page = workUnit.Page;
	    UploadSet uploadSet = workUnit.UploadSet;
	    if (e != null) {		    // An exception was passed in
		switch (e.Error) {
		    case SessionError.InvalidSession:	    // Attempt to login again.
			BeginInvoke(uploadStatusDelegate, new object[] {"Retrying Login", "", ""});
			try {
			    account.Session.Login();
			}
			catch {
			    totalErrors++;
			    contiguousErrors++;
			    Delay();
			}
			break;
		    case SessionError.NetworkError:	    // Retry
		    case SessionError.ServerError:
		    case SessionError.UnknownError:
		    case SessionError.UnknownSoapError:
			// It's possible that in some of the above cases we want to either
			// abort or ignore this page, but for now, we'll just retry.
			totalErrors++;
			contiguousErrors++;
			Delay();
			break;
		}
	    }
	    else {
		contiguousErrors = 0;
		page.pageID = (int)result;
		uploadSet.Serialize();
		BeginInvoke(uploadLogDelegate, new object[] {page.title + "- Page Created"});
	    }
	    Next();
	}

	public void UploadDone(object result, object state, SessionException e) {
	    WorkUnit workUnit = (WorkUnit)state;
	    Image image = workUnit.Image;
	    Page page = workUnit.Page;
	    UploadSet uploadSet = workUnit.UploadSet;
	    if (e != null) {		    // An exception was passed
		switch (e.Error) {
		    case SessionError.InvalidSession:	    // Attempt to login again.
			BeginInvoke(uploadStatusDelegate, new object[] {"Retrying Login", "", ""});
			try {
			    account.Session.Login();
			}
			catch {
			    totalErrors++;
			    contiguousErrors++;
			    Delay();
			}
			break;
		    case SessionError.NetworkError:	    // Retry
		    case SessionError.ServerError:
		    case SessionError.UnknownError:
		    case SessionError.UnknownSoapError:
			// It's possible that in some of the above cases we want to either
			// abort or ignore this page, but for now, we'll just retry.
			totalErrors++;
			contiguousErrors++;
			Delay();
			break;
		    case SessionError.EventFull:
			// Event has expired.  Remove all uploads for this event from the queue.
			// Use a delegate to execute this since calling RemoveUploads() directly
			// appears to be a bad idea.  Use return since RemoveUploads() will call Next().
			BeginInvoke(removeUploadsDelegate, new object[] {uploadSet.eventID, " - not uploaded.  Event is full"});
			return;
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
			// Event is full.  Remove all uploads for this event from the queue.
			// Use a delegate to execute this since calling RemoveUploads() directly
			// appears to be a bad idea.  Use return since RemoveUploads() will call Next().
			BeginInvoke(removeUploadsDelegate, new object[] {uploadSet.eventID, " - not uploaded.  Event has expired"});
			return;
		    case SessionError.InvalidImage:
			// Server didn't like the image.  Mark as error and move on.
			totalErrors++;
			contiguousErrors++;
			errorFiles++;
			work.Dequeue();
			image.Status = ImageStatus.Error;	// Mark file in error
			image.Uploading = false;
			BeginInvoke(uploadLogDelegate, new object[] {image.FileName + " - Server rejected image."});
			uploadSet.Serialize();
			uploadSet.AsyncUpdateNodes(false);
			BeginInvoke(progressIncrementDelegate, new object[] {(int)image.Size});
			break;
		}
	    }
	    else {
		contiguousErrors = 0;
		uploadedFiles++;
		work.Dequeue();	// Can now safely remove from queue
		image.ImageID = (string)result;
		image.Status = ImageStatus.Uploaded;
		image.Uploading = false;
		BeginInvoke(uploadLogDelegate, new object[] {image.FileName + " - Uploaded"});
		uploadSet.Serialize();
		uploadSet.AsyncUpdateNodes(false);
		BeginInvoke(progressIncrementDelegate, new object[] {(int)image.Size});
	    }
	    Next();
	}

	private void Delay() {
	    int delay = 1 << contiguousErrors;
	    if (delay > 600)
		delay = 600;	    // Make sure maximum retry time is 10 minutes.
	    while (delay > 0 && !cancelling) {
		BeginInvoke(uploadStatusDelegate, new object[] {"Error", "Retrying in " + delay + " seconds", ""});
		Thread.Sleep(1000);
		delay--;
	    }
	}

	// Called to remove uploads from the work queue when they can't be completed for some
	// major reason.  (Event full or expired).
	// Apparently this must be invoked on the main thread so we use
	// a delegate to call it.
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
		    BeginInvoke(uploadLogDelegate, new object[] {workUnit.Image.FileName + reason});
		    BeginInvoke(progressIncrementDelegate, new object[] {(int)workUnit.Image.Size});
		}
	    }
	    Next();
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
	protected override void Dispose(bool disposing)	{
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
	private void InitializeComponent()
	{
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
	    BeginInvoke(cancelButtonDelegate, new object[] {false});	    // Turn Cancel button off
	    BeginInvoke(uploadStatusDelegate, new object[] {"Cancelling.  Please Wait", "", ""});
	}
    }

    public class WorkUnit {
	public UploadSet UploadSet {
	    get {return uploadSet;}
	}
	private UploadSet uploadSet;

	public Page Page {
	    get {return page;}
	}
	private Page page;
	
	public Image Image {
	    get {return image;}
	}
	private Image image;

	public WorkUnit(UploadSet uploadSet, Page page, Image image) {
	    this.uploadSet = uploadSet;
	    this.page = page;
	    this.image  = image;
	}
    }
}

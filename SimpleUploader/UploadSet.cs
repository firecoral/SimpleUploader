using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;


namespace UploadExpress {
    public enum UploadStatus { Ready, Complete };

    public class UploadChangeArgs : EventArgs {
	public enum ChangeType { Status, Size, PageList };
	public ChangeType type;
	public object obj;
	public UploadChangeArgs(ChangeType type, object obj) {
	    this.type = type;
	    this.obj = obj;
	}
    }

    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    [Serializable]
    public class UploadSet : IDeserializationCallback {
	public int eventID;		    // Keep ID here for persistence
	public string eventTitle;
	public string dataDirPath;
	public string fileName;
	public ArrayList pageList = new ArrayList();
	public delegate void RefreshDelegate(bool refresh);
	[NonSerialized]public RefreshDelegate refreshDelegate;

	[NonSerialized] public UploadExpress context;	    // To retrieve properties

	public TreeNode node {
	    get {
		if (_node == null) {
		    _node = new TreeNode();
		    _node.Tag = this;
		}
		return _node;
	    }
	}
	[NonSerialized] private TreeNode _node;

	// Save email to associate with a specific account
	public string Email {
	    get {return _email;}
	    set {_email = value;}
	}
	string _email;

	public UploadStatus Status {
	    get {return _status;}
	    set {
		_status = value;
		AsyncUpdateNodes(false);
		this.Serialize();
	    }
	}
	UploadStatus _status;

	public long PendingSize {
	    get {return _pendingSize;}
	    set {
		_pendingSize = value;
	    }
	}
	[NonSerialized] private long _pendingSize;
	
	public string UploadStatusString {
	    get {
		switch (_status) {
		    case UploadStatus.Ready:
			return "Ready for Upload";
		    case UploadStatus.Complete:
			return "Complete";
		    default:
			return "Unknown";
		}
	    }
	}

	public UploadSet(string dataDirPath, int eventID, string title, UploadExpress context, string email) {
	    this.eventID = eventID;
	    eventTitle = title;
	    this.dataDirPath = dataDirPath;
	    this.context = context;
	    _status = UploadStatus.Ready;
	    _email = email;
	    int i = 0;
	    do {
		fileName = dataDirPath + eventID.ToString() + "-" + i++.ToString();
	    } while (File.Exists(fileName));
	    this.Serialize();
	    UpdateNodes(true);
	    refreshDelegate = new RefreshDelegate(UpdateNodes);
	}
	//
	// Add a new page containing images found directly in the given folder.
	//
	public void AddPage(string path, int maxPageImages, Account.SortOrders sortOrder, bool top) {
	    DirectoryInfo dir = new DirectoryInfo(path);
	    ArrayList files = new ArrayList();
	    files.AddRange(dir.GetFiles("*.jpg"));
	    DirectoryInfo[] dirs = dir.GetDirectories();
	    if (files.Count == 0 && dirs.Length == 0) {
		// XXX Error message (Probably not due to recursion)
		return;
	    }
	    if (sortOrder == Account.SortOrders.Name)
		files.Sort(new NameComparer());
	    else if (sortOrder == Account.SortOrders.CreateDate)
		files.Sort(new DateComparer());

	    int pageCnt = 0;
	    if (files.Count > 0) {
		Page page = new Page(dir.Name);
		int i = 0;
		foreach (FileInfo file in files) {
		    if (++i> maxPageImages) {
			if (pageCnt++ == 0) {
			    page.title = dir.Name + "-01";
			    pageCnt = 2;
			}
			pageList.Add(page);
			if (pageCnt < 10)
			    page = new Page(dir.Name + "-0" + pageCnt);    // XXX Cheap 0 pad used here
			else 
			    page = new Page(dir.Name + "-" + pageCnt);
			i = 1;
		    }
		    Image image = new Image(file);
		    _pendingSize += file.Length;
		    page.imageList.Add(image);
		}
		pageList.Add(page);
	    }
	    if (dirs.Length > 0) {
		foreach (DirectoryInfo dir2 in dirs) {
		    this.AddPage(dir2.FullName, maxPageImages, sortOrder, false);
		}
	    }
	    if (top)
		this.Serialize();
	    node.Expand();
	}
	
	class NameComparer: IComparer {
	    public int Compare(object info1, object info2) {
		FileInfo fileInfo1 = info1 as FileInfo;
		FileInfo fileInfo2 = info2 as FileInfo;
		string name1 = fileInfo1 == null ? "" : fileInfo1.Name;
		string name2 = fileInfo2 == null ? "" : fileInfo2.Name;
		return name1.CompareTo(name2);
	    }
	}

	class DateComparer: IComparer {
	    public int Compare(object info1, object info2) {
		FileInfo fileInfo1 = info1 as FileInfo;
		FileInfo fileInfo2 = info2 as FileInfo;
		DateTime date1 = fileInfo1 == null ? DateTime.Now : fileInfo1.CreationTimeUtc;
		DateTime date2 = fileInfo2 == null ? DateTime.Now : fileInfo2.CreationTimeUtc;
		return date1.CompareTo(date2);
	    }
	};

	// This method adds all of the unloaded images in the upload set to the
	// work queue for the calling upload object.
	public long QueueWork(Queue work) {
	    long pendingSize = 0;
	    foreach (Page page in pageList) {
		foreach (Image image in page.imageList) {
		    if (image.ImageID == null) {
			image.Uploading = true;
			work.Enqueue(new WorkUnit(this, page, image));
			pendingSize += image.Size;
		    }
		}
	    }
	    return pendingSize;
	}

	// Set the status to Complete if all the images are complete.
	public void UpdateStatus() {
	    foreach (Page page in pageList) {
		foreach (Image image in page.imageList) {
		    switch (image.Status) {
			case ImageStatus.Ready:
			case ImageStatus.Error:
		    	    Status = UploadStatus.Ready;
			    Serialize();
			    return;
		    }
		}
	    }
	    Status = UploadStatus.Complete;
	    Serialize();
	}

	public string Import(string selectedPath) {
	    FolderBrowserDialog dlg = new FolderBrowserDialog();
	    dlg.ShowNewFolderButton = false;
	    if (selectedPath != null)
		dlg.SelectedPath = selectedPath;

	    if (dlg.ShowDialog() == DialogResult.OK) {
		AddPage(dlg.SelectedPath, context.CurrentAccount.MaxPageImages, context.CurrentAccount.SortOrder, true);
		AsyncUpdateNodes(true);
		Status = UploadStatus.Ready;
		return dlg.SelectedPath;
	    }
	    return null;
	}

	// Perform an asynchronous node update
	public void AsyncUpdateNodes(bool refresh) {
	    context.BeginInvoke(refreshDelegate, new object[] {refresh});
	}

	// Windows demands that all changes to controls (in this case, the TreeNodes defined
	// in these objects) be updated in the main (creating) thread.  To simplify this, I've
	// isolated all updates in this method.  Note that this does cause some inefficiencies,
	// but I'm assuming that our users have fast enough computers.
	// The refresh argument should be true if the structure of the tree has changed (rather
	// than just the Text in some of the nodes.  In this case, we will refresh the node lists.
	public void UpdateNodes(bool refresh) {
	    if (node.TreeView != null)
		node.TreeView.BeginUpdate();
	    // First, update the node for this uploadSet.
	    string uploadString = eventTitle;
	    switch (_status) {
		case UploadStatus.Ready:
		    uploadString += " - Ready for Upload";
		    break;
		case UploadStatus.Complete:
		    uploadString += " - Complete";
		    break;
	    }
	    if (uploadString != node.Text)
		node.Text = uploadString;
	    // Now update the pages and images.
	    if (refresh)
		node.Nodes.Clear();
	    foreach (Page pg in pageList) {
		if (refresh)
		    pg.node.Nodes.Clear();
		foreach (Image im in pg.imageList) {
		    string imageString = im.Title;
		    switch (im.Status) {
			case ImageStatus.Error:
			    imageString += " - Image error";
			    break;
			case ImageStatus.Uploaded:
			    imageString +=  " - Complete";
			    break;
			default:
			    imageString += " - " + im.Size / 1000 + " KB";
			    break;
		    }
		    if (imageString != im.node.Text)
			im.node.Text = imageString;
		    if (refresh)
			pg.node.Nodes.Add(im.node);
		}
		if (refresh)
		    node.Nodes.Add(pg.node);
	    }
	    if (node.TreeView != null)
		node.TreeView.EndUpdate();
	}

	public void Serialize() {
            // This is hokey.  I can't serialize the uploadSet when there are events
            // associated with UploadSetChanged.  So I remove them here, serialize, and
            // replace them.

            //	    Delegate[] list = null;
            //	    if (UploadSetChanged != null) {
            //		list = UploadSetChanged.GetInvocationList();
            //		foreach (UploadSetChangedHandler del in list) {
            //		    UploadSetChanged -= del;
            //		}
            //	    }
            using (FileStream cfgStrm = new FileStream(fileName, FileMode.Create)) {
                SoapFormatter fmtr = new SoapFormatter();
                fmtr.Serialize(cfgStrm, this);
            }
//	    if (list != null) {
//		foreach (UploadSetChangedHandler del in list) {
//		    UploadSetChanged += del;
//		}
//	    }
	}

	//
	// Delete the on-disk copy of this uploadSet.
	//
	public bool Delete() {
	    foreach (Page page in pageList)
		foreach (Image image in page.imageList)
		    if (image.Uploading)
			return false;

	    if (node != null) {
		node.TreeView.Nodes.Remove(node);
		File.Delete(fileName);
	    }
	    return true;
	}
	    
	public static UploadSet GetUploadSet(string fileName, UploadExpress context) {
            try {
                using (FileStream cfgStrm = new FileStream(fileName, FileMode.Open)) {
                    SoapFormatter fmtr = new SoapFormatter();
                    UploadSet ret = (UploadSet)fmtr.Deserialize(cfgStrm);
                    if (!ret.fileName.Equals(fileName)) {
                        ret.fileName = fileName;
                        ret.Serialize();
                    }
                    ret.context = context;
                    return ret;
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                return null;
            }
        }

	public virtual void OnDeserialization(Object Sender) {
	    refreshDelegate = new RefreshDelegate(UpdateNodes);
	    UpdateNodes(true);
	    _pendingSize = ComputeSize();
         }
	
	public long ComputeSize() {
	    long size = 0;
	    if (_status != UploadStatus.Complete)		
		foreach (Page page in pageList)
		    foreach (Image image in page.imageList)
			if (image.Status != ImageStatus.Uploaded)
			    size += image.Size;
	    return size;
	}

	public void ShrinkPending(long size) {
	    _pendingSize -= size;
	}

	public override string ToString() {
	    return eventTitle + " " + UploadStatusString;
	}
    }
    /// <summary>
    /// Class for an page
    /// pageID: set when the page is successfully created.
    /// title: Page name for creation.
    /// </summary>
    [Serializable]
    public class Page  {
	public int pageID;
	public string title;
	public int maxImages;
	public int currentImages;
	public ArrayList imageList = new ArrayList();

	public TreeNode node {
	    get {
		if (_node == null) {
		    _node = new TreeNode();
		    _node.Text = title;
		    _node.Tag = this;
		}
		return _node;
	    }
	}
	[NonSerialized] private TreeNode _node;

	public Page(string title) {
	    this.title = title;
	}

	public bool Delete() {
	    foreach (Image image in imageList)
		if (image.Uploading)
		    return false;
	    if (node != null) {
		UploadSet uploadSet = (UploadSet)node.Parent.Tag;
		uploadSet.pageList.Remove(this);
		node.Parent.Nodes.Remove(node);
		uploadSet.Serialize();
	    }
	    return true;
	}
    }

    public enum ImageStatus {
	Ready = 1,		    // Image has not yet been uploaded
	Uploaded,		    // Image has been successfully uploaded
	Error,			    // Image had some error during upload.  - will retry on next upload.
    };

    /// <summary>
    /// This represents an individual image.
    /// imageID: set if the image has been successfully uploaded.
    /// path: filename of the image.
    /// caption: caption for this image.
    /// </summary>
    [Serializable]
    public class Image {
	// Refreshes the node associated with this image
	public string ImageID {
	    get {return _imageID;}
	    set {_imageID = value;}
	}
	string _imageID;

	public string Title {
	    get {return _title;}
	}
	string _title;

	public string Path {
	    get {return _path;}
	}
	string _path;

	// Convenience function
	public string FileName {
	    get {return _path.Substring(_path.LastIndexOf('\\')+1);}
	}

	public long Size {
	    get {return _size;}
	}
	long _size;

	public ImageStatus Status {
	    get {return _status;}
	    set {_status = value;}
	}
	ImageStatus _status;

	public TreeNode node {
	    get {
		if (_node == null) {
		    _node = new TreeNode();
		    _node.Tag = this;
		}
		return _node;
	    }
	}
	[NonSerialized] private TreeNode _node;

	public bool Uploading {
	    get {return _uploading;}
	    set {_uploading = value;}
	}
	[NonSerialized] private bool _uploading;

	public Image(FileInfo fileInfo) {
	    _path = fileInfo.FullName;
	    _status = ImageStatus.Ready;
	    _title = fileInfo.Name;
	    _size = fileInfo.Length;
	    _uploading = false;
	}

	public bool Delete() {
	    if (Uploading)
		return false;
	    if (node != null) {
		UploadSet uploadSet = (UploadSet)node.Parent.Parent.Tag;
		Page page = (Page)node.Parent.Tag;
		page.imageList.Remove(this);
		node.Parent.Nodes.Remove(node);
		uploadSet.Serialize();
	    }
	    return true;
	}
    }
}

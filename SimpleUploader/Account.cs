using System;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.Windows.Forms;
using DigiProofs.SoapUpload;


namespace UploadExpress {
    /// <summary>
    /// Summary description for Class1.
    /// </summary>
    [Serializable]
    public class Account : ListViewItem, ISerializable {
	public string Server {
	    get {return server;}
	    set {server = value;}
	}
	private string server;

	public string Email {
	    get {return this.Text;}
	    set {this.Text = value;}
	}

	public string Password {
	    get {return password;}
	    set {password = value;}
	}
	private string password;

	public int MaxPageImages {
	    get {return maxPageImages;}
	    set {maxPageImages = value;}
	}
	private int maxPageImages;

	public enum SortOrders {
	    Name = 0,
	    CreateDate = 1,
	    Unsorted = 2,
	}
	public SortOrders SortOrder {
	    get {return sortOrder;}
	    set {sortOrder = value;}
	}
	private SortOrders sortOrder;

	public bool UseCompression {
	    get {return compression;}
	    set {compression = value;}
	}
	private bool compression;

	public int CompressionRate {
	    get {return compressionRate;}
	    set {compressionRate = value;}
	}
	private int compressionRate;

	public bool IsDefault {
	    get {return isDefault;}
	    set {
		isDefault = value;
		if (isDefault) {
		    if (this.SubItems.Count > 1)
			this.SubItems[1].Text = "Default";
		    else
			this.SubItems.Add("Default");
		}
		else{ 
		    if (this.SubItems.Count > 1)
			this.SubItems[1].Text = "";
		    else
			this.SubItems.Add("");
		}
	    }
	}
	private bool isDefault;

	public bool ProxyOn {
	    get {return proxyOn;}
	    set {proxyOn = value;}
	}
	private bool proxyOn;

	public string ProxyHost {
	    get {return proxyHost;}
	    set {proxyHost = value;}
	}
	private String proxyHost;

	public int ProxyPort {
	    get {return proxyPort;}
	    set {proxyPort = value;}
	}
	private int proxyPort;

	public bool ForceHTTP10 {
	    get {return forceHTTP10;}
	    set {forceHTTP10 = value;}
	}
	private bool forceHTTP10;

	public string SelectedPath {
	    get {return _selectedPath;}
	    set {_selectedPath = value;}
	}
	private string _selectedPath;

	public Upload Upload {
	    get {return UploadSession;}
	    set {UploadSession = value;}
	}
	private Upload UploadSession;

	public ArrayList UploadSetList {
	    get {return uploadSets;}
	    set {uploadSets = value;}
	}
	private ArrayList uploadSets;

	public NetSession Session {
	    get {
		string proxy = null;
		if (proxyOn)
		    proxy = proxyHost + ":" + proxyPort.ToString();
		if (session == null)
		    session = new NetSession(server, this.Text, password, proxy, forceHTTP10);
		return session;
	    }
	}
	private NetSession session;

	// Used to create a default properties object
	public Account() {
	    maxPageImages = 50;
	    sortOrder = SortOrders.Name;
	    compression = true;
	    compressionRate = 80;
	    server = "u.digiproofs.com";
	    proxyOn = false;
	}

	protected Account(SerializationInfo info, StreamingContext context) {
	    server = info.GetString("s");
	    this.Text = info.GetString("e");
	    password = info.GetString("p");
	    maxPageImages = info.GetInt32("m");
	    sortOrder = (SortOrders)info.GetInt32("so");
	    compression = info.GetBoolean("c");
	    compressionRate = info.GetInt32("r");
	    IsDefault = info.GetBoolean("d");
	    proxyOn = info.GetBoolean("p1");
	    proxyHost = info.GetString("p2");
	    proxyPort = info.GetInt32("p3");
	    forceHTTP10 = info.GetBoolean("H10");
	    _selectedPath = info.GetString("sp");
	}

	//[SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
	public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
	    info.AddValue("s", server);
	    info.AddValue("e", this.Text);
	    info.AddValue("p", password);
	    info.AddValue("m", maxPageImages);
	    info.AddValue("so", (int)sortOrder);
	    info.AddValue("c", compression);
	    info.AddValue("r", compressionRate);
	    info.AddValue("d", isDefault);
	    info.AddValue("p1", proxyOn);
	    info.AddValue("p2", proxyHost);
	    info.AddValue("p3", proxyPort);
	    info.AddValue("H10", forceHTTP10);
	    info.AddValue("sp", _selectedPath);
	}

	public void RefreshUploadSets(UploadExpress context, string dataDirPath) {
	    uploadSets = new ArrayList();
	    DirectoryInfo dir = new DirectoryInfo(dataDirPath);
	    FileInfo[] files = dir.GetFiles();
	    foreach (FileInfo file in files) {
		try {
		    UploadSet uploadSet = UploadSet.GetUploadSet(file.FullName, context);
		    // Only show those associated with the current account
		    if (uploadSet.Email == Email) {
			Event ev = null;
			// Limit to events currently in the session eventlist.  Some old
			// ones may have expired.
			foreach (Event ev2 in Session.EventList) {
			    if (ev2.eventID == uploadSet.eventID) {
				ev = ev2;
				break;
			    }
			}
			if (ev == null)	    // If no event was found
			    break;
			uploadSets.Add(uploadSet);
		    }
		}
		catch {
		    Console.WriteLine("ERROR Adding set {0}", file.FullName);
		}
	    }
	}

	public override string ToString() {
	    return this.Text;
	}
    }
    
    [Serializable]
    public class AccountList : ArrayList {
	public delegate void AccountListChangedHandler(object uploadSet, EventArgs e);
	public event AccountListChangedHandler AccountListChanged;
	private string prefPath;

	public Account DefaultAccount {
	    get {
		foreach (Account acct in this) {
		    if (acct.IsDefault)
			return acct;
		}
		return null;
	    }
	    set {
		foreach (Account acct in this)
		    if (acct.IsDefault)
			acct.IsDefault = false;
		value.IsDefault = true;
		this.Serialize();
		if (AccountListChanged != null)
		    AccountListChanged(this, EventArgs.Empty);
	    }
	}

	public AccountList(string prefPath) {   
	    this.prefPath = prefPath;
	}

	public void Add(Account acct) {
	    base.Add(acct);
	    if (this.Count == 1)
		acct.IsDefault = true;
	    this.Serialize();
	    if (AccountListChanged != null)
		AccountListChanged(this, EventArgs.Empty);
	}
	
	public void Remove(Account acct) {
	    base.Remove(acct);
	    if (acct.IsDefault && this.Count != 0) {
		((Account)this[0]).IsDefault = true;
	    }
	    this.Serialize();
	    if (AccountListChanged != null)
		AccountListChanged(this, EventArgs.Empty);
	}

	// Some change has occured and we want to refresh the Account list.
	public void Refresh() {
	    this.Serialize();
	    if (AccountListChanged != null)
		AccountListChanged(this, EventArgs.Empty);
	}

	public void Serialize() {
	    Delegate[] list = null;
	    if (AccountListChanged != null) {
		list = AccountListChanged.GetInvocationList();
		foreach (AccountListChangedHandler del in list) {
		    AccountListChanged -= del;
		}
	    }
	    FileStream cfgStrm = new FileStream(prefPath, FileMode.Create);
	    SoapFormatter fmtr = new SoapFormatter();
	    fmtr.Serialize(cfgStrm, this);
	    cfgStrm.Close();
	    if (list != null) {
		foreach (AccountListChangedHandler del in list) {
		    AccountListChanged += del;
		}
	    }
	}
	
	public static AccountList GetAccounts(string prefPath) {
	    FileStream cfgStrm = new FileStream(prefPath, FileMode.Open);
	    SoapFormatter fmtr = new SoapFormatter();
	    try {
		AccountList ret = (AccountList)fmtr.Deserialize(cfgStrm);
		return ret;
	    }
	    finally {
		cfgStrm.Close();
	    }
	}
    }
}

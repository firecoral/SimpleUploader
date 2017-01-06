using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using DigiProofs.JSONUploader;
using Newtonsoft.Json;

[assembly: ComVisible(false)]
namespace UploadExpress {

    /// <summary>
    /// Information for an account that the user can use to log in and upload to.
    /// We provide for multiple accounts since some pros use both self and fulfillment accounts.
    /// In addition, it's nice for testing.
    /// </summary>
    ///

    // These exceptions are generally unlikely and should be treated as fatal.
    public class AccountException : Exception {
        public AccountException(string reason) : base(reason) {
        }
    }

    // The JsonObject and JsonProperty properties are used to limit serialization to specific
    // properties.  If the ListViewItem base class gets involved, serialization gets trashed.
    [JsonObject(MemberSerialization.OptIn)]
    public class Account : ListViewItem {
        [JsonProperty]
	public string Server {
	    get {return server;}
	    set {server = value;}
	}
	private string server;

        [JsonProperty]
        public string Email {
	    get {return this.Text;}
	    set {this.Text = value;}
	}

        [JsonProperty]
        public string Token {
            get { return uploadToken; }
            set { uploadToken = value; }
        }
        private string uploadToken;

        [JsonProperty]
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
        [JsonProperty]
        public SortOrders SortOrder {
	    get {return sortOrder;}
	    set {sortOrder = value;}
	}
	private SortOrders sortOrder;

        [JsonProperty]
        public bool UseCompression {
	    get { return compression;}
	    set {compression = value;}
	}
	private bool compression;

        [JsonProperty]
        public int CompressionRate {
	    get { return compressionRate; }
	    set {compressionRate = value;}
	}
	private int compressionRate;

        [JsonProperty]
        public bool IsDefault {
	    get { return isDefault; }
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

        [JsonProperty]
        public bool ProxyOn {
	    get {return proxyOn;}
	    set {proxyOn = value;}
	}
	private bool proxyOn;

        [JsonProperty]
        public string ProxyHost {
	    get {return proxyHost;}
	    set {proxyHost = value;}
	}
	private String proxyHost;

        [JsonProperty]
        public int ProxyPort {
	    get {return proxyPort;}
	    set {proxyPort = value;}
	}
	private int proxyPort;

        [JsonProperty]
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
                return session;
            }
            set { session = value; }
	}
	private NetSession session;

        /// <summary>
        /// Create a new Account with default values.
        /// </summary>
        public Account() {
	    maxPageImages = 50;
	    sortOrder = SortOrders.Name;
	    compression = true;
	    compressionRate = 80;
	    server = "u.digiproofs.com";
	    proxyOn = false;
        }

        public Account(string email) : this() {
            this.Email = email;
        }

        public override string ToString() {
	    return this.Text;
	}
    }
    
    public class AccountList : List<Account> {
	public delegate void AccountListChangedHandler(object sender, EventArgs e);
	public event AccountListChangedHandler AccountListChanged;

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
		this.SaveAccounts();
		if (AccountListChanged != null)
		    AccountListChanged(this, EventArgs.Empty);
	    }
	}

	public AccountList() {
	}

        /// <summary>
        /// Add a (newly created) account to the Account list.
        /// </summary>
	public new void Add(Account acct) {
	    base.Add(acct);
            string jsonAccount = JsonConvert.SerializeObject(acct);
            if (this.Count == 1)
		acct.IsDefault = true;
	    this.SaveAccounts();
	    if (AccountListChanged != null)
		AccountListChanged(this, EventArgs.Empty);
	}

        /// <summary>
        /// Remove an account from the account list.
        /// </summary>
        public new void Remove(Account acct) {
	    base.Remove(acct);
	    if (acct.IsDefault && this.Count != 0) {
		((Account)this[0]).IsDefault = true;
	    }
	    this.SaveAccounts();
	    if (AccountListChanged != null)
		AccountListChanged(this, EventArgs.Empty);
	}

	// Some change has occured and we want to refresh the Account list.
	public void Refresh() {
	    this.SaveAccounts();
	    if (AccountListChanged != null)
		AccountListChanged(this, EventArgs.Empty);
	}

        /// <summary>
        /// Save the current set of accounts to the application properties.
        /// </summary>
	public void SaveAccounts () {
            try {
                string jsonAccountList = JsonConvert.SerializeObject(this);
                Properties.Settings.Default.accounts = jsonAccountList;
                Properties.Settings.Default.Save();
            }
            catch {
                throw new AccountException("Could not save AccountList.");
            }
        }

        /// <summary>
        /// Read the initial set of accounts from the application properties.
        /// </summary>
        public static AccountList GetAccounts() {
            try {
                string jsonAccountList = Properties.Settings.Default.accounts;
                if (jsonAccountList == null || jsonAccountList.Length == 0) {
                    return new AccountList();
                }
                AccountList accountList = JsonConvert.DeserializeObject<AccountList>(jsonAccountList);
                return accountList;
            }
            catch {
                throw new AccountException("Could not get AccountList.");
            }
        }
    }
}

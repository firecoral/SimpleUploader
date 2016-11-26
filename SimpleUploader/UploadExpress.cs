using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DigiProofs.JSONUploader;
using DigiProofs.Logger;
using System.Reflection;


namespace UploadExpress
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class UploadExpress : System.Windows.Forms.Form {
	public string dataDirPath;
	private AccountList accounts;

        public Account CurrentAccount {
            get { return currentAccount; }
            set { currentAccount = value; }
        }
	private Account currentAccount;

        public LogList Log {
            get { return log; }
            set { log = value; }
        }
        private LogList log; 

	private AccountListDlg accountListDlg;
	private TreeNode contextNode;	    // To keep track of the object associated with a ContextMenu

	
	private MainMenu mainMenu1;
	private System.Windows.Forms.MenuItem menuNew;
	private System.Windows.Forms.MenuItem menuImport;
	private System.Windows.Forms.MenuItem menuFile;
	private System.Windows.Forms.MenuItem menuExit;
        private System.Windows.Forms.MenuItem menuAccounts;
        private System.Windows.Forms.ToolBar toolBar1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolBarButton new_btn;
        private System.Windows.Forms.ToolBarButton sep1;
        private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.ComboBox accountList1;
        private System.Windows.Forms.StatusBar statusBar1;
        private System.Windows.Forms.MenuItem menuUpload;
        private System.Windows.Forms.ToolBarButton upload_btn;
        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.ToolBarButton import_btn;
        private System.Windows.Forms.MenuItem menuRefresh;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.ToolBarButton sep2;
        private System.Windows.Forms.ToolBarButton purge_btn;
        private System.Windows.Forms.MenuItem DeleteUploadSet;
        private System.Windows.Forms.MenuItem DeletePage;
        private System.Windows.Forms.MenuItem DeleteImage;
        private System.Windows.Forms.MenuItem menuPurge;
        private System.Windows.Forms.MenuItem menuLog;
        private System.Windows.Forms.MenuItem menuAbout;
        private System.Windows.Forms.MenuItem menuHelp;
        private System.ComponentModel.IContainer components;

	public UploadExpress() {
            // If this is a new version, the following does a simple upgrade of the
            // settings into the new version.
            if (Properties.Settings.Default.UpgradeRequired) {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.UpgradeRequired = false;
                Properties.Settings.Default.Save();
            }
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            Log = new LogList();
	    this.new_btn.Tag = menuNew;
	    this.import_btn.Tag = menuImport;
	    this.upload_btn.Tag = menuUpload;
	    this.purge_btn.Tag = menuPurge;

            Shown += UploadExpress_Shown;
        }

        // This is just a stub function for now, but can be used if in the future
        // we need to do some initialization before creating the UploadExpress form.
        public async Task InitializeAsync() {
            await Task.FromResult<object>(null);
        }

        // We want to wait until the form is displayed since we may be updating labels
        // in the form.
        private async void UploadExpress_Shown(object sender, EventArgs e) {
            await AccountsAsync();
        }


        public async Task AccountsAsync() {
            // Generate data file directory and make sure it exists.
            dataDirPath = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            dataDirPath += Path.DirectorySeparatorChar;
            dataDirPath += "DigiProofs";
            if (!Directory.Exists(dataDirPath)) {
                try {
                    Directory.CreateDirectory(dataDirPath);
                }
                catch {
                    //XXX  process Error
                }
            }
            dataDirPath += Path.DirectorySeparatorChar;

            log.Add(new LogEntry("Loading Accounts", ""));
            accounts = AccountList.GetAccounts();
            if (accounts.Count == 0) {
                await InitialAccount();
            }
            accounts.AccountListChanged += new AccountList.AccountListChangedHandler(accountListChanged);
            accountListChanged(accounts, EventArgs.Empty);
        }

        // <summary>
        // Prompt for a email and password.  This will loop until it gets a valid
        // email/password.  If the user cancels the dialog, it will display a message
        // and the app will end.
        // On success the user will be logged into an account.
        // </summary>
        private async Task InitialAccount() {
            string token = null;
            log.Add(new LogEntry("Adding Initial Account", ""));
            statusBar1.Text = "No DigiProofs Account";
            InitialAccount initialAccountDialog = new InitialAccount();
            // We will loop here until they have a successful GetToken() or cancel the dialog.
            while (token == null) {
                initialAccountDialog.ShowDialog();
                if (initialAccountDialog.DialogResult == DialogResult.OK) {
                    string email = initialAccountDialog.Email;
                    string password = initialAccountDialog.Password;
                    //
                    // We are going to create an account here, but we won't save it unless
                    // we successfully obtain a token for this account and password.
                    //
                    Account account = new Account(initialAccountDialog.Email);
                    string proxy = null;
                    if (account.ProxyOn)
                        proxy = account.ProxyHost + ":" + account.ProxyPort.ToString();
                    account.Session = new NetSession(Log, account.Server, account.Email, account.Token, proxy);
                    token = await GetToken(account, password);
                    
                    account.IsDefault = true;
                    accounts.Add(account);
                    CurrentAccount = account;
                    log.Add(new LogEntry(String.Format("Added Account {0}", account.Email), ""));
                    if (token != null) {
                        statusBar1.Text = "Logged in as " + CurrentAccount.Email;
                        account.Token = token;
                        await account.Session.GetEventListAsync();
                        log.Add(new LogEntry(String.Format("Obtained Events for {0}", account.Email), ""));
                    }
                    else {
                        statusBar1.Text = "Logged in failed";
                    }
                    accounts.SaveAccounts();
                    return;
                }
                // if the dialog is cancelled, we abort here.
                MessageBox.Show("You must provide an account to use this program",
                            "UploadExpress",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                //Environment.Exit(0);
                return;
            }
        }

        public async Task LoginAsync() {
            if (CurrentAccount == null) {
                if (accounts.Count == 0) {
                    await InitialAccount();
                    return;  // InitialAccounts does all the work and is guaranteed to return with a valid CurrentAccount
                }
                else {
                    foreach (Account acct in accounts) {
                        if (acct.IsDefault) {
                            CurrentAccount = acct;
                            break;
                        }
                    }
                    if (CurrentAccount == null) {
                        // This should never happen.
                        log.Add(new LogEntry(String.Format("Missing default account (which shouldn't happen).  Using {0}", CurrentAccount.Email), ""));
                        CurrentAccount = accounts[0];
                    }
                }
            }
            if (CurrentAccount.Session == null) {
                string proxy = null;
                if (CurrentAccount.ProxyOn)
                    proxy = CurrentAccount.ProxyHost + ":" + CurrentAccount.ProxyPort.ToString();
                CurrentAccount.Session = new NetSession(Log, CurrentAccount.Server, CurrentAccount.Email, CurrentAccount.Token, proxy);
                if (CurrentAccount.Token == null) {
                    log.Add(new LogEntry(String.Format("No Token for Account {0}", CurrentAccount.Email), ""));
                    AccountPassword accountPasswordDialog = new AccountPassword();
                    accountPasswordDialog.ShowDialog();
                    if (accountPasswordDialog.DialogResult == DialogResult.OK) {
                        string password = accountPasswordDialog.Password;
                        string token = await GetToken(CurrentAccount, password);
                        CurrentAccount.Token = token;
                        accounts.SaveAccounts();
                    }
                    if (CurrentAccount.Token == null) {
                        return;
                    }
                }
                statusBar1.Text = "Logging into " + CurrentAccount.Email;
                await CurrentAccount.Session.GetEventListAsync();
                log.Add(new LogEntry(String.Format("Obtained Events for {0}", CurrentAccount.Email), ""));
                statusBar1.Text = "Logged in as " + CurrentAccount.Email;
            }
            return;
        }

        // <summary>
        // Try to obtain an upload token from the upload servers.  On success, return the token
        // string.  On failure, display a message and return null.
        // </summary>
        public async Task<string> GetToken(Account account, string password) {
            try {
                string token = await account.Session.GetToken(password);
                return token;
            }
            catch (SessionException ex) {
                log.Add(new LogEntry("Login Exception", ex.ToString()));
                string message = "Unexpected Error";
                switch (ex.Error) {
                    case SessionError.LoginFail:
                        message = "Couldn't log into " + CurrentAccount.Email + "." + Environment.NewLine + "Please check your email and password.";
                        statusBar1.Text = "Login Failed";
                        break;
                    case SessionError.NetworkError:
                        message = "Connection Error: Couldn't connect to the DigiProofs uploaders." +
                                        Environment.NewLine +
                                        "Please make sure your computer is connected to the internet." +
                                        Environment.NewLine +
                                        "If the problem persists, please contact DigiProofs support (support@digiproofs.com).";
                        statusBar1.Text = "Connection Failed";
                        break;
                    case SessionError.ServerError:
                        message = "Server Error: An error occurred on the DigiProofs uploaders." +
                                        Environment.NewLine +
                                        "Please try restarting this application." +
                                        Environment.NewLine +
                                        "If the problem persists, please contact DigiProofs support (support@digiproofs.com).";
                        statusBar1.Text = "Server Error";
                        break;
                    case SessionError.UnknownError:
                        message = "Connection Error: Couldn't log into DigiProofs Upload Service." + Environment.NewLine + "Error: " + ex.Message;
                        statusBar1.Text = "Login Error";
                        break;
                    default:
                        message = "Unexpected Error - Could not log in:" + Environment.NewLine + "Error: " + ex.Message;
                        statusBar1.Text = "Login Error";
                        break;
                }
                MessageBox.Show(message, "UploadExpress", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        public void RefreshUploadSets(Account account) {
            ArrayList uploadSets = new ArrayList();
            DirectoryInfo dir = new DirectoryInfo(dataDirPath);
            FileInfo[] files = dir.GetFiles();
            List<string> uploadFiles = new List<string>();
            foreach (FileInfo file in files) {
                try {
                    uploadFiles.Add(file.FullName);
                    UploadSet uploadSet = UploadSet.GetUploadSet(file.FullName, this);
                    // Only show those associated with the current account
                    if (uploadSet.Email == account.Email) {
                        Event ev = null;
                        // Limit to events currently in the session eventlist.  Some old
                        // ones may have expired.
                        foreach (Event ev2 in account.Session.EventList) {
                            if (ev2.event_id == uploadSet.eventID) {
                                ev = ev2;
                                break;
                            }
                        }
                        if (ev == null)     // If no event was found
                            break;
                        uploadSets.Add(uploadSet);
                    }
                }
                catch {
                    Console.WriteLine("ERROR Adding set {0}", file.FullName);
                }
            }
            log.Add(new LogEntry("Restoring Cached uploads:", String.Join<string>(Environment.NewLine + "  ", uploadFiles)));
            account.UploadSetList = uploadSets;
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UploadExpress));
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuFile = new System.Windows.Forms.MenuItem();
            this.menuNew = new System.Windows.Forms.MenuItem();
            this.menuImport = new System.Windows.Forms.MenuItem();
            this.menuUpload = new System.Windows.Forms.MenuItem();
            this.menuPurge = new System.Windows.Forms.MenuItem();
            this.menuAccounts = new System.Windows.Forms.MenuItem();
            this.menuRefresh = new System.Windows.Forms.MenuItem();
            this.menuExit = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuHelp = new System.Windows.Forms.MenuItem();
            this.menuLog = new System.Windows.Forms.MenuItem();
            this.menuAbout = new System.Windows.Forms.MenuItem();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.DeleteUploadSet = new System.Windows.Forms.MenuItem();
            this.DeletePage = new System.Windows.Forms.MenuItem();
            this.DeleteImage = new System.Windows.Forms.MenuItem();
            this.toolBar1 = new System.Windows.Forms.ToolBar();
            this.new_btn = new System.Windows.Forms.ToolBarButton();
            this.import_btn = new System.Windows.Forms.ToolBarButton();
            this.sep1 = new System.Windows.Forms.ToolBarButton();
            this.upload_btn = new System.Windows.Forms.ToolBarButton();
            this.sep2 = new System.Windows.Forms.ToolBarButton();
            this.purge_btn = new System.Windows.Forms.ToolBarButton();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.accountList1 = new System.Windows.Forms.ComboBox();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuFile,
            this.menuItem1});
            // 
            // menuFile
            // 
            this.menuFile.Index = 0;
            this.menuFile.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuNew,
            this.menuImport,
            this.menuUpload,
            this.menuPurge,
            this.menuAccounts,
            this.menuRefresh,
            this.menuExit});
            this.menuFile.Text = "File";
            // 
            // menuNew
            // 
            this.menuNew.Index = 0;
            this.menuNew.Shortcut = System.Windows.Forms.Shortcut.CtrlN;
            this.menuNew.Text = "New";
            this.menuNew.Click += new System.EventHandler(this.menuNew_Click);
            // 
            // menuImport
            // 
            this.menuImport.Index = 1;
            this.menuImport.Shortcut = System.Windows.Forms.Shortcut.CtrlI;
            this.menuImport.Text = "Import";
            this.menuImport.Click += new System.EventHandler(this.menuImport_Click);
            // 
            // menuUpload
            // 
            this.menuUpload.Index = 2;
            this.menuUpload.Shortcut = System.Windows.Forms.Shortcut.CtrlU;
            this.menuUpload.Text = "Upload";
            this.menuUpload.Click += new System.EventHandler(this.start_Click);
            // 
            // menuPurge
            // 
            this.menuPurge.Index = 3;
            this.menuPurge.Shortcut = System.Windows.Forms.Shortcut.CtrlP;
            this.menuPurge.Text = "Purge";
            this.menuPurge.Click += new System.EventHandler(this.menuPurge_Click);
            // 
            // menuAccounts
            // 
            this.menuAccounts.Index = 4;
            this.menuAccounts.Text = "Accounts";
            this.menuAccounts.Click += new System.EventHandler(this.menuAccounts_Click);
            // 
            // menuRefresh
            // 
            this.menuRefresh.Index = 5;
            this.menuRefresh.Text = "Refresh Login";
            this.menuRefresh.Click += new System.EventHandler(this.menuRefresh_Click);
            // 
            // menuExit
            // 
            this.menuExit.Index = 6;
            this.menuExit.Text = "Exit";
            this.menuExit.Click += new System.EventHandler(this.menuExit_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 1;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuHelp,
            this.menuLog,
            this.menuAbout});
            this.menuItem1.Text = "Help";
            // 
            // menuHelp
            // 
            this.menuHelp.Index = 0;
            this.menuHelp.Text = "Help";
            this.menuHelp.Click += new System.EventHandler(this.menuHelp_Click);
            // 
            // menuLog
            // 
            this.menuLog.Index = 1;
            this.menuLog.Text = "Log";
            this.menuLog.Click += new System.EventHandler(this.menuLog_Click);
            // 
            // menuAbout
            // 
            this.menuAbout.Index = 2;
            this.menuAbout.Text = "About";
            this.menuAbout.Click += new System.EventHandler(this.menuAbout_Click);
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.DeleteUploadSet,
            this.DeletePage,
            this.DeleteImage});
            this.contextMenu1.Popup += new System.EventHandler(this.BeforePopup);
            // 
            // DeleteUploadSet
            // 
            this.DeleteUploadSet.Index = 0;
            this.DeleteUploadSet.Text = "Delete Upload Set";
            this.DeleteUploadSet.Click += new System.EventHandler(this.DeleteUploadSet_Click);
            // 
            // DeletePage
            // 
            this.DeletePage.Index = 1;
            this.DeletePage.Text = "Delete Page";
            this.DeletePage.Click += new System.EventHandler(this.DeletePage_Click);
            // 
            // DeleteImage
            // 
            this.DeleteImage.Index = 2;
            this.DeleteImage.Text = "Delete Image";
            this.DeleteImage.Click += new System.EventHandler(this.DeleteImage_Click);
            // 
            // toolBar1
            // 
            this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.new_btn,
            this.import_btn,
            this.sep1,
            this.upload_btn,
            this.sep2,
            this.purge_btn});
            this.toolBar1.DropDownArrows = true;
            this.toolBar1.ImageList = this.imageList1;
            this.toolBar1.Location = new System.Drawing.Point(0, 0);
            this.toolBar1.Name = "toolBar1";
            this.toolBar1.ShowToolTips = true;
            this.toolBar1.Size = new System.Drawing.Size(520, 42);
            this.toolBar1.TabIndex = 4;
            this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
            // 
            // new_btn
            // 
            this.new_btn.ImageIndex = 0;
            this.new_btn.Name = "new_btn";
            this.new_btn.Tag = "";
            this.new_btn.Text = "New";
            this.new_btn.ToolTipText = "Add new upload set";
            // 
            // import_btn
            // 
            this.import_btn.ImageIndex = 4;
            this.import_btn.Name = "import_btn";
            this.import_btn.Text = "Import";
            this.import_btn.ToolTipText = "Import Images into selected Upload Set";
            // 
            // sep1
            // 
            this.sep1.Name = "sep1";
            this.sep1.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // upload_btn
            // 
            this.upload_btn.ImageIndex = 2;
            this.upload_btn.Name = "upload_btn";
            this.upload_btn.Tag = "";
            this.upload_btn.Text = "Upload";
            this.upload_btn.ToolTipText = "Begin Uploading";
            // 
            // sep2
            // 
            this.sep2.Name = "sep2";
            this.sep2.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
            // 
            // purge_btn
            // 
            this.purge_btn.ImageIndex = 1;
            this.purge_btn.Name = "purge_btn";
            this.purge_btn.Text = "Purge";
            this.purge_btn.ToolTipText = "Remove Completed Upload Sets";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            this.imageList1.Images.SetKeyName(2, "");
            this.imageList1.Images.SetKeyName(3, "");
            this.imageList1.Images.SetKeyName(4, "");
            // 
            // accountList1
            // 
            this.accountList1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.accountList1.Location = new System.Drawing.Point(256, 8);
            this.accountList1.Name = "accountList1";
            this.accountList1.Size = new System.Drawing.Size(168, 21);
            this.accountList1.TabIndex = 5;
            this.accountList1.SelectedValueChanged += new System.EventHandler(this.accountList1_SelectedIndexChanged);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 587);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(520, 22);
            this.statusBar1.TabIndex = 6;
            // 
            // treeView1
            // 
            this.treeView1.BackColor = System.Drawing.SystemColors.Window;
            this.treeView1.ContextMenu = this.contextMenu1;
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.HideSelection = false;
            this.treeView1.Location = new System.Drawing.Point(0, 42);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(520, 545);
            this.treeView1.TabIndex = 7;
            // 
            // UploadExpress
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(520, 609);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.accountList1);
            this.Controls.Add(this.toolBar1);
            this.Controls.Add(this.statusBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mainMenu1;
            this.Name = "UploadExpress";
            this.Text = "UploadExpress";
            this.ResumeLayout(false);
            this.PerformLayout();

	}
	#endregion
	#region MenuActions
	//
	// Invoke the properties dialog
	//
	private void menuExit_Click(object sender, System.EventArgs e) {
            Environment.Exit(0);
	}

	//
	// Invoke the properties dialog
	//
	private void menuAccounts_Click(object sender, System.EventArgs e) {
	    if (accountListDlg == null) {
		accountListDlg = new AccountListDlg(accounts);
	    }
	    accountListDlg.Show();
	    accountListDlg.BringToFront();

	}

	// 
	// Select an Event, then create a new UploadView for that event.
	//
	private async void menuNew_Click(object sender, System.EventArgs e) {
            await LoginAsync();
            if (CurrentAccount == null || CurrentAccount.Session == null || CurrentAccount.Token == null)
                return;
            try {
		if (CurrentAccount.Session.EventList.Length == 0) {
		    MessageBox.Show("You have no events available for uploading in this account." + Environment.NewLine +
			"Please log into your account and create a new event for this upload.",
			"UploadExpress",
			MessageBoxButtons.OK,
			MessageBoxIcon.Error);
		    return;
		}
	    }
	    catch (SessionException ex) {
		switch (ex.Error) {
		    case SessionError.LoginFail:
			MessageBox.Show("Couldn't log into " + CurrentAccount.Email + "." + Environment.NewLine + "Please check your email and password.",
			    "UploadExpress",
			    MessageBoxButtons.OK,
			    MessageBoxIcon.Error);
			break;
		    default:
			MessageBox.Show("Error getting account list for Session.",
			    "UploadExpress",
			    MessageBoxButtons.OK,
			    MessageBoxIcon.Error);
			break;
		}
		return;
	    }


	    EventChooser chooseEvent = new EventChooser(new ArrayList(CurrentAccount.Session.EventList));
	    chooseEvent.ShowDialog();
	    if (chooseEvent.DialogResult == DialogResult.OK) {
		Event ev = chooseEvent.GetSelectedEvent();
		UploadSet uploadSet = new UploadSet(dataDirPath, ev.event_id, ev.title, this, CurrentAccount.Email);
		CurrentAccount.UploadSetList.Add(uploadSet);
		treeView1.BeginUpdate();
		treeView1.Nodes.Add(uploadSet.node);
		treeView1.SelectedNode = uploadSet.node;
		treeView1.EndUpdate();
	    }
	}

	//
	// Show the about dialog
	//
	private void menuAbout_Click(object sender, System.EventArgs e) {
            Assembly thisAssem = typeof(UploadExpress).Assembly;
            AssemblyName thisAssemName = thisAssem.GetName();
            string company = thisAssem.GetCustomAttribute<AssemblyCom‌​panyAttribute>().Com‌​pany;
            string copyright = thisAssem.GetCustomAttribute<AssemblyCopyrightAttribute>().Copyright;
            string version = thisAssemName.Version.ToString();
            string name = thisAssemName.Name.ToString();

            About aboutDlg = new About(name, version, company, copyright);
	    aboutDlg.ShowDialog();
	}

	//
	// Try to provide some help.
	//
	private void menuHelp_Click(object sender, System.EventArgs e) {
	    if (CurrentAccount == null)
		Process.Start("http://www.digiproofs.com/UploaderHelp.pdf");
	    else
		Process.Start("http://" + CurrentAccount.Server + "/UploaderHelp.pdf");
	}


	// 
	// Import new pages and images into the selected uploadset.
	//
	private async void menuImport_Click(object sender, System.EventArgs e) {
            await LoginAsync();
            if (CurrentAccount == null || CurrentAccount.Session == null || CurrentAccount.Token == null)
                return;
            TreeNode node = treeView1.SelectedNode;
	    if (node == null) {
		MessageBox.Show("Please select an Upload Set below to import into.  Use 'New' to create a new Upload Set",
		    "UploadExpress",
		    MessageBoxButtons.OK,
		    MessageBoxIcon.Error);
		return;
	    }
	    while (node.Parent != null)
		node = node.Parent;
	    string path = ((UploadSet)node.Tag).Import(CurrentAccount.SelectedPath);
	    if (path != null) {
		CurrentAccount.SelectedPath = path;	// Save current selected path
		accounts.SaveAccounts();
	    }
	}

	private async void start_Click(object sender, System.EventArgs e) {
            await LoginAsync();
            if (CurrentAccount == null || CurrentAccount.Session == null || CurrentAccount.Token == null)
                return;
            if (CurrentAccount.Upload == null) {
		CurrentAccount.Upload = new Upload(Log);
	    }
	    Upload upload = CurrentAccount.Upload;
	    if (upload.Uploading()) {
		upload.Show();
		upload.BringToFront();
		return;
	    }
	    if (upload.Setup(CurrentAccount))
               await upload.Process();		// Start the upload loop
	}

	// 
	// Refresh the login for the current account.
	//
	private async void menuRefresh_Click(object sender, System.EventArgs e) {
            if (CurrentAccount != null) {
                CurrentAccount.Session = null;
                CurrentAccount.Token = null;
            }
            await LoginAsync();
	}

	// 
	// Display the log
	//
	private void menuLog_Click(object sender, System.EventArgs e) {
	    new LogViewer(Log.ToString()).Show();
	}

	#endregion

	private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e) {
	    ToolBarButton tbb = e.Button;
	    MenuItem mi = (MenuItem)tbb.Tag;
	    mi.PerformClick();
	}

	// Called when some other function causes a change in the account list.
	// We want to reload the account list combo box on the tool bar here.
	// We need to make sure that appropriate Account is still selected.
	// If the CurrentAccount is set and still exists in the account list,
	// leave that one selected.  Otherwise, use the default Object.
	private void accountListChanged(object sender, System.EventArgs e) {
	    AccountList accounts = (AccountList)sender;
	    Account selectedAccount = null;	    // Which account should be selected
	    foreach (Account acct in accounts) {
		if (acct == CurrentAccount) {
		    selectedAccount = acct;
		    break;
		}
		if (acct.IsDefault) {
		    selectedAccount = acct;
		}
	    }

	    accountList1.Items.Clear();
	    accountList1.Items.AddRange(accounts.ToArray());
	    if (accounts.Count > 0 && selectedAccount != null)
		accountList1.SelectedItem = selectedAccount;
	}

        private async void accountList1_SelectedIndexChanged(object sender, System.EventArgs e) {
	    CurrentAccount = (Account)accountList1.SelectedItem;
            log.Add(new LogEntry(String.Format("Selected Account changed to {0}", CurrentAccount.Email), ""));
            Cursor.Current = Cursors.WaitCursor;

            treeView1.Nodes.Clear();
            await LoginAsync();
            if (CurrentAccount == null || CurrentAccount.Session == null || CurrentAccount.Token == null)
                return;

            // now populate the UploadSetList
            if (CurrentAccount.UploadSetList == null)
		RefreshUploadSets(CurrentAccount);
	    foreach (UploadSet uploadSet in CurrentAccount.UploadSetList) {
		treeView1.Nodes.Add(uploadSet.node);
	    }
	    Cursor.Current = Cursors.Arrow;
	}

	private void DeleteUploadSet_Click(object sender, System.EventArgs e) {
	    if (contextNode != null && contextNode.Tag.GetType() == Type.GetType("UploadExpress.UploadSet")) {
		treeView1.BeginUpdate();
		if (((UploadSet)(contextNode.Tag)).Delete() == false) {
		    MessageBox.Show("Could not delete UploadSet.  Images are being uploaded.",
			"UploadExpress",
			MessageBoxButtons.OK,
			MessageBoxIcon.Error);
		}
		CurrentAccount.UploadSetList.Remove((UploadSet)(contextNode.Tag));
		treeView1.EndUpdate();
	    }
	}

	private void DeletePage_Click(object sender, System.EventArgs e) {
	    if (contextNode != null && contextNode.Tag.GetType() == Type.GetType("UploadExpress.Page")) {
		treeView1.BeginUpdate();
		if (((Page)(contextNode.Tag)).Delete() == false) {
		    MessageBox.Show("Could not delete page.  Images are being uploaded.",
			"UploadExpress",
			MessageBoxButtons.OK,
			MessageBoxIcon.Error);
		}
		treeView1.EndUpdate();
	    }
	}

	private void DeleteImage_Click(object sender, System.EventArgs e) {
	    if (contextNode != null && contextNode.Tag.GetType() == Type.GetType("UploadExpress.Image")) {
		treeView1.BeginUpdate();
		if (((Image)(contextNode.Tag)).Delete() == false) {
		    MessageBox.Show("Could not delete Image.  Image is currently being uploaded.",
			"UploadExpress",
			MessageBoxButtons.OK,
			MessageBoxIcon.Error);
		}
		treeView1.EndUpdate();
	    }
	}

	//
	// Remove all completed UploadSets for the current account
	//
	private void menuPurge_Click(object sender, System.EventArgs e) {
	    treeView1.BeginUpdate();
	    foreach (UploadSet uploadSet in CurrentAccount.UploadSetList.ToArray()) {
		if (uploadSet.Status == UploadStatus.Complete) {
		    CurrentAccount.UploadSetList.Remove(uploadSet);
		    uploadSet.Delete();
		}
	    }
	    treeView1.EndUpdate();
	}

	private void BeforePopup(object sender, System.EventArgs e) {
	    contextMenu1.MenuItems.Clear();
	    contextNode = treeView1.GetNodeAt(treeView1.PointToClient(System.Windows.Forms.Cursor.Position));
	    if (contextNode == null)
		return;
	    Type nodeType = contextNode.Tag.GetType();
	    if (nodeType == Type.GetType("UploadExpress.UploadSet")) {
		contextMenu1.MenuItems.Add(DeleteUploadSet);
	    }
	    else if (nodeType == Type.GetType("UploadExpress.Page")) {
		contextMenu1.MenuItems.Add(DeletePage);
	    }
	    else if (nodeType == Type.GetType("UploadExpress.Image")) {
		contextMenu1.MenuItems.Add(DeleteImage);
	    }
	}
    }

    // Main has been refactored here as per: https://msdn.microsoft.com/en-us/magazine/mt620013.aspx
    public class Start1 {
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());

            Start1 p = new Start1();
            p.ExitRequested += p_ExitRequested;
            Task programStart = p.StartAsync();
            HandleExceptions(programStart);

            Application.Run();
        }

        private static async void HandleExceptions(Task task) {
            try {
                await Task.Yield(); //ensure this runs as a continuation
                await task;
            }
            catch (Exception ex) {
                //deal with exception, either with message box
                //or delegating to general exception handling logic you may have wired up 
                //e.g. to Application.ThreadException and AppDomain.UnhandledException
                MessageBox.Show(ex.ToString());

                Application.Exit();
            }
        }

        static void p_ExitRequested(object sender, EventArgs e) {
            Application.ExitThread();
        }

        private readonly UploadExpress m_mainForm;
        private Start1() {
            m_mainForm = new UploadExpress();
            m_mainForm.FormClosed += m_mainForm_FormClosed;
        }

        public async Task StartAsync() {
            await m_mainForm.InitializeAsync();
            m_mainForm.Show();

        }

        public event EventHandler<EventArgs> ExitRequested;
        void m_mainForm_FormClosed(object sender, FormClosedEventArgs e) {
            OnExitRequested(EventArgs.Empty);
        }

        protected virtual void OnExitRequested(EventArgs e) {
            if (ExitRequested != null)
                ExitRequested(this, e);
        }
    }
}
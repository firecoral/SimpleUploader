using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.Data;
using System.Net;
using DigiProofs.SoapUpload;

namespace UploadExpress
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class UploadExpress : System.Windows.Forms.Form {
	private string prefPath;
	private string acctsPath;
	public string dataDirPath;
	private AccountList accounts;
	private Account currentAccount;
	private AccountListDlg accountListDlg;
	private TreeNode contextNode;	    // To keep track of the object associated with a ContextMenu

	
	private System.Windows.Forms.MainMenu mainMenu1;
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
	    //
	    // Required for Windows Form Designer support
	    //
	    InitializeComponent();
	    this.new_btn.Tag = menuNew;
	    this.import_btn.Tag = menuImport;
	    this.upload_btn.Tag = menuUpload;
	    this.purge_btn.Tag = menuPurge;
	    Setup();

	    // Get Current account list
	    try {
		accounts = AccountList.GetAccounts(acctsPath);
	    }
	    catch {
		accounts = new AccountList(acctsPath);
	    }
	    if (accounts.Count == 0) {
		statusBar1.Text = "No DigiProofs Account";
		MessageBox.Show("Please use File -> Accounts to add your DigiProofs account.",
		    "UploadExpress",
		    MessageBoxButtons.OK,
		    MessageBoxIcon.Warning);
	    }

	    accounts.AccountListChanged += new AccountList.AccountListChangedHandler(accountListChanged);
	    accountListChanged(accounts, EventArgs.Empty);
	}

	public void Setup() {
	    // Generate the preferences path and make sure our
	    // ApplicationData directory exists.
	    prefPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
	    prefPath += Path.DirectorySeparatorChar;
	    prefPath += "DigiProofs";
	    if (!Directory.Exists(prefPath)) {
		try {
		    Directory.CreateDirectory(prefPath);
		}
		catch {
		    //XXX  process Error
		}
	    }
	    prefPath += Path.DirectorySeparatorChar;
	    acctsPath = prefPath;
	    prefPath += "prefs";
	    acctsPath += "accounts";

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
	}

	public Account GetCurrentAccount() {
	    return currentAccount;
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

	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main() {
	    // Should catch all stray exception here. XXX
	    Application.Run(new UploadExpress());
	}

	#region MenuActions
	//
	// Invoke the properties dialog
	//
	private void menuExit_Click(object sender, System.EventArgs e) {
	    this.Dispose();
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
	private void menuNew_Click(object sender, System.EventArgs e) {
	    if (currentAccount == null) {
		MessageBox.Show("Please use File -> Account to setup your DigiProofs Account.",
		    "UploadExpress",
		    MessageBoxButtons.OK,
		    MessageBoxIcon.Error);
		return;
	    }
	    try {
		if (currentAccount.Session.EventList.Length == 0) {
		    MessageBox.Show("You have no events available for uploading in this account." + Environment.NewLine +
			"Please log into your account and purchase a new event for this upload.",
			"UploadExpress",
			MessageBoxButtons.OK,
			MessageBoxIcon.Error);
		    return;
		}
	    }
	    catch (SessionException ex) {
		switch (ex.Error) {
		    case SessionError.LoginFail:
			MessageBox.Show("Couldn't log into " + currentAccount.Email + "." + Environment.NewLine + "Please check your email and password.",
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


	    EventChooser chooseEvent = new EventChooser(new ArrayList(currentAccount.Session.EventList));
	    chooseEvent.ShowDialog();
	    if (chooseEvent.DialogResult == DialogResult.OK) {
		Event ev = chooseEvent.GetSelectedEvent();
		UploadSet uploadSet = new UploadSet(dataDirPath, ev.eventID, ev.title, this, currentAccount.Email);
		currentAccount.UploadSetList.Add(uploadSet);
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
	    About aboutDlg = new About();
	    aboutDlg.ShowDialog();
	}

	//
	// Try to provide some help.
	//
	private void menuHelp_Click(object sender, System.EventArgs e) {
	    if (currentAccount == null)
		Process.Start("http://www.digiproofs.com/UploaderHelp.pdf");
	    else
		Process.Start("http://" + currentAccount.Server + "/UploaderHelp.pdf");
	}


	// 
	// Import new pages and images into the selected uploadset.
	//
	private void menuImport_Click(object sender, System.EventArgs e) {
	    if (currentAccount == null) {
		MessageBox.Show("Please use File -> Account to setup your DigiProofs Account.",
		    "UploadExpress",
		    MessageBoxButtons.OK,
		    MessageBoxIcon.Error);
		return;
	    }

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
	    string path = ((UploadSet)node.Tag).Import(this.currentAccount.SelectedPath);
	    if (path != null) {
		currentAccount.SelectedPath = path;	// Save current selected path
		accounts.Serialize();
	    }
	}

	private void start_Click(object sender, System.EventArgs e) {
	    if (currentAccount.Upload == null) {
		currentAccount.Upload = new Upload();
	    }
	    Upload upload = currentAccount.Upload;
	    if (upload.Uploading()) {
		upload.Show();
		upload.BringToFront();
		return;
	    }
	    if (upload.Setup(currentAccount))
		upload.Next();		// Start the upload
	}

	// 
	// Refresh the login for the current account.
	//
	private void menuRefresh_Click(object sender, System.EventArgs e) {
	    if (currentAccount == null) {
		MessageBox.Show("Please use File -> Account to setup your DigiProofs Account.",
		    "UploadExpress",
		    MessageBoxButtons.OK,
		    MessageBoxIcon.Error);
		return;
	    }
	    try {
		statusBar1.Text = "Logging into " + currentAccount.Email;
		currentAccount.Session.Login();
		statusBar1.Text = "Logged in as " + currentAccount.Email;
	    }
	    catch (WebException ex) {
		if ((int)((HttpWebResponse)ex.Response).StatusCode == 400) {
		    MessageBox.Show("Couldn't log into " + currentAccount.Email + ".  Please check your email and password.",
			"UploadExpress",
			MessageBoxButtons.OK,
			MessageBoxIcon.Error);
		    statusBar1.Text = "Login Error";
		}
		else {
		    MessageBox.Show(ex.Message + " Status: " + ex.Status,
			"UploadExpress",
			MessageBoxButtons.OK,
			MessageBoxIcon.Error);
		    statusBar1.Text = "Login Error";
		}
	    }
	}

	// 
	// Show the log for the current Session.
	//
	private void menuLog_Click(object sender, System.EventArgs e) {
	    if (currentAccount == null) {
		MessageBox.Show("Please use File -> Account to setup your DigiProofs Account.",
		    "UploadExpress",
		    MessageBoxButtons.OK,
		    MessageBoxIcon.Error);
		return;
	    }
	    new LogViewer(currentAccount.Session.GetLogs()).Show();
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
	// If the currentAccount is set and still exists in the account list,
	// leave that one selected.  Otherwise, use the default Object.
	private void accountListChanged(object sender, System.EventArgs e) {
	    AccountList accounts = (AccountList)sender;
	    Account selectedAccount = null;	    // Which account should be selected
	    foreach (Account acct in accounts) {
		if (acct == currentAccount) {
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

        private void accountList1_SelectedIndexChanged(object sender, System.EventArgs e) {
	    currentAccount = (Account)accountList1.SelectedItem;
	    if (currentAccount == null) {
		statusBar1.Text = "Not logged in";
	    }
	    else {
		Cursor.Current = Cursors.WaitCursor;
		treeView1.Nodes.Clear();
		// Need to do a lot with error checking here.
		try {
		    statusBar1.Text = "Logging into " + currentAccount.Email;
		    if (!currentAccount.Session.LoggedIn)	// This may try to login twice (on error)
			currentAccount.Session.Login();
		    statusBar1.Text = "Logged in as " + currentAccount.Email;
		}
		catch (SessionException ex) {
		    switch (ex.Error) {
			case SessionError.LoginFail:
			    MessageBox.Show("Couldn't log into " + currentAccount.Email + "." + Environment.NewLine + "Please check your email and password.",
				"UploadExpress",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			    statusBar1.Text = "Login Failed";
			    break;
			case SessionError.NetworkError:
			    MessageBox.Show("Connection Error: Couldn't connect to the DigiProofs uploaders." + 
					    Environment.NewLine + 
					    "Please make sure your computer is connected to the internet." +
					    Environment.NewLine +
					    "If the problem persists, please contact DigiProofs support (support@digiproofs.com).",
				"UploadExpress",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
				statusBar1.Text = "Connection Failed";
			    break;
			case SessionError.ServerError:
			    MessageBox.Show("Server Error: An error occurred on the DigiProofs uploaders." + 
					    Environment.NewLine + 
					    "Please try restarting this application." +
					    Environment.NewLine +
					    "If the problem persists, please contact DigiProofs support (support@digiproofs.com).",
				"UploadExpress",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			    statusBar1.Text = "Server Error";
			    break;
			case SessionError.UnknownSoapError:
			    MessageBox.Show("Connection Error: Couldn't log into DigiProofs Upload Service." + Environment.NewLine + "Error: " + ex.Message,
				"UploadExpress",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			    statusBar1.Text = "Login Error";
			    break;
			default:
			    MessageBox.Show("Unexpected Error - Could not log in:" + Environment.NewLine + "Error: " + ex.Message,
				"UploadExpress",
				MessageBoxButtons.OK,
				MessageBoxIcon.Error);
			    statusBar1.Text = "Login Error";
			    break;
		    }
		}
		// now populate the UploadSetList
		if (currentAccount.UploadSetList == null)
		    currentAccount.RefreshUploadSets(this, dataDirPath);
		foreach (UploadSet uploadSet in currentAccount.UploadSetList) {
		    treeView1.Nodes.Add(uploadSet.node);
		}
		Cursor.Current = Cursors.Arrow;
	    }
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
		currentAccount.UploadSetList.Remove((UploadSet)(contextNode.Tag));
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
	    foreach (UploadSet uploadSet in currentAccount.UploadSetList.ToArray()) {
		if (uploadSet.Status == UploadStatus.Complete) {
		    currentAccount.UploadSetList.Remove(uploadSet);
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
}

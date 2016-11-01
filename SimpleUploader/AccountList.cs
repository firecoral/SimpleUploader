using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace UploadExpress
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class AccountListDlg : System.Windows.Forms.Form
    {
	private System.Windows.Forms.Button add_btn;
	private System.Windows.Forms.Button remove_btn;
	private System.Windows.Forms.Button properties_btn;
	private System.Windows.Forms.Button close_btn;
	private System.Windows.Forms.ListView listView1;
	private System.Windows.Forms.ColumnHeader columnHeader1;
	private System.Windows.Forms.ColumnHeader columnHeader2;
	private System.Windows.Forms.Button default_btn;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;
	private AccountList accountList;

	public AccountListDlg(AccountList accounts) {
	    //
	    // Required for Windows Form Designer support
	    //
	    InitializeComponent();
	    accountListChanged(accounts, EventArgs.Empty);
	    accounts.AccountListChanged += new AccountList.AccountListChangedHandler(accountListChanged);
	}
	
	private void accountListChanged(object sender, System.EventArgs e) {
	    AccountList accounts = (AccountList)sender;
	    accountList = accounts;
	    listView1.BeginUpdate();
	    listView1.Items.Clear();

	    foreach (Account acct in accounts) {
		listView1.Items.Add(acct);
	    }
	    listView1.EndUpdate();
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
	    this.add_btn = new System.Windows.Forms.Button();
	    this.remove_btn = new System.Windows.Forms.Button();
	    this.properties_btn = new System.Windows.Forms.Button();
	    this.close_btn = new System.Windows.Forms.Button();
	    this.listView1 = new System.Windows.Forms.ListView();
	    this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
	    this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
	    this.default_btn = new System.Windows.Forms.Button();
	    this.SuspendLayout();
	    // 
	    // add_btn
	    // 
	    this.add_btn.Location = new System.Drawing.Point(336, 32);
	    this.add_btn.Name = "add_btn";
	    this.add_btn.Size = new System.Drawing.Size(88, 23);
	    this.add_btn.TabIndex = 0;
	    this.add_btn.Text = "Add";
	    this.add_btn.Click += new System.EventHandler(this.add_btn_Click);
	    // 
	    // remove_btn
	    // 
	    this.remove_btn.Enabled = false;
	    this.remove_btn.Location = new System.Drawing.Point(336, 72);
	    this.remove_btn.Name = "remove_btn";
	    this.remove_btn.Size = new System.Drawing.Size(88, 23);
	    this.remove_btn.TabIndex = 1;
	    this.remove_btn.Text = "Remove";
	    this.remove_btn.Click += new System.EventHandler(this.remove_btn_Click);
	    // 
	    // properties_btn
	    // 
	    this.properties_btn.Enabled = false;
	    this.properties_btn.Location = new System.Drawing.Point(336, 112);
	    this.properties_btn.Name = "properties_btn";
	    this.properties_btn.Size = new System.Drawing.Size(88, 23);
	    this.properties_btn.TabIndex = 2;
	    this.properties_btn.Text = "Properties";
	    this.properties_btn.Click += new System.EventHandler(this.properties_btn_Click);
	    // 
	    // close_btn
	    // 
	    this.close_btn.Location = new System.Drawing.Point(336, 288);
	    this.close_btn.Name = "close_btn";
	    this.close_btn.Size = new System.Drawing.Size(88, 23);
	    this.close_btn.TabIndex = 3;
	    this.close_btn.Text = "Close";
	    this.close_btn.Click += new System.EventHandler(this.close_btn_Click);
	    // 
	    // listView1
	    // 
	    this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
											this.columnHeader1,
											this.columnHeader2});
	    this.listView1.FullRowSelect = true;
	    this.listView1.HideSelection = false;
	    this.listView1.Location = new System.Drawing.Point(32, 32);
	    this.listView1.MultiSelect = false;
	    this.listView1.Name = "listView1";
	    this.listView1.Size = new System.Drawing.Size(288, 240);
	    this.listView1.TabIndex = 4;
	    this.listView1.View = System.Windows.Forms.View.Details;
	    this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
	    // 
	    // columnHeader1
	    // 
	    this.columnHeader1.Text = "Account Email";
	    this.columnHeader1.Width = 152;
	    // 
	    // columnHeader2
	    // 
	    this.columnHeader2.Text = "Default";
	    this.columnHeader2.Width = 131;
	    // 
	    // default_btn
	    // 
	    this.default_btn.Enabled = false;
	    this.default_btn.Location = new System.Drawing.Point(336, 152);
	    this.default_btn.Name = "default_btn";
	    this.default_btn.Size = new System.Drawing.Size(88, 23);
	    this.default_btn.TabIndex = 5;
	    this.default_btn.Text = "Set as Default";
	    this.default_btn.Click += new System.EventHandler(this.default_btn_Click);
	    // 
	    // AccountListDlg
	    // 
	    this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
	    this.ClientSize = new System.Drawing.Size(440, 342);
	    this.Controls.Add(this.default_btn);
	    this.Controls.Add(this.listView1);
	    this.Controls.Add(this.close_btn);
	    this.Controls.Add(this.properties_btn);
	    this.Controls.Add(this.remove_btn);
	    this.Controls.Add(this.add_btn);
	    this.MaximizeBox = false;
	    this.MinimizeBox = false;
	    this.Name = "AccountListDlg";
	    this.ShowInTaskbar = false;
	    this.Text = "Accounts";
	    this.Closing += new System.ComponentModel.CancelEventHandler(this.AccountListDlg_Closing);
	    this.ResumeLayout(false);

	}
	#endregion

	private void close_btn_Click(object sender, System.EventArgs e) {
	    this.Hide();
	}

	private void AccountListDlg_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
	    e.Cancel = true;
	    this.Hide();
	}

	private void add_btn_Click(object sender, System.EventArgs e) {
	    AccountDlg accountDlg = new AccountDlg();
	    accountDlg.ShowDialog();
	    if (accountDlg.DialogResult == DialogResult.OK) {
		// verify that email is not already in use
		foreach (Account acct in accountList) {
		    if (acct.Email == accountDlg.Account.Email) {
			MessageBox.Show("You already have an account for the email " + acct.Email + ".",
			    "UploadExpress",
			    MessageBoxButtons.OK,
			    MessageBoxIcon.Error);
			return;
		    }
		}
		accountList.Add(accountDlg.Account);
	    }
	}

        private void listView1_SelectedIndexChanged(object sender, System.EventArgs e) {
	    ListView.SelectedListViewItemCollection selected = listView1.SelectedItems;
	    if (selected.Count == 0) {
		remove_btn.Enabled = false;
		properties_btn.Enabled = false;
		default_btn.Enabled = false;
	    }
	    else {
		Account acct = (Account)selected[0];
		remove_btn.Enabled = true;
		properties_btn.Enabled = true;
		if (acct.IsDefault)
		    default_btn.Enabled = false;
		else
		    default_btn.Enabled = true;
	    }    		
	}

        private void default_btn_Click(object sender, System.EventArgs e) {
	    Account acct = (Account)listView1.SelectedItems[0];
	    accountList.DefaultAccount = acct;
	}

        private void properties_btn_Click(object sender, System.EventArgs e) {
	    AccountDlg accountDlg = new AccountDlg((Account)listView1.SelectedItems[0]);
	    accountDlg.ShowDialog();
	    if (accountDlg.DialogResult == DialogResult.OK) {
		// verify that email is not already in use
		foreach (Account acct in accountList) {
		    if (acct != accountDlg.Account && acct.Email == accountDlg.Account.Email) {
			MessageBox.Show("You already have an account for the email " + acct.Email + ".",
			    "UploadExpress",
			    MessageBoxButtons.OK,
			    MessageBoxIcon.Error);
			return;
		    }
		}
		// May need to change emails in all the UploadMgrs here.
		accountList.Refresh();
	    }
	}

        private void remove_btn_Click(object sender, System.EventArgs e) {
	    Account acct = (Account)listView1.SelectedItems[0];
	    accountList.Remove(acct);
	    if (accountList.Count == 0) {
		remove_btn.Enabled = false;
		properties_btn.Enabled = false;
		default_btn.Enabled = false;
	    }
	}
    }
}

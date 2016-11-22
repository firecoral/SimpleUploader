using System;
using System.Windows.Forms;

namespace UploadExpress
{
    /// <summary>
    /// Summary description for Form2.
    /// </summary>
    public class AccountDlg : System.Windows.Forms.Form {
	private System.Windows.Forms.Button ok_btn;
	private System.Windows.Forms.Button cancel_btn;
	private System.Windows.Forms.TabPage tabPage1;
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.TabControl tab;
	private System.Windows.Forms.TabPage tabPage2;
	private System.Windows.Forms.TextBox email;
	private System.Windows.Forms.TextBox server;
	private System.Windows.Forms.Label label3;
        private System.Windows.Forms.NumericUpDown images;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
	private bool tab2shown;

	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox sortOrder;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox proxyOn;
        private System.Windows.Forms.TextBox proxyHost;
        private System.Windows.Forms.TextBox proxyPort;
        private GroupBox groupBox2;
        private TrackBar amount;
        private CheckBox compression;
        private Label amountVal;

        public Account Account {
	    get {return acct;}
	}
	private Account acct;
        
	// For editing an existing account
	public AccountDlg(Account acct) {
	    //
	    // Required for Windows Form Designer support
	    //
	    InitializeComponent();
	    this.tab.Controls.Remove(this.tabPage2);	    // Hide this page for now.
	    tab2shown = false;
	    this.acct = acct;
	    Initialize(acct);
	}

	// For a new account
	public AccountDlg() {
	    //
	    // Required for Windows Form Designer support
	    //
	    InitializeComponent();
	    this.tab.Controls.Remove(this.tabPage2);	    // Hide this page for now.
	    tab2shown = false;
	    acct = new Account();
	    Initialize(acct);
	}

	private void Initialize(Account acct) {
	    email.Text = acct.Email;
	    server.Text = acct.Server;

	    sortOrder.Items.Clear();
	    sortOrder.Items.Insert((int)Account.SortOrders.Name, "Name");
	    sortOrder.Items.Insert((int)Account.SortOrders.CreateDate, "Date Created");
	    sortOrder.Items.Insert((int)Account.SortOrders.Unsorted, "Unsorted");
	    sortOrder.SelectedIndex = (int)acct.SortOrder;

	    proxyOn.Checked = acct.ProxyOn;
	    proxyHost.Text = acct.ProxyHost;
	    proxyPort.Text = acct.ProxyPort.ToString();
	    if (acct.ProxyOn) {
		proxyHost.Enabled = true;
		proxyPort.Enabled = true;
	    }
	    else {
		proxyHost.Enabled = false;
		proxyPort.Enabled = false;
	    }

	    amountVal.Text = acct.CompressionRate.ToString();
	    amount.Value = acct.CompressionRate / 10;
	    compression.Checked = acct.UseCompression;
	    amount.Enabled = acct.UseCompression;
	    amountVal.Enabled = acct.UseCompression;
	    this.images.Value = new System.Decimal(new int[] {
								 acct.MaxPageImages,
								 0,
								 0,
								 0});
	}

	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose(bool disposing) {
	    if (disposing) {
		if(components != null) {
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
            this.ok_btn = new System.Windows.Forms.Button();
            this.cancel_btn = new System.Windows.Forms.Button();
            this.tab = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.proxyPort = new System.Windows.Forms.TextBox();
            this.proxyHost = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.proxyOn = new System.Windows.Forms.CheckBox();
            this.sortOrder = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.images = new System.Windows.Forms.NumericUpDown();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.email = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.label3 = new System.Windows.Forms.Label();
            this.server = new System.Windows.Forms.TextBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.amount = new System.Windows.Forms.TrackBar();
            this.compression = new System.Windows.Forms.CheckBox();
            this.amountVal = new System.Windows.Forms.Label();
            this.tab.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.images)).BeginInit();
            this.tabPage2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.amount)).BeginInit();
            this.SuspendLayout();
            // 
            // ok_btn
            // 
            this.ok_btn.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.ok_btn.Location = new System.Drawing.Point(248, 547);
            this.ok_btn.Name = "ok_btn";
            this.ok_btn.Size = new System.Drawing.Size(75, 23);
            this.ok_btn.TabIndex = 0;
            this.ok_btn.Text = "OK";
            this.ok_btn.Click += new System.EventHandler(this.ok_btn_Click);
            // 
            // cancel_btn
            // 
            this.cancel_btn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.cancel_btn.Location = new System.Drawing.Point(344, 547);
            this.cancel_btn.Name = "cancel_btn";
            this.cancel_btn.Size = new System.Drawing.Size(75, 23);
            this.cancel_btn.TabIndex = 1;
            this.cancel_btn.Text = "Cancel";
            // 
            // tab
            // 
            this.tab.Controls.Add(this.tabPage1);
            this.tab.Controls.Add(this.tabPage2);
            this.tab.Location = new System.Drawing.Point(8, 24);
            this.tab.Name = "tab";
            this.tab.SelectedIndex = 0;
            this.tab.Size = new System.Drawing.Size(432, 502);
            this.tab.TabIndex = 2;
            this.tab.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AccountDlg_KeyDown);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.groupBox2);
            this.tabPage1.Controls.Add(this.groupBox1);
            this.tabPage1.Controls.Add(this.sortOrder);
            this.tabPage1.Controls.Add(this.label6);
            this.tabPage1.Controls.Add(this.images);
            this.tabPage1.Controls.Add(this.label5);
            this.tabPage1.Controls.Add(this.label4);
            this.tabPage1.Controls.Add(this.email);
            this.tabPage1.Controls.Add(this.label1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Size = new System.Drawing.Size(424, 476);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Account";
            this.tabPage1.Click += new System.EventHandler(this.tabPage1_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.proxyPort);
            this.groupBox1.Controls.Add(this.proxyHost);
            this.groupBox1.Controls.Add(this.label8);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.proxyOn);
            this.groupBox1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.groupBox1.Location = new System.Drawing.Point(15, 298);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(392, 160);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Proxy Settings";
            this.groupBox1.Enter += new System.EventHandler(this.groupBox1_Enter);
            // 
            // proxyPort
            // 
            this.proxyPort.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.proxyPort.Location = new System.Drawing.Point(144, 112);
            this.proxyPort.Name = "proxyPort";
            this.proxyPort.Size = new System.Drawing.Size(72, 22);
            this.proxyPort.TabIndex = 4;
            // 
            // proxyHost
            // 
            this.proxyHost.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.proxyHost.Location = new System.Drawing.Point(144, 72);
            this.proxyHost.Name = "proxyHost";
            this.proxyHost.Size = new System.Drawing.Size(152, 22);
            this.proxyHost.TabIndex = 3;
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(16, 120);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(112, 24);
            this.label8.TabIndex = 2;
            this.label8.Text = "Port:";
            // 
            // label7
            // 
            this.label7.Location = new System.Drawing.Point(16, 80);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(112, 24);
            this.label7.TabIndex = 1;
            this.label7.Text = "Address:";
            // 
            // proxyOn
            // 
            this.proxyOn.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.proxyOn.Location = new System.Drawing.Point(16, 32);
            this.proxyOn.Name = "proxyOn";
            this.proxyOn.Size = new System.Drawing.Size(144, 24);
            this.proxyOn.TabIndex = 0;
            this.proxyOn.Text = "Use Proxy";
            this.proxyOn.CheckedChanged += new System.EventHandler(this.proxyOn_CheckedChanged);
            // 
            // sortOrder
            // 
            this.sortOrder.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.sortOrder.Location = new System.Drawing.Point(160, 138);
            this.sortOrder.Name = "sortOrder";
            this.sortOrder.Size = new System.Drawing.Size(160, 21);
            this.sortOrder.TabIndex = 10;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(24, 138);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(136, 32);
            this.label6.TabIndex = 9;
            this.label6.Text = "Image Sorting";
            // 
            // images
            // 
            this.images.Location = new System.Drawing.Point(160, 90);
            this.images.Maximum = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.images.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.images.Name = "images";
            this.images.Size = new System.Drawing.Size(50, 20);
            this.images.TabIndex = 8;
            this.images.Value = new decimal(new int[] {
            36,
            0,
            0,
            0});
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(224, 92);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 16);
            this.label5.TabIndex = 7;
            this.label5.Text = "(maximum 200)";
            this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(24, 90);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(152, 23);
            this.label4.TabIndex = 6;
            this.label4.Text = "Images Per Page";
            // 
            // email
            // 
            this.email.Location = new System.Drawing.Point(160, 48);
            this.email.MaxLength = 75;
            this.email.Name = "email";
            this.email.Size = new System.Drawing.Size(176, 20);
            this.email.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(24, 48);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 23);
            this.label1.TabIndex = 2;
            this.label1.Text = "Email";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.label3);
            this.tabPage2.Controls.Add(this.server);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Size = new System.Drawing.Size(424, 476);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Settings";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(33, 113);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 23);
            this.label3.TabIndex = 1;
            this.label3.Text = "Server";
            // 
            // server
            // 
            this.server.Location = new System.Drawing.Point(113, 113);
            this.server.MaxLength = 128;
            this.server.Name = "server";
            this.server.Size = new System.Drawing.Size(256, 20);
            this.server.TabIndex = 0;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.amountVal);
            this.groupBox2.Controls.Add(this.amount);
            this.groupBox2.Controls.Add(this.compression);
            this.groupBox2.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(15, 187);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(392, 100);
            this.groupBox2.TabIndex = 12;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Compression";
            // 
            // amount
            // 
            this.amount.LargeChange = 1;
            this.amount.Location = new System.Drawing.Point(192, 43);
            this.amount.Name = "amount";
            this.amount.Size = new System.Drawing.Size(104, 45);
            this.amount.TabIndex = 6;
            this.amount.ValueChanged += new System.EventHandler(this.amount_ValueChanged);
            // 
            // compression
            // 
            this.compression.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.compression.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.compression.Location = new System.Drawing.Point(20, 43);
            this.compression.Name = "compression";
            this.compression.Size = new System.Drawing.Size(140, 24);
            this.compression.TabIndex = 5;
            this.compression.Text = "Enable";
            this.compression.CheckedChanged += new System.EventHandler(this.compression_CheckedChanged);
            // 
            // amountVal
            // 
            this.amountVal.Location = new System.Drawing.Point(333, 45);
            this.amountVal.Name = "amountVal";
            this.amountVal.Size = new System.Drawing.Size(36, 23);
            this.amountVal.TabIndex = 7;
            this.amountVal.Text = "100";
            // 
            // AccountDlg
            // 
            this.AcceptButton = this.ok_btn;
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.CancelButton = this.cancel_btn;
            this.ClientSize = new System.Drawing.Size(448, 594);
            this.ControlBox = false;
            this.Controls.Add(this.tab);
            this.Controls.Add(this.cancel_btn);
            this.Controls.Add(this.ok_btn);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AccountDlg";
            this.Text = "Properties";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AccountDlg_KeyDown);
            this.tab.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.tabPage1.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.images)).EndInit();
            this.tabPage2.ResumeLayout(false);
            this.tabPage2.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.amount)).EndInit();
            this.ResumeLayout(false);

	}
	#endregion

        private void ok_btn_Click(object sender, System.EventArgs e) {
	    acct.Email = email.Text;
	    acct.ProxyOn = proxyOn.Checked;
	    acct.ProxyHost = proxyHost.Text;
	    acct.ProxyPort = Convert.ToInt32(proxyPort.Text);
	    acct.MaxPageImages = (int)images.Value;
	    acct.SortOrder = (Account.SortOrders)sortOrder.SelectedIndex;
	    acct.Server = server.Text;
	    acct.UseCompression = compression.Checked;
	    acct.CompressionRate = amount.Value * 10;
	    // XXX if (acct.Session != null)
		// XXX acct.Session.Update(acct.Server, acct.Email, acct.Password);
	}

        // Update the label when the trackbar is changed.
	private void amount_ValueChanged(object sender, System.EventArgs e) {
	    amountVal.Text = (amount.Value * 10).ToString();
	}

        private void compression_CheckedChanged(object sender, System.EventArgs e) {
	    amount.Enabled = compression.Checked;
	    amountVal.Enabled = compression.Checked;
	}

        private void AccountDlg_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e) {
	    if (e.KeyCode == Keys.F1 && e.Shift && e.Control) {
		if (tab2shown) {
		    this.tab.Controls.Remove(this.tabPage2);
		    e.Handled = true;
		    tab2shown = false;
		}
		else {
		    this.tab.Controls.Add(this.tabPage2);
		    e.Handled = true;
		    tab2shown = true;
		}
	    }
	}

        private void proxyOn_CheckedChanged(object sender, System.EventArgs e) {
	    if (proxyOn.Checked) {
		proxyHost.Enabled = true;
		proxyPort.Enabled = true;
	    }
	    else {
		proxyHost.Enabled = false;
		proxyPort.Enabled = false;
	    }
	}

        private void tabPage1_Click(object sender, EventArgs e) {

        }

        private void groupBox1_Enter(object sender, EventArgs e) {

        }
    }
}

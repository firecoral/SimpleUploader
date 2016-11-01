using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace UploadExpress
{
	/// <summary>
	/// Summary description for About.
	/// </summary>
	public class About : System.Windows.Forms.Form
	{
	    private System.Windows.Forms.Label version;
	    private System.Windows.Forms.Label title;
	    private System.Windows.Forms.Label copyright;
	private System.Windows.Forms.Button okay;
	    private System.Windows.Forms.Label label1;
	    /// <summary>
	    /// Required designer variable.
	    /// </summary>
	    private System.ComponentModel.Container components = null;

	    public About() {
		    //
		// Required for Windows Form Designer support
		//
		InitializeComponent();
//		Assembly a = Assembly.GetExecutingAssembly();
//		Version v = a.GetName().Version;
//		version.Text = String.Format("Version {0}.{1}.{2}.{3}", v.Major, v.Minor, v.Build, v.Revision);
		version.Text = "Version " + Application.ProductVersion;
		copyright.Text = "Copyright 2012 " + Application.CompanyName;
		title.Text = Application.ProductName;
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
		this.version = new System.Windows.Forms.Label();
		this.title = new System.Windows.Forms.Label();
		this.copyright = new System.Windows.Forms.Label();
		this.okay = new System.Windows.Forms.Button();
		this.label1 = new System.Windows.Forms.Label();
		this.SuspendLayout();
		// 
		// version
		// 
		this.version.Location = new System.Drawing.Point(24, 56);
		this.version.Name = "version";
		this.version.Size = new System.Drawing.Size(264, 24);
		this.version.TabIndex = 0;
		// 
		// title
		// 
		this.title.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
		this.title.Location = new System.Drawing.Point(24, 32);
		this.title.Name = "title";
		this.title.Size = new System.Drawing.Size(264, 24);
		this.title.TabIndex = 1;
		// 
		// copyright
		// 
		this.copyright.Location = new System.Drawing.Point(24, 72);
		this.copyright.Name = "copyright";
		this.copyright.Size = new System.Drawing.Size(264, 24);
		this.copyright.TabIndex = 2;
		// 
		// okay
		// 
		this.okay.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.okay.Location = new System.Drawing.Point(96, 176);
		this.okay.Name = "okay";
		this.okay.Size = new System.Drawing.Size(128, 32);
		this.okay.TabIndex = 3;
		this.okay.Text = "OK";
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(24, 120);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(272, 40);
		this.label1.TabIndex = 4;
		this.label1.Text = "For information about or assistance with this product please contact DigiProofs a" +
		    "t support@digiproofs.com or 650-691-4040.";
		// 
		// About
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(336, 238);
		this.Controls.Add(this.label1);
		this.Controls.Add(this.okay);
		this.Controls.Add(this.copyright);
		this.Controls.Add(this.title);
		this.Controls.Add(this.version);
		this.Name = "About";
		this.Text = "About";
		this.ResumeLayout(false);

	    }
	    #endregion

	}
}

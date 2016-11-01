using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

namespace UploadExpress
{
    /// <summary>
    /// Summary description for LogViewer.
    /// </summary>
    public class LogViewer : System.Windows.Forms.Form
    {
	private System.Windows.Forms.Panel panel1;
	private System.Windows.Forms.Button save_btn;
	private System.Windows.Forms.Button close_btn;
	private System.Windows.Forms.TextBox textBox1;
	    /// <summary>
	    /// Required designer variable.
	    /// </summary>
	    private System.ComponentModel.Container components = null;

	    public LogViewer(string text)
	    {
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
		textBox1.Text = text;

		//
		// TODO: Add any constructor code after InitializeComponent call
		//
	    }

	    /// <summary>
	    /// Clean up any resources being used.
	    /// </summary>
	    protected override void Dispose( bool disposing )
	    {
		    if( disposing )
		    {
			    if(components != null)
			    {
				    components.Dispose();
			    }
		    }
		    base.Dispose( disposing );
	    }

	    #region Windows Form Designer generated code
	    /// <summary>
	    /// Required method for Designer support - do not modify
	    /// the contents of this method with the code editor.
	    /// </summary>
	    private void InitializeComponent()
	    {
		this.panel1 = new System.Windows.Forms.Panel();
		this.save_btn = new System.Windows.Forms.Button();
		this.close_btn = new System.Windows.Forms.Button();
		this.textBox1 = new System.Windows.Forms.TextBox();
		this.panel1.SuspendLayout();
		this.SuspendLayout();
		// 
		// panel1
		// 
		this.panel1.Controls.Add(this.close_btn);
		this.panel1.Controls.Add(this.save_btn);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
		this.panel1.Location = new System.Drawing.Point(0, 0);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(504, 56);
		this.panel1.TabIndex = 0;
		// 
		// save_btn
		// 
		this.save_btn.Location = new System.Drawing.Point(32, 16);
		this.save_btn.Name = "save_btn";
		this.save_btn.Size = new System.Drawing.Size(88, 23);
		this.save_btn.TabIndex = 0;
		this.save_btn.Text = "Save To File";
		this.save_btn.Click += new System.EventHandler(this.save_btn_Click);
		// 
		// close_btn
		// 
		this.close_btn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.close_btn.Location = new System.Drawing.Point(384, 16);
		this.close_btn.Name = "close_btn";
		this.close_btn.TabIndex = 1;
		this.close_btn.Text = "Close";
		this.close_btn.Click += new System.EventHandler(this.close_btn_Click);
		// 
		// textBox1
		// 
		this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textBox1.HideSelection = false;
		this.textBox1.Location = new System.Drawing.Point(0, 56);
		this.textBox1.Multiline = true;
		this.textBox1.Name = "textBox1";
		this.textBox1.ReadOnly = true;
		this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.textBox1.Size = new System.Drawing.Size(504, 502);
		this.textBox1.TabIndex = 1;
		this.textBox1.Text = "";
		// 
		// LogViewer
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.CancelButton = this.close_btn;
		this.ClientSize = new System.Drawing.Size(504, 558);
		this.Controls.Add(this.textBox1);
		this.Controls.Add(this.panel1);
		this.MinimizeBox = false;
		this.Name = "LogViewer";
		this.Text = "Session Log Viewer";
		this.panel1.ResumeLayout(false);
		this.ResumeLayout(false);

	    }
	    #endregion

	private void close_btn_Click(object sender, System.EventArgs e) {
	    Close();
	}

	private void save_btn_Click(object sender, System.EventArgs e) {
	    SaveFileDialog dlg = new SaveFileDialog();
	    dlg.FileName = "UploadLog.txt";

	    if(dlg.ShowDialog() == DialogResult.OK) {
		Stream myStream;
		if ((myStream = dlg.OpenFile()) != null) {
		    StreamWriter strm = new StreamWriter(myStream);
		    strm.Write(textBox1.Text);
		    strm.Close();
                    myStream.Close();
		}
	    }
	}
    }
}

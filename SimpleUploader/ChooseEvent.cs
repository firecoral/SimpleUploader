using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using DigiProofs.SoapUpload;

namespace UploadExpress
{
	/// <summary>
	/// Summary description for Form2.
	/// </summary>
	public class EventChooser : System.Windows.Forms.Form {
	    private System.Windows.Forms.Label label1;
	    private System.Windows.Forms.Button button1;
	    private System.Windows.Forms.Button button2;
	    private System.Windows.Forms.ComboBox eventList;
	    /// <summary>
	    /// Required designer variable.
	    /// </summary>
	    private System.ComponentModel.Container components = null;

	    public EventChooser(ArrayList events) {
		    //
		    // Required for Windows Form Designer support
		    //
		    InitializeComponent();

		    eventList.DataSource = events;
	    }

	    public Event GetSelectedEvent() {
		return (Event)eventList.SelectedItem;
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
		this.label1 = new System.Windows.Forms.Label();
		this.eventList = new System.Windows.Forms.ComboBox();
		this.button1 = new System.Windows.Forms.Button();
		this.button2 = new System.Windows.Forms.Button();
		this.SuspendLayout();
		// 
		// label1
		// 
		this.label1.Location = new System.Drawing.Point(24, 16);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(224, 23);
		this.label1.TabIndex = 0;
		this.label1.Text = "Please select the Event for this upload.";
		// 
		// eventList
		// 
		this.eventList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.eventList.Location = new System.Drawing.Point(24, 56);
		this.eventList.Name = "eventList";
		this.eventList.Size = new System.Drawing.Size(272, 21);
		this.eventList.TabIndex = 1;
		// 
		// button1
		// 
		this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
		this.button1.Location = new System.Drawing.Point(40, 104);
		this.button1.Name = "button1";
		this.button1.TabIndex = 2;
		this.button1.Text = "OK";
		// 
		// button2
		// 
		this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.button2.Location = new System.Drawing.Point(160, 104);
		this.button2.Name = "button2";
		this.button2.TabIndex = 3;
		this.button2.Text = "Cancel";
		// 
		// EventChooser
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.ClientSize = new System.Drawing.Size(336, 150);
		this.Controls.Add(this.button2);
		this.Controls.Add(this.button1);
		this.Controls.Add(this.eventList);
		this.Controls.Add(this.label1);
		this.Name = "EventChooser";
		this.Text = "Choose Event";
		this.ResumeLayout(false);

	    }
	    #endregion

	}
}

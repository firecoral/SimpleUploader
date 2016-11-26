using System;


namespace UploadExpress {
    /// <summary>
    /// Summary description for About.
    /// </summary>
    public class About : System.Windows.Forms.Form {
        private System.Windows.Forms.Label versionLabel;
        private System.Windows.Forms.Label titleLabel;
        private System.Windows.Forms.Label copyrightLabel;
        private System.Windows.Forms.Button okay;
        private System.Windows.Forms.Label label1;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public About(string name, string version, string company, string copyright) {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            // Copy passed in information (from AssemblyInfo.cs) into dialog.
            versionLabel.Text = "Version " + version;
            copyrightLabel.Text = String.Format("{0} by {1}", copyright, company);
            titleLabel.Text = name;
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
            this.versionLabel = new System.Windows.Forms.Label();
            this.titleLabel = new System.Windows.Forms.Label();
            this.copyrightLabel = new System.Windows.Forms.Label();
            this.okay = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // versionLabel
            // 
            this.versionLabel.Location = new System.Drawing.Point(24, 56);
            this.versionLabel.Name = "versionLabel";
            this.versionLabel.Size = new System.Drawing.Size(264, 24);
            this.versionLabel.TabIndex = 0;
            // 
            // titleLabel
            // 
            this.titleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.titleLabel.Location = new System.Drawing.Point(24, 32);
            this.titleLabel.Name = "titleLabel";
            this.titleLabel.Size = new System.Drawing.Size(264, 24);
            this.titleLabel.TabIndex = 1;
            // 
            // copyrightLabel
            // 
            this.copyrightLabel.Location = new System.Drawing.Point(24, 72);
            this.copyrightLabel.Name = "copyrightLabel";
            this.copyrightLabel.Size = new System.Drawing.Size(264, 24);
            this.copyrightLabel.TabIndex = 2;
            // 
            // okay
            // 
            this.okay.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okay.Location = new System.Drawing.Point(90, 148);
            this.okay.Name = "okay";
            this.okay.Size = new System.Drawing.Size(128, 32);
            this.okay.TabIndex = 3;
            this.okay.Text = "OK";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(24, 105);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(272, 40);
            this.label1.TabIndex = 4;
            this.label1.Text = "For information about or assistance with this product please contact DigiProofs a" +
    "t support@digiproofs.com";
            // 
            // About
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(319, 214);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.okay);
            this.Controls.Add(this.copyrightLabel);
            this.Controls.Add(this.titleLabel);
            this.Controls.Add(this.versionLabel);
            this.Name = "About";
            this.Text = "About";
            this.ResumeLayout(false);

        }
        #endregion
    }
}

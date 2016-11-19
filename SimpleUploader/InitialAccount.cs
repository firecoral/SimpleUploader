using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UploadExpress {
    public partial class InitialAccount : Form {
        public InitialAccount() {
            InitializeComponent();
            textBox1.Validating += textBox1_Validating;
            textBox2.Validating += textBox2_Validating;
        }
    }
}

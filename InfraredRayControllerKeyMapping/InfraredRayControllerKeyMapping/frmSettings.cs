using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace InfraredRayControllerKeyMapping
{
    public partial class frmSettings : Form
    {
        public frmSettings(UserSettings us)
        {
            InitializeComponent();

            string[] ports = System.IO.Ports.SerialPort.GetPortNames();

            if (ports != null)
            {
                this.comboBox1.Items.AddRange(ports);
            }

            if (us != null)
            {
                this.comboBox1.Text = us.SerialPort;
                this.checkBox1.Checked = us.IsAutoSaveWhenAppClosed;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        public string PortName
        {
            get
            {
                return comboBox1.Text;
            }
        }

        public bool IsSendCmdWhenReceiveKeyCode
        {
            get
            {
                return this.checkBox1.Checked;
            }
        }    
    }
}

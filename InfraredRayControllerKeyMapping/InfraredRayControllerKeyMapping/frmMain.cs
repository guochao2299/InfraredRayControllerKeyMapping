using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace InfraredRayControllerKeyMapping
{
    public partial class frmMain : Form
    {
        private Dictionary<string, InfraredRayController> m_controller = null;
        private UserSettings m_settings = null;
        private SerialPort m_port = null;

        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                this.tvControllers.SuspendLayout();

                m_settings = UserSettings.LoadFromFile();
                m_settings.UserSettingsChanged += delegate(object obj,EventArgs ea)
                {
                    this.lblStatus.Text = m_settings.ToString();
                };
                this.lblStatus.Text = m_settings.ToString();

                m_controller = InfraredRayController.LoadIRCsFromFile();

                if (m_controller.Count > 0)
                {
                    foreach (string s in m_controller.Keys)
                    {
                        this.tvControllers.Nodes.Add(new TreeNode(s));
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("加载配置文件和控制器信息失败，错误消息为:" + ex.Message);
            }
            finally
            {
                this.tvControllers.ResumeLayout();
                this.Cursor = Cursors.Default;
            }
        }


        private const int SELECE_INDEX = 0;
        private const int CODE_INDEX = 1;
        private const int CMD_INDEX = 2;
        private delegate void UpdateSerialPortDataHandler(string cnt);
        private void UpdateSerialPortData(string cnt)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new UpdateSerialPortDataHandler(UpdateSerialPortData), cnt);
            }
            else
            {
                this.dataGridView1.ClearSelection();
                string code = cnt.Replace("irCode: ", "").TrimEnd('\r','\n');
                foreach (DataGridViewRow dgvr in this.dataGridView1.Rows)
                {
                    if (string.Equals(code, Convert.ToString(dgvr.Cells[CODE_INDEX].Value), StringComparison.OrdinalIgnoreCase))
                    {
                        dgvr.Selected = true;
                        return;
                    }                    
                }

                this.dataGridView1.Rows.Add(false, code, string.Empty);

                if (this.tvControllers.SelectedNode != null)
                {
                    KeyCommandPair kcp=new KeyCommandPair();
                    kcp.Key=code;
                    m_controller[this.tvControllers.SelectedNode.Text].KeyCommands.Add(kcp);
                }
            }
        }

        private void SerialDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            UpdateSerialPortData(indata);
        }

        private void toolSerialControl_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (m_port != null && m_port.IsOpen)
                {
                    m_port.Close();
                    m_port.Dispose();
                    m_port = null;

                    this.toolSerialControl.Text = "开启串口监控";
                    this.toolSerialControl.Checked = false;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(m_settings.SerialPort))
                    {
                        MessageBox.Show("当前串口为空，请在设置窗口中选中可用端口");
                        return;
                    }

                    string[] ports = SerialPort.GetPortNames();

                    if (ports == null || ports.Length <= 0)
                    {
                        MessageBox.Show("本机没有可用串口!");
                        return;
                    }

                    if (!ports.Any(r => string.Compare(r, m_settings.SerialPort, true) == 0))
                    {
                        MessageBox.Show(string.Format("端口{0}不存在，请在设置窗口中选中可用端口"));
                        return;
                    }

                    m_port = new SerialPort(m_settings.SerialPort, 9600);
                    m_port.Parity = Parity.None;
                    m_port.StopBits = StopBits.One;
                    m_port.DataBits = 8;
                    m_port.Handshake = Handshake.None;
                    m_port.RtsEnable = true;

                    m_port.DataReceived += new SerialDataReceivedEventHandler(SerialDataReceived);

                    m_port.Open();

                    this.toolSerialControl.Text = "关闭串口监控";
                    this.toolSerialControl.Checked = true;
                }                    
            }
            catch (Exception ex)
            {
                MessageBox.Show("串口操作失败，错误消息为：" + ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void toolSettings_Click(object sender, EventArgs e)
        {
            frmSettings fs = new frmSettings(m_settings);
            if (fs.ShowDialog(this)== System.Windows.Forms.DialogResult.OK)
            {
                m_settings.IsAutoSaveWhenAppClosed = fs.IsSendCmdWhenReceiveKeyCode;
                m_settings.SerialPort = fs.PortName;
            }
        }

        private void SaveControllers()
        {
            if (m_controller.Count > 0)
            {
                foreach (InfraredRayController irc in m_controller.Values)
                {
                    irc.Serialize2File();
                }
            }
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (m_port != null && m_port.IsOpen)
                {
                    m_port.Close();
                    m_port.Dispose();
                    m_port = null;
                }

                m_settings.Serialize2File();

                if (m_settings.IsAutoSaveWhenAppClosed)
                {
                    SaveControllers();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show( ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void toolNew_Click(object sender, EventArgs e)
        {
            TreeNode tnNew = new TreeNode(string.Format("遥控器{0}",m_controller.Count+1));
            this.tvControllers.Nodes.Insert(0, tnNew);
            this.tvControllers.LabelEdit = true;
            tnNew.BeginEdit();
        }

        private void tvControllers_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.Label))
            {
                e.CancelEdit = true;
                MessageBox.Show("控制器名称不能为空或者空格，请重新填写");                
                e.Node.BeginEdit();
                return;
            }

            if (m_controller.ContainsKey(e.Label))
            {
                e.CancelEdit = true;
                MessageBox.Show(string.Format("控制器名称{0}已经存在，请重新填写", e.Node.Text));
                e.Node.BeginEdit();
                return;
            }
            
            InfraredRayController irc = new InfraredRayController();
            irc.Name = e.Label;
            m_controller.Add(e.Label, irc);
            this.tvControllers.SelectedNode = e.Node;
            e.Node.EndEdit(false);
            this.tvControllers.LabelEdit = false;
        }

        private void tvControllers_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (!m_controller.ContainsKey(e.Node.Text))
            {
                return;
            }

            this.Cursor = Cursors.WaitCursor;
            this.dataGridView1.SuspendLayout();
            this.dataGridView1.Rows.Clear();

            foreach (KeyCommandPair kcp in m_controller[e.Node.Text].KeyCommands)
            {
                this.dataGridView1.Rows.Add(false, kcp.Key, kcp.Command);
            }

            this.dataGridView1.ResumeLayout();
            this.Cursor = Cursors.Default;            
        }

        private void toolSave_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                SaveControllers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void toolDeleteCodes_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            try
            {
                this.dataGridView1.SuspendLayout();

                if (this.tvControllers.SelectedNode == null)
                {
                    return;
                }

                for (int i = this.dataGridView1.Rows.Count - 1; i >= 0; i--)
                {
                    if (Convert.ToBoolean(this.dataGridView1.Rows[i].Cells[SELECE_INDEX].Value))
                    {
                        KeyCommandPair kcp=new KeyCommandPair();
                        kcp.Key=Convert.ToString(this.dataGridView1.Rows[i].Cells[CODE_INDEX].Value);
                        m_controller[this.tvControllers.SelectedNode.Text].KeyCommands.Remove(kcp);
                        this.dataGridView1.Rows.RemoveAt(i);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                this.dataGridView1.ResumeLayout();
                this.Cursor = Cursors.Default;
            }
        }

        private void dataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if ((e.ColumnIndex == CMD_INDEX) && (this.tvControllers.SelectedNode!=null))
            {
                KeyCommandPair kcp = m_controller[this.tvControllers.SelectedNode.Text].KeyCommands.Single(r =>
                    string.Compare(r.Key, Convert.ToString(this.dataGridView1.Rows[e.RowIndex].Cells[CODE_INDEX].Value), true) == 0);

                kcp.Command = Convert.ToString(this.dataGridView1.Rows[e.RowIndex].Cells[e.ColumnIndex].Value) ?? string.Empty;
            }
        }
    }
}

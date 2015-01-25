using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TcpProxyServer
{
    public enum AppState
    {
        Starting, Started, Stoping, Stoped, Error
    }

    public partial class frmProxyServer : Form
    {
        private AppState appState = AppState.Stoped;

        private delegate void ShowMsg(string msg);
        private ShowMsg AddMsgDelegate;

        private ProxyServer proxyServer = null;


        public frmProxyServer()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                proxyServer  = new ProxyServer();

                // 加个委托显示msg,因为on系列都是在工作线程中调用的,ui不允许直接操作
                AddMsgDelegate = new ShowMsg(AddMsg);

                proxyServer.AddMsgDelegate = new ProxyServer.ShowMsg(AddMsg);

                SetAppState(AppState.Stoped);
            }
            catch (Exception ex)
            {
                SetAppState(AppState.Error);
                AddMsg(ex.Message);
            }
        }

        /// <summary>
        /// 往listbox加一条项目
        /// </summary>
        /// <param name="msg"></param>
        void AddMsg(string msg)
        {
            if (this.lbxMsg.InvokeRequired)
            {
                // 很帅的调自己
                this.lbxMsg.Invoke(AddMsgDelegate, msg);
            }
            else
            {
                if (this.lbxMsg.Items.Count > 100)
                {
                    this.lbxMsg.Items.RemoveAt(0);
                }
                this.lbxMsg.Items.Add(msg);
                this.lbxMsg.TopIndex = this.lbxMsg.Items.Count - (int)(this.lbxMsg.Height / this.lbxMsg.ItemHeight);
            }
        }


        /// <summary>
        /// 设置程序状态
        /// </summary>
        /// <param name="state"></param>
        void SetAppState(AppState state)
        {
            appState = state;
            this.btnStart.Enabled = (appState == AppState.Stoped);
            this.btnStop.Enabled = (appState == AppState.Started);
            this.txtBindAddr.Enabled = (appState == AppState.Stoped);
            this.txtBindPort.Enabled = (appState == AppState.Stoped);
            this.txtTargetAddr.Enabled = (appState == AppState.Stoped);
            this.txtTargetPort.Enabled = (appState == AppState.Stoped);
            this.txtDisConn.Enabled = (appState == AppState.Started);
            this.btnDisconn.Enabled = (appState == AppState.Started && this.txtDisConn.Text.Length > 0);
        }

        private void txtDisConn_TextChanged(object sender, EventArgs e)
        {
            // CONNID框被改变事件
            this.btnDisconn.Enabled = (appState == AppState.Started && this.txtDisConn.Text.Length > 0);
        }

        private void lbxMsg_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 清理listbox
            if (e.KeyChar == 'c' || e.KeyChar == 'C')
            {
                this.lbxMsg.Items.Clear();
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                proxyServer.BindAddr = this.txtBindAddr.Text.Trim();
                proxyServer.BindPort = ushort.Parse(this.txtBindPort.Text.Trim());
                proxyServer.TargetAddr = this.txtTargetAddr.Text.Trim();
                proxyServer.TargetPort = ushort.Parse(this.txtTargetPort.Text.Trim());

                // 写在这个位置是上面可能会异常
                SetAppState(AppState.Starting);

                // 启动服务
                if (proxyServer.Start())
                {
                    SetAppState(AppState.Started);
                    AddMsg(string.Format("$Server Start OK -> ({0}:{1}->{2}:{3})", 
                                            proxyServer.BindAddr, proxyServer.BindPort,
                                            proxyServer.TargetAddr, proxyServer.TargetPort));
                }
                else
                {
                    SetAppState(AppState.Stoped);
                }
            }
            catch (Exception ex)
            {
                AddMsg(ex.Message);
            }
        }

        private void btnDisconn_Click(object sender, EventArgs e)
        {
            try
            {
                IntPtr connId = (IntPtr)Convert.ToUInt32(this.txtDisConn.Text.Trim());

                // 断开指定客户
                if (proxyServer.Disconnect(connId))
                {
                    AddMsg(string.Format("$({0}) Disconnect OK", connId));
                }
                else
                {
                    throw new Exception(string.Format("Disconnect({0}) Error", connId));
                }
            }
            catch (Exception ex)
            {
                AddMsg(ex.Message);
            }
        }
        private void btnStop_Click(object sender, EventArgs e)
        {
            SetAppState(AppState.Stoping);

            // 停止服务
            AddMsg("$Server Stop");
            if (proxyServer.Stop())
            {
                SetAppState(AppState.Stoped);
            }
            else
            {
                AddMsg("$Stop Error");
            }
        }

        private void frmProxyServer_FormClosing(object sender, FormClosingEventArgs e)
        {
            proxyServer.Stop();
        }

    }

}

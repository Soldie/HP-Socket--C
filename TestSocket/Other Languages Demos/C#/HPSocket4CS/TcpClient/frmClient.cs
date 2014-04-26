using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HPSocketCS;
using System.Runtime.InteropServices;

namespace TcpClient
{
    public enum AppState
    {
        Starting, Started, Stoping, Stoped, Error
    }

    public partial class frmClient : Form
    {
        private AppState appState = AppState.Stoped;

        private delegate void ConnectUpdateUiDelegate();
        private delegate void SetAppStateDelegate(AppState state);
        private delegate void ShowMsg(string msg);
        private ShowMsg AddMsgDelegate;
        HPSocketCS.TcpClient client = new HPSocketCS.TcpClient();
        
        public frmClient()
        {
            InitializeComponent();
        }

        private void frmClient_Load(object sender, EventArgs e)
        {
            try
            {
                // 加个委托显示msg,因为on系列都是在工作线程中调用的,ui不允许直接操作
                AddMsgDelegate = new ShowMsg(AddMsg);


                // 设置 Socket 监听器回调函数
                client.SetCallback(OnPrepareConnect, OnConnect, OnSend, OnReceive, OnClose, OnError);


                SetAppState(AppState.Stoped);
            }
            catch (Exception ex)
            {
                SetAppState(AppState.Error);
                AddMsg(ex.Message);
            }
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            try
            {
                String ip = this.txtIpAddress.Text.Trim();
                ushort port = ushort.Parse(this.txtPort.Text.Trim());

                // 写在这个位置是上面可能会异常
                SetAppState(AppState.Starting);

                AddMsg(string.Format("$Client Starting ... -> ({0}:{1})", ip, port));

                if (client.Start(ip, port, this.cbxAsyncConn.Checked))
                {
                    if (cbxAsyncConn.Checked == false)
                    {
                        SetAppState(AppState.Started);
                    }

                    AddMsg(string.Format("$Client Start OK -> ({0}:{1})", ip, port));
                }
                else
                {
                    SetAppState(AppState.Stoped);
                    throw new Exception(string.Format("$Client Start Error -> {0}({1})", client.GetLastErrorDesc(), client.GetlastError()));
                }
            }
            catch (Exception ex)
            {
                AddMsg(ex.Message);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {

            // 停止服务
            AddMsg("$Server Stop");
            if (client.Stop())
            {
                SetAppState(AppState.Stoped);
            }
            else
            {
                AddMsg(string.Format("$Stop Error -> {0}({1})", client.GetLastErrorDesc(), client.GetlastError()));
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                string send = this.txtSend.Text;
                if (send.Length == 0)
                {
                    return;
                }

                byte[] bytes = Encoding.Default.GetBytes(send);
                uint dwConnId = client.GetConnectionId();

                // 发送
                if (client.Send(bytes, bytes.Length))
                {
                    AddMsg(string.Format("$ ({0}) Send OK --> {1}", dwConnId, send));
                }
                else
                {
                    AddMsg(string.Format("$ ({0}) Send Fail --> {1} ({2})", dwConnId, send, bytes.Length));
                }

            }
            catch (Exception)
            {

            }

        }

        private void lbxMsg_KeyPress(object sender, KeyPressEventArgs e)
        {

            // 清理listbox
            if (e.KeyChar == 'c' || e.KeyChar == 'C')
            {
                this.lbxMsg.Items.Clear();
            }
        }

        void ConnectUpdateUi()
        {
            if (this.cbxAsyncConn.Checked == true)
            {
                SetAppState(AppState.Started);
            }
        }

        HandleResult OnPrepareConnect(uint dwConnID, uint socket)
        {
            return HandleResult.Ok;
        }

        HandleResult OnConnect(uint dwConnID)
        {
            // 已连接 到达一次
            // 如果是异步联接,更新界面状态

            this.Invoke(new ConnectUpdateUiDelegate(ConnectUpdateUi));

            AddMsg(string.Format(" > [{0},OnConnect]", dwConnID));

            return HandleResult.Ok;
        }

        HandleResult OnSend(uint dwConnID, IntPtr pData, int iLength)
        {
            // 客户端发数据了
            AddMsg(string.Format(" > [{0},OnSend] -> ({1} bytes)", dwConnID, iLength));

            return HandleResult.Ok;
        }

        HandleResult OnReceive(uint dwConnID, IntPtr pData, int iLength)
        {
            // 数据到达了

            AddMsg(string.Format(" > [{0},OnReceive] -> ({1} bytes)", dwConnID, iLength));

            return HandleResult.Ok;
        }

        HandleResult OnClose(uint dwConnID)
        {
            // 连接关闭了

            AddMsg(string.Format(" > [{0},OnClose]", dwConnID));

            // 通知界面
            this.Invoke(new SetAppStateDelegate(SetAppState), AppState.Stoped);
            return HandleResult.Ok;
        }

        HandleResult OnError(uint dwConnID, SocketOperation enOperation, int iErrorCode)
        {
            // 出错了

            AddMsg(string.Format(" > [{0},OnError] -> OP:{1},CODE:{2}", dwConnID, enOperation, iErrorCode));
 
            // 通知界面,只处理了连接错误,也没进行是不是连接错误的判断,所以有错误就会设置界面
            // 生产环境请自己控制
            this.Invoke(new SetAppStateDelegate(SetAppState), AppState.Stoped);

            return HandleResult.Ok;
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
            this.txtIpAddress.Enabled = (appState == AppState.Stoped);
            this.txtPort.Enabled = (appState == AppState.Stoped);
            this.cbxAsyncConn.Enabled = (appState == AppState.Stoped);
            this.btnSend.Enabled = (appState == AppState.Started);
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

        private void frmClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            client.Destroy();
        }

    }
}

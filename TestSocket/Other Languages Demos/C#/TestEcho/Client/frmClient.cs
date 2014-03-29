using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HPSocket;
using System.Runtime.InteropServices;

namespace TcpClient
{
    public enum EnAppState
    {
        ST_STARTING, ST_STARTED, ST_STOPING, ST_STOPED
    }

    public partial class frmClient : Form
    {
        private EnAppState enAppState = EnAppState.ST_STOPED;
        private IntPtr pClient = IntPtr.Zero;
        private IntPtr pListener = IntPtr.Zero;

        private delegate void ConnectUpdateUiDelegate();
        private delegate void SetAppStateDelegate(EnAppState state);
        private delegate void ShowMsg(string msg);
        private ShowMsg AddMsgDelegate;


        private static HPSocket.HPSocketSdk.HP_FN_OnSend OnSendCallback;
        private static HPSocket.HPSocketSdk.HP_FN_OnConnect OnConnectCallback;
        private static HPSocket.HPSocketSdk.HP_FN_OnReceive OnReceiveCallback;
        private static HPSocket.HPSocketSdk.HP_FN_OnClose OnCloseOnClose;
        private static HPSocket.HPSocketSdk.HP_FN_OnError OnErrorCallback;


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


                // 创建监听器对象
                pListener = HPSocketSdk.Create_HP_TcpClientListener();
                // 创建 Socket 对象
                pClient = HPSocketSdk.Create_HP_TcpClient(pListener);

                // 设置 Socket 监听器回调函数
                OnSendCallback = new HPSocketSdk.HP_FN_OnSend(OnSend);
                OnConnectCallback = new HPSocket.HPSocketSdk.HP_FN_OnConnect(OnConnect);
                OnReceiveCallback = new HPSocketSdk.HP_FN_OnReceive(OnReceive);
                OnCloseOnClose = new HPSocketSdk.HP_FN_OnClose(OnClose);
                OnErrorCallback = new HPSocketSdk.HP_FN_OnError(OnError);

                // 设置 Socket 监听器回调函数
                HPSocketSdk.HP_Set_FN_Client_OnConnect(pListener, OnConnectCallback);
                HPSocketSdk.HP_Set_FN_Client_OnSend(pListener, OnSendCallback);
                HPSocketSdk.HP_Set_FN_Client_OnReceive(pListener, OnReceiveCallback);
                HPSocketSdk.HP_Set_FN_Client_OnClose(pListener, OnCloseOnClose);
                HPSocketSdk.HP_Set_FN_Client_OnError(pListener, OnErrorCallback);

                SetAppState(EnAppState.ST_STOPED);
            }
            catch (Exception ex)
            {
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
                SetAppState(EnAppState.ST_STARTING);

                AddMsg(string.Format("$Client Starting ... -> ({0}:{1})", ip, port));

                if (HPSocketSdk.HP_Client_Start(pClient, ip, port, this.cbxAsyncConn.Checked))
                {
                    if (cbxAsyncConn.Checked == false)
                    {
                        SetAppState(EnAppState.ST_STARTED);
                    }
                }
                else
                {
                    SetAppState(EnAppState.ST_STOPED);
                    throw new Exception(string.Format("$Client Start Error -> {0}({1})", HP_Client_GetLastErrorDesc(pClient), HPSocketSdk.HP_Client_GetLastError(pClient)));
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
            if (HPSocketSdk.HP_Client_Stop(pClient))
            {
                SetAppState(EnAppState.ST_STOPED);
            }
            else
            {
                AddMsg(string.Format("$Stop Error -> {0}({1})", HP_Client_GetLastErrorDesc(pClient), HPSocketSdk.HP_Client_GetLastError(pClient)));
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
                uint dwConnId = HPSocketSdk.HP_Client_GetConnectionID(pClient);

                // 发送
                if (HPSocketSdk.HP_Client_Send(pClient, dwConnId, bytes, bytes.Length))
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
                SetAppState(EnAppState.ST_STARTED);
            }
        }

        HPSocketSdk.En_HP_HandleResult OnConnect(uint dwConnID)
        {
            // 已连接 到达一次
            // 如果是异步联接,更新界面状态

            this.Invoke(new ConnectUpdateUiDelegate(ConnectUpdateUi));

            AddMsg(string.Format(" > [{0},OnConnect]", dwConnID));

            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }

        HPSocketSdk.En_HP_HandleResult OnSend(uint dwConnID, IntPtr pData, int iLength)
        {
            // 客户端发数据了
            AddMsg(string.Format(" > [{0},OnSend] -> ({1} bytes)", dwConnID, iLength));

            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }

        HPSocketSdk.En_HP_HandleResult OnReceive(uint dwConnID, IntPtr pData, int iLength)
        {
            // 数据到达了

            AddMsg(string.Format(" > [{0},OnReceive] -> ({1} bytes)", dwConnID, iLength));

            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }

        HPSocketSdk.En_HP_HandleResult OnClose(uint dwConnID)
        {
            // 连接关闭了

            AddMsg(string.Format(" > [{0},OnClose]", dwConnID));

            // 通知界面
            this.Invoke(new SetAppStateDelegate(SetAppState), EnAppState.ST_STOPED);
            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }

        HPSocketSdk.En_HP_HandleResult OnError(uint dwConnID, HPSocketSdk.En_HP_SocketOperation enOperation, int iErrorCode)
        {
            // 出错了

            AddMsg(string.Format(" > [{0},OnError] -> OP:{1},CODE:{2}", dwConnID, enOperation, iErrorCode));

            // 通知界面,只处理了连接错误,也没进行是不是连接错误的判断,所以有错误就会设置界面
            // 生产环境请自己控制
            this.Invoke(new SetAppStateDelegate(SetAppState), EnAppState.ST_STOPED);

            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }



        /// <summary>
        /// 封装HPSocketSdk.HP_Client_GetLastErrorDesc()使得非托管内存到托管字符串的转换
        /// </summary>
        /// <param name="spServer"></param>
        /// <returns></returns>
        string HP_Client_GetLastErrorDesc(IntPtr spClient)
        {
            IntPtr ptr = HPSocketSdk.HP_Client_GetLastErrorDesc(spClient);
            String desc = Marshal.PtrToStringUni(ptr);
            // Marshal.FreeHGlobal(ptr);
            return desc;
        }

        /// <summary>
        /// 设置程序状态
        /// </summary>
        /// <param name="state"></param>
        void SetAppState(EnAppState state)
        {
            enAppState = state;
            this.btnStart.Enabled = (enAppState == EnAppState.ST_STOPED);
            this.btnStop.Enabled = (enAppState == EnAppState.ST_STARTED);
            this.txtIpAddress.Enabled = (enAppState == EnAppState.ST_STOPED);
            this.txtPort.Enabled = (enAppState == EnAppState.ST_STOPED);
            this.cbxAsyncConn.Enabled = (enAppState == EnAppState.ST_STOPED);
            this.btnSend.Enabled = (enAppState == EnAppState.ST_STARTED);
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
            }
        }

        private void frmClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            HPSocketSdk.Destroy_HP_TcpClient(pClient);
            HPSocketSdk.Destroy_HP_TcpClientListener(pListener);
        }

    }
}

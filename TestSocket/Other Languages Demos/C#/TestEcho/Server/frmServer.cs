using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using HPSocket;

namespace TcpServer
{
    public enum EnAppState
    {
        ST_STARTING, ST_STARTED, ST_STOPING, ST_STOPED
    }

    public partial class frmServer : Form
    {
        private EnAppState enAppState = EnAppState.ST_STOPED;
        private IntPtr pServer = IntPtr.Zero;
        private IntPtr pListener = IntPtr.Zero;

        private delegate void ShowMsg(string msg);
        private ShowMsg AddMsgDelegate;


        private static HPSocket.HPSocketSdk.HP_FN_OnAccept OnOnAcceptCallback;
        private static HPSocket.HPSocketSdk.HP_FN_OnSend OnSendCallback;
        private static HPSocket.HPSocketSdk.HP_FN_OnPrepareListen OnPrepareListenCallback;
        private static HPSocket.HPSocketSdk.HP_FN_OnReceive OnReceiveCallback;
        private static HPSocket.HPSocketSdk.HP_FN_OnClose OnCloseCallback;
        private static HPSocket.HPSocketSdk.HP_FN_OnError OnErrorCallback;
        private static HPSocket.HPSocketSdk.HP_FN_OnServerShutdown OnServerShutdownCallback;


        private string title = "Echo Server [ 'C' - clear list box ]";
        public frmServer()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                this.Text = title;
                // 本机测试没必要改地址,有需求请注释或删除
                this.txtIpAddress.ReadOnly = true;

                // 加个委托显示msg,因为on系列都是在工作线程中调用的,ui不允许直接操作
                AddMsgDelegate = new ShowMsg(AddMsg);

                // 创建监听器对象
                pListener = HPSocketSdk.Create_HP_TcpServerListener();

                // 创建 Socket 对象
                pServer = HPSocketSdk.Create_HP_TcpServer(pListener);

                // 设置 Socket 监听器回调函数
                OnOnAcceptCallback = new HPSocketSdk.HP_FN_OnAccept(OnAccept);
                OnSendCallback = new HPSocketSdk.HP_FN_OnSend(OnSend);
                OnPrepareListenCallback = new HPSocket.HPSocketSdk.HP_FN_OnPrepareListen(OnPrepareListen);
                OnReceiveCallback = new HPSocketSdk.HP_FN_OnReceive(OnReceive);
                OnCloseCallback = new HPSocketSdk.HP_FN_OnClose(OnClose);
                OnErrorCallback = new HPSocketSdk.HP_FN_OnError(OnError);
                OnServerShutdownCallback = new HPSocketSdk.HP_FN_OnServerShutdown(OnServerShutdown);

                // 设置 Socket 监听器回调函数
                HPSocketSdk.HP_Set_FN_Server_OnPrepareListen(pListener,OnPrepareListenCallback);
                HPSocketSdk.HP_Set_FN_Server_OnAccept(pListener, OnOnAcceptCallback);
                HPSocketSdk.HP_Set_FN_Server_OnSend(pListener,OnSendCallback );
                HPSocketSdk.HP_Set_FN_Server_OnReceive(pListener,OnReceiveCallback);
                HPSocketSdk.HP_Set_FN_Server_OnClose(pListener, OnCloseCallback);
                HPSocketSdk.HP_Set_FN_Server_OnError(pListener,OnErrorCallback);
                HPSocketSdk.HP_Set_FN_Server_OnServerShutdown(pListener, OnServerShutdownCallback);

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

                // 启动服务
                if (HPSocketSdk.HP_Server_Start(pServer, ip, port))
                {
                    this.Text = string.Format("{2} - ({0}:{1})", ip, port, title);
                    SetAppState(EnAppState.ST_STARTED);
                    throw new Exception(string.Format("$Server Start OK -> ({0}:{1})", ip, port));
                }
                else
                {
                    SetAppState(EnAppState.ST_STOPED);
                    throw new Exception(string.Format("$Server Start Error -> {0}({1})", HP_Server_GetLastErrorDesc(pServer), HPSocketSdk.HP_Server_GetLastError(pServer)));
                }
            }
            catch (Exception ex)
            {
                AddMsg(ex.Message);
            }
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            SetAppState(EnAppState.ST_STOPING);

            // 停止服务
            AddMsg("$Server Stop");
            if (HPSocketSdk.HP_Server_Stop(pServer))
            {
                this.Text = title;
                SetAppState(EnAppState.ST_STOPED);
            }
            else
            {
                AddMsg(string.Format("$Stop Error -> {0}({1})", HP_Server_GetLastErrorDesc(pServer), HPSocketSdk.HP_Server_GetLastError(pServer)));
            }
        }

        private void btnDisconn_Click(object sender, EventArgs e)
        {
            try
            {
                uint dwConnId = Convert.ToUInt32(this.txtDisConn.Text.Trim());

                // 断开指定客户
                if (HPSocketSdk.HP_Server_Disconnect(pServer, dwConnId, true))
                {
                    AddMsg(string.Format("$({0}) Disconnect OK", dwConnId));
                }
                else
                {
                    throw new Exception(string.Format("Disconnect({0}) Error", dwConnId));
                }
            }
            catch (Exception ex)
            {
                AddMsg(ex.Message);
            }
        }


        HPSocketSdk.En_HP_HandleResult OnPrepareListen(IntPtr soListen)
        {
            // 监听事件到达了,一般没什么用吧?

            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }

        HPSocketSdk.En_HP_HandleResult OnAccept(uint dwConnID, IntPtr pClient)
        {
            // 客户进入了


            // 获取客户端ip和端口
            StringBuilder ip = new StringBuilder();
            int ipLength = 40; // 一定要给大小,否则会失败
            ushort port = 0;
            if (HPSocketSdk.HP_Server_GetClientAddress(pServer, dwConnID, ip, ref ipLength, ref port) && ipLength > 0)
            {
                AddMsg(string.Format(" > [{0},OnAccept] -> PASS({1}:{2})", dwConnID, ip.ToString(), port));
            }
            else
            {
                AddMsg(string.Format(" > [{0},OnAccept] -> HP_Server_GetClientAddress() Error", dwConnID));
            }
            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }

        HPSocketSdk.En_HP_HandleResult OnSend(uint dwConnID, IntPtr pData, int iLength)
        {
            // 服务器发数据了


            AddMsg(string.Format(" > [{0},OnSend] -> ({1} bytes)", dwConnID, iLength));

            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }

        HPSocketSdk.En_HP_HandleResult OnReceive(uint dwConnID, IntPtr pData, int iLength)
        {
            // 数据到达了
            try
            {
                AddMsg(string.Format(" > [{0},OnReceive] -> ({1} bytes)", dwConnID, iLength));
;

                HPSocketSdk.En_HP_HandleResult result = HPSocketSdk.En_HP_HandleResult.HP_HR_ERROR;
                if (HPSocketSdk.HP_Server_Send(pServer, dwConnID, pData, iLength))
                {
                    result = HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
                }
                return result;
            }
            catch (Exception)
            {

                return HPSocketSdk.En_HP_HandleResult.HP_HR_IGNORE;
            }         
        }

        HPSocketSdk.En_HP_HandleResult OnClose(uint dwConnID)
        {
            // 客户端开了


            AddMsg(string.Format(" > [{0},OnClose]", dwConnID));
            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }

        HPSocketSdk.En_HP_HandleResult OnError(uint dwConnID, HPSocketSdk.En_HP_SocketOperation enOperation, int iErrorCode)
        {
            // 客户出错了

            AddMsg(string.Format(" > [{0},OnError] -> OP:{1},CODE:{2}", dwConnID, enOperation, iErrorCode));
            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
        }

        HPSocketSdk.En_HP_HandleResult OnServerShutdown()
        {
            // 服务关闭了


            AddMsg(" > [OnServerShutdown]");
            return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;
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
            this.txtDisConn.Enabled = (enAppState == EnAppState.ST_STARTED);
            this.btnDisconn.Enabled = (enAppState == EnAppState.ST_STARTED && this.txtDisConn.Text.Length > 0);
        }

        /// <summary>
        /// 封装HPSocketSdk.HP_Server_GetLastErrorDesc()使得非托管内存到托管字符串的转换
        /// </summary>
        /// <param name="spServer"></param>
        /// <returns></returns>
        string HP_Server_GetLastErrorDesc(IntPtr spServer)
        {
            IntPtr ptr = HPSocketSdk.HP_Server_GetLastErrorDesc(spServer);
            String desc = Marshal.PtrToStringUni(ptr);
            // Marshal.FreeHGlobal(ptr);
            return desc;
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

        private void txtDisConn_TextChanged(object sender, EventArgs e)
        {
            // CONNID框被改变事件
            this.btnDisconn.Enabled = (enAppState == EnAppState.ST_STARTED && this.txtDisConn.Text.Length > 0);
        }

        private void lbxMsg_KeyPress(object sender, KeyPressEventArgs e)
        {
            // 清理listbox
            if (e.KeyChar == 'c' || e.KeyChar == 'C')
            {
                this.lbxMsg.Items.Clear();
            }
        }

        private void frmServer_FormClosed(object sender, FormClosedEventArgs e)
        {
            HPSocketSdk.Destroy_HP_TcpServer(pServer);
            HPSocketSdk.Destroy_HP_TcpServerListener(pListener);
        }
    }
}

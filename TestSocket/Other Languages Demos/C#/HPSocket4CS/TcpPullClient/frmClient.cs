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

namespace TcpPullClient
{
    public enum EnAppState
    {
        ST_STARTING, ST_STARTED, ST_STOPING, ST_STOPED, ST_ERROR
    }

    public partial class frmClient : Form
    {
        private EnAppState enAppState = EnAppState.ST_STOPED;

        private delegate void ConnectUpdateUiDelegate();
        private delegate void SetAppStateDelegate(EnAppState state);
        private delegate void ShowMsg(string msg);
        private ShowMsg AddMsgDelegate;
        HPSocketCS.TcpPullClient client = new HPSocketCS.TcpPullClient();

        int id = 0;

        // 包头大小
        int pkgHeaderSize = Marshal.SizeOf(new PkgHeader());
        PkgInfo pkgInfo = new PkgInfo();

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

                pkgInfo.IsHeader = true;
                pkgInfo.Length = pkgHeaderSize;
                
                // 设置 Socket 监听器回调函数
                client.SetCallback(OnPrepareConnect, OnConnect, OnSend, OnReceive, OnClose, OnError);


                SetAppState(EnAppState.ST_STOPED);
            }
            catch (Exception ex)
            {
                SetAppState(EnAppState.ST_ERROR);
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

                if (client.Start(ip, port, this.cbxAsyncConn.Checked))
                {
                    if (cbxAsyncConn.Checked == false)
                    {
                        SetAppState(EnAppState.ST_STARTED);
                    }
                }
                else
                {
                    SetAppState(EnAppState.ST_STOPED);
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
                SetAppState(EnAppState.ST_STOPED);
            }
            else
            {
                AddMsg(string.Format("$Stop Error -> {0}({1})", client.GetLastErrorDesc(), client.GetlastError()));
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            IntPtr bufferPtr = IntPtr.Zero;
            try
            {
                string send = this.txtSend.Text;
                if (send.Length == 0)
                {
                    return;
                }

                // 封包头
                PkgHeader header = new PkgHeader();
                header.Id = ++id;
                header.BodySize = send.Length;
                byte[] headerBytes = StructToBytes(header);

                // 封包体
                byte[] bodyBytes = Encoding.Default.GetBytes(send);

                // 组合最终发送的封包 (封包头+封包体)
                byte[] sendBytes = GetSendBuffer(headerBytes, bodyBytes);

                // 发送
                uint dwConnId = client.GetConnectionId();
                if (client.Send(dwConnId, sendBytes, sendBytes.Length))
                {
                    AddMsg(string.Format("$ ({0}) Send OK --> {1}", dwConnId, send));
                }
                else
                {
                    AddMsg(string.Format("$ ({0}) Send Fail --> {1} ({2})", dwConnId, send, sendBytes.Length));
                }

            }
            catch (Exception)
            {

            }
            finally
            {
                if (bufferPtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(bufferPtr);
                }
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

        En_HP_HandleResult OnPrepareConnect(uint dwConnID, uint socket)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        En_HP_HandleResult OnConnect(uint dwConnID)
        {
            // 已连接 到达一次
            // 如果是异步联接,更新界面状态

            this.Invoke(new ConnectUpdateUiDelegate(ConnectUpdateUi));

            AddMsg(string.Format(" > [{0},OnConnect]", dwConnID));

            return En_HP_HandleResult.HP_HR_OK;
        }

        En_HP_HandleResult OnSend(uint dwConnID, IntPtr pData, int iLength)
        {
            // 客户端发数据了
            AddMsg(string.Format(" > [{0},OnSend] -> ({1} bytes)", dwConnID, iLength));

            return En_HP_HandleResult.HP_HR_OK;
        }

        En_HP_HandleResult OnReceive(uint dwConnID, int iLength)
        {
            // 数据到达了

            // 需要长度
            int required = pkgInfo.Length;

            // 剩余大小
            int remain = iLength;


            while (remain >= required)
            {
                IntPtr bufferPtr = IntPtr.Zero;
                try
                {
                    remain -= required;
                    bufferPtr = Marshal.AllocHGlobal(required); ;
                    if (client.Fetch(dwConnID, bufferPtr, required) == En_HP_FetchResult.HP_FR_OK)
                    {
                        if (pkgInfo.IsHeader == true)
                        {
                            PkgHeader header = (PkgHeader)Marshal.PtrToStructure(bufferPtr, typeof(PkgHeader));

                            // 调试信息
                            Console.WriteLine("[Client] head -> Id: {0}, BodySize: {1}\r\n", header.Id, header.BodySize);

                            required = header.BodySize;
                        }
                        else
                        {
                            // 调试信息
                            string recvString = Marshal.PtrToStringAnsi(bufferPtr, required);
                            Console.WriteLine("[Client] body -> text: {0}\r\n", recvString);

                            required = pkgHeaderSize;
                        }

                        AddMsg(string.Format(" > [{0},OnReceive] -> ({1} bytes)", dwConnID, pkgInfo.Length));

                        pkgInfo.IsHeader = !pkgInfo.IsHeader;
                        pkgInfo.Length = required;
                    }
                }
                catch
                {
                    return En_HP_HandleResult.HP_HR_ERROR;
                }
                finally
                {
                    if (bufferPtr != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(bufferPtr);
                        bufferPtr = IntPtr.Zero;
                    }
                }
            }

            return En_HP_HandleResult.HP_HR_OK;
        }

        En_HP_HandleResult OnClose(uint dwConnID)
        {
            // 连接关闭了

            AddMsg(string.Format(" > [{0},OnClose]", dwConnID));

            // 通知界面
            this.Invoke(new SetAppStateDelegate(SetAppState), EnAppState.ST_STOPED);
            return En_HP_HandleResult.HP_HR_OK;
        }

        En_HP_HandleResult OnError(uint dwConnID, En_HP_SocketOperation enOperation, int iErrorCode)
        {
            // 出错了

            AddMsg(string.Format(" > [{0},OnError] -> OP:{1},CODE:{2}", dwConnID, enOperation, iErrorCode));

            // 通知界面,只处理了连接错误,也没进行是不是连接错误的判断,所以有错误就会设置界面
            // 生产环境请自己控制
            this.Invoke(new SetAppStateDelegate(SetAppState), EnAppState.ST_STOPED);

            return En_HP_HandleResult.HP_HR_OK;
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
                this.lbxMsg.TopIndex = this.lbxMsg.Items.Count - (int)(this.lbxMsg.Height / this.lbxMsg.ItemHeight);
            }
        }

        private void frmClient_FormClosed(object sender, FormClosedEventArgs e)
        {
            client.Destroy();
        }

        /// <summary>
        /// 结构体转指针
        /// </summary>
        /// <param name="ojb"></param>
        /// <returns></returns>
        private byte[] StructToBytes(object ojb)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int objSize = Marshal.SizeOf(ojb);

                ptr = Marshal.AllocHGlobal(objSize);
                Marshal.StructureToPtr(ojb, ptr, false);

                byte[] bytes = new byte[objSize];
                Marshal.Copy(ptr, bytes, 0, objSize);

                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
            
        }

        private byte[] GetSendBuffer(byte[] headerBytes, byte[] bodyBytes)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int bufferSize = headerBytes.Length + bodyBytes.Length;
                ptr = Marshal.AllocHGlobal(bufferSize);

                // 拷贝包头到缓冲区首部
                Marshal.Copy(headerBytes, 0, ptr, headerBytes.Length);

                // 拷贝包体到缓冲区剩余部分
                Marshal.Copy(bodyBytes, 0, ptr + headerBytes.Length, bodyBytes.Length);

                byte[] bytes = new byte[bufferSize];
                Marshal.Copy(ptr, bytes, 0, bufferSize);

                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }

        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PkgHeader
    {
        public int Id;
        public int BodySize;
    }

    [StructLayout(LayoutKind.Sequential)]
    public class PkgInfo
    {
        public bool IsHeader;
        public int Length;
    }
}

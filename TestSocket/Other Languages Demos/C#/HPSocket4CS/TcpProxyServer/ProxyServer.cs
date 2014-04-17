using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HPSocketCS;
using System.Runtime.InteropServices;

namespace TcpProxyServer
{
    public class ProxyServer
    {
        /// <summary>
        /// 绑定地址
        /// </summary>
        public string BindAddr { get; set; }

        /// <summary>
        /// 绑定端口
        /// </summary>
        public ushort BindPort { get; set; }

        /// <summary>
        /// 目标地址
        /// </summary>
        public string TargetAddr { get; set; }

        /// <summary>
        /// 目标端口
        /// </summary>
        public ushort TargetPort { get; set; }

        // 为了简单直接定义了一个支持log输出的委托
        public delegate void ShowMsg(string msg);
        /// <summary>
        /// 日志输出
        /// </summary>
        public ShowMsg AddMsgDelegate;

        protected TcpServer server = new TcpServer();
        protected TcpAgent agent = new TcpAgent();


        public ProxyServer()
        {
            server.SetCallback(OnServerPrepareListen, OnServerAccept, OnServerSend, OnServerReceive, OnServerClose,
                               OnServerError, OnServerShutdown);

            agent.SetCallback(OnAgentPrepareConnect, OnAgentConnect, OnAgentSend, OnAgentReceive, OnAgentClose,
                              OnAgentError, OnAgentShutdown);
        }

        public bool Start()
        {
            if (string.IsNullOrEmpty(BindAddr) || string.IsNullOrEmpty(TargetAddr) ||
                BindPort == 0 || TargetPort == 0 || AddMsgDelegate == null)
            {
                throw new Exception("请先设置属性[BindAddr,TargetAddr,BindPort,TargetPort,AddMsgDelegate]");
            }

            bool isStart = server.Start(BindAddr, BindPort);
            if (isStart == false)
            {
                AddMsg(string.Format(" > Server start fail -> {0}({1})", server.GetLastErrorDesc(), server.GetlastError()));
                return isStart;
            }

            isStart = agent.Start(BindAddr, false);
            if (isStart == false)
            {
                AddMsg(string.Format(" > Server start fail -> {0}({1})", agent.GetLastErrorDesc(), agent.GetlastError()));
                return isStart;
            }

            return isStart;
        }

        public bool Stop()
        {
            return server.Stop() && agent.Stop();
        }

        private void AddMsg(string msg)
        {
            AddMsgDelegate(msg);
        }


        public bool Disconnect(uint connId, bool force = true)
        {
            return server.Disconnect(connId, force);
        }

        //////////////////////////////Agent//////////////////////////////////////////////////

        /// <summary>
        /// 准备连接了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnAgentPrepareConnect(uint dwConnID, uint socket)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 已连接
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnAgentConnect(uint dwConnID)
        {
            AddMsg(string.Format(" > [{0},OnAgentConnect]", dwConnID));
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 客户端发数据了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnAgentSend(uint dwConnID, IntPtr pData, int iLength)
        {
            AddMsg(string.Format(" > [{0},OnAgentSend] -> ({1} bytes)", dwConnID, iLength));
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 数据到达了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnAgentReceive(uint dwConnID, IntPtr pData, int iLength)
        {
            // 获取附加数据
            IntPtr extraPtr = IntPtr.Zero;
            if (agent.GetConnectionExtra(dwConnID, ref extraPtr) == false)
            {
                return En_HP_HandleResult.HP_HR_ERROR;
            }

            ConnExtraData extra = (ConnExtraData)Marshal.PtrToStructure(extraPtr, typeof(ConnExtraData));
            AddMsg(string.Format(" > [{0},OnAgentReceive] -> ({1} bytes)", dwConnID, iLength));
            if (extra.Server.Send(extra.ConnIdForServer, pData, iLength) == false)
            {
                return En_HP_HandleResult.HP_HR_ERROR;
            }

            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 连接关闭了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnAgentClose(uint dwConnID)
        {
            AddMsg(string.Format(" > [{0},OnAgentClose]", dwConnID));

            // 获取附加数据
            IntPtr extraPtr = IntPtr.Zero;
            if (agent.GetConnectionExtra(dwConnID, ref extraPtr) == false)
            {
                return En_HP_HandleResult.HP_HR_ERROR;
            }

            ConnExtraData extra = (ConnExtraData)Marshal.PtrToStructure(extraPtr, typeof(ConnExtraData));

            agent.SetConnectionExtra(dwConnID, null);

            if (extra.FreeType == 0)
            {

                // 由Target断开连接,释放server连接
                extra.FreeType = 1;
                server.SetConnectionExtra(extra.ConnIdForServer, extra);
                extra.Server.Disconnect(extra.ConnIdForServer);
            }


            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 出错了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="enOperation"></param>
        /// <param name="iErrorCode"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnAgentError(uint dwConnID, En_HP_SocketOperation enOperation, int iErrorCode)
        {
            AddMsg(string.Format(" > [{0},OnAgentError] -> OP:{1},CODE:{2}", dwConnID, enOperation, iErrorCode));
            // return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;

            // 因为要释放附加数据,所以直接返回OnAgentClose()了
            return OnAgentClose(dwConnID);
        }

        /// <summary>
        /// Agent关闭了
        /// </summary>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnAgentShutdown()
        {
            AddMsg(" > [OnAgentShutdown]");
            return En_HP_HandleResult.HP_HR_OK;
        }

        //////////////////////////////Server//////////////////////////////////////////////////

        /// <summary>
        /// 监听事件
        /// </summary>
        /// <param name="soListen"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnServerPrepareListen(IntPtr soListen)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 客户进入
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pClient"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnServerAccept(uint dwConnID, IntPtr pClient)
        {
            // 获取客户端ip和端口
            string ip = string.Empty;
            ushort port = 0;
            if (server.GetRemoteAddress(dwConnID, ref ip, ref port))
            {
                AddMsg(string.Format(" > [{0},OnServerAccept] -> PASS({1}:{2})", dwConnID, ip.ToString(), port));
            }
            else
            {
                AddMsg(string.Format(" > [{0},OnServerAccept] -> HP_Server_GetClientAddress() Error", dwConnID));
            }

            uint clientConnId = 0;

            // 一次不成功的事偶尔可能发生,三次连接都不成功,那就真连不上了
            // 当server有连接进入,使用agent连接到目标服务器
            if (agent.Connect(TargetAddr, TargetPort, ref clientConnId) == false)
            {
                if (agent.Connect(TargetAddr, TargetPort, ref clientConnId) == false)
                {
                    if (agent.Connect(TargetAddr, TargetPort, ref clientConnId) == false)
                    {
                        AddMsg(string.Format(" > [Client->Connect] fail -> ID:{0}", clientConnId));
                        return En_HP_HandleResult.HP_HR_ERROR;
                    }
                }
            }


            // 设置附加数据
            ConnExtraData extra = new ConnExtraData();
            extra.ConnIdForServer = dwConnID;
            extra.ConnIdForClient = clientConnId;
            extra.Server = server;
            extra.FreeType = 0;
            if (server.SetConnectionExtra(dwConnID, extra) == false)
            {
                AddMsg(string.Format(" > [{0},OnServerAccept] -> server.SetConnectionExtra fail", dwConnID));
                return En_HP_HandleResult.HP_HR_ERROR;
            }

            if (agent.SetConnectionExtra(clientConnId, extra) == false)
            {
                server.SetConnectionExtra(dwConnID, null);
                AddMsg(string.Format(" > [{0}-{1},OnServerAccept] -> agent.SetConnectionExtra fail", dwConnID, clientConnId));
                return En_HP_HandleResult.HP_HR_ERROR;
            }

            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 服务器发数据了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnServerSend(uint dwConnID, IntPtr pData, int iLength)
        {
            AddMsg(string.Format(" > [Server->OnServerSend] -> ({0} bytes)", iLength));
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 数据到达了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnServerReceive(uint dwConnID, IntPtr pData, int iLength)
        {

            try
            {
                // 获取附加数据
                IntPtr extraPtr = IntPtr.Zero;

                if (server.GetConnectionExtra(dwConnID, ref extraPtr) == false)
                {
                    return En_HP_HandleResult.HP_HR_ERROR;
                }

                // extra 就是accept里传入的附加数据了
                ConnExtraData extra = (ConnExtraData)Marshal.PtrToStructure(extraPtr, typeof(ConnExtraData));

                AddMsg(string.Format(" > [Server->OnServerReceive] -> ({0} bytes)", iLength));

                // 服务端收到数据了,应该调用agent发送到顶层服务器,实现 client(N)->server->targetServer 的中转
                if (agent.Send(extra.ConnIdForClient, pData, iLength) == false)
                {
                    return En_HP_HandleResult.HP_HR_ERROR;
                }

                return En_HP_HandleResult.HP_HR_OK;

            }
            catch (Exception)
            {
                return En_HP_HandleResult.HP_HR_ERROR;
            }
            
        }

        /// <summary>
        /// 客户离开了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnServerClose(uint dwConnID)
        {
            // 获取附加数据
            IntPtr extraPtr = IntPtr.Zero;
            if (server.GetConnectionExtra(dwConnID, ref extraPtr) == false)
            {
                return En_HP_HandleResult.HP_HR_ERROR;
            }

            // extra 就是accept里传入的附加数据了
            ConnExtraData extra = (ConnExtraData)Marshal.PtrToStructure(extraPtr, typeof(ConnExtraData));
            if (extra.FreeType == 0)
            {
                // 由client(N)断开连接,释放agent数据
                agent.Disconnect(extra.ConnIdForClient);
                agent.SetConnectionExtra(extra.ConnIdForClient, null);
            }

            server.SetConnectionExtra(dwConnID, null);

            AddMsg(string.Format(" > [{0},OnServerClose]", dwConnID));
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 出错了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="enOperation"></param>
        /// <param name="iErrorCode"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnServerError(uint dwConnID, En_HP_SocketOperation enOperation, int iErrorCode)
        {
            AddMsg(string.Format(" > [{0},OnServerError] -> OP:{1},CODE:{2}", dwConnID, enOperation, iErrorCode));
            // return HPSocketSdk.En_HP_HandleResult.HP_HR_OK;

            // 因为要释放附加数据,所以直接返回OnServerClose()了
            return OnServerClose(dwConnID);
        }

        /// <summary>
        /// 服务关闭了
        /// </summary>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnServerShutdown()
        {
            AddMsg(" > [OnServerShutdown]");
            return En_HP_HandleResult.HP_HR_OK;
        }

        ////////////////////////////////////////////////////////////////////////////////
    }


    [StructLayout(LayoutKind.Sequential)]
    public class ConnExtraData
    {
        // server的CONNID
        public uint ConnIdForServer;

        // client的CONNID
        public uint ConnIdForClient;

        // 保存server端指针,方便在cclient里调用
        public TcpServer Server;

        // 释放方式
        public uint FreeType;
    }
}

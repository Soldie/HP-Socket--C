using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using HPSocketCS.SDK;

namespace HPSocketCS
{
    public class TcpServer
    {
        protected IntPtr _pServer = IntPtr.Zero;
        protected IntPtr pServer
        {
            get
            {
                //if (_pServer == IntPtr.Zero)
                //{
                //    throw new Exception("pServer == 0");
                //}

                return _pServer;
            }

            set
            {
                _pServer = value;
            }
        }

        protected IntPtr pListener = IntPtr.Zero;

        protected HPSocketSdk.HP_FN_OnAccept OnAcceptCallback;
        protected HPSocketSdk.HP_FN_OnSend OnSendCallback;
        protected HPSocketSdk.HP_FN_OnPrepareListen OnPrepareListenCallback;
        protected HPSocketSdk.HP_FN_OnReceive OnReceiveCallback;
        protected HPSocketSdk.HP_FN_OnClose OnCloseCallback;
        protected HPSocketSdk.HP_FN_OnError OnErrorCallback;
        protected HPSocketSdk.HP_FN_OnServerShutdown OnServerShutdownCallback;

        protected bool IsSetCallback = false;
        protected bool IsCreate = false;

        /// <summary>
        /// tcpserver构造
        /// </summary>
        public TcpServer()
        {
            CreateListener();
        }

        ~TcpServer()
        {
            //if (HasStarted() == true)
            //{
            //    Stop();
            //}

            Destroy();
        }

        /// <summary>
        /// 创建socket监听&服务组件
        /// </summary>
        /// <param name="isUseDefaultCallback">是否使用tcpserver类默认回调函数</param>
        /// <returns></returns>
        public virtual bool CreateListener()
        {
            if (IsCreate == true || pListener != IntPtr.Zero || pServer != IntPtr.Zero)
            {
                return false;
            }

            pListener = HPSocketSdk.Create_HP_TcpServerListener();
            if (pListener == IntPtr.Zero)
            {
                return false;
            }
            pServer = HPSocketSdk.Create_HP_TcpServer(pListener);
            if (pServer == IntPtr.Zero)
            {
                return false;
            }

            IsCreate = true;

            return true;
        }

        /// <summary>
        /// 释放TcpServer和TcpServerListener
        /// </summary>
        public virtual void Destroy()
        {
            Stop();

            if (pServer != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpServer(pServer);
                pServer = IntPtr.Zero;
            }
            if (pListener != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpServerListener(pListener);
                pListener = IntPtr.Zero;
            }

            IsCreate = false;
        }


        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool Start(string address, ushort port)
        {
            if (IsSetCallback == false)
            {
               // throw new Exception("请在调用Start方法前先调用SetCallback()方法");
            }

            if (HasStarted() == true)
            {
                return false;
            }

            return HPSocketSdk.HP_Server_Start(pServer, address, port);
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            if (HasStarted() == false)
            {
                return false;
            }

            return HPSocketSdk.HP_Server_Stop(pServer);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(uint connId, byte[] bytes, int size)
        {
            return HPSocketSdk.HP_Server_Send(pServer, connId, bytes, size);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(uint connId, IntPtr bufferPtr, int size)
        {
            return HPSocketSdk.HP_Server_Send(pServer, connId, bufferPtr, size);
        }

        /// <summary>
        /// 断开与某个客户的连接
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="bForce">是否强制断开</param>
        /// <returns></returns>
        public bool Disconnect(uint dwConnID, bool force = true)
        {
            return HPSocketSdk.HP_Server_Disconnect(pServer, dwConnID, force);
        }

        /// <summary>
        /// 断开超过指定时间的连接
        /// </summary>
        /// <param name="period">毫秒</param>
        /// <param name="force">强制</param>
        /// <returns></returns>
        public bool DisconnectLongConnections(uint period, bool force = true)
        {
            return HPSocketSdk.HP_Server_DisconnectLongConnections(pServer, period, force);
        }

        /// <summary>
        /// 获取某个连接的远程ip和端口
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool GetRemoteAddress(uint connId, ref string ip, ref ushort port)
        {
            int ipLength = 40;

            StringBuilder sb = new StringBuilder(ipLength);

            bool ret = HPSocketSdk.HP_Server_GetRemoteAddress(pServer, connId, sb, ref ipLength, ref port) && ipLength > 0;
            if (ret == true)
            {
                ip = sb.ToString();
            }

            return ret;
        }

        /// <summary>
        /// 获取错误码
        /// </summary>
        /// <returns></returns>
        public En_HP_SocketError GetlastError()
        {
            return HPSocketSdk.HP_Server_GetLastError(pServer);
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <returns></returns>
        public string GetLastErrorDesc()
        {
            IntPtr ptr = HPSocketSdk.HP_Server_GetLastErrorDesc(pServer);
            string desc = Marshal.PtrToStringUni(ptr);
            return desc;
        }

        /// <summary>
        /// 设置连接的附加数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="obj">如果为null,则为释放设置的数据</param>
        /// <returns></returns>
        public bool SetConnectionExtra(uint connId, object obj)
        {

            IntPtr ptr = IntPtr.Zero;
            if (obj == null)
            {
                // 释放附加数据
                if (HPSocketSdk.HP_Server_GetConnectionExtra(pServer, connId, ref ptr) && ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
                ptr = IntPtr.Zero;
                return HPSocketSdk.HP_Server_GetConnectionExtra(pServer, connId, ref ptr);
            }
            else
            {
                // 设置附加数据
                ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
                Marshal.StructureToPtr(obj, ptr, false);
                return HPSocketSdk.HP_Server_SetConnectionExtra(pServer, connId, ptr);
            }
        }

        /// <summary>
        /// 获取附加数据
        /// 如设置的是个结构体/类对象,可以用 Type objA = (Type)Marshal.PtrToStructure(ptr, typeof(Type)) 获取
        /// 其中Type是结构体/类名,ptr是该方法的传出值,在该方法返回为true的时候可用
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public bool GetConnectionExtra(uint connId, ref IntPtr ptr)
        {
            return HPSocketSdk.HP_Server_GetConnectionExtra(pServer, connId, ref ptr) && ptr != IntPtr.Zero;
        }

        /// <summary>
        /// 获取连接数
        /// </summary>
        /// <returns></returns>
        public uint GetConnectionCount()
        {
            return HPSocketSdk.HP_Server_GetConnectionCount(pServer);
        }

        // 是否启动
        public bool HasStarted()
        {
            if (pServer == IntPtr.Zero)
            {
                return false;
            }
            return HPSocketSdk.HP_Server_HasStarted(pServer);
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        public En_HP_ServiceState GetState()
        {
            return HPSocketSdk.HP_Server_GetState(pServer);
        }

        /// <summary>
        /// 获取监听socket的地址信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool GetListenAddress(ref string ip, ref ushort port)
        {
            int ipLength = 40;

            StringBuilder sb = new StringBuilder(ipLength);

            bool ret = HPSocketSdk.HP_Server_GetListenAddress(pServer, sb, ref ipLength, ref port);
            if (ret == true)
            {
                ip = sb.ToString();
            }
            return ret;
        }

        /// <summary>
        /// 获取指定连接的连接时长
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="period"></param>
        /// <returns></returns>
        public bool GetConnectPeriod(uint connId, ref uint period)
        {
            return HPSocketSdk.HP_Server_GetConnectPeriod(pServer, connId, ref period);
        }


        ///////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 设置工作线程数量（通常设置为 2 * CPU + 2）
        /// </summary>
        /// <param name="val"></param>
        public void SetWorkerThreadCount(uint val)
        {
            HPSocketSdk.HP_Server_SetWorkerThreadCount(pServer, val);
        }

        /// <summary>
        /// 设置 Accept 预投递 Socket 数量（通常设置为工作线程数的 1 - 2 倍）
        /// </summary>
        /// <param name="val"></param>
        public void SetAcceptSocketCount(uint val)
        {
            HPSocketSdk.HP_TcpServer_SetAcceptSocketCount(pServer, val);
        }

        /// <summary>
        /// 设置通信数据缓冲区大小（根据平均通信数据包大小调整设置，通常设置为 1024 的倍数）
        /// </summary>
        /// <param name="val"></param>
        public void SetSocketBufferSize(uint val)
        {
            HPSocketSdk.HP_TcpServer_SetSocketBufferSize(pServer, val);
        }

        /// <summary>
        /// 设置监听 Socket 的等候队列大小（根据并发连接数量调整设置）
        /// </summary>
        /// <param name="val"></param>
        public void SetSocketListenQueue(uint val)
        {
            HPSocketSdk.HP_TcpServer_SetSocketListenQueue(pServer, val);
        }

        /// <summary>
        /// 设置 Socket 缓存对象锁定时间（毫秒，在锁定期间该 Socket 缓存对象不能被获取使用）
        /// </summary>
        /// <param name="val"></param>
        public void HP_Server_SetFreeSocketObjLockTime(uint val)
        {
            HPSocketSdk.HP_Server_SetFreeSocketObjLockTime(pServer, val);
        }

        /// <summary>
        /// 设置 Socket 缓存池大小（通常设置为平均并发连接数量的 1/3 - 1/2）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeSocketObjPool(uint val)
        {
            HPSocketSdk.HP_Server_SetFreeSocketObjPool(pServer, val);
        }

        /// <summary>
        /// 设置内存块缓存池大小（通常设置为 Socket 缓存池大小的 2 - 3 倍）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeBufferObjPool(uint val)
        {
            HPSocketSdk.HP_Server_SetFreeBufferObjPool(pServer, val);
        }

        /// <summary>
        /// 设置 Socket 缓存池回收阀值（通常设置为 Socket 缓存池大小的 3 倍）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeSocketObjHold(uint val)
        {
            HPSocketSdk.HP_Server_SetFreeSocketObjHold(pServer, val);
        }

        /// <summary>
        /// 设置内存块缓存池回收阀值（通常设置为内存块缓存池大小的 3 倍）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeBufferObjHold(uint val)
        {
            HPSocketSdk.HP_Server_SetFreeBufferObjHold(pServer, val);
        }

        /// <summary>
        /// 设置心跳包间隔（毫秒，0 则不发送心跳包）
        /// </summary>
        /// <param name="val"></param>
        public void SetKeepAliveTime(uint val)
        {
            HPSocketSdk.HP_TcpServer_SetKeepAliveTime(pServer, val);
        }

        /// <summary>
        /// 设置心跳确认包检测间隔（毫秒，0 不发送心跳包，如果超过若干次 [默认：WinXP 5 次, Win7 10 次] 检测不到心跳确认包则认为已断线）
        /// </summary>
        /// <param name="val"></param>
        public void SetKeepAliveInterval(uint val)
        {
            HPSocketSdk.HP_TcpServer_SetKeepAliveInterval(pServer, val);
        }

        /// <summary>
        /// 设置关闭服务前等待连接关闭的最长时限（毫秒，0 则不等待）
        /// </summary>
        /// <param name="val"></param>
        public void SetMaxShutdownWaitTime(uint val)
        {
            HPSocketSdk.HP_Server_SetMaxShutdownWaitTime(pServer, val);
        }

        /// <summary>
        /// 获取工作线程数量
        /// </summary>
        /// <returns></returns>
        public uint GetWorkerThreadCount()
        {
            return HPSocketSdk.HP_Server_GetWorkerThreadCount(pServer);
        }

        /// <summary>
        /// 获取 Accept 预投递 Socket 数量
        /// </summary>
        /// <returns></returns>
        public uint GetAcceptSocketCount()
        {
            return HPSocketSdk.HP_TcpServer_GetAcceptSocketCount(pServer);
        }

        /// <summary>
        /// 获取通信数据缓冲区大小
        /// </summary>
        /// <returns></returns>
        public uint GetSocketBufferSize()
        {
            return HPSocketSdk.HP_TcpServer_GetSocketBufferSize(pServer);
        }

        /// <summary>
        /// 获取监听 Socket 的等候队列大小
        /// </summary>
        /// <returns></returns>
        public uint GetSocketListenQueue()
        {
            return HPSocketSdk.HP_TcpServer_GetSocketListenQueue(pServer);
        }

        /// <summary>
        /// 获取 Socket 缓存对象锁定时间
        /// </summary>
        /// <returns></returns>
        public uint GetFreeSocketObjLockTime()
        {
            return HPSocketSdk.HP_Server_GetFreeSocketObjLockTime(pServer);
        }

        /// <summary>
        /// 获取 Socket 缓存池大小
        /// </summary>
        /// <returns></returns>
        public uint GetFreeSocketObjPool()
        {
            return HPSocketSdk.HP_Server_GetFreeSocketObjPool(pServer);
        }

        /// <summary>
        /// 获取内存块缓存池大小
        /// </summary>
        /// <returns></returns>
        public uint GetFreeBufferObjPool()
        {
            return HPSocketSdk.HP_Server_GetFreeBufferObjPool(pServer);
        }

        /// <summary>
        /// 获取 Socket 缓存池回收阀值
        /// </summary>
        /// <returns></returns>
        public uint GetFreeSocketObjHold()
        {
            return HPSocketSdk.HP_Server_GetFreeSocketObjHold(pServer);
        }

        /// <summary>
        /// 获取内存块缓存池回收阀值
        /// </summary>
        /// <returns></returns>
        public uint GetFreeBufferObjHold()
        {
            return HPSocketSdk.HP_Server_GetFreeBufferObjHold(pServer);
        }

        /// <summary>
        /// 获取心跳检查次数
        /// </summary>
        /// <returns></returns>
        public uint GetKeepAliveTime()
        {
            return HPSocketSdk.HP_TcpServer_GetKeepAliveTime(pServer);
        }

        /// <summary>
        /// 获取心跳检查间隔
        /// </summary>
        /// <returns></returns>
        public uint GetKeepAliveInterval()
        {
            return HPSocketSdk.HP_TcpServer_GetKeepAliveInterval(pServer);
        }

        /// <summary>
        /// 获取关闭服务前等待连接关闭的最长时限
        /// </summary>
        /// <returns></returns>
        public uint GetMaxShutdownWaitTime()
        {
            return HPSocketSdk.HP_Server_GetMaxShutdownWaitTime(pServer);
        }

        ///////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 设置回调函数
        /// </summary>
        /// <param name="prepareListen"></param>
        /// <param name="accept"></param>
        /// <param name="send"></param>
        /// <param name="recv"></param>
        /// <param name="close"></param>
        /// <param name="error"></param>
        /// <param name="shutdown"></param>
        public virtual void SetCallback(HPSocketSdk.HP_FN_OnPrepareListen prepareListen, HPSocketSdk.HP_FN_OnAccept accept, HPSocketSdk.HP_FN_OnSend send, HPSocketSdk.HP_FN_OnReceive recv, HPSocketSdk.HP_FN_OnClose close, HPSocketSdk.HP_FN_OnError error, HPSocketSdk.HP_FN_OnServerShutdown shutdown)
        {
            if (IsSetCallback == true)
            {
                throw new Exception("已经调用过SetCallback()方法,如果您确定没手动调用过该方法,并想要手动设置各回调函数,请在构造该类构造函数中传false值,并再次调用该方法。");
            }

            // 设置 Socket 监听器回调函数
            OnAcceptCallback = new HPSocketSdk.HP_FN_OnAccept(accept);
            OnSendCallback = new HPSocketSdk.HP_FN_OnSend(send);
            OnPrepareListenCallback = new HPSocketSdk.HP_FN_OnPrepareListen(prepareListen);
            OnReceiveCallback = new HPSocketSdk.HP_FN_OnReceive(recv);
            OnCloseCallback = new HPSocketSdk.HP_FN_OnClose(close);
            OnErrorCallback = new HPSocketSdk.HP_FN_OnError(error);
            OnServerShutdownCallback = new HPSocketSdk.HP_FN_OnServerShutdown(shutdown);

            // 设置 Socket 监听器回调函数
            HPSocketSdk.HP_Set_FN_Server_OnPrepareListen(pListener, OnPrepareListenCallback);
            HPSocketSdk.HP_Set_FN_Server_OnAccept(pListener, OnAcceptCallback);
            HPSocketSdk.HP_Set_FN_Server_OnSend(pListener, OnSendCallback);
            HPSocketSdk.HP_Set_FN_Server_OnReceive(pListener, OnReceiveCallback);
            HPSocketSdk.HP_Set_FN_Server_OnClose(pListener, OnCloseCallback);
            HPSocketSdk.HP_Set_FN_Server_OnError(pListener, OnErrorCallback);
            HPSocketSdk.HP_Set_FN_Server_OnServerShutdown(pListener, OnServerShutdownCallback);

            IsSetCallback = true;

        }

        public virtual void SetOnServerShutdownCallback(HPSocketSdk.HP_FN_OnServerShutdown shutdown)
        {
            OnServerShutdownCallback = new HPSocketSdk.HP_FN_OnServerShutdown(shutdown);
            HPSocketSdk.HP_Set_FN_Server_OnServerShutdown(pListener, OnServerShutdownCallback);
        }

        public virtual void SetOnErrorCallback(HPSocketSdk.HP_FN_OnError error)
        {
            OnErrorCallback = new HPSocketSdk.HP_FN_OnError(error);
            HPSocketSdk.HP_Set_FN_Server_OnError(pListener, OnErrorCallback);
        }

        public virtual void SetOnCloseCallback(HPSocketSdk.HP_FN_OnClose close)
        {
            OnCloseCallback = new HPSocketSdk.HP_FN_OnClose(close);
            HPSocketSdk.HP_Set_FN_Server_OnClose(pListener, OnCloseCallback);
        }

        public virtual void SetOnReceiveCallback(HPSocketSdk.HP_FN_OnReceive recv)
        {
            OnReceiveCallback = new HPSocketSdk.HP_FN_OnReceive(recv);
            HPSocketSdk.HP_Set_FN_Server_OnReceive(pListener, OnReceiveCallback);
        }

        public virtual void SetOnPrepareListenCallback(HPSocketSdk.HP_FN_OnPrepareListen prepareListen)
        {
            OnPrepareListenCallback = new HPSocketSdk.HP_FN_OnPrepareListen(prepareListen);
            HPSocketSdk.HP_Set_FN_Server_OnPrepareListen(pListener, OnPrepareListenCallback);
        }

        public virtual void SetOnAcceptCallback(HPSocketSdk.HP_FN_OnAccept accept)
        {
            OnAcceptCallback = new HPSocketSdk.HP_FN_OnAccept(accept);
            HPSocketSdk.HP_Set_FN_Server_OnAccept(pListener, OnAcceptCallback);
        }

        public virtual void SetOnSendCallback(HPSocketSdk.HP_FN_OnSend send)
        {
            OnSendCallback = new HPSocketSdk.HP_FN_OnSend(send);
            HPSocketSdk.HP_Set_FN_Server_OnSend(pListener, OnSendCallback);
        }


        ///////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// 监听事件
        /// </summary>
        /// <param name="soListen"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnPrepareListen(IntPtr soListen)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 客户进入
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pClient"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnAccept(uint dwConnID, IntPtr pClient)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 服务器发数据了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnSend(uint dwConnID, IntPtr pData, int iLength)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 数据到达了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnReceive(uint dwConnID, IntPtr pData, int iLength)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 客户离开了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnClose(uint dwConnID)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 出错了
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="enOperation"></param>
        /// <param name="iErrorCode"></param>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnError(uint dwConnID, En_HP_SocketOperation enOperation, int iErrorCode)
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

        /// <summary>
        /// 服务关闭了
        /// </summary>
        /// <returns></returns>
        protected virtual En_HP_HandleResult OnServerShutdown()
        {
            return En_HP_HandleResult.HP_HR_OK;
        }

    }
}

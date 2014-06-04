using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HPSocketCS.SDK;
using System.Runtime.InteropServices;

namespace HPSocketCS
{
    public class TcpAgent
    {
        protected IntPtr _pAgent = IntPtr.Zero;

        protected IntPtr pAgent
        {
            get
            {
                //if (_pClient == IntPtr.Zero)
                //{
                //    throw new Exception("pClient == 0");
                //}

                return _pAgent;
            }

            set
            {
                _pAgent = value;
            }
        }


        protected IntPtr pListener = IntPtr.Zero;

        protected HPSocketSdk.OnConnect OnConnectCallback;
        protected HPSocketSdk.OnSend OnSendCallback;
        protected HPSocketSdk.OnPrepareConnect OnPrepareConnectCallback;
        protected HPSocketSdk.OnReceive OnReceiveCallback;
        protected HPSocketSdk.OnClose OnCloseCallback;
        protected HPSocketSdk.OnError OnErrorCallback;
        protected HPSocketSdk.OnAgentShutdown OnAgentShutdownCallback;

        protected bool IsSetCallback = false;
        protected bool IsCreate = false;

        public TcpAgent()
        {
            CreateListener();
        }

        ~TcpAgent()
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
        /// <param name="isUseDefaultCallback">是否使用tcpAgent类默认回调函数</param>
        /// <returns></returns>
        protected virtual bool CreateListener()
        {
            if (IsCreate == true || pListener != IntPtr.Zero || pAgent != IntPtr.Zero)
            {
                return false;
            }

            pListener = HPSocketSdk.Create_HP_TcpAgentListener();
            if (pListener == IntPtr.Zero)
            {
                return false;
            }

            pAgent = HPSocketSdk.Create_HP_TcpAgent(pListener);
            if (pAgent == IntPtr.Zero)
            {
                return false;
            }

            IsCreate = true;

            return true;
        }

        /// <summary>
        /// 释放TcpAgent和TcpAgentListener
        /// </summary>
        public virtual void Destroy()
        {
            Stop();

            if (pAgent != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpAgent(pAgent);
                pAgent = IntPtr.Zero;
            }
            if (pListener != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpAgentListener(pListener);
                pListener = IntPtr.Zero;
            }

            IsCreate = false;
        }

        /// <summary>
        /// 启动通讯组件
        /// 启动完成后可开始连接远程服务器
        /// </summary>
        /// <param name="address"></param>
        /// <param name="async">是否异步</param>
        /// <returns></returns>
        public bool Start(string address, bool async = false)
        {
            if (string.IsNullOrEmpty(address) == true)
            {
                throw new Exception("address is null");
            }

            if (IsSetCallback == false)
            {
                // throw new Exception("请在调用Start方法前先调用SetCallback()方法");
            }

            if (HasStarted() == true)
            {
                return false;
            }

            return HPSocketSdk.HP_Agent_Start(pAgent, address, async);
        }

        /// <summary>
        /// 停止通讯组件
        /// </summary>
        /// <returns></returns>
        public bool Stop()
        {
            if (HasStarted() == false)
            {
                return false;
            }
            return HPSocketSdk.HP_Agent_Stop(pAgent);
        }


        /// <summary>
        /// 连接服务器，连接成功后 IAgentListener 会接收到 OnConnect() 事件
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="connId"></param>
        /// <returns></returns>
        public bool Connect(string address, ushort port, ref IntPtr connId)
        {
            return HPSocketSdk.HP_Agent_Connect(pAgent, address, port, ref connId);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(IntPtr connId, byte[] bytes, int size)
        {
            return HPSocketSdk.HP_Agent_Send(pAgent, connId, bytes, size);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(IntPtr connId, IntPtr bufferPtr, int size)
        {
            return HPSocketSdk.HP_Agent_Send(pAgent, connId, bufferPtr, size);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bytes"></param>
        /// <param name="offset">针对bytes的偏移</param>
        /// <param name="size">发多大</param>
        /// <returns></returns>
        public bool Send(IntPtr connId, byte[] bytes, int offset, int size)
        {
            return HPSocketSdk.HP_Agent_SendPart(pAgent, connId, bytes, size, offset);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="offset">针对bufferPtr的偏移</param>
        /// <param name="size">发多大</param>
        /// <returns></returns>
        public bool Send(IntPtr connId, IntPtr bufferPtr, int offset, int size)
        {
            return HPSocketSdk.HP_Agent_SendPart(pAgent, connId, bufferPtr, size, offset);
        }

        /// <summary>
        /// 断开某个连接
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="force">强制</param>
        /// <returns></returns>
        public bool Disconnect(IntPtr connId, bool force = true)
        {
            return HPSocketSdk.HP_Agent_Disconnect(pAgent, connId, force);
        }

        /// <summary>
        /// 设置连接的附加数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public bool SetConnectionExtra(IntPtr connId, object obj)
        {

            IntPtr ptr = IntPtr.Zero;
            if (obj == null)
            {
                // 释放附加数据
                if (HPSocketSdk.HP_Agent_GetConnectionExtra(pAgent, connId, ref ptr) && ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(ptr);
                }
                ptr = IntPtr.Zero;
                return HPSocketSdk.HP_Agent_SetConnectionExtra(pAgent, connId, ptr);
            }
            else
            {
                // 设置附加数据
                ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
                Marshal.StructureToPtr(obj, ptr, false);
                return HPSocketSdk.HP_Agent_SetConnectionExtra(pAgent, connId, ptr);
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
        public bool GetConnectionExtra(IntPtr connId, ref IntPtr ptr)
        {
            return HPSocketSdk.HP_Agent_GetConnectionExtra(pAgent, connId, ref ptr) && ptr != IntPtr.Zero;
        }

        /// <summary>
        /// 获取错误码
        /// </summary>
        /// <returns></returns>
        public SocketError GetlastError()
        {
            return HPSocketSdk.HP_Agent_GetLastError(pAgent);
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <returns></returns>
        public string GetLastErrorDesc()
        {
            IntPtr ptr = HPSocketSdk.HP_Agent_GetLastErrorDesc(pAgent);
            string desc = Marshal.PtrToStringUni(ptr);
            return desc;
        }

        /// <summary>
        /// 获取连接中未发出数据的长度
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool GetPendingDataLength(IntPtr connId, ref int length)
        {
            return HPSocketSdk.HP_Agent_GetPendingDataLength(pAgent, connId, ref length);
        }

        // 是否启动
        public bool HasStarted()
        {
            if (pAgent == IntPtr.Zero)
            {
                return false;
            }
            return HPSocketSdk.HP_Agent_HasStarted(pAgent);
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        public ServiceState GetState()
        {
            return HPSocketSdk.HP_Agent_GetState(pAgent);
        }

        /// <summary>
        /// 获取连接数
        /// </summary>
        /// <returns></returns>
        public uint GetConnectionCount()
        {
            return HPSocketSdk.HP_Agent_GetConnectionCount(pAgent);
        }

        /// <summary>
        /// 获取所有连接数,未获取到连接数返回null
        /// </summary>
        /// <returns></returns>
        public IntPtr[] GetAllConnectionIDs()
        {
            IntPtr[] arr = null;
            do
            {
                uint count = GetConnectionCount();
                if (count == 0)
                {
                    break;
                }
                arr = new IntPtr[count];
                if (HPSocketSdk.HP_Agent_GetAllConnectionIDs(pAgent, arr, ref count))
                {
                    if (arr.Length > count)
                    {
                        IntPtr[] newArr = new IntPtr[count];
                        Array.Copy(arr, newArr, count);
                        arr = newArr;
                    }
                    break;
                }
            } while (true);

            return arr;
        }

        /// <summary>
        /// 获取监听socket的地址信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool GetLocalAddress(IntPtr connId, ref string ip, ref ushort port)
        {
            int ipLength = 40;

            StringBuilder sb = new StringBuilder(ipLength);

            bool ret = HPSocketSdk.HP_Agent_GetLocalAddress(pAgent, connId, sb, ref ipLength, ref port);
            if (ret == true)
            {
                ip = sb.ToString();
            }
            return ret;
        }

        /// <summary>
        /// 获取该组件对象的连接Id
        /// </summary>
        /// <returns></returns>
        public bool GetRemoteAddress(IntPtr connId, ref string ip, ref ushort port)
        {
            int ipLength = 40;

            StringBuilder sb = new StringBuilder(ipLength);

            bool ret = HPSocketSdk.HP_Agent_GetRemoteAddress(pAgent, connId, sb, ref ipLength, ref port);
            if (ret == true)
            {
                ip = sb.ToString();
            }
            return ret;
        }

        ///////////////////////////////////////////////////////////////////////////////////////
        /// <summary>
        /// 置是否启用地址重用机制（默认：不启用）
        /// </summary>
        /// <param name="bReuseAddress"></param>
        public void TcpAgent_SetReuseAddress(bool reuseAddress)
        {
            HPSocketSdk.HP_TcpAgent_SetReuseAddress(pAgent, reuseAddress);
        }

        /// <summary>
        /// 检测是否启用地址重用机制
        /// </summary>
        /// <returns></returns>
        public bool TcpAgent_IsReuseAddress()
        {
            return HPSocketSdk.HP_TcpAgent_IsReuseAddress(pAgent);
        }

        /// <summary>
        /// 设置工作线程数量（通常设置为 2 * CPU + 2）
        /// </summary>
        /// <param name="val"></param>
        public void SetWorkerThreadCount(uint val)
        {
            HPSocketSdk.HP_Agent_SetWorkerThreadCount(pAgent, val);
        }

        /// <summary>
        /// 设置通信数据缓冲区大小（根据平均通信数据包大小调整设置，通常设置为 1024 的倍数）
        /// </summary>
        /// <param name="val"></param>
        public void SetSocketBufferSize(uint val)
        {
            HPSocketSdk.HP_TcpAgent_SetSocketBufferSize(pAgent, val);
        }

        /// <summary>
        /// 设置 Socket 缓存对象锁定时间（毫秒，在锁定期间该 Socket 缓存对象不能被获取使用）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeSocketObjLockTime(uint val)
        {
            HPSocketSdk.HP_Agent_SetFreeSocketObjLockTime(pAgent, val);
        }

        /// <summary>
        /// 设置 Socket 缓存池大小（通常设置为平均并发连接数量的 1/3 - 1/2）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeSocketObjPool(uint val)
        {
            HPSocketSdk.HP_Agent_SetFreeSocketObjPool(pAgent, val);
        }

        /// <summary>
        /// 设置内存块缓存池大小（通常设置为 Socket 缓存池大小的 2 - 3 倍）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeBufferObjPool(uint val)
        {
            HPSocketSdk.HP_Agent_SetFreeBufferObjPool(pAgent, val);
        }

        /// <summary>
        /// 设置 Socket 缓存池回收阀值（通常设置为 Socket 缓存池大小的 3 倍）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeSocketObjHold(uint val)
        {
            HPSocketSdk.HP_Agent_SetFreeSocketObjHold(pAgent, val);
        }

        /// <summary>
        /// 设置内存块缓存池回收阀值（通常设置为内存块缓存池大小的 3 倍）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeBufferObjHold(uint val)
        {
            HPSocketSdk.HP_Agent_SetFreeBufferObjHold(pAgent, val);
        }

        /// <summary>
        /// 设置心跳包间隔（毫秒，0 则不发送心跳包）
        /// </summary>
        /// <param name="val"></param>
        public void SetKeepAliveTime(uint val)
        {
            HPSocketSdk.HP_TcpAgent_SetKeepAliveTime(pAgent, val);
        }

        /// <summary>
        /// 设置心跳确认包检测间隔（毫秒，0 不发送心跳包，如果超过若干次 [默认：WinXP 5 次, Win7 10 次] 检测不到心跳确认包则认为已断线）
        /// </summary>
        /// <param name="val"></param>
        public void SetKeepAliveInterval(uint val)
        {
            HPSocketSdk.HP_TcpAgent_SetKeepAliveInterval(pAgent, val);
        }

        /// <summary>
        /// 设置关闭服务前等待连接关闭的最长时限（毫秒，0 则不等待）
        /// </summary>
        /// <param name="val"></param>
        public void SetMaxShutdownWaitTime(uint val)
        {
            HPSocketSdk.HP_Agent_SetMaxShutdownWaitTime(pAgent, val);
        }

        /// <summary>
        /// 获取工作线程数量
        /// </summary>
        /// <returns></returns>
        public uint GetWorkerThreadCount()
        {
            return HPSocketSdk.HP_Agent_GetWorkerThreadCount(pAgent);
        }

        /// <summary>
        /// 获取通信数据缓冲区大小
        /// </summary>
        /// <returns></returns>
        public uint GetSocketBufferSize()
        {
            return HPSocketSdk.HP_TcpAgent_GetSocketBufferSize(pAgent);
        }

        /// <summary>
        /// 获取 Socket 缓存对象锁定时间
        /// </summary>
        /// <returns></returns>
        public uint GetFreeSocketObjLockTime()
        {
            return HPSocketSdk.HP_Agent_GetFreeSocketObjLockTime(pAgent);
        }

        /// <summary>
        /// 获取 Socket 缓存池大小
        /// </summary>
        /// <returns></returns>
        public uint GetFreeSocketObjPool()
        {
            return HPSocketSdk.HP_Agent_GetFreeSocketObjPool(pAgent);
        }

        /// <summary>
        /// 获取内存块缓存池大小
        /// </summary>
        /// <returns></returns>
        public uint GetFreeBufferObjPool()
        {
            return HPSocketSdk.HP_Agent_GetFreeBufferObjPool(pAgent);
        }

        /// <summary>
        /// 获取 Socket 缓存池回收阀值
        /// </summary>
        /// <returns></returns>
        public uint GetFreeSocketObjHold()
        {
            return HPSocketSdk.HP_Agent_GetFreeSocketObjHold(pAgent);
        }

        /// <summary>
        /// 获取内存块缓存池回收阀值
        /// </summary>
        /// <returns></returns>
        public uint GetFreeBufferObjHold()
        {
            return HPSocketSdk.HP_Agent_GetFreeBufferObjHold(pAgent);
        }

        /// <summary>
        /// 获取心跳检查次数
        /// </summary>
        /// <returns></returns>
        public uint GetKeepAliveTime()
        {
            return HPSocketSdk.HP_TcpAgent_GetKeepAliveTime(pAgent);
        }

        /// <summary>
        /// 获取心跳检查间隔
        /// </summary>
        /// <returns></returns>
        public uint GetKeepAliveInterval()
        {
            return HPSocketSdk.HP_TcpAgent_GetKeepAliveInterval(pAgent);
        }

        /// <summary>
        /// 获取关闭服务前等待连接关闭的最长时限
        /// </summary>
        /// <returns></returns>
        public uint GetMaxShutdownWaitTime()
        {
            return HPSocketSdk.HP_Agent_GetMaxShutdownWaitTime(pAgent);
        }

        /// <summary>
        /// 获取系统返回的错误码
        /// </summary>
        /// <returns></returns>
        public int SYSGetLastError()
        {
            return HPSocketSdk.SYS_GetLastError();
        }

        /// <summary>
        /// 根据错误码返回错误信息
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public string GetSocketErrorDesc(SocketError code)
        {
            IntPtr ptr = HPSocketSdk.HP_GetSocketErrorDesc(code);
            string desc = Marshal.PtrToStringUni(ptr);
            return desc;
        }

        /// <summary>
        /// 设置数据发送策略
        /// </summary>
        /// <param name="enSendPolicy"></param>
        public void SetSendPolicy(SendPolicy policy)
        {
            HPSocketSdk.HP_Agent_SetSendPolicy(pAgent, policy);
        }

        /// <summary>
        /// 获取数据发送策略
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        public SendPolicy GetSendPolicy()
        {
            return HPSocketSdk.HP_Agent_GetSendPolicy(pAgent);
        }

        /// <summary>
        /// 设置数据接收策略
        /// </summary>
        /// <param name="enSendPolicy"></param>
        public void SetRecvPolicy(RecvPolicy policy)
        {
            HPSocketSdk.HP_Agent_SetRecvPolicy(pAgent, policy);
        }

        /// <summary>
        /// 获取数据接收策略
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        public RecvPolicy GetRecvPolicy()
        {
            return HPSocketSdk.HP_Agent_GetRecvPolicy(pAgent);
        }
        ///////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 设置回调函数
        /// </summary>
        /// <param name="prepareConnect"></param>
        /// <param name="connect"></param>
        /// <param name="send"></param>
        /// <param name="recv"></param>
        /// <param name="close"></param>
        /// <param name="error"></param>
        /// <param name="agentShutdown"></param>
        public void SetCallback(HPSocketSdk.OnPrepareConnect prepareConnect, HPSocketSdk.OnConnect connect,
            HPSocketSdk.OnSend send, HPSocketSdk.OnReceive recv, HPSocketSdk.OnClose close,
            HPSocketSdk.OnError error, HPSocketSdk.OnAgentShutdown agentShutdown)
        {
            if (IsSetCallback == true)
            {
                throw new Exception("已经调用过SetCallback()方法,如果您确定没手动调用过该方法,并想要手动设置各回调函数,请在构造该类构造函数中传false值,并再次调用该方法。");
            }


            // 设置 Socket 监听器回调函数
            OnConnectCallback = new HPSocketSdk.OnConnect(connect);
            OnSendCallback = new HPSocketSdk.OnSend(send);
            OnPrepareConnectCallback = new HPSocketSdk.OnPrepareConnect(prepareConnect);
            OnReceiveCallback = new HPSocketSdk.OnReceive(recv);
            OnCloseCallback = new HPSocketSdk.OnClose(close);
            OnErrorCallback = new HPSocketSdk.OnError(error);
            OnAgentShutdownCallback = new HPSocketSdk.OnAgentShutdown(agentShutdown);

            // 设置 Socket 监听器回调函数
            HPSocketSdk.HP_Set_FN_Agent_OnPrepareConnect(pListener, OnPrepareConnectCallback);
            HPSocketSdk.HP_Set_FN_Agent_OnConnect(pListener, OnConnectCallback);
            HPSocketSdk.HP_Set_FN_Agent_OnSend(pListener, OnSendCallback);
            HPSocketSdk.HP_Set_FN_Agent_OnReceive(pListener, OnReceiveCallback);
            HPSocketSdk.HP_Set_FN_Agent_OnClose(pListener, OnCloseCallback);
            HPSocketSdk.HP_Set_FN_Agent_OnError(pListener, OnErrorCallback);
            HPSocketSdk.HP_Set_FN_Agent_OnAgentShutdown(pListener, OnAgentShutdownCallback);

            IsSetCallback = true;
        }

        public virtual void SetOnAgentShutdownCallback(HPSocketSdk.OnAgentShutdown agentShutdown)
        {
            OnAgentShutdownCallback = new HPSocketSdk.OnAgentShutdown(agentShutdown);
            HPSocketSdk.HP_Set_FN_Agent_OnAgentShutdown(pListener, OnAgentShutdownCallback);
        }

        public virtual void SetOnErrorCallback(HPSocketSdk.OnError error)
        {
            OnErrorCallback = new HPSocketSdk.OnError(error);
            HPSocketSdk.HP_Set_FN_Server_OnError(pListener, OnErrorCallback);
        }

        public virtual void SetOnCloseCallback(HPSocketSdk.OnClose close)
        {
            OnCloseCallback = new HPSocketSdk.OnClose(close);
            HPSocketSdk.HP_Set_FN_Server_OnClose(pListener, OnCloseCallback);
        }

        public virtual void SetOnReceiveCallback(HPSocketSdk.OnReceive recv)
        {
            OnReceiveCallback = new HPSocketSdk.OnReceive(recv);
            HPSocketSdk.HP_Set_FN_Server_OnReceive(pListener, OnReceiveCallback);
        }

        public virtual void SetOnPrepareConnectCallback(HPSocketSdk.OnPrepareConnect prepareConnect)
        {
            OnPrepareConnectCallback = new HPSocketSdk.OnPrepareConnect(prepareConnect);
            HPSocketSdk.HP_Set_FN_Agent_OnPrepareConnect(pListener, OnPrepareConnectCallback);
        }

        public virtual void SetOnConnectCallback(HPSocketSdk.OnConnect connect)
        {
            OnConnectCallback = new HPSocketSdk.OnConnect(connect);
            HPSocketSdk.HP_Set_FN_Agent_OnConnect(pListener, OnConnectCallback);
        }

        public virtual void SetOnSendCallback(HPSocketSdk.OnSend send)
        {
            OnSendCallback = new HPSocketSdk.OnSend(send);
            HPSocketSdk.HP_Set_FN_Server_OnSend(pListener, OnSendCallback);
        }

        /////////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 准备连接了
        /// </summary>
        /// <param name="dwConnId"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        protected virtual HandleResult OnPrepareConnect(IntPtr dwConnId, uint socket)
        {
            return HandleResult.Ok;
        }

        /// <summary>
        /// 已连接
        /// </summary>
        /// <param name="dwConnId"></param>
        /// <returns></returns>
        protected virtual HandleResult OnConnect(IntPtr dwConnId)
        {
            return HandleResult.Ok;
        }

        /// <summary>
        /// 客户端发数据了
        /// </summary>
        /// <param name="dwConnId"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual HandleResult OnSend(IntPtr dwConnId, IntPtr pData, int iLength)
        {
            return HandleResult.Ok;
        }

        /// <summary>
        /// 数据到达了
        /// </summary>
        /// <param name="dwConnId"></param>
        /// <param name="pData"></param>
        /// <param name="iLength"></param>
        /// <returns></returns>
        protected virtual HandleResult OnReceive(IntPtr dwConnId, IntPtr pData, int iLength)
        {
            return HandleResult.Ok;
        }

        /// <summary>
        /// 连接关闭了
        /// </summary>
        /// <param name="dwConnId"></param>
        /// <returns></returns>
        protected virtual HandleResult OnClose(IntPtr dwConnId)
        {
            return HandleResult.Ok;
        }

        /// <summary>
        /// 出错了
        /// </summary>
        /// <param name="dwConnId"></param>
        /// <param name="enOperation"></param>
        /// <param name="iErrorCode"></param>
        /// <returns></returns>
        protected virtual HandleResult OnError(IntPtr dwConnId, SocketOperation enOperation, int iErrorCode)
        {
            return HandleResult.Ok;
        }

        /// <summary>
        /// Agent关闭了
        /// </summary>
        /// <returns></returns>
        protected virtual HandleResult OnAgentShutdown()
        {
            return HandleResult.Ok;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using HPSocketCS.SDK;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace HPSocketCS
{
    public class TcpClient
    {
        protected IntPtr _pClient = IntPtr.Zero;

        protected IntPtr pClient
        {
            get
            {
                //if (_pClient == IntPtr.Zero)
                //{
                //    throw new Exception("pClient == 0");
                //}

                return _pClient;
            }

            set
            {
                _pClient = value;
            }
        }

        protected IntPtr pListener = IntPtr.Zero;

        protected bool IsSetCallback = false;
        protected bool IsCreate = false;

        /****************************************************/
        /************** TcpClient 回调函数 **************/

        public delegate HandleResult OnPrepareConnect(TcpClient sender, uint socket);
        public delegate HandleResult OnConnect(TcpClient sender);
        public delegate HandleResult OnSend(TcpClient sender, IntPtr pData, int iLength);
        public delegate HandleResult OnReceive(TcpClient sender, IntPtr pData, int iLength);
        public delegate HandleResult OnClose(TcpClient sender);
        public delegate HandleResult OnError(TcpClient sender, SocketOperation enOperation, int iErrorCode);

        protected OnPrepareConnect OnPrepareConnectCallback;
        protected OnConnect OnConnectCallback;
        protected OnSend OnSendCallback;
        protected OnReceive OnReceiveCallback;
        protected OnClose OnCloseCallback;
        protected OnError OnErrorCallback;

        protected HPSocketSdk.OnPrepareConnect SDK_OnPrepareConnectCallback;
        protected HPSocketSdk.OnConnect SDK_OnConnectCallback;
        protected HPSocketSdk.OnSend SDK_OnSendCallback;
        protected HPSocketSdk.OnReceive SDK_OnReceiveCallback;
        protected HPSocketSdk.OnClose SDK_OnCloseCallback;
        protected HPSocketSdk.OnError SDK_OnErrorCallback;

        HandleResult SDK_OnPrepareConnect(IntPtr pClient, uint socket)
        {
           return OnPrepareConnectCallback(this, socket);
        }

        HandleResult SDK_OnConnect(IntPtr pClient)
        {
            return OnConnectCallback(this);
        }

        HandleResult SDK_OnSend(IntPtr pClient, IntPtr pData, int iLength)
        {
            return OnSendCallback(this, pData, iLength);
        }

        HandleResult SDK_OnReceive(IntPtr pClient, IntPtr pData, int iLength)
        {
            return OnReceiveCallback(this, pData, iLength);
        }

        HandleResult SDK_OnClose(IntPtr pClient)
        {
            return OnCloseCallback(this);
        }

        HandleResult SDK_OnError(IntPtr pClient, SocketOperation enOperation, int iErrorCode)
        {
            return OnErrorCallback(this, enOperation, iErrorCode);
        }

        public TcpClient()
        {
            CreateListener();
        }

        ~TcpClient()
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
        protected virtual bool CreateListener()
        {
            if (IsCreate == true || pListener != IntPtr.Zero || pClient != IntPtr.Zero)
            {
                return false;
            }

            pListener = HPSocketSdk.Create_HP_TcpClientListener();
            if (pListener == IntPtr.Zero)
            {
                return false;
            }

            pClient = HPSocketSdk.Create_HP_TcpClient(pListener);
            if (pClient == IntPtr.Zero)
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

            if (pClient != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpClient(pClient);
                pClient = IntPtr.Zero;
            }
            if (pListener != IntPtr.Zero)
            {
                HPSocketSdk.Destroy_HP_TcpClientListener(pListener);
                pListener = IntPtr.Zero;
            }

            IsCreate = false;
        }

        /// <summary>
        /// 启动通讯组件并连接到服务器
        /// </summary>
        /// <param name="address"></param>
        /// <param name="port"></param>
        /// <param name="async">是否异步</param>
        /// <returns></returns>
        public bool Start(string address, ushort port, bool async = false)
        {
            if (string.IsNullOrEmpty(address) == true)
            {
                throw new Exception("address is null");
            }
            else if (port == 0)
            {
                throw new Exception("port is zero");
            }

            if (IsSetCallback == false)
            {
                // throw new Exception("请在调用Start方法前先调用SetCallback()方法");
            }

            if (HasStarted() == true)
            {
                return false;
            }

            return HPSocketSdk.HP_Client_Start(pClient, address, port, async);
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
            return HPSocketSdk.HP_Client_Stop(pClient);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bytes"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(byte[] bytes, int size)
        {
            return HPSocketSdk.HP_Client_Send(pClient, bytes, size);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send(IntPtr bufferPtr, int size)
        {
            return HPSocketSdk.HP_Client_Send(pClient, bufferPtr, size);
        }


        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send<T>(T obj)
        {
            byte[] buffer = StructureToByte<T>(obj);
            return Send(buffer, buffer.Length);
        }

        /// <summary>
        /// 序列化对象后发送数据,序列化对象所属类必须标记[Serializable]
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool SendBySerializable(object obj)
        {
            byte[] buffer = ObjectToBytes(obj);
            return Send(buffer, buffer.Length);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bytes"></param>
        /// <param name="offset">针对bytes的偏移</param>
        /// <param name="size">发多大</param>
        /// <returns></returns>
        public bool Send(byte[] bytes, int offset, int size)
        {
            return HPSocketSdk.HP_Client_SendPart(pClient, bytes, size, offset);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="offset">针对bufferPtr的偏移</param>
        /// <param name="size">发多大</param>
        /// <returns></returns>
        public bool Send(IntPtr bufferPtr, int offset, int size)
        {
            return HPSocketSdk.HP_Client_SendPart(pClient, bufferPtr, size, offset);
        }

        /// <summary>
        /// 发送多组数据
        /// 向指定连接发送多组数据
        /// TCP - 顺序发送所有数据包
        /// </summary>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffers">发送缓冲区数组</param>
        /// <param name="iCount">发送缓冲区数目</param>
        /// <returns>TRUE.成功,FALSE.失败，可通过 SYSGetLastError() 获取 Windows 错误代码</returns>
        public bool SendPackets(WSABUF[] buffers, int count)
        {
            return HPSocketSdk.HP_Client_SendPackets(pClient, buffers, count);
        }


        /// <summary>
        /// 发送多组数据
        /// 向指定连接发送多组数据
        /// TCP - 顺序发送所有数据包
        /// </summary>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffers">发送缓冲区数组</param>
        /// <param name="iCount">发送缓冲区数目</param>
        /// <returns>TRUE.成功,FALSE.失败，可通过 SYSGetLastError() 获取 Windows 错误代码</returns>
        public bool SendPackets<T>(T[] objects)
        {
            bool ret = false;

            WSABUF[] buffer = new WSABUF[objects.Length];
            IntPtr[] ptrs = new IntPtr[buffer.Length];
            try
            {

                for (int i = 0; i < objects.Length; i++)
                {
                    buffer[i].Length = Marshal.SizeOf(typeof(T));

                    ptrs[i] = Marshal.AllocHGlobal(buffer[i].Length);
                    Marshal.StructureToPtr(objects[i], ptrs[i], true);

                    buffer[i].Buffer = ptrs[i];
                }
                ret = SendPackets(buffer, buffer.Length);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                for (int i = 0; i < ptrs.Length; i++)
                {
                    if (ptrs[i] != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(ptrs[i]);
                    }
                }
            }

            return ret;
        }

        /// <summary>
        /// 名称：发送小文件
        /// 描述：向指定连接发送 4096 KB 以下的小文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="head">头部附加数据</param>
        /// <param name="tail">尾部附加数据</param>
        /// <returns>TRUE.成功,FALSE.失败，可通过 SYSGetLastError() 获取 Windows 错误代码</returns>
        public bool SendSmallFile(string filePath, ref WSABUF head, ref WSABUF tail)
        {
            return HPSocketSdk.HP_TcpClient_SendSmallFile(pClient, filePath, ref head, ref tail);
        }

        /// <summary>
        /// 名称：发送小文件
        /// 描述：向指定连接发送 4096 KB 以下的小文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="head">头部附加数据,可以为null</param>
        /// <param name="tail">尾部附加数据,可以为null</param>
        /// <returns>TRUE.成功,FALSE.失败，可通过 SYSGetLastError() 获取 Windows 错误代码</returns>
        public bool SendSmallFile(string filePath, byte[] head, byte[] tail)
        {
            IntPtr pHead = IntPtr.Zero;
            IntPtr pTail = IntPtr.Zero;
            WSABUF wsaHead = new WSABUF() { Length = 0, Buffer = pHead };
            WSABUF wsatail = new WSABUF() { Length = 0, Buffer = pTail };
            if (head != null)
            {
                wsaHead.Length = head.Length;
                wsaHead.Buffer = Marshal.UnsafeAddrOfPinnedArrayElement(head, 0);
            }

            if (tail != null)
            {
                wsaHead.Length = tail.Length;
                wsaHead.Buffer = Marshal.UnsafeAddrOfPinnedArrayElement(tail, 0);
            }

            return SendSmallFile(filePath, ref wsaHead, ref wsatail);
        }

        /// <summary>
        /// 名称：发送小文件
        /// 描述：向指定连接发送 4096 KB 以下的小文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="head">头部附加数据,可以为null</param>
        /// <param name="tail">尾部附加数据,可以为null</param>
        /// <returns>TRUE.成功,FALSE.失败，可通过 SYSGetLastError() 获取 Windows 错误代码</returns>
        public bool SendSmallFile<T1, T2>(string filePath, T1 head, T2 tail)
        {

            byte[] headBuffer = null;
            if (head != null)
            {
                headBuffer = StructureToByte<T1>(head);
            }

            byte[] tailBuffer = null;
            if (tail != null)
            {
                StructureToByte<T1>(head);
            }
            return SendSmallFile(filePath, headBuffer, tailBuffer);
        }

        /// <summary>
        /// 获取错误码
        /// </summary>
        /// <returns></returns>
        public SocketError GetlastError()
        {
            return HPSocketSdk.HP_Client_GetLastError(pClient);
        }

        /// <summary>
        /// 获取错误信息
        /// </summary>
        /// <returns></returns>
        public string GetLastErrorDesc()
        {
            IntPtr ptr = HPSocketSdk.HP_Client_GetLastErrorDesc(pClient);
            string desc = Marshal.PtrToStringUni(ptr);
            return desc;
        }

        /// <summary>
        /// 获取连接中未发出数据的长度
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool GetPendingDataLength(ref int length)
        {
            return HPSocketSdk.HP_Client_GetPendingDataLength(pClient, ref length);
        }

        /// <summary>
        /// 设置连接的附加数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="obj">如果为null,则为释放设置的数据</param>
        /// <returns></returns>
        public void SetExtra(IntPtr client, object obj)
        {
            // 释放附加数据
            IntPtr ptr = HPSocketSdk.HP_Client_GetExtra(client);
            if (ptr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(ptr);
                ptr = IntPtr.Zero;
            }

            if (obj != null)
            {
                // 设置附加数据
                ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
                Marshal.StructureToPtr(obj, ptr, false);
            }
            HPSocketSdk.HP_Client_SetExtra(pClient, ptr);
        }

        /// <summary>
        /// 获取附加数据
        /// 如设置的是个结构体/类对象,可以用 Type objA = (Type)Marshal.PtrToStructure(ptr, typeof(Type)) 获取
        /// 其中Type是结构体/类名,ptr是该方法的传出值,在该方法返回为true的时候可用
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="ptr"></param>
        /// <returns></returns>
        public IntPtr GetExtra(IntPtr client)
        {
            return HPSocketSdk.HP_Client_GetExtra(client);
        }

        // 是否启动
        public bool HasStarted()
        {
            if (pClient == IntPtr.Zero)
            {
                return false;
            }
            return HPSocketSdk.HP_Client_HasStarted(pClient);
        }

        /// <summary>
        /// 获取状态
        /// </summary>
        /// <returns></returns>
        public ServiceState GetState()
        {
            return HPSocketSdk.HP_Client_GetState(pClient);
        }

        /// <summary>
        /// 获取监听socket的地址信息
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="ipLength"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public bool GetListenAddress(ref string ip, ref ushort port)
        {
            int ipLength = 40;

            StringBuilder sb = new StringBuilder(ipLength);

            bool ret = HPSocketSdk.HP_Client_GetLocalAddress(pClient, sb, ref ipLength, ref port);
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
        public IntPtr GetConnectionId()
        {
            return HPSocketSdk.HP_Client_GetConnectionID(pClient);
        }

        ///////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 设置内存块缓存池大小（通常设置为 -> PUSH 模型：5 - 10；PULL 模型：10 - 20 ）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeBufferPoolSize(uint val)
        {
            HPSocketSdk.HP_Client_SetFreeBufferPoolSize(pClient, val);
        }

        /// <summary>
        ///  设置内存块缓存池回收阀值（通常设置为内存块缓存池大小的 3 倍）
        /// </summary>
        /// <param name="val"></param>
        public void SetFreeBufferPoolHold(uint val)
        {
            HPSocketSdk.HP_Client_SetFreeBufferPoolHold(pClient, val);
        }

        /// <summary>
        /// 获取内存块缓存池大小
        /// </summary>
        /// <returns></returns>
        public uint GetFreeBufferPoolSize()
        {
            return HPSocketSdk.HP_Client_GetFreeBufferPoolSize(pClient);
        }

        /// <summary>
        /// 获取内存块缓存池回收阀值
        /// </summary>
        /// <returns></returns>
        public uint GetFreeBufferPoolHold()
        {
            return HPSocketSdk.HP_Client_GetFreeBufferPoolHold(pClient);
        }

        ///////////////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 设置通信数据缓冲区大小（根据平均通信数据包大小调整设置，通常设置为：(N * 1024) - sizeof(TBufferObj)）
        /// </summary>
        /// <param name="val"></param>
        public void SetSocketBufferSize(uint val)
        {
            HPSocketSdk.HP_TcpClient_SetSocketBufferSize(pClient, val);
        }

        /// <summary>
        /// 设置心跳包间隔（毫秒，0 则不发送心跳包）
        /// </summary>
        /// <param name="val"></param>
        public void SetKeepAliveTime(uint val)
        {
            HPSocketSdk.HP_TcpClient_SetKeepAliveTime(pClient, val);
        }

        /// <summary>
        /// 设置心跳确认包检测间隔（毫秒，0 不发送心跳包，如果超过若干次 [默认：WinXP 5 次, Win7 10 次] 检测不到心跳确认包则认为已断线）
        /// </summary>
        /// <returns></returns>
        public void SetKeepAliveInterval(uint val)
        {
            HPSocketSdk.HP_TcpClient_SetKeepAliveInterval(pClient, val);
        }

        /// <summary>
        /// 获取通信数据缓冲区大小
        /// </summary>
        /// <returns></returns>
        public uint GetSocketBufferSize()
        {
            return HPSocketSdk.HP_TcpClient_GetSocketBufferSize(pClient);
        }

        /// <summary>
        /// 获取心跳检查次数
        /// </summary>
        /// <returns></returns>
        public uint GetKeepAliveTime()
        {
            return HPSocketSdk.HP_TcpClient_GetKeepAliveTime(pClient);
        }

        /// <summary>
        /// 获取心跳检查间隔
        /// </summary>
        /// <returns></returns>
        public uint GetKeepAliveInterval()
        {
            return HPSocketSdk.HP_TcpClient_GetKeepAliveInterval(pClient);
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
        public void SetCallback(OnPrepareConnect prepareConnect, OnConnect connect,
            OnSend send, OnReceive recv, OnClose close,
            OnError error)
        {
            if (IsSetCallback == true)
            {
                throw new Exception("已经调用过SetCallback()方法,如果您确定没手动调用过该方法,并想要手动设置各回调函数,请在构造该类构造函数中传false值,并再次调用该方法。");
            }

            // 设置 Socket 监听器回调函数
            SetOnPrepareConnectCallback(prepareConnect);
            SetOnConnectCallback(connect);
            SetOnSendCallback(send);
            SetOnReceiveCallback(recv);
            SetOnCloseCallback(close);
            SetOnErrorCallback(error);

            IsSetCallback = true;
        }

        public virtual void SetOnPrepareConnectCallback(OnPrepareConnect prepareConnect)
        {
            if (prepareConnect != null)
            {
                OnPrepareConnectCallback = new OnPrepareConnect(prepareConnect);
                SDK_OnPrepareConnectCallback = new HPSocketSdk.OnPrepareConnect(SDK_OnPrepareConnect);
            }
            else
            {
                OnPrepareConnectCallback = null;
                SDK_OnPrepareConnectCallback = null;
            }
 
            HPSocketSdk.HP_Set_FN_Client_OnPrepareConnect(pListener, SDK_OnPrepareConnectCallback);
        }

        public virtual void SetOnConnectCallback(OnConnect connect)
        {
            if (connect != null)
            {
                OnConnectCallback = new OnConnect(connect);
                SDK_OnConnectCallback = new HPSocketSdk.OnConnect(SDK_OnConnect);
            }
            else
            {
                OnConnectCallback = null;
                SDK_OnConnectCallback = null;
            }

            HPSocketSdk.HP_Set_FN_Client_OnConnect(pListener, SDK_OnConnectCallback);
        }

        public virtual void SetOnSendCallback(OnSend send)
        {
            if (send != null)
            {
                OnSendCallback = new OnSend(send);
                SDK_OnSendCallback = new HPSocketSdk.OnSend(SDK_OnSend);
            }
            else
            {
                OnSendCallback = null;
                SDK_OnSendCallback = null;
            }

             HPSocketSdk.HP_Set_FN_Client_OnSend(pListener, SDK_OnSendCallback);
        }

        public virtual void SetOnReceiveCallback(OnReceive recv)
        {
            if (recv != null)
            {
                OnReceiveCallback = new OnReceive(recv);
                SDK_OnReceiveCallback = new HPSocketSdk.OnReceive(SDK_OnReceive);
            }
            else
            {
                OnReceiveCallback = null;
                SDK_OnReceiveCallback = null;
            }

            HPSocketSdk.HP_Set_FN_Client_OnReceive(pListener, SDK_OnReceiveCallback);
        }

        public virtual void SetOnCloseCallback(OnClose close)
        {
            if (close != null)
            {
                OnCloseCallback = new OnClose(close);
                SDK_OnCloseCallback = new HPSocketSdk.OnClose(SDK_OnClose);
            }
            else
            {
                OnCloseCallback = null;
                SDK_OnCloseCallback = null;
            }

             HPSocketSdk.HP_Set_FN_Client_OnClose(pListener, SDK_OnCloseCallback);
        }

        public virtual void SetOnErrorCallback(OnError error)
        {
            if (error != null)
            {
                OnErrorCallback = new OnError(error);
                SDK_OnErrorCallback = new HPSocketSdk.OnError(SDK_OnError);
            }
            else
            {
                OnErrorCallback = null;
                SDK_OnErrorCallback = null;
            }

            HPSocketSdk.HP_Set_FN_Client_OnError(pListener, SDK_OnErrorCallback);
        }

        ///////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// 获取系统返回的错误码
        /// </summary>
        /// <returns></returns>
        public int SYSGetLastError()
        {
            return HPSocketSdk.SYS_GetLastError();
        }

        /// <summary>
        /// 调用系统的 ::WSAGetLastError() 方法获取通信错误代码
        /// </summary>
        /// <returns></returns>
        public int SYSWSAGetLastError()
        {
            return HPSocketSdk.SYS_WSAGetLastError();
        }

        /// <summary>
        /// 调用系统的 setsockopt()
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="level"></param>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        /// 
        public int SYS_SetSocketOption(IntPtr sock, int level, int name, IntPtr val, int len)
        {
            return HPSocketSdk.SYS_SetSocketOption(sock, level, name, val, len);
        }

        /// <summary>
        /// 调用系统的 getsockopt()
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="level"></param>
        /// <param name="name"></param>
        /// <param name="val"></param>
        /// <param name="len"></param>
        /// <returns></returns>
        /// 
        public int SYSGetSocketOption(IntPtr sock, int level, int name, IntPtr val, ref int len)
        {
            return HPSocketSdk.SYS_GetSocketOption(sock, level, name, val, ref len);
        }
        /// <summary>
        /// 调用系统的 ioctlsocket()
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="cmd"></param>
        /// <param name="arg"></param>
        /// <returns></returns>
        /// 
        public int SYSIoctlSocket(IntPtr sock, long cmd, IntPtr arg)
        {
            return HPSocketSdk.SYS_IoctlSocket(sock, cmd, arg);
        }

        /// <summary>
        /// 调用系统的 ::WSAIoctl()
        /// </summary>
        /// <param name="sock"></param>
        /// <param name="dwIoControlCode"></param>
        /// <param name="lpvInBuffer"></param>
        /// <param name="cbInBuffer"></param>
        /// <param name="lpvOutBuffer"></param>
        /// <param name="cbOutBuffer"></param>
        /// <param name="lpcbBytesReturned"></param>
        /// <returns></returns>
        public int SYS_WSAIoctl(IntPtr sock, uint dwIoControlCode, IntPtr lpvInBuffer, uint cbInBuffer,
                                              IntPtr lpvOutBuffer, uint cbOutBuffer, uint lpcbBytesReturned)
        {
            return HPSocketSdk.SYS_WSAIoctl(sock, dwIoControlCode, lpvInBuffer, cbInBuffer,
                                            lpvOutBuffer, cbOutBuffer, lpcbBytesReturned);
        }

        /// <summary>
        /// 由结构体转换为byte数组
        /// </summary>
        public byte[] StructureToByte<T>(T structure)
        {
            int size = Marshal.SizeOf(typeof(T));
            byte[] buffer = new byte[size];
            IntPtr bufferIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.StructureToPtr(structure, bufferIntPtr, true);
                Marshal.Copy(bufferIntPtr, buffer, 0, size);
            }
            finally
            {
                Marshal.FreeHGlobal(bufferIntPtr);
            }
            return buffer;
        }

        /// <summary>
        /// 由byte数组转换为结构体
        /// </summary>
        public T ByteToStructure<T>(byte[] dataBuffer)
        {
            object structure = null;
            int size = Marshal.SizeOf(typeof(T));
            IntPtr allocIntPtr = Marshal.AllocHGlobal(size);
            try
            {
                Marshal.Copy(dataBuffer, 0, allocIntPtr, size);
                structure = Marshal.PtrToStructure(allocIntPtr, typeof(T));
            }
            finally
            {
                Marshal.FreeHGlobal(allocIntPtr);
            }
            return (T)structure;
        }

        /// <summary>
        /// 对象序列化成byte[]
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public byte[] ObjectToBytes(object obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                IFormatter formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                return ms.GetBuffer();
            }
        }

        /// <summary>
        /// byte[]序列化成对象
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public object BytesToObject(byte[] bytes)
        {
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                IFormatter formatter = new BinaryFormatter();
                return formatter.Deserialize(ms);
            }
        }
    }
}

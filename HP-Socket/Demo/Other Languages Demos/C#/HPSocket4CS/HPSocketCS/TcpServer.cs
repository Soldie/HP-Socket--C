using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using HPSocketCS.SDK;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;

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

        protected HPSocketSdk.OnAccept OnAcceptCallback;
        protected HPSocketSdk.OnSend OnSendCallback;
        protected HPSocketSdk.OnPrepareListen OnPrepareListenCallback;
        protected HPSocketSdk.OnReceive OnReceiveCallback;
        protected HPSocketSdk.OnClose OnCloseCallback;
        protected HPSocketSdk.OnError OnErrorCallback;
        protected HPSocketSdk.OnServerShutdown OnServerShutdownCallback;

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
        protected virtual bool CreateListener()
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
        public bool Send(IntPtr connId, byte[] bytes, int size)
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
        public bool Send(IntPtr connId, IntPtr bufferPtr, int size)
        {
            return HPSocketSdk.HP_Server_Send(pServer, connId, bufferPtr, size);
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
            return HPSocketSdk.HP_Server_SendPart(pServer, connId, bytes, size, offset);
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
            return HPSocketSdk.HP_Server_SendPart(pServer, connId, bufferPtr, size, offset);
        }

        /// <summary>
        /// 发送数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool Send<T>(IntPtr connId, T obj)
        {
            byte[] buffer = StructureToByte<T>(obj);
            return Send(connId, buffer, buffer.Length);
        }

        /// <summary>
        /// 序列化对象后发送数据,序列化对象所属类必须标记[Serializable]
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="bufferPtr"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public bool SendBySerializable(IntPtr connId, object obj)
        {
            byte[] buffer = ObjectToBytes(obj);
            return Send(connId, buffer, buffer.Length);
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
        public bool SendPackets(IntPtr connId, WSABUF[] pBuffers, int count)
        {
            return HPSocketSdk.HP_Server_SendPackets(pServer, connId, pBuffers, count);
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
        public bool SendPackets<T>(IntPtr connId, T[] objects)
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
                ret = SendPackets(connId, buffer, buffer.Length);
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
        /// <param name="connId"></param>
        /// <param name="filePath">文件路径</param>
        /// <param name="head">头部附加数据</param>
        /// <param name="tail">尾部附加数据</param>
        /// <returns>TRUE.成功,FALSE.失败，可通过 SYSGetLastError() 获取 Windows 错误代码</returns>
        public bool SendSmallFile(IntPtr connId, string filePath, ref WSABUF head, ref WSABUF tail)
        {
            return HPSocketSdk.HP_TcpServer_SendSmallFile(pServer, connId, filePath, ref head, ref tail);
        }

        /// <summary>
        /// 名称：发送小文件
        /// 描述：向指定连接发送 4096 KB 以下的小文件
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="filePath">文件路径</param>
        /// <param name="head">头部附加数据,可以为null</param>
        /// <param name="tail">尾部附加数据,可以为null</param>
        /// <returns>TRUE.成功,FALSE.失败，可通过 SYSGetLastError() 获取 Windows 错误代码</returns>
        public bool SendSmallFile(IntPtr connId, string filePath, byte[] head, byte[] tail)
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

            return SendSmallFile(connId, filePath, ref wsaHead, ref wsatail);
        }

        /// <summary>
        /// 名称：发送小文件
        /// 描述：向指定连接发送 4096 KB 以下的小文件
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="filePath">文件路径</param>
        /// <param name="head">头部附加数据,可以为null</param>
        /// <param name="tail">尾部附加数据,可以为null</param>
        /// <returns>TRUE.成功,FALSE.失败，可通过 SYSGetLastError() 获取 Windows 错误代码</returns>
        public bool SendSmallFile<T1, T2>(IntPtr connId, string filePath, T1 head, T2 tail)
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
            return SendSmallFile(connId, filePath, headBuffer, tailBuffer);
        }

        /// <summary>
        /// 断开与某个客户的连接
        /// </summary>
        /// <param name="dwConnId"></param>
        /// <param name="bForce">是否强制断开</param>
        /// <returns></returns>
        public bool Disconnect(IntPtr dwConnId, bool force = true)
        {
            return HPSocketSdk.HP_Server_Disconnect(pServer, dwConnId, force);
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
        public bool GetRemoteAddress(IntPtr connId, ref string ip, ref ushort port)
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
        public SocketError GetlastError()
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
        /// 获取连接中未发出数据的长度
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public bool GetPendingDataLength(IntPtr connId, ref int length)
        {
            return HPSocketSdk.HP_Server_GetPendingDataLength(pServer, connId, ref length);
        }

        /// <summary>
        /// 设置连接的附加数据
        /// </summary>
        /// <param name="connId"></param>
        /// <param name="obj">如果为null,则为释放设置的数据</param>
        /// <returns></returns>
        public bool SetConnectionExtra(IntPtr connId, object obj)
        {

            IntPtr ptr = IntPtr.Zero;
            // 释放附加数据
            if (HPSocketSdk.HP_Server_GetConnectionExtra(pServer, connId, ref ptr) && ptr != IntPtr.Zero)
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

            return HPSocketSdk.HP_Server_SetConnectionExtra(pServer, connId, ptr);
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
            return HPSocketSdk.HP_Server_GetConnectionExtra(pServer, connId, ref ptr) && ptr != IntPtr.Zero;
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
        public ServiceState GetState()
        {
            return HPSocketSdk.HP_Server_GetState(pServer);
        }

        /// <summary>
        /// 获取连接数
        /// </summary>
        /// <returns></returns>
        public uint GetConnectionCount()
        {
            return HPSocketSdk.HP_Server_GetConnectionCount(pServer);
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
                if (HPSocketSdk.HP_Server_GetAllConnectionIDs(pServer, arr, ref count))
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
        public bool GetConnectPeriod(IntPtr connId, ref uint period)
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
        /// 设置 Accept 预投递数量（根据负载调整设置，Accept 预投递数量越大则支持的并发连接请求越多）
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
        public void Server_SetFreeSocketObjLockTime(uint val)
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
        /// 获取 Accept 预投递数量
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
            HPSocketSdk.HP_Server_SetSendPolicy(pServer, policy);
        }

        /// <summary>
        /// 获取数据发送策略
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        public SendPolicy GetSendPolicy()
        {
            return HPSocketSdk.HP_Server_GetSendPolicy(pServer);
        }

        /// <summary>
        /// 设置数据接收策略
        /// </summary>
        /// <param name="enSendPolicy"></param>
        public void SetRecvPolicy(RecvPolicy policy)
        {
            HPSocketSdk.HP_Server_SetRecvPolicy(pServer, policy);
        }

        /// <summary>
        /// 获取数据接收策略
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        public RecvPolicy GetRecvPolicy()
        {
            return HPSocketSdk.HP_Server_GetRecvPolicy(pServer);
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
        public virtual void SetCallback(HPSocketSdk.OnPrepareListen prepareListen, HPSocketSdk.OnAccept accept, HPSocketSdk.OnSend send, HPSocketSdk.OnReceive recv, HPSocketSdk.OnClose close, HPSocketSdk.OnError error, HPSocketSdk.OnServerShutdown shutdown)
        {
            if (IsSetCallback == true)
            {
                throw new Exception("已经调用过SetCallback()方法,如果您确定没手动调用过该方法,并想要手动设置各回调函数,请在构造该类构造函数中传false值,并再次调用该方法。");
            }

            // 设置 Socket 监听器回调函数
            OnAcceptCallback = new HPSocketSdk.OnAccept(accept);
            OnSendCallback = new HPSocketSdk.OnSend(send);
            OnPrepareListenCallback = new HPSocketSdk.OnPrepareListen(prepareListen);
            OnReceiveCallback = new HPSocketSdk.OnReceive(recv);
            OnCloseCallback = new HPSocketSdk.OnClose(close);
            OnErrorCallback = new HPSocketSdk.OnError(error);
            OnServerShutdownCallback = new HPSocketSdk.OnServerShutdown(shutdown);

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

        public virtual void SetOnServerShutdownCallback(HPSocketSdk.OnServerShutdown shutdown)
        {
            OnServerShutdownCallback = new HPSocketSdk.OnServerShutdown(shutdown);
            HPSocketSdk.HP_Set_FN_Server_OnServerShutdown(pListener, OnServerShutdownCallback);
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

        public virtual void SetOnPrepareListenCallback(HPSocketSdk.OnPrepareListen prepareListen)
        {
            OnPrepareListenCallback = new HPSocketSdk.OnPrepareListen(prepareListen);
            HPSocketSdk.HP_Set_FN_Server_OnPrepareListen(pListener, OnPrepareListenCallback);
        }

        public virtual void SetOnAcceptCallback(HPSocketSdk.OnAccept accept)
        {
            OnAcceptCallback = new HPSocketSdk.OnAccept(accept);
            HPSocketSdk.HP_Set_FN_Server_OnAccept(pListener, OnAcceptCallback);
        }

        public virtual void SetOnSendCallback(HPSocketSdk.OnSend send)
        {
            OnSendCallback = new HPSocketSdk.OnSend(send);
            HPSocketSdk.HP_Set_FN_Server_OnSend(pListener, OnSendCallback);
        }


        ///////////////////////////////////////////////////////////////////////////////////////


        /// <summary>
        /// 监听事件
        /// </summary>
        /// <param name="soListen"></param>
        /// <returns></returns>
        protected virtual HandleResult OnPrepareListen(IntPtr soListen)
        {
            return HandleResult.Ok;
        }

        /// <summary>
        /// 客户进入
        /// </summary>
        /// <param name="dwConnId"></param>
        /// <param name="pClient"></param>
        /// <returns></returns>
        protected virtual HandleResult OnAccept(IntPtr dwConnId, IntPtr pClient)
        {
            return HandleResult.Ok;
        }

        /// <summary>
        /// 服务器发数据了
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
        /// 客户离开了
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
        /// 服务关闭了
        /// </summary>
        /// <returns></returns>
        protected virtual HandleResult OnServerShutdown()
        {
            return HandleResult.Ok;
        }

        /////////////////////////////////////////////////////////////////////////

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

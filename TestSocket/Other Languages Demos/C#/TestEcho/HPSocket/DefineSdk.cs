using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace HPSocket
{

    /// <summary>
    /// Unicode版本
    /// 以下函数名和参数名均复制与HPSocket4C.h,看不顺眼请自己加别名和修改参数名称
    /// </summary>
    public class HPSocketSdk
    {
        /// <summary>
        /// 通信组件服务状态,用程序可以通过通信组件的 GetState() 方法获取组件当前服务状态
        /// </summary>
        public enum En_HP_ServiceState
        {
            HP_SS_STARTING = 0,	// 正在启动
            HP_SS_STARTED = 1,	// 已经启动
            HP_SS_STOPING = 2,	// 正在停止
            HP_SS_STOPED = 3,	// 已经启动
        }

        /// <summary>
        /// Socket 操作类型,应用程序的 OnErrror() 事件中通过该参数标识是哪种操作导致的错误
        /// </summary>
        public enum En_HP_SocketOperation
        {
            HP_SO_UNKNOWN = 0,	// Unknown
            HP_SO_ACCEPT = 1,	// Acccept
            HP_SO_CONNECT = 2,	// Connnect
            HP_SO_SEND = 3,	// Send
            HP_SO_RECEIVE = 4,	// Receive
        };

        /// <summary>
        /// 事件通知处理结果,事件通知的返回值，不同的返回值会影响通信组件的后续行为
        /// </summary>
        public enum En_HP_HandleResult
        {
            HP_HR_OK = 0,	// 成功
            HP_HR_IGNORE = 1,	// 忽略
            HP_HR_ERROR = 2,	// 错误
        };


        /// <summary>
        /// 操作结果代码,Start() / Stop() 方法执行失败时，可通过 GetLastError() 获取错误代码
        /// </summary>
        public enum En_HP_ServerError
        {
            /// <summary>
            /// 成功
            /// </summary>
            HP_SE_OK = 0,

            // 下面代码提示如果要显示详细信息,请参考HP_SE_OK自己修改
            HP_SE_ILLEGAL_STATE = 1,	// 当前状态不允许操作
            HP_SE_INVALID_PARAM = 2,	// 非法参数
            HP_SE_SOCKET_CREATE = 3,	// 创建监听 SOCKET 失败
            HP_SE_SOCKET_BIND = 4,	// 绑定监听地址失败
            HP_SE_SOCKET_PREPARE = 5,	// 设置监听 SOCKET 失败
            HP_SE_SOCKET_LISTEN = 6,	// 启动监听失败
            HP_SE_CP_CREATE = 7,	// 创建完成端口失败
            HP_SE_WORKER_THREAD_CREATE = 8,	// 创建工作线程失败
            HP_SE_DETECT_THREAD_CREATE = 9,	// 创建监测线程失败
            HP_SE_SOCKE_ATTACH_TO_CP = 10,	// 监听 SOCKET 绑定到完成端口失败
        };

        /// <summary>
        /// 操作结果代码,Start() / Stop() 方法执行失败时，可通过 GetLastError() 获取错误代码
        /// </summary>
        public enum En_HP_ClientError
        {
            HP_CE_OK = 0,	// 成功
            HP_CE_ILLEGAL_STATE = 1,	// 当前状态不允许操作
            HP_CE_INVALID_PARAM = 2,	// 非法参数
            HP_CE_SOCKET_CREATE_FAIL = 3,	// 创建 Client Socket 失败
            HP_CE_SOCKET_PREPARE_FAIL = 4,	// 设置 Client Socket 失败
            HP_CE_CONNECT_SERVER_FAIL = 5,	// 连接服务器失败
            HP_CE_WORKER_CREATE_FAIL = 6,	// 创建工作线程失败
            HP_CE_DETECTOR_CREATE_FAIL = 7,	// 创建监测线程失败
            HP_CE_NETWORK_ERROR = 8,	// 网络错误
            HP_CE_DATA_PROC_ERROR = 9,	// 数据处理错误
        };

        /// <summary>
        /// 数据抓取结果,数据抓取操作的返回值
        /// </summary>
        public enum En_HP_FetchResult
        {
            HP_FR_OK = 0,	// 成功
            HP_FR_LENGTH_TOO_LONG = 1,	// 抓取长度过大
            HP_FR_DATA_NOT_FOUND = 2,	// 找不到 ConnID 对应的数据
        };


        /* 公共回调函数 */
        public delegate En_HP_HandleResult HP_FN_OnSend(uint dwConnID, IntPtr pData, int iLength);
        public delegate En_HP_HandleResult HP_FN_OnReceive(uint dwConnID, IntPtr pData, int iLength);
        public delegate En_HP_HandleResult HP_FN_OnPullReceive(uint dwConnID, int iLength);
        public delegate En_HP_HandleResult HP_FN_OnClose(uint dwConnID);
        public delegate En_HP_HandleResult HP_FN_OnError(uint dwConnID, En_HP_SocketOperation enOperation, int iErrorCode);

        /* 服务端回调函数 */
        public delegate En_HP_HandleResult HP_FN_OnPrepareListen(IntPtr soListen);

        /// <summary>
        /// OnAccept
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pClient">如果为 TCP 连接，pClient为 SOCKET 句柄；如果为 UDP 连接，pClient为 SOCKADDR_IN 指针；</param>
        /// <returns></returns>
        public delegate En_HP_HandleResult HP_FN_OnAccept(uint dwConnID, IntPtr pClient);
        public delegate En_HP_HandleResult HP_FN_OnServerShutdown();

        /* 客户端回调函数 */
        public delegate En_HP_HandleResult HP_FN_OnPrepareConnect(uint dwConnID, object socket);
        public delegate En_HP_HandleResult HP_FN_OnConnect(uint dwConnID);

        /// <summary>
        /// 创建 HP_TcpPullServerListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern IntPtr Create_HP_TcpPullServerListener();

        /// <summary>
        /// 创建 HP_TcpPullServer 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern IntPtr Create_HP_TcpPullServer(IntPtr pListener);

        /// <summary>
        /// 创建 HP_TcpServerListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern IntPtr Create_HP_TcpServerListener();

        /// <summary>
        /// 创建 HP_TcpServer 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern IntPtr Create_HP_TcpServer(IntPtr pListener);


        /// <summary>
        /// 名称：启动通信组件
        /// 描述：启动服务端通信组件，启动完成后可开始接收客户端连接并收发数据
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="pszBindAddress">监听地址</param>
        /// <param name="usPort">监听端口</param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport("HPSocket4C_U.dll", CharSet = CharSet.Unicode)]
        public static extern bool HP_Server_Start(IntPtr pServer, String pszBindAddress, ushort usPort);

        /// <summary>
        /// 关闭服务端通信组件，关闭完成后断开所有客户端连接并释放所有资源
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern bool HP_Server_Stop(IntPtr pServer);

        /// <summary>
        /// 用户通过该方法向指定客户端发送数据
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffer">发送数据长度</param>
        /// <param name="iLength">发送数据长度</param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern bool HP_Server_Send(IntPtr pServer, uint dwConnID, byte[] pBuffer, int iLength);

        /// <summary>
        /// 用户通过该方法向指定客户端发送数据
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffer">发送数据长度</param>
        /// <param name="iLength">发送数据长度</param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern bool HP_Server_Send(IntPtr pServer, uint dwConnID, IntPtr pBuffer, int iLength);

        /// <summary>
        /// 断开与某个客户端的连接
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="bForce">是否强制断开连接</param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern bool HP_Server_Disconnect(IntPtr pServer, uint dwConnID, bool bForce);

        /// <summary>
        /// 获取最近一次失败操作的错误代码
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern En_HP_ServerError HP_Server_GetLastError(IntPtr pServer);

        /// <summary>
        /// 获取最近一次失败操作的错误描述
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern IntPtr HP_Server_GetLastErrorDesc(IntPtr pServer);

        /// <summary>
        /// 获取某个客户端连接的地址信息
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID"></param>
        /// <param name="lpszAddress"></param>
        /// <param name="piAddressLen">传入传出值,大小最好在222.222.222.222的长度以上</param>
        /// <param name="pusPort"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern bool HP_Server_GetClientAddress(IntPtr pServer, uint dwConnID, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszAddress, ref int piAddressLen, ref ushort pusPort);

        /***************************** Server 回调函数设置方法 *****************************/
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Server_OnPrepareListen(IntPtr pListener, HP_FN_OnPrepareListen fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Server_OnAccept(IntPtr pListener, HP_FN_OnAccept fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Server_OnSend(IntPtr pListener, HP_FN_OnSend fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Server_OnReceive(IntPtr pListener, HP_FN_OnReceive fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Server_OnPullReceive(IntPtr pListener, HP_FN_OnPullReceive fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Server_OnClose(IntPtr pListener, HP_FN_OnClose fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Server_OnError(IntPtr pListener, HP_FN_OnError fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Server_OnServerShutdown(IntPtr pListener, HP_FN_OnServerShutdown fn);
        /**********************************************************************************/



        /// <summary>
        /// 创建 HP_TcpClientListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern IntPtr Create_HP_TcpClientListener();


        /// <summary>
        /// 创建 HP_TcpClient 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern IntPtr Create_HP_TcpClient(IntPtr pListener);


        /***************************** Client 回调函数设置方法 *****************************/
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Client_OnPrepareConnect(IntPtr pListener, HP_FN_OnPrepareConnect fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Client_OnConnect(IntPtr pListener, HP_FN_OnConnect fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Client_OnSend(IntPtr pListener, HP_FN_OnSend fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Client_OnReceive(IntPtr pListener, HP_FN_OnReceive fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Client_OnPullReceive(IntPtr pListener, HP_FN_OnPullReceive fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Client_OnClose(IntPtr pListener, HP_FN_OnClose fn);
        [DllImport("HPSocket4C_U.dll")]
        public static extern void HP_Set_FN_Client_OnError(IntPtr pListener, HP_FN_OnError fn);

        /**************************************************************************/


        /// <summary>
        /// 获取最近一次失败操作的错误代码
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern IntPtr HP_Client_GetLastError(IntPtr pClient);

        /// <summary>
        /// 获取最近一次失败操作的错误描述
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern IntPtr HP_Client_GetLastErrorDesc(IntPtr pClient);

        /// <summary>
        /// 启动客户端通信组件并连接服务端，启动完成后可开始收发数据
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="pszRemoteAddress">服务端地址</param>
        /// <param name="usPort">服务端端口</param>
        /// <param name="bAsyncConnect">是否采用异步 Connnect</param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport("HPSocket4C_U.dll", CharSet = CharSet.Unicode)]
        public static extern bool HP_Client_Start(IntPtr pClient, String pszRemoteAddress, ushort usPort, bool bAsyncConnect);

        /// <summary>
        /// 关闭客户端通信组件，关闭完成后断开与服务端的连接并释放所有资源
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern bool HP_Client_Stop(IntPtr pClient);

        /// <summary>
        /// 用户通过该方法向服务端发送数据
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwConnID">连接 ID（保留参数，目前该参数并未使用）</param>
        /// <param name="pBuffer">发送数据缓冲区</param>
        /// <param name="iLength">发送数据长度</param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern bool HP_Client_Send(IntPtr pClient, uint dwConnID, byte[] pBuffer, int iLength);


        /// <summary>
        /// 获取该组件对象的连接 ID
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern uint HP_Client_GetConnectionID(IntPtr pClient);

        /// <summary>
        /// 销毁 HP_TcpClient 对象
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern void Destroy_HP_TcpClient(IntPtr pClient);

        /// <summary>
        /// 销毁 HP_TcpClientListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern void Destroy_HP_TcpClientListener(IntPtr pListener);

        /// <summary>
        /// 销毁 HP_TcpServer 对象
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern void Destroy_HP_TcpServer(IntPtr pServer);

        /// <summary>
        /// 销毁 HP_TcpServerListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport("HPSocket4C_U.dll")]
        public static extern void Destroy_HP_TcpServerListener(IntPtr pListener);

    }
}

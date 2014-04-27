using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using HPSocketCS;

namespace HPSocketCS
{

    /// <summary>
    /// 通信组件服务状态,用程序可以通过通信组件的 GetState() 方法获取组件当前服务状态
    /// </summary>
    public enum ServiceState
    {
        Starting = 0,   // 正在启动
        Started = 1,    // 已经启动
        Stoping = 2,    // 正在停止
        Stoped = 3,     // 已经启动
    }

    /// <summary>
    /// Socket 操作类型,应用程序的 OnErrror() 事件中通过该参数标识是哪种操作导致的错误
    /// </summary>
    public enum SocketOperation
    {
        Unknown = 0,    // Unknown
        Acccept = 1,    // Acccept
        Connnect = 2,   // Connnect
        Send = 3,       // Send
        Receive = 4,    // Receive
    };

    /// <summary>
    /// 事件通知处理结果,事件通知的返回值，不同的返回值会影响通信组件的后续行为
    /// </summary>
    public enum HandleResult
    {
        Ok = 0,         // 成功
        Ignore = 1,     // 忽略
        Error = 2,      // 错误
    };


    /// <summary>
    /// 名称：操作结果代码
    /// 描述：组件 Start() / Stop() 方法执行失败时，可通过 GetLastError() 获取错误代码
    /// </summary>
    public enum SocketError
    {
        Ok = 0,                 // 成功
        IllegalState = 1,       // 当前状态不允许操作
        InvalidParam = 2,       // 非法参数
        SocketCreate = 3,       // 创建 SOCKET 失败
        SocketBind = 4,         // 绑定 SOCKET 失败
        SocketPrepare = 5,      // 设置 SOCKET 失败
        SocketListen = 6,       // 监听 SOCKET 失败
        CPCreate = 7,           // 创建完成端口失败
        WorkerThreadCreate = 8, // 创建工作线程失败
        DetectThreadCreate = 9, // 创建监测线程失败
        SocketAttachToCP = 10,  // 绑定完成端口失败
        ConnectServer = 11,     // 连接服务器失败
        Network = 12,           // 网络错误
        DataProc = 13,          // 数据处理错误
        DataSend = 14,          // 数据发送失败
    };

    /// <summary>
    /// 数据抓取结果,数据抓取操作的返回值
    /// </summary>
    public enum FetchResult
    {
        Ok = 0,             // 成功
        LengthTooLong = 1,  // 抓取长度过大
        DataNotFound = 2,   // 找不到 ConnID 对应的数据
    };


    /****************************************************/
    /************** sockaddr结构体,udp服务器时OnAccept最后个参数可转化 **************/
    [StructLayout(LayoutKind.Sequential)]
    public struct in_addr
    {
        public ulong S_addr;
    }

    //[StructLayout(LayoutKind.Sequential)]
    //public struct in_addr
    //{
    //    public byte s_b1, s_b2, s_b3, s_b4;
    //}

    [StructLayout(LayoutKind.Sequential)]
    public struct sockaddr_in
    {
        short sin_family;
        ushort sin_port;
        in_addr sin_addr;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public char[] sLibNamesin_zero;
    }

    /****************************************************/
}


namespace HPSocketCS.SDK
{

    /// <summary>
    /// Unicode版本
    /// </summary>
    public class HPSocketSdk
    {
        /// <summary>
        /// HPSocket的文件路径
        /// </summary>
        private const string SOCKET_DLL_PATH = "HPSocket4C_U.dll";

        /*****************************************************************************************************/
        /******************************************** 公共类、接口 ********************************************/
        /*****************************************************************************************************/



        /****************************************************/
        /************** HPSocket4C.dll 回调函数 **************/
        /* 公共回调函数 */
        public delegate HandleResult OnSend(uint dwConnID, IntPtr pData, int iLength);
        public delegate HandleResult OnReceive(uint dwConnID, IntPtr pData, int iLength);
        public delegate HandleResult OnPullReceive(uint dwConnID, int iLength);
        public delegate HandleResult OnClose(uint dwConnID);
        public delegate HandleResult OnError(uint dwConnID, SocketOperation enOperation, int iErrorCode);

        /* 服务端回调函数 */
        public delegate HandleResult OnPrepareListen(IntPtr soListen);

        /// <summary>
        /// OnAccept
        /// </summary>
        /// <param name="dwConnID"></param>
        /// <param name="pClient">如果为 TCP 连接，pClient为 SOCKET 句柄；如果为 UDP 连接，pClient为 sockaddr_in 指针；</param>
        /// <returns></returns>
        public delegate HandleResult OnAccept(uint dwConnID, IntPtr pClient);
        public delegate HandleResult OnServerShutdown();

        /* 客户端和 Agent 回调函数 */
        public delegate HandleResult OnPrepareConnect(uint dwConnID, uint socket);
        public delegate HandleResult OnConnect(uint dwConnID);

        /* Agent 回调函数 */
        public delegate HandleResult OnAgentShutdown();

        /****************************************************/
        /************** HPSocket4C.dll 导出函数 **************/

        /// <summary>
        /// 创建 TcpServer 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpServer(IntPtr pListener);

        /// <summary>
        /// 创建 TcpClient 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpClient(IntPtr pListener);

        /// <summary>
        /// 创建 TcpAgent 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpAgent(IntPtr pListener);

        /// <summary>
        /// 创建 TcpPullServer 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpPullServer(IntPtr pListener);

        /// <summary>
        /// 创建 TcpPullClient 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpPullClient(IntPtr pListener);

        /// <summary>
        /// 创建 TcpPullAgent 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpPullAgent(IntPtr pListener);

        /// <summary>
        /// 创建 UdpServer 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_UdpServer(IntPtr pListener);

        /// <summary>
        /// 创建 UdpClient 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_UdpClient(IntPtr pListener);


        /// <summary>
        /// 销毁 TcpServer 对象
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpServer(IntPtr pServer);

        /// <summary>
        /// 销毁 TcpClient 对象
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpClient(IntPtr pClient);

        /// <summary>
        /// 销毁 TcpAgent 对象
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpAgent(IntPtr pAgent);

        /// <summary>
        /// 销毁 TcpPullServer 对象
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpPullServer(IntPtr pServer);

        /// <summary>
        /// 销毁 TcpPullClient 对象
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpPullClient(IntPtr pClient);

        /// <summary>
        /// 销毁 TcpPullAgent 对象
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpPullAgent(IntPtr pAgent);

        /// <summary>
        /// 销毁 UdpServer 对象
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_UdpServer(IntPtr pServer);

        /// <summary>
        /// 销毁 UdpClient 对象
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_UdpClient(IntPtr pClient);


        /// <summary>
        /// 创建 TcpServerListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpServerListener();

        /// <summary>
        /// 创建 TcpClientListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpClientListener();

        /// <summary>
        /// 创建 TcpAgentListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpAgentListener();

        /// <summary>
        /// 创建 TcpPullServerListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpPullServerListener();

        /// <summary>
        /// 创建 TcpPullClientListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpPullClientListener();

        /// <summary>
        /// 创建 TcpPullAgentListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_TcpPullAgentListener();

        /// <summary>
        /// 创建 UdpServerListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_UdpServerListener();

        /// <summary>
        /// 创建 UdpClientListener 对象
        /// </summary>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr Create_HP_UdpClientListener();


        /// <summary>
        /// 销毁 TcpServerListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpServerListener(IntPtr pListener);

        /// <summary>
        /// 销毁 TcpClientListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpClientListener(IntPtr pListener);

        /// <summary>
        /// 销毁 TcpAgentListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpAgentListener(IntPtr pListener);


        /// <summary>
        /// 销毁 TcpPullServerListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpPullServerListener(IntPtr pListener);

        /// <summary>
        /// 销毁 TcpPullClientListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpPullClientListener(IntPtr pListener);

        /// <summary>
        /// 销毁 TcpPullAgentListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_TcpPullAgentListener(IntPtr pListener);

        /// <summary>
        /// 销毁 UdpServerListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_UdpServerListener(IntPtr pListener);

        /// <summary>
        /// 销毁 UdpClientListener 对象
        /// </summary>
        /// <param name="pListener"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void Destroy_HP_UdpClientListener(IntPtr pListener);


        /**********************************************************************************/
        /***************************** Server 回调函数设置方法 *****************************/

        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Server_OnPrepareListen(IntPtr pListener, OnPrepareListen fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Server_OnAccept(IntPtr pListener, OnAccept fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Server_OnSend(IntPtr pListener, OnSend fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Server_OnReceive(IntPtr pListener, OnReceive fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Server_OnPullReceive(IntPtr pListener, OnPullReceive fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Server_OnClose(IntPtr pListener, OnClose fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Server_OnError(IntPtr pListener, OnError fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Server_OnServerShutdown(IntPtr pListener, OnServerShutdown fn);

        /**********************************************************************************/
        /***************************** Client 回调函数设置方法 *****************************/

        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Client_OnPrepareConnect(IntPtr pListener, OnPrepareConnect fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Client_OnConnect(IntPtr pListener, OnConnect fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Client_OnSend(IntPtr pListener, OnSend fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Client_OnReceive(IntPtr pListener, OnReceive fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Client_OnPullReceive(IntPtr pListener, OnPullReceive fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Client_OnClose(IntPtr pListener, OnClose fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Client_OnError(IntPtr pListener, OnError fn);

        /**********************************************************************************/
        /****************************** Agent 回调函数设置方法 *****************************/

        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Agent_OnPrepareConnect(IntPtr pListener, OnPrepareConnect fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Agent_OnConnect(IntPtr pListener, OnConnect fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Agent_OnSend(IntPtr pListener, OnSend fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Agent_OnReceive(IntPtr pListener, OnReceive fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Agent_OnPullReceive(IntPtr pListener, OnPullReceive fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Agent_OnClose(IntPtr pListener, OnClose fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Agent_OnError(IntPtr pListener, OnError fn);
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Set_FN_Agent_OnAgentShutdown(IntPtr pListener, OnAgentShutdown fn);

        /**************************************************************************/
        /***************************** Server 操作方法 *****************************/

        /// <summary>
        /// 名称：启动通信组件
        /// 描述：启动服务端通信组件，启动完成后可开始接收客户端连接并收发数据
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="pszBindAddress">监听地址</param>
        /// <param name="usPort">监听端口</param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport(SOCKET_DLL_PATH, CharSet = CharSet.Unicode)]
        public static extern bool HP_Server_Start(IntPtr pServer, String pszBindAddress, ushort usPort);

        /// <summary>
        /// 关闭服务端通信组件，关闭完成后断开所有客户端连接并释放所有资源
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_Stop(IntPtr pServer);

        /// <summary>
        /// 用户通过该方法向指定客户端发送数据
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffer">发送数据长度</param>
        /// <param name="iLength">发送数据长度</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool HP_Server_Send(IntPtr pServer, uint dwConnID, byte[] pBuffer, int iLength);

        /// <summary>
        /// 用户通过该方法向指定客户端发送数据
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffer">发送数据长度</param>
        /// <param name="iLength">发送数据长度</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, SetLastError = true)]
        public static extern bool HP_Server_Send(IntPtr pServer, uint dwConnID, IntPtr pBuffer, int iLength);

        /// <summary>
        /// 用户通过该方法向指定客户端发送数据
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID"></param>
        /// <param name="pBuffer"></param>
        /// <param name="iLength"></param>
        /// <param name="iOffset">针对pBuffer的偏移</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool HP_Server_SendPart(IntPtr pServer, uint dwConnID, byte[] pBuffer, int iLength, int iOffset);

        /// <summary>
        /// 用户通过该方法向指定客户端发送数据
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID"></param>
        /// <param name="pBuffer"></param>
        /// <param name="iLength"></param>
        /// <param name="iOffset">针对pBuffer的偏移</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, SetLastError = true)]
        public static extern bool HP_Server_SendPart(IntPtr pServer, uint dwConnID, IntPtr pBuffer, int iLength, int iOffset);


        /// <summary>
        /// 断开与某个客户端的连接
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="bForce">是否强制断开连接</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_Disconnect(IntPtr pServer, uint dwConnID, bool bForce);

        /// <summary>
        /// 断开超过指定时长的连接
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwPeriod">时长（毫秒）</param>
        /// <param name="bForce">是否强制断开连接</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_DisconnectLongConnections(IntPtr pServer, uint dwPeriod, bool bForce);

        /******************************************************************************/
        /***************************** Server 属性访问方法 *****************************/

        /// <summary>
        /// 设置连接的附加数据
        /// 是否为连接绑定附加数据或者绑定什么样的数据，均由应用程序只身决定
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pExtra"></param>
        /// <returns>若返回 false 失败则为（无效的连接 ID）</returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_SetConnectionExtra(IntPtr pServer, uint dwConnID, IntPtr pExtra);

        /// <summary>
        /// 获取连接的附加数据
        /// 是否为连接绑定附加数据或者绑定什么样的数据，均由应用程序只身决定
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pExtra">数据指针</param>
        /// <returns>若返回 false 失败则为（无效的连接 ID）</returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_GetConnectionExtra(IntPtr pServer, uint dwConnID, ref IntPtr pExtra);

        /// <summary>
        /// 检查通信组件是否已启动
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_HasStarted(IntPtr pServer);

        /// <summary>
        /// 查看通信组件当前状态
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern ServiceState HP_Server_GetState(IntPtr pServer);

        /// <summary>
        /// 获取最近一次失败操作的错误代码
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern SocketError HP_Server_GetLastError(IntPtr pServer);

        /// <summary>
        /// 获取最近一次失败操作的错误描述
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr HP_Server_GetLastErrorDesc(IntPtr pServer);


        /// <summary>
        /// 获取连接中未发出数据的长度
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID"></param>
        /// <param name="piPending"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_GetPendingDataLength(IntPtr pServer, uint dwConnID, ref int piPending);

        /// <summary>
        /// 获取客户端连接数
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Server_GetConnectionCount(IntPtr pServer);

        /// <summary>
        /// 获取某个客户端连接时长（毫秒）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID"></param>
        /// <param name="pdwPeriod"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_GetConnectPeriod(IntPtr pServer, uint dwConnID, ref uint pdwPeriod);

        /// <summary>
        /// 获取监听 Socket 的地址信息
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="lpszAddress"></param>
        /// <param name="piAddressLen"></param>
        /// <param name="pusPort"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_GetListenAddress(IntPtr pServer, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszAddress, ref int piAddressLen, ref ushort pusPort);

        /// <summary>
        /// 获取某个客户端连接的地址信息
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID"></param>
        /// <param name="lpszAddress"></param>
        /// <param name="piAddressLen">传入传出值,大小最好在222.222.222.222的长度以上</param>
        /// <param name="pusPort"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Server_GetRemoteAddress(IntPtr pServer, uint dwConnID, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszAddress, ref int piAddressLen, ref ushort pusPort);


        /// <summary>
        /// 设置 Socket 缓存对象锁定时间（毫秒，在锁定期间该 Socket 缓存对象不能被获取使用）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwFreeSocketObjLockTime"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Server_SetFreeSocketObjLockTime(IntPtr pServer, uint dwFreeSocketObjLockTime);

        /// <summary>
        /// 设置 Socket 缓存池大小（通常设置为平均并发连接数量的 1/3 - 1/2）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwFreeSocketObjPool"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Server_SetFreeSocketObjPool(IntPtr pServer, uint dwFreeSocketObjPool);

        /// <summary>
        /// 设置内存块缓存池大小（通常设置为 Socket 缓存池大小的 2 - 3 倍）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwFreeBufferObjPool"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Server_SetFreeBufferObjPool(IntPtr pServer, uint dwFreeBufferObjPool);

        /// <summary>
        /// 设置 Socket 缓存池回收阀值（通常设置为 Socket 缓存池大小的 3 倍）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwFreeSocketObjHold"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Server_SetFreeSocketObjHold(IntPtr pServer, uint dwFreeSocketObjHold);

        /// <summary>
        /// 设置内存块缓存池回收阀值（通常设置为内存块缓存池大小的 3 倍）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwFreeBufferObjHold"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Server_SetFreeBufferObjHold(IntPtr pServer, uint dwFreeBufferObjHold);

        /// <summary>
        /// 设置工作线程数量（通常设置为 2 * CPU + 2）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwWorkerThreadCount"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Server_SetWorkerThreadCount(IntPtr pServer, uint dwWorkerThreadCount);

        /// <summary>
        /// 设置关闭服务前等待连接关闭的最长时限（毫秒，0 则不等待）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwMaxShutdownWaitTime"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Server_SetMaxShutdownWaitTime(IntPtr pServer, uint dwMaxShutdownWaitTime);


        /// <summary>
        /// 获取 Socket 缓存对象锁定时间
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Server_GetFreeSocketObjLockTime(IntPtr pServer);

        /// <summary>
        /// 获取 Socket 缓存池大小
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Server_GetFreeSocketObjPool(IntPtr pServer);

        /// <summary>
        /// 获取内存块缓存池大小
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Server_GetFreeBufferObjPool(IntPtr pServer);

        /// <summary>
        /// 获取 Socket 缓存池回收阀值
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Server_GetFreeSocketObjHold(IntPtr pServer);

        /// <summary>
        /// 获取内存块缓存池回收阀值
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Server_GetFreeBufferObjHold(IntPtr pServer);

        /// <summary>
        /// 获取工作线程数量
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Server_GetWorkerThreadCount(IntPtr pServer);

        /// <summary>
        /// 获取关闭服务前等待连接关闭的最长时限
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Server_GetMaxShutdownWaitTime(IntPtr pServer);

        /**********************************************************************************/
        /***************************** TCP Server 属性访问方法 *****************************/

        /// <summary>
        /// 设置 Accept 预投递 Socket 数量（通常设置为工作线程数的 1 - 2 倍）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwAcceptSocketCount"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpServer_SetAcceptSocketCount(IntPtr pServer, uint dwAcceptSocketCount);

        /// <summary>
        /// 设置通信数据缓冲区大小（根据平均通信数据包大小调整设置，通常设置为 1024 的倍数）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwSocketBufferSize"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpServer_SetSocketBufferSize(IntPtr pServer, uint dwSocketBufferSize);

        /// <summary>
        /// 设置监听 Socket 的等候队列大小（根据并发连接数量调整设置）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwSocketListenQueue"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpServer_SetSocketListenQueue(IntPtr pServer, uint dwSocketListenQueue);

        /// <summary>
        /// 设置心跳包间隔（毫秒，0 则不发送心跳包）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwKeepAliveTime"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpServer_SetKeepAliveTime(IntPtr pServer, uint dwKeepAliveTime);

        /// <summary>
        /// 设置心跳确认包检测间隔（毫秒，0 不发送心跳包，如果超过若干次 [默认：WinXP 5 次, Win7 10 次] 检测不到心跳确认包则认为已断线）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwKeepAliveInterval"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpServer_SetKeepAliveInterval(IntPtr pServer, uint dwKeepAliveInterval);


        /// <summary>
        /// 获取 Accept 预投递 Socket 数量
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpServer_GetAcceptSocketCount(IntPtr pServer);

        /// <summary>
        /// 获取通信数据缓冲区大小
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpServer_GetSocketBufferSize(IntPtr pServer);

        /// <summary>
        /// 获取监听 Socket 的等候队列大小
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpServer_GetSocketListenQueue(IntPtr pServer);

        /// <summary>
        /// 获取心跳检查次数
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpServer_GetKeepAliveTime(IntPtr pServer);

        /// <summary>
        /// 获取心跳检查间隔
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpServer_GetKeepAliveInterval(IntPtr pServer);


        /**********************************************************************************/
        /***************************** UDP Server 属性访问方法 *****************************/

        /// <summary>
        /// 设置数据报文最大长度（建议在局域网环境下不超过 1472 字节，在广域网环境下不超过 548 字节）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwMaxDatagramSize"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_UdpServer_SetMaxDatagramSize(IntPtr pServer, uint dwMaxDatagramSize);

        /// <summary>
        /// 获取数据报文最大长度
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_UdpServer_GetMaxDatagramSize(IntPtr pServer);

        /// <summary>
        /// 设置监测包尝试次数（0 则不发送监测跳包，如果超过最大尝试次数则认为已断线）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwMaxDatagramSize"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_UdpServer_SetDetectAttempts(IntPtr pServer, uint dwMaxDatagramSize);

        /// <summary>
        /// 设置监测包发送间隔（秒，0 不发送监测包）
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwMaxDatagramSize"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_UdpServer_SetDetectInterval(IntPtr pServer, uint dwMaxDatagramSize);

        /// <summary>
        /// 获取心跳检查次数
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_UdpServer_GetDetectAttempts(IntPtr pServer);

        /// <summary>
        /// 获取心跳检查间隔
        /// </summary>
        /// <param name="pServer"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_UdpServer_GetDetectInterval(IntPtr pServer);
        /******************************************************************************/
        /***************************** Client 组件操作方法 *****************************/

        /// <summary>
        /// 启动客户端通信组件并连接服务端，启动完成后可开始收发数据
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="pszRemoteAddress">服务端地址</param>
        /// <param name="usPort">服务端端口</param>
        /// <param name="bAsyncConnect">是否采用异步 Connnect</param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport(SOCKET_DLL_PATH, CharSet = CharSet.Unicode)]
        public static extern bool HP_Client_Start(IntPtr pClient, string pszRemoteAddress, ushort usPort, bool bAsyncConnect);

        /// <summary>
        /// 关闭客户端通信组件，关闭完成后断开与服务端的连接并释放所有资源
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Client_Stop(IntPtr pClient);

        /// <summary>
        /// 用户通过该方法向服务端发送数据
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwConnID">连接 ID（保留参数，目前该参数并未使用）</param>
        /// <param name="pBuffer">发送数据缓冲区</param>
        /// <param name="iLength">发送数据长度</param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport("HPSocket4C_U.dll", CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool HP_Client_Send(IntPtr pClient, byte[] pBuffer, int iLength);

        /// <summary>
        /// 用户通过该方法向服务端发送数据
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwConnID">连接 ID（保留参数，目前该参数并未使用）</param>
        /// <param name="pBuffer">发送数据缓冲区</param>
        /// <param name="iLength">发送数据长度</param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport(SOCKET_DLL_PATH, SetLastError = true)]
        public static extern bool HP_Client_Send(IntPtr pClient, IntPtr pBuffer, int iLength);

        /// <summary>
        /// 用户通过该方法向服务端发送数据
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="pBuffer"></param>
        /// <param name="iLength"></param>
        /// <param name="iOffset">针对pBuffer的偏移</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool HP_Client_SendPart(IntPtr pClient, byte[] pBuffer, int iLength, int iOffset);

        /// <summary>
        /// 用户通过该方法向服务端发送数据
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="pBuffer"></param>
        /// <param name="iLength"></param>
        /// <param name="iOffset">针对pBuffer的偏移</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, SetLastError = true)]
        public static extern bool HP_Client_SendPart(IntPtr pClient, IntPtr pBuffer, int iLength, int iOffset);

        /******************************************************************************/
        /***************************** Client 属性访问方法 *****************************/

        /// <summary>
        /// 检查通信组件是否已启动
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Client_HasStarted(IntPtr pClient);

        /// <summary>
        /// 查看通信组件当前状态
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern ServiceState HP_Client_GetState(IntPtr pClient);

        /// <summary>
        /// 获取最近一次失败操作的错误代码
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern SocketError HP_Client_GetLastError(IntPtr pClient);

        /// <summary>
        /// 获取最近一次失败操作的错误描述
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr HP_Client_GetLastErrorDesc(IntPtr pClient);

        /// <summary>
        /// 获取该组件对象的连接 ID
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Client_GetConnectionID(IntPtr pClient);

        /// <summary>
        /// 获取 Client Socket 的地址信息
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="lpszAddress"></param>
        /// <param name="piAddressLen"></param>
        /// <param name="pusPort"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Client_GetLocalAddress(IntPtr pClient, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszAddress, ref int piAddressLen, ref ushort pusPort);

        /// <summary>
        /// 获取连接中未发出数据的长度
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="piPending"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Client_GetPendingDataLength(IntPtr pClient, ref int piPending);

        /// <summary>
        /// 设置内存块缓存池大小（通常设置为 -> PUSH 模型：5 - 10；PULL 模型：10 - 20 ）
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwFreeBufferPoolSize"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Client_SetFreeBufferPoolSize(IntPtr pClient, uint dwFreeBufferPoolSize);

        /// <summary>
        /// 设置内存块缓存池回收阀值（通常设置为内存块缓存池大小的 3 倍）
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwFreeBufferPoolHold"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Client_SetFreeBufferPoolHold(IntPtr pClient, uint dwFreeBufferPoolHold);

        /// <summary>
        /// 获取内存块缓存池大小
        /// </summary>
        /// <param name="pClient"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Client_GetFreeBufferPoolSize(IntPtr pClient);

        /// <summary>
        /// 获取内存块缓存池回收阀值
        /// </summary>
        /// <param name="pClient"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Client_GetFreeBufferPoolHold(IntPtr pClient);

        /**********************************************************************************/
        /***************************** TCP Client 属性访问方法 *****************************/

        /// <summary>
        /// 设置通信数据缓冲区大小（根据平均通信数据包大小调整设置，通常设置为：(N * 1024) - sizeof(TBufferObj)）
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwSocketBufferSize"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpClient_SetSocketBufferSize(IntPtr pClient, uint dwSocketBufferSize);

        /// <summary>
        /// 设置心跳包间隔（毫秒，0 则不发送心跳包）
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwKeepAliveTime"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpClient_SetKeepAliveTime(IntPtr pClient, uint dwKeepAliveTime);

        /// <summary>
        /// 设置心跳确认包检测间隔（毫秒，0 不发送心跳包，如果超过若干次 [默认：WinXP 5 次, Win7 10 次] 检测不到心跳确认包则认为已断线）
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwKeepAliveInterval"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpClient_SetKeepAliveInterval(IntPtr pClient, uint dwKeepAliveInterval);

        /// <summary>
        /// 获取通信数据缓冲区大小
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpClient_GetSocketBufferSize(IntPtr pClient);

        /// <summary>
        /// 获取心跳检查次数
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpClient_GetKeepAliveTime(IntPtr pClient);

        /// <summary>
        /// 获取心跳检查间隔
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpClient_GetKeepAliveInterval(IntPtr pClient);

        /**********************************************************************************/
        /***************************** UDP Client 属性访问方法 *****************************/

        /// <summary>
        /// 设置数据报文最大长度（建议在局域网环境下不超过 1472 字节，在广域网环境下不超过 548 字节）
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwMaxDatagramSize"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_UdpClient_SetMaxDatagramSize(IntPtr pClient, uint dwMaxDatagramSize);

        /// <summary>
        /// 获取数据报文最大长度
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_UdpClient_GetMaxDatagramSize(IntPtr pClient);

        /// <summary>
        /// 设置监测包尝试次数（0 则不发送监测跳包，如果超过最大尝试次数则认为已断线
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwDetectAttempts"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_UdpClient_SetDetectAttempts(IntPtr pClient, uint dwDetectAttempts);

        /// <summary>
        /// 设置监测包发送间隔（秒，0 不发送监测包）
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwDetectInterval"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_UdpClient_SetDetectInterval(IntPtr pClient, uint dwDetectInterval);

        /// <summary>
        /// 获取心跳检查次数
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_UdpClient_GetDetectAttempts(IntPtr pClient);

        /// <summary>
        /// 获取心跳检查间隔
        /// </summary>
        /// <param name="pClient"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_UdpClient_GetDetectInterval(IntPtr pClient);

        /**************************************************************************/
        /***************************** Agent 操作方法 *****************************/

        /// <summary>
        /// 启动通信组件
        /// 启动通信代理组件，启动完成后可开始连接远程服务器
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="pszBindAddress">监听地址</param>
        /// <param name="bAsyncConnect">是否采用异步 Connect</param>
        /// <returns>失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport(SOCKET_DLL_PATH, CharSet = CharSet.Unicode)]
        public static extern bool HP_Agent_Start(IntPtr pAgent, String pszBindAddress, bool bAsyncConnect);

        /// <summary>
        /// 关闭通信组件
        /// 关闭通信组件，关闭完成后断开所有连接并释放所有资源
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns>-- 失败，可通过 GetLastError() 获取错误代码</returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_Stop(IntPtr pAgent);

        /// <summary>
        /// 连接服务器
        /// 连接服务器，连接成功后 IAgentListener 会接收到 OnConnect() 事件
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="pszBindAddress">服务端地址</param>
        /// <param name="usPort">服务端端口</param>
        /// <param name="pdwConnID">传出连接 ID</param>
        /// <returns>失败，可通过 SYS_GetLastError() 获取 Windows 错误代码</returns>
        [DllImport(SOCKET_DLL_PATH, CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern bool HP_Agent_Connect(IntPtr pAgent, String pszBindAddress, ushort usPort, ref uint pdwConnID);

        /// <summary>
        /// 发送数据
        /// 用户通过该方法向指定连接发送数据
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffer">发送数据缓冲区</param>
        /// <param name="iLength">发送数据长度</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, CharSet = CharSet.Ansi, SetLastError = true)]
        public static extern bool HP_Agent_Send(IntPtr pAgent, uint dwConnID, byte[] pBuffer, int iLength);

        /// <summary>
        /// 发送数据
        /// 用户通过该方法向指定连接发送数据
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffer">发送数据缓冲区</param>
        /// <param name="iLength">发送数据长度</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, SetLastError = true)]
        public static extern bool HP_Agent_Send(IntPtr pAgent, uint dwConnID, IntPtr pBuffer, int iLength);

        /// <summary>
        /// 发送数据
        /// 用户通过该方法向指定连接发送数据
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID"></param>
        /// <param name="pBuffer"></param>
        /// <param name="iLength"></param>
        /// <param name="iOffset">针对pBuffer的偏移</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, SetLastError = true)]
        public static extern bool HP_Agent_SendPart(IntPtr pAgent, uint dwConnID, byte[] pBuffer, int iLength, int iOffset);

        /// <summary>
        /// 发送数据
        /// 用户通过该方法向指定连接发送数据
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID"></param>
        /// <param name="pBuffer"></param>
        /// <param name="iLength"></param>
        /// <param name="iOffset">针对pBuffer的偏移</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH, SetLastError = true)]
        public static extern bool HP_Agent_SendPart(IntPtr pAgent, uint dwConnID, IntPtr pBuffer, int iLength, int iOffset);

        /// <summary>
        /// 断开某个连接
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="bForce">是否强制断开连接</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_Disconnect(IntPtr pAgent, uint dwConnID, bool bForce);

        /// <summary>
        /// 断开超过指定时长的连接
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwPeriod">时长（毫秒）</param>
        /// <param name="bForce">是否强制断开连接</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_DisconnectLongConnections(IntPtr pAgent, uint dwPeriod, bool bForce);

        /******************************************************************************/
        /***************************** Agent 属性访问方法 *****************************/

        /// <summary>
        /// 设置连接的附加数据
        /// 是否为连接绑定附加数据或者绑定什么样的数据，均由应用程序只身决定
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pExtra">数据</param>
        /// <returns>FALSE    -- 失败（无效的连接 ID）</returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_SetConnectionExtra(IntPtr pAgent, uint dwConnID, IntPtr pExtra);

        /// <summary>
        /// 获取连接的附加数据
        /// 是否为连接绑定附加数据或者绑定什么样的数据，均由应用程序只身决定
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID"></param>
        /// <param name="pExtra"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_GetConnectionExtra(IntPtr pAgent, uint dwConnID, ref IntPtr pExtra);

        /// <summary>
        /// 检查通信组件是否已启动
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_HasStarted(IntPtr pAgent);

        /// <summary>
        /// 查看通信组件当前状态
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern ServiceState HP_Agent_GetState(IntPtr pAgent);

        /// <summary>
        /// 获取连接数
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Agent_GetConnectionCount(IntPtr pAgent);

        /// <summary>
        /// 获取某个连接时长（毫秒）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID"></param>
        /// <param name="pdwPeriod"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_GetConnectPeriod(IntPtr pAgent, uint dwConnID, ref uint pdwPeriod);

        /// <summary>
        /// 获取监听 Socket 的地址信息
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID"></param>
        /// <param name="lpszAddress"></param>
        /// <param name="piAddressLen"></param>
        /// <param name="pusPort"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_GetLocalAddress(IntPtr pAgent, uint dwConnID, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszAddress, ref int piAddressLen, ref ushort pusPort);

        /// <summary>
        /// 获取某个连接的地址信息
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID"></param>
        /// <param name="lpszAddress"></param>
        /// <param name="piAddressLen"></param>
        /// <param name="pusPort"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_GetRemoteAddress(IntPtr pAgent, uint dwConnID, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpszAddress, ref int piAddressLen, ref ushort pusPort);

        /// <summary>
        /// 获取最近一次失败操作的错误代码
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern SocketError HP_Agent_GetLastError(IntPtr pAgent);

        /// <summary>
        /// 获取最近一次失败操作的错误描述
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr HP_Agent_GetLastErrorDesc(IntPtr pAgent);

        /// <summary>
        /// 获取连接中未发出数据的长度
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID"></param>
        /// <param name="piPending"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_Agent_GetPendingDataLength(IntPtr pAgent, uint dwConnID, ref int piPending);

        /// <summary>
        /// 设置 Socket 缓存对象锁定时间（毫秒，在锁定期间该 Socket 缓存对象不能被获取使用）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwFreeSocketObjLockTime"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Agent_SetFreeSocketObjLockTime(IntPtr pAgent, uint dwFreeSocketObjLockTime);

        /// <summary>
        /// 设置 Socket 缓存池大小（通常设置为平均并发连接数量的 1/3 - 1/2）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwFreeSocketObjPool"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Agent_SetFreeSocketObjPool(IntPtr pAgent, uint dwFreeSocketObjPool);

        /// <summary>
        /// 设置内存块缓存池大小（通常设置为 Socket 缓存池大小的 2 - 3 倍）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwFreeBufferObjPool"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Agent_SetFreeBufferObjPool(IntPtr pAgent, uint dwFreeBufferObjPool);

        /// <summary>
        /// 设置 Socket 缓存池回收阀值（通常设置为 Socket 缓存池大小的 3 倍）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwFreeSocketObjHold"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Agent_SetFreeSocketObjHold(IntPtr pAgent, uint dwFreeSocketObjHold);

        /// <summary>
        /// 设置内存块缓存池回收阀值（通常设置为内存块缓存池大小的 3 倍）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwFreeBufferObjHold"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Agent_SetFreeBufferObjHold(IntPtr pAgent, uint dwFreeBufferObjHold);

        /// <summary>
        /// 设置工作线程数量（通常设置为 2 * CPU + 2）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwWorkerThreadCount"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Agent_SetWorkerThreadCount(IntPtr pAgent, uint dwWorkerThreadCount);

        /// <summary>
        /// 设置关闭组件前等待连接关闭的最长时限（毫秒，0 则不等待）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwMaxShutdownWaitTime"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_Agent_SetMaxShutdownWaitTime(IntPtr pAgent, uint dwMaxShutdownWaitTime);

        /// <summary>
        /// 获取 Socket 缓存对象锁定时间
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Agent_GetFreeSocketObjLockTime(IntPtr pAgent);

        /// <summary>
        /// 获取 Socket 缓存池大小
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Agent_GetFreeSocketObjPool(IntPtr pAgent);

        /// <summary>
        /// 获取内存块缓存池大小
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Agent_GetFreeBufferObjPool(IntPtr pAgent);

        /// <summary>
        /// 获取 Socket 缓存池回收阀值
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Agent_GetFreeSocketObjHold(IntPtr pAgent);

        /// <summary>
        /// 获取内存块缓存池回收阀值
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Agent_GetFreeBufferObjHold(IntPtr pAgent);

        /// <summary>
        /// 获取工作线程数量
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Agent_GetWorkerThreadCount(IntPtr pAgent);

        /// <summary>
        /// 获取关闭组件前等待连接关闭的最长时限
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_Agent_GetMaxShutdownWaitTime(IntPtr pAgent);

        /**********************************************************************************/
        /***************************** TCP Agent 属性访问方法 *****************************/

        /// <summary>
        /// 置是否启用地址重用机制（默认：不启用）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="bReuseAddress"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpAgent_SetReuseAddress(IntPtr pAgent, bool bReuseAddress);

        /// <summary>
        /// 检测是否启用地址重用机制
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern bool HP_TcpAgent_IsReuseAddress(IntPtr pAgent);

        /// <summary>
        /// 设置通信数据缓冲区大小（根据平均通信数据包大小调整设置，通常设置为 1024 的倍数）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwSocketBufferSize"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpAgent_SetSocketBufferSize(IntPtr pAgent, uint dwSocketBufferSize);

        /// <summary>
        /// 设置心跳包间隔（毫秒，0 则不发送心跳包）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwKeepAliveTime"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpAgent_SetKeepAliveTime(IntPtr pAgent, uint dwKeepAliveTime);

        /// <summary>
        /// 设置心跳确认包检测间隔（毫秒，0 不发送心跳包，如果超过若干次 [默认：WinXP 5 次, Win7 10 次] 检测不到心跳确认包则认为已断线）
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwKeepAliveInterval"></param>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern void HP_TcpAgent_SetKeepAliveInterval(IntPtr pAgent, uint dwKeepAliveInterval);

        /// <summary>
        /// 获取通信数据缓冲区大小
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpAgent_GetSocketBufferSize(IntPtr pAgent);

        /// <summary>
        /// 获取心跳检查次数
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpAgent_GetKeepAliveTime(IntPtr pAgent);

        /// <summary>
        /// 获取心跳检查间隔
        /// </summary>
        /// <param name="pAgent"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern uint HP_TcpAgent_GetKeepAliveInterval(IntPtr pAgent);

        /***************************************************************************************/
        /***************************** TCP Pull Server 组件操作方法 *****************************/

        /// <summary>
        /// 抓取数据
        /// 用户通过该方法从 Socket 组件中抓取数据
        /// </summary>
        /// <param name="pServer"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffer">数据抓取缓冲区</param>
        /// <param name="iLength">抓取数据长度</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern FetchResult HP_TcpPullServer_Fetch(IntPtr pServer, uint dwConnID, IntPtr pBuffer, int iLength);

        /***************************************************************************************/
        /***************************** TCP Pull Server 属性访问方法 *****************************/

        /***************************************************************************************/
        /***************************** TCP Pull Client 组件操作方法 *****************************/

        /// <summary>
        /// 抓取数据
        /// 用户通过该方法从 Socket 组件中抓取数据
        /// </summary>
        /// <param name="pClient"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffer">数据抓取缓冲区</param>
        /// <param name="iLength">抓取数据长度</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern FetchResult HP_TcpPullClient_Fetch(IntPtr pClient, uint dwConnID, IntPtr pBuffer, int iLength);

        /***************************************************************************************/
        /***************************** TCP Pull Client 属性访问方法 *****************************/

        /***************************************************************************************/
        /***************************** TCP Pull Agent 组件操作方法 *****************************/

        /// <summary>
        /// 抓取数据
        /// 用户通过该方法从 Socket 组件中抓取数据
        /// </summary>
        /// <param name="pAgent"></param>
        /// <param name="dwConnID">连接 ID</param>
        /// <param name="pBuffer">数据抓取缓冲区</param>
        /// <param name="iLength">抓取数据长度</param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern FetchResult HP_TcpPullAgent_Fetch(IntPtr pAgent, uint dwConnID, IntPtr pBuffer, int iLength);

        /***************************************************************************************/
        /***************************** TCP Pull Agent 属性访问方法 *****************************/

        /***************************************************************************************/
        /*************************************** 其它方法 ***************************************/

        /// <summary>
        /// 获取错误描述文本
        /// </summary>
        /// <param name="enCode"></param>
        /// <returns></returns>
        [DllImport(SOCKET_DLL_PATH)]
        public static extern IntPtr HP_GetSocketErrorDesc(SocketError enCode);

        /// <summary>
        /// 调用系统的 ::GetLastError() 方法获取系统错误代码
        /// </summary>
        /// <returns></returns>
        public static int SYS_GetLastError()
        {
            return Marshal.GetLastWin32Error();
        }
    }
}

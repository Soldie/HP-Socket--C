unit HPSocketSDKUnit;

interface

uses
    Winapi.Windows;

type
    PInteger = ^Integer;
    PUShort = ^USHORT;

    // 应用程序状态
    EnAppState = (ST_STARTING, ST_STARTED, ST_STOPING, ST_STOPED);

    { *****************************************************************************************************/
      /******************************************** 公共类、接口 ********************************************/
      /*****************************************************************************************************/

      /************************************************************************
      名称：通信组件服务状态
      描述：应用程序可以通过通信组件的 GetState() 方法获取组件当前服务状态
      ************************************************************************ }
    En_HP_ServiceState = (HP_SS_STARTING = 0, // 正在启动
      HP_SS_STARTED = 1, // 已经启动
      HP_SS_STOPING = 2, // 正在停止
      HP_SS_STOPED = 3 // 已经启动
      );

    { ************************************************************************
      名称：Socket 操作类型
      描述：应用程序的 OnErrror() 事件中通过该参数标识是哪种操作导致的错误
      ************************************************************************ }
    En_HP_SocketOperation = (HP_SO_UNKNOWN = 0, // Unknown
      HP_SO_ACCEPT = 1, // Acccept
      HP_SO_CONNECT = 2, // Connnect
      HP_SO_SEND = 3, // Send
      HP_SO_RECEIVE = 4 // Receive
      );

    { ************************************************************************
      名称：事件通知处理结果
      描述：事件通知的返回值，不同的返回值会影响通信组件的后续行为
      ************************************************************************ }
    En_HP_HandleResult = (HP_HR_OK = 0, // 成功
      HP_HR_IGNORE = 1, // 忽略
      HP_HR_ERROR = 2 // 错误
      );

    { ************************************************************************
      名称：操作结果代码
      描述：Start() / Stop() 方法执行失败时，可通过 GetLastError() 获取错误代码
      ************************************************************************ }
    En_HP_SocketError = (HP_SE_OK = 0, // 成功
      HP_SE_ILLEGAL_STATE = 1, // 当前状态不允许操作
      HP_SE_INVALID_PARAM = 2, // 非法参数
      HP_SE_SOCKET_CREATE = 3, // 创建 SOCKET 失败
      HP_SE_SOCKET_BIND = 4, // 绑定 SOCKET 失败
      HP_SE_SOCKET_PREPARE = 5, // 设置 SOCKET 失败
      HP_SE_SOCKET_LISTEN = 6, // 监听 SOCKET 失败
      HP_SE_CP_CREATE = 7, // 创建完成端口失败
      HP_SE_WORKER_THREAD_CREATE = 8, // 创建工作线程失败
      HP_SE_DETECT_THREAD_CREATE = 9, // 创建监测线程失败
      HP_SE_SOCKE_ATTACH_TO_CP = 10, // 绑定完成端口失败
      HP_SE_CONNECT_SERVER = 11, // 连接服务器失败
      HP_SE_NETWORK = 12, // 网络错误
      HP_SE_DATA_PROC = 13, // 数据处理错误
      HP_SE_DATA_SEND = 14 // 数据发送失败
      );

    { ************************************************************************
      名称：数据抓取结果
      描述：数据抓取操作的返回值
      ************************************************************************ }
    En_HP_FetchResult = (HP_FR_OK = 0, // 成功
      HP_FR_LENGTH_TOO_LONG = 1, // 抓取长度过大
      HP_FR_DATA_NOT_FOUND = 2 // 找不到 ConnID 对应的数据
      );

    { **************************************************** }

    // 公共回调函数
    HP_FN_OnSend = function(dwConnID: DWORD; const pData: Pointer; iLength: Integer): En_HP_HandleResult; stdcall;
    HP_FN_OnReceive = function(dwConnID: DWORD; const pData: Pointer; iLength: Integer): En_HP_HandleResult; stdcall;
    HP_FN_OnClose = function(dwConnID: DWORD): En_HP_HandleResult; stdcall;
    HP_FN_OnError = function(dwConnID: DWORD; enOperation: En_HP_SocketOperation; iErrorCode: Integer): En_HP_HandleResult; stdcall;

    // 服务端回调函数
    HP_FN_OnPrepareListen = function(soListen: Pointer): En_HP_HandleResult; stdcall;
    // 如果为 TCP 连接，pClient为 SOCKET 句柄；如果为 UDP 连接，pClient为 SOCKADDR_IN 指针；
    HP_FN_OnAccept = function(dwConnID: DWORD; pClient: Pointer): En_HP_HandleResult; stdcall;
    HP_FN_OnServerShutdown = function(): En_HP_HandleResult; stdcall;

    // 客户端回调函数
    HP_FN_OnPrepareConnect = function(dwConnID: DWORD; socket: Pointer): En_HP_HandleResult; stdcall;
    HP_FN_OnConnect = function(dwConnID: DWORD): En_HP_HandleResult; stdcall;

const
    HPSocketDLL = 'HPSocket4C_U.dll';

    // 创建 HP_TcpPullServerListener 对象
function Create_HP_TcpServerListener(): Pointer; stdcall; external HPSocketDLL;

// 创建 HP_TcpServer 对象
function Create_HP_TcpServer(pListener: Pointer): Pointer; stdcall; external HPSocketDLL;

{ /*
  * 名称：启动通信组件
  * 描述：启动服务端通信组件，启动完成后可开始接收客户端连接并收发数据
  *
  * 参数：		pszBindAddress	-- 监听地址
  *			usPort			-- 监听端口
  * 返回值：	TRUE	-- 成功
  *			FALSE	-- 失败，可通过 GetLastError() 获取错误代码
  */ }
function HP_Server_Start(pServer: Pointer; pszBindAddress: PWideChar; usPort: USHORT): Boolean; stdcall; external HPSocketDLL;

{ /*
  * 名称：关闭通信组件
  * 描述：关闭服务端通信组件，关闭完成后断开所有客户端连接并释放所有资源
  *
  * 参数：
  * 返回值：	TRUE	-- 成功
  *			FALSE	-- 失败，可通过 GetLastError() 获取错误代码
  */ }
function HP_Server_Stop(pServer: Pointer): Boolean; stdcall; external HPSocketDLL;

{ /*
  * 名称：断开连接
  * 描述：断开与某个客户端的连接
  *
  * 参数：		dwConnID	-- 连接 ID
  *			bForce		-- 是否强制断开连接
  * 返回值：	TRUE	-- 成功
  *			FALSE	-- 失败
  */ }
function HP_Server_Disconnect(pServer: Pointer; dwConnID: DWORD; bForce: Boolean): Boolean; stdcall; external HPSocketDLL;

// 获取某个客户端连接的地址信息
function HP_Server_GetRemoteAddress(pServer: Pointer; dwConnID: DWORD; lpszAddress: PWideChar; piAddressLen: PInteger; pusPort: PUShort): Boolean; stdcall;
  external HPSocketDLL;

// 获取最近一次失败操作的错误代码
function HP_Server_GetLastError(pServer: Pointer): Integer; stdcall; external HPSocketDLL;

// 获取最近一次失败操作的错误描述
function HP_Server_GetLastErrorDesc(pServer: Pointer): PWideChar; stdcall; external HPSocketDLL;

// 销毁 HP_TcpServer 对象
procedure Destroy_HP_TcpServer(pServer: Pointer); stdcall; external HPSocketDLL;

// 销毁 HP_TcpClient 对象
procedure Destroy_HP_TcpClient(pClient: Pointer); stdcall; external HPSocketDLL;

// 销毁 HP_TcpServerListener 对象
procedure Destroy_HP_TcpServerListener(pServer: Pointer); stdcall; external HPSocketDLL;

// 销毁 HP_TcpClientListener 对象
procedure Destroy_HP_TcpClientListener(pClient: Pointer); stdcall; external HPSocketDLL;

{ ***************************** Server 回调函数设置方法 ***************************** }

procedure HP_Set_FN_Server_OnPrepareListen(pListener: Pointer; fn: HP_FN_OnPrepareListen); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Server_OnAccept(pListener: Pointer; fn: HP_FN_OnAccept); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Server_OnSend(pListener: Pointer; fn: HP_FN_OnSend); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Server_OnReceive(pListener: Pointer; fn: HP_FN_OnReceive); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Server_OnClose(pListener: Pointer; fn: HP_FN_OnClose); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Server_OnError(pListener: Pointer; fn: HP_FN_OnError); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Server_OnServerShutdown(pListener: Pointer; fn: HP_FN_OnServerShutdown); stdcall; external HPSocketDLL;

{ ********************************************************************************** }

{ ***************************** Client 组件操作方法 *****************************/

  /*
  * 名称：启动通信组件
  * 描述：启动客户端通信组件并连接服务端，启动完成后可开始收发数据
  *
  * 参数：		pszRemoteAddress	-- 服务端地址
  *			usPort				-- 服务端端口
  *			bAsyncConnect		-- 是否采用异步 Connnect
  * 返回值：	TRUE	-- 成功
  *			FALSE	-- 失败，可通过 GetLastError() 获取错误代码
  * }
function HP_Client_Start(pClient: Pointer; pszRemoteAddress: PWideChar; usPort: USHORT; bAsyncConnect: Boolean): Boolean; stdcall; external HPSocketDLL;

{ /*
  * 名称：关闭通信组件
  * 描述：关闭客户端通信组件，关闭完成后断开与服务端的连接并释放所有资源
  *
  * 参数：
  * 返回值：	TRUE	-- 成功
  *			FALSE	-- 失败，可通过 GetLastError() 获取错误代码
  */ }
function HP_Client_Stop(pClient: Pointer): Boolean; stdcall; external HPSocketDLL;

{ /*
  * 名称：发送数据
  * 描述：用户通过该方法向服务端发送数据
  *
  * 参数：		dwConnID	-- 连接 ID（保留参数，目前该参数并未使用）
  *			pBuffer		-- 发送数据缓冲区
  *			iLength		-- 发送数据长度
  * 返回值：	TRUE	-- 成功
  *			FALSE	-- 失败，可通过 GetLastError() 获取错误代码
  */ }
function HP_Client_Send(pClient: Pointer; const pBuffer: Pointer; iLength: Integer): Boolean; stdcall; external HPSocketDLL;


// 获取最近一次失败操作的错误代码
function HP_Client_GetLastError(pServer: Pointer): Integer; stdcall; external HPSocketDLL;

// 获取最近一次失败操作的错误描述
function HP_Client_GetLastErrorDesc(pServer: Pointer): PWideChar; stdcall; external HPSocketDLL;

// 创建 HP_TcpClientListener 对象
function Create_HP_TcpClientListener(): Pointer; stdcall; external HPSocketDLL;

// 创建 HP_TcpClient 对象
function Create_HP_TcpClient(pListener: Pointer): Pointer; stdcall; external HPSocketDLL;

// 获取该组件对象的连接 ID
function HP_Client_GetConnectionID(pClient: Pointer): DWORD; stdcall; external HPSocketDLL;

{ /***************************** Client 回调函数设置方法 *****************************/ }
procedure HP_Set_FN_Client_OnPrepareConnect(pListener: Pointer; fn: HP_FN_OnPrepareConnect); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Client_OnConnect(pListener: Pointer; fn: HP_FN_OnConnect); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Client_OnSend(pListener: Pointer; fn: HP_FN_OnSend); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Client_OnReceive(pListener: Pointer; fn: HP_FN_OnReceive); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Client_OnClose(pListener: Pointer; fn: HP_FN_OnClose); stdcall; external HPSocketDLL;
procedure HP_Set_FN_Client_OnError(pListener: Pointer; fn: HP_FN_OnError); stdcall; external HPSocketDLL;
{ /**************************************************************************/ }

implementation

end.

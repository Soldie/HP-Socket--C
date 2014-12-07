
// ServerDlg.cpp : implementation file
//

#include "stdafx.h"
#include "Server.h"
#include "ServerDlg.h"
#include "afxdialogex.h"
#include "../../../../Common/Src/WaitFor.h"

#ifdef _WIN64
	#ifdef _DEBUG
		#pragma comment(lib, "../../../Bin/HPSocket/x64/HPSocket_UD.lib")
	#else
		#pragma comment(lib, "../../../Bin/HPSocket/x64/HPSocket_U.lib")
	#endif
#else
	#ifdef _DEBUG
		#pragma comment(lib, "../../../Bin/HPSocket/x86/HPSocket_UD.lib")
	#else
		#pragma comment(lib, "../../../Bin/HPSocket/x86/HPSocket_U.lib")
	#endif
#endif

// CServerDlg dialog

#define DEFAULT_ADDRESS	_T("0.0.0.0")
#define DEFAULT_PORT	_T("5555")

#ifdef _DEBUG
	#define DEF_SHOW_LOG	BST_CHECKED
#else
	#define DEF_SHOW_LOG	BST_UNCHECKED
#endif

#define DEF_HTTP_PORT	80
#define DEF_HTTPS_PORT	443
#define HTTPS_FLAG		"CONNECT"
#define HTTP_SCHEME		"http://"
#define HTTP_1_1		"HTTP/1.1"
#define HEAD_CONN		"Connection:"
#define HEAD_PROXY_CONN	"Proxy-Connection:"
#define HTTPS_RESP		"HTTP/1.1 200 Connection Established\r\n\r\n"
#define IE_OPT_DIALOG	"rundll32.exe shell32.dll, Control_RunDLL inetcpl.cpl, , 4"

static int HTTP_SCHEME_LEN		= (int)strlen(HTTP_SCHEME);
static int HEAD_CONN_LEN		= (int)strlen(HEAD_CONN);
static int HEAD_PROXY_CONN_LEN	= (int)strlen(HEAD_PROXY_CONN);
static int HTTPS_RESP_LEN		= (int)strlen(HTTPS_RESP);

CServerDlg::CServerDlg(CWnd* pParent /*=nullptr*/)
	: CDialogEx(CServerDlg::IDD, pParent)
	, m_pServerListener(new CTcpServerListenerImpl(this))
	, m_pAgentListener(new CTcpAgentListenerImpl(this))
	, m_Server(m_pServerListener)
	, m_Agent(m_pAgentListener)
{
	m_hIcon = AfxGetApp()->LoadIcon(IDR_MAINFRAME);
}

void CServerDlg::DoDataExchange(CDataExchange* pDX)
{
	CDialogEx::DoDataExchange(pDX);
	DDX_Control(pDX, IDC_INFO, m_Info);
	DDX_Control(pDX, IDC_START, m_Start);
	DDX_Control(pDX, IDC_STOP, m_Stop);
	DDX_Control(pDX, IDC_PORT, m_Port);
	DDX_Control(pDX, IDC_OPTIONS, m_Options);
	DDX_Control(pDX, IDC_SHOWLOG, m_ShowLog);
}

BEGIN_MESSAGE_MAP(CServerDlg, CDialogEx)
	ON_WM_PAINT()
	ON_WM_QUERYDRAGICON()
	ON_BN_CLICKED(IDC_START, &CServerDlg::OnBnClickedStart)
	ON_BN_CLICKED(IDC_STOP, &CServerDlg::OnBnClickedStop)
	ON_MESSAGE(USER_INFO_MSG, OnUserInfoMsg)
	ON_WM_VKEYTOITEM()
	ON_WM_CLOSE()
	ON_BN_CLICKED(IDC_OPTIONS, &CServerDlg::OnBnClickedOptions)
END_MESSAGE_MAP()


// CServerDlg message handlers

BOOL CServerDlg::OnInitDialog()
{
	CDialogEx::OnInitDialog();

	// Set the icon for this dialog.  The framework does this automatically
	//  when the application's main window is not a dialog
	SetIcon(m_hIcon, TRUE);			// Set big icon
	SetIcon(m_hIcon, FALSE);		// Set small icon

	// TODO: Add extra initialization here

	CString strTitle;
	CString strOriginTitle;
	m_Port.SetWindowText(DEFAULT_PORT);
	m_ShowLog.SetCheck(DEF_SHOW_LOG);

	::SetMainWnd(this);
	::SetInfoList(&m_Info);
	SetAppState(ST_STOPPED);

	return TRUE;  // return TRUE  unless you set the focus to a control
}

void CServerDlg::OnClose()
{
	/*
	if(m_Server->GetState() != SS_STOPED)
	{
		this->MessageBox(_T("stop IOCP Server first, pls !"), _T("forbiddden"));
		return;
	}
	*/

	::SetMainWnd(nullptr);
	__super::OnClose();

	if(m_Server->HasStarted())
		m_Server->Stop();

	if(m_Agent->HasStarted())
		m_Agent->Stop();
}

// If you add a minimize button to your dialog, you will need the code below
//  to draw the icon.  For MFC applications using the document/view model,
//  this is automatically done for you by the framework.

void CServerDlg::OnPaint()
{
	if (IsIconic())
	{
		CPaintDC dc(this); // device context for painting

		SendMessage(WM_ICONERASEBKGND, reinterpret_cast<WPARAM>(dc.GetSafeHdc()), 0);

		// Center icon in client rectangle
		int cxIcon = GetSystemMetrics(SM_CXICON);
		int cyIcon = GetSystemMetrics(SM_CYICON);
		CRect rect;
		GetClientRect(&rect);
		int x = (rect.Width() - cxIcon + 1) / 2;
		int y = (rect.Height() - cyIcon + 1) / 2;

		// Draw the icon
		dc.DrawIcon(x, y, m_hIcon);
	}
	else
	{
		CDialogEx::OnPaint();
	}
}

// The system calls this function to obtain the cursor to display while the user drags
//  the minimized window.
HCURSOR CServerDlg::OnQueryDragIcon()
{
	return static_cast<HCURSOR>(m_hIcon);
}

BOOL CServerDlg::PreTranslateMessage(MSG* pMsg)
{
	if (
			pMsg->message == WM_KEYDOWN		
			&&(	pMsg->wParam == VK_ESCAPE	 
			||	pMsg->wParam == VK_CANCEL	
			||	pMsg->wParam == VK_RETURN	
		))
		return TRUE;

	return CDialog::PreTranslateMessage(pMsg);
}

void CServerDlg::SetAppState(EnAppState state)
{
	if(m_enState == state)
		return;

	m_enState = state;

	if(this->GetSafeHwnd() == nullptr)
		return;

	m_Start.EnableWindow(m_enState == ST_STOPPED);
	m_Stop.EnableWindow(m_enState == ST_STARTED);
	m_Port.EnableWindow(m_enState == ST_STOPPED);
	//m_Options.EnableWindow(m_enState == ST_STOPPED);
	m_ShowLog.EnableWindow(m_enState == ST_STOPPED);
}

void CServerDlg::OnBnClickedStart()
{
	CString strPort;
	m_Port.GetWindowText(strPort);
	USHORT usPort = (USHORT)_ttoi(strPort);

	if(usPort == 0)
	{
		MessageBox(_T("Listen Port invalid, pls check!"), _T("Params Error"), MB_OK);
		m_Port.SetFocus();
		return;
	}

	m_bLog = (m_ShowLog.GetCheck() == BST_CHECKED);

	SetAppState(ST_STARTING);

	//m_Server->SetFreeSocketObjPool(500);
	//m_Server->SetFreeSocketObjHold(1500);
	//m_Server->SetFreeBufferObjPool(2000);
	//m_Server->SetFreeBufferObjHold(6000);
	//m_Server->SetAcceptSocketCount(50);

	m_Server->SetSendPolicy(SP_DIRECT);
	m_Agent->SetSendPolicy(SP_DIRECT);
	m_Server->SetRecvPolicy(RP_PARALLEL);
	m_Agent->SetRecvPolicy(RP_PARALLEL);

	if(m_Server->Start(DEFAULT_ADDRESS, usPort))
	{
		VERIFY(m_Agent->Start(nullptr, FALSE));

		::LogServerStart(DEFAULT_ADDRESS, usPort);
		SetAppState(ST_STARTED);
	}
	else
	{
		::LogServerStartFail(m_Server->GetLastError(), m_Server->GetLastErrorDesc());
		SetAppState(ST_STOPPED);
	}
}

void CServerDlg::OnBnClickedStop()
{
	SetAppState(ST_STOPPING);

	VERIFY(m_Server->Stop());
	VERIFY(m_Agent->Stop());

	::LogServerStop();
	SetAppState(ST_STOPPED);
}

void CServerDlg::OnBnClickedOptions()
{
	::WinExec(IE_OPT_DIALOG, 0);
}

int CServerDlg::OnVKeyToItem(UINT nKey, CListBox* pListBox, UINT nIndex)
{
	if(nKey == 'C')
		pListBox->ResetContent();

	return __super::OnVKeyToItem(nKey, pListBox, nIndex);
}

LRESULT CServerDlg::OnUserInfoMsg(WPARAM wp, LPARAM lp)
{
	info_msg* msg = (info_msg*)wp;

	::LogInfoMsg(msg);

	return 0;
}

EnHandleResult CTcpServerListenerImpl::OnPrepareListen(SOCKET soListen)
{
	TCHAR szAddress[40];
	int iAddressLen = sizeof(szAddress) / sizeof(TCHAR);
	USHORT usPort;

	m_pDlg->m_Server->GetListenAddress(szAddress, iAddressLen, usPort);
	::PostOnPrepareListen(szAddress, usPort);

	return HR_OK;
}

EnHandleResult CTcpServerListenerImpl::OnAccept(CONNID dwConnID, SOCKET soClient)
{
	//if(m_pDlg->m_bLog) ::PostOnAccept2(dwConnID);

	return HR_OK;
}

EnHandleResult CTcpServerListenerImpl::OnSend(CONNID dwConnID, const BYTE* pData, int iLength)
{
	//if(m_pDlg->m_bLog) ::PostOnSend(dwConnID, pData, iLength);

	return HR_OK;
}

EnHandleResult CTcpServerListenerImpl::OnReceive(CONNID dwConnID, const BYTE* pData, int iLength)
{
	//if(m_pDlg->m_bLog) ::PostOnReceive(dwConnID, pData, iLength);

	CONNID dwAgentID = 0;

	if(!m_pDlg->m_Server->GetConnectionExtra(dwConnID, (PVOID*)&dwAgentID))
		return HR_IGNORE;

	EnHandleResult rs	= HR_ERROR;
	BYTE* pData2		= (BYTE*)pData;
	int iLength2		= iLength;

	BOOL isHttps;
	CString strAddr;
	USHORT usPort;

	if(dwAgentID == 0)
	{
		if(ParseRequestHeader(pData2, iLength2, isHttps, strAddr, usPort))
		{
			if(m_pDlg->m_Agent->Connect(strAddr, usPort, &dwAgentID))
			{
				m_pDlg->m_Agent->SetConnectionExtra(dwAgentID, (PVOID)dwConnID);
				m_pDlg->m_Server->SetConnectionExtra(dwConnID, (PVOID)dwAgentID);

				if(isHttps)	
				{
					if(m_pDlg->m_Server->Send(dwConnID, (BYTE*)HTTPS_RESP, HTTPS_RESP_LEN))
						rs = HR_OK;
				}
				else
				{
					if(m_pDlg->m_Agent->Send(dwAgentID, pData2, iLength2))
						rs = HR_OK;
				}
			}
		}
	}
	else
	{
		BOOL isOK = TRUE;

		if(CheckIfHttp(pData2, iLength2))
			isOK = ParseRequestHeader(pData2, iLength2, isHttps, strAddr, usPort);

		if(isOK && m_pDlg->m_Agent->Send(dwAgentID, pData2, iLength2))
			rs = HR_OK;
	}

	return rs;
}

EnHandleResult CTcpServerListenerImpl::OnClose(CONNID dwConnID)
{
	//if(m_pDlg->m_bLog) ::PostOnClose(dwConnID);

	DetachConnInfo(dwConnID);

	return HR_OK;
}

EnHandleResult CTcpServerListenerImpl::OnError(CONNID dwConnID, EnSocketOperation enOperation, int iErrorCode)
{
	//if(m_pDlg->m_bLog) ::PostOnError(dwConnID, enOperation, iErrorCode);

	DetachConnInfo(dwConnID);

	return HR_OK;
}

EnHandleResult CTcpServerListenerImpl::OnShutdown()
{
	return HR_OK;
}

void CTcpServerListenerImpl::DetachConnInfo(CONNID dwConnID)
{
	CONNID dwAgentID = 0;
	BOOL bExist		 = m_pDlg->m_Server->GetConnectionExtra(dwConnID, (PVOID*)&dwAgentID);

	if(bExist && dwAgentID != 0)
	{
		m_pDlg->m_Agent->SetConnectionExtra(dwAgentID, (PVOID)0);
		m_pDlg->m_Agent->Disconnect(dwAgentID);
	}
}

BOOL CTcpServerListenerImpl::ParseRequestHeader(BYTE*& pData, int& iLength, BOOL& isHttps, CString& strAddr, USHORT& usPort)
{
	usPort	= 0;
	isHttps	= FALSE;
	strAddr.Empty();

	BYTE* pTemp	= pData;
	int i		= 0;

	for(; i < iLength; ++i, ++pTemp)
	{
		if(*pTemp == '\r')
			break;
	}

	if(i == iLength)
		return FALSE;

	CStringA strContent;

	int iBufferLen	= (int)(pTemp - pData);
	LPSTR lpstr		= strContent.GetBufferSetLength(iBufferLen);
	memcpy(lpstr, pData, iBufferLen);

	int iPos		= 0;
	int iPos1		= 0;
	int iSchemePos	= 0;
	BOOL bHttp11	= FALSE;

	CStringA strHost;
	CStringA strToken = strContent.Tokenize(" ", iPos);

	for(int i = 0; !strToken.IsEmpty() && i < 3; i++)
	{
		if(i == 0)
		{
			if(strToken.CompareNoCase(HTTPS_FLAG) == 0)
				isHttps = TRUE;
			else
				iPos1 = iPos;

			strToken = strContent.Tokenize(" ", iPos);
		}
		else if(i == 1)
		{
			if(isHttps)
				strHost = strToken;
			else
			{
				if(strToken.Find(HTTP_SCHEME) != 0)
					return FALSE;

				int iPos2	= 0;
				iSchemePos	= iPos1;

				strToken.Replace(HTTP_SCHEME, "");
				strHost		= strToken.Tokenize("/", iPos2);
			}

			strToken = strContent.Tokenize(" ", iPos);
		}
		else if(i == 2)
		{
			if(strToken.CompareNoCase(HTTP_1_1) == 0)
				bHttp11 = TRUE;
		}
	}

	if(strHost.IsEmpty())
		return FALSE;

	iPos = 0;
	strToken = strHost.Tokenize(":", iPos);

	for(int i = 0; !strToken.IsEmpty() && i < 2; i++)
	{
		if(i == 0)
		{
			strAddr = strToken;
			strToken = strHost.Tokenize(":", iPos);
		}
		else if(i == 1)
		{
			usPort = atoi(strToken);
		}
	}

	if(usPort == 0)
		usPort = isHttps ? DEF_HTTPS_PORT : DEF_HTTP_PORT;

	if(!isHttps && bHttp11)
	{
		int iMoveLen = strHost.GetLength() + HTTP_SCHEME_LEN;
		memcpy(pData + iMoveLen, pData, iSchemePos);

		pData	+= iMoveLen;
		iLength	-= iMoveLen;
	}

	++pTemp;
	BYTE* pHead	= pTemp;
	int iRange	= iLength - (int)(pHead - pData);

	for(i = 0; i < iRange; ++i, ++pTemp)
	{
		if(*pTemp == '\r')
		{
			if(_strnicmp((char*)pHead, HEAD_PROXY_CONN, HEAD_PROXY_CONN_LEN) == 0)
			{
				int iMoveLen = HEAD_PROXY_CONN_LEN - HEAD_CONN_LEN;
				memcpy(pHead + iMoveLen, HEAD_CONN, HEAD_CONN_LEN);

				int iCopyLen = (int)(pHead - pData);
				memcpy(pData + iMoveLen, pData, iCopyLen);

				pData	+= iMoveLen;
				iLength	-= iMoveLen;
			
				break;
			}
			else if(*(pTemp - 1) == '\r' || *(pTemp - 2) == '\r')
				break;
		}
		else if(*pTemp == '\n')
		{
			++pTemp;
			pHead = pTemp;
		}
	}

	return TRUE;
}

BOOL CTcpServerListenerImpl::CheckIfHttp(const BYTE* pData, int iLength)
{
	BYTE c = *pData;

	if(c < 'A' || (c > 'Z' && c < 'a') || c > 'z')
		return FALSE;

	LPCSTR lpszData = (LPCSTR)pData;

	if(_strnicmp(lpszData, "GET "		, 4) == 0)
		return TRUE;
	if(_strnicmp(lpszData, "POST "		, 5) == 0)
		return TRUE;
	if(_strnicmp(lpszData, "HEAD "		, 5) == 0)
		return TRUE;
	if(_strnicmp(lpszData, "PUT "		, 4) == 0)
		return TRUE;
	if(_strnicmp(lpszData, "DELETE "	, 7) == 0)
		return TRUE;
	if(_strnicmp(lpszData, "TRACE "		, 6) == 0)
		return TRUE;
	if(_strnicmp(lpszData, "OPTIONS "	, 8) == 0)
		return TRUE;

	return FALSE;
}

EnHandleResult CTcpAgentListenerImpl::OnConnect(CONNID dwConnID)
{
	if(m_pDlg->m_bLog)
	{
		TCHAR szAddress[40];
		int iAddressLen = sizeof(szAddress) / sizeof(TCHAR);
		USHORT usPort;

		m_pDlg->m_Agent->GetRemoteAddress(dwConnID, szAddress, iAddressLen, usPort);

		::PostOnConnect2(dwConnID, szAddress, usPort);
	}

	return HR_OK;
}

EnHandleResult CTcpAgentListenerImpl::OnSend(CONNID dwConnID, const BYTE* pData, int iLength)
{
	if(m_pDlg->m_bLog) ::PostOnSend(dwConnID, pData, iLength);

	return HR_OK;
}

EnHandleResult CTcpAgentListenerImpl::OnReceive(CONNID dwConnID, const BYTE* pData, int iLength)
{
	if(m_pDlg->m_bLog) ::PostOnReceive(dwConnID, pData, iLength);

	CONNID dwServerID	= 0;
	BOOL bExist			= TRUE;
	
	while(TRUE)
	{
		bExist = m_pDlg->m_Agent->GetConnectionExtra(dwConnID, (PVOID*)&dwServerID);

		if(bExist && dwServerID == 0)
			::WaitWithMessageLoop(10);
		else
			break;
	}

	if(!bExist)
		return HR_IGNORE;
	if(!m_pDlg->m_Server->Send(dwServerID, pData, iLength))
		return HR_ERROR;

	return HR_OK;
}

EnHandleResult CTcpAgentListenerImpl::OnClose(CONNID dwConnID)
{
	if(m_pDlg->m_bLog) ::PostOnClose(dwConnID);

	DetachConnInfo(dwConnID);

	return HR_OK;
}

EnHandleResult CTcpAgentListenerImpl::OnError(CONNID dwConnID, EnSocketOperation enOperation, int iErrorCode)
{
	if(m_pDlg->m_bLog) ::PostOnError(dwConnID, enOperation, iErrorCode);

	DetachConnInfo(dwConnID);

	return HR_OK;
}

EnHandleResult CTcpAgentListenerImpl::OnShutdown()
{
	::PostOnShutdown();

	return HR_OK;
}

void CTcpAgentListenerImpl::DetachConnInfo(CONNID dwConnID)
{
	CONNID dwServerID	= 0;
	BOOL bExist			= m_pDlg->m_Agent->GetConnectionExtra(dwConnID, (PVOID*)&dwServerID);

	if(bExist && dwServerID != 0)
	{
		m_pDlg->m_Server->SetConnectionExtra(dwServerID, (PVOID)0);
		m_pDlg->m_Server->Disconnect(dwServerID);
	}
}

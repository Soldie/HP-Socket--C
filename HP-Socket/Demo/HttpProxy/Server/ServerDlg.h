
// ServerDlg.h : header file
//

#pragma once
#include "afxwin.h"

#include "../../../Src/HPSocket.h"
#include "../../Global/helper.h"

class CServerDlg;

class CTcpServerListenerImpl : public CTcpServerListener
{
public:
	CTcpServerListenerImpl(CServerDlg* pDlg) : m_pDlg(pDlg)
	{

	}

private:
	virtual EnHandleResult OnPrepareListen(ITcpServer* pSender, SOCKET soListen);
	virtual EnHandleResult OnSend(ITcpServer* pSender, CONNID dwConnID, const BYTE* pData, int iLength);
	virtual EnHandleResult OnReceive(ITcpServer* pSender, CONNID dwConnID, const BYTE* pData, int iLength);
	virtual EnHandleResult OnClose(ITcpServer* pSender, CONNID dwConnID, EnSocketOperation enOperation, int iErrorCode);
	virtual EnHandleResult OnAccept(ITcpServer* pSender, CONNID dwConnID, SOCKET soClient);
	virtual EnHandleResult OnShutdown(ITcpServer* pSender);

private:
	void DetachConnInfo(CONNID dwConnID);
	BOOL ParseRequestHeader(BYTE*& pData, int& iLength, BOOL& isHttps, CString& strAddr, USHORT& usPort);
	BOOL CheckIfHttp(const BYTE* pData, int iLength);

public:
	CServerDlg* m_pDlg;
};

class CTcpAgentListenerImpl : public CTcpAgentListener
{
public:
	CTcpAgentListenerImpl(CServerDlg* pDlg) : m_pDlg(pDlg)
	{

	}

private:
	virtual EnHandleResult OnSend(ITcpAgent* pSender, CONNID dwConnID, const BYTE* pData, int iLength);
	virtual EnHandleResult OnReceive(ITcpAgent* pSender, CONNID dwConnID, const BYTE* pData, int iLength);
	virtual EnHandleResult OnClose(ITcpAgent* pSender, CONNID dwConnID, EnSocketOperation enOperation, int iErrorCode);
	virtual EnHandleResult OnConnect(ITcpAgent* pSender, CONNID dwConnID);
	virtual EnHandleResult OnShutdown(ITcpAgent* pSender);

private:
	void DetachConnInfo(CONNID dwConnID);

public:
	CServerDlg* m_pDlg;
};

// CServerDlg dialog
class CServerDlg : public CDialogEx
{
// Construction
public:
	CServerDlg(CWnd* pParent = nullptr);	// standard constructor

// Dialog Data
	enum { IDD = IDD_SERVER_DIALOG };

protected:
	virtual void DoDataExchange(CDataExchange* pDX);	// DDX/DDV support
	virtual BOOL PreTranslateMessage(MSG* pMsg);

// Implementation
protected:
	HICON m_hIcon;

	// Generated message map functions
	virtual BOOL OnInitDialog();
	afx_msg void OnPaint();
	afx_msg HCURSOR OnQueryDragIcon();
	afx_msg void OnBnClickedStart();
	afx_msg void OnBnClickedStop();
	afx_msg LRESULT CServerDlg::OnUserInfoMsg(WPARAM wp, LPARAM lp);
	afx_msg int OnVKeyToItem(UINT nKey, CListBox* pListBox, UINT nIndex);
	afx_msg void OnBnClickedOptions();
	afx_msg void OnClose();

	DECLARE_MESSAGE_MAP()

public:
	void SetAppState(EnAppState state);

private:
	CListBox m_Info;
	CEdit m_Port;
	CButton m_Start;
	CButton m_Stop;
	CButton m_Options;
	CButton m_ShowLog;
	EnAppState m_enState;

public:
	smart_simple_ptr<CTcpServerListenerImpl> m_pServerListener;
	smart_simple_ptr<CTcpAgentListenerImpl>	 m_pAgentListener;
	CTcpServerPtr	m_Server;
	CTcpAgentPtr	m_Agent;

	BOOL m_bLog;
};

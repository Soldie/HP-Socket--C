
// ClientDlg.h : header file
//

#pragma once
#include "afxwin.h"
#include "../../../Src/HttpClient.h"
#include "../../Global/helper.h"


// CClientDlg dialog
class CClientDlg : public CDialogEx
{
// Construction
public:
	CClientDlg(CWnd* pParent = NULL);	// standard constructor

// Dialog Data
	enum { IDD = IDD_CLIENT_DIALOG };

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
	afx_msg void OnBnClickedHeaderAdd();
	afx_msg void OnBnClickedSend();

	void SendHttp();
	void SendWebSocket();

	afx_msg void OnBnClickedStart();
	afx_msg void OnBnClickedStop();
	afx_msg LRESULT OnUserInfoMsg(WPARAM wp, LPARAM lp);
	afx_msg void OnCbnSelchangeMethod();
	afx_msg int OnVKeyToItem(UINT nKey, CListBox* pListBox, UINT nIndex);
	DECLARE_MESSAGE_MAP()

private:
	void SetAppState(EnAppState state);
	BOOL CheckStarted(BOOL bRestart = TRUE);
	static void CheckSetCookie(IHttpSyncClient* pHttpClient);
	static CStringA GetHeaderSummary(IHttpSyncClient* pHttpClient, LPCSTR lpszSep = "  ", int iSepCount = 0, BOOL bWithContentLength = TRUE);

private:
	CButton m_Send;
	CListBox m_Info;
	CEdit m_Address;
	CEdit m_Port;
	CButton m_Start;
	CButton m_Stop;
	CComboBox m_Method;
	CComboBox m_Schema;
	CEdit m_Path;
	CEdit m_HeaderName;
	CEdit m_HeaderValue;
	CButton m_HeaderAdd;
	CListBox m_Headers;
	CEdit m_Body;

	BOOL m_bWebSocket;
	EnAppState m_enState;

	unique_ptr<IHttpSyncClient> m_pClient;
};

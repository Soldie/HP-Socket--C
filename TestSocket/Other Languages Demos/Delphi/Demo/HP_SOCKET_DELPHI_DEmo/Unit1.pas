unit Unit1;

interface

uses
  Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants,
  System.Classes, Vcl.Graphics, HPSOCKET4C, HPSOCKET4CHelper,
  Vcl.Controls, Vcl.Forms, Vcl.Dialogs, Vcl.StdCtrls, Vcl.ExtCtrls,
  Vcl.ComCtrls;

type
  TForm1 = class(TForm)
    Panel1: TPanel;
    btnsend: TButton;
    edtmessage: TEdit;
    Panel2: TPanel;
    btnstart: TButton;
    btnstop: TButton;
    chkasync: TCheckBox;
    edtserverip: TEdit;
    edtserverport: TEdit;
    Label1: TLabel;
    Label2: TLabel;
    Panel3: TPanel;
    RichEdit1: TRichEdit;
    procedure FormCreate(Sender: TObject);
    procedure FormDestroy(Sender: TObject);
    procedure btnsendClick(Sender: TObject);
    procedure btnstartClick(Sender: TObject);
    procedure btnstopClick(Sender: TObject);
  private
    { Private declarations }
    m_spClient: HP_TcpPullClient;
    m_spListener: HP_TcpClientListener;
    m_enState: EnAppState;

    procedure SetAppState(state: EnAppState);
  public
    { Public declarations }
  end;

function OnConnect(dwConnID: HP_CONNID): En_HP_HandleResult; stdcall;
function OnSend(dwConnID: HP_CONNID; pData: PByte; iLength: Integer)
  : En_HP_HandleResult; stdcall;
function OnReceive(dwConnID: HP_CONNID; iLength: Integer)
  : En_HP_HandleResult; stdcall;
function OnClose(dwConnID: HP_CONNID): En_HP_HandleResult; stdcall;
function OnError(dwConnID: HP_CONNID; enOperation: En_HP_SocketOperation;
  iErrorCode: Integer): En_HP_HandleResult; stdcall;

var
  Form1: TForm1;

implementation

{$R *.dfm}

procedure TForm1.btnsendClick(Sender: TObject);
var
 data: string;
begin
  data:= edtmessage.Text;
  if HP_Client_Send(m_spClient, HP_Client_GetConnectionID(m_spClient),
    PByte(data), Length(data)*2) then
    RichEdit1.Lines.Add(Format('[%s Sent]%s',
      [inttostr(HP_Client_GetConnectionID(m_spClient)), edtmessage.Text]))
  else
  begin
   // RichEdit1.Lines.Add(Format('[%d Sent]%s',
   //   [HP_Client_GetConnectionID(m_spClient), 'sent failed']));
  end;
end;

procedure TForm1.btnstartClick(Sender: TObject);
begin
  if HP_Client_Start(m_spClient, PWideChar(edtserverip.Text),
    StrToInt(edtserverport.Text), chkasync.Checked) then
  begin
    SetAppState(ST_STARTING);
  end;
end;

procedure TForm1.btnstopClick(Sender: TObject);
begin
  SetAppState(ST_STOPING);
  HP_Client_Stop(m_spClient);
end;

procedure TForm1.FormCreate(Sender: TObject);
begin
  m_spListener := Create_HP_TcpPullClientListener();
  m_spClient := Create_HP_TcpPullClient(m_spListener);
  HP_Set_FN_Client_OnConnect(m_spListener, @OnConnect);
  HP_Set_FN_Client_OnSend(m_spListener, @OnSend);
  HP_Set_FN_Client_OnPullReceive(m_spListener, @OnReceive);
  HP_Set_FN_Client_OnClose(m_spListener, @OnClose);
  HP_Set_FN_Client_OnError(m_spListener, @OnError);
end;

procedure TForm1.FormDestroy(Sender: TObject);
begin
  Destroy_HP_TcpPullClient(m_spClient);
  Destroy_HP_TcpPullClientListener(m_spListener);
end;

function OnClose(dwConnID: HP_CONNID): En_HP_HandleResult;
begin
  Form1.SetAppState(ST_STOPED);
  Result := HP_HR_OK;
end;

function OnConnect(dwConnID: HP_CONNID): En_HP_HandleResult;
var
  addr: array[0..39] of Char;
  ileng: Integer;
  iPort: DWORD;
begin
  Result := HP_HR_OK;
  ZeroMemory(PChar(@addr), 40);
  ileng:= 40;
  HP_Client_GetLocalAddress(Form1.m_spClient, PChar(@addr), PINT(@ileng), PWORD(@iPort));
  Form1.RichEdit1.Lines.Add(string(addr) + ':' + IntToStr(iPort));
  Form1.SetAppState(ST_STARTED);
end;

function OnError(dwConnID: HP_CONNID; enOperation: En_HP_SocketOperation;
  iErrorCode: Integer): En_HP_HandleResult;
begin
  Result := HP_HR_OK;
  Form1.SetAppState(ST_STOPED);
end;

function OnReceive(dwConnID: HP_CONNID; iLength: Integer): En_HP_HandleResult;
var
  buffer: TBytes;
  s: string;
  ret: En_HP_FetchResult;
begin
  Result := HP_HR_OK;
  SetLength(buffer, iLength);
  while True do
  begin
   ret:= HP_TcpPullClient_Fetch(Form1.m_spClient, dwConnID, PByte(buffer), iLength);
    s:= s + WideStringOf(buffer);
    if ret = HP_FR_OK then Break;    
  end;

  Form1.RichEdit1.Lines.Add('[GET]' + s);
end;

function OnSend(dwConnID: HP_CONNID; pData: PByte; iLength: Integer)
  : En_HP_HandleResult;
var
  s: WideString;
  tmp: TBytes;
begin
  Result := HP_HR_OK;
  SetLength(tmp, iLength);
  Move(pData^, pbyte(tmp)^, iLength);
  s:= WideStringOf(tmp);
  Form1.RichEdit1.Lines.Add('[Sent]' + s);

end;

procedure TForm1.SetAppState(state: EnAppState);
begin
  m_enState := state;
  chkasync.Enabled := (m_enState = ST_STOPED);
  btnstart.Enabled := (m_enState = ST_STOPED);
  btnstop.Enabled := (m_enState = ST_STARTED);
  btnsend.Enabled := (m_enState = ST_STARTED);
  edtserverip.Enabled := (m_enState = ST_STOPED);
  edtserverport.Enabled := (m_enState = ST_STOPED);
end;

end.

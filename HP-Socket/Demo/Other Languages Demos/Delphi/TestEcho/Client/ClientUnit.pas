unit ClientUnit;

interface

uses
    Winapi.Windows, Winapi.Messages, System.SysUtils, System.Variants, System.Classes, Vcl.Graphics,
    Vcl.Controls, Vcl.Forms, Vcl.Dialogs, Vcl.StdCtrls, HPSocketSDKUnit, ExePublic;

type
    TForm1 = class(TForm)
        lstMsg: TListBox;
        edtIpAddress: TEdit;
        edtPort: TEdit;
        btnStart: TButton;
        btnStop: TButton;
        chkAsyncConnect: TCheckBox;
        edtMsg: TEdit;
        btnSend: TButton;
        procedure btnStartClick(Sender: TObject);
        procedure btnStopClick(Sender: TObject);
        procedure FormCreate(Sender: TObject);
        procedure btnSendClick(Sender: TObject);
        procedure lstMsgKeyPress(Sender: TObject; var Key: Char);
        procedure FormClose(Sender: TObject; var Action: TCloseAction);
    private
        { Private declarations }
        procedure AddMsg(msg: string);
        procedure SetAppState(state: EnAppState);
    public
        { Public declarations }
    end;

    TDoClientRec = class(TThread)
      Flen : Integer;
      fData: Pointer;
      RecMsg : PTMsg;
      procedure showmsg;
    protected
      procedure Execute;override;
    public

      constructor Create(len : Integer; pData: Pointer);
    end;

var
    Form1: TForm1;
    appState: EnAppState;
    pClient: Pointer;
    pListener: Pointer;

implementation

{$R *.dfm}

procedure TForm1.SetAppState(state: EnAppState);
begin
    appState := state;
    btnStart.Enabled := (appState = EnAppState.ST_STOPED);
    btnStop.Enabled := (appState = EnAppState.ST_STARTED);
    edtIpAddress.Enabled := (appState = EnAppState.ST_STOPED);
    chkAsyncConnect.Enabled := (appState = EnAppState.ST_STOPED);
    btnSend.Enabled := (appState = EnAppState.ST_STARTED);
end;

procedure TForm1.AddMsg(msg: string);
begin
    if lstMsg.Items.Count > 100 then
    begin
        lstMsg.Items.Clear;
    end;
    lstMsg.Items.Add(msg);
end;

function OnConnect(dwConnID: DWORD): En_HP_HandleResult; stdcall;
begin
    if Form1.chkAsyncConnect.Checked = True then
        Form1.SetAppState(ST_STARTED);
    Result := HP_HR_OK;
end;

function HowManyChineseChar(Const s: String): Integer;
var
    SW: WideString;
    C: String;
    i, WCount: Integer;
begin
    SW := s;
    WCount := 0;
    For i := 1 to Length(SW) do
    begin
        C := SW[i];
        if Length(C) > $7F then
            Inc(WCount)

    end;
    Result := WCount;
end;

function SendString(str: string): Boolean;
var
    {
    sendBuffer: array of byte;
    sendStr: AnsiString;
    sendLength: Integer;  }
  SendMsg : PTMsg;
begin
    {
    sendStr := AnsiString(str);
    // 获取ansi字符串的长度
    sendLength := Length(sendStr);
    // 设置buf数组的长度
    SetLength(sendBuffer, sendLength);
    // 复制数据到buf数组
    Move(sendStr[1], sendBuffer[0], sendLength);    }


    New(SendMsg);
    SendMsg.nType := 1000;
    SendMsg.nMsg := str;

    Result := HP_Client_Send(pClient, SendMsg, SizeOf(ttmsg));
end;

procedure TForm1.btnSendClick(Sender: TObject);
var
    dwConnID: DWORD;
begin
    dwConnID := HP_Client_GetConnectionID(pClient);
    if SendString(edtMsg.Text) then
    begin
        AddMsg(Format('$ (%d) Send OK --> %s', [dwConnID, edtMsg.Text]));
    end
    else
    begin
        AddMsg(Format('$ (%d) Send Fail --> %s', [dwConnID, edtMsg.Text]));
    end;
end;

procedure TForm1.btnStartClick(Sender: TObject);
var
    ip: PWideChar;
    port: USHORT;
begin
    // 异常检查自己做
    ip := PWideChar(edtIpAddress.Text);
    port := USHORT(StrToInt(edtPort.Text));

    // 写在这个位置是上面可能会异常
    SetAppState(ST_STARTING);

    if (HP_Client_Start(pClient, ip, port, chkAsyncConnect.Checked)) then
    begin
        if chkAsyncConnect.Checked = False then
            SetAppState(ST_STARTED);
        AddMsg(Format('$Client Starting ... -> (%s:%d)', [ip, port]));
    end
    else
    begin
        SetAppState(ST_STOPED);
        AddMsg(Format('$Client Start Error -> %s(%d)', [HP_Client_GetLastErrorDesc(pClient),
            Integer( HP_Client_GetLastError(pClient))]));
    end;

end;

function OnSend(dwConnID: DWORD; const pData: Pointer; iLength: Integer): En_HP_HandleResult; stdcall;
begin
    Form1.AddMsg(Format(' > [%d,OnSend] -> (%d bytes)', [dwConnID, iLength]));
    Result := HP_HR_OK;
end;

function OnReceive(dwConnID: DWORD; const pData: Pointer; iLength: Integer): En_HP_HandleResult; stdcall;
var
    testString: AnsiString;

    doRec : TDoClientRec;
begin
    Form1.AddMsg(Format(' > [%d,OnReceive] -> (%d bytes)', [dwConnID, iLength]));

    {// 以下是一个pData转字符串的演示
    SetLength(testString, iLength);
    Move(pData^, testString[1],  iLength);
    Form1.AddMsg(Format(' > [%d,OnReceive] -> say:%s', [dwConnId, testString]));
    }

    doRec := TDoClientRec.Create(iLength, pData);
    doRec.Resume;

    Result := HP_HR_OK;
end;

function OnCloseConn(dwConnID: DWORD): En_HP_HandleResult; stdcall;
begin

    Form1.AddMsg(Format(' > [%d,OnCloseConn]', [dwConnID]));

    Form1.SetAppState(ST_STOPED);
    Result := HP_HR_OK;
end;

function OnError(dwConnID: DWORD; enOperation: En_HP_SocketOperation; iErrorCode: Integer): En_HP_HandleResult; stdcall;
begin

    Form1.AddMsg(Format('> [%d,OnError] -> OP:%d,CODE:%d', [dwConnID, Integer(enOperation), iErrorCode]));

    Form1.SetAppState(ST_STOPED);
    Result := HP_HR_OK;
end;

procedure TForm1.btnStopClick(Sender: TObject);
begin

    SetAppState(ST_STOPING);

    // 停止服务
    AddMsg('$Server Stop');
    if (HP_Client_Stop(pClient)) then
        SetAppState(ST_STOPED)
    else
    begin
        AddMsg(Format('$Stop Error -> %s(%d)', [HP_Client_GetLastErrorDesc(pClient),
                Integer(HP_Client_GetLastError(pClient))]));
    end;
end;

procedure TForm1.FormClose(Sender: TObject; var Action: TCloseAction);
begin
    // 销毁 Socket 对象
    Destroy_HP_TcpClient(pClient);

    // 销毁监听器对象
    Destroy_HP_TcpClientListener(pListener);
end;

procedure TForm1.FormCreate(Sender: TObject);
begin
    // 创建监听器对象
    pListener := Create_HP_TcpClientListener();
    // 创建 Socket 对象
    pClient := Create_HP_TcpClient(pListener);

    // 设置 Socket 监听器回调函数
    HP_Set_FN_Client_OnConnect(pListener, OnConnect);
    HP_Set_FN_Client_OnSend(pListener, OnSend);
    HP_Set_FN_Client_OnReceive(pListener, OnReceive);
    HP_Set_FN_Client_OnClose(pListener, OnCloseConn);
    HP_Set_FN_Client_OnError(pListener, OnError);

    SetAppState(ST_STOPED)
end;

procedure TForm1.lstMsgKeyPress(Sender: TObject; var Key: Char);
begin
    if (Key = 'c') or (Key = 'C') then
        lstMsg.Items.Clear;
end;

{ TDoClientRec }

constructor TDoClientRec.Create(len: Integer; pData: Pointer);
begin
  inherited Create(True);
  Flen := len;
  Move(pdata, fData, Flen);
end;

procedure TDoClientRec.Execute;
begin
  inherited;
  try
    Move(fdata, RecMsg, Flen);
    try
      case RecMsg.nType of
        1001 : begin
                 Synchronize(showmsg);
               end;
      end;
    except

    end;
  finally

  end;
end;

procedure TDoClientRec.showmsg;
begin
  Form1.AddMsg('Rec:' + RecMsg.nMsg);
end;

end.

object Form1: TForm1
  Left = 0
  Top = 0
  BorderIcons = [biSystemMenu, biMinimize]
  BorderStyle = bsSingle
  Caption = 'Echo-Client [ '#39'C'#39' - clear list box ]'
  ClientHeight = 331
  ClientWidth = 458
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  Position = poScreenCenter
  OnClose = FormClose
  OnCreate = FormCreate
  PixelsPerInch = 96
  TextHeight = 13
  object lstMsg: TListBox
    Left = 1
    Top = 24
    Width = 457
    Height = 274
    ItemHeight = 13
    TabOrder = 0
    OnKeyPress = lstMsgKeyPress
  end
  object edtIpAddress: TEdit
    Left = 1
    Top = 304
    Width = 121
    Height = 21
    TabOrder = 1
    Text = '127.0.0.1'
  end
  object edtPort: TEdit
    Left = 128
    Top = 304
    Width = 48
    Height = 21
    TabOrder = 2
    Text = '5555'
  end
  object btnStart: TButton
    Left = 335
    Top = 304
    Width = 57
    Height = 23
    Caption = 'Start'
    TabOrder = 4
    OnClick = btnStartClick
  end
  object btnStop: TButton
    Left = 398
    Top = 304
    Width = 57
    Height = 23
    Caption = 'Stop'
    TabOrder = 5
    OnClick = btnStopClick
  end
  object chkAsyncConnect: TCheckBox
    Left = 182
    Top = 306
    Width = 83
    Height = 17
    Caption = 'Async Conn'
    Checked = True
    State = cbChecked
    TabOrder = 3
  end
  object edtMsg: TEdit
    Left = 1
    Top = 1
    Width = 395
    Height = 21
    TabOrder = 6
    Text = 'text to be sent'
  end
  object btnSend: TButton
    Left = 399
    Top = 0
    Width = 57
    Height = 23
    Caption = 'Send'
    TabOrder = 7
    OnClick = btnSendClick
  end
end

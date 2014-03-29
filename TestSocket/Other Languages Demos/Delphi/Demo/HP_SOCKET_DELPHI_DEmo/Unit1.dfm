object Form1: TForm1
  Left = 0
  Top = 0
  Caption = 'HP_SOCKET ECHO CLIENT'
  ClientHeight = 393
  ClientWidth = 548
  Color = clBtnFace
  Font.Charset = DEFAULT_CHARSET
  Font.Color = clWindowText
  Font.Height = -11
  Font.Name = 'Tahoma'
  Font.Style = []
  OldCreateOrder = False
  OnCreate = FormCreate
  OnDestroy = FormDestroy
  PixelsPerInch = 96
  TextHeight = 13
  object Panel1: TPanel
    AlignWithMargins = True
    Left = 3
    Top = 357
    Width = 542
    Height = 33
    Align = alBottom
    Caption = 'Panel1'
    TabOrder = 0
    object btnsend: TButton
      AlignWithMargins = True
      Left = 495
      Top = 4
      Width = 43
      Height = 25
      Align = alRight
      Caption = 'send'
      TabOrder = 0
      OnClick = btnsendClick
    end
    object edtmessage: TEdit
      AlignWithMargins = True
      Left = 4
      Top = 4
      Width = 485
      Height = 25
      Align = alClient
      TabOrder = 1
      Text = 'Message to send'
      ExplicitHeight = 21
    end
  end
  object Panel2: TPanel
    AlignWithMargins = True
    Left = 3
    Top = 3
    Width = 542
    Height = 30
    Align = alTop
    TabOrder = 1
    object Label1: TLabel
      AlignWithMargins = True
      Left = 4
      Top = 4
      Width = 46
      Height = 22
      Align = alLeft
      Alignment = taCenter
      Caption = 'server ip:'
      Layout = tlCenter
      ExplicitHeight = 13
    end
    object Label2: TLabel
      AlignWithMargins = True
      Left = 152
      Top = 4
      Width = 54
      Height = 22
      Align = alLeft
      Alignment = taCenter
      Caption = 'server port'
      Layout = tlCenter
      ExplicitLeft = 170
      ExplicitHeight = 13
    end
    object btnstart: TButton
      AlignWithMargins = True
      Left = 440
      Top = 4
      Width = 46
      Height = 22
      Align = alRight
      Caption = 'start'
      TabOrder = 0
      OnClick = btnstartClick
    end
    object btnstop: TButton
      AlignWithMargins = True
      Left = 492
      Top = 4
      Width = 46
      Height = 22
      Align = alRight
      Caption = 'stop'
      TabOrder = 1
      OnClick = btnstopClick
    end
    object chkasync: TCheckBox
      AlignWithMargins = True
      Left = 276
      Top = 4
      Width = 82
      Height = 22
      Align = alLeft
      Caption = 'Async Conn'
      TabOrder = 2
    end
    object edtserverip: TEdit
      AlignWithMargins = True
      Left = 56
      Top = 4
      Width = 90
      Height = 22
      Align = alLeft
      TabOrder = 3
      Text = '127.0.0.1'
      ExplicitHeight = 21
    end
    object edtserverport: TEdit
      AlignWithMargins = True
      Left = 212
      Top = 4
      Width = 58
      Height = 22
      Align = alLeft
      TabOrder = 4
      Text = '5555'
      ExplicitHeight = 21
    end
  end
  object Panel3: TPanel
    AlignWithMargins = True
    Left = 3
    Top = 39
    Width = 542
    Height = 312
    Align = alClient
    Caption = 'Panel3'
    TabOrder = 2
    object RichEdit1: TRichEdit
      AlignWithMargins = True
      Left = 4
      Top = 4
      Width = 534
      Height = 304
      Align = alClient
      Font.Charset = GB2312_CHARSET
      Font.Color = clWindowText
      Font.Height = -11
      Font.Name = 'Tahoma'
      Font.Style = []
      ParentFont = False
      TabOrder = 0
    end
  end
end

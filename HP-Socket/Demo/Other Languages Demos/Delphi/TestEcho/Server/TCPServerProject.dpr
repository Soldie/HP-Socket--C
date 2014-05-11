program TCPServerProject;

uses
  Vcl.Forms,
  ServerUnit in 'ServerUnit.pas' {Form1},
  HPSocketSDKUnit in 'HPSocketSDKUnit.pas';

{$R *.res}

begin
  Application.Initialize;
  Application.MainFormOnTaskbar := True;
  Application.CreateForm(TForm1, Form1);
  Application.Run;
end.

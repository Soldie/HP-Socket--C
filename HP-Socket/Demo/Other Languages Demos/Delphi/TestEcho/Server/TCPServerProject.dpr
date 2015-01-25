program TCPServerProject;

uses
  Forms,
  ServerUnit in 'ServerUnit.pas' {Form1},
  HPSocketSDKUnit in 'HPSocketSDKUnit.pas',
  ExePublic in '..\ExePublic.pas';

{$R *.res}

begin
  Application.Initialize;
  Application.MainFormOnTaskbar := True;
  Application.CreateForm(TForm1, Form1);
  application.Run;
end.

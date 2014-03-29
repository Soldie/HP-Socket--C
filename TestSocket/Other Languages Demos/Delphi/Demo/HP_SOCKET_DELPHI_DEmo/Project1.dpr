program Project1;

uses
  Vcl.Forms,
  Unit1 in 'Unit1.pas' {Form1} ,
  HPSOCKET4C in 'HPSOCKET4C.PAS',
  HPSOCKET4CHelper in 'HPSOCKET4CHelper.pas';

{$R *.res}

begin
  Application.Initialize;
  Application.MainFormOnTaskbar := True;
  Application.CreateForm(TForm1, Form1);
  Application.Run;

end.

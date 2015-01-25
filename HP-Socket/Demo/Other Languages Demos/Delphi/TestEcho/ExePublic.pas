unit ExePublic;

interface

type
  PTMsg = ^TTMsg;
  TTMsg = record
    nType : Integer;
    //可以选择array of char等来作为传递指针数组
    //本例值提供传递字符串模式。
    nMsg : string[254];
  end;

implementation

end.

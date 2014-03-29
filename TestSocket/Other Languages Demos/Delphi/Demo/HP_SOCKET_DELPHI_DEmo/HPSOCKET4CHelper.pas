unit HPSOCKET4CHelper;

interface

const
  EVT_ON_SEND: string = 'OnSend';
  EVT_ON_RECEIVE: string = 'OnReceive';
  EVT_ON_CLOSE: string = 'OnClose';
  EVT_ON_ERROR: string = 'OnError';
  EVT_ON_PREPARE_CONNECT: string = 'OnPrepareConnect';
  EVT_ON_PREPARE_LISTEN: string = 'OnPrepareListen';
  EVT_ON_ACCEPT: string = 'OnAccept';
  EVT_ON_CONNECT: string = 'OnConnect';
  EVT_ON_SHUTDOWN: string = 'OnShutdown';
  EVT_ON_END_TEST: string = 'END TEST';

type
  EnAppState = (ST_STARTING = 0, ST_STARTED, ST_STOPING, ST_STOPED);

implementation

end.

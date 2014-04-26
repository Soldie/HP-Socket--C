package global;

import java.io.ByteArrayInputStream;
import java.io.ByteArrayOutputStream;
import java.io.ObjectInputStream;
import java.io.ObjectOutputStream;
import java.io.Serializable;
import java.util.Deque;
import java.util.LinkedList;

import javax.swing.DefaultListModel;
import javax.swing.JFrame;
import javax.swing.JList;
import javax.swing.SwingUtilities;

import pfm.ServerApp;

public class Util
{
	public enum AppState
	{
		STARTING, STARTED, CONNECTING, CONNECTED, STOPPING, STOPPED;
	}
	
	private static final int MAX_LOG_RECORD_LENGTH	= 1000;
	private static final Object LOCK				= new Object();
	private static final MsgRender RENDER			= new MsgRender();
	private static Deque<String> msgList			= new LinkedList<String>();
	
	public static abstract class EventBase<T extends JFrame>
	{
		protected T app;

		protected EventBase(T app)
		{
			this.app = app;
		}

	}
	
	public static class InfoMsg
	{
		long connId;
		String event;
		int length;
		String content;
		
		public InfoMsg(long connId, String event)
		{
			this(connId, event, 0, null);
		}
		
		public InfoMsg(long connId, String event, String content)
		{
			this(connId, event, content == null ? 0 : content.length(), content);
		}
		
		public InfoMsg(String event)
		{
			this(0, event, 0, null);
		}
		
		public InfoMsg(String event, String content)
		{
			this(0, event, content == null ? 0 : content.length(), content);
		}
		
		public InfoMsg(long connId, String event, int length, String content)
		{
			this.connId = connId;
			this.event = event;
			this.length = length;
			this.content = content;
		}
		
	}

	public static final String DEF_BIND_ADDRESS			= "0.0.0.0";
	public static final String DEF_CONN_ADDRESS			= "127.0.0.1";
	public static final short DEF_SERVER_PORT			= 5555;
	
	public static final String EVT_ON_SEND				= "OnSend";
	public static final String EVT_ON_RECEIVE			= "OnReceive";
	public static final String EVT_ON_CLOSE				= "OnClose";
	public static final String EVT_ON_ERROR				= "OnError";
	public static final String EVT_ON_PREPARE_CONNECT	= "OnPrepareConnect";
	public static final String EVT_ON_PREPARE_LISTEN	= "OnPrepareListen";
	public static final String EVT_ON_ACCEPT			= "OnAccept";
	public static final String EVT_ON_CONNECT			= "OnConnect";
	public static final String EVT_ON_SHUTDOWN			= "OnShutdown";
	public static final String EVT_ON_END_TEST			= "END TEST";
	

	private static JFrame mainFrame;
	private static JList infoList;
	private static DefaultListModel listModel;
	
	public static final void logServerStart(String lpszAddress, short port)
	{
		logMsg("$ Server Start OK --> (%s : %d)", lpszAddress, port);
	}

	public static final void logServerStartFail(int code, String lpszDesc)
	{
		logMsg("$ Server Start Fail --> %s (%d)", lpszDesc, code);
	}

	public static final void logServerStop()
	{
		logMsg("$ Server Stop");
	}

	public static final void logServerStopFail(int code, String lpszDesc)
	{
		logMsg("$ Server Stop Fail --> %s (%d)", lpszDesc, code);
	}

	public static final void logClientStart(String lpszAddress, short port)
	{
		logMsg("$ Client Start OK --> (%s : %d)", lpszAddress, port);
	}

	public static final void logClientStarting(String lpszAddress, short port)
	{
		logMsg("$ Client Starting ... --> (%s : %d)", lpszAddress, port);
	}

	public static final void logClientStartFail(int code, String lpszDesc)
	{
		logMsg("$ Client Start Fail --> %s (%d)", lpszDesc, code);
	}

	public static final void logClientStopping(long dwConnID)
	{
		logMsg("$ Client Stopping ... --> (%d)", dwConnID);
	}

	public static final void logClientStopFail(int code, String lpszDesc)
	{
		logMsg("$ Client Stop Fail --> %s (%d)", lpszDesc, code);
	}

	public static final void logClientSendFail(int iSequence, int iSocketIndex, int code, String lpszDesc)
	{
		logMsg("$ Client Send Fail [SOCK: %d, SEQ: %d] --> %s (%d)", iSocketIndex, iSequence, lpszDesc, code);
	}

	public static final void logSend(long dwConnID, String lpszContent)
	{
		logMsg("$ (%d) Send OK --> %s", dwConnID, lpszContent);
	}

	public static final void logSendFail(long dwConnID, int code, String lpszDesc)
	{
		logMsg("$ (%d) Send Fail --> %s (%d)", dwConnID, lpszDesc, code);
	}

	public static final void logDisconnect(long dwConnID)
	{
		logMsg("$ (%d) Disconnect OK", dwConnID);
	}

	public static final void logDisconnectFail(long dwConnID)
	{
		logMsg("$ (%d) Disconnect Fail", dwConnID);
	}

	public static final void logDetect(long dwConnID)
	{
		logMsg("$ (%d) Detect Connection OK", dwConnID);
	}

	public static final void logDetectFail(long dwConnID)
	{
		logMsg("$ (%d) Detect Connection Fail", dwConnID);
	}

	public static final void logOnConnect(long dwConnID, String lpszAddress, short usPort)
	{
		String lpszContent = String.format("local address: %s:%d", lpszAddress, usPort);
		logInfoMsg(new InfoMsg(dwConnID, EVT_ON_CONNECT, lpszContent));
	}

	public static final void logOnConnect2(long dwConnID)
	{
		logInfoMsg(new InfoMsg(dwConnID, EVT_ON_CONNECT));
	}

	public static final void logOnSend(long dwConnID, int iLength)
	{
		String lpszContent = String.format("(%d bytes)", iLength);
		logInfoMsg(new InfoMsg(dwConnID, EVT_ON_SEND, lpszContent));
	}

	public static final void logOnReceive(long dwConnID, int iLength)
	{
		String lpszContent = String.format("(%d bytes)", iLength);
		logInfoMsg(new InfoMsg(dwConnID, EVT_ON_RECEIVE, lpszContent));
	}

	public static final void logOnClose(long dwConnID)
	{
		logInfoMsg(new InfoMsg(dwConnID, EVT_ON_CLOSE));
	}

	public static final void logOnError(long dwConnID, int enOperation, int iErrorCode)
	{
		String lpszContent = String.format("OP: %d, CODE: %d", enOperation, iErrorCode);
		logInfoMsg(new InfoMsg(dwConnID, EVT_ON_ERROR, lpszContent));
	}

	public static final void logOnAccept(long dwConnID, String lpszAddress, short usPort, boolean bPass)
	{
		String lpszContent = String.format("%s (%s:%d)", bPass ? "PASS" : "REJECT", lpszAddress, usPort);
		logInfoMsg(new InfoMsg(dwConnID, EVT_ON_ACCEPT, lpszContent));
	}

	public static final void logOnAccept2(long dwConnID)
	{
		logInfoMsg(new InfoMsg(dwConnID, EVT_ON_ACCEPT));
	}

	public static final void logOnPrepareListen(String lpszAddress, short usPort)
	{
		String lpszContent = String.format("bind address: %s:%d", lpszAddress, usPort);
		logInfoMsg(new InfoMsg(EVT_ON_PREPARE_LISTEN, lpszContent));
	}

	public static final void logOnPrepareConnect(long dwConnID)
	{
		logInfoMsg(new InfoMsg(dwConnID, EVT_ON_PREPARE_CONNECT));
	}

	public static final void logOnShutdown()
	{
		logInfoMsg(new InfoMsg(EVT_ON_SHUTDOWN));
	}

	public static final void logServerStatics(long llTotalSent, long llTotalReceived)
	{
		String lpszContent = String.format(" *** Summary: send - %d, recv - %d", llTotalSent, llTotalReceived);
		logInfoMsg(new InfoMsg(EVT_ON_END_TEST, lpszContent));
	}

	public static final void logTimeConsuming(long dwTickCount)
	{
		String lpszContent = String.format("Total Time Consuming: %d", dwTickCount);
		logInfoMsg(new InfoMsg(EVT_ON_END_TEST, lpszContent));
	}

	public static final void logInfoMsg(InfoMsg infoMsg)
	{
		String msg;
		
		if(infoMsg.connId > 0)
		{
			if(infoMsg.length > 0)
				msg = String.format("  > [ %d, %s ] -> %s", infoMsg.connId, infoMsg.event, infoMsg.content);
			else
				msg = String.format("  > [ %d, %s ]", infoMsg.connId, infoMsg.event);
		}
		else
		{
			if(infoMsg.length > 0)
				msg = String.format("  > [ %s ] -> %s", infoMsg.event, infoMsg.content);
			else
				msg = String.format("  > [ %s ]", infoMsg.event);
			
		}
		
		logMsg(msg);
	}
	
	public static final void logMsg(String msg, Object ... params)
	{
		msg = params.length >= 0 ? String.format(msg, params) : msg;
		
		synchronized(LOCK)
		{
			msgList.addLast(msg);
		}
		
		SwingUtilities.invokeLater(RENDER);
	}
	
	public static final void logMsgImmediately(String msg, Object ... params)
	{
		msg = params.length >= 0 ? String.format(msg, params) : msg;
		
		synchronized(LOCK)
		{
			msgList.addLast(msg);
		}
		
		RENDER.run();
		
		if(infoList != null)
			infoList.paint(infoList.getGraphics());

	}
	
	static class MsgRender implements Runnable
	{

		@Override
		public void run()
		{
			if(mainFrame == null || msgList.isEmpty())
				return;
			
			infoList.invalidate();
			
			int index	 = infoList.getSelectedIndex();
			int count	 = listModel.getSize();
			boolean last = index == -1 || index == count -1;

			synchronized(LOCK)
			{
				while(msgList.size() > 0)
				{
					String msg = msgList.removeFirst();
					
					if(count >= MAX_LOG_RECORD_LENGTH)
					{
						listModel.remove(0);
						--count;
					}
					
					listModel.addElement(msg);
				}
			}
			
			if(last)
			{
				index = listModel.getSize() - 1;
				infoList.setSelectedIndex(index);
				infoList.ensureIndexIsVisible(index);
			}

			infoList.validate();
		}
	}

	public static final void clearInfoList()
	{
		if(listModel != null)
			listModel.removeAllElements();
	}
	
	public static final JFrame getMainFrame()
	{
		return mainFrame;
	}

	public static final void setMainFrame(JFrame mainFrame)
	{
		Util.mainFrame = mainFrame;
		
		if(mainFrame == null)
			setInfoList(null);
	}

	public static final JList getInfoList()
	{
		return infoList;
	}

	public static final void setInfoList(JList infoList)
	{
		Util.infoList = infoList;
		
		if(infoList != null)
			listModel = (DefaultListModel)infoList.getModel();
		else
			listModel = null;
	}

	public static final byte[] object2ByteArray(Serializable obj)
	{
		try
		{
			ByteArrayOutputStream bos = new ByteArrayOutputStream();
			ObjectOutputStream oos = new ObjectOutputStream(bos);
			oos.writeObject(obj);
			return bos.toByteArray();
		}
		catch(Exception e)
		{
			throw new RuntimeException(e);
		}
	}
	
	@SuppressWarnings("unchecked")
	public static final <T extends Serializable> T byteArray2Object(byte[] bytes)
	{
		try
		{
			ByteArrayInputStream bis = new ByteArrayInputStream(bytes);
			ObjectInputStream ois = new ObjectInputStream(bis);
			return (T)ois.readObject();
		}
		catch(Exception e)
		{
			throw new RuntimeException(e);
		}
	}
}

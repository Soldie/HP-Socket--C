package test;

import java.io.IOException;

import org.jessma.hpsocket.Callback.OnAgentShutdown;
import org.jessma.hpsocket.Callback.OnClose;
import org.jessma.hpsocket.Callback.OnConnect;
import org.jessma.hpsocket.Callback.OnError;
import org.jessma.hpsocket.Callback.OnPrepareConnect;
import org.jessma.hpsocket.Callback.OnPullReceive;
import org.jessma.hpsocket.Callback.OnReceive;
import org.jessma.hpsocket.Callback.OnSend;
import org.jessma.hpsocket.Constant.FetchResult;
import org.jessma.hpsocket.Constant.HandleResult;
import org.jessma.hpsocket.Constant.SocketError;
import org.jessma.hpsocket.HPSocketObjBase.Mode;
import org.jessma.hpsocket.SocketAddress;
import org.jessma.hpsocket.unicode.TcpAgent;

import com.sun.jna.NativeLong;
import com.sun.jna.Pointer;
import com.sun.jna.ptr.LongByReference;
import com.sun.jna.ptr.NativeLongByReference;
import com.sun.jna.ptr.PointerByReference;

public class Main
{
	static TcpAgent agent = TcpAgent.create(Mode.PUSH);
	
	public static void main(String[] args) throws IOException, InterruptedException
	{
		//System.out.println( System.getProperty("os.name"));
		//System.out.println( System.getProperty("os.version"));
		//System.out.println( System.getProperty("os.arch"));
		
		agent.setCallBackOnPrepareConnect(new OnPrepareConnectImpl());
		agent.setCallBackOnConnect(new OnConnectImpl());
		agent.setCallBackOnReceive(new OnReceiveImpl());
		agent.setCallBackOnPullReceive(new OnPullReceiveImpl());
		agent.setCallBackOnSend(new OnSendImpl());
		agent.setCallBackOnClose(new OnCloseImpl());
		agent.setCallBackOnError(new OnErrorImpl());
		agent.setCallBackOnAgentShutdown(new OnAgentShutdownImpl());
		
		if(agent.start("127.0.0.1", false))
		{
		}
		else
		{
			String desc = agent.getLastErrorDesc();		
			System.err.println(desc);
			
			System.exit(agent.getLastError());
		}
		
		NativeLongByReference pdwConnID = new NativeLongByReference();

		if(agent.connect("localhost", (short)5555, pdwConnID))
		{
			NativeLong dwConnID = pdwConnID.getValue();
			
			for(int i = 1; i <= 10; i++)
			{
				String text = "伤神小怪兽 - " + i;
				byte[] data = text.getBytes();
				
				if(!agent.send(dwConnID, data, data.length))
				{
					System.err.println("Send Fail -> " + TcpAgent.getNativeLastError() + ", " + TcpAgent.getSocketErrorDesc(SocketError.SE_DATA_SEND));
					break;
				}
			}
		}
		else
		{
			System.err.println("Connect Fail -> " + TcpAgent.getNativeLastError() + ", " + TcpAgent.getSocketErrorDesc(SocketError.SE_CONNECT_SERVER));
		}
		
		//System.in.read();
		
		for(int i = 1; i <=10; i++)
		{
			Thread.sleep(100);
			Thread.yield();
		}
		
		//agent.stop();
		TcpAgent.destroy(agent);
		
		for(int i = 1; i <=10; i++)
		{
			Thread.sleep(100);
			Thread.yield();
		}
		
		//System.in.read();
		
		System.exit(0);
	}
	
	static class OnPrepareConnectImpl implements OnPrepareConnect
	{
		
		@Override
		public int invoke(NativeLong dwConnID, Pointer socket)
		{
			System.out.println("OnPrepareConnect: " + dwConnID);
			return HandleResult.HR_OK;
		}
	}
	
	static class OnConnectImpl implements OnConnect
	{
		@Override
		public int invoke(NativeLong dwConnID)
		{
			System.out.println("OnConnect: " + dwConnID);
			
			LongByReference ref = new LongByReference(123);
			Pointer pExt = ref.getPointer();
			agent.setConnectionExtra(dwConnID, pExt);

			SocketAddress local = agent.getLocalAddress(dwConnID);
			SocketAddress remote = agent.getRemoteAddress(dwConnID);

			System.out.printf("\t-> %s:%d\n", remote.getAddress(), remote.getPort());

			return HandleResult.HR_OK;
		}
	}
	
	static class OnReceiveImpl implements OnReceive
	{

		@Override
		public int invoke(NativeLong dwConnID, Pointer pData, int iLength)
		{
			System.out.println("OnReceive: " + dwConnID + ", " + iLength);
			System.out.println("\t-> " + new String(pData.getByteArray(0, iLength)));
			return HandleResult.HR_OK;
		}

	}
	
	static class OnPullReceiveImpl implements OnPullReceive
	{

		@Override
		public int invoke(NativeLong dwConnID, int iLength)
		{
			PointerByReference ppExt = new PointerByReference();
			agent.getConnectionExtra(dwConnID, ppExt);
			Pointer pExt = ppExt.getValue();
			long v = pExt.getLong(0);

			byte[] pBuffer = new byte[iLength];
			int fr = agent.fetch(dwConnID, pBuffer, iLength);
			
			if(fr == FetchResult.FR_OK)
			{
				System.out.println("OnPullReceive: " + dwConnID + ", " + iLength);
				System.out.println("\t-> " + new String(pBuffer));
			}
			
			
			return HandleResult.HR_OK;
		}
		
	}
	
	static class OnSendImpl implements OnSend
	{
	
		@Override
		public int invoke(NativeLong dwConnID, Pointer pData, int iLength)
		{
			System.out.println("OnSend: " + dwConnID + ", " + iLength);
			System.out.println("\t-> " + new String(pData.getByteArray(0, iLength)));
			return HandleResult.HR_OK;
		}
		
	}
	
	static class OnCloseImpl implements OnClose
	{
		@Override
		public int invoke(NativeLong dwConnID)
		{
			System.out.println("OnClose: " + dwConnID);
			return HandleResult.HR_OK;
		}
	}
	
	static class OnErrorImpl implements OnError
	{
		
		@Override
		public int invoke(NativeLong dwConnID, int enOperation, int iErrorCode)
		{
			System.out.println("OnError: " + dwConnID + ", " + enOperation + ", " + iErrorCode);
			return HandleResult.HR_OK;
		}
		
	}
	
	static class OnAgentShutdownImpl implements OnAgentShutdown
	{
		@Override
		public int invoke()
		{
			System.out.println("OnAgentShutdown");
			return HandleResult.HR_OK;
		}
	}
}

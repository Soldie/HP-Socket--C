/*
 * Main.java
 *
 * Created on __DATE__, __TIME__
 */

package pull;

import global.Message;
import global.Util;
import global.Util.AppState;
import global.Util.EventBase;

import java.util.HashMap;
import java.util.Map;

import javax.swing.DefaultListModel;
import javax.swing.UIManager;
import javax.swing.UnsupportedLookAndFeelException;

import org.jessma.hpsocket.Callback.OnAccept;
import org.jessma.hpsocket.Callback.OnClose;
import org.jessma.hpsocket.Callback.OnError;
import org.jessma.hpsocket.Callback.OnPrepareListen;
import org.jessma.hpsocket.Callback.OnPullReceive;
import org.jessma.hpsocket.Callback.OnSend;
import org.jessma.hpsocket.Callback.OnServerShutdown;
import org.jessma.hpsocket.Constant.FetchResult;
import org.jessma.hpsocket.Constant.HandleResult;
import org.jessma.hpsocket.HPSocketObjBase.Mode;
import org.jessma.hpsocket.Helper;
import org.jessma.hpsocket.SocketAddress;
import org.jessma.hpsocket.unicode.TcpServer;

import com.sun.jna.NativeLong;
import com.sun.jna.Pointer;

import static global.Util.*;
import static global.Util.AppState.STARTED;
import static global.Util.AppState.STARTING;
import static global.Util.AppState.STOPPED;
import static global.Util.AppState.STOPPING;

/**
 *
 * @author  __USER__
 */
@SuppressWarnings("serial")
public class ServerApp extends javax.swing.JFrame
{
	private AppState state;
	private String reject;

	private TcpServer server;

	Map<NativeLong, Message> clients = new HashMap<NativeLong, Message>();

	/** Creates new form Main */
	public ServerApp()
	{
		initComponents();
		afterInitComponents();
	}

	private void afterInitComponents()
	{
		setLocationRelativeTo(null);
		setMainFrame(this);
		setInfoList(lsInfo);
		setAppState(STOPPED);
		btnStart.requestFocus();

		server = TcpServer.create(Mode.PULL);
		server.setCallBackOnPrepareListen(new OnPrepareListenImpl(this));
		server.setCallBackOnAccept(new OnAcceptImpl(this));
		server.setCallBackOnPullReceive(new OnPullReceiveImpl(this));
		server.setCallBackOnSend(new OnSendImpl(this));
		server.setCallBackOnClose(new OnCloseImpl(this));
		server.setCallBackOnError(new OnErrorImpl(this));
		server.setCallBackOnServerShutdown(new OnServerShutdownImpl(this));

	}

	private void setAppState(AppState state)
	{
		if(this.state == state)
			return;
		if(getMainFrame() == null)
			return;

		this.state = state;

		btnStart.setEnabled(state == STOPPED);
		btnStop.setEnabled(state == STARTED);
		txtReject.setEnabled(state == STOPPED);
		btnDisConn.setEnabled(state == STARTED);
		
		btnStart.paint(btnStart.getGraphics());
		btnStop.paint(btnStop.getGraphics());
		txtReject.paint(txtReject.getGraphics());
		btnDisConn.paint(btnDisConn.getGraphics());
	}

	private void btnStartActionPerformed(java.awt.event.ActionEvent evt)
	{
		setAppState(STARTING);

		reject = txtReject.getText();

		if(server.start(DEF_BIND_ADDRESS, DEF_SERVER_PORT))
		{
			logServerStart(DEF_BIND_ADDRESS, DEF_SERVER_PORT);
			setAppState(STARTED);
		}
		else
		{
			logServerStartFail(server.getLastError(), server.getLastErrorDesc());
			setAppState(STOPPED);
		}
	}

	private void btnStopActionPerformed(java.awt.event.ActionEvent evt)
	{
		setAppState(STOPPING);

		if(server.stop())
		{
			logServerStop();
			setAppState(STOPPED);
		}
		else
			assert false;
	}

	private void btnDisConnActionPerformed(java.awt.event.ActionEvent evt)
	{
		long connID = Helper.str2Long_0(txtDisConn.getText());

		if(connID != 0)
		{
			if(server.disconnect(new NativeLong(connID), true))
				logDisconnect(connID);
			else
				logDisconnectFail(connID);
		}
	}

	private void lsInfoKeyPressed(java.awt.event.KeyEvent evt)
	{
		char c = evt.getKeyChar();
		if(c == 'c' || c == 'C')
			clearInfoList();
	}

	private void formWindowClosing(java.awt.event.WindowEvent evt)
	{
		setMainFrame(null);
		TcpServer.destroy(server);

		System.out.printf("good bye~ %s@%d\n", this.getClass().getName(), hashCode());
	}

	private static class OnPrepareListenImpl extends EventBase<ServerApp> implements OnPrepareListen
	{
		OnPrepareListenImpl(ServerApp app)
		{
			super(app);
		}

		@Override
		public int invoke(Pointer socket)
		{
			SocketAddress addr = app.server.getListenAddress();
			logOnPrepareListen(addr.getAddress(), addr.getPort());

			return HandleResult.HR_OK;
		}
	}

	private static class OnAcceptImpl extends EventBase<ServerApp> implements OnAccept
	{
		OnAcceptImpl(ServerApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, Pointer socket)
		{
			boolean pass = true;
			SocketAddress addr = app.server.getRemoteAddress(dwConnID);

			if(!app.reject.isEmpty())
			{
				if(app.reject.compareToIgnoreCase(addr.getAddress()) == 0)
					pass = false;
			}

			logOnAccept(dwConnID.longValue(), addr.getAddress(), addr.getPort(), pass);

			if(pass)
				app.addMessage(dwConnID);

			return pass ? HandleResult.HR_OK : HandleResult.HR_ERROR;
		}
	}

	private static class OnPullReceiveImpl extends EventBase<ServerApp> implements OnPullReceive
	{
		OnPullReceiveImpl(ServerApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, int iLength)
		{
			Message message = app.getMessage(dwConnID);

			if(message != null)
			{
				int required = message.size;
				int remain = iLength;

				while(remain >= required)
				{
					remain -= required;
					byte[] bytes = new byte[required];

					int result = app.server.fetch(dwConnID, bytes, bytes.length);

					if(result == FetchResult.FR_OK)
					{
						if(message.isHeader)
						{
							Message.Header header = Util.byteArray2Object(bytes);
							System.out.println("[Server] " + header);

							required = header.bodyLength;
						}
						else
						{
							Message.Body body = Util.byteArray2Object(bytes);
							System.out.println("[Server] " + body);

							required = Message.Header.SIZE;
						}

						message.isHeader = !message.isHeader;
						message.size = required;

						logOnReceive(dwConnID.longValue(), bytes.length);

						if(!app.server.send(dwConnID, bytes))
							return HandleResult.HR_ERROR;
					}
				}
			}

			return HandleResult.HR_OK;
		}
	}

	private static class OnSendImpl extends EventBase<ServerApp> implements OnSend
	{
		OnSendImpl(ServerApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, Pointer pData, int iLength)
		{
			logOnSend(dwConnID.longValue(), iLength);

			return HandleResult.HR_OK;
		}
	}

	private static class OnCloseImpl extends EventBase<ServerApp> implements OnClose
	{
		OnCloseImpl(ServerApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID)
		{
			logOnClose(dwConnID.longValue());
			app.removeMessage(dwConnID);

			return HandleResult.HR_OK;
		}
	}

	private static class OnErrorImpl extends EventBase<ServerApp> implements OnError
	{
		OnErrorImpl(ServerApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, int enOperation, int iErrorCode)
		{
			logOnError(dwConnID.longValue(), enOperation, iErrorCode);
			app.removeMessage(dwConnID);

			return HandleResult.HR_OK;
		}
	}

	private static class OnServerShutdownImpl extends EventBase<ServerApp> implements OnServerShutdown
	{
		OnServerShutdownImpl(ServerApp app)
		{
			super(app);
		}

		@Override
		public int invoke()
		{
			logOnShutdown();

			return HandleResult.HR_OK;
		}
	}

	private Message getMessage(NativeLong dwConnID)
	{
		return clients.get(dwConnID);
	}

	private void addMessage(NativeLong dwConnID)
	{
		Helper.syncTryPut(clients, dwConnID, new Message());
	}

	private void removeMessage(NativeLong dwConnID)
	{
		Helper.syncTryRemove(clients, dwConnID);
	}

	//GEN-BEGIN:initComponents
	// <editor-fold defaultstate="collapsed" desc="Generated Code">
	private void initComponents()
	{

		jScrollPane1 = new javax.swing.JScrollPane();
		lsInfo = new javax.swing.JList();
		jLabel1 = new javax.swing.JLabel();
		txtReject = new javax.swing.JTextField();
		jLabel2 = new javax.swing.JLabel();
		txtDisConn = new javax.swing.JTextField();
		btnStop = new javax.swing.JButton();
		btnDisConn = new javax.swing.JButton();
		btnStart = new javax.swing.JButton();

		setDefaultCloseOperation(javax.swing.WindowConstants.EXIT_ON_CLOSE);
		setTitle("Pull Server [ 'C' - clear list box ]");
		setName("frmServer");
		setResizable(false);
		addWindowListener(new java.awt.event.WindowAdapter()
		{
			public void windowClosing(java.awt.event.WindowEvent evt)
			{
				formWindowClosing(evt);
			}
		});

		lsInfo.setFont(new java.awt.Font("新宋体", 0, 11));
		lsInfo.setModel(new DefaultListModel());
		lsInfo.setSelectionMode(javax.swing.ListSelectionModel.SINGLE_SELECTION);
		lsInfo.setName("lsInfo");
		lsInfo.setVisibleRowCount(12);
		lsInfo.addKeyListener(new java.awt.event.KeyAdapter()
		{
			public void keyPressed(java.awt.event.KeyEvent evt)
			{
				lsInfoKeyPressed(evt);
			}
		});
		jScrollPane1.setViewportView(lsInfo);

		jLabel1.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel1.setText("Reject Addr:");

		txtReject.setFont(new java.awt.Font("新宋体", 0, 12));
		txtReject.setName("txtReject");

		jLabel2.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel2.setText("Conn ID:");

		txtDisConn.setFont(new java.awt.Font("新宋体", 0, 12));
		txtDisConn.setName("txtDisConn");

		btnStop.setFont(new java.awt.Font("新宋体", 0, 12));
		btnStop.setText("Stop");
		btnStop.setName("btnStop");
		btnStop.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnStopActionPerformed(evt);
			}
		});

		btnDisConn.setFont(new java.awt.Font("新宋体", 0, 12));
		btnDisConn.setText("Dis Conn");
		btnDisConn.setName("btnDisConn");
		btnDisConn.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnDisConnActionPerformed(evt);
			}
		});

		btnStart.setFont(new java.awt.Font("新宋体", 0, 12));
		btnStart.setText("Start");
		btnStart.setName("btnStart");
		btnStart.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnStartActionPerformed(evt);
			}
		});

		javax.swing.GroupLayout layout = new javax.swing.GroupLayout(getContentPane());
		getContentPane().setLayout(layout);
		layout.setHorizontalGroup(layout
			.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
			.addGroup(
				javax.swing.GroupLayout.Alignment.TRAILING,
				layout.createSequentialGroup().addContainerGap().addComponent(jLabel1)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
					.addComponent(txtReject, javax.swing.GroupLayout.PREFERRED_SIZE, 97, javax.swing.GroupLayout.PREFERRED_SIZE)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(jLabel2)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
					.addComponent(txtDisConn, javax.swing.GroupLayout.PREFERRED_SIZE, 29, javax.swing.GroupLayout.PREFERRED_SIZE)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnDisConn)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, 24, Short.MAX_VALUE).addComponent(btnStart)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
					.addComponent(btnStop, javax.swing.GroupLayout.PREFERRED_SIZE, 69, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap())
			.addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 537, Short.MAX_VALUE));
		layout.setVerticalGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
			.addGroup(
				javax.swing.GroupLayout.Alignment.TRAILING,
				layout
					.createSequentialGroup()
					.addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 275, Short.MAX_VALUE)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED)
					.addGroup(
						layout
							.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE, false)
							.addComponent(jLabel1)
							.addComponent(txtDisConn, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
								javax.swing.GroupLayout.PREFERRED_SIZE)
							.addComponent(txtReject, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
								javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(jLabel2).addComponent(btnDisConn).addComponent(btnStart)
							.addComponent(btnStop, javax.swing.GroupLayout.PREFERRED_SIZE, 25, javax.swing.GroupLayout.PREFERRED_SIZE))
					.addContainerGap()));

		pack();
	}// </editor-fold>
	//GEN-END:initComponents

	/**
	 * @param args the command line arguments
	 * @throws UnsupportedLookAndFeelException 
	 * @throws IllegalAccessException 
	 * @throws InstantiationException 
	 * @throws ClassNotFoundException 
	 */
	public static void main(String args[]) throws Exception
	{
		UIManager.setLookAndFeel(UIManager.getSystemLookAndFeelClassName());
		java.awt.EventQueue.invokeLater(new Runnable()
		{
			public void run()
			{
				new ServerApp().setVisible(true);
			}
		});
	}

	//GEN-BEGIN:variables
	// Variables declaration - do not modify
	private javax.swing.JButton btnDisConn;
	private javax.swing.JButton btnStart;
	private javax.swing.JButton btnStop;
	private javax.swing.JLabel jLabel1;
	private javax.swing.JLabel jLabel2;
	private javax.swing.JScrollPane jScrollPane1;
	private javax.swing.JList lsInfo;
	private javax.swing.JTextField txtDisConn;
	private javax.swing.JTextField txtReject;
	// End of variables declaration//GEN-END:variables

}

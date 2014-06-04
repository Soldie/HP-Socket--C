/*
 * Main.java
 *
 * Created on __DATE__, __TIME__
 */

package pull;

import global.Message;
import global.Message.Body;
import global.Message.Header;
import global.Util;
import global.Util.AppState;
import global.Util.EventBase;

import javax.swing.DefaultListModel;
import javax.swing.UIManager;
import javax.swing.UnsupportedLookAndFeelException;

import org.jessma.hpsocket.Callback.OnClose;
import org.jessma.hpsocket.Callback.OnConnect;
import org.jessma.hpsocket.Callback.OnError;
import org.jessma.hpsocket.Callback.OnPrepareConnect;
import org.jessma.hpsocket.Callback.OnPullReceive;
import org.jessma.hpsocket.Callback.OnSend;
import org.jessma.hpsocket.Constant.FetchResult;
import org.jessma.hpsocket.Constant.HandleResult;
import org.jessma.hpsocket.Constant.SocketError;
import org.jessma.hpsocket.HPSocketObjBase.Mode;
import org.jessma.hpsocket.Helper;
import org.jessma.hpsocket.SocketAddress;
import org.jessma.hpsocket.unicode.TcpClient;

import com.sun.jna.NativeLong;
import com.sun.jna.Pointer;

import static global.Util.clearInfoList;
import static global.Util.getMainFrame;
import static global.Util.logClientStartFail;
import static global.Util.logClientStarting;
import static global.Util.logClientStopping;
import static global.Util.logOnClose;
import static global.Util.logOnConnect;
import static global.Util.logOnError;
import static global.Util.logOnReceive;
import static global.Util.logOnSend;
import static global.Util.logSend;
import static global.Util.logSendFail;
import static global.Util.setInfoList;
import static global.Util.setMainFrame;
import static global.Util.AppState.STARTED;
import static global.Util.AppState.STARTING;
import static global.Util.AppState.STOPPED;
import static global.Util.AppState.STOPPING;

/**
 *
 * @author  __USER__
 */
@SuppressWarnings("serial")
public class ClientApp extends javax.swing.JFrame
{
	private AppState state;
	private Message message = new Message();

	private TcpClient client;

	/** Creates new form Main */
	public ClientApp()
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
		chkAsync.setSelected(true);
		btnStart.requestFocus();

		client = TcpClient.create(Mode.PULL);
		client.setCallBackOnPrepareConnect(new OnPrepareConnectImpl(this));
		client.setCallBackOnConnect(new OnConnectImpl(this));
		client.setCallBackOnSend(new OnSendImpl(this));
		client.setCallBackOnPullReceive(new OnPullReceiveImpl(this));
		client.setCallBackOnClose(new OnCloseImpl(this));
		client.setCallBackOnError(new OnErrorImpl(this));
	}

	private void setAppState(AppState state)
	{
		if(this.state == state)
			return;
		if(getMainFrame() == null)
			return;

		this.state = state;

		chkAsync.setEnabled(state == STOPPED);
		btnStart.setEnabled(state == STOPPED);
		btnStop.setEnabled(state == STARTED);
		btnSend.setEnabled(state == STARTED);
		txtServerAddr.setEnabled(state == STOPPED);
		txtPort.setEnabled(state == STOPPED);
		
		chkAsync.paint(chkAsync.getGraphics());
		btnStart.paint(btnStart.getGraphics());
		btnStop.paint(btnStop.getGraphics());
		btnSend.paint(btnSend.getGraphics());
		txtServerAddr.paint(txtServerAddr.getGraphics());
		txtPort.paint(txtPort.getGraphics());
	}

	private void btnStartActionPerformed(java.awt.event.ActionEvent evt)
	{
		setAppState(STARTING);

		String serverAddr = Helper.safeTrimString(txtServerAddr.getText());
		short port = Helper.str2Short_0(txtPort.getText());
		boolean async = chkAsync.isSelected();

		message.reset();

		logClientStarting(serverAddr, port);

		if(!client.start(serverAddr, port, async))
		{
			logClientStartFail(client.getLastError(), client.getLastErrorDesc());
			setAppState(STOPPED);
		}
	}

	private void btnStopActionPerformed(java.awt.event.ActionEvent evt)
	{
		setAppState(STOPPING);

		logClientStopping(client.getConnectionID().longValue());

		if(!client.stop())
			assert false;
	}

	private void btnSendActionPerformed(java.awt.event.ActionEvent evt)
	{
		String desc		= txtContent.getText();
		byte[] buffer	= Message.toByteArray(new Header(), new Body("伤神小怪兽", 23, desc));
		long connID		= client.getConnectionID().longValue();

		if(client.send(buffer, buffer.length))
			logSend(connID, desc);
		else
			logSendFail(connID, TcpClient.getNativeLastError(), TcpClient.getSocketErrorDesc(SocketError.SE_DATA_SEND));
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
		TcpClient.destroy(client);

		System.out.printf("good bye~ %s@%d\n", this.getClass().getName(), hashCode());
	}

	private static class OnPrepareConnectImpl extends EventBase<ClientApp> implements OnPrepareConnect
	{

		public OnPrepareConnectImpl(ClientApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, Pointer socket)
		{
			return HandleResult.HR_IGNORE;
		}

	}

	private static class OnConnectImpl extends EventBase<ClientApp> implements OnConnect
	{

		public OnConnectImpl(ClientApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID)
		{
			SocketAddress address = app.client.getLocalAddress();

			logOnConnect(dwConnID.longValue(), address.getAddress(), address.getPort());
			app.setAppState(STARTED);

			return HandleResult.HR_OK;
		}

	}

	private static class OnSendImpl extends EventBase<ClientApp> implements OnSend
	{

		public OnSendImpl(ClientApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, Pointer pData, int iLength)
		{
			logOnSend(dwConnID.longValue(), iLength);
			return HandleResult.HR_IGNORE;
		}

	}

	private static class OnPullReceiveImpl extends EventBase<ClientApp> implements OnPullReceive
	{

		public OnPullReceiveImpl(ClientApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, int iLength)
		{
			int required = app.message.size;
			int remain = iLength;

			while(remain >= required)
			{
				remain -= required;
				byte[] bytes = new byte[required];

				int result = app.client.fetch(dwConnID, bytes, bytes.length);

				if(result == FetchResult.FR_OK)
				{
					if(app.message.isHeader)
					{
						Message.Header header = Util.byteArray2Object(bytes);
						System.out.println("[Client] " + header);

						required = header.bodyLength;
					}
					else
					{
						Message.Body body = Util.byteArray2Object(bytes);
						System.out.println("[Client] " + body);

						required = Message.Header.SIZE;
					}

					app.message.isHeader = !app.message.isHeader;
					app.message.size = required;

					logOnReceive(dwConnID.longValue(), bytes.length);
				}
			}

			return HandleResult.HR_OK;
		}

	}

	private static class OnCloseImpl extends EventBase<ClientApp> implements OnClose
	{

		public OnCloseImpl(ClientApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID)
		{
			logOnClose(dwConnID.longValue());
			app.setAppState(STOPPED);

			return HandleResult.HR_OK;
		}

	}

	private static class OnErrorImpl extends EventBase<ClientApp> implements OnError
	{

		public OnErrorImpl(ClientApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, int enOperation, int iErrorCode)
		{
			logOnError(dwConnID.longValue(), enOperation, iErrorCode);
			app.setAppState(STOPPED);

			return HandleResult.HR_OK;
		}

	}

	//GEN-BEGIN:initComponents
	// <editor-fold defaultstate="collapsed" desc="Generated Code">
	private void initComponents()
	{

		jScrollPane1 = new javax.swing.JScrollPane();
		lsInfo = new javax.swing.JList();
		txtContent = new javax.swing.JTextField();
		btnSend = new javax.swing.JButton();
		jLabel1 = new javax.swing.JLabel();
		txtServerAddr = new javax.swing.JTextField();
		jLabel2 = new javax.swing.JLabel();
		txtPort = new javax.swing.JTextField();
		btnStop = new javax.swing.JButton();
		btnStart = new javax.swing.JButton();
		chkAsync = new javax.swing.JCheckBox();

		setDefaultCloseOperation(javax.swing.WindowConstants.EXIT_ON_CLOSE);
		setTitle("Pull Client [ 'C' - clear list box ]");
		setCursor(new java.awt.Cursor(java.awt.Cursor.DEFAULT_CURSOR));
		setName("frmClient");
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
		lsInfo.setVisibleRowCount(12);
		lsInfo.addKeyListener(new java.awt.event.KeyAdapter()
		{
			public void keyPressed(java.awt.event.KeyEvent evt)
			{
				lsInfoKeyPressed(evt);
			}
		});
		jScrollPane1.setViewportView(lsInfo);

		txtContent.setFont(new java.awt.Font("新宋体", 0, 12));
		txtContent.setText("text to be sent");

		btnSend.setFont(new java.awt.Font("新宋体", 0, 12));
		btnSend.setText("Send");
		btnSend.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnSendActionPerformed(evt);
			}
		});

		jLabel1.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel1.setText("Server Addr:");

		txtServerAddr.setFont(new java.awt.Font("新宋体", 0, 12));
		txtServerAddr.setText("127.0.0.1");

		jLabel2.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel2.setText("Port:");

		txtPort.setFont(new java.awt.Font("新宋体", 0, 12));
		txtPort.setText("5555");

		btnStop.setFont(new java.awt.Font("新宋体", 0, 12));
		btnStop.setText("Stop");
		btnStop.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnStopActionPerformed(evt);
			}
		});

		btnStart.setFont(new java.awt.Font("新宋体", 0, 12));
		btnStart.setText("Start");
		btnStart.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnStartActionPerformed(evt);
			}
		});

		chkAsync.setText("Async Connect");

		javax.swing.GroupLayout layout = new javax.swing.GroupLayout(getContentPane());
		getContentPane().setLayout(layout);
		layout.setHorizontalGroup(layout
			.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
			.addGroup(
				layout
					.createSequentialGroup()
					.addContainerGap()
					.addGroup(
						layout
							.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
							.addGroup(
								javax.swing.GroupLayout.Alignment.TRAILING,
								layout.createSequentialGroup().addComponent(txtContent, javax.swing.GroupLayout.DEFAULT_SIZE, 454, Short.MAX_VALUE)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED).addComponent(btnSend).addGap(8, 8, 8))
							.addGroup(
								layout.createSequentialGroup().addComponent(jLabel1)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
									.addComponent(txtServerAddr, javax.swing.GroupLayout.PREFERRED_SIZE, 101, javax.swing.GroupLayout.PREFERRED_SIZE)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(jLabel2)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
									.addComponent(txtPort, javax.swing.GroupLayout.PREFERRED_SIZE, 41, javax.swing.GroupLayout.PREFERRED_SIZE)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(chkAsync)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, 4, Short.MAX_VALUE).addComponent(btnStart)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
									.addComponent(btnStop, javax.swing.GroupLayout.PREFERRED_SIZE, 71, javax.swing.GroupLayout.PREFERRED_SIZE)
									.addContainerGap())))
			.addComponent(jScrollPane1, javax.swing.GroupLayout.Alignment.TRAILING, javax.swing.GroupLayout.DEFAULT_SIZE, 537, Short.MAX_VALUE));
		layout.setVerticalGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(
			javax.swing.GroupLayout.Alignment.TRAILING,
			layout
				.createSequentialGroup()
				.addContainerGap()
				.addGroup(
					layout
						.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
						.addComponent(btnSend)
						.addComponent(txtContent, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
							javax.swing.GroupLayout.PREFERRED_SIZE))
				.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, 10, Short.MAX_VALUE)
				.addComponent(jScrollPane1, javax.swing.GroupLayout.PREFERRED_SIZE, 238, javax.swing.GroupLayout.PREFERRED_SIZE)
				.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
				.addGroup(
					layout
						.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
						.addComponent(jLabel1)
						.addComponent(jLabel2)
						.addComponent(txtPort, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
							javax.swing.GroupLayout.PREFERRED_SIZE)
						.addComponent(btnStop)
						.addComponent(txtServerAddr, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
							javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(btnStart).addComponent(chkAsync)).addGap(6, 6, 6)));

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
			@Override
			public void run()
			{
				new ClientApp().setVisible(true);
			}
		});
	}

	//GEN-BEGIN:variables
	// Variables declaration - do not modify
	private javax.swing.JButton btnSend;
	private javax.swing.JButton btnStart;
	private javax.swing.JButton btnStop;
	private javax.swing.JCheckBox chkAsync;
	private javax.swing.JLabel jLabel1;
	private javax.swing.JLabel jLabel2;
	private javax.swing.JScrollPane jScrollPane1;
	private javax.swing.JList lsInfo;
	private javax.swing.JTextField txtContent;
	private javax.swing.JTextField txtPort;
	private javax.swing.JTextField txtServerAddr;
	// End of variables declaration//GEN-END:variables

}

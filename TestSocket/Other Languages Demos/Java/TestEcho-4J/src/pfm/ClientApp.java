/*
 * Client.java
 *
 * Created on __DATE__, __TIME__
 */

package pfm;

import global.Util.AppState;
import global.Util.EventBase;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicLong;

import javax.swing.DefaultListModel;
import javax.swing.JOptionPane;
import javax.swing.UIManager;
import javax.swing.UnsupportedLookAndFeelException;

import org.jessma.hpsocket.Callback.OnAgentShutdown;
import org.jessma.hpsocket.Callback.OnClose;
import org.jessma.hpsocket.Callback.OnConnect;
import org.jessma.hpsocket.Callback.OnReceive;
import org.jessma.hpsocket.Callback.OnSend;
import org.jessma.hpsocket.Helper;
import org.jessma.hpsocket.Callback.OnError;
import org.jessma.hpsocket.Callback.OnPrepareConnect;
import org.jessma.hpsocket.Constant.HandleResult;
import org.jessma.hpsocket.Constant.SocketError;
import org.jessma.hpsocket.HPSocketObjBase.Mode;
import org.jessma.hpsocket.mbcs.TcpAgent;

import com.sun.jna.NativeLong;
import com.sun.jna.Pointer;
import com.sun.jna.ptr.NativeLongByReference;

import static global.Util.logOnShutdown;

import static global.Util.getMainFrame;
import static global.Util.AppState.STARTED;
import static global.Util.AppState.STOPPED;

import static global.Util.clearInfoList;

import static global.Util.*;
import static global.Util.AppState.*;

/**
 *
 * @author  __USER__
 */
@SuppressWarnings("serial")
public class ClientApp extends javax.swing.JFrame
{
	private AppState state;
	private AtomicLong totalSend = new AtomicLong(0);
	private AtomicLong totalRecv = new AtomicLong(0);
	private long expectRecv;
	private long beginTime;
	private long timeConsuming;

	private TcpAgent agent;

	private List<NativeLong> connIDs = new ArrayList<NativeLong>();

	/** Creates new form Client */
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
		btnStart.requestFocus();

		agent = TcpAgent.create(Mode.PUSH);
		agent.setCallBackOnPrepareConnect(new OnPrepareConnectImpl(this));
		agent.setCallBackOnConnect(new OnConnectImpl(this));
		agent.setCallBackOnSend(new OnSendImpl(this));
		agent.setCallBackOnReceive(new OnReceiveImpl(this));
		agent.setCallBackOnClose(new OnCloseImpl(this));
		agent.setCallBackOnError(new OnErrorImpl(this));
		agent.setCallBackOnAgentShutdown(new OnAgentShutdownImpl(this));
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
		txtServerAddr.setEnabled(state == STOPPED);
		txtPort.setEnabled(state == STOPPED);
		lsTimes.setEnabled(state == STOPPED);
		lsInterval.setEnabled(state == STOPPED);
		lsSockets.setEnabled(state == STOPPED);
		lsLength.setEnabled(state == STOPPED);

		btnStart.paint(btnStart.getGraphics());
		btnStop.paint(btnStop.getGraphics());
		txtServerAddr.paint(txtServerAddr.getGraphics());
		txtPort.paint(txtPort.getGraphics());
		lsTimes.paint(lsTimes.getGraphics());
		lsInterval.paint(lsInterval.getGraphics());
		lsSockets.paint(lsSockets.getGraphics());
		lsLength.paint(lsLength.getGraphics());
	}

	private boolean checkParams(String serverAddr, short port, int times, int interval, int threads, int length)
	{
		boolean isOK = (!serverAddr.isEmpty() && port != 0 && times > 0 && interval >= 0 && threads > 0 && length > 0);
		if(!isOK)
			JOptionPane.showMessageDialog(this, "One or more settings not valid, pls check !", "Params Error", JOptionPane.WARNING_MESSAGE);

		return isOK;
	}

	private void reset(long exp)
	{
		totalSend.set(0);
		totalRecv.set(0);

		beginTime		= 0;
		timeConsuming	= 0;
		expectRecv		= exp;
	}

	private void btnStartActionPerformed(java.awt.event.ActionEvent evt)
	{
		String serverAddr = Helper.safeTrimString(txtServerAddr.getText());
		short port = Helper.str2Short_0(txtPort.getText());
		int times = Helper.str2Int_0(lsTimes.getSelectedItem().toString());
		int interval = Helper.str2Int_0(lsInterval.getSelectedItem().toString());
		int sockets = Helper.str2Int_0(lsSockets.getSelectedItem().toString());
		int length = Helper.str2Int_0(lsLength.getSelectedItem().toString());

		if(!checkParams(serverAddr, port, times, interval, sockets, length))
			return;

		setAppState(STARTING);
		reset((long)times * sockets * length);

		boolean isOK = false;

		if(agent.start(null, false))
		{
			for(int i = 0; i < sockets; i++)
			{
				NativeLongByReference pdwConnID = new NativeLongByReference();
				if(agent.connect(DEF_CONN_ADDRESS, DEF_SERVER_PORT, pdwConnID))
				{
					connIDs.add(pdwConnID.getValue());
					if(i == sockets - 1)
						isOK = true;
				}
				else
				{
					logClientStartFail(TcpAgent.getNativeLastError(), TcpAgent.getSocketErrorDesc(SocketError.SE_CONNECT_SERVER));
					break;
				}
			}
		}
		else
			logClientStartFail(agent.getLastError(), agent.getLastErrorDesc());

		if(!isOK)
		{
			agent.stop();
			setAppState(STOPPED);

			return;
		}

		logClientStart(serverAddr, port);

		long sendDelay = 3;
		logMsgImmediately(" *** willing to send data after %d seconds ...", sendDelay);
		Helper.sleep(sendDelay * 1000);
		logMsgImmediately(" *** Go Now !");

		setAppState(STARTED);

		byte[] sendBuffer = new byte[length];
		beginTime = System.currentTimeMillis();

		LOOP: for(int i = 0; i < times; i++)
		{
			for(int j = 0; j < sockets; j++)
			{
				if(!agent.send(connIDs.get(j), sendBuffer, sendBuffer.length))
				{
					logClientSendFail(i + 1, j + 1, TcpAgent.getNativeLastError(), TcpAgent.getSocketErrorDesc(SocketError.SE_DATA_SEND));
					break LOOP;
				}
			}

			if(interval > 0)
				Helper.sleep(interval);
		}
	}

	private void btnStopActionPerformed(java.awt.event.ActionEvent evt)
	{
		setAppState(STOPPING);

		agent.stop();
		connIDs.clear();

		long ttSend = totalSend.get();
		long ttRecv = totalRecv.get();

		logMsgImmediately(" *** Summary: expect -  %d, send - %d, recv - %d", expectRecv, ttSend, ttRecv);

		if(expectRecv == ttSend && expectRecv == ttRecv)
			logMsgImmediately(" *** Success: time consuming -  %d millisecond !", timeConsuming);
		else
			logMsgImmediately(" *** Fail: manual terminated ? (or data lost)");

		setAppState(STOPPED);
	}

	private void formWindowClosing(java.awt.event.WindowEvent evt)
	{
		setMainFrame(null);
		TcpAgent.destroy(agent);

		System.out.printf("good bye~ %s@%d\n", this.getClass().getName(), hashCode());
	}

	private void lsInfoKeyPressed(java.awt.event.KeyEvent evt)
	{
		char c = evt.getKeyChar();
		if(c == 'c' || c == 'C')
			clearInfoList();
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
			return HandleResult.HR_OK;
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
			logOnConnect2(dwConnID.longValue());
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
			app.totalSend.addAndGet(iLength);
			return HandleResult.HR_OK;
		}

	}

	private static class OnReceiveImpl extends EventBase<ClientApp> implements OnReceive
	{

		public OnReceiveImpl(ClientApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, Pointer pData, int iLength)
		{
			long ttRecv = app.totalRecv.addAndGet(iLength);

			if(ttRecv == app.expectRecv)
			{
				app.timeConsuming = System.currentTimeMillis() - app.beginTime;
				logTimeConsuming(app.timeConsuming);
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
			return HandleResult.HR_OK;
		}

	}

	private static class OnAgentShutdownImpl extends EventBase<ClientApp> implements OnAgentShutdown
	{

		public OnAgentShutdownImpl(ClientApp app)
		{
			super(app);
		}

		@Override
		public int invoke()
		{
			logOnShutdown();
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
		jLabel1 = new javax.swing.JLabel();
		btnStop = new javax.swing.JButton();
		txtServerAddr = new javax.swing.JTextField();
		lsTimes = new javax.swing.JComboBox();
		jLabel2 = new javax.swing.JLabel();
		lsSockets = new javax.swing.JComboBox();
		jLabel3 = new javax.swing.JLabel();
		lsLength = new javax.swing.JComboBox();
		jLabel4 = new javax.swing.JLabel();
		lsInterval = new javax.swing.JComboBox();
		jLabel5 = new javax.swing.JLabel();
		jLabel6 = new javax.swing.JLabel();
		btnStart = new javax.swing.JButton();
		txtPort = new javax.swing.JTextField();

		setDefaultCloseOperation(javax.swing.WindowConstants.EXIT_ON_CLOSE);
		setTitle("PFM Client [ 'C' - clear list box ]");
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
		lsInfo.addKeyListener(new java.awt.event.KeyAdapter()
		{
			public void keyPressed(java.awt.event.KeyEvent evt)
			{
				lsInfoKeyPressed(evt);
			}
		});
		jScrollPane1.setViewportView(lsInfo);

		jLabel1.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel1.setText("Test Times:");

		btnStop.setFont(new java.awt.Font("新宋体", 0, 12));
		btnStop.setText("Stop");
		btnStop.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnStopActionPerformed(evt);
			}
		});

		txtServerAddr.setFont(new java.awt.Font("新宋体", 0, 12));
		txtServerAddr.setText("127.0.0.1");

		lsTimes.setEditable(true);
		lsTimes.setFont(new java.awt.Font("新宋体", 0, 12));
		lsTimes.setMaximumRowCount(9);
		lsTimes.setModel(new javax.swing.DefaultComboBoxModel(new String[] { "1", "5", "10", "30", "50", "100", "300", "500", "1000" }));
		lsTimes.setSelectedIndex(5);

		jLabel2.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel2.setText("Sockets:");

		lsSockets.setEditable(true);
		lsSockets.setFont(new java.awt.Font("新宋体", 0, 12));
		lsSockets.setMaximumRowCount(9);
		lsSockets.setModel(new javax.swing.DefaultComboBoxModel(new String[] { "1", "5", "10", "30", "50", "100", "300", "500", "1000" }));
		lsSockets.setSelectedIndex(5);

		jLabel3.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel3.setText("Length:");

		lsLength.setEditable(true);
		lsLength.setFont(new java.awt.Font("新宋体", 0, 12));
		lsLength.setMaximumRowCount(7);
		lsLength.setModel(new javax.swing.DefaultComboBoxModel(new String[] { "1", "10", "100", "1000", "3000", "5000", "10000" }));
		lsLength.setSelectedIndex(5);

		jLabel4.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel4.setText("Interval:");

		lsInterval.setEditable(true);
		lsInterval.setFont(new java.awt.Font("新宋体", 0, 12));
		lsInterval.setMaximumRowCount(9);
		lsInterval.setModel(new javax.swing.DefaultComboBoxModel(new String[] { "0", "1", "3", "5", "10", "20", "30", "60", "100" }));
		lsInterval.setSelectedIndex(1);

		jLabel5.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel5.setText("Server Addr:");

		jLabel6.setFont(new java.awt.Font("新宋体", 0, 12));
		jLabel6.setText("Port:");

		btnStart.setFont(new java.awt.Font("新宋体", 0, 12));
		btnStart.setText("Start");
		btnStart.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnStartActionPerformed(evt);
			}
		});

		txtPort.setFont(new java.awt.Font("新宋体", 0, 12));
		txtPort.setText("5555");

		javax.swing.GroupLayout layout = new javax.swing.GroupLayout(getContentPane());
		getContentPane().setLayout(layout);
		layout.setHorizontalGroup(layout
			.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
			.addGroup(
				layout.createSequentialGroup().addContainerGap().addComponent(jLabel1)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
					.addComponent(lsTimes, javax.swing.GroupLayout.PREFERRED_SIZE, 72, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(10, 10, 10)
					.addComponent(jLabel2).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
					.addComponent(lsSockets, javax.swing.GroupLayout.PREFERRED_SIZE, 58, javax.swing.GroupLayout.PREFERRED_SIZE)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED).addComponent(jLabel3)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
					.addComponent(lsLength, javax.swing.GroupLayout.PREFERRED_SIZE, 71, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(20, 20, 20)
					.addComponent(jLabel4).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
					.addComponent(lsInterval, javax.swing.GroupLayout.PREFERRED_SIZE, 50, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap())
			.addGroup(
				layout.createSequentialGroup().addContainerGap().addComponent(jLabel5)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
					.addComponent(txtServerAddr, javax.swing.GroupLayout.PREFERRED_SIZE, 98, javax.swing.GroupLayout.PREFERRED_SIZE)
					.addGap(18, 18, 18).addComponent(jLabel6).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
					.addComponent(txtPort, javax.swing.GroupLayout.PREFERRED_SIZE, 47, javax.swing.GroupLayout.PREFERRED_SIZE)
					.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, 91, Short.MAX_VALUE)
					.addComponent(btnStart, javax.swing.GroupLayout.PREFERRED_SIZE, 68, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(18, 18, 18)
					.addComponent(btnStop, javax.swing.GroupLayout.PREFERRED_SIZE, 67, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap())
			.addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 537, Short.MAX_VALUE));
		layout.setVerticalGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(
			layout
				.createSequentialGroup()
				.addContainerGap()
				.addGroup(
					layout
						.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
						.addGroup(
							layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(jLabel1)
								.addComponent(lsTimes, javax.swing.GroupLayout.PREFERRED_SIZE, 23, javax.swing.GroupLayout.PREFERRED_SIZE))
						.addGroup(
							layout
								.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
								.addComponent(jLabel2)
								.addComponent(lsSockets, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
									javax.swing.GroupLayout.PREFERRED_SIZE)
								.addComponent(lsLength, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
									javax.swing.GroupLayout.PREFERRED_SIZE)
								.addComponent(jLabel3)
								.addComponent(lsInterval, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
									javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(jLabel4)))
				.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
				.addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 236, Short.MAX_VALUE)
				.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED)
				.addGroup(
					layout
						.createParallelGroup(javax.swing.GroupLayout.Alignment.TRAILING)
						.addGroup(
							layout
								.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
								.addComponent(jLabel6)
								.addComponent(txtPort, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
									javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(btnStop)
								.addComponent(btnStart, javax.swing.GroupLayout.PREFERRED_SIZE, 25, javax.swing.GroupLayout.PREFERRED_SIZE))
						.addGroup(
							layout
								.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE)
								.addComponent(txtServerAddr, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
									javax.swing.GroupLayout.PREFERRED_SIZE).addComponent(jLabel5))).addContainerGap()));

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
				new ClientApp().setVisible(true);
			}
		});
	}

	//GEN-BEGIN:variables
	// Variables declaration - do not modify
	private javax.swing.JButton btnStart;
	private javax.swing.JButton btnStop;
	private javax.swing.JLabel jLabel1;
	private javax.swing.JLabel jLabel2;
	private javax.swing.JLabel jLabel3;
	private javax.swing.JLabel jLabel4;
	private javax.swing.JLabel jLabel5;
	private javax.swing.JLabel jLabel6;
	private javax.swing.JScrollPane jScrollPane1;
	private javax.swing.JList lsInfo;
	private javax.swing.JComboBox lsInterval;
	private javax.swing.JComboBox lsLength;
	private javax.swing.JComboBox lsSockets;
	private javax.swing.JComboBox lsTimes;
	private javax.swing.JTextField txtPort;
	private javax.swing.JTextField txtServerAddr;
	// End of variables declaration//GEN-END:variables

}

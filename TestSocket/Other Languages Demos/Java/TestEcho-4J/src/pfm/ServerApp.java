/*
 * Server.java
 *
 * Created on __DATE__, __TIME__
 */

package pfm;

import global.Util.AppState;

import java.util.concurrent.atomic.AtomicInteger;
import java.util.concurrent.atomic.AtomicLong;

import javax.swing.DefaultListModel;
import javax.swing.UIManager;
import javax.swing.UnsupportedLookAndFeelException;

import org.jessma.hpsocket.Callback.OnAccept;
import org.jessma.hpsocket.Callback.OnClose;
import org.jessma.hpsocket.Callback.OnError;
import org.jessma.hpsocket.Callback.OnPrepareListen;
import org.jessma.hpsocket.Callback.OnReceive;
import org.jessma.hpsocket.Callback.OnSend;
import org.jessma.hpsocket.Callback.OnServerShutdown;
import org.jessma.hpsocket.Constant.HandleResult;
import org.jessma.hpsocket.HPSocketObjBase.Mode;
import org.jessma.hpsocket.SocketAddress;
import org.jessma.hpsocket.mbcs.TcpServer;

import com.sun.jna.NativeLong;
import com.sun.jna.Pointer;

import static global.Util.*;
import static global.Util.AppState.*;

/**
 *
 * @author  __USER__
 */
@SuppressWarnings("serial")
public class ServerApp extends javax.swing.JFrame
{
	private AppState state;
	private AtomicInteger threadCount = new AtomicInteger(0);
	private AtomicLong totalSend = new AtomicLong(0);
	private AtomicLong totalRecv = new AtomicLong(0);

	private TcpServer server;

	/** Creates new form Server */
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

		server = TcpServer.create(Mode.PUSH);
		server.setCallBackOnPrepareListen(new OnPrepareListenImpl(this));
		server.setCallBackOnAccept(new OnAcceptImpl(this));
		server.setCallBackOnReceive(new OnReceiveImpl(this));
		server.setCallBackOnSend(new OnSendImpl(this));
		server.setCallBackOnClose(new OnCloseImpl(this));
		server.setCallBackOnError(new OnErrorImpl(this));
		server.setCallBackOnServerShutdown(new OnServerShutdownImpl(this));
	}

	private void reset(boolean resetThreadCount)
	{
		if(resetThreadCount)
			threadCount.set(0);

		totalSend.set(0L);
		totalRecv.set(0L);
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

		btnStart.paint(btnStart.getGraphics());
		btnStop.paint(btnStop.getGraphics());
	}

	private void btnStartActionPerformed(java.awt.event.ActionEvent evt)
	{
		setAppState(STARTING);

		reset(true);

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

	private void formWindowClosing(java.awt.event.WindowEvent evt)
	{
		setMainFrame(null);
		TcpServer.destroy(server);

		System.out.printf("good bye~ %s@%d\n", this.getClass().getName(), hashCode());
	}

	private void lsInfoKeyPressed(java.awt.event.KeyEvent evt)
	{
		char c = evt.getKeyChar();
		if(c == 'c' || c == 'C')
			clearInfoList();
	}

	private int statistics()
	{
		if(threadCount.decrementAndGet() == 0)
		{
			logServerStatics(totalSend.get(), totalRecv.get());
		}

		return HandleResult.HR_OK;
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
			if(app.threadCount.getAndIncrement() == 0)
				app.reset(false);

			logOnAccept2(dwConnID.longValue());

			return HandleResult.HR_OK;
		}
	}

	private static class OnReceiveImpl extends EventBase<ServerApp> implements OnReceive
	{
		OnReceiveImpl(ServerApp app)
		{
			super(app);
		}

		@Override
		public int invoke(NativeLong dwConnID, Pointer pData, int iLength)
		{
			app.totalRecv.addAndGet(iLength);

			if(app.server.send(dwConnID, pData, iLength))
				return HandleResult.HR_OK;
			else
				return HandleResult.HR_ERROR;
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
			app.totalSend.addAndGet(iLength);

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
			app.statistics();

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
			app.statistics();

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

	//GEN-BEGIN:initComponents
	// <editor-fold defaultstate="collapsed" desc="Generated Code">
	private void initComponents()
	{

		jScrollPane1 = new javax.swing.JScrollPane();
		lsInfo = new javax.swing.JList();
		btnStop = new javax.swing.JButton();
		btnStart = new javax.swing.JButton();

		setDefaultCloseOperation(javax.swing.WindowConstants.EXIT_ON_CLOSE);
		setTitle("PFM Server [ 'C' - clear list box ]");
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

		javax.swing.GroupLayout layout = new javax.swing.GroupLayout(getContentPane());
		getContentPane().setLayout(layout);
		layout.setHorizontalGroup(layout
			.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING)
			.addGroup(
				javax.swing.GroupLayout.Alignment.TRAILING,
				layout.createSequentialGroup().addContainerGap(375, Short.MAX_VALUE)
					.addComponent(btnStart, javax.swing.GroupLayout.PREFERRED_SIZE, 67, javax.swing.GroupLayout.PREFERRED_SIZE).addGap(18, 18, 18)
					.addComponent(btnStop, javax.swing.GroupLayout.PREFERRED_SIZE, 67, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap())
			.addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 537, Short.MAX_VALUE));
		layout.setVerticalGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(
			javax.swing.GroupLayout.Alignment.TRAILING,
			layout.createSequentialGroup().addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 275, Short.MAX_VALUE)
				.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED)
				.addGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.BASELINE).addComponent(btnStop).addComponent(btnStart))
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
	private javax.swing.JButton btnStart;
	private javax.swing.JButton btnStop;
	private javax.swing.JScrollPane jScrollPane1;
	private javax.swing.JList lsInfo;
	// End of variables declaration//GEN-END:variables
}

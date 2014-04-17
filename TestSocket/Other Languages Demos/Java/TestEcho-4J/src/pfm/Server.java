/*
 * Server.java
 *
 * Created on __DATE__, __TIME__
 */

package pfm;

import global.Util.AppState;

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
import org.jessma.hpsocket.mbcs.TcpServer;

import com.sun.jna.NativeLong;
import com.sun.jna.Pointer;

/**
 *
 * @author  __USER__
 */
public class Server extends javax.swing.JFrame
{
	private static final String ADDRESS = "0.0.0.0";
	private static final short PORT = 5555;

	private AppState state;
	private volatile long totalSend;
	private volatile long totalRecv;
	private int threadCount;

	DefaultListModel lsInfoModel;

	private TcpServer server;

	/** Creates new form Server */
	public Server()
	{
		initComponents();
		afterInitComponents();
	}

	private void afterInitComponents()
	{
		setLocationRelativeTo(null);
		btnStart.requestFocus();
		
		lsInfoModel	= (DefaultListModel)lsInfo.getModel();
		server		= TcpServer.create(Mode.PUSH);
		
		server.setCallBackOnPrepareListen(new OnPrepareListenImpl());
		server.setCallBackOnAccept(new OnAcceptImpl());
		server.setCallBackOnReceive(new OnReceiveImpl());
		server.setCallBackOnSend(new OnSendImpl());
		server.setCallBackOnClose(new OnCloseImpl());
		server.setCallBackOnError(new OnErrorImpl());
		server.setCallBackOnServerShutdown(new OnServerShutdownImpl());

		lsInfoModel.addElement("功能还没实现，亲~");
	}
	
	private void reset(boolean resetThreadCount)
	{
		if(resetThreadCount)
			threadCount = 0;
		
		totalSend = 0;
		totalRecv = 0;
	}

	private void btnStartActionPerformed(java.awt.event.ActionEvent evt)
	{
		// TODO add your handling code here:
	}

	private void btnStopActionPerformed(java.awt.event.ActionEvent evt)
	{
		// TODO add your handling code here:
	}

	private void formWindowClosed(java.awt.event.WindowEvent evt)
	{
		TcpServer.destroy(server);
	}

	private void lsInfoKeyPressed(java.awt.event.KeyEvent evt)
	{
		char c = evt.getKeyChar();
		if(c == 'c' || c == 'c')
			lsInfoModel.removeAllElements();
	}
	
	static class OnPrepareListenImpl implements OnPrepareListen
	{
		@Override
		public int invoke(Pointer arg0)
		{
			return HandleResult.HR_OK;
		}
	}
	
	static class OnAcceptImpl implements OnAccept
	{
		@Override
		public int invoke(NativeLong arg0, Pointer arg1)
		{
			return HandleResult.HR_OK;
		}
	}
	
	static class OnReceiveImpl implements OnReceive
	{
		@Override
		public int invoke(NativeLong arg0, Pointer arg1, int arg2)
		{
			return HandleResult.HR_OK;
		}
	}
	
	static class OnSendImpl implements OnSend
	{
		@Override
		public int invoke(NativeLong arg0, Pointer arg1, int arg2)
		{
			return HandleResult.HR_OK;
		}
	}
	
	static class OnCloseImpl implements OnClose
	{
		@Override
		public int invoke(NativeLong arg0)
		{
			return HandleResult.HR_OK;
		}
	}
	
	static class OnErrorImpl implements OnError
	{
		@Override
		public int invoke(NativeLong arg0, int arg1, int arg2)
		{
			return HandleResult.HR_OK;
		}
	}
	
	static class OnServerShutdownImpl implements OnServerShutdown
	{
		@Override
		public int invoke()
		{
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
			public void windowClosed(java.awt.event.WindowEvent evt)
			{
				formWindowClosed(evt);
			}
		});

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

		btnStop.setText("Stop");
		btnStop.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnStopActionPerformed(evt);
			}
		});

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
				layout.createSequentialGroup().addContainerGap(377, Short.MAX_VALUE).addComponent(btnStart).addGap(18, 18, 18)
					.addComponent(btnStop, javax.swing.GroupLayout.PREFERRED_SIZE, 67, javax.swing.GroupLayout.PREFERRED_SIZE).addContainerGap())
			.addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 537, Short.MAX_VALUE));
		layout.setVerticalGroup(layout.createParallelGroup(javax.swing.GroupLayout.Alignment.LEADING).addGroup(
			javax.swing.GroupLayout.Alignment.TRAILING,
			layout.createSequentialGroup().addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 271, Short.MAX_VALUE)
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
				new Server().setVisible(true);
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

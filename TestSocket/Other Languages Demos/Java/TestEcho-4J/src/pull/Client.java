/*
 * Main.java
 *
 * Created on __DATE__, __TIME__
 */

package pull;

import java.awt.event.KeyEvent;

import javax.swing.DefaultListModel;
import javax.swing.UIManager;
import javax.swing.UnsupportedLookAndFeelException;

/**
 *
 * @author  __USER__
 */
@SuppressWarnings("serial")
public class Client extends javax.swing.JFrame
{

	DefaultListModel lsInfoModel;
	
	/** Creates new form Main */
	public Client()
	{
		initComponents();
		afterInitComponents();
	}

	private void afterInitComponents()
	{
		setLocationRelativeTo(null);
		btnStart.requestFocus();
		lsInfoModel = (DefaultListModel)lsInfo.getModel();
		
		lsInfoModel.addElement("功能还没实现，亲~");
	}

	private void btnStartActionPerformed(java.awt.event.ActionEvent evt)
	{
		// TODO add your handling code here:
	}

	private void btnStopActionPerformed(java.awt.event.ActionEvent evt)
	{
		// TODO add your handling code here:
	}

	private void btnConnectActionPerformed(java.awt.event.ActionEvent evt)
	{
		// TODO add your handling code here:
	}

	private void btnSendActionPerformed(java.awt.event.ActionEvent evt)
	{
		// TODO add your handling code here:
	}

	private void lsInfoKeyPressed(java.awt.event.KeyEvent evt)
	{
		char c = evt.getKeyChar();
		if(c == 'c' || c == 'c')
			lsInfoModel.removeAllElements();
	}

	private void txtContentKeyReleased(java.awt.event.KeyEvent evt)
	{
		// TODO add your handling code here:
	}

	private void formWindowClosed(java.awt.event.WindowEvent evt)
	{
		// TODO add your handling code here:
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
		btnConnect = new javax.swing.JButton();
		btnStart = new javax.swing.JButton();

		setDefaultCloseOperation(javax.swing.WindowConstants.EXIT_ON_CLOSE);
		setTitle("Pull Client [ 'C' - clear list box ]");
		setCursor(new java.awt.Cursor(java.awt.Cursor.DEFAULT_CURSOR));
		setName("frmClient");
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
		lsInfo.setVisibleRowCount(12);
		lsInfo.addKeyListener(new java.awt.event.KeyAdapter()
		{
			public void keyPressed(java.awt.event.KeyEvent evt)
			{
				lsInfoKeyPressed(evt);
			}
		});
		jScrollPane1.setViewportView(lsInfo);

		txtContent.setText("text to be sent");
		txtContent.addKeyListener(new java.awt.event.KeyAdapter()
		{
			public void keyReleased(java.awt.event.KeyEvent evt)
			{
				txtContentKeyReleased(evt);
			}
		});

		btnSend.setText("Send");
		btnSend.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnSendActionPerformed(evt);
			}
		});

		jLabel1.setText("Server Addr:");

		txtServerAddr.setText("127.0.0.1");

		jLabel2.setText("Port:");

		txtPort.setText("5555");

		btnStop.setText("Stop");
		btnStop.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnStopActionPerformed(evt);
			}
		});

		btnConnect.setText("Dis Connect");
		btnConnect.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnConnectActionPerformed(evt);
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
								layout
									.createSequentialGroup()
									.addComponent(jLabel1)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
									.addComponent(txtServerAddr, javax.swing.GroupLayout.PREFERRED_SIZE, 101, javax.swing.GroupLayout.PREFERRED_SIZE)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.UNRELATED)
									.addComponent(jLabel2)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
									.addComponent(txtPort, javax.swing.GroupLayout.PREFERRED_SIZE, 41, javax.swing.GroupLayout.PREFERRED_SIZE)
									.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE,
										Short.MAX_VALUE).addComponent(btnStart).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
									.addComponent(btnConnect).addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED)
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
				.addPreferredGap(javax.swing.LayoutStyle.ComponentPlacement.RELATED, javax.swing.GroupLayout.DEFAULT_SIZE, Short.MAX_VALUE)
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
						.addComponent(btnConnect)
						.addComponent(btnStart)
						.addComponent(txtServerAddr, javax.swing.GroupLayout.PREFERRED_SIZE, javax.swing.GroupLayout.DEFAULT_SIZE,
							javax.swing.GroupLayout.PREFERRED_SIZE)).addGap(6, 6, 6)));

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
				new Client().setVisible(true);
			}
		});
	}

	//GEN-BEGIN:variables
	// Variables declaration - do not modify
	private javax.swing.JButton btnConnect;
	private javax.swing.JButton btnSend;
	private javax.swing.JButton btnStart;
	private javax.swing.JButton btnStop;
	private javax.swing.JLabel jLabel1;
	private javax.swing.JLabel jLabel2;
	private javax.swing.JScrollPane jScrollPane1;
	private javax.swing.JList lsInfo;
	private javax.swing.JTextField txtContent;
	private javax.swing.JTextField txtPort;
	private javax.swing.JTextField txtServerAddr;
	// End of variables declaration//GEN-END:variables

}

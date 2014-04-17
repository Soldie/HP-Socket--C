/*
 * Main.java
 *
 * Created on __DATE__, __TIME__
 */

package pull;

import javax.swing.DefaultListModel;
import javax.swing.UIManager;
import javax.swing.UnsupportedLookAndFeelException;

/**
 *
 * @author  __USER__
 */
public class Server extends javax.swing.JFrame
{

	DefaultListModel lsInfoModel;
	
	/** Creates new form Main */
	public Server()
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

	private void btnDisConnActionPerformed(java.awt.event.ActionEvent evt)
	{
		// TODO add your handling code here:
	}

	private void lsInfoKeyPressed(java.awt.event.KeyEvent evt)
	{
		char c = evt.getKeyChar();
		if(c == 'c' || c == 'c')
			lsInfoModel.removeAllElements();
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
		addWindowListener(new java.awt.event.WindowAdapter()
		{
			public void windowClosed(java.awt.event.WindowEvent evt)
			{
				formWindowClosed(evt);
			}
		});

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

		jLabel1.setText("Reject Addr:");

		txtReject.setName("txtReject");

		jLabel2.setText("Conn ID:");

		txtDisConn.setName("txtDisConn");

		btnStop.setText("Stop");
		btnStop.setName("btnStop");
		btnStop.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnStopActionPerformed(evt);
			}
		});

		btnDisConn.setText("Dis Conn");
		btnDisConn.setName("btnDisConn");
		btnDisConn.addActionListener(new java.awt.event.ActionListener()
		{
			public void actionPerformed(java.awt.event.ActionEvent evt)
			{
				btnDisConnActionPerformed(evt);
			}
		});

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
					.addComponent(jScrollPane1, javax.swing.GroupLayout.DEFAULT_SIZE, 271, Short.MAX_VALUE)
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
				new Server().setVisible(true);
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

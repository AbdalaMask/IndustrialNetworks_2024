using System;
using System.ComponentModel;
using System.Drawing;
using System.IO.Ports;
using System.Linq;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;

namespace NetStudio.IPS.Controls;

[DesignTimeVisible(true)]
public class SerialControl : UserControl
{
	private Channel channel;

	private SerialAdapter adapter;

	private IContainer components;

	private GroupBox gbSerialPort;

	private Label label6;

	private ComboBox cboxHandshake;

	private Label label5;

	private Label label4;

	private ComboBox cboxStopbits;

	private ComboBox cboxParity;

	private ComboBox cboxDatabits;

	private ComboBox cboxBaudRate;

	private ComboBox cboxPortName;

	private Label label3;

	private Label label2;

	private Label label1;

	private GroupBox groupBox2;

	private Label label8;

	private NumericUpDown txtReceivingDelay;

	private NumericUpDown txtReceiveTimeout;

	private Label label10;

	private NumericUpDown txtSendTimeout;

	private Label label9;

	public SerialControl()
	{
		InitializeComponent();
	}

	protected override void OnLoad(EventArgs eventArgs_0)
	{
		base.OnLoad(eventArgs_0);
		try
		{
			cboxPortName.DataSource = SerialPort.GetPortNames();
			ComboBox.ObjectCollection items = cboxBaudRate.Items;
			object[] items2 = new string[15]
			{
				"1200", "2400", "4800", "9600", "14400", "19200", "28800", "38400", "56000", "57600",
				"115200", "230400", "460800", "576000", "921600"
			};
			items.AddRange(items2);
			ComboBox.ObjectCollection items3 = cboxDatabits.Items;
			items2 = new string[2] { "7", "8" };
			items3.AddRange(items2);
			cboxParity.DataSource = Extensions.GetDictionary<Parity>().ToList();
			cboxParity.DisplayMember = "Value";
			cboxParity.ValueMember = "Key";
			cboxStopbits.DataSource = Extensions.GetDictionary<StopBits>().ToList();
			cboxStopbits.DisplayMember = "Value";
			cboxStopbits.ValueMember = "Key";
			cboxHandshake.DataSource = Extensions.GetDictionary<Handshake>().ToList();
			cboxHandshake.DisplayMember = "Value";
			cboxHandshake.ValueMember = "Key";
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void SetChannel(Channel channel)
	{
		try
		{
			this.channel = channel;
			if (this.channel != null)
			{
				if (this.channel.Adapter == null)
				{
					this.channel.Adapter = new SerialAdapter();
				}
				adapter = this.channel.Adapter;
				cboxPortName.DataBindings.Clear();
				cboxPortName.DataBindings.Add("Text", adapter, "PortName");
				cboxBaudRate.DataBindings.Clear();
				cboxBaudRate.DataBindings.Add("Text", adapter, "BaudRate");
				cboxDatabits.DataBindings.Clear();
				cboxDatabits.DataBindings.Add("Text", adapter, "Databits");
				cboxParity.DataBindings.Clear();
				cboxParity.DataBindings.Add("SelectedValue", adapter, "Parity");
				cboxStopbits.DataBindings.Clear();
				cboxStopbits.DataBindings.Add("SelectedValue", adapter, "Stopbits");
				cboxHandshake.DataBindings.Clear();
				cboxHandshake.DataBindings.Add("SelectedValue", adapter, "Handshake");
				txtSendTimeout.DataBindings.Clear();
				txtSendTimeout.DataBindings.Add("Value", adapter, "SendTimeout", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
				txtReceiveTimeout.DataBindings.Clear();
				txtReceiveTimeout.DataBindings.Add("Value", adapter, "ReceiveTimeout", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
				txtReceivingDelay.DataBindings.Clear();
				txtReceivingDelay.DataBindings.Add("Value", adapter, "WaitingTime", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.gbSerialPort = new System.Windows.Forms.GroupBox();
		this.label6 = new System.Windows.Forms.Label();
		this.cboxHandshake = new System.Windows.Forms.ComboBox();
		this.label5 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.cboxStopbits = new System.Windows.Forms.ComboBox();
		this.cboxParity = new System.Windows.Forms.ComboBox();
		this.cboxDatabits = new System.Windows.Forms.ComboBox();
		this.cboxBaudRate = new System.Windows.Forms.ComboBox();
		this.cboxPortName = new System.Windows.Forms.ComboBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.label8 = new System.Windows.Forms.Label();
		this.txtReceivingDelay = new System.Windows.Forms.NumericUpDown();
		this.txtReceiveTimeout = new System.Windows.Forms.NumericUpDown();
		this.label10 = new System.Windows.Forms.Label();
		this.txtSendTimeout = new System.Windows.Forms.NumericUpDown();
		this.label9 = new System.Windows.Forms.Label();
		this.gbSerialPort.SuspendLayout();
		this.groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtReceivingDelay).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtReceiveTimeout).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtSendTimeout).BeginInit();
		base.SuspendLayout();
		this.gbSerialPort.Controls.Add(this.label6);
		this.gbSerialPort.Controls.Add(this.cboxHandshake);
		this.gbSerialPort.Controls.Add(this.label5);
		this.gbSerialPort.Controls.Add(this.label4);
		this.gbSerialPort.Controls.Add(this.cboxStopbits);
		this.gbSerialPort.Controls.Add(this.cboxParity);
		this.gbSerialPort.Controls.Add(this.cboxDatabits);
		this.gbSerialPort.Controls.Add(this.cboxBaudRate);
		this.gbSerialPort.Controls.Add(this.cboxPortName);
		this.gbSerialPort.Controls.Add(this.label3);
		this.gbSerialPort.Controls.Add(this.label2);
		this.gbSerialPort.Controls.Add(this.label1);
		this.gbSerialPort.ForeColor = System.Drawing.Color.Navy;
		this.gbSerialPort.Location = new System.Drawing.Point(9, 6);
		this.gbSerialPort.Name = "gbSerialPort";
		this.gbSerialPort.Size = new System.Drawing.Size(323, 229);
		this.gbSerialPort.TabIndex = 82;
		this.gbSerialPort.TabStop = false;
		this.gbSerialPort.Text = "Serial Port";
		this.label6.AutoSize = true;
		this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label6.Location = new System.Drawing.Point(32, 196);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(69, 15);
		this.label6.TabIndex = 25;
		this.label6.Text = "Handshake:";
		this.cboxHandshake.FormattingEnabled = true;
		this.cboxHandshake.Location = new System.Drawing.Point(128, 193);
		this.cboxHandshake.Name = "cboxHandshake";
		this.cboxHandshake.Size = new System.Drawing.Size(185, 23);
		this.cboxHandshake.TabIndex = 12;
		this.label5.AutoSize = true;
		this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label5.Location = new System.Drawing.Point(48, 163);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(53, 15);
		this.label5.TabIndex = 23;
		this.label5.Text = "Stopbits:";
		this.label4.AutoSize = true;
		this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label4.Location = new System.Drawing.Point(61, 129);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(40, 15);
		this.label4.TabIndex = 22;
		this.label4.Text = "Parity:";
		this.cboxStopbits.FormattingEnabled = true;
		this.cboxStopbits.Location = new System.Drawing.Point(128, 160);
		this.cboxStopbits.Name = "cboxStopbits";
		this.cboxStopbits.Size = new System.Drawing.Size(185, 23);
		this.cboxStopbits.TabIndex = 11;
		this.cboxParity.FormattingEnabled = true;
		this.cboxParity.Location = new System.Drawing.Point(128, 126);
		this.cboxParity.Name = "cboxParity";
		this.cboxParity.Size = new System.Drawing.Size(185, 23);
		this.cboxParity.TabIndex = 10;
		this.cboxDatabits.FormattingEnabled = true;
		this.cboxDatabits.Location = new System.Drawing.Point(128, 93);
		this.cboxDatabits.Name = "cboxDatabits";
		this.cboxDatabits.Size = new System.Drawing.Size(185, 23);
		this.cboxDatabits.TabIndex = 9;
		this.cboxBaudRate.FormattingEnabled = true;
		this.cboxBaudRate.Location = new System.Drawing.Point(128, 59);
		this.cboxBaudRate.Name = "cboxBaudRate";
		this.cboxBaudRate.Size = new System.Drawing.Size(185, 23);
		this.cboxBaudRate.TabIndex = 8;
		this.cboxPortName.FormattingEnabled = true;
		this.cboxPortName.Location = new System.Drawing.Point(128, 26);
		this.cboxPortName.Name = "cboxPortName";
		this.cboxPortName.Size = new System.Drawing.Size(185, 23);
		this.cboxPortName.TabIndex = 7;
		this.label3.AutoSize = true;
		this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label3.Location = new System.Drawing.Point(46, 96);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(53, 15);
		this.label3.TabIndex = 16;
		this.label3.Text = "Databits:";
		this.label2.AutoSize = true;
		this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label2.Location = new System.Drawing.Point(44, 62);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(57, 15);
		this.label2.TabIndex = 16;
		this.label2.Text = "Baudrate:";
		this.label1.AutoSize = true;
		this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label1.Location = new System.Drawing.Point(36, 29);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(65, 15);
		this.label1.TabIndex = 15;
		this.label1.Text = "Port name:";
		this.groupBox2.Controls.Add(this.label8);
		this.groupBox2.Controls.Add(this.txtReceivingDelay);
		this.groupBox2.Controls.Add(this.txtReceiveTimeout);
		this.groupBox2.Controls.Add(this.label10);
		this.groupBox2.Controls.Add(this.txtSendTimeout);
		this.groupBox2.Controls.Add(this.label9);
		this.groupBox2.ForeColor = System.Drawing.Color.Navy;
		this.groupBox2.Location = new System.Drawing.Point(9, 241);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(323, 121);
		this.groupBox2.TabIndex = 84;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Set time: miniseconds";
		this.label8.AutoSize = true;
		this.label8.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label8.Location = new System.Drawing.Point(19, 89);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(81, 15);
		this.label8.TabIndex = 24;
		this.label8.Text = "Receive delay:";
		this.txtReceivingDelay.Location = new System.Drawing.Point(128, 87);
		this.txtReceivingDelay.Maximum = new decimal(new int[4] { 150, 0, 0, 0 });
		this.txtReceivingDelay.Name = "txtReceivingDelay";
		this.txtReceivingDelay.Size = new System.Drawing.Size(185, 23);
		this.txtReceivingDelay.TabIndex = 15;
		this.txtReceiveTimeout.Location = new System.Drawing.Point(128, 56);
		this.txtReceiveTimeout.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.txtReceiveTimeout.Minimum = new decimal(new int[4] { 250, 0, 0, 0 });
		this.txtReceiveTimeout.Name = "txtReceiveTimeout";
		this.txtReceiveTimeout.Size = new System.Drawing.Size(185, 23);
		this.txtReceiveTimeout.TabIndex = 14;
		this.txtReceiveTimeout.Value = new decimal(new int[4] { 250, 0, 0, 0 });
		this.label10.AutoSize = true;
		this.label10.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label10.Location = new System.Drawing.Point(6, 58);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(95, 15);
		this.label10.TabIndex = 22;
		this.label10.Text = "Receive timeout:";
		this.txtSendTimeout.Location = new System.Drawing.Point(128, 24);
		this.txtSendTimeout.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.txtSendTimeout.Minimum = new decimal(new int[4] { 250, 0, 0, 0 });
		this.txtSendTimeout.Name = "txtSendTimeout";
		this.txtSendTimeout.Size = new System.Drawing.Size(185, 23);
		this.txtSendTimeout.TabIndex = 13;
		this.txtSendTimeout.Value = new decimal(new int[4] { 250, 0, 0, 0 });
		this.label9.AutoSize = true;
		this.label9.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label9.Location = new System.Drawing.Point(20, 26);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(81, 15);
		this.label9.TabIndex = 20;
		this.label9.Text = "Send timeout:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.gbSerialPort);
		base.Name = "SerialControl";
		base.Size = new System.Drawing.Size(340, 364);
		this.gbSerialPort.ResumeLayout(false);
		this.gbSerialPort.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtReceivingDelay).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtReceiveTimeout).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtSendTimeout).EndInit();
		base.ResumeLayout(false);
	}
}

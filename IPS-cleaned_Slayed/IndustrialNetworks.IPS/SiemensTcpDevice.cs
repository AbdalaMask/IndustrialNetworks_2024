using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.IPS.Local;
using NetStudio.Siemens.Models;

namespace NetStudio.IPS;

public class SiemensTcpDevice : Form
{
	private Channel channel;

	private Device? device;

	public EventDeviceChanged? OnDeviceChanged;

	private IContainer components;

	private Button btnCancel;

	private Button btnSave;

	private ErrorProvider errorProvider1;

	private GroupBox groupBox1;

	private TextBox txtDescription;

	private GroupBox groupBox2;

	private Label label8;

	private NumericUpDown txtReceivingDelay;

	private NumericUpDown txtReceiveTimeout;

	private Label label10;

	private NumericUpDown txtSendTimeout;

	private Label label9;

	private GroupBox gbEthernet;

	private NumericUpDown txtPort;

	private Label lblPort;

	private TextBox txtIPAddress;

	private Label lblIPAddress;

	private GroupBox gbGeneral;

	private NumericUpDown txtConnectRetries;

	private Label label6;

	private CheckBox chkAutoReconnect;

	private CheckBox chkActive;

	private TextBox txtDeviceName;

	private Label label1;

	private TextBox txtChannel;

	private Label label2;

	private ComboBox cboxDeviceType;

	private Label label4;

	public SiemensTcpDevice(Channel channel, Device? device = null)
	{
		InitializeComponent();
		try
		{
			this.channel = channel;
			txtChannel.Text = channel.Name;
			if (device != null)
			{
				Text = "Edit: Device";
				this.device = (Device)device.Clone();
			}
			else
			{
				Text = "Add new: Device";
				this.device = new Device
				{
					Adapter = new EthernetAdapter()
				};
				this.device.BlockSize = this.device.GetBlockSize(this.channel.Protocol);
			}
			if (channel.ConnectionType == ConnectionType.Serial)
			{
				this.device.Adapter = null;
			}
			else
			{
				this.device.BlockSize = 255;
				this.device.Adapter = this.device.Adapter ?? new EthernetAdapter(ProtocolType.Tcp);
				if ("Add new: Device".Equals(Text))
				{
					this.device.Adapter.Port = 102;
				}
			}
			this.device.ChannelId = channel.Id;
			txtDeviceName.DataBindings.Add("Text", this.device, "Name", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			chkActive.DataBindings.Add("Checked", this.device, "Active", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			chkAutoReconnect.DataBindings.Add("Checked", this.device, "AutoReconnect", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtConnectRetries.DataBindings.Add("Value", this.device, "ConnectRetries", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtDescription.DataBindings.Add("Text", this.device, "Description", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtIPAddress.DataBindings.Clear();
			txtIPAddress.DataBindings.Add("Text", this.device.Adapter, "IPAddress", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtPort.DataBindings.Clear();
			txtPort.DataBindings.Add("Value", this.device.Adapter, "Port", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtSendTimeout.DataBindings.Clear();
			txtSendTimeout.DataBindings.Add("Value", this.device.Adapter, "SendTimeout", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtReceiveTimeout.DataBindings.Clear();
			txtReceiveTimeout.DataBindings.Add("Value", this.device.Adapter, "ReceiveTimeout", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtReceivingDelay.DataBindings.Clear();
			txtReceivingDelay.DataBindings.Add("Value", this.device, "ReceivingDelay", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			cboxDeviceType.DisplayMember = "Value";
			cboxDeviceType.ValueMember = "Key";
			Binding binding = new Binding("SelectedValue", this.device, "DeviceType", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			binding.Format += delegate(object? sender, ConvertEventArgs e)
			{
				if (e.Value != null)
				{
					e.Value = (CPUType)e.Value;
				}
			};
			cboxDeviceType.DataBindings.Add(binding);
			cboxDeviceType.Validating += CboxDeviceType_Validating;
			cboxDeviceType.DataSource = Extensions.GetDictionary<CPUType>().ToList();
			txtDeviceName.KeyDown += OnTextBoxKeyDown;
			txtDescription.KeyDown += OnTextBoxKeyDown;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void FormS7Device_Load(object sender, EventArgs e)
	{
		try
		{
			base.ActiveControl = txtDeviceName;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnSave_Click(object? sender, EventArgs e)
	{
		try
		{
			if (!ValidateInputs())
			{
				return;
			}
			if (device == null)
			{
				throw new Exception("Device is null");
			}
			if (Text.StartsWith("add", StringComparison.OrdinalIgnoreCase))
			{
				EditHelper.AddDevice(device);
				if (OnDeviceChanged != null)
				{
					OnDeviceChanged(device, isAddnew: true);
				}
			}
			else
			{
				EditHelper.EditDevice(device);
				if (OnDeviceChanged != null)
				{
					OnDeviceChanged(device, isAddnew: false);
				}
			}
			base.DialogResult = DialogResult.OK;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnTextBoxKeyDown(object? sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			btnSave_Click(sender, e);
		}
	}

	private void txtDeviceName_Validating(object sender, CancelEventArgs e)
	{
		TextBox textBox = (TextBox)sender;
		if (!string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
		{
			e.Cancel = false;
			errorProvider1.SetError(textBox, null);
			errorProvider1.Clear();
		}
		else
		{
			e.Cancel = true;
			textBox.Focus();
			errorProvider1.SetIconAlignment(txtDeviceName, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtDeviceName, 2);
			errorProvider1.SetError(textBox, "Please enter your device name");
		}
	}

	private void CboxDeviceType_Validating(object? sender, CancelEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		errorProvider1.SetError(comboBox, null);
		if (comboBox.SelectedValue == null)
		{
			e.Cancel = true;
			comboBox.Focus();
			errorProvider1.SetIconAlignment(comboBox, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(comboBox, 5);
			errorProvider1.SetError(comboBox, "Please choose your CPU type");
		}
		else
		{
			e.Cancel = false;
			errorProvider1.SetError(comboBox, null);
			errorProvider1.Clear();
		}
	}

	private bool ValidateInputs()
	{
		errorProvider1.Clear();
		if (string.IsNullOrEmpty(txtIPAddress.Text) || string.IsNullOrWhiteSpace(txtIPAddress.Text))
		{
			errorProvider1.SetIconAlignment(txtIPAddress, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtIPAddress, 4);
			errorProvider1.SetError(txtIPAddress, "Please enter your IP Address");
		}
		if (!Utility.IsIpV4AddressValid(txtIPAddress.Text))
		{
			errorProvider1.SetIconAlignment(txtIPAddress, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtIPAddress, 4);
			errorProvider1.SetError(txtIPAddress, "IP Address: Invalid.");
		}
		if (string.IsNullOrEmpty(txtDeviceName.Text) || string.IsNullOrWhiteSpace(txtDeviceName.Text))
		{
			errorProvider1.SetIconAlignment(txtDeviceName, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtDeviceName, 4);
			errorProvider1.SetError(txtDeviceName, "Please enter your device name");
		}
		if (string.IsNullOrEmpty(cboxDeviceType.Text) || string.IsNullOrWhiteSpace(cboxDeviceType.Text))
		{
			errorProvider1.SetIconAlignment(cboxDeviceType, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(cboxDeviceType, 4);
			errorProvider1.SetError(cboxDeviceType, "Please select your CPU type.");
		}
		return !errorProvider1.HasErrors;
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (Control.ModifierKeys == Keys.None && keyData == Keys.Escape)
		{
			Close();
			return true;
		}
		return base.ProcessDialogKey(keyData);
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
		this.components = new System.ComponentModel.Container();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.SiemensTcpDevice));
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnSave = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.txtDescription = new System.Windows.Forms.TextBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.label8 = new System.Windows.Forms.Label();
		this.txtReceivingDelay = new System.Windows.Forms.NumericUpDown();
		this.txtReceiveTimeout = new System.Windows.Forms.NumericUpDown();
		this.label10 = new System.Windows.Forms.Label();
		this.txtSendTimeout = new System.Windows.Forms.NumericUpDown();
		this.label9 = new System.Windows.Forms.Label();
		this.gbEthernet = new System.Windows.Forms.GroupBox();
		this.txtPort = new System.Windows.Forms.NumericUpDown();
		this.lblPort = new System.Windows.Forms.Label();
		this.txtIPAddress = new System.Windows.Forms.TextBox();
		this.lblIPAddress = new System.Windows.Forms.Label();
		this.gbGeneral = new System.Windows.Forms.GroupBox();
		this.cboxDeviceType = new System.Windows.Forms.ComboBox();
		this.label4 = new System.Windows.Forms.Label();
		this.txtConnectRetries = new System.Windows.Forms.NumericUpDown();
		this.label6 = new System.Windows.Forms.Label();
		this.chkAutoReconnect = new System.Windows.Forms.CheckBox();
		this.chkActive = new System.Windows.Forms.CheckBox();
		this.txtDeviceName = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.txtChannel = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtReceivingDelay).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtReceiveTimeout).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtSendTimeout).BeginInit();
		this.gbEthernet.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtPort).BeginInit();
		this.gbGeneral.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtConnectRetries).BeginInit();
		base.SuspendLayout();
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(338, 425);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(76, 30);
		this.btnCancel.TabIndex = 8;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnSave.Location = new System.Drawing.Point(256, 425);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(76, 30);
		this.btnSave.TabIndex = 7;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.errorProvider1.ContainerControl = this;
		this.groupBox1.Controls.Add(this.txtDescription);
		this.groupBox1.ForeColor = System.Drawing.Color.Navy;
		this.groupBox1.Location = new System.Drawing.Point(9, 156);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(405, 138);
		this.groupBox1.TabIndex = 94;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Description";
		this.txtDescription.Location = new System.Drawing.Point(10, 24);
		this.txtDescription.Multiline = true;
		this.txtDescription.Name = "txtDescription";
		this.txtDescription.Size = new System.Drawing.Size(386, 100);
		this.txtDescription.TabIndex = 7;
		this.groupBox2.Controls.Add(this.label8);
		this.groupBox2.Controls.Add(this.txtReceivingDelay);
		this.groupBox2.Controls.Add(this.txtReceiveTimeout);
		this.groupBox2.Controls.Add(this.label10);
		this.groupBox2.Controls.Add(this.txtSendTimeout);
		this.groupBox2.Controls.Add(this.label9);
		this.groupBox2.ForeColor = System.Drawing.Color.Navy;
		this.groupBox2.Location = new System.Drawing.Point(216, 300);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(198, 119);
		this.groupBox2.TabIndex = 93;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Set time: miniseconds";
		this.label8.AutoSize = true;
		this.label8.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label8.Location = new System.Drawing.Point(16, 90);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(81, 15);
		this.label8.TabIndex = 24;
		this.label8.Text = "Receive delay:";
		this.txtReceivingDelay.Location = new System.Drawing.Point(114, 88);
		this.txtReceivingDelay.Maximum = new decimal(new int[4] { 150, 0, 0, 0 });
		this.txtReceivingDelay.Name = "txtReceivingDelay";
		this.txtReceivingDelay.Size = new System.Drawing.Size(75, 23);
		this.txtReceivingDelay.TabIndex = 12;
		this.txtReceiveTimeout.Location = new System.Drawing.Point(114, 57);
		this.txtReceiveTimeout.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.txtReceiveTimeout.Minimum = new decimal(new int[4] { 250, 0, 0, 0 });
		this.txtReceiveTimeout.Name = "txtReceiveTimeout";
		this.txtReceiveTimeout.Size = new System.Drawing.Size(75, 23);
		this.txtReceiveTimeout.TabIndex = 11;
		this.txtReceiveTimeout.Value = new decimal(new int[4] { 250, 0, 0, 0 });
		this.label10.AutoSize = true;
		this.label10.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label10.Location = new System.Drawing.Point(3, 59);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(95, 15);
		this.label10.TabIndex = 22;
		this.label10.Text = "Receive timeout:";
		this.txtSendTimeout.Location = new System.Drawing.Point(114, 26);
		this.txtSendTimeout.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.txtSendTimeout.Minimum = new decimal(new int[4] { 250, 0, 0, 0 });
		this.txtSendTimeout.Name = "txtSendTimeout";
		this.txtSendTimeout.Size = new System.Drawing.Size(75, 23);
		this.txtSendTimeout.TabIndex = 10;
		this.txtSendTimeout.Value = new decimal(new int[4] { 250, 0, 0, 0 });
		this.label9.AutoSize = true;
		this.label9.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label9.Location = new System.Drawing.Point(17, 28);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(81, 15);
		this.label9.TabIndex = 20;
		this.label9.Text = "Send timeout:";
		this.gbEthernet.Controls.Add(this.txtPort);
		this.gbEthernet.Controls.Add(this.lblPort);
		this.gbEthernet.Controls.Add(this.txtIPAddress);
		this.gbEthernet.Controls.Add(this.lblIPAddress);
		this.gbEthernet.ForeColor = System.Drawing.Color.Navy;
		this.gbEthernet.Location = new System.Drawing.Point(9, 300);
		this.gbEthernet.Name = "gbEthernet";
		this.gbEthernet.Size = new System.Drawing.Size(201, 119);
		this.gbEthernet.TabIndex = 92;
		this.gbEthernet.TabStop = false;
		this.gbEthernet.Text = "Ethernet";
		this.txtPort.Location = new System.Drawing.Point(79, 53);
		this.txtPort.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.txtPort.Name = "txtPort";
		this.txtPort.Size = new System.Drawing.Size(113, 23);
		this.txtPort.TabIndex = 9;
		this.txtPort.Value = new decimal(new int[4] { 502, 0, 0, 0 });
		this.lblPort.AutoSize = true;
		this.lblPort.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblPort.Location = new System.Drawing.Point(41, 57);
		this.lblPort.Name = "lblPort";
		this.lblPort.Size = new System.Drawing.Size(32, 15);
		this.lblPort.TabIndex = 16;
		this.lblPort.Text = "Port:";
		this.txtIPAddress.Location = new System.Drawing.Point(79, 20);
		this.txtIPAddress.Name = "txtIPAddress";
		this.txtIPAddress.Size = new System.Drawing.Size(113, 23);
		this.txtIPAddress.TabIndex = 8;
		this.txtIPAddress.Text = "127.0.0.1";
		this.lblIPAddress.AutoSize = true;
		this.lblIPAddress.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblIPAddress.Location = new System.Drawing.Point(8, 24);
		this.lblIPAddress.Name = "lblIPAddress";
		this.lblIPAddress.Size = new System.Drawing.Size(65, 15);
		this.lblIPAddress.TabIndex = 14;
		this.lblIPAddress.Text = "IP Address:";
		this.gbGeneral.Controls.Add(this.cboxDeviceType);
		this.gbGeneral.Controls.Add(this.label4);
		this.gbGeneral.Controls.Add(this.txtConnectRetries);
		this.gbGeneral.Controls.Add(this.label6);
		this.gbGeneral.Controls.Add(this.chkAutoReconnect);
		this.gbGeneral.Controls.Add(this.chkActive);
		this.gbGeneral.Controls.Add(this.txtDeviceName);
		this.gbGeneral.Controls.Add(this.label1);
		this.gbGeneral.Controls.Add(this.txtChannel);
		this.gbGeneral.Controls.Add(this.label2);
		this.gbGeneral.ForeColor = System.Drawing.Color.Navy;
		this.gbGeneral.Location = new System.Drawing.Point(9, 3);
		this.gbGeneral.Name = "gbGeneral";
		this.gbGeneral.Size = new System.Drawing.Size(405, 147);
		this.gbGeneral.TabIndex = 91;
		this.gbGeneral.TabStop = false;
		this.gbGeneral.Text = "General";
		this.cboxDeviceType.FormattingEnabled = true;
		this.cboxDeviceType.Location = new System.Drawing.Point(106, 86);
		this.cboxDeviceType.Name = "cboxDeviceType";
		this.cboxDeviceType.Size = new System.Drawing.Size(100, 23);
		this.cboxDeviceType.TabIndex = 2;
		this.label4.AutoSize = true;
		this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label4.Location = new System.Drawing.Point(13, 90);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(60, 15);
		this.label4.TabIndex = 100;
		this.label4.Text = "CPU Type:";
		this.txtConnectRetries.Location = new System.Drawing.Point(326, 86);
		this.txtConnectRetries.Maximum = new decimal(new int[4] { 3600, 0, 0, 0 });
		this.txtConnectRetries.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.txtConnectRetries.Name = "txtConnectRetries";
		this.txtConnectRetries.Size = new System.Drawing.Size(70, 23);
		this.txtConnectRetries.TabIndex = 4;
		this.txtConnectRetries.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.label6.AutoSize = true;
		this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label6.Location = new System.Drawing.Point(227, 90);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(96, 15);
		this.label6.TabIndex = 99;
		this.label6.Text = "Connect retries:";
		this.chkAutoReconnect.AutoSize = true;
		this.chkAutoReconnect.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkAutoReconnect.Location = new System.Drawing.Point(229, 119);
		this.chkAutoReconnect.Name = "chkAutoReconnect";
		this.chkAutoReconnect.Size = new System.Drawing.Size(108, 19);
		this.chkAutoReconnect.TabIndex = 6;
		this.chkAutoReconnect.Text = "Auto reconnect";
		this.chkAutoReconnect.UseVisualStyleBackColor = true;
		this.chkActive.AutoSize = true;
		this.chkActive.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkActive.Location = new System.Drawing.Point(106, 119);
		this.chkActive.Name = "chkActive";
		this.chkActive.Size = new System.Drawing.Size(59, 19);
		this.chkActive.TabIndex = 5;
		this.chkActive.Text = "Active";
		this.chkActive.UseVisualStyleBackColor = true;
		this.txtDeviceName.Location = new System.Drawing.Point(106, 50);
		this.txtDeviceName.Name = "txtDeviceName";
		this.txtDeviceName.Size = new System.Drawing.Size(290, 23);
		this.txtDeviceName.TabIndex = 1;
		this.label1.AutoSize = true;
		this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label1.Location = new System.Drawing.Point(4, 21);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(87, 15);
		this.label1.TabIndex = 0;
		this.label1.Text = "Channel name:";
		this.txtChannel.Location = new System.Drawing.Point(106, 18);
		this.txtChannel.Name = "txtChannel";
		this.txtChannel.ReadOnly = true;
		this.txtChannel.Size = new System.Drawing.Size(290, 23);
		this.txtChannel.TabIndex = 9;
		this.label2.AutoSize = true;
		this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label2.Location = new System.Drawing.Point(4, 54);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(78, 15);
		this.label2.TabIndex = 2;
		this.label2.Text = "Device name:";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(424, 464);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.gbEthernet);
		base.Controls.Add(this.gbGeneral);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSave);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "SiemensTcpDevice";
		this.Text = "Device";
		base.Load += new System.EventHandler(FormS7Device_Load);
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtReceivingDelay).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtReceiveTimeout).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtSendTimeout).EndInit();
		this.gbEthernet.ResumeLayout(false);
		this.gbEthernet.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtPort).EndInit();
		this.gbGeneral.ResumeLayout(false);
		this.gbGeneral.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtConnectRetries).EndInit();
		base.ResumeLayout(false);
	}
}

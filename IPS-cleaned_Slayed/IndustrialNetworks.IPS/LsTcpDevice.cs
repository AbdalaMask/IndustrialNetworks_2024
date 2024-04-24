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
using NetStudio.LS.Xgt;

namespace NetStudio.IPS;

public class LsTcpDevice : Form
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

	private Label label3;

	private NumericUpDown txtStationNo;

	private NumericUpDown txtPort;

	private ComboBox cboxCompanyID;

	private Label label4;

	private NumericUpDown txtSlotNo;

	private Label label5;

	private NumericUpDown txtBaseNo;

	private Label label7;

	private ComboBox cboxCpuSeries;

	private Label label11;

	public LsTcpDevice(Channel channel, Device? device = null)
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
				this.device.Adapter = this.device.Adapter ?? new EthernetAdapter(ProtocolType.Tcp);
				if (this.device.Adapter.Port == 0)
				{
					this.device.Adapter.Port = 2004;
				}
				if (this.device.Adapter.Port == 2004)
				{
					this.device.Adapter.ProtocolType = ProtocolType.Tcp;
				}
				else if (this.device.Adapter.Port == 2005)
				{
					this.device.Adapter.ProtocolType = ProtocolType.Udp;
				}
			}
			this.device.ChannelId = channel.Id;
			txtDeviceName.DataBindings.Add("Text", this.device, "Name", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtStationNo.DataBindings.Add("Value", this.device, "StationNo", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtBaseNo.DataBindings.Add("Value", this.device, "BaseNo", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtSlotNo.DataBindings.Add("Value", this.device, "SlotNo", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			chkActive.DataBindings.Add("Checked", this.device, "Active", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			chkAutoReconnect.DataBindings.Add("Checked", this.device, "AutoReconnect", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtConnectRetries.DataBindings.Add("Value", this.device, "ConnectRetries", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtDescription.DataBindings.Add("Text", this.device, "Description", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtIPAddress.DataBindings.Add("Text", this.device.Adapter, "IPAddress", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtPort.DataBindings.Add("Value", this.device.Adapter, "Port", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtSendTimeout.DataBindings.Add("Value", this.device.Adapter, "SendTimeout", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtReceiveTimeout.DataBindings.Add("Value", this.device.Adapter, "ReceiveTimeout", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtReceivingDelay.DataBindings.Add("Value", this.device, "ReceivingDelay", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			cboxCompanyID.DisplayMember = "Value";
			cboxCompanyID.ValueMember = "Key";
			cboxCompanyID.DataSource = Extensions.GetDictionary<CompanyID>().ToList();
			Binding binding = new Binding("SelectedValue", this.device, "CompanyId", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			binding.Format += delegate(object? sender, ConvertEventArgs e)
			{
				if (e.Value != null)
				{
					e.Value = (CompanyID)e.Value;
				}
			};
			cboxCompanyID.DataBindings.Add(binding);
			cboxCompanyID.Validating += CboxCompanyId_Validating;
			cboxCpuSeries.DisplayMember = "Value";
			cboxCpuSeries.ValueMember = "Key";
			cboxCpuSeries.DataSource = Extensions.GetDictionary<CpuSeries>().ToList();
			Binding binding2 = new Binding("SelectedValue", this.device, "DeviceType", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			binding2.Format += delegate(object? sender, ConvertEventArgs e)
			{
				if (e.Value != null)
				{
					e.Value = (CpuSeries)e.Value;
				}
			};
			cboxCpuSeries.DataBindings.Add(binding2);
			cboxCpuSeries.Validating += CboxCpuSeries_Validating;
			txtDeviceName.KeyDown += OnTextBoxKeyDown;
			txtStationNo.KeyDown += OnTextBoxKeyDown;
			txtDescription.KeyDown += OnTextBoxKeyDown;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void CboxCompanyId_Validating(object? sender, CancelEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		errorProvider1.SetError(comboBox, null);
		if (comboBox.SelectedValue == null)
		{
			e.Cancel = true;
			comboBox.Focus();
			errorProvider1.SetIconAlignment(comboBox, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(comboBox, 5);
			errorProvider1.SetError(comboBox, "Please choose Company Id");
		}
		else
		{
			e.Cancel = false;
			errorProvider1.SetError(comboBox, null);
			errorProvider1.Clear();
		}
	}

	private void CboxCpuSeries_Validating(object? sender, CancelEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		errorProvider1.SetError(comboBox, null);
		if (comboBox.SelectedValue == null)
		{
			e.Cancel = true;
			comboBox.Focus();
			errorProvider1.SetIconAlignment(comboBox, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(comboBox, 5);
			errorProvider1.SetError(comboBox, "Please choose CPU Series");
		}
		else
		{
			e.Cancel = false;
			errorProvider1.SetError(comboBox, null);
			errorProvider1.Clear();
		}
	}

	private void FormVigorDevice_Load(object sender, EventArgs e)
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
			errorProvider1.Clear();
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

	private bool ValidateInputs()
	{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.LsTcpDevice));
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
		this.cboxCpuSeries = new System.Windows.Forms.ComboBox();
		this.label11 = new System.Windows.Forms.Label();
		this.txtBaseNo = new System.Windows.Forms.NumericUpDown();
		this.label7 = new System.Windows.Forms.Label();
		this.txtSlotNo = new System.Windows.Forms.NumericUpDown();
		this.cboxCompanyID = new System.Windows.Forms.ComboBox();
		this.txtConnectRetries = new System.Windows.Forms.NumericUpDown();
		this.label6 = new System.Windows.Forms.Label();
		this.chkAutoReconnect = new System.Windows.Forms.CheckBox();
		this.chkActive = new System.Windows.Forms.CheckBox();
		this.txtDeviceName = new System.Windows.Forms.TextBox();
		this.label1 = new System.Windows.Forms.Label();
		this.txtChannel = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label3 = new System.Windows.Forms.Label();
		this.txtStationNo = new System.Windows.Forms.NumericUpDown();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		this.groupBox1.SuspendLayout();
		this.groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtReceivingDelay).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtReceiveTimeout).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtSendTimeout).BeginInit();
		this.gbEthernet.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtPort).BeginInit();
		this.gbGeneral.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtBaseNo).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtSlotNo).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtConnectRetries).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtStationNo).BeginInit();
		base.SuspendLayout();
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(322, 485);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(76, 30);
		this.btnCancel.TabIndex = 16;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnSave.Location = new System.Drawing.Point(240, 485);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(76, 30);
		this.btnSave.TabIndex = 15;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.errorProvider1.ContainerControl = this;
		this.groupBox1.Controls.Add(this.txtDescription);
		this.groupBox1.ForeColor = System.Drawing.Color.Navy;
		this.groupBox1.Location = new System.Drawing.Point(9, 218);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(389, 133);
		this.groupBox1.TabIndex = 90;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Description";
		this.txtDescription.Location = new System.Drawing.Point(10, 24);
		this.txtDescription.Multiline = true;
		this.txtDescription.Name = "txtDescription";
		this.txtDescription.Size = new System.Drawing.Size(367, 100);
		this.txtDescription.TabIndex = 9;
		this.groupBox2.Controls.Add(this.label8);
		this.groupBox2.Controls.Add(this.txtReceivingDelay);
		this.groupBox2.Controls.Add(this.txtReceiveTimeout);
		this.groupBox2.Controls.Add(this.label10);
		this.groupBox2.Controls.Add(this.txtSendTimeout);
		this.groupBox2.Controls.Add(this.label9);
		this.groupBox2.ForeColor = System.Drawing.Color.Navy;
		this.groupBox2.Location = new System.Drawing.Point(211, 360);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(187, 119);
		this.groupBox2.TabIndex = 89;
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
		this.txtReceivingDelay.Size = new System.Drawing.Size(63, 23);
		this.txtReceivingDelay.TabIndex = 14;
		this.txtReceiveTimeout.Location = new System.Drawing.Point(114, 57);
		this.txtReceiveTimeout.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.txtReceiveTimeout.Minimum = new decimal(new int[4] { 250, 0, 0, 0 });
		this.txtReceiveTimeout.Name = "txtReceiveTimeout";
		this.txtReceiveTimeout.Size = new System.Drawing.Size(63, 23);
		this.txtReceiveTimeout.TabIndex = 13;
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
		this.txtSendTimeout.Size = new System.Drawing.Size(63, 23);
		this.txtSendTimeout.TabIndex = 12;
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
		this.gbEthernet.Location = new System.Drawing.Point(9, 360);
		this.gbEthernet.Name = "gbEthernet";
		this.gbEthernet.Size = new System.Drawing.Size(196, 119);
		this.gbEthernet.TabIndex = 88;
		this.gbEthernet.TabStop = false;
		this.gbEthernet.Text = "Ethernet";
		this.txtPort.Location = new System.Drawing.Point(74, 58);
		this.txtPort.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.txtPort.Name = "txtPort";
		this.txtPort.Size = new System.Drawing.Size(109, 23);
		this.txtPort.TabIndex = 11;
		this.txtPort.Value = new decimal(new int[4] { 502, 0, 0, 0 });
		this.lblPort.AutoSize = true;
		this.lblPort.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblPort.Location = new System.Drawing.Point(36, 60);
		this.lblPort.Name = "lblPort";
		this.lblPort.Size = new System.Drawing.Size(32, 15);
		this.lblPort.TabIndex = 16;
		this.lblPort.Text = "Port:";
		this.txtIPAddress.Location = new System.Drawing.Point(76, 26);
		this.txtIPAddress.Name = "txtIPAddress";
		this.txtIPAddress.Size = new System.Drawing.Size(109, 23);
		this.txtIPAddress.TabIndex = 10;
		this.txtIPAddress.Text = "127.0.0.1";
		this.lblIPAddress.AutoSize = true;
		this.lblIPAddress.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblIPAddress.Location = new System.Drawing.Point(5, 27);
		this.lblIPAddress.Name = "lblIPAddress";
		this.lblIPAddress.Size = new System.Drawing.Size(65, 15);
		this.lblIPAddress.TabIndex = 14;
		this.lblIPAddress.Text = "IP Address:";
		this.gbGeneral.Controls.Add(this.cboxCpuSeries);
		this.gbGeneral.Controls.Add(this.label11);
		this.gbGeneral.Controls.Add(this.txtBaseNo);
		this.gbGeneral.Controls.Add(this.label7);
		this.gbGeneral.Controls.Add(this.txtSlotNo);
		this.gbGeneral.Controls.Add(this.cboxCompanyID);
		this.gbGeneral.Controls.Add(this.txtConnectRetries);
		this.gbGeneral.Controls.Add(this.label6);
		this.gbGeneral.Controls.Add(this.chkAutoReconnect);
		this.gbGeneral.Controls.Add(this.chkActive);
		this.gbGeneral.Controls.Add(this.txtDeviceName);
		this.gbGeneral.Controls.Add(this.label1);
		this.gbGeneral.Controls.Add(this.txtChannel);
		this.gbGeneral.Controls.Add(this.label2);
		this.gbGeneral.Controls.Add(this.label5);
		this.gbGeneral.Controls.Add(this.label4);
		this.gbGeneral.Controls.Add(this.label3);
		this.gbGeneral.Controls.Add(this.txtStationNo);
		this.gbGeneral.ForeColor = System.Drawing.Color.Navy;
		this.gbGeneral.Location = new System.Drawing.Point(9, 4);
		this.gbGeneral.Name = "gbGeneral";
		this.gbGeneral.Size = new System.Drawing.Size(389, 208);
		this.gbGeneral.TabIndex = 87;
		this.gbGeneral.TabStop = false;
		this.gbGeneral.Text = "General";
		this.cboxCpuSeries.FormattingEnabled = true;
		this.cboxCpuSeries.Location = new System.Drawing.Point(290, 82);
		this.cboxCpuSeries.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
		this.cboxCpuSeries.Name = "cboxCpuSeries";
		this.cboxCpuSeries.Size = new System.Drawing.Size(87, 23);
		this.cboxCpuSeries.TabIndex = 3;
		this.label11.AutoSize = true;
		this.label11.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label11.Location = new System.Drawing.Point(220, 86);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(66, 15);
		this.label11.TabIndex = 978;
		this.label11.Text = "CPU Series:";
		this.txtBaseNo.Location = new System.Drawing.Point(109, 115);
		this.txtBaseNo.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
		this.txtBaseNo.Maximum = new decimal(new int[4] { 15, 0, 0, 0 });
		this.txtBaseNo.Name = "txtBaseNo";
		this.txtBaseNo.Size = new System.Drawing.Size(92, 23);
		this.txtBaseNo.TabIndex = 4;
		this.label7.AutoSize = true;
		this.label7.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label7.Location = new System.Drawing.Point(4, 119);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(53, 15);
		this.label7.TabIndex = 103;
		this.label7.Text = "Base No:";
		this.txtSlotNo.Location = new System.Drawing.Point(290, 115);
		this.txtSlotNo.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
		this.txtSlotNo.Maximum = new decimal(new int[4] { 15, 0, 0, 0 });
		this.txtSlotNo.Name = "txtSlotNo";
		this.txtSlotNo.Size = new System.Drawing.Size(88, 23);
		this.txtSlotNo.TabIndex = 5;
		this.cboxCompanyID.FormattingEnabled = true;
		this.cboxCompanyID.Location = new System.Drawing.Point(109, 82);
		this.cboxCompanyID.Margin = new System.Windows.Forms.Padding(2, 1, 2, 1);
		this.cboxCompanyID.Name = "cboxCompanyID";
		this.cboxCompanyID.Size = new System.Drawing.Size(92, 23);
		this.cboxCompanyID.TabIndex = 2;
		this.txtConnectRetries.Location = new System.Drawing.Point(109, 148);
		this.txtConnectRetries.Maximum = new decimal(new int[4] { 3600, 0, 0, 0 });
		this.txtConnectRetries.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.txtConnectRetries.Name = "txtConnectRetries";
		this.txtConnectRetries.Size = new System.Drawing.Size(92, 23);
		this.txtConnectRetries.TabIndex = 6;
		this.txtConnectRetries.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.label6.AutoSize = true;
		this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label6.Location = new System.Drawing.Point(4, 152);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(90, 15);
		this.label6.TabIndex = 99;
		this.label6.Text = "Connect retries:";
		this.chkAutoReconnect.AutoSize = true;
		this.chkAutoReconnect.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkAutoReconnect.Location = new System.Drawing.Point(109, 180);
		this.chkAutoReconnect.Name = "chkAutoReconnect";
		this.chkAutoReconnect.Size = new System.Drawing.Size(108, 19);
		this.chkAutoReconnect.TabIndex = 8;
		this.chkAutoReconnect.Text = "Auto reconnect";
		this.chkAutoReconnect.UseVisualStyleBackColor = true;
		this.chkActive.AutoSize = true;
		this.chkActive.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkActive.Location = new System.Drawing.Point(290, 180);
		this.chkActive.Name = "chkActive";
		this.chkActive.Size = new System.Drawing.Size(59, 19);
		this.chkActive.TabIndex = 9;
		this.chkActive.Text = "Active";
		this.chkActive.UseVisualStyleBackColor = true;
		this.txtDeviceName.Location = new System.Drawing.Point(109, 50);
		this.txtDeviceName.Name = "txtDeviceName";
		this.txtDeviceName.Size = new System.Drawing.Size(268, 23);
		this.txtDeviceName.TabIndex = 1;
		this.label1.AutoSize = true;
		this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label1.Location = new System.Drawing.Point(4, 21);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(87, 15);
		this.label1.TabIndex = 0;
		this.label1.Text = "Channel name:";
		this.txtChannel.Location = new System.Drawing.Point(109, 20);
		this.txtChannel.Name = "txtChannel";
		this.txtChannel.ReadOnly = true;
		this.txtChannel.Size = new System.Drawing.Size(268, 23);
		this.txtChannel.TabIndex = 9;
		this.label2.AutoSize = true;
		this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label2.Location = new System.Drawing.Point(4, 52);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(78, 15);
		this.label2.TabIndex = 2;
		this.label2.Text = "Device name:";
		this.label5.AutoSize = true;
		this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label5.Location = new System.Drawing.Point(233, 119);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(49, 15);
		this.label5.TabIndex = 89;
		this.label5.Text = "Slot No:";
		this.label4.AutoSize = true;
		this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label4.Location = new System.Drawing.Point(4, 86);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(75, 15);
		this.label4.TabIndex = 89;
		this.label4.Text = "Company Id:";
		this.label3.AutoSize = true;
		this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label3.Location = new System.Drawing.Point(216, 152);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(66, 15);
		this.label3.TabIndex = 89;
		this.label3.Text = "Station No:";
		this.txtStationNo.Location = new System.Drawing.Point(290, 148);
		this.txtStationNo.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.txtStationNo.Name = "txtStationNo";
		this.txtStationNo.Size = new System.Drawing.Size(87, 23);
		this.txtStationNo.TabIndex = 27;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(408, 524);
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
		base.Name = "LsTcpDevice";
		this.Text = "Device";
		base.Load += new System.EventHandler(FormVigorDevice_Load);
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
		((System.ComponentModel.ISupportInitialize)this.txtBaseNo).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtSlotNo).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtConnectRetries).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtStationNo).EndInit();
		base.ResumeLayout(false);
	}
}

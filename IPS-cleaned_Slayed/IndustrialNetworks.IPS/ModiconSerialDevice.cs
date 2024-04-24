using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.DataTypes;
using NetStudio.Common.Manager;
using NetStudio.IPS.Local;

namespace NetStudio.IPS;

public class ModiconSerialDevice : Form
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

	private GroupBox gbGeneral;

	private NumericUpDown txtConnectRetries;

	private Label label6;

	private CheckBox chkAutoReconnect;

	private CheckBox chkActive;

	private NumericUpDown txtBaseAddress;

	private TextBox txtDeviceName;

	private Label label5;

	private Label label1;

	private ComboBox cboxByteOrder;

	private TextBox txtChannel;

	private Label lblProtocol;

	private Label label2;

	private NumericUpDown txtBlockSize;

	private Label label3;

	private Label label4;

	private NumericUpDown txtStationNo;

	public ModiconSerialDevice(Channel channel, Device? device = null)
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
				this.device = new Device();
				this.device.BlockSize = this.device.GetBlockSize(this.channel.Protocol);
			}
			this.device.ChannelId = channel.Id;
			txtDeviceName.DataBindings.Add("Text", this.device, "Name", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtStationNo.DataBindings.Add("Value", this.device, "StationNo", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtConnectRetries.DataBindings.Add("Value", this.device, "ConnectRetries", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			chkActive.DataBindings.Add("Checked", this.device, "Active", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			chkAutoReconnect.DataBindings.Add("Checked", this.device, "AutoReconnect", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtDescription.DataBindings.Add("Text", this.device, "Description", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtBaseAddress.DataBindings.Add("Value", this.device, "BaseAddress", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtBlockSize.DataBindings.Add("Value", this.device, "BlockSize", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			cboxByteOrder.DataBindings.Clear();
			cboxByteOrder.DataSource = Extensions.GetDictionary<ByteOrder>().ToList();
			cboxByteOrder.DataBindings.Add("SelectedValue", this.device, "ByteOrder");
			cboxByteOrder.DisplayMember = "Value";
			cboxByteOrder.ValueMember = "Key";
			txtDeviceName.KeyDown += OnTextBoxKeyDown;
			txtStationNo.KeyDown += OnTextBoxKeyDown;
			txtBlockSize.KeyDown += OnTextBoxKeyDown;
			txtDescription.KeyDown += OnTextBoxKeyDown;
			cboxByteOrder.KeyDown += OnTextBoxKeyDown;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void FormDevice_Load(object sender, EventArgs e)
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
			if (errorProvider1.HasErrors)
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.ModiconSerialDevice));
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnSave = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.txtDescription = new System.Windows.Forms.TextBox();
		this.gbGeneral = new System.Windows.Forms.GroupBox();
		this.txtConnectRetries = new System.Windows.Forms.NumericUpDown();
		this.label6 = new System.Windows.Forms.Label();
		this.chkAutoReconnect = new System.Windows.Forms.CheckBox();
		this.chkActive = new System.Windows.Forms.CheckBox();
		this.txtBaseAddress = new System.Windows.Forms.NumericUpDown();
		this.txtDeviceName = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.cboxByteOrder = new System.Windows.Forms.ComboBox();
		this.txtChannel = new System.Windows.Forms.TextBox();
		this.lblProtocol = new System.Windows.Forms.Label();
		this.label2 = new System.Windows.Forms.Label();
		this.txtBlockSize = new System.Windows.Forms.NumericUpDown();
		this.label3 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.txtStationNo = new System.Windows.Forms.NumericUpDown();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		this.groupBox1.SuspendLayout();
		this.gbGeneral.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtConnectRetries).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtBaseAddress).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtBlockSize).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtStationNo).BeginInit();
		base.SuspendLayout();
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(387, 338);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(76, 30);
		this.btnCancel.TabIndex = 11;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnSave.Location = new System.Drawing.Point(305, 338);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(76, 30);
		this.btnSave.TabIndex = 10;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.errorProvider1.ContainerControl = this;
		this.groupBox1.Controls.Add(this.txtDescription);
		this.groupBox1.ForeColor = System.Drawing.Color.Navy;
		this.groupBox1.Location = new System.Drawing.Point(10, 194);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(453, 138);
		this.groupBox1.TabIndex = 88;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Description";
		this.txtDescription.Location = new System.Drawing.Point(10, 24);
		this.txtDescription.Multiline = true;
		this.txtDescription.Name = "txtDescription";
		this.txtDescription.Size = new System.Drawing.Size(434, 100);
		this.txtDescription.TabIndex = 6;
		this.gbGeneral.Controls.Add(this.txtConnectRetries);
		this.gbGeneral.Controls.Add(this.label6);
		this.gbGeneral.Controls.Add(this.chkAutoReconnect);
		this.gbGeneral.Controls.Add(this.chkActive);
		this.gbGeneral.Controls.Add(this.txtBaseAddress);
		this.gbGeneral.Controls.Add(this.txtDeviceName);
		this.gbGeneral.Controls.Add(this.label5);
		this.gbGeneral.Controls.Add(this.label1);
		this.gbGeneral.Controls.Add(this.cboxByteOrder);
		this.gbGeneral.Controls.Add(this.txtChannel);
		this.gbGeneral.Controls.Add(this.lblProtocol);
		this.gbGeneral.Controls.Add(this.label2);
		this.gbGeneral.Controls.Add(this.txtBlockSize);
		this.gbGeneral.Controls.Add(this.label3);
		this.gbGeneral.Controls.Add(this.label4);
		this.gbGeneral.Controls.Add(this.txtStationNo);
		this.gbGeneral.ForeColor = System.Drawing.Color.Navy;
		this.gbGeneral.Location = new System.Drawing.Point(10, 3);
		this.gbGeneral.Name = "gbGeneral";
		this.gbGeneral.Size = new System.Drawing.Size(453, 185);
		this.gbGeneral.TabIndex = 87;
		this.gbGeneral.TabStop = false;
		this.gbGeneral.Text = "General";
		this.txtConnectRetries.Location = new System.Drawing.Point(374, 153);
		this.txtConnectRetries.Maximum = new decimal(new int[4] { 3600, 0, 0, 0 });
		this.txtConnectRetries.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.txtConnectRetries.Name = "txtConnectRetries";
		this.txtConnectRetries.Size = new System.Drawing.Size(70, 23);
		this.txtConnectRetries.TabIndex = 98;
		this.txtConnectRetries.Value = new decimal(new int[4] { 3, 0, 0, 0 });
		this.label6.AutoSize = true;
		this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label6.Location = new System.Drawing.Point(275, 157);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(96, 15);
		this.label6.TabIndex = 99;
		this.label6.Text = "Connect retries:";
		this.chkAutoReconnect.AutoSize = true;
		this.chkAutoReconnect.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkAutoReconnect.Location = new System.Drawing.Point(108, 155);
		this.chkAutoReconnect.Name = "chkAutoReconnect";
		this.chkAutoReconnect.Size = new System.Drawing.Size(108, 19);
		this.chkAutoReconnect.TabIndex = 97;
		this.chkAutoReconnect.Text = "Auto reconnect";
		this.chkAutoReconnect.UseVisualStyleBackColor = true;
		this.chkActive.AutoSize = true;
		this.chkActive.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkActive.Location = new System.Drawing.Point(10, 155);
		this.chkActive.Name = "chkActive";
		this.chkActive.Size = new System.Drawing.Size(59, 19);
		this.chkActive.TabIndex = 94;
		this.chkActive.Text = "Active";
		this.chkActive.UseVisualStyleBackColor = true;
		this.txtBaseAddress.Location = new System.Drawing.Point(374, 119);
		this.txtBaseAddress.Maximum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.txtBaseAddress.Name = "txtBaseAddress";
		this.txtBaseAddress.Size = new System.Drawing.Size(70, 23);
		this.txtBaseAddress.TabIndex = 5;
		this.txtBaseAddress.Value = new decimal(new int[4] { 1, 0, 0, 0 });
		this.txtDeviceName.Location = new System.Drawing.Point(106, 50);
		this.txtDeviceName.Name = "txtDeviceName";
		this.txtDeviceName.Size = new System.Drawing.Size(338, 23);
		this.txtDeviceName.TabIndex = 1;
		this.label5.AutoSize = true;
		this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label5.Location = new System.Drawing.Point(292, 123);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(79, 15);
		this.label5.TabIndex = 93;
		this.label5.Text = "Base Address:";
		this.label1.AutoSize = true;
		this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label1.Location = new System.Drawing.Point(4, 21);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(87, 15);
		this.label1.TabIndex = 0;
		this.label1.Text = "Channel name:";
		this.cboxByteOrder.FormattingEnabled = true;
		this.cboxByteOrder.Location = new System.Drawing.Point(108, 119);
		this.cboxByteOrder.Name = "cboxByteOrder";
		this.cboxByteOrder.Size = new System.Drawing.Size(158, 23);
		this.cboxByteOrder.TabIndex = 4;
		this.txtChannel.Location = new System.Drawing.Point(106, 18);
		this.txtChannel.Name = "txtChannel";
		this.txtChannel.ReadOnly = true;
		this.txtChannel.Size = new System.Drawing.Size(338, 23);
		this.txtChannel.TabIndex = 9;
		this.lblProtocol.AutoSize = true;
		this.lblProtocol.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblProtocol.Location = new System.Drawing.Point(6, 123);
		this.lblProtocol.Name = "lblProtocol";
		this.lblProtocol.Size = new System.Drawing.Size(64, 15);
		this.lblProtocol.TabIndex = 91;
		this.lblProtocol.Text = "Byte order:";
		this.label2.AutoSize = true;
		this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label2.Location = new System.Drawing.Point(4, 54);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(78, 15);
		this.label2.TabIndex = 2;
		this.label2.Text = "Device name:";
		this.txtBlockSize.Location = new System.Drawing.Point(374, 84);
		this.txtBlockSize.Maximum = new decimal(new int[4] { 125, 0, 0, 0 });
		this.txtBlockSize.Minimum = new decimal(new int[4] { 1, 0, 0, 0 });
		this.txtBlockSize.Name = "txtBlockSize";
		this.txtBlockSize.Size = new System.Drawing.Size(70, 23);
		this.txtBlockSize.TabIndex = 3;
		this.txtBlockSize.Value = new decimal(new int[4] { 100, 0, 0, 0 });
		this.label3.AutoSize = true;
		this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label3.Location = new System.Drawing.Point(6, 88);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(66, 15);
		this.label3.TabIndex = 89;
		this.label3.Text = "Station No:";
		this.label4.AutoSize = true;
		this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label4.Location = new System.Drawing.Point(272, 88);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(99, 15);
		this.label4.TabIndex = 90;
		this.label4.Text = "Block Size(Word):";
		this.txtStationNo.Location = new System.Drawing.Point(108, 84);
		this.txtStationNo.Maximum = new decimal(new int[4] { 10000, 0, 0, 0 });
		this.txtStationNo.Name = "txtStationNo";
		this.txtStationNo.Size = new System.Drawing.Size(158, 23);
		this.txtStationNo.TabIndex = 2;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(474, 380);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.gbGeneral);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSave);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "ModiconSerialDevice";
		this.Text = "Device";
		base.Load += new System.EventHandler(FormDevice_Load);
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.gbGeneral.ResumeLayout(false);
		this.gbGeneral.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtConnectRetries).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtBaseAddress).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtBlockSize).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtStationNo).EndInit();
		base.ResumeLayout(false);
	}
}

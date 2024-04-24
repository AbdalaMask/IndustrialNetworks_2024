using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.DataTypes;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.Delta.Models;
using NetStudio.Fatek;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Local;
using NetStudio.Keyence.MC;
using NetStudio.LS;
using NetStudio.Mitsubishi.Dedicated;
using NetStudio.Mitsubishi.FXSerial;
using NetStudio.Mitsubishi.MC;
using NetStudio.Mitsubishi.SLMP;
using NetStudio.Modbus;
using NetStudio.Omron;
using NetStudio.Panasonic.Mewtocol;
using NetStudio.Siemens;
using NetStudio.Vigor;

namespace NetStudio.IPS;

public class FormRangeTag : Form
{
	private Group group;

	private Tag tagTemplate;

	private TRange range;

	private Channel channel;

	private Device device;

	public EventRangeTagChanged? OnRangeTagChanged;

	private IContainer components;

	private Button btnCancel;

	private Button btnSave;

	private ErrorProvider errorProvider1;

	private GroupBox groupBox1;

	private Label label1;

	private TextBox txtChannel;

	private Label label2;

	private TextBox txtDevice;

	private Label label3;

	private TextBox txtGroup;

	private Label label4;

	private TextBox txtTagName;

	private TextBox txtAddress;

	private Label lblAddress;

	private NumericUpDown txtQuantity;

	private Label label5;

	private Label label6;

	private ComboBox cboxMode;

	private Label lblProtocol;

	private ComboBox cboxDataType;

	public FormRangeTag(Channel channel, Device device, Group group)
	{
		InitializeComponent();
		OnInitialize(channel, device, group);
	}

	public void OnInitialize(Channel channel, Device device, Group group)
	{
		try
		{
			if (channel.Manufacturer == Manufacturer.PANASONIC)
			{
				lblAddress.Text = "FP Address";
			}
			else
			{
				lblAddress.Text = "Address";
			}
			txtChannel.Text = channel.Name;
			txtDevice.Text = device.Name;
			txtGroup.Text = group.Name;
			this.channel = channel;
			this.device = device;
			this.group = group;
			tagTemplate = new Tag();
			range = new TRange
			{
				Protocol = channel.Protocol
			};
			tagTemplate.ChannelId = channel.Id;
			tagTemplate.DeviceId = device.Id;
			tagTemplate.GroupId = group.Id;
			lblAddress.Text = tagTemplate.GetAddressLabel(channel.Manufacturer);
			new Binding("Enabled", tagTemplate, "DataType", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged).Format += delegate(object? sender, ConvertEventArgs e)
			{
				if (e.Value != null)
				{
					DataType dataType = (DataType)e.Value;
					e.Value = dataType == DataType.TIME16 || dataType == DataType.TIME32 || dataType == DataType.STRING;
				}
			};
			txtTagName.DataBindings.Clear();
			txtTagName.DataBindings.Add("Text", tagTemplate, "Name", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtAddress.DataBindings.Clear();
			txtAddress.DataBindings.Add("Text", tagTemplate, "Address", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			cboxDataType.DataBindings.Clear();
			cboxDataType.DataSource = Extensions.GetDictionary<DataType>().ToList();
			cboxDataType.DataBindings.Add("SelectedValue", tagTemplate, "DataType");
			cboxDataType.DisplayMember = "Value";
			cboxDataType.ValueMember = "Key";
			cboxMode.DataBindings.Clear();
			cboxMode.DataSource = Extensions.GetDictionary<TagMode>().ToList();
			cboxMode.DataBindings.Add("SelectedValue", tagTemplate, "Mode");
			cboxMode.DisplayMember = "Value";
			cboxMode.ValueMember = "Key";
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async void btnSave_Click(object sender, EventArgs e)
	{
		try
		{
			if (tagTemplate == null)
			{
				throw new Exception("Tag is null");
			}
			errorProvider1.SetError(txtTagName, null);
			errorProvider1.SetError(txtAddress, null);
			errorProvider1.Clear();
			if (string.IsNullOrEmpty(txtTagName.Text) || string.IsNullOrWhiteSpace(txtTagName.Text))
			{
				base.ActiveControl = txtTagName;
				errorProvider1.SetIconAlignment(txtTagName, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(txtTagName, 2);
				errorProvider1.SetError(txtTagName, "Please enter your tag prefix");
			}
			if (string.IsNullOrEmpty(txtAddress.Text) || string.IsNullOrWhiteSpace(txtAddress.Text))
			{
				base.ActiveControl = txtAddress;
				errorProvider1.SetIconAlignment(txtAddress, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(txtAddress, 2);
				errorProvider1.SetError(txtAddress, "Please enter your address");
			}
			if (errorProvider1.HasErrors)
			{
				return;
			}
			ValidateResult validateResult = TagValidate();
			if (validateResult.Status == ValidateStatus.Valid)
			{
				range.Template = tagTemplate;
				range.Quantity = (int)txtQuantity.Value;
				base.Opacity = 0.0;
				await WaitFormManager.ShowAsync(this, "Creating...");
				List<Tag> tags = EditHelper.AddTagRange(range);
				if (OnRangeTagChanged != null)
				{
					OnRangeTagChanged(tags);
				}
				await WaitFormManager.CloseAsync();
				base.DialogResult = DialogResult.OK;
			}
			else
			{
				MessageBox.Show(this, validateResult.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			base.Opacity = 100.0;
		}
	}

	private void txtTagName_Validating(object sender, CancelEventArgs e)
	{
		TextBox textBox = (TextBox)sender;
		errorProvider1.SetError(textBox, null);
		errorProvider1.Clear();
		if (!string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
		{
			e.Cancel = false;
			return;
		}
		e.Cancel = true;
		errorProvider1.SetIconAlignment(txtTagName, ErrorIconAlignment.MiddleLeft);
		errorProvider1.SetIconPadding(txtTagName, 2);
		errorProvider1.SetError(textBox, "Please enter your tag name");
	}

	private void txtAddress_Validating(object sender, CancelEventArgs e)
	{
		TextBox textBox = (TextBox)sender;
		errorProvider1.SetError(textBox, null);
		errorProvider1.Clear();
		if (!string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
		{
			e.Cancel = false;
			textBox.Text = textBox.Text.ToUpper();
			if (textBox.Text.StartsWith("%"))
			{
				textBox.Text = textBox.Text.Substring(1);
			}
		}
		else
		{
			e.Cancel = true;
			errorProvider1.SetIconAlignment(txtAddress, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtAddress, 2);
			errorProvider1.SetError(textBox, "Please enter your address");
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

	private void CreateTagTemplate(IpsProtocolType protocol)
	{
		if (tagTemplate != null)
		{
			switch (protocol)
			{
			case IpsProtocolType.S7_TCP:
				range.Memory = S7Utility.GetMemory(tagTemplate.Address);
				range.Offset = S7Utility.GetOffset(tagTemplate.Address);
				range.Size = S7Utility.GetSizeOfDataType(tagTemplate);
				break;
			case IpsProtocolType.CNET_XGT_PROTOCOL:
			case IpsProtocolType.FENET_XGT_PROTOCOL:
				range.Memory = XgtUtility.GetMemory(tagTemplate);
				range.Offset = XgtUtility.GetOffset(tagTemplate);
				range.Size = XgtUtility.GetSizeOfDataType(tagTemplate);
				break;
			case IpsProtocolType.MEWTOCOL_PROTOCOL:
				MewtocolUtility.IncrementWordAddress(tagTemplate);
				break;
			case IpsProtocolType.MISUBISHI_MC_PROTOCOL:
				tagTemplate.Address = NetStudio.Mitsubishi.MC.MCUtility.IncrementWordAddress(device, tagTemplate);
				break;
			case IpsProtocolType.MISUBISHI_SLMP_PROTOCOL:
				tagTemplate.Address = SLMPUtility.IncrementWordAddress(device, tagTemplate);
				break;
			case IpsProtocolType.DEDICATED1_PROTOCOL:
			case IpsProtocolType.DEDICATED4_PROTOCOL:
				tagTemplate.Address = DedicatedUtility.IncrementWordAddress(tagTemplate);
				break;
			case IpsProtocolType.FX_SERIAL_PROTOCOL:
				tagTemplate.Address = FXSerialUtility.IncrementByteAddress(tagTemplate);
				break;
			case IpsProtocolType.FINS_TCP_PROTOCOL:
			case IpsProtocolType.FINS_UDP_PROTOCOL:
			case IpsProtocolType.HOSTLINK_FINS_PROTOCOL:
			case IpsProtocolType.HOSTLINK_CMODE_PROTOCOL:
				OmronUtility.IncrementWordAddress(tagTemplate);
				break;
			case IpsProtocolType.MODBUS_TCP:
			case IpsProtocolType.MODBUS_RTU:
			case IpsProtocolType.MODBUS_ASCII:
				ModbusUtility.IncrementAddress(tagTemplate);
				break;
			case IpsProtocolType.VS_PROTOCOL:
				VSUtility.IncrementWordAddress(tagTemplate);
				break;
			case IpsProtocolType.VB_PROTOCOL:
				VBUtility.IncrementWordAddress(tagTemplate);
				break;
			case IpsProtocolType.FATEK_PROTOCOL:
				FatekUtility.IncrementWordAddress(tagTemplate);
				break;
			case IpsProtocolType.S7_MPI:
			case IpsProtocolType.S7_PPI:
			case IpsProtocolType.ASCII_PROTOCOL:
			case IpsProtocolType.DELTA_ASCII:
			case IpsProtocolType.DELTA_RTU:
			case IpsProtocolType.DELTA_TCP:
				break;
			case IpsProtocolType.KEYENCE_MC_PROTOCOL:
				tagTemplate.Address = NetStudio.Keyence.MC.MCUtility.IncrementWordAddress(device, tagTemplate);
				break;
			}
		}
	}

	private ValidateResult TagValidate()
	{
		ValidateResult validateResult = new ValidateResult
		{
			Status = ValidateStatus.Invalid,
			Message = "Data type and Address are not match."
		};
		try
		{
			switch (channel.Protocol)
			{
			case IpsProtocolType.S7_TCP:
				validateResult = S7Utility.TagValidate(tagTemplate);
				break;
			case IpsProtocolType.CNET_XGT_PROTOCOL:
			case IpsProtocolType.FENET_XGT_PROTOCOL:
				validateResult = XgtUtility.TagValidate(device, tagTemplate);
				break;
			case IpsProtocolType.MEWTOCOL_PROTOCOL:
				validateResult = MewtocolUtility.TagValidate(tagTemplate);
				break;
			case IpsProtocolType.MISUBISHI_MC_PROTOCOL:
				validateResult = NetStudio.Mitsubishi.MC.MCUtility.TagValidate(tagTemplate);
				break;
			case IpsProtocolType.MISUBISHI_SLMP_PROTOCOL:
				validateResult = SLMPUtility.TagValidate(tagTemplate);
				break;
			case IpsProtocolType.DEDICATED1_PROTOCOL:
			case IpsProtocolType.DEDICATED4_PROTOCOL:
				validateResult = DedicatedUtility.TagValidate(tagTemplate);
				break;
			case IpsProtocolType.FX_SERIAL_PROTOCOL:
				validateResult = FXSerialUtility.TagValidate(tagTemplate);
				break;
			case IpsProtocolType.FINS_TCP_PROTOCOL:
			case IpsProtocolType.FINS_UDP_PROTOCOL:
			case IpsProtocolType.HOSTLINK_FINS_PROTOCOL:
			case IpsProtocolType.HOSTLINK_CMODE_PROTOCOL:
				validateResult = OmronUtility.TagValidate(tagTemplate);
				break;
			default:
				validateResult.Status = ValidateStatus.Valid;
				break;
			case IpsProtocolType.VS_PROTOCOL:
				validateResult = VSUtility.TagValidate(tagTemplate);
				break;
			case IpsProtocolType.VB_PROTOCOL:
				validateResult = VBUtility.TagValidate(tagTemplate);
				break;
			case IpsProtocolType.FATEK_PROTOCOL:
				validateResult = FatekUtility.TagValidate(tagTemplate);
				break;
			case IpsProtocolType.DELTA_ASCII:
			case IpsProtocolType.DELTA_RTU:
			case IpsProtocolType.DELTA_TCP:
				validateResult = ((2 != device.DeviceType) ? DeltaUtility.AhAsSeries.TagValidate(tagTemplate) : DeltaUtility.DvpSeries.TagValidate(tagTemplate));
				break;
			case IpsProtocolType.KEYENCE_MC_PROTOCOL:
				validateResult = NetStudio.Keyence.MC.MCUtility.TagValidate(device, tagTemplate);
				break;
			}
		}
		catch (Exception ex)
		{
			validateResult.Status = ValidateStatus.Invalid;
			validateResult.Message = ex.Message;
		}
		return validateResult;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormRangeTag));
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnSave = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.label6 = new System.Windows.Forms.Label();
		this.cboxMode = new System.Windows.Forms.ComboBox();
		this.lblProtocol = new System.Windows.Forms.Label();
		this.cboxDataType = new System.Windows.Forms.ComboBox();
		this.txtQuantity = new System.Windows.Forms.NumericUpDown();
		this.label5 = new System.Windows.Forms.Label();
		this.txtAddress = new System.Windows.Forms.TextBox();
		this.lblAddress = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.txtChannel = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.txtDevice = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.txtGroup = new System.Windows.Forms.TextBox();
		this.label4 = new System.Windows.Forms.Label();
		this.txtTagName = new System.Windows.Forms.TextBox();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtQuantity).BeginInit();
		base.SuspendLayout();
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(333, 222);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(76, 30);
		this.btnCancel.TabIndex = 8;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnSave.Location = new System.Drawing.Point(251, 222);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(76, 30);
		this.btnSave.TabIndex = 7;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.errorProvider1.ContainerControl = this;
		this.groupBox1.Controls.Add(this.label6);
		this.groupBox1.Controls.Add(this.cboxMode);
		this.groupBox1.Controls.Add(this.lblProtocol);
		this.groupBox1.Controls.Add(this.cboxDataType);
		this.groupBox1.Controls.Add(this.txtQuantity);
		this.groupBox1.Controls.Add(this.label5);
		this.groupBox1.Controls.Add(this.txtAddress);
		this.groupBox1.Controls.Add(this.lblAddress);
		this.groupBox1.Controls.Add(this.label1);
		this.groupBox1.Controls.Add(this.txtChannel);
		this.groupBox1.Controls.Add(this.label2);
		this.groupBox1.Controls.Add(this.txtDevice);
		this.groupBox1.Controls.Add(this.label3);
		this.groupBox1.Controls.Add(this.txtGroup);
		this.groupBox1.Controls.Add(this.label4);
		this.groupBox1.Controls.Add(this.txtTagName);
		this.groupBox1.ForeColor = System.Drawing.Color.Navy;
		this.groupBox1.Location = new System.Drawing.Point(7, 2);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(402, 214);
		this.groupBox1.TabIndex = 10;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "General";
		this.label6.AutoSize = true;
		this.label6.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label6.Location = new System.Drawing.Point(245, 183);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(46, 15);
		this.label6.TabIndex = 51;
		this.label6.Text = "Access:";
		this.cboxMode.FormattingEnabled = true;
		this.cboxMode.Location = new System.Drawing.Point(294, 179);
		this.cboxMode.Name = "cboxMode";
		this.cboxMode.Size = new System.Drawing.Size(101, 23);
		this.cboxMode.TabIndex = 49;
		this.lblProtocol.AutoSize = true;
		this.lblProtocol.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblProtocol.Location = new System.Drawing.Point(30, 183);
		this.lblProtocol.Name = "lblProtocol";
		this.lblProtocol.Size = new System.Drawing.Size(60, 15);
		this.lblProtocol.TabIndex = 50;
		this.lblProtocol.Text = "Data type:";
		this.cboxDataType.FormattingEnabled = true;
		this.cboxDataType.Location = new System.Drawing.Point(113, 179);
		this.cboxDataType.Name = "cboxDataType";
		this.cboxDataType.Size = new System.Drawing.Size(115, 23);
		this.cboxDataType.TabIndex = 48;
		this.txtQuantity.Location = new System.Drawing.Point(294, 144);
		this.txtQuantity.Maximum = new decimal(new int[4] { 4096, 0, 0, 0 });
		this.txtQuantity.Minimum = new decimal(new int[4] { 2, 0, 0, 0 });
		this.txtQuantity.Name = "txtQuantity";
		this.txtQuantity.Size = new System.Drawing.Size(101, 23);
		this.txtQuantity.TabIndex = 47;
		this.txtQuantity.Value = new decimal(new int[4] { 16, 0, 0, 0 });
		this.label5.AutoSize = true;
		this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label5.Location = new System.Drawing.Point(235, 148);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(56, 15);
		this.label5.TabIndex = 46;
		this.label5.Text = "Quantity:";
		this.label5.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.txtAddress.Location = new System.Drawing.Point(113, 144);
		this.txtAddress.Name = "txtAddress";
		this.txtAddress.Size = new System.Drawing.Size(115, 23);
		this.txtAddress.TabIndex = 40;
		this.lblAddress.AutoSize = true;
		this.lblAddress.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblAddress.Location = new System.Drawing.Point(38, 148);
		this.lblAddress.Name = "lblAddress";
		this.lblAddress.Size = new System.Drawing.Size(52, 15);
		this.lblAddress.TabIndex = 41;
		this.lblAddress.Text = "Address:";
		this.lblAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label1.AutoSize = true;
		this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label1.Location = new System.Drawing.Point(3, 23);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(87, 15);
		this.label1.TabIndex = 39;
		this.label1.Text = "Channel name:";
		this.txtChannel.Location = new System.Drawing.Point(113, 20);
		this.txtChannel.Name = "txtChannel";
		this.txtChannel.ReadOnly = true;
		this.txtChannel.Size = new System.Drawing.Size(282, 23);
		this.txtChannel.TabIndex = 41;
		this.label2.AutoSize = true;
		this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label2.Location = new System.Drawing.Point(12, 53);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(78, 15);
		this.label2.TabIndex = 40;
		this.label2.Text = "Device name:";
		this.txtDevice.Location = new System.Drawing.Point(113, 49);
		this.txtDevice.Name = "txtDevice";
		this.txtDevice.ReadOnly = true;
		this.txtDevice.Size = new System.Drawing.Size(282, 23);
		this.txtDevice.TabIndex = 42;
		this.label3.AutoSize = true;
		this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label3.Location = new System.Drawing.Point(29, 114);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(61, 15);
		this.label3.TabIndex = 44;
		this.label3.Text = "Tag name:";
		this.txtGroup.Location = new System.Drawing.Point(113, 80);
		this.txtGroup.Name = "txtGroup";
		this.txtGroup.ReadOnly = true;
		this.txtGroup.Size = new System.Drawing.Size(282, 23);
		this.txtGroup.TabIndex = 43;
		this.label4.AutoSize = true;
		this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label4.Location = new System.Drawing.Point(14, 83);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(76, 15);
		this.label4.TabIndex = 45;
		this.label4.Text = "Group name:";
		this.txtTagName.Location = new System.Drawing.Point(113, 111);
		this.txtTagName.Name = "txtTagName";
		this.txtTagName.Size = new System.Drawing.Size(282, 23);
		this.txtTagName.TabIndex = 38;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(418, 262);
		base.Controls.Add(this.groupBox1);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSave);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormRangeTag";
		this.Text = "Add an array of Tags";
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtQuantity).EndInit();
		base.ResumeLayout(false);
	}
}

using System;
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
using NetStudio.IPS.Entity;
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

public class FormTag : Form
{
	private Group group;

	private Tag? tagOld;

	private Tag? tg;

	private Channel channel;

	private Device device;

	public EventTagChanged? OnTagChanged;

	private IContainer components;

	private Button btnCancel;

	private Button btnSave;

	private ErrorProvider errorProvider1;

	private TabControl tabControl1;

	private TabPage tabPageGeneral;

	private TabPage tabPageScaling;

	private GroupBox gbRawValueRange;

	private NumericUpDown txtRawHighValue;

	private Label lblPort;

	private NumericUpDown txtRawLowValue;

	private Label label8;

	private GroupBox gbScaledValueRange;

	private NumericUpDown txtScaledLowValue;

	private Label label9;

	private NumericUpDown txtScaledHighValue;

	private Label label11;

	private CheckBox chkScaling;

	private NumericUpDown txtOffset;

	private Label lblOffset;

	private Button btnSaveAndClose;

	private Label label1;

	private TextBox txtChannel;

	private Label label2;

	private Label lblAddress;

	private TextBox txtDevice;

	private Label lblProtocol;

	private Label label3;

	private ComboBox cboxDataType;

	private TextBox txtGroup;

	private Label label4;

	private TextBox txtTagName;

	private TextBox txtAddress;

	private Label label5;

	private ComboBox cboxMode;

	private NumericUpDown txtResolution;

	private Label lblResolution;

	private Label label15;

	private GroupBox groupBox1;

	private TextBox txtDescription;

	private GroupBox groupBox2;

	private GroupBox groupBox3;

	private GroupBox groupBox4;

	private CheckBox chkOperator;

	public FormTag(Channel channel, Device device, Group group, Tag? tag_1, EditMode mode = EditMode.None)
	{
		InitializeComponent();
		OnInitialize(channel, device, group, tag_1, mode);
	}

	public void OnInitialize(Channel channel, Device device, Group group, Tag? tag_1, EditMode mode = EditMode.None)
	{
		try
		{
			txtChannel.Text = channel.Name;
			txtDevice.Text = device.Name;
			txtGroup.Text = group.Name;
			this.channel = channel;
			this.device = device;
			this.group = group;
			switch (mode)
			{
			default:
				throw new NotSupportedException();
			case EditMode.AddNew:
				Text = "Add new tag";
				tg = new Tag();
				base.ActiveControl = txtTagName;
				break;
			case EditMode.Continuous:
				Text = "Add new tag";
				if (tag_1 != null)
				{
					if (tagOld == null)
					{
						tagOld = (Tag)tag_1.Clone();
					}
					tg = tag_1;
					IncrementAddress(channel.Protocol);
				}
				base.ActiveControl = txtTagName;
				break;
			case EditMode.Edit:
				if (tag_1 == null)
				{
					throw new ArgumentNullException();
				}
				Text = "Edit tag";
				tg = (Tag)tag_1.Clone();
				btnSaveAndClose.Visible = false;
				break;
			case EditMode.Copy:
				if (tag_1 == null)
				{
					throw new ArgumentNullException();
				}
				if (tagOld == null)
				{
					tagOld = (Tag)tag_1.Clone();
				}
				Text = "Copy tag";
				tg = tag_1;
				tg.Id = 0;
				IncrementAddress(channel.Protocol);
				break;
			}
			tg = tg ?? new Tag();
			tg.ChannelId = channel.Id;
			tg.DeviceId = device.Id;
			tg.GroupId = group.Id;
			lblAddress.Text = tg.GetAddressLabel(channel.Manufacturer);
			chkScaling.DataBindings.Clear();
			chkScaling.DataBindings.Add("Checked", tg, "IsScaling", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			new Binding("Checked", tg, "IsScaling", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			lblOffset.Text = (tg.IsScaling ? "Offset:" : "Divide by:");
			chkScaling.CheckedChanged += delegate
			{
				lblOffset.Text = (chkScaling.Checked ? "Offset:" : "Divide by:");
			};
			txtResolution.DataBindings.Clear();
			Binding binding = new Binding("Enabled", tg, "DataType", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			binding.Format += delegate(object? sender, ConvertEventArgs e)
			{
				if (e.Value != null)
				{
					DataType dataType = (DataType)e.Value;
					e.Value = dataType == DataType.TIME16 || dataType == DataType.TIME32 || dataType == DataType.STRING;
				}
			};
			txtResolution.DataBindings.Add(binding);
			txtResolution.DataBindings.Add("Value", tg, "Resolution", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			chkOperator.DataBindings.Clear();
			chkOperator.DataBindings.Add(new Binding("Enabled", tg, "IsScaling", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged));
			chkOperator.Text = (tg.IsOperator ? "Operator: Plus" : "Operator: Subtract");
			chkOperator.CheckedChanged += delegate
			{
				chkOperator.Text = (chkOperator.Checked ? "Operator: Plus" : "Operator: Subtract");
			};
			txtOffset.DataBindings.Clear();
			txtOffset.DataBindings.Add("Value", tg, "Offset", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtRawHighValue.DataBindings.Clear();
			txtRawHighValue.DataBindings.Add(new Binding("Enabled", chkScaling, "Checked", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged));
			txtRawHighValue.DataBindings.Add("Value", tg, "AImax", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtRawLowValue.DataBindings.Clear();
			txtRawLowValue.DataBindings.Add(new Binding("Enabled", chkScaling, "Checked", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged));
			txtRawLowValue.DataBindings.Add("Value", tg, "AImin", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtScaledHighValue.DataBindings.Clear();
			txtScaledHighValue.DataBindings.Add(new Binding("Enabled", chkScaling, "Checked", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged));
			txtScaledHighValue.DataBindings.Add("Value", tg, "RLmax", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtScaledLowValue.DataBindings.Clear();
			txtScaledLowValue.DataBindings.Add(new Binding("Enabled", chkScaling, "Checked", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged));
			txtScaledLowValue.DataBindings.Add("Value", tg, "RLmin", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtTagName.DataBindings.Clear();
			txtTagName.DataBindings.Add("Text", tg, "Name", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtAddress.DataBindings.Clear();
			txtAddress.DataBindings.Add("Text", tg, "Address", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtDescription.DataBindings.Clear();
			txtDescription.DataBindings.Add("Text", tg, "Description", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			cboxDataType.DataBindings.Clear();
			cboxDataType.DataSource = Extensions.GetDictionary<DataType>().ToList();
			cboxDataType.DataBindings.Add("SelectedValue", tg, "DataType");
			cboxDataType.DisplayMember = "Value";
			cboxDataType.ValueMember = "Key";
			cboxMode.DataBindings.Clear();
			cboxMode.DataSource = Extensions.GetDictionary<TagMode>().ToList();
			cboxMode.DataBindings.Add("SelectedValue", tg, "Mode");
			cboxMode.DisplayMember = "Value";
			cboxMode.ValueMember = "Key";
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnSave_Click(object sender, EventArgs e)
	{
		try
		{
			if (tg == null)
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
				errorProvider1.SetIconPadding(txtTagName, 3);
				errorProvider1.SetError(txtTagName, "Please enter your tag name");
			}
			if (string.IsNullOrEmpty(txtAddress.Text) || string.IsNullOrWhiteSpace(txtAddress.Text))
			{
				base.ActiveControl = txtAddress;
				errorProvider1.SetIconAlignment(txtAddress, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(txtAddress, 3);
				errorProvider1.SetError(txtAddress, "Please enter your address");
			}
			if (chkScaling.Checked)
			{
				if (txtRawHighValue.Value <= 0m)
				{
					base.ActiveControl = txtRawHighValue;
					errorProvider1.SetIconAlignment(txtRawHighValue, ErrorIconAlignment.MiddleLeft);
					errorProvider1.SetIconPadding(txtRawHighValue, 3);
					errorProvider1.SetError(txtRawHighValue, "Please enter high raw.");
				}
				if (txtScaledHighValue.Value <= 0m)
				{
					base.ActiveControl = txtScaledHighValue;
					errorProvider1.SetIconAlignment(txtScaledHighValue, ErrorIconAlignment.MiddleLeft);
					errorProvider1.SetIconPadding(txtScaledHighValue, 3);
					errorProvider1.SetError(txtScaledHighValue, "Please enter high scaled.");
				}
			}
			if (errorProvider1.HasErrors)
			{
				return;
			}
			ValidateResult validateResult = TagValidate();
			if (validateResult.Status == ValidateStatus.Valid)
			{
				if (!Text.StartsWith("add", StringComparison.OrdinalIgnoreCase) && !Text.StartsWith("copy", StringComparison.OrdinalIgnoreCase))
				{
					EditHelper.EditTag(tg);
					if (OnTagChanged != null)
					{
						OnTagChanged(tg, isAddnew: false);
					}
				}
				else
				{
					EditHelper.AddTag(tg);
					if (OnTagChanged != null)
					{
						OnTagChanged(tg, isAddnew: true, hasAddNew: true);
					}
				}
				if (!btnSaveAndClose.Visible)
				{
					base.DialogResult = DialogResult.OK;
				}
			}
			else
			{
				MessageBox.Show(this, validateResult.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnSaveAndClose_Click(object sender, EventArgs e)
	{
		try
		{
			if (tg == null)
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
				errorProvider1.SetError(txtTagName, "Please enter your tag name");
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
				if (!Text.StartsWith("add", StringComparison.OrdinalIgnoreCase) && !Text.StartsWith("copy", StringComparison.OrdinalIgnoreCase))
				{
					EditHelper.EditTag(tg);
					if (OnTagChanged != null)
					{
						OnTagChanged(tg, isAddnew: false);
					}
				}
				else
				{
					EditHelper.AddTag(tg);
					if (OnTagChanged != null)
					{
						OnTagChanged(tg, isAddnew: true);
					}
				}
				base.DialogResult = DialogResult.OK;
			}
			else
			{
				MessageBox.Show(this, validateResult.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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
			if (textBox.Text.StartsWith("%") && device.DeviceType != 2)
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

	private void IncrementAddress(IpsProtocolType protocol)
	{
		if (tg != null)
		{
			switch (protocol)
			{
			case IpsProtocolType.S7_TCP:
				S7Utility.IncrementAddress(tg);
				break;
			case IpsProtocolType.CNET_XGT_PROTOCOL:
			case IpsProtocolType.FENET_XGT_PROTOCOL:
				XgtUtility.IncrementWordAddress(device, tg);
				break;
			case IpsProtocolType.MEWTOCOL_PROTOCOL:
				MewtocolUtility.IncrementWordAddress(tg);
				break;
			case IpsProtocolType.MISUBISHI_MC_PROTOCOL:
				tg.Address = NetStudio.Mitsubishi.MC.MCUtility.IncrementWordAddress(device, tg);
				break;
			case IpsProtocolType.MISUBISHI_SLMP_PROTOCOL:
				tg.Address = SLMPUtility.IncrementWordAddress(device, tg);
				break;
			case IpsProtocolType.DEDICATED1_PROTOCOL:
			case IpsProtocolType.DEDICATED4_PROTOCOL:
				tg.Address = DedicatedUtility.IncrementWordAddress(tg);
				break;
			case IpsProtocolType.FX_SERIAL_PROTOCOL:
				tg.Address = FXSerialUtility.IncrementByteAddress(tg);
				break;
			case IpsProtocolType.FINS_TCP_PROTOCOL:
			case IpsProtocolType.FINS_UDP_PROTOCOL:
			case IpsProtocolType.HOSTLINK_FINS_PROTOCOL:
			case IpsProtocolType.HOSTLINK_CMODE_PROTOCOL:
				OmronUtility.IncrementWordAddress(tg);
				break;
			case IpsProtocolType.MODBUS_TCP:
			case IpsProtocolType.MODBUS_RTU:
			case IpsProtocolType.MODBUS_ASCII:
				ModbusUtility.IncrementAddress(tg);
				break;
			case IpsProtocolType.VS_PROTOCOL:
				VSUtility.IncrementWordAddress(tg);
				break;
			case IpsProtocolType.VB_PROTOCOL:
				VBUtility.IncrementWordAddress(tg);
				break;
			case IpsProtocolType.FATEK_PROTOCOL:
				FatekUtility.IncrementWordAddress(tg);
				break;
			case IpsProtocolType.S7_MPI:
			case IpsProtocolType.S7_PPI:
			case IpsProtocolType.ASCII_PROTOCOL:
				break;
			case IpsProtocolType.DELTA_ASCII:
			case IpsProtocolType.DELTA_RTU:
			case IpsProtocolType.DELTA_TCP:
				DeltaUtility.IncrementAddress(tg);
				break;
			case IpsProtocolType.KEYENCE_MC_PROTOCOL:
				tg.Address = NetStudio.Keyence.MC.MCUtility.IncrementWordAddress(device, tg);
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
				validateResult = S7Utility.TagValidate(tg);
				break;
			default:
				validateResult.Status = ValidateStatus.Valid;
				break;
			case IpsProtocolType.CNET_XGT_PROTOCOL:
			case IpsProtocolType.FENET_XGT_PROTOCOL:
				validateResult = XgtUtility.TagValidate(device, tg);
				break;
			case IpsProtocolType.MEWTOCOL_PROTOCOL:
				validateResult = MewtocolUtility.TagValidate(tg);
				break;
			case IpsProtocolType.MISUBISHI_MC_PROTOCOL:
				validateResult = NetStudio.Mitsubishi.MC.MCUtility.TagValidate(tg);
				break;
			case IpsProtocolType.MISUBISHI_SLMP_PROTOCOL:
				validateResult = SLMPUtility.TagValidate(tg);
				break;
			case IpsProtocolType.DEDICATED1_PROTOCOL:
			case IpsProtocolType.DEDICATED4_PROTOCOL:
				validateResult = DedicatedUtility.TagValidate(tg);
				break;
			case IpsProtocolType.FX_SERIAL_PROTOCOL:
				validateResult = FXSerialUtility.TagValidate(tg);
				break;
			case IpsProtocolType.FINS_TCP_PROTOCOL:
			case IpsProtocolType.FINS_UDP_PROTOCOL:
			case IpsProtocolType.HOSTLINK_FINS_PROTOCOL:
			case IpsProtocolType.HOSTLINK_CMODE_PROTOCOL:
				validateResult = OmronUtility.TagValidate(tg);
				break;
			case IpsProtocolType.MODBUS_TCP:
			case IpsProtocolType.MODBUS_RTU:
			case IpsProtocolType.MODBUS_ASCII:
				validateResult = ModbusUtility.TagValidate(tg);
				break;
			case IpsProtocolType.VS_PROTOCOL:
				validateResult = VSUtility.TagValidate(tg);
				break;
			case IpsProtocolType.VB_PROTOCOL:
				validateResult = VBUtility.TagValidate(tg);
				break;
			case IpsProtocolType.FATEK_PROTOCOL:
				validateResult = FatekUtility.TagValidate(tg);
				break;
			case IpsProtocolType.DELTA_ASCII:
			case IpsProtocolType.DELTA_RTU:
			case IpsProtocolType.DELTA_TCP:
				validateResult = ((2 != device.DeviceType) ? DeltaUtility.AhAsSeries.TagValidate(tg) : DeltaUtility.DvpSeries.TagValidate(tg));
				break;
			case IpsProtocolType.KEYENCE_MC_PROTOCOL:
				validateResult = NetStudio.Keyence.MC.MCUtility.TagValidate(device, tg);
				break;
			}
		}
		catch (Exception ex)
		{
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormTag));
		this.btnCancel = new System.Windows.Forms.Button();
		this.btnSave = new System.Windows.Forms.Button();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.tabControl1 = new System.Windows.Forms.TabControl();
		this.tabPageGeneral = new System.Windows.Forms.TabPage();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.cboxMode = new System.Windows.Forms.ComboBox();
		this.txtResolution = new System.Windows.Forms.NumericUpDown();
		this.txtTagName = new System.Windows.Forms.TextBox();
		this.lblResolution = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.label5 = new System.Windows.Forms.Label();
		this.txtGroup = new System.Windows.Forms.TextBox();
		this.cboxDataType = new System.Windows.Forms.ComboBox();
		this.txtAddress = new System.Windows.Forms.TextBox();
		this.label3 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.lblProtocol = new System.Windows.Forms.Label();
		this.txtChannel = new System.Windows.Forms.TextBox();
		this.txtDevice = new System.Windows.Forms.TextBox();
		this.label2 = new System.Windows.Forms.Label();
		this.lblAddress = new System.Windows.Forms.Label();
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.txtDescription = new System.Windows.Forms.TextBox();
		this.tabPageScaling = new System.Windows.Forms.TabPage();
		this.groupBox4 = new System.Windows.Forms.GroupBox();
		this.label15 = new System.Windows.Forms.Label();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.chkOperator = new System.Windows.Forms.CheckBox();
		this.chkScaling = new System.Windows.Forms.CheckBox();
		this.gbRawValueRange = new System.Windows.Forms.GroupBox();
		this.txtRawLowValue = new System.Windows.Forms.NumericUpDown();
		this.label8 = new System.Windows.Forms.Label();
		this.txtRawHighValue = new System.Windows.Forms.NumericUpDown();
		this.lblPort = new System.Windows.Forms.Label();
		this.gbScaledValueRange = new System.Windows.Forms.GroupBox();
		this.txtScaledLowValue = new System.Windows.Forms.NumericUpDown();
		this.label9 = new System.Windows.Forms.Label();
		this.txtScaledHighValue = new System.Windows.Forms.NumericUpDown();
		this.label11 = new System.Windows.Forms.Label();
		this.txtOffset = new System.Windows.Forms.NumericUpDown();
		this.lblOffset = new System.Windows.Forms.Label();
		this.btnSaveAndClose = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		this.tabControl1.SuspendLayout();
		this.tabPageGeneral.SuspendLayout();
		this.groupBox2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtResolution).BeginInit();
		this.groupBox1.SuspendLayout();
		this.tabPageScaling.SuspendLayout();
		this.groupBox4.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.gbRawValueRange.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtRawLowValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtRawHighValue).BeginInit();
		this.gbScaledValueRange.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.txtScaledLowValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtScaledHighValue).BeginInit();
		((System.ComponentModel.ISupportInitialize)this.txtOffset).BeginInit();
		base.SuspendLayout();
		this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.btnCancel.Location = new System.Drawing.Point(353, 377);
		this.btnCancel.Name = "btnCancel";
		this.btnCancel.Size = new System.Drawing.Size(76, 30);
		this.btnCancel.TabIndex = 8;
		this.btnCancel.Text = "Cancel";
		this.btnCancel.UseVisualStyleBackColor = true;
		this.btnSave.Location = new System.Drawing.Point(271, 377);
		this.btnSave.Name = "btnSave";
		this.btnSave.Size = new System.Drawing.Size(76, 30);
		this.btnSave.TabIndex = 7;
		this.btnSave.Text = "Save";
		this.btnSave.UseVisualStyleBackColor = true;
		this.btnSave.Click += new System.EventHandler(btnSave_Click);
		this.errorProvider1.ContainerControl = this;
		this.tabControl1.Controls.Add(this.tabPageGeneral);
		this.tabControl1.Controls.Add(this.tabPageScaling);
		this.tabControl1.Location = new System.Drawing.Point(2, 1);
		this.tabControl1.Name = "tabControl1";
		this.tabControl1.SelectedIndex = 0;
		this.tabControl1.Size = new System.Drawing.Size(431, 370);
		this.tabControl1.TabIndex = 27;
		this.tabPageGeneral.Controls.Add(this.groupBox2);
		this.tabPageGeneral.Controls.Add(this.groupBox1);
		this.tabPageGeneral.Location = new System.Drawing.Point(4, 24);
		this.tabPageGeneral.Name = "tabPageGeneral";
		this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageGeneral.Size = new System.Drawing.Size(423, 342);
		this.tabPageGeneral.TabIndex = 0;
		this.tabPageGeneral.Text = "General";
		this.groupBox2.Controls.Add(this.cboxMode);
		this.groupBox2.Controls.Add(this.txtResolution);
		this.groupBox2.Controls.Add(this.txtTagName);
		this.groupBox2.Controls.Add(this.lblResolution);
		this.groupBox2.Controls.Add(this.label4);
		this.groupBox2.Controls.Add(this.label5);
		this.groupBox2.Controls.Add(this.txtGroup);
		this.groupBox2.Controls.Add(this.cboxDataType);
		this.groupBox2.Controls.Add(this.txtAddress);
		this.groupBox2.Controls.Add(this.label3);
		this.groupBox2.Controls.Add(this.label1);
		this.groupBox2.Controls.Add(this.lblProtocol);
		this.groupBox2.Controls.Add(this.txtChannel);
		this.groupBox2.Controls.Add(this.txtDevice);
		this.groupBox2.Controls.Add(this.label2);
		this.groupBox2.Controls.Add(this.lblAddress);
		this.groupBox2.ForeColor = System.Drawing.Color.Navy;
		this.groupBox2.Location = new System.Drawing.Point(8, 5);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(407, 212);
		this.groupBox2.TabIndex = 50;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "General";
		this.cboxMode.FormattingEnabled = true;
		this.cboxMode.Location = new System.Drawing.Point(295, 177);
		this.cboxMode.Name = "cboxMode";
		this.cboxMode.Size = new System.Drawing.Size(101, 23);
		this.cboxMode.TabIndex = 5;
		this.txtResolution.Location = new System.Drawing.Point(114, 177);
		this.txtResolution.Maximum = new decimal(new int[4] { 1024, 0, 0, 0 });
		this.txtResolution.Name = "txtResolution";
		this.txtResolution.Size = new System.Drawing.Size(103, 23);
		this.txtResolution.TabIndex = 4;
		this.txtTagName.Location = new System.Drawing.Point(114, 110);
		this.txtTagName.Name = "txtTagName";
		this.txtTagName.Size = new System.Drawing.Size(282, 23);
		this.txtTagName.TabIndex = 1;
		this.txtTagName.Validating += new System.ComponentModel.CancelEventHandler(txtTagName_Validating);
		this.lblResolution.AutoSize = true;
		this.lblResolution.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblResolution.Location = new System.Drawing.Point(25, 181);
		this.lblResolution.Name = "lblResolution";
		this.lblResolution.Size = new System.Drawing.Size(66, 15);
		this.lblResolution.TabIndex = 42;
		this.lblResolution.Text = "Resolution:";
		this.lblResolution.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.label4.AutoSize = true;
		this.label4.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label4.Location = new System.Drawing.Point(15, 82);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(76, 15);
		this.label4.TabIndex = 37;
		this.label4.Text = "Group name:";
		this.label5.AutoSize = true;
		this.label5.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label5.Location = new System.Drawing.Point(243, 181);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(46, 15);
		this.label5.TabIndex = 41;
		this.label5.Text = "Access:";
		this.txtGroup.Location = new System.Drawing.Point(114, 79);
		this.txtGroup.Name = "txtGroup";
		this.txtGroup.ReadOnly = true;
		this.txtGroup.Size = new System.Drawing.Size(282, 23);
		this.txtGroup.TabIndex = 34;
		this.cboxDataType.FormattingEnabled = true;
		this.cboxDataType.Location = new System.Drawing.Point(295, 143);
		this.cboxDataType.Name = "cboxDataType";
		this.cboxDataType.Size = new System.Drawing.Size(101, 23);
		this.cboxDataType.TabIndex = 3;
		this.txtAddress.Location = new System.Drawing.Point(114, 143);
		this.txtAddress.Name = "txtAddress";
		this.txtAddress.Size = new System.Drawing.Size(103, 23);
		this.txtAddress.TabIndex = 2;
		this.txtAddress.Validating += new System.ComponentModel.CancelEventHandler(txtAddress_Validating);
		this.label3.AutoSize = true;
		this.label3.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label3.Location = new System.Drawing.Point(30, 113);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(61, 15);
		this.label3.TabIndex = 35;
		this.label3.Text = "Tag name:";
		this.label1.AutoSize = true;
		this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label1.Location = new System.Drawing.Point(4, 22);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(87, 15);
		this.label1.TabIndex = 25;
		this.label1.Text = "Channel name:";
		this.lblProtocol.AutoSize = true;
		this.lblProtocol.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblProtocol.Location = new System.Drawing.Point(229, 147);
		this.lblProtocol.Name = "lblProtocol";
		this.lblProtocol.Size = new System.Drawing.Size(60, 15);
		this.lblProtocol.TabIndex = 38;
		this.lblProtocol.Text = "Data type:";
		this.txtChannel.Location = new System.Drawing.Point(114, 19);
		this.txtChannel.Name = "txtChannel";
		this.txtChannel.ReadOnly = true;
		this.txtChannel.Size = new System.Drawing.Size(282, 23);
		this.txtChannel.TabIndex = 32;
		this.txtDevice.Location = new System.Drawing.Point(114, 48);
		this.txtDevice.Name = "txtDevice";
		this.txtDevice.ReadOnly = true;
		this.txtDevice.Size = new System.Drawing.Size(282, 23);
		this.txtDevice.TabIndex = 33;
		this.label2.AutoSize = true;
		this.label2.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label2.Location = new System.Drawing.Point(13, 52);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(78, 15);
		this.label2.TabIndex = 28;
		this.label2.Text = "Device name:";
		this.lblAddress.AutoSize = true;
		this.lblAddress.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblAddress.Location = new System.Drawing.Point(39, 147);
		this.lblAddress.Name = "lblAddress";
		this.lblAddress.Size = new System.Drawing.Size(52, 15);
		this.lblAddress.TabIndex = 39;
		this.lblAddress.Text = "Address:";
		this.lblAddress.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
		this.groupBox1.Controls.Add(this.txtDescription);
		this.groupBox1.ForeColor = System.Drawing.Color.Navy;
		this.groupBox1.Location = new System.Drawing.Point(8, 223);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(407, 109);
		this.groupBox1.TabIndex = 49;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "Description";
		this.txtDescription.Location = new System.Drawing.Point(10, 22);
		this.txtDescription.Multiline = true;
		this.txtDescription.Name = "txtDescription";
		this.txtDescription.Size = new System.Drawing.Size(386, 74);
		this.txtDescription.TabIndex = 6;
		this.tabPageScaling.Controls.Add(this.groupBox4);
		this.tabPageScaling.Controls.Add(this.groupBox3);
		this.tabPageScaling.Location = new System.Drawing.Point(4, 24);
		this.tabPageScaling.Name = "tabPageScaling";
		this.tabPageScaling.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageScaling.Size = new System.Drawing.Size(423, 342);
		this.tabPageScaling.TabIndex = 1;
		this.tabPageScaling.Text = "Scaling";
		this.tabPageScaling.UseVisualStyleBackColor = true;
		this.groupBox4.Controls.Add(this.label15);
		this.groupBox4.ForeColor = System.Drawing.Color.Navy;
		this.groupBox4.Location = new System.Drawing.Point(6, 178);
		this.groupBox4.Name = "groupBox4";
		this.groupBox4.Size = new System.Drawing.Size(411, 158);
		this.groupBox4.TabIndex = 50;
		this.groupBox4.TabStop = false;
		this.groupBox4.Text = "Note";
		this.label15.AutoSize = true;
		this.label15.ForeColor = System.Drawing.Color.Crimson;
		this.label15.Location = new System.Drawing.Point(16, 21);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(206, 30);
		this.label15.TabIndex = 23;
		this.label15.Text = "- Only modbus protocol is supported.\r\n- Data types: INT, UINT, DINT, UDINT.";
		this.groupBox3.Controls.Add(this.chkOperator);
		this.groupBox3.Controls.Add(this.chkScaling);
		this.groupBox3.Controls.Add(this.gbRawValueRange);
		this.groupBox3.Controls.Add(this.gbScaledValueRange);
		this.groupBox3.Controls.Add(this.txtOffset);
		this.groupBox3.Controls.Add(this.lblOffset);
		this.groupBox3.ForeColor = System.Drawing.Color.Navy;
		this.groupBox3.Location = new System.Drawing.Point(6, 6);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(411, 166);
		this.groupBox3.TabIndex = 49;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "General";
		this.chkOperator.AutoSize = true;
		this.chkOperator.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkOperator.Location = new System.Drawing.Point(98, 26);
		this.chkOperator.Name = "chkOperator";
		this.chkOperator.Size = new System.Drawing.Size(123, 19);
		this.chkOperator.TabIndex = 22;
		this.chkOperator.Text = "Operator: Subtract";
		this.chkOperator.UseVisualStyleBackColor = true;
		this.chkScaling.AutoSize = true;
		this.chkScaling.ForeColor = System.Drawing.SystemColors.ControlText;
		this.chkScaling.Location = new System.Drawing.Point(16, 26);
		this.chkScaling.Name = "chkScaling";
		this.chkScaling.Size = new System.Drawing.Size(58, 19);
		this.chkScaling.TabIndex = 1;
		this.chkScaling.Text = "Linear";
		this.chkScaling.UseVisualStyleBackColor = true;
		this.gbRawValueRange.Controls.Add(this.txtRawLowValue);
		this.gbRawValueRange.Controls.Add(this.label8);
		this.gbRawValueRange.Controls.Add(this.txtRawHighValue);
		this.gbRawValueRange.Controls.Add(this.lblPort);
		this.gbRawValueRange.ForeColor = System.Drawing.Color.Navy;
		this.gbRawValueRange.Location = new System.Drawing.Point(16, 61);
		this.gbRawValueRange.Name = "gbRawValueRange";
		this.gbRawValueRange.Size = new System.Drawing.Size(183, 88);
		this.gbRawValueRange.TabIndex = 10;
		this.gbRawValueRange.TabStop = false;
		this.gbRawValueRange.Text = "Raw Value Range";
		this.txtRawLowValue.Location = new System.Drawing.Point(62, 52);
		this.txtRawLowValue.Maximum = new decimal(new int[4] { 2147483647, 0, 0, 0 });
		this.txtRawLowValue.Minimum = new decimal(new int[4] { -2147483648, 0, 0, -2147483648 });
		this.txtRawLowValue.Name = "txtRawLowValue";
		this.txtRawLowValue.Size = new System.Drawing.Size(108, 23);
		this.txtRawLowValue.TabIndex = 4;
		this.label8.AutoSize = true;
		this.label8.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label8.Location = new System.Drawing.Point(7, 56);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(32, 15);
		this.label8.TabIndex = 19;
		this.label8.Text = "Low:";
		this.txtRawHighValue.Location = new System.Drawing.Point(62, 23);
		this.txtRawHighValue.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.txtRawHighValue.Minimum = new decimal(new int[4] { -2147483648, 0, 0, -2147483648 });
		this.txtRawHighValue.Name = "txtRawHighValue";
		this.txtRawHighValue.Size = new System.Drawing.Size(108, 23);
		this.txtRawHighValue.TabIndex = 3;
		this.lblPort.AutoSize = true;
		this.lblPort.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblPort.Location = new System.Drawing.Point(7, 27);
		this.lblPort.Name = "lblPort";
		this.lblPort.Size = new System.Drawing.Size(36, 15);
		this.lblPort.TabIndex = 16;
		this.lblPort.Text = "High:";
		this.gbScaledValueRange.Controls.Add(this.txtScaledLowValue);
		this.gbScaledValueRange.Controls.Add(this.label9);
		this.gbScaledValueRange.Controls.Add(this.txtScaledHighValue);
		this.gbScaledValueRange.Controls.Add(this.label11);
		this.gbScaledValueRange.ForeColor = System.Drawing.Color.Navy;
		this.gbScaledValueRange.Location = new System.Drawing.Point(210, 61);
		this.gbScaledValueRange.Name = "gbScaledValueRange";
		this.gbScaledValueRange.Size = new System.Drawing.Size(184, 88);
		this.gbScaledValueRange.TabIndex = 21;
		this.gbScaledValueRange.TabStop = false;
		this.gbScaledValueRange.Text = "Scaled Value Range";
		this.txtScaledLowValue.DecimalPlaces = 3;
		this.txtScaledLowValue.Location = new System.Drawing.Point(67, 52);
		this.txtScaledLowValue.Maximum = new decimal(new int[4] { 2147483647, 0, 0, 0 });
		this.txtScaledLowValue.Minimum = new decimal(new int[4] { -2147483648, 0, 0, -2147483648 });
		this.txtScaledLowValue.Name = "txtScaledLowValue";
		this.txtScaledLowValue.Size = new System.Drawing.Size(108, 23);
		this.txtScaledLowValue.TabIndex = 6;
		this.label9.AutoSize = true;
		this.label9.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label9.Location = new System.Drawing.Point(10, 56);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(32, 15);
		this.label9.TabIndex = 19;
		this.label9.Text = "Low:";
		this.txtScaledHighValue.DecimalPlaces = 3;
		this.txtScaledHighValue.Location = new System.Drawing.Point(67, 23);
		this.txtScaledHighValue.Maximum = new decimal(new int[4] { -1, 0, 0, 0 });
		this.txtScaledHighValue.Minimum = new decimal(new int[4] { -2147483648, 0, 0, -2147483648 });
		this.txtScaledHighValue.Name = "txtScaledHighValue";
		this.txtScaledHighValue.Size = new System.Drawing.Size(108, 23);
		this.txtScaledHighValue.TabIndex = 5;
		this.label11.AutoSize = true;
		this.label11.ForeColor = System.Drawing.SystemColors.ControlText;
		this.label11.Location = new System.Drawing.Point(10, 27);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(36, 15);
		this.label11.TabIndex = 16;
		this.label11.Text = "High:";
		this.txtOffset.DecimalPlaces = 3;
		this.txtOffset.Location = new System.Drawing.Point(298, 24);
		this.txtOffset.Maximum = new decimal(new int[4] { 100000, 0, 0, 0 });
		this.txtOffset.Name = "txtOffset";
		this.txtOffset.Size = new System.Drawing.Size(96, 23);
		this.txtOffset.TabIndex = 2;
		this.lblOffset.AutoSize = true;
		this.lblOffset.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblOffset.Location = new System.Drawing.Point(236, 28);
		this.lblOffset.Name = "lblOffset";
		this.lblOffset.Size = new System.Drawing.Size(59, 15);
		this.lblOffset.TabIndex = 21;
		this.lblOffset.Text = "Divide by:";
		this.btnSaveAndClose.Location = new System.Drawing.Point(172, 377);
		this.btnSaveAndClose.Name = "btnSaveAndClose";
		this.btnSaveAndClose.Size = new System.Drawing.Size(93, 30);
		this.btnSaveAndClose.TabIndex = 9;
		this.btnSaveAndClose.Text = "Save && Close";
		this.btnSaveAndClose.UseVisualStyleBackColor = true;
		this.btnSaveAndClose.Click += new System.EventHandler(btnSaveAndClose_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(439, 416);
		base.Controls.Add(this.btnSaveAndClose);
		base.Controls.Add(this.tabControl1);
		base.Controls.Add(this.btnCancel);
		base.Controls.Add(this.btnSave);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormTag";
		this.Text = "Tag";
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		this.tabControl1.ResumeLayout(false);
		this.tabPageGeneral.ResumeLayout(false);
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtResolution).EndInit();
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		this.tabPageScaling.ResumeLayout(false);
		this.groupBox4.ResumeLayout(false);
		this.groupBox4.PerformLayout();
		this.groupBox3.ResumeLayout(false);
		this.groupBox3.PerformLayout();
		this.gbRawValueRange.ResumeLayout(false);
		this.gbRawValueRange.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtRawLowValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtRawHighValue).EndInit();
		this.gbScaledValueRange.ResumeLayout(false);
		this.gbScaledValueRange.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.txtScaledLowValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtScaledHighValue).EndInit();
		((System.ComponentModel.ISupportInitialize)this.txtOffset).EndInit();
		base.ResumeLayout(false);
	}
}

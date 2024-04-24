using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;

namespace NetStudio.IPS.Controls;

[DesignTimeVisible(true)]
public class GeneralControl : UserControl
{
	private Channel channel;

	public EventConnectionTypeChanged OnConnectionTypeChanged;

	private IContainer components;

	private GroupBox groupBox1;

	private TextBox txtDescription;

	private Label lblProtocol;

	private ComboBox cboxProtocol;

	private Label lblManufacturer;

	private ComboBox cboxManufacturer;

	private ComboBox cboxConnType;

	private Label lblConnectionType;

	private TextBox txtName;

	private Label lblName;

	private ErrorProvider errorProvider1;

	private GroupBox gbDescription;

	public bool HasErrors
	{
		get
		{
			if (cboxProtocol.SelectedValue == null)
			{
				cboxProtocol.Focus();
				errorProvider1.SetIconAlignment(cboxProtocol, ErrorIconAlignment.MiddleLeft);
				errorProvider1.SetIconPadding(cboxProtocol, 5);
				errorProvider1.SetError(cboxProtocol, "Please choose your protocol");
			}
			return errorProvider1.HasErrors;
		}
	}

	public GeneralControl()
	{
		InitializeComponent();
	}

	protected override void OnLoad(EventArgs eventArgs_0)
	{
		base.OnLoad(eventArgs_0);
		if (base.DesignMode)
		{
			return;
		}
		try
		{
			cboxConnType.DataSource = Extensions.GetDictionary<ConnectionType>().ToList();
			cboxManufacturer.DataSource = Extensions.GetDictionary<Manufacturer>().ToList();
			cboxConnType.DisplayMember = "Value";
			cboxConnType.ValueMember = "Key";
			cboxConnType.DataBindings.Add("SelectedValue", channel, "ConnectionType");
			cboxConnType.SelectionChangeCommitted += cboxConnType_SelectionChangeCommitted;
			cboxManufacturer.DisplayMember = "Value";
			cboxManufacturer.ValueMember = "Key";
			cboxManufacturer.DataBindings.Add("SelectedValue", channel, "Manufacturer");
			cboxManufacturer.SelectedIndexChanged += cboxManufacturer_SelectedIndexChanged;
			cboxProtocol.DisplayMember = "Value";
			cboxProtocol.ValueMember = "Key";
			cboxProtocol.DataBindings.Add("SelectedValue", channel, "Protocol");
			txtName.DataBindings.Add("Text", channel, "Name", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			txtName.Validating += txtName_Validating;
			txtDescription.DataBindings.Add("Text", channel, "Description", formattingEnabled: true, DataSourceUpdateMode.OnPropertyChanged);
			base.ActiveControl = txtName;
			cboxProtocol.Validating += CboxProtocol_Validating;
			SelectProtocolType(channel.Manufacturer);
			cboxProtocol.SelectedValue = channel.Protocol;
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
				SelectConnectionType(channel.ConnectionType);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void cboxConnType_SelectionChangeCommitted(object? sender, EventArgs e)
	{
		try
		{
			if (sender is ComboBox comboBox && comboBox.SelectedValue is ConnectionType)
			{
				ConnectionType connectionType = (ConnectionType)comboBox.SelectedValue;
				if (connectionType == ConnectionType.Serial)
				{
					channel.Adapter = new SerialAdapter();
				}
				if (OnConnectionTypeChanged != null)
				{
					OnConnectionTypeChanged(connectionType);
				}
				SelectConnectionType(connectionType);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SelectConnectionType(ConnectionType connectionType)
	{
		if (cboxManufacturer != null && cboxManufacturer.SelectedValue is Manufacturer)
		{
			Manufacturer manufacturer_ = (Manufacturer)cboxManufacturer.SelectedValue;
			channel.ConnectionType = connectionType;
			SelectProtocolType(manufacturer_);
		}
	}

	private void cboxManufacturer_SelectedIndexChanged(object? sender, EventArgs e)
	{
		try
		{
			if (sender is ComboBox comboBox && comboBox.SelectedValue is Manufacturer)
			{
				Manufacturer manufacturer_ = (Manufacturer)comboBox.SelectedValue;
				SelectProtocolType(manufacturer_);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SelectProtocolType(Manufacturer manufacturer_0)
	{
		switch (manufacturer_0)
		{
		case Manufacturer.IPC:
			if (channel.ConnectionType == ConnectionType.Ethernet)
			{
				cboxProtocol.DataSource = ProtocolSource.IPC_TCP.ToList();
			}
			else
			{
				cboxProtocol.DataSource = ProtocolSource.IPC_SERIAL.ToList();
			}
			break;
		case Manufacturer.SIEMENS:
			if (channel.ConnectionType == ConnectionType.Ethernet)
			{
				cboxProtocol.DataSource = ProtocolSource.SIEMENS_TCP.ToList();
			}
			else
			{
				cboxProtocol.DataSource = ProtocolSource.SIEMENS_SERIAL.ToList();
			}
			break;
		default:
			throw new NotSupportedException();
		case Manufacturer.MITSUBISHI:
			if (channel.ConnectionType == ConnectionType.Ethernet)
			{
				cboxProtocol.DataSource = ProtocolSource.MISUBISHI_TCP.ToList();
			}
			else
			{
				cboxProtocol.DataSource = ProtocolSource.MISUBISHI_SERIAL.ToList();
			}
			break;
		case Manufacturer.OMRON:
			if (channel.ConnectionType == ConnectionType.Ethernet)
			{
				cboxProtocol.DataSource = ProtocolSource.OMRON_ETHERNET.ToList();
			}
			else
			{
				cboxProtocol.DataSource = ProtocolSource.OMRON_SERIAL.ToList();
			}
			break;
		case Manufacturer.PANASONIC:
			cboxProtocol.DataSource = ProtocolSource.PANASONIC.ToList();
			break;
		case Manufacturer.LS:
			cboxProtocol.DataSource = ProtocolSource.LSIS.ToList();
			break;
		case Manufacturer.DELTA:
			if (channel.ConnectionType == ConnectionType.Ethernet)
			{
				cboxProtocol.DataSource = ProtocolSource.DELTA_ETHERNET.ToList();
			}
			else
			{
				cboxProtocol.DataSource = ProtocolSource.DELTA_ETHERNET.ToList();
			}
			break;
		case Manufacturer.FATEK:
			cboxProtocol.DataSource = ProtocolSource.FATEK.ToList();
			break;
		case Manufacturer.VIGOR:
			cboxProtocol.DataSource = ProtocolSource.VIGOR.ToList();
			break;
		case Manufacturer.KEYENCE:
			if (channel.ConnectionType == ConnectionType.Ethernet)
			{
				cboxProtocol.DataSource = ProtocolSource.KEYENCE_ETHERNET.ToList();
			}
			else
			{
				cboxProtocol.DataSource = ProtocolSource.KEYENCE_SERIAL.ToList();
			}
			break;
		}
	}

	private void txtName_Validating(object? sender, CancelEventArgs e)
	{
		TextBox textBox = (TextBox)sender;
		errorProvider1.SetError(textBox, null);
		if (!string.IsNullOrEmpty(textBox.Text) && !string.IsNullOrWhiteSpace(textBox.Text))
		{
			e.Cancel = false;
			errorProvider1.SetError(textBox, null);
			errorProvider1.SetError(cboxProtocol, null);
			errorProvider1.Clear();
		}
		else
		{
			e.Cancel = true;
			textBox.Focus();
			errorProvider1.SetIconAlignment(txtName, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(txtName, 5);
			errorProvider1.SetError(textBox, "Please enter your channel name");
		}
	}

	private void CboxProtocol_Validating(object? sender, CancelEventArgs e)
	{
		ComboBox comboBox = (ComboBox)sender;
		errorProvider1.SetError(comboBox, null);
		if (comboBox.SelectedValue == null)
		{
			e.Cancel = true;
			comboBox.Focus();
			errorProvider1.SetIconAlignment(comboBox, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(comboBox, 5);
			errorProvider1.SetError(comboBox, "Please choose your protocol");
		}
		else
		{
			e.Cancel = false;
			errorProvider1.SetError(comboBox, null);
			errorProvider1.Clear();
		}
	}

	public bool ValidateProtocol()
	{
		if (cboxProtocol.SelectedValue == null)
		{
			cboxProtocol.Focus();
			errorProvider1.SetIconAlignment(cboxProtocol, ErrorIconAlignment.MiddleLeft);
			errorProvider1.SetIconPadding(cboxProtocol, 5);
			errorProvider1.SetError(cboxProtocol, "Please choose your protocol");
		}
		return errorProvider1.HasErrors;
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
		this.groupBox1 = new System.Windows.Forms.GroupBox();
		this.lblProtocol = new System.Windows.Forms.Label();
		this.cboxProtocol = new System.Windows.Forms.ComboBox();
		this.lblManufacturer = new System.Windows.Forms.Label();
		this.cboxManufacturer = new System.Windows.Forms.ComboBox();
		this.cboxConnType = new System.Windows.Forms.ComboBox();
		this.lblConnectionType = new System.Windows.Forms.Label();
		this.txtName = new System.Windows.Forms.TextBox();
		this.lblName = new System.Windows.Forms.Label();
		this.txtDescription = new System.Windows.Forms.TextBox();
		this.errorProvider1 = new System.Windows.Forms.ErrorProvider(this.components);
		this.gbDescription = new System.Windows.Forms.GroupBox();
		this.groupBox1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).BeginInit();
		this.gbDescription.SuspendLayout();
		base.SuspendLayout();
		this.groupBox1.Controls.Add(this.lblProtocol);
		this.groupBox1.Controls.Add(this.cboxProtocol);
		this.groupBox1.Controls.Add(this.lblManufacturer);
		this.groupBox1.Controls.Add(this.cboxManufacturer);
		this.groupBox1.Controls.Add(this.cboxConnType);
		this.groupBox1.Controls.Add(this.lblConnectionType);
		this.groupBox1.Controls.Add(this.txtName);
		this.groupBox1.Controls.Add(this.lblName);
		this.groupBox1.ForeColor = System.Drawing.Color.Navy;
		this.groupBox1.Location = new System.Drawing.Point(6, 7);
		this.groupBox1.Name = "groupBox1";
		this.groupBox1.Size = new System.Drawing.Size(328, 156);
		this.groupBox1.TabIndex = 81;
		this.groupBox1.TabStop = false;
		this.groupBox1.Text = "General";
		this.lblProtocol.AutoSize = true;
		this.lblProtocol.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblProtocol.Location = new System.Drawing.Point(7, 125);
		this.lblProtocol.Name = "lblProtocol";
		this.lblProtocol.Size = new System.Drawing.Size(55, 15);
		this.lblProtocol.TabIndex = 15;
		this.lblProtocol.Text = "Protocol:";
		this.cboxProtocol.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Suggest;
		this.cboxProtocol.FormattingEnabled = true;
		this.cboxProtocol.Location = new System.Drawing.Point(121, 121);
		this.cboxProtocol.Name = "cboxProtocol";
		this.cboxProtocol.Size = new System.Drawing.Size(200, 23);
		this.cboxProtocol.TabIndex = 3;
		this.lblManufacturer.AutoSize = true;
		this.lblManufacturer.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblManufacturer.Location = new System.Drawing.Point(7, 58);
		this.lblManufacturer.Name = "lblManufacturer";
		this.lblManufacturer.Size = new System.Drawing.Size(78, 15);
		this.lblManufacturer.TabIndex = 13;
		this.lblManufacturer.Text = "Manufacture:";
		this.cboxManufacturer.FormattingEnabled = true;
		this.cboxManufacturer.Location = new System.Drawing.Point(121, 54);
		this.cboxManufacturer.Name = "cboxManufacturer";
		this.cboxManufacturer.Size = new System.Drawing.Size(200, 23);
		this.cboxManufacturer.TabIndex = 1;
		this.cboxConnType.FormattingEnabled = true;
		this.cboxConnType.Location = new System.Drawing.Point(121, 87);
		this.cboxConnType.Name = "cboxConnType";
		this.cboxConnType.Size = new System.Drawing.Size(200, 23);
		this.cboxConnType.TabIndex = 2;
		this.lblConnectionType.AutoSize = true;
		this.lblConnectionType.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblConnectionType.Location = new System.Drawing.Point(5, 91);
		this.lblConnectionType.Name = "lblConnectionType";
		this.lblConnectionType.Size = new System.Drawing.Size(98, 15);
		this.lblConnectionType.TabIndex = 10;
		this.lblConnectionType.Text = "Connection type:";
		this.txtName.Location = new System.Drawing.Point(121, 22);
		this.txtName.Name = "txtName";
		this.txtName.Size = new System.Drawing.Size(200, 23);
		this.txtName.TabIndex = 0;
		this.lblName.AutoSize = true;
		this.lblName.ForeColor = System.Drawing.SystemColors.ControlText;
		this.lblName.Location = new System.Drawing.Point(7, 25);
		this.lblName.Name = "lblName";
		this.lblName.Size = new System.Drawing.Size(87, 15);
		this.lblName.TabIndex = 8;
		this.lblName.Text = "Channel name:";
		this.txtDescription.Location = new System.Drawing.Point(8, 22);
		this.txtDescription.Multiline = true;
		this.txtDescription.Name = "txtDescription";
		this.txtDescription.Size = new System.Drawing.Size(314, 154);
		this.txtDescription.TabIndex = 4;
		this.errorProvider1.ContainerControl = this;
		this.gbDescription.Controls.Add(this.txtDescription);
		this.gbDescription.ForeColor = System.Drawing.Color.Navy;
		this.gbDescription.Location = new System.Drawing.Point(6, 175);
		this.gbDescription.Name = "gbDescription";
		this.gbDescription.Size = new System.Drawing.Size(328, 186);
		this.gbDescription.TabIndex = 82;
		this.gbDescription.TabStop = false;
		this.gbDescription.Text = "Description";
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.Controls.Add(this.gbDescription);
		base.Controls.Add(this.groupBox1);
		base.Name = "GeneralControl";
		base.Size = new System.Drawing.Size(340, 364);
		this.groupBox1.ResumeLayout(false);
		this.groupBox1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.errorProvider1).EndInit();
		this.gbDescription.ResumeLayout(false);
		this.gbDescription.PerformLayout();
		base.ResumeLayout(false);
	}
}

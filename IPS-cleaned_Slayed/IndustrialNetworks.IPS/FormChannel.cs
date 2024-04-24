using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.Common.IndusCom;
using NetStudio.Common.Manager;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Local;

namespace NetStudio.IPS;

public class FormChannel : Form
{
	private Channel channel;

	public EventChannelChanged? OnChannelChanged;

	private GeneralControl generalControl;

	private SerialControl serialControl;

	private int position;

	private IContainer components;

	private Button btnNext;

	private Button btnPrevious;

	private DxTabControl dxTabControl1;

	private TabPage tabPageGeneral;

	private TabPage tabPageEthernet;

	private TabPage tabPageSerial;

	public FormChannel(Channel? channel = null)
	{
		InitializeComponent();
		try
		{
			if (channel != null)
			{
				Text = "Edit: Channel";
				this.channel = (Channel)channel.Clone();
				base.ActiveControl = btnNext;
			}
			else
			{
				Text = "Add new: Channel";
				this.channel = new Channel();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		OnInitializeTabPage();
	}

	private void OnInitializeTabPage()
	{
		try
		{
			generalControl = new GeneralControl();
			serialControl = new SerialControl();
			tabPageGeneral.Controls.Add(generalControl);
			tabPageSerial.Controls.Add(serialControl);
			if (channel == null)
			{
				return;
			}
			if (generalControl != null)
			{
				generalControl.SetChannel(channel);
			}
			bool mode = channel.ConnectionType == ConnectionType.Ethernet;
			SetEthernetMode(mode);
			GeneralControl obj = generalControl;
			obj.OnConnectionTypeChanged = (EventConnectionTypeChanged)Delegate.Combine(obj.OnConnectionTypeChanged, (EventConnectionTypeChanged)delegate(ConnectionType connectionType)
			{
				try
				{
					mode = connectionType == ConnectionType.Ethernet;
					SetEthernetMode(mode);
				}
				catch (Exception ex2)
				{
					MessageBox.Show(this, ex2.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			});
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnPrevious_Click(object sender, EventArgs e)
	{
		try
		{
			if (position > 0)
			{
				position--;
				if (channel.ConnectionType == ConnectionType.Serial)
				{
					btnNext.Text = "Next";
				}
			}
			dxTabControl1.SelectedIndex = position;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnNext_Click(object? sender, EventArgs e)
	{
		try
		{
			if (generalControl.HasErrors)
			{
				return;
			}
			if (btnNext.Text == "Save")
			{
				if (channel.ConnectionType == ConnectionType.Ethernet)
				{
					channel.Adapter = null;
				}
				if (Text.StartsWith("edit", StringComparison.OrdinalIgnoreCase))
				{
					EditHelper.EditChannel(channel);
					if (OnChannelChanged != null)
					{
						OnChannelChanged(channel, isAddnew: false);
					}
				}
				else
				{
					EditHelper.AddChannel(channel);
					if (OnChannelChanged != null)
					{
						OnChannelChanged(channel, isAddnew: true);
					}
				}
				base.DialogResult = DialogResult.OK;
				return;
			}
			if (position != 1 && channel.ConnectionType == ConnectionType.Serial)
			{
				position++;
				if (serialControl != null)
				{
					serialControl.SetChannel(channel);
				}
				btnNext.Text = "Save";
			}
			dxTabControl1.SelectedIndex = position;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
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

	private void SetEthernetMode(bool mode)
	{
		btnNext.Text = (mode ? "Save" : "Next");
		btnPrevious.Visible = !mode;
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.FormChannel));
		this.btnNext = new System.Windows.Forms.Button();
		this.btnPrevious = new System.Windows.Forms.Button();
		this.dxTabControl1 = new NetStudio.IPS.Controls.DxTabControl();
		this.tabPageGeneral = new System.Windows.Forms.TabPage();
		this.tabPageSerial = new System.Windows.Forms.TabPage();
		this.tabPageEthernet = new System.Windows.Forms.TabPage();
		this.dxTabControl1.SuspendLayout();
		base.SuspendLayout();
		this.btnNext.Location = new System.Drawing.Point(268, 376);
		this.btnNext.Name = "btnNext";
		this.btnNext.Size = new System.Drawing.Size(71, 30);
		this.btnNext.TabIndex = 16;
		this.btnNext.Text = "Next";
		this.btnNext.UseVisualStyleBackColor = true;
		this.btnNext.Click += new System.EventHandler(btnNext_Click);
		this.btnPrevious.Location = new System.Drawing.Point(191, 376);
		this.btnPrevious.Name = "btnPrevious";
		this.btnPrevious.Size = new System.Drawing.Size(71, 30);
		this.btnPrevious.TabIndex = 17;
		this.btnPrevious.Text = "Previous";
		this.btnPrevious.UseVisualStyleBackColor = true;
		this.btnPrevious.Click += new System.EventHandler(btnPrevious_Click);
		this.dxTabControl1.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
		this.dxTabControl1.Controls.Add(this.tabPageGeneral);
		this.dxTabControl1.Controls.Add(this.tabPageSerial);
		this.dxTabControl1.Controls.Add(this.tabPageEthernet);
		this.dxTabControl1.Dock = System.Windows.Forms.DockStyle.Top;
		this.dxTabControl1.ItemSize = new System.Drawing.Size(0, 1);
		this.dxTabControl1.Location = new System.Drawing.Point(0, 0);
		this.dxTabControl1.Margin = new System.Windows.Forms.Padding(0);
		this.dxTabControl1.Name = "dxTabControl1";
		this.dxTabControl1.Padding = new System.Drawing.Point(0, 0);
		this.dxTabControl1.SelectedIndex = 0;
		this.dxTabControl1.Size = new System.Drawing.Size(346, 377);
		this.dxTabControl1.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
		this.dxTabControl1.TabIndex = 18;
		this.tabPageGeneral.Location = new System.Drawing.Point(4, 5);
		this.tabPageGeneral.Name = "tabPageGeneral";
		this.tabPageGeneral.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageGeneral.Size = new System.Drawing.Size(338, 368);
		this.tabPageGeneral.TabIndex = 0;
		this.tabPageGeneral.Text = "General";
		this.tabPageGeneral.UseVisualStyleBackColor = true;
		this.tabPageSerial.Location = new System.Drawing.Point(4, 5);
		this.tabPageSerial.Name = "tabPageSerial";
		this.tabPageSerial.Size = new System.Drawing.Size(338, 368);
		this.tabPageSerial.TabIndex = 2;
		this.tabPageSerial.Text = "Serial";
		this.tabPageSerial.UseVisualStyleBackColor = true;
		this.tabPageEthernet.Location = new System.Drawing.Point(4, 5);
		this.tabPageEthernet.Name = "tabPageEthernet";
		this.tabPageEthernet.Padding = new System.Windows.Forms.Padding(3);
		this.tabPageEthernet.Size = new System.Drawing.Size(338, 368);
		this.tabPageEthernet.TabIndex = 1;
		this.tabPageEthernet.Text = "Ethernet";
		this.tabPageEthernet.UseVisualStyleBackColor = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(346, 414);
		base.Controls.Add(this.dxTabControl1);
		base.Controls.Add(this.btnPrevious);
		base.Controls.Add(this.btnNext);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormChannel";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Text = "Channel";
		this.dxTabControl1.ResumeLayout(false);
		base.ResumeLayout(false);
	}
}

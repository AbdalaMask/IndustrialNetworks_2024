using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Interfaces;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Entity;

namespace NetStudio.IPS.Monitor;

public class FormDeviceMonitor : Form
{
	private BindingSourceSync source;

	private IDriverServerManager _DriverManager;

	private IContainer components;

	private StatusStrip statusStrip;

	private ToolStripStatusLabel lblYoutube;

	private DataGrid dgvDv;

	private ContextMenuStrip contextMenuDevice;

	private ToolStripMenuItem mnActive;

	private ToolStripMenuItem mnDeactive;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem mnReconnect;

	private DataGridViewTextBoxColumn colStatus;

	private DataGridViewTextBoxColumn colName;

	private DataGridViewTextBoxColumn colStationNo;

	private DataGridViewCheckBoxColumn colActive;

	private DataGridViewCheckBoxColumn colAutoReconnect;

	private DataGridViewTextBoxColumn colBlockSize;

	private DataGridViewTextBoxColumn colByteOrder;

	private DataGridViewTextBoxColumn colDescription;

	public FormDeviceMonitor()
	{
		InitializeComponent();
	}

	public FormDeviceMonitor(IDriverServerManager manager)
		: this()
	{
		_DriverManager = manager;
	}

	private void FormDeviceMonitor_Load(object sender, EventArgs e)
	{
		try
		{
			dgvDv.CellFormatting += OnDataGridViewCellFormatting;
			dgvDv.SelectionChanged += OnDataGridViewSelectionChanged;
			source = new BindingSourceSync();
			source.DataSource = ClientDataSource.Devices.Values.ToList();
			dgvDv.DataSource = source;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void dgvDv_MouseDown(object sender, MouseEventArgs e)
	{
		OnDataGridViewSelectionChanged(sender, e);
	}

	private void OnDataGridViewSelectionChanged(object? sender, EventArgs e)
	{
		try
		{
			if (source != null && source.Current is Device device)
			{
				SetActive(device.Active);
				colStatus.Selected = false;
				colStatus.DefaultCellStyle.ForeColor = Color.White;
				if (device.Status != DeviceStatus.Connecting && device.Status != DeviceStatus.Connected)
				{
					colStatus.DefaultCellStyle.BackColor = Color.Red;
				}
				else
				{
					colStatus.DefaultCellStyle.BackColor = Color.Green;
				}
				dgvDv.RefreshEdit();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDataGridViewCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
	{
		try
		{
			if (dgvDv.Columns[e.ColumnIndex].Name == "colStatus" && e.Value != null)
			{
				e.CellStyle.ForeColor = Color.White;
				if ((DeviceStatus)Enum.Parse(typeof(DeviceStatus), e.Value.ToString()) == DeviceStatus.Connected)
				{
					e.CellStyle.BackColor = Color.Green;
				}
				else
				{
					e.CellStyle.BackColor = Color.Red;
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void lblYoutube_Click(object sender, EventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo("https://www.youtube.com/NetStudio")
			{
				UseShellExecute = true
			});
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnActive_Click(object sender, EventArgs e)
	{
		try
		{
			if (source.Current is Device device)
			{
				device.Active = true;
				dgvDv.RefreshEdit();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnDeactive_Click(object sender, EventArgs e)
	{
		try
		{
			if (source.Current is Device device)
			{
				device.Active = false;
				dgvDv.RefreshEdit();
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void SetActive(bool active)
	{
		mnActive.Visible = !active;
		mnDeactive.Visible = active;
	}

	private void mnReconnect_Click(object sender, EventArgs e)
	{
		try
		{
			if (source.Current is Device device)
			{
				device.Status = DeviceStatus.Reconnecting;
				dgvDv.RefreshEdit();
			}
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
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle8 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle9 = new System.Windows.Forms.DataGridViewCellStyle();
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.Monitor.FormDeviceMonitor));
		this.statusStrip = new System.Windows.Forms.StatusStrip();
		this.lblYoutube = new System.Windows.Forms.ToolStripStatusLabel();
		this.dgvDv = new NetStudio.IPS.Controls.DataGrid();
		this.contextMenuDevice = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnActive = new System.Windows.Forms.ToolStripMenuItem();
		this.mnDeactive = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.mnReconnect = new System.Windows.Forms.ToolStripMenuItem();
		this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colStationNo = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colActive = new System.Windows.Forms.DataGridViewCheckBoxColumn();
		this.colAutoReconnect = new System.Windows.Forms.DataGridViewCheckBoxColumn();
		this.colBlockSize = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colByteOrder = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.colDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.statusStrip.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvDv).BeginInit();
		this.contextMenuDevice.SuspendLayout();
		base.SuspendLayout();
		this.statusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.lblYoutube });
		this.statusStrip.Location = new System.Drawing.Point(0, 574);
		this.statusStrip.Name = "statusStrip";
		this.statusStrip.Size = new System.Drawing.Size(966, 22);
		this.statusStrip.TabIndex = 3;
		this.statusStrip.Text = "statusStrip1";
		this.lblYoutube.ForeColor = System.Drawing.Color.DarkSlateGray;
		this.lblYoutube.Name = "lblYoutube";
		this.lblYoutube.Size = new System.Drawing.Size(120, 17);
		this.lblYoutube.Text = "©Industrial Networks";
		this.lblYoutube.Click += new System.EventHandler(lblYoutube_Click);
		this.dgvDv.AllowUserToAddRows = false;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.DarkGray;
		this.dgvDv.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dgvDv.BackgroundColor = System.Drawing.Color.LightGray;
		this.dgvDv.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvDv.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
		this.dgvDv.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvDv.Columns.AddRange(this.colStatus, this.colName, this.colStationNo, this.colActive, this.colAutoReconnect, this.colBlockSize, this.colByteOrder, this.colDescription);
		this.dgvDv.ContextMenuStrip = this.contextMenuDevice;
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dgvDv.DefaultCellStyle = dataGridViewCellStyle3;
		this.dgvDv.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dgvDv.Location = new System.Drawing.Point(0, 0);
		this.dgvDv.MultiSelect = false;
		this.dgvDv.Name = "dgvDv";
		this.dgvDv.ReadOnly = true;
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvDv.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.LightGray;
		this.dgvDv.RowsDefaultCellStyle = dataGridViewCellStyle5;
		this.dgvDv.RowTemplate.Height = 25;
		this.dgvDv.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dgvDv.Size = new System.Drawing.Size(966, 574);
		this.dgvDv.TabIndex = 4;
		this.dgvDv.MouseDown += new System.Windows.Forms.MouseEventHandler(dgvDv_MouseDown);
		this.contextMenuDevice.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.mnActive, this.mnDeactive, this.toolStripSeparator1, this.mnReconnect });
		this.contextMenuDevice.Name = "contextMenuLogs";
		this.contextMenuDevice.Size = new System.Drawing.Size(131, 76);
		this.mnActive.Name = "mnActive";
		this.mnActive.Size = new System.Drawing.Size(130, 22);
		this.mnActive.Text = "Active";
		this.mnActive.Click += new System.EventHandler(mnActive_Click);
		this.mnDeactive.Name = "mnDeactive";
		this.mnDeactive.Size = new System.Drawing.Size(130, 22);
		this.mnDeactive.Text = "Deactive";
		this.mnDeactive.Click += new System.EventHandler(mnDeactive_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(127, 6);
		this.mnReconnect.Name = "mnReconnect";
		this.mnReconnect.Size = new System.Drawing.Size(130, 22);
		this.mnReconnect.Text = "&Reconnect";
		this.mnReconnect.Click += new System.EventHandler(mnReconnect_Click);
		this.colStatus.DataPropertyName = "Status";
		dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.colStatus.DefaultCellStyle = dataGridViewCellStyle6;
		this.colStatus.HeaderText = "Status";
		this.colStatus.MinimumWidth = 90;
		this.colStatus.Name = "colStatus";
		this.colStatus.ReadOnly = true;
		this.colStatus.Width = 90;
		this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.colName.DataPropertyName = "Name";
		this.colName.HeaderText = "Device Name";
		this.colName.MinimumWidth = 150;
		this.colName.Name = "colName";
		this.colName.ReadOnly = true;
		this.colName.Width = 150;
		this.colStationNo.DataPropertyName = "StationNo";
		dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colStationNo.DefaultCellStyle = dataGridViewCellStyle7;
		this.colStationNo.HeaderText = "Station No.";
		this.colStationNo.MinimumWidth = 60;
		this.colStationNo.Name = "colStationNo";
		this.colStationNo.ReadOnly = true;
		this.colStationNo.Width = 60;
		this.colActive.DataPropertyName = "Active";
		this.colActive.HeaderText = "Active";
		this.colActive.MinimumWidth = 50;
		this.colActive.Name = "colActive";
		this.colActive.ReadOnly = true;
		this.colActive.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.colActive.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.colActive.Width = 50;
		this.colAutoReconnect.DataPropertyName = "AutoReconnect";
		this.colAutoReconnect.HeaderText = "Auto reconnect";
		this.colAutoReconnect.MinimumWidth = 70;
		this.colAutoReconnect.Name = "colAutoReconnect";
		this.colAutoReconnect.ReadOnly = true;
		this.colAutoReconnect.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.colAutoReconnect.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.colAutoReconnect.Width = 70;
		this.colBlockSize.DataPropertyName = "BlockSize";
		dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.colBlockSize.DefaultCellStyle = dataGridViewCellStyle8;
		this.colBlockSize.HeaderText = "Block Size";
		this.colBlockSize.MinimumWidth = 90;
		this.colBlockSize.Name = "colBlockSize";
		this.colBlockSize.ReadOnly = true;
		this.colBlockSize.Width = 90;
		this.colByteOrder.DataPropertyName = "ByteOrder";
		dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.colByteOrder.DefaultCellStyle = dataGridViewCellStyle9;
		this.colByteOrder.HeaderText = "Byte Order";
		this.colByteOrder.MinimumWidth = 100;
		this.colByteOrder.Name = "colByteOrder";
		this.colByteOrder.ReadOnly = true;
		this.colDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.colDescription.DataPropertyName = "Description";
		this.colDescription.HeaderText = "Description";
		this.colDescription.Name = "colDescription";
		this.colDescription.ReadOnly = true;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(966, 596);
		base.Controls.Add(this.dgvDv);
		base.Controls.Add(this.statusStrip);
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "FormDeviceMonitor";
		this.Text = "Device monitoring";
		base.Load += new System.EventHandler(FormDeviceMonitor_Load);
		this.statusStrip.ResumeLayout(false);
		this.statusStrip.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvDv).EndInit();
		this.contextMenuDevice.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

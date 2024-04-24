using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS.HMI;

public class AlarmViewer : UserControl
{
	private BindingSourceRuntime bindingSource;

	private IContainer components;

	private ToolStrip toolStrip1;

	private DataGrid dataGrid1;

	private ToolStripButton btnAcknowledgeAll;

	private ToolStripButton btnAcknowledge;

	private DataGridViewTextBoxColumn txtDTime;

	private DataGridViewTextBoxColumn txtAlarmText;

	private DataGridViewTextBoxColumn txtAnalogAlarmClass;

	private DataGridViewTextBoxColumn txtAlarmStatus;

	public AlarmViewer()
	{
		InitializeComponent();
		dataGrid1.CellFormatting += OnDataGridCellFormatting;
		bindingSource = new BindingSourceRuntime();
		dataGrid1.DataSource = bindingSource;
	}

	private void OnDataGridCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
	{
		try
		{
			if (e.ColumnIndex >= 0 && ClientDataSource.AnalogAlarms.Count != 0 && e.RowIndex < ClientDataSource.AnalogAlarms.Count && ClientDataSource.AnalogAlarms[e.RowIndex] != null && dataGrid1.Columns[e.ColumnIndex].Name == "txtDTime")
			{
				e.Value = ClientDataSource.AnalogAlarms[e.RowIndex].DTime.ToString("MM/dd/yyyy hh:mm:ss tt");
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void OnInitialize()
	{
		bindingSource.DataSource = ClientDataSource.AnalogAlarms;
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
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle5 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle6 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle7 = new System.Windows.Forms.DataGridViewCellStyle();
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.btnAcknowledgeAll = new System.Windows.Forms.ToolStripButton();
		this.btnAcknowledge = new System.Windows.Forms.ToolStripButton();
		this.dataGrid1 = new NetStudio.IPS.Controls.DataGrid();
		this.txtDTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtAlarmText = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtAnalogAlarmClass = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtAlarmStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.toolStrip1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGrid1).BeginInit();
		base.SuspendLayout();
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.btnAcknowledgeAll, this.btnAcknowledge });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(763, 25);
		this.toolStrip1.TabIndex = 0;
		this.toolStrip1.Text = "toolStrip1";
		this.btnAcknowledgeAll.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.btnAcknowledgeAll.Image = NetStudio.IPS.Properties.Resources.Resources_512__checked;
		this.btnAcknowledgeAll.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnAcknowledgeAll.Name = "btnAcknowledgeAll";
		this.btnAcknowledgeAll.Size = new System.Drawing.Size(116, 22);
		this.btnAcknowledgeAll.Text = "Acknowledge All";
		this.btnAcknowledge.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.btnAcknowledge.Image = NetStudio.IPS.Properties.Resources.Resources_512__checked;
		this.btnAcknowledge.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnAcknowledge.Name = "btnAcknowledge";
		this.btnAcknowledge.Size = new System.Drawing.Size(99, 22);
		this.btnAcknowledge.Text = "Acknowledge";
		this.dataGrid1.AllowUserToAddRows = false;
		this.dataGrid1.AllowUserToDeleteRows = false;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.DarkGray;
		this.dataGrid1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dataGrid1.BackgroundColor = System.Drawing.Color.LightGray;
		this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGrid1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
		this.dataGrid1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGrid1.Columns.AddRange(this.txtDTime, this.txtAlarmText, this.txtAnalogAlarmClass, this.txtAlarmStatus);
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dataGrid1.DefaultCellStyle = dataGridViewCellStyle3;
		this.dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dataGrid1.Location = new System.Drawing.Point(0, 25);
		this.dataGrid1.MultiSelect = false;
		this.dataGrid1.Name = "dataGrid1";
		this.dataGrid1.ReadOnly = true;
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGrid1.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
		this.dataGrid1.RowHeadersWidth = 53;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.LightGray;
		this.dataGrid1.RowsDefaultCellStyle = dataGridViewCellStyle5;
		this.dataGrid1.RowTemplate.Height = 25;
		this.dataGrid1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGrid1.Size = new System.Drawing.Size(763, 144);
		this.dataGrid1.TabIndex = 1;
		this.dataGrid1.VirtualMode = true;
		this.txtDTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtDTime.DataPropertyName = "DTime";
		this.txtDTime.HeaderText = "Date time";
		this.txtDTime.MinimumWidth = 200;
		this.txtDTime.Name = "txtDTime";
		this.txtDTime.ReadOnly = true;
		this.txtDTime.Width = 200;
		this.txtAlarmText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.txtAlarmText.DataPropertyName = "AlarmText";
		this.txtAlarmText.HeaderText = "Message";
		this.txtAlarmText.Name = "txtAlarmText";
		this.txtAlarmText.ReadOnly = true;
		this.txtAnalogAlarmClass.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtAnalogAlarmClass.DataPropertyName = "AlarmClassesId";
		dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.txtAnalogAlarmClass.DefaultCellStyle = dataGridViewCellStyle6;
		this.txtAnalogAlarmClass.HeaderText = "Alarm type";
		this.txtAnalogAlarmClass.MinimumWidth = 125;
		this.txtAnalogAlarmClass.Name = "txtAnalogAlarmClass";
		this.txtAnalogAlarmClass.ReadOnly = true;
		this.txtAnalogAlarmClass.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.txtAnalogAlarmClass.Width = 125;
		this.txtAlarmStatus.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtAlarmStatus.DataPropertyName = "Status";
		dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.txtAlarmStatus.DefaultCellStyle = dataGridViewCellStyle7;
		this.txtAlarmStatus.HeaderText = "Status";
		this.txtAlarmStatus.MinimumWidth = 125;
		this.txtAlarmStatus.Name = "txtAlarmStatus";
		this.txtAlarmStatus.ReadOnly = true;
		this.txtAlarmStatus.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.txtAlarmStatus.Width = 125;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		base.Controls.Add(this.dataGrid1);
		base.Controls.Add(this.toolStrip1);
		base.Name = "AlarmViewer";
		base.Size = new System.Drawing.Size(763, 169);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGrid1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

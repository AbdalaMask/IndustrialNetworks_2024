using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Entity;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS.Controls;

public class AlarmViewer : UserControl
{
	private BindingSourceSync bindingSource;

	private IContainer components;

	private ToolStrip toolStrip1;

	private DataGrid dataGrid1;

	private DataGridViewTextBoxColumn DateTime;

	private DataGridViewTextBoxColumn Message;

	private DataGridViewTextBoxColumn AlarmType;

	private DataGridViewTextBoxColumn Status;

	private ToolStripButton btnAcknowledgeAll;

	private ToolStripButton btnAcknowledge;

	public AlarmViewer()
	{
		InitializeComponent();
		bindingSource = new BindingSourceSync();
		dataGrid1.DataSource = bindingSource;
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
		this.DateTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Message = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.AlarmType = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Status = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.toolStrip1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGrid1).BeginInit();
		base.SuspendLayout();
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[2] { this.btnAcknowledgeAll, this.btnAcknowledge });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(653, 25);
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
		this.dataGrid1.AllowUserToOrderColumns = true;
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
		this.dataGrid1.Columns.AddRange(this.DateTime, this.Message, this.AlarmType, this.Status);
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
		this.dataGrid1.Size = new System.Drawing.Size(653, 183);
		this.dataGrid1.TabIndex = 1;
		this.dataGrid1.VirtualMode = true;
		this.DateTime.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.DateTime.DataPropertyName = "DateTime";
		this.DateTime.HeaderText = "Date time";
		this.DateTime.MinimumWidth = 200;
		this.DateTime.Name = "DateTime";
		this.DateTime.ReadOnly = true;
		this.DateTime.Width = 200;
		this.Message.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.Message.DataPropertyName = "Message";
		this.Message.HeaderText = "Message";
		this.Message.Name = "Message";
		this.Message.ReadOnly = true;
		this.AlarmType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.AlarmType.DataPropertyName = "AlarmType";
		dataGridViewCellStyle6.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.AlarmType.DefaultCellStyle = dataGridViewCellStyle6;
		this.AlarmType.HeaderText = "Alarm type";
		this.AlarmType.MinimumWidth = 125;
		this.AlarmType.Name = "AlarmType";
		this.AlarmType.ReadOnly = true;
		this.AlarmType.Width = 125;
		this.Status.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Status.DataPropertyName = "Status";
		dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.Status.DefaultCellStyle = dataGridViewCellStyle7;
		this.Status.HeaderText = "Status";
		this.Status.MinimumWidth = 125;
		this.Status.Name = "Status";
		this.Status.ReadOnly = true;
		this.Status.Width = 125;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		base.Controls.Add(this.dataGrid1);
		base.Controls.Add(this.toolStrip1);
		base.Name = "AlarmViewer";
		base.Size = new System.Drawing.Size(653, 208);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.dataGrid1).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

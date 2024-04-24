using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.Historiant;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Local;

namespace NetStudio.IPS.Editor.Historiant;

public class FormLoggingCycle : Form
{
	private LoggingCycle _LoggingCycle;

	private BindingSource _BindingSource;

	public EventSelectLoggingCycleChanged? OnSelectLoggingCycleChanged;

	private const char ESC = '\u001b';

	private List<LoggingCycle> list_0;

	private IContainer components;

	private DataGrid dataGrid_0;

	private TextBox txtSearchBox;

	private GroupBox groupBox2;

	private Button btnClose;

	private ContextMenuStrip cmLoggingCycle;

	private ToolStripMenuItem mnDelete;

	private DataGridViewTextBoxColumn CycleTime;

	private DataGridViewComboBoxColumn CycleUnit;

	public FormLoggingCycle(bool isSelectMode = false)
	{
		InitializeComponent();
		dataGrid_0.ReadOnly = isSelectMode;
		btnClose.Visible = !isSelectMode;
	}

	private void FormLoggingCycle_Load(object sender, EventArgs e)
	{
		try
		{
			_LoggingCycle = new LoggingCycle();
			_BindingSource = new BindingSource();
			CycleTime.ValueType = typeof(short);
			CycleUnit.DataSource = Extensions.GetDictionary<CycleUnit>().ToList();
			CycleUnit.DisplayMember = "Value";
			CycleUnit.ValueMember = "Key";
			dataGrid_0.UserDeletingRow += OnDataGridViewUserDeletingRow;
			dataGrid_0.CellValidating += OnDgvCellValidating;
			dataGrid_0.RowValidating += OnDgvRowValidating;
			dataGrid_0.CellMouseDoubleClick += OnDgvCellMouseDoubleClick;
			dataGrid_0.CellValueChanged += OnDgv_CellValueChanged;
			dataGrid_0.DataSource = _BindingSource;
			txtSearchBox.Focus();
			LoadData();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnFormLoggingCycleFormClosing(object? sender, FormClosingEventArgs e)
	{
		try
		{
			if (e.Cancel)
			{
				MessageBox.Show(this, "Please press the ESC button before pressing the close window button.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void txtSearchBox_Enter(object sender, EventArgs e)
	{
		list_0 = (List<LoggingCycle>)_BindingSource.DataSource;
	}

	private void searchBox_TextChanged(object sender, EventArgs e)
	{
		_BindingSource.DataSource = list_0.FindAll((LoggingCycle loggingCycle_0) => loggingCycle_0.CycleTime.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || loggingCycle_0.CycleUnit.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase));
	}

	private void OnDgv_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
	{
		AppHelper.DataChanged = true;
	}

	private void OnDgvCellMouseDoubleClick(object? sender, DataGridViewCellMouseEventArgs e)
	{
		try
		{
			LoggingCycle cycle = (LoggingCycle)_BindingSource.Current;
			if (OnSelectLoggingCycleChanged != null)
			{
				OnSelectLoggingCycleChanged(cycle);
				base.DialogResult = DialogResult.OK;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvRowValidating(object? sender, DataGridViewCellCancelEventArgs e)
	{
		try
		{
			e.Cancel = false;
			if (e.RowIndex == dataGrid_0.NewRowIndex)
			{
				return;
			}
			dataGrid_0.Rows[e.RowIndex].ErrorText = string.Empty;
			List<LoggingCycle> source = (List<LoggingCycle>)_BindingSource.DataSource;
			LoggingCycle current = (LoggingCycle)_BindingSource.Current;
			if (current == null)
			{
				return;
			}
			if (current.CycleTime > 0)
			{
				if (source.FirstOrDefault((LoggingCycle loggingCycle_0) => loggingCycle_0.Id != current.Id && loggingCycle_0.CycleTime == current.CycleTime && loggingCycle_0.CycleUnit == current.CycleUnit) != null)
				{
					e.Cancel = true;
					dataGrid_0.Rows[e.RowIndex].ErrorText = "The cycle time is already exist.";
				}
				else if (current.Id == 0)
				{
					current.Id = e.RowIndex + 1;
				}
			}
			else
			{
				e.Cancel = true;
				dataGrid_0.Rows[e.RowIndex].ErrorText = "The cycle time must be greater than 0.";
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvCellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
	{
		try
		{
			uint result = 0u;
			dataGrid_0.Rows[e.RowIndex].ErrorText = string.Empty;
			if (e.ColumnIndex == 0)
			{
				if (!uint.TryParse(e.FormattedValue.ToString(), out result))
				{
					e.Cancel = true;
					dataGrid_0.Rows[e.RowIndex].ErrorText = "The value must be a non-negative integer.";
				}
				else if (result < 1)
				{
					e.Cancel = true;
					dataGrid_0.Rows[e.RowIndex].ErrorText = "The cycle time must be greater than 0.";
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDataGridViewUserDeletingRow(object? sender, DataGridViewRowCancelEventArgs e)
	{
		try
		{
			if (MessageBox.Show(this, "Do you want to delete it?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				AppHelper.DataChanged = true;
			}
			else
			{
				e.Cancel = true;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void LoadData()
	{
		try
		{
			if (EditHelper.IndusProtocol != null)
			{
				_BindingSource.DataSource = EditHelper.IndusProtocol.LoggingCycles;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnClose_Click(object sender, EventArgs e)
	{
		try
		{
			base.DialogResult = DialogResult.OK;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnDelete_Click(object sender, EventArgs e)
	{
		try
		{
			LoggingCycle loggingCycle = (LoggingCycle)_BindingSource.Current;
			if (loggingCycle != null && loggingCycle.Id > 0 && MessageBox.Show(this, "Do you want to delete it?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				_BindingSource.RemoveCurrent();
				AppHelper.DataChanged = true;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.Editor.Historiant.FormLoggingCycle));
		this.dataGrid_0 = new NetStudio.IPS.Controls.DataGrid();
		this.CycleTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.CycleUnit = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.cmLoggingCycle = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnDelete = new System.Windows.Forms.ToolStripMenuItem();
		this.txtSearchBox = new System.Windows.Forms.TextBox();
		this.groupBox2 = new System.Windows.Forms.GroupBox();
		this.btnClose = new System.Windows.Forms.Button();
		((System.ComponentModel.ISupportInitialize)this.dataGrid_0).BeginInit();
		this.cmLoggingCycle.SuspendLayout();
		this.groupBox2.SuspendLayout();
		base.SuspendLayout();
		dataGridViewCellStyle.BackColor = System.Drawing.Color.DarkGray;
		this.dataGrid_0.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dataGrid_0.BackgroundColor = System.Drawing.Color.LightGray;
		this.dataGrid_0.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGrid_0.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
		this.dataGrid_0.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dataGrid_0.Columns.AddRange(this.CycleTime, this.CycleUnit);
		this.dataGrid_0.ContextMenuStrip = this.cmLoggingCycle;
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dataGrid_0.DefaultCellStyle = dataGridViewCellStyle3;
		this.dataGrid_0.Location = new System.Drawing.Point(8, 59);
		this.dataGrid_0.MultiSelect = false;
		this.dataGrid_0.Name = "dgv";
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dataGrid_0.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
		this.dataGrid_0.RowHeadersWidth = 53;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.LightGray;
		this.dataGrid_0.RowsDefaultCellStyle = dataGridViewCellStyle5;
		this.dataGrid_0.RowTemplate.Height = 25;
		this.dataGrid_0.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dataGrid_0.Size = new System.Drawing.Size(362, 456);
		this.dataGrid_0.TabIndex = 2;
		this.dataGrid_0.VirtualMode = true;
		this.CycleTime.DataPropertyName = "CycleTime";
		this.CycleTime.HeaderText = "Cycle time";
		this.CycleTime.MinimumWidth = 120;
		this.CycleTime.Name = "CycleTime";
		this.CycleTime.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.CycleTime.Width = 120;
		this.CycleUnit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.CycleUnit.DataPropertyName = "CycleUnit";
		this.CycleUnit.DisplayStyleForCurrentCellOnly = true;
		this.CycleUnit.HeaderText = "Cycle unit";
		this.CycleUnit.Name = "CycleUnit";
		this.CycleUnit.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.CycleUnit.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.cmLoggingCycle.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.mnDelete });
		this.cmLoggingCycle.Name = "cmLoggingCycle";
		this.cmLoggingCycle.Size = new System.Drawing.Size(132, 26);
		this.cmLoggingCycle.Text = "Delete";
		this.mnDelete.Name = "mnDelete";
		this.mnDelete.ShortcutKeys = System.Windows.Forms.Keys.Delete;
		this.mnDelete.Size = new System.Drawing.Size(131, 22);
		this.mnDelete.Text = "&Delete";
		this.mnDelete.Click += new System.EventHandler(mnDelete_Click);
		this.txtSearchBox.Location = new System.Drawing.Point(7, 17);
		this.txtSearchBox.Name = "txtSearchBox";
		this.txtSearchBox.Size = new System.Drawing.Size(349, 23);
		this.txtSearchBox.TabIndex = 1;
		this.txtSearchBox.TextChanged += new System.EventHandler(searchBox_TextChanged);
		this.txtSearchBox.Enter += new System.EventHandler(txtSearchBox_Enter);
		this.groupBox2.Controls.Add(this.txtSearchBox);
		this.groupBox2.Location = new System.Drawing.Point(8, 4);
		this.groupBox2.Name = "groupBox2";
		this.groupBox2.Size = new System.Drawing.Size(362, 49);
		this.groupBox2.TabIndex = 0;
		this.groupBox2.TabStop = false;
		this.groupBox2.Text = "Search";
		this.btnClose.ForeColor = System.Drawing.SystemColors.ControlText;
		this.btnClose.Location = new System.Drawing.Point(287, 521);
		this.btnClose.Name = "btnClose";
		this.btnClose.Size = new System.Drawing.Size(83, 30);
		this.btnClose.TabIndex = 3;
		this.btnClose.Text = "Close";
		this.btnClose.UseVisualStyleBackColor = true;
		this.btnClose.Click += new System.EventHandler(btnClose_Click);
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(382, 563);
		base.Controls.Add(this.btnClose);
		base.Controls.Add(this.groupBox2);
		base.Controls.Add(this.dataGrid_0);
		base.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.MaximizeBox = false;
		base.MinimizeBox = false;
		base.Name = "FormLoggingCycle";
		base.ShowInTaskbar = false;
		this.Text = "Logging cycle";
		base.Load += new System.EventHandler(FormLoggingCycle_Load);
		((System.ComponentModel.ISupportInitialize)this.dataGrid_0).EndInit();
		this.cmLoggingCycle.ResumeLayout(false);
		this.groupBox2.ResumeLayout(false);
		this.groupBox2.PerformLayout();
		base.ResumeLayout(false);
	}
}

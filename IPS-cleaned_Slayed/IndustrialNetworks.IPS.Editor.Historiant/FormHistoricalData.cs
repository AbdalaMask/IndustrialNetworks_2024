using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.Historiant;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Local;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS.Editor.Historiant;

public class FormHistoricalData : Form
{
	private DataLog? _DataLog;

	private BindingSource _BsDataLog;

	private BindingSource _BsLoggingTag;

	private const char ESC = '\u001b';

	private const string ADD_NEW = "<Add new>";

	private bool keyESC;

	private IContainer components;

	private ToolStrip toolStrip1;

	private ToolStripButton btnAdd;

	private ToolStripButton btnEdit;

	private ToolStripButton btnDelete;

	private SplitContainer splitContainer1;

	private ToolStripButton btnLoggingCycle;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripLabel lblSearchBox;

	private ToolStripTextBox txtSearchBox;

	private ToolStripButton btnClearSearchBox;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripLabel lblTotalTags;

	private Panel panel1;

	private DataGrid dgvLogTags;

	private Label label1;

	private Panel panel2;

	private Label label2;

	private DataGrid dgvDataLogs;

	private ContextMenuStrip contextDatalogs;

	private ToolStripMenuItem mnDeleteDatalog;

	private ContextMenuStrip contextLogging;

	private ToolStripMenuItem mnDeleteLogging;

	private DataGridViewTextBoxColumn DataLogName;

	private DataGridViewComboBoxColumn StorageType;

	private DataGridViewTextBoxColumn DataRecordsPerLog;

	private DataGridViewTextBoxColumn Path;

	private DataGridViewTextBoxColumn ServerName;

	private DataGridViewTextBoxColumn Login;

	private DataGridViewTextBoxColumn Password;

	private DataGridViewCheckBoxColumn Active;

	private DataGridViewTextBoxColumn DLDescription;

	private DataGridViewTextBoxColumn txtDataLogsId;

	private DataGridViewTextBoxColumn LogName;

	private DataGridViewTextBoxColumn TagName;

	private DataGridViewComboBoxColumn cboxMode;

	private DataGridViewTextBoxColumn CycleName;

	private DataGridViewTextBoxColumn HighLimit;

	private DataGridViewTextBoxColumn LowLimit;

	private DataGridViewComboBoxColumn cboxLoggingLimit;

	private DataGridViewTextBoxColumn txDescription;

	private DataGridViewTextBoxColumn txtLoggingTagId;

	private DataGridViewTextBoxColumn Cycle;

	public FormHistoricalData()
	{
		InitializeComponent();
		AppHelper.ReloadLoggingTag = false;
		_BsDataLog = new BindingSource();
		_BsLoggingTag = new BindingSource();
	}

	private async void FormHistoricalData_Load(object sender, EventArgs e)
	{
		try
		{
			await WaitFormManager.ShowAsync(this, "Loading...");
			ApiResponse apiResponse = await LoadHistoricalData();
			if (apiResponse.Success)
			{
				HighLimit.ValueType = typeof(decimal);
				LowLimit.ValueType = typeof(decimal);
				StorageType.DataSource = Extensions.GetDictionary<StorageType>().ToList();
				StorageType.DisplayMember = "Value";
				StorageType.ValueMember = "Key";
				cboxMode.DataSource = Extensions.GetDictionary<LoggingMode>().ToList();
				cboxMode.DisplayMember = "Value";
				cboxMode.ValueMember = "Key";
				cboxLoggingLimit.DataSource = Extensions.GetDictionary<LoggingLimit>().ToList();
				cboxLoggingLimit.DisplayMember = "Value";
				cboxLoggingLimit.ValueMember = "Key";
				dgvDataLogs.DataSource = _BsDataLog;
				dgvDataLogs.DefaultValuesNeeded += OnDgvDataLogsDefaultValuesNeeded;
				dgvDataLogs.CellBeginEdit += OnDgvDataLogsCellBeginEdit;
				dgvDataLogs.CellValueChanged += OnDgvDataLogsCellValueChanged;
				dgvDataLogs.RowValidating += OnDgvDataLogsRowValidating;
				dgvDataLogs.EditingControlShowing += OnDgvDataLogsEditingControlShowing;
				dgvDataLogs.CellFormatting += OnDgvDataLogsCellFormatting;
				dgvDataLogs.SelectionChanged += OnDgvDataLogsSelectionChanged;
				dgvDataLogs.UserDeletingRow += OnDgvDataLogsUserDeletingRow;
				dgvLogTags.DataSource = _BsLoggingTag;
				dgvLogTags.DefaultValuesNeeded += OnDgvLogTagsDefaultValuesNeeded;
				dgvLogTags.CellValueChanged += OnDgvLogTags_CellValueChanged;
				dgvLogTags.CellMouseDoubleClick += dgvLogTags_CellMouseDoubleClick;
				dgvLogTags.RowValidating += OnDgvLogTagsRowValidating;
				dgvLogTags.UserDeletingRow += OnDgvLogTagsUserDeletingRow;
				await WaitFormManager.CloseAsync();
				return;
			}
			throw new Exception(apiResponse.Message);
		}
		catch (Exception ex)
		{
			await WaitFormManager.CloseAsync();
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private async Task<ApiResponse> LoadHistoricalData()
	{
		ApiResponse apiResponse = null;
		if (EditHelper.IndusProtocol == null)
		{
			apiResponse = await EditHelper.OnLoadProject();
		}
		if (EditHelper.IndusProtocol != null && (apiResponse == null || (apiResponse != null && apiResponse.Success)))
		{
			_BsDataLog.DataSource = EditHelper.IndusProtocol.DataLogs;
			if (EditHelper.IndusProtocol.DataLogs.Count > 0)
			{
				foreach (DataLog dataLog in EditHelper.IndusProtocol.DataLogs)
				{
					dataLog.OldDataLogName = dataLog.DataLogName;
				}
				_DataLog = EditHelper.IndusProtocol.DataLogs.First();
				_BsLoggingTag.DataSource = _DataLog.LoggingTags;
			}
		}
		return apiResponse ?? new ApiResponse
		{
			Message = "Read request successfully.",
			Success = true
		};
	}

	public void ValidateDataToSave()
	{
		if (EditHelper.IndusProtocol != null)
		{
			EditHelper.IndusProtocol.DataLogs = (List<DataLog>)_BsDataLog.DataSource;
		}
	}

	private void btnLoggingCycle_Click(object sender, EventArgs e)
	{
		try
		{
			FormLoggingCycle formLoggingCycle = new FormLoggingCycle();
			formLoggingCycle.StartPosition = FormStartPosition.CenterParent;
			formLoggingCycle.ShowInTaskbar = false;
			formLoggingCycle.ShowDialog();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvDataLogsDefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
	{
		if (e.Row.IsNewRow)
		{
			if (e.Row.Cells["txtDataLogsId"].Value == null || string.Format("{0}", e.Row.Cells["txtDataLogsId"].Value) == "0")
			{
				e.Row.Cells["txtDataLogsId"].Value = e.Row.Index + 1;
			}
			e.Row.Cells["DataLogName"].Value = "<Add new>";
			if (dgvDataLogs.Rows.Count > 1)
			{
				DataGridViewRow dataGridViewRow = dgvDataLogs.Rows[e.Row.Index - 1];
				e.Row.Cells["StorageType"].Value = dataGridViewRow.Cells["StorageType"].Value;
				e.Row.Cells["DataRecordsPerLog"].Value = dataGridViewRow.Cells["DataRecordsPerLog"].Value;
			}
		}
	}

	private void OnDgvDataLogsCellBeginEdit(object? sender, DataGridViewCellCancelEventArgs e)
	{
		try
		{
			if (e.ColumnIndex == 0)
			{
				DataGridViewCell dataGridViewCell = dgvDataLogs[e.ColumnIndex, e.RowIndex];
				if (dataGridViewCell.Value != null && dataGridViewCell.Value.ToString() == "<Add new>")
				{
					dgvDataLogs[e.ColumnIndex, e.RowIndex].Value = $"Datalog{_BsDataLog.Count}";
					if (dgvLogTags.ReadOnly)
					{
						dgvLogTags.ReadOnly = false;
					}
					dgvLogTags.AllowUserToAddRows = !dgvLogTags.ReadOnly;
					_DataLog = (DataLog)_BsDataLog.Current;
					_BsLoggingTag.DataSource = _DataLog.LoggingTags;
				}
			}
			if (e.RowIndex > 0 && e.ColumnIndex == 0)
			{
				int rowIndex = e.RowIndex - 1;
				dgvDataLogs[1, e.RowIndex].Value = dgvDataLogs[1, rowIndex].Value;
				dgvDataLogs[2, e.RowIndex].Value = dgvDataLogs[2, rowIndex].Value;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvDataLogsCellValueChanged(object? sender, DataGridViewCellEventArgs e)
	{
		try
		{
			AppHelper.DataChanged = true;
			DataGridView dataGridView = (DataGridView)sender;
			DataGridViewRow dataGridViewRow = dataGridView.Rows[e.RowIndex];
			DataGridViewCell dataGridViewCell = dataGridViewRow.Cells[dataGridView.Columns["StorageType"].Index];
			DataGridViewCell dataGridViewCell2 = dataGridViewRow.Cells[dataGridView.Columns["ServerName"].Index];
			DataGridViewCell dataGridViewCell3 = dataGridViewRow.Cells[dataGridView.Columns["Login"].Index];
			DataGridViewCell dataGridViewCell4 = dataGridViewRow.Cells[dataGridView.Columns["Password"].Index];
			DataGridViewCell dataGridViewCell5 = dataGridViewRow.Cells[dataGridView.Columns["Path"].Index];
			if (dataGridViewCell.Value == null)
			{
				return;
			}
			if ((StorageType)dataGridViewCell.Value == NetStudio.Common.Historiant.StorageType.Database)
			{
				dataGridViewCell5.Value = null;
				dataGridViewCell5.ReadOnly = true;
				dataGridViewCell2.ReadOnly = false;
				dataGridViewCell3.ReadOnly = false;
				dataGridViewCell4.ReadOnly = false;
				return;
			}
			dataGridViewCell2.Value = null;
			dataGridViewCell2.ReadOnly = true;
			dataGridViewCell3.Value = null;
			dataGridViewCell3.ReadOnly = true;
			dataGridViewCell4.Value = null;
			dataGridViewCell4.ReadOnly = true;
			if (dataGridViewCell5.Value == null)
			{
				dataGridViewCell5.Value = "C:\\DataLogs";
			}
			dataGridViewCell5.ReadOnly = false;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvDataLogsRowValidating(object? sender, DataGridViewCellCancelEventArgs e)
	{
		try
		{
			DataGridView dataGridView = (DataGridView)sender;
			if (e.RowIndex == dataGridView.NewRowIndex)
			{
				return;
			}
			e.Cancel = false;
			DataGridViewRow dataGridViewRow = dataGridView.Rows[e.RowIndex];
			DataGridViewCell dataGridViewCell = dataGridViewRow.Cells[dataGridView.Columns["StorageType"].Index];
			dataGridViewCell.ErrorText = string.Empty;
			DataGridViewCell dataGridViewCell2 = dataGridViewRow.Cells[dataGridView.Columns["ServerName"].Index];
			dataGridViewCell2.ErrorText = string.Empty;
			DataGridViewCell dataGridViewCell3 = dataGridViewRow.Cells[dataGridView.Columns["Login"].Index];
			dataGridViewCell3.ErrorText = string.Empty;
			DataGridViewCell dataGridViewCell4 = dataGridViewRow.Cells[dataGridView.Columns["Password"].Index];
			dataGridViewCell4.ErrorText = string.Empty;
			DataGridViewCell dataGridViewCell5 = dataGridViewRow.Cells[dataGridView.Columns["Path"].Index];
			dataGridViewCell5.ErrorText = string.Empty;
			if (dataGridViewCell.Value != null)
			{
				if ((StorageType)dataGridViewCell.Value == NetStudio.Common.Historiant.StorageType.Database)
				{
					dataGridViewCell5.Value = null;
					bool flag = Validate(dataGridView, dataGridViewCell2, "server name") && Validate(dataGridView, dataGridViewCell3, "login") && Validate(dataGridView, dataGridViewCell4, "password");
					e.Cancel = !flag;
				}
				else
				{
					dataGridViewCell2.Value = null;
					dataGridViewCell3.Value = null;
					dataGridViewCell4.Value = null;
					e.Cancel = !IsPath(dataGridView, dataGridViewCell5);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvDataLogsKeyPress(object? sender, KeyPressEventArgs e)
	{
		try
		{
			if (e.KeyChar == '\u001b')
			{
				_BsLoggingTag.ResetBindings(metadataChanged: true);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private bool Validate(DataGridView gridView, DataGridViewCell cell, string name)
	{
		if (cell.Value != null && (cell.Value == null || (!string.IsNullOrEmpty(cell.Value.ToString()) && !"<Double click>".Equals(cell.Value.ToString()))))
		{
			return true;
		}
		cell.ErrorText = "Please enter a " + name;
		return false;
	}

	private bool IsPath(DataGridView gridView, DataGridViewCell cell)
	{
		cell.ErrorText = string.Empty;
		if (cell.Value != null && (cell.Value == null || (!string.IsNullOrEmpty(cell.Value.ToString()) && !"<Double click>".Equals(cell.Value.ToString()))))
		{
			return true;
		}
		cell.ErrorText = "Please enter a path";
		return false;
	}

	private void OnDgvDataLogsEditingControlShowing(object? sender, DataGridViewEditingControlShowingEventArgs e)
	{
		if (dgvDataLogs.CurrentCell.ColumnIndex == 6)
		{
			TextBox textBox = (TextBox)e.Control;
			if (textBox != null)
			{
				textBox.PasswordChar = '*';
			}
		}
		else if (e.Control is TextBox)
		{
			TextBox textBox2 = (TextBox)e.Control;
			if (textBox2 != null)
			{
				textBox2.PasswordChar = '\0';
			}
		}
	}

	private void OnDgvDataLogsCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
	{
		if (e.ColumnIndex == 6 && e.Value != null)
		{
			e.Value = new string('*', e.Value.ToString().Length);
			dgvDataLogs.Rows[e.RowIndex].Cells[e.ColumnIndex].ToolTipText = "©Industrial Networks";
		}
	}

	private void OnDgvDataLogsSelectionChanged(object? sender, EventArgs e)
	{
        try
        {
			if (EditHelper.IndusProtocol == null)
			{
				return;
			}
            int num = _BsDataLog.IndexOf(_BsDataLog.Current);
            if (num < 0)
			{
				return;
			}
			_DataLog = EditHelper.IndusProtocol.DataLogs[num];
			if (_DataLog != null)
			{
				if (_DataLog.DataLogName == "<Add new>")
				{
					dgvLogTags.ReadOnly = true;
				}
				else
				{
					dgvLogTags.ReadOnly = false;
				}
				_BsLoggingTag.DataSource = _DataLog.LoggingTags;
				dgvLogTags.AllowUserToAddRows = !dgvLogTags.ReadOnly;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvDataLogsUserDeletingRow(object? sender, DataGridViewRowCancelEventArgs e)
	{
		try
		{
			DataLog dataLog = (DataLog)_BsDataLog.Current;
			if (MessageBox.Show(this, "Do you want to remove the DataLog(" + dataLog.DataLogName + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

	private void mnDeleteDatalog_Click(object sender, EventArgs e)
	{
		try
		{
			DataLog dataLog = (DataLog)_BsDataLog.Current;
			if (dataLog != null && DialogResult.Yes == MessageBox.Show(this, "Do you want to remove the DataLog(" + dataLog.DataLogName + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
			{
				_BsDataLog.RemoveCurrent();
				AppHelper.DataChanged = true;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void btnClearSearchBox_Click(object sender, EventArgs e)
	{
		try
		{
			txtSearchBox.Text = string.Empty;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void searchBox_TextChanged(object sender, EventArgs e)
	{
		if (_DataLog != null)
		{
			_BsLoggingTag.DataSource = _DataLog.LoggingTags.FindAll((LoggingTag loggingtg) => loggingtg.TagName.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || loggingtg.Mode.GetDescription().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || loggingtg.LoggingLimit.GetDescription().ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || loggingtg.CycleName.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || loggingtg.LowLimit.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || loggingtg.HighLimit.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase));
		}
	}

	private void OnDgvLogTags_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
	{
		AppHelper.DataChanged = true;
	}

	private void OnDgvLogTagsDefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
	{
		try
		{
			if (e.Row.IsNewRow)
			{
				if (e.Row.Cells["txtLoggingTagId"].Value == null || string.Format("{0}", e.Row.Cells["txtLoggingTagId"].Value) == "0")
				{
					e.Row.Cells["txtLoggingTagId"].Value = e.Row.Index + 1;
				}
				e.Row.Cells["TagName"].Value = "<Double click>";
				if (dgvDataLogs.Rows.Count > 1)
				{
					DataGridViewRow dataGridViewRow = dgvLogTags.Rows[e.Row.Index - 1];
					e.Row.Cells["cboxMode"].Value = dataGridViewRow.Cells["cboxMode"].Value;
					e.Row.Cells["HighLimit"].Value = dataGridViewRow.Cells["HighLimit"].Value;
					e.Row.Cells["LowLimit"].Value = dataGridViewRow.Cells["LowLimit"].Value;
					e.Row.Cells["cboxLoggingLimit"].Value = dataGridViewRow.Cells["cboxLoggingLimit"].Value;
					e.Row.Cells["Cycle"].Value = dataGridViewRow.Cells["Cycle"].Value;
					e.Row.Cells["CycleName"].Value = dataGridViewRow.Cells["CycleName"].Value;
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private void dgvLogTags_CellMouseDoubleClick(object? sender, DataGridViewCellMouseEventArgs e)
	{
		DataGridViewCellMouseEventArgs dataGridViewCellMouseEventArgs_0 = e;
		try
		{
			DataGridView gridView = (DataGridView)sender;
			DataGridViewRow dataGridViewRow_0 = gridView.Rows[dataGridViewCellMouseEventArgs_0.RowIndex];
			LoggingTag loggingTag = (LoggingTag)_BsLoggingTag.Current;
			if (dataGridViewCellMouseEventArgs_0.RowIndex < 0 || loggingTag == null)
			{
				return;
			}
			switch (dataGridViewCellMouseEventArgs_0.ColumnIndex)
			{
			case 1:
			{
				FormTagEditor obj2 = new FormTagEditor(isSelectMode: true)
				{
					StartPosition = FormStartPosition.CenterParent,
					ShowInTaskbar = false
				};
				obj2.OnSelectTagChanged = (EventSelectTagChanged)Delegate.Combine(obj2.OnSelectTagChanged, (EventSelectTagChanged)delegate(Tag tg)
				{
					AppHelper.DataChanged = true;
					gridView.BeginEdit(selectAll: true);
					if (loggingTag != null)
					{
						if (string.IsNullOrEmpty(loggingTag.TagName) || loggingTag.TagName == "<Double click>")
						{
							_BsLoggingTag.AddNew();
							_BsLoggingTag.RemoveAt(_BsLoggingTag.Count - 1);
						}
						loggingTag.ChannelId = tg.ChannelId;
						loggingTag.DeviceId = tg.DeviceId;
						loggingTag.GroupId = tg.GroupId;
						loggingTag.TagId = tg.Id;
						loggingTag.TagName = tg.FullName;
						int index = gridView.Columns["LogName"].Index;
						DataGridViewCell dataGridViewCell = dataGridViewRow_0.Cells[index];
						dataGridViewCell.ErrorText = string.Empty;
						dataGridViewCell.Value = loggingTag.LogName;
						if (!IsLogName(gridView, dataGridViewCell))
						{
							return false;
						}
						int index2 = gridView.Columns["TagName"].Index;
						DataGridViewCell dataGridViewCell2 = dataGridViewRow_0.Cells[index2];
						dataGridViewCell2.ErrorText = string.Empty;
						dataGridViewCell2.Value = loggingTag.TagName;
						if (!IsTagName(gridView, dataGridViewCell2))
						{
							return false;
						}
						int index3 = gridView.Columns["CycleName"].Index;
						DataGridViewCell dataGridViewCell3 = dataGridViewRow_0.Cells[index3];
						dataGridViewCell3.ErrorText = string.Empty;
						dataGridViewCell3.Value = loggingTag.CycleName;
						if (dataGridViewCellMouseEventArgs_0.RowIndex > 0)
						{
							dataGridViewCell3.Value = dataGridViewRow_0.Cells[gridView.Columns["CycleName"].Index - 1].Value;
						}
						if (dataGridViewCellMouseEventArgs_0.RowIndex > 0 && dataGridViewCellMouseEventArgs_0.ColumnIndex == 0)
						{
							int rowIndex = dataGridViewCellMouseEventArgs_0.RowIndex - 1;
							LoggingTag loggingTag2 = (LoggingTag)_BsLoggingTag[dataGridViewCellMouseEventArgs_0.RowIndex - 1];
							loggingTag.Cycle = loggingTag2.Cycle;
							gridView[1, dataGridViewCellMouseEventArgs_0.RowIndex].Value = gridView[1, rowIndex].Value;
							gridView[2, dataGridViewCellMouseEventArgs_0.RowIndex].Value = gridView[2, rowIndex].Value;
							gridView[3, dataGridViewCellMouseEventArgs_0.RowIndex].Value = gridView[3, rowIndex].Value;
							gridView[4, dataGridViewCellMouseEventArgs_0.RowIndex].Value = gridView[4, rowIndex].Value;
							gridView[5, dataGridViewCellMouseEventArgs_0.RowIndex].Value = gridView[5, rowIndex].Value;
						}
					}
					return true;
				});
				obj2.ShowDialog(this);
				break;
			}
			case 3:
			{
				if (loggingTag.Mode == LoggingMode.OnChange)
				{
					break;
				}
				FormLoggingCycle obj = new FormLoggingCycle(isSelectMode: true)
				{
					StartPosition = FormStartPosition.CenterParent,
					ShowInTaskbar = false
				};
				obj.OnSelectLoggingCycleChanged = (EventSelectLoggingCycleChanged)Delegate.Combine(obj.OnSelectLoggingCycleChanged, (EventSelectLoggingCycleChanged)delegate(LoggingCycle cycle)
				{
					AppHelper.DataChanged = true;
					if (loggingTag != null)
					{
						loggingTag.Cycle = cycle;
						dataGridViewRow_0.Cells[gridView.Columns["CycleName"].Index].ErrorText = string.Empty;
					}
				});
				obj.ShowDialog();
				break;
			}
			case 2:
			case 4:
			case 5:
				break;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvLogTagsRowValidating(object? sender, DataGridViewCellCancelEventArgs e)
	{
		try
		{
			if (e.RowIndex == dgvLogTags.NewRowIndex)
			{
				return;
			}
			e.Cancel = false;
			DataGridView dataGridView = (DataGridView)sender;
			DataGridViewRow dataGridViewRow = dataGridView.Rows[e.RowIndex];
			DataGridViewCell dataGridViewCell = dataGridViewRow.Cells[dataGridView.Columns["HighLimit"].Index];
			dataGridViewCell.ErrorText = string.Empty;
			DataGridViewCell dataGridViewCell2 = dataGridViewRow.Cells[dataGridView.Columns["LowLimit"].Index];
			dataGridViewCell2.ErrorText = string.Empty;
			dataGridView.Rows[dataGridViewCell.RowIndex].ErrorText = string.Empty;
			if (dataGridViewCell2.Value != null && decimal.Parse(dataGridViewCell.Value.ToString()) < decimal.Parse(dataGridViewCell2.Value.ToString()))
			{
				dataGridViewCell.ErrorText = "The high limit cannot be less than the low limit.";
				dataGridView.Rows[dataGridViewCell.RowIndex].ErrorText = "The high limit cannot be less than the low limit.";
				e.Cancel = true;
				return;
			}
			DataGridViewCell dataGridViewCell3 = dataGridViewRow.Cells[dataGridView.Columns["TagName"].Index];
			dataGridViewCell3.ErrorText = string.Empty;
			DataGridViewCell dataGridViewCell4 = dataGridViewRow.Cells[dataGridView.Columns["CycleName"].Index];
			dataGridViewCell4.ErrorText = string.Empty;
			DataGridViewCell dataGridViewCell5 = dataGridViewRow.Cells[dataGridView.Columns["cboxMode"].Index];
			if (dataGridViewCell5.Value != null && !keyESC)
			{
				if ((LoggingMode)dataGridViewCell5.Value == LoggingMode.Cyclic)
				{
					e.Cancel = !IsTagName(dataGridView, dataGridViewCell3) || !IsCycleName(dataGridView, dataGridViewCell4);
				}
			}
			else
			{
				keyESC = false;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvLogTagsUserDeletingRow(object? sender, DataGridViewRowCancelEventArgs e)
	{
		try
		{
			LoggingTag loggingTag = (LoggingTag)_BsLoggingTag.Current;
			if (MessageBox.Show(this, "Do you want to remove the tag(" + loggingTag.TagName + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

	private void mnDeleteLogging_Click(object sender, EventArgs e)
	{
		try
		{
			LoggingTag loggingTag = (LoggingTag)_BsLoggingTag.Current;
			if (loggingTag != null && MessageBox.Show(this, "Do you want to remove the tag(" + loggingTag.TagName + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				_BsLoggingTag.RemoveCurrent();
				AppHelper.DataChanged = true;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvLogTagsKeyPress(object? sender, KeyPressEventArgs e)
	{
		try
		{
			if (keyESC = e.KeyChar == '\u001b' && _BsLoggingTag.Count < 2)
			{
				_BsLoggingTag.ResetBindings(metadataChanged: true);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private bool IsLogName(DataGridView gridView, DataGridViewCell cell)
	{
		
		cell.ErrorText = string.Empty;
		gridView.Rows[cell.RowIndex].ErrorText = string.Empty;
		if (cell.Value != null && (cell.Value == null || (!string.IsNullOrEmpty(cell.Value.ToString()) && !"<Double click>".Equals(cell.Value.ToString()))))
		{
			List<LoggingTag> list = (List<LoggingTag>)_BsLoggingTag.DataSource;
			if (list != null && list.Count > 0 && list.Count((LoggingTag loggingtg) => loggingtg.LogName == $"{cell.Value}") > 1)
			{
				cell.ErrorText = "The log name already exists.";
				gridView.Rows[cell.RowIndex].ErrorText = "The log name already exists.";
				return false;
			}
			return true;
		}
		cell.ErrorText = "Please enter a log name.";
		gridView.Rows[cell.RowIndex].ErrorText = "Please enter a log name";
		return false;
	}

	private bool IsTagName(DataGridView gridView, DataGridViewCell cell)
	{
		
		cell.ErrorText = string.Empty;
		gridView.Rows[cell.RowIndex].ErrorText = string.Empty;
		if (cell.Value != null && (cell.Value == null || (!string.IsNullOrEmpty(cell.Value.ToString()) && !"<Double click>".Equals(cell.Value.ToString()))))
		{
			List<LoggingTag> list = (List<LoggingTag>)_BsLoggingTag.DataSource;
			if (list != null && list.Count > 0 && list.Count((LoggingTag loggingtg) => loggingtg.TagName == $"{cell.Value}") > 1)
			{
				cell.ErrorText = "The tag name already exists.";
				gridView.Rows[cell.RowIndex].ErrorText = "The tag name already exists.";
				return false;
			}
			return true;
		}
		cell.ErrorText = "Please enter a tag name.";
		gridView.Rows[cell.RowIndex].ErrorText = "Please enter a tag name";
		return false;
	}

	private bool IsCycleName(DataGridView gridView, DataGridViewCell cell)
	{
		cell.ErrorText = string.Empty;
		if (cell.Value != null && (cell.Value == null || (!string.IsNullOrEmpty(cell.Value.ToString()) && !"<Double click>".Equals(cell.Value.ToString()))))
		{
			return true;
		}
		cell.ErrorText = "Please enter a logging cycle.";
		return false;
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
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle10 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle11 = new System.Windows.Forms.DataGridViewCellStyle();
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.btnAdd = new System.Windows.Forms.ToolStripButton();
		this.btnEdit = new System.Windows.Forms.ToolStripButton();
		this.btnDelete = new System.Windows.Forms.ToolStripButton();
		this.btnLoggingCycle = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.lblSearchBox = new System.Windows.Forms.ToolStripLabel();
		this.txtSearchBox = new System.Windows.Forms.ToolStripTextBox();
		this.btnClearSearchBox = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.lblTotalTags = new System.Windows.Forms.ToolStripLabel();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.dgvDataLogs = new NetStudio.IPS.Controls.DataGrid();
		this.DataLogName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.StorageType = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.DataRecordsPerLog = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Path = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ServerName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Login = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Password = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Active = new System.Windows.Forms.DataGridViewCheckBoxColumn();
		this.DLDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtDataLogsId = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.contextDatalogs = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnDeleteDatalog = new System.Windows.Forms.ToolStripMenuItem();
		this.panel2 = new System.Windows.Forms.Panel();
		this.label2 = new System.Windows.Forms.Label();
		this.dgvLogTags = new NetStudio.IPS.Controls.DataGrid();
		this.contextLogging = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnDeleteLogging = new System.Windows.Forms.ToolStripMenuItem();
		this.panel1 = new System.Windows.Forms.Panel();
		this.label1 = new System.Windows.Forms.Label();
		this.LogName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.TagName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.cboxMode = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.CycleName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.HighLimit = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.LowLimit = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.cboxLoggingLimit = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.txDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtLoggingTagId = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Cycle = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.toolStrip1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvDataLogs).BeginInit();
		this.contextDatalogs.SuspendLayout();
		this.panel2.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvLogTags).BeginInit();
		this.contextLogging.SuspendLayout();
		this.panel1.SuspendLayout();
		base.SuspendLayout();
		this.toolStrip1.BackColor = System.Drawing.Color.Snow;
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[10] { this.btnAdd, this.btnEdit, this.btnDelete, this.btnLoggingCycle, this.toolStripSeparator1, this.lblSearchBox, this.txtSearchBox, this.btnClearSearchBox, this.toolStripSeparator2, this.lblTotalTags });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(1113, 25);
		this.toolStrip1.TabIndex = 0;
		this.toolStrip1.Text = "toolStrip1";
		this.btnAdd.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnAdd.Image = NetStudio.IPS.Properties.Resources.Resources_512_add_new;
		this.btnAdd.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnAdd.Name = "btnAdd";
		this.btnAdd.Size = new System.Drawing.Size(23, 22);
		this.btnAdd.Text = "toolStripButton1";
		this.btnAdd.ToolTipText = "Add new";
		this.btnAdd.Visible = false;
		this.btnEdit.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnEdit.Image = NetStudio.IPS.Properties.Resources.Resources_512_edit_tag;
		this.btnEdit.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnEdit.Name = "btnEdit";
		this.btnEdit.Size = new System.Drawing.Size(23, 22);
		this.btnEdit.Text = "toolStripButton2";
		this.btnEdit.ToolTipText = "Edit";
		this.btnEdit.Visible = false;
		this.btnDelete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnDelete.Image = NetStudio.IPS.Properties.Resources.Resources_512_remove;
		this.btnDelete.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(23, 22);
		this.btnDelete.Text = "Delete";
		this.btnDelete.Visible = false;
		this.btnLoggingCycle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnLoggingCycle.Image = NetStudio.IPS.Properties.Resources.Resources_512_cycle_time;
		this.btnLoggingCycle.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnLoggingCycle.Name = "btnLoggingCycle";
		this.btnLoggingCycle.Size = new System.Drawing.Size(23, 22);
		this.btnLoggingCycle.Text = "Logging cycle";
		this.btnLoggingCycle.Click += new System.EventHandler(btnLoggingCycle_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
		this.lblSearchBox.Name = "lblSearchBox";
		this.lblSearchBox.Size = new System.Drawing.Size(45, 22);
		this.lblSearchBox.Text = "Search:";
		this.txtSearchBox.Name = "txtSearchBox";
		this.txtSearchBox.Size = new System.Drawing.Size(350, 25);
		this.txtSearchBox.TextChanged += new System.EventHandler(searchBox_TextChanged);
		this.btnClearSearchBox.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
		this.btnClearSearchBox.Image = NetStudio.IPS.Properties.Resources.Resources_512_close;
		this.btnClearSearchBox.ImageTransparentColor = System.Drawing.Color.Magenta;
		this.btnClearSearchBox.Name = "btnClearSearchBox";
		this.btnClearSearchBox.Size = new System.Drawing.Size(23, 22);
		this.btnClearSearchBox.Text = "Clear";
		this.btnClearSearchBox.Click += new System.EventHandler(btnClearSearchBox_Click);
		this.toolStripSeparator2.Name = "toolStripSeparator2";
		this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
		this.lblTotalTags.Name = "lblTotalTags";
		this.lblTotalTags.Size = new System.Drawing.Size(69, 22);
		this.lblTotalTags.Text = "Total: 0 tags";
		this.splitContainer1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
		this.splitContainer1.Panel1.Controls.Add(this.dgvDataLogs);
		this.splitContainer1.Panel1.Controls.Add(this.panel2);
		this.splitContainer1.Panel2.Controls.Add(this.dgvLogTags);
		this.splitContainer1.Panel2.Controls.Add(this.panel1);
		this.splitContainer1.Size = new System.Drawing.Size(1113, 645);
		this.splitContainer1.SplitterDistance = 241;
		this.splitContainer1.TabIndex = 2;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.DarkGray;
		this.dgvDataLogs.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dgvDataLogs.BackgroundColor = System.Drawing.Color.LightGray;
		this.dgvDataLogs.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvDataLogs.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
		this.dgvDataLogs.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvDataLogs.Columns.AddRange(this.DataLogName, this.StorageType, this.DataRecordsPerLog, this.Path, this.ServerName, this.Login, this.Password, this.Active, this.DLDescription, this.txtDataLogsId);
		this.dgvDataLogs.ContextMenuStrip = this.contextDatalogs;
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dgvDataLogs.DefaultCellStyle = dataGridViewCellStyle3;
		this.dgvDataLogs.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dgvDataLogs.Location = new System.Drawing.Point(0, 28);
		this.dgvDataLogs.MultiSelect = false;
		this.dgvDataLogs.Name = "dgvDataLogs";
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvDataLogs.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
		this.dgvDataLogs.RowHeadersWidth = 53;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.LightGray;
		this.dgvDataLogs.RowsDefaultCellStyle = dataGridViewCellStyle5;
		this.dgvDataLogs.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dgvDataLogs.Size = new System.Drawing.Size(1109, 209);
		this.dgvDataLogs.TabIndex = 2;
		this.dgvDataLogs.VirtualMode = true;
		this.DataLogName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.DataLogName.DataPropertyName = "DataLogName";
		this.DataLogName.HeaderText = "Name";
		this.DataLogName.MinimumWidth = 150;
		this.DataLogName.Name = "DataLogName";
		this.DataLogName.ToolTipText = "Add new";
		this.DataLogName.Width = 150;
		this.StorageType.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.StorageType.DataPropertyName = "StorageType";
		this.StorageType.DisplayStyleForCurrentCellOnly = true;
		this.StorageType.HeaderText = "Storage type";
		this.StorageType.MinimumWidth = 120;
		this.StorageType.Name = "StorageType";
		this.StorageType.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.StorageType.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.StorageType.Width = 120;
		this.DataRecordsPerLog.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.DataRecordsPerLog.DataPropertyName = "DataRecordsPerLog";
		this.DataRecordsPerLog.HeaderText = "Data records per log";
		this.DataRecordsPerLog.MinimumWidth = 140;
		this.DataRecordsPerLog.Name = "DataRecordsPerLog";
		this.DataRecordsPerLog.Width = 140;
		this.Path.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Path.DataPropertyName = "Path";
		this.Path.HeaderText = "Path";
		this.Path.MinimumWidth = 120;
		this.Path.Name = "Path";
		this.Path.Width = 120;
		this.ServerName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.ServerName.DataPropertyName = "ServerName";
		this.ServerName.HeaderText = "Server name";
		this.ServerName.MinimumWidth = 100;
		this.ServerName.Name = "ServerName";
		this.Login.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Login.DataPropertyName = "Login";
		this.Login.HeaderText = "Login";
		this.Login.MinimumWidth = 100;
		this.Login.Name = "Login";
		this.Password.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.Password.DataPropertyName = "Password";
		this.Password.HeaderText = "Password";
		this.Password.MinimumWidth = 100;
		this.Password.Name = "Password";
		this.Password.ToolTipText = "Password";
		this.Active.DataPropertyName = "Active";
		this.Active.FillWeight = 70f;
		this.Active.HeaderText = "Active";
		this.Active.MinimumWidth = 70;
		this.Active.Name = "Active";
		this.Active.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.Active.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.Active.Width = 70;
		this.DLDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.DLDescription.DataPropertyName = "Description";
		this.DLDescription.HeaderText = "Description";
		this.DLDescription.Name = "DLDescription";
		this.txtDataLogsId.DataPropertyName = "Id";
		this.txtDataLogsId.HeaderText = "Id";
		this.txtDataLogsId.Name = "txtDataLogsId";
		this.txtDataLogsId.Visible = false;
		this.contextDatalogs.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.mnDeleteDatalog });
		this.contextDatalogs.Name = "contextDatalogs";
		this.contextDatalogs.Size = new System.Drawing.Size(108, 26);
		this.mnDeleteDatalog.Image = NetStudio.IPS.Properties.Resources.Resources_512_Delete;
		this.mnDeleteDatalog.Name = "mnDeleteDatalog";
		this.mnDeleteDatalog.Size = new System.Drawing.Size(107, 22);
		this.mnDeleteDatalog.Text = "&Delete";
		this.mnDeleteDatalog.Click += new System.EventHandler(mnDeleteDatalog_Click);
		this.panel2.Controls.Add(this.label2);
		this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
		this.panel2.Location = new System.Drawing.Point(0, 0);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(1109, 28);
		this.panel2.TabIndex = 1;
		this.label2.AutoSize = true;
		this.label2.Font = new System.Drawing.Font("Segoe UI Semibold", 10f, System.Drawing.FontStyle.Bold);
		this.label2.Location = new System.Drawing.Point(5, 4);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(68, 19);
		this.label2.TabIndex = 0;
		this.label2.Text = "Data logs";
		dataGridViewCellStyle6.BackColor = System.Drawing.Color.DarkGray;
		this.dgvLogTags.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
		this.dgvLogTags.BackgroundColor = System.Drawing.Color.LightGray;
		this.dgvLogTags.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle7.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle7.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvLogTags.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
		this.dgvLogTags.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvLogTags.Columns.AddRange(this.LogName, this.TagName, this.cboxMode, this.CycleName, this.HighLimit, this.LowLimit, this.cboxLoggingLimit, this.txDescription, this.txtLoggingTagId, this.Cycle);
		this.dgvLogTags.ContextMenuStrip = this.contextLogging;
		dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dgvLogTags.DefaultCellStyle = dataGridViewCellStyle8;
		this.dgvLogTags.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dgvLogTags.Location = new System.Drawing.Point(0, 28);
		this.dgvLogTags.MultiSelect = false;
		this.dgvLogTags.Name = "dgvLogTags";
		dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle9.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle9.Font = new System.Drawing.Font("Segoe UI", 9f);
		dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvLogTags.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
		this.dgvLogTags.RowHeadersWidth = 53;
		dataGridViewCellStyle10.BackColor = System.Drawing.Color.LightGray;
		this.dgvLogTags.RowsDefaultCellStyle = dataGridViewCellStyle10;
		this.dgvLogTags.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dgvLogTags.Size = new System.Drawing.Size(1109, 368);
		this.dgvLogTags.TabIndex = 3;
		this.dgvLogTags.VirtualMode = true;
		this.contextLogging.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.mnDeleteLogging });
		this.contextLogging.Name = "contextLogging";
		this.contextLogging.Size = new System.Drawing.Size(108, 26);
		this.mnDeleteLogging.Image = NetStudio.IPS.Properties.Resources.Resources_512_Delete;
		this.mnDeleteLogging.Name = "mnDeleteLogging";
		this.mnDeleteLogging.Size = new System.Drawing.Size(107, 22);
		this.mnDeleteLogging.Text = "&Delete";
		this.mnDeleteLogging.Click += new System.EventHandler(mnDeleteLogging_Click);
		this.panel1.Controls.Add(this.label1);
		this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
		this.panel1.Location = new System.Drawing.Point(0, 0);
		this.panel1.Name = "panel1";
		this.panel1.Size = new System.Drawing.Size(1109, 28);
		this.panel1.TabIndex = 0;
		this.label1.AutoSize = true;
		this.label1.Font = new System.Drawing.Font("Segoe UI Semibold", 10f, System.Drawing.FontStyle.Bold);
		this.label1.Location = new System.Drawing.Point(5, 4);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(90, 19);
		this.label1.TabIndex = 0;
		this.label1.Text = "Logging tags";
		this.LogName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.LogName.DataPropertyName = "LogName";
		this.LogName.HeaderText = "Name";
		this.LogName.MinimumWidth = 150;
		this.LogName.Name = "LogName";
		this.LogName.Width = 150;
		this.TagName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.TagName.DataPropertyName = "TagName";
		this.TagName.HeaderText = "Tag name";
		this.TagName.MinimumWidth = 150;
		this.TagName.Name = "TagName";
		this.TagName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.TagName.Width = 150;
		this.cboxMode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.cboxMode.DataPropertyName = "Mode";
		this.cboxMode.DisplayStyleForCurrentCellOnly = true;
		this.cboxMode.HeaderText = "Mode";
		this.cboxMode.MinimumWidth = 120;
		this.cboxMode.Name = "cboxMode";
		this.cboxMode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.cboxMode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.cboxMode.Width = 120;
		this.CycleName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.CycleName.DataPropertyName = "CycleName";
		dataGridViewCellStyle11.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
		this.CycleName.DefaultCellStyle = dataGridViewCellStyle11;
		this.CycleName.HeaderText = "Logging cycle";
		this.CycleName.MinimumWidth = 140;
		this.CycleName.Name = "CycleName";
		this.CycleName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.CycleName.Width = 140;
		this.HighLimit.DataPropertyName = "HighLimit";
		this.HighLimit.HeaderText = "High limit";
		this.HighLimit.Name = "HighLimit";
		this.LowLimit.DataPropertyName = "LowLimit";
		this.LowLimit.HeaderText = "Low limit";
		this.LowLimit.Name = "LowLimit";
		this.cboxLoggingLimit.DataPropertyName = "LoggingLimit";
		this.cboxLoggingLimit.DisplayStyleForCurrentCellOnly = true;
		this.cboxLoggingLimit.HeaderText = "Range for logging limits";
		this.cboxLoggingLimit.MinimumWidth = 170;
		this.cboxLoggingLimit.Name = "cboxLoggingLimit";
		this.cboxLoggingLimit.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.cboxLoggingLimit.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.cboxLoggingLimit.Width = 170;
		this.txDescription.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.txDescription.DataPropertyName = "Description";
		this.txDescription.HeaderText = "Description";
		this.txDescription.Name = "txDescription";
		this.txtLoggingTagId.DataPropertyName = "Id";
		this.txtLoggingTagId.HeaderText = "Id";
		this.txtLoggingTagId.Name = "txtLoggingTagId";
		this.txtLoggingTagId.Visible = false;
		this.Cycle.DataPropertyName = "Cycle";
		this.Cycle.HeaderText = "Cycle";
		this.Cycle.Name = "Cycle";
		this.Cycle.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(1113, 670);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.toolStrip1);
		base.Name = "FormHistoricalData";
		this.Text = "Historical data";
		base.Load += new System.EventHandler(FormHistoricalData_Load);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dgvDataLogs).EndInit();
		this.contextDatalogs.ResumeLayout(false);
		this.panel2.ResumeLayout(false);
		this.panel2.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvLogTags).EndInit();
		this.contextLogging.ResumeLayout(false);
		this.panel1.ResumeLayout(false);
		this.panel1.PerformLayout();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

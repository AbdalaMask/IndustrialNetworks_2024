using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.Alarms;
using NetStudio.Common.Manager;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Local;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS.Editor.Alarms;

public class FormAlarms : Form
{
	private BindingSource _bsAlarmClasses;

	private BindingSource _bsAnalogAlarms;

	private IContainer components;

	private DataGridViewTextBoxColumn AlarmName;

	private ToolStrip toolStrip1;

	private ToolStripLabel lblSearchBox;

	private ToolStripTextBox txtSearchBox;

	private ToolStripButton btnClearSearchBox;

	private ToolStripSeparator toolStripSeparator2;

	private ToolStripLabel lblTotalTags;

	private TabControl tcAlarm;

	private TabPage plDiscreteAlarms;

	private TabPage plAnalogAlarms;

	private DataGrid dgvAnalogAlarm;

	private TabPage plControllerAlarms;

	private TabPage plAlarmClasses;

	private DataGrid dgvAlarmClasses;

	private DataGrid dgvDiscreteAlarm;

	private DataGridViewTextBoxColumn txtDiscreteAcknowlegmentTag;

	private DataGridViewTextBoxColumn txtAnalogAcknowlegmentTag;

	private DataGridViewTextBoxColumn txtDiscreteId;

	private DataGridViewTextBoxColumn txtDiscreteAlarmName;

	private DataGridViewTextBoxColumn txtDiscreteAlarmText;

	private DataGridViewComboBoxColumn cboxDiscreteAlarmClasses;

	private DataGridViewTextBoxColumn txtDiscreteTriggerTag;

	private DataGridViewTextBoxColumn chkDiscreteTriggerBit;

	private DataGridViewTextBoxColumn DisplayName;

	private DataGridViewTextBoxColumn AlarmClassesId;

	private DataGridViewTextBoxColumn AlarmClassName;

	private DataGridViewTextBoxColumn StatusMachine;

	private DataGridViewTextBoxColumn EmailAddress;

	private DataGridViewColorPickerColumn BackgroundIncoming;

	private DataGridViewColorPickerColumn TextColorIncoming;

	private DataGridViewColorPickerColumn AcknowledgedColorIncoming;

	private DataGridViewColorPickerColumn BackgroundOutcoming;

	private DataGridViewColorPickerColumn TextColorOutComing;

	private DataGridViewColorPickerColumn AcknowledgedColorOutcoming;

	private DataGridViewTextBoxColumn txtAnalogId;

	private DataGridViewTextBoxColumn txtAnalogAlarmName;

	private DataGridViewTextBoxColumn txtAnalogAlarmText;

	private DataGridViewComboBoxColumn cboxAnalogAlarmClass;

	private DataGridViewTextBoxColumn txtAnalogTriggerTag;

	private DataGridViewTextBoxColumn txtAnalogLimit;

	private DataGridViewComboBoxColumn cboxAnalogLimitMode;

	private DataGridViewTextBoxColumn DTime;

	private DataGridViewTextBoxColumn AlarmStatus;

	private DataGridViewCheckBoxColumn Logging;

	public FormAlarms()
	{
		InitializeComponent();
		_bsAlarmClasses = new BindingSource();
		dgvAlarmClasses.DataSource = _bsAlarmClasses;
		_bsAnalogAlarms = new BindingSource();
		dgvAnalogAlarm.DataSource = _bsAnalogAlarms;
	}

	private async void FormAlarms_Load(object sender, EventArgs e)
	{
		try
		{
			await WaitFormManager.ShowAsync(this, "Loading...");
			ApiResponse apiResponse = await OnInitializeAlarm();
			if (apiResponse.Success)
			{
				cboxAnalogLimitMode.DataSource = Extensions.GetDictionary<LimitMode>().ToList();
				cboxAnalogLimitMode.DisplayMember = "Value";
				cboxAnalogLimitMode.ValueMember = "Key";
				cboxAnalogAlarmClass.DataSource = EditHelper.IndusProtocol.Alarms.AlarmClasses;
				cboxAnalogAlarmClass.DisplayMember = "Name";
				cboxAnalogAlarmClass.ValueMember = "Id";
				cboxAnalogAlarmClass.DataPropertyName = "AlarmClassesId";
				cboxDiscreteAlarmClasses.DataSource = EditHelper.IndusProtocol.Alarms.AlarmClasses;
				cboxDiscreteAlarmClasses.DisplayMember = "Name";
				cboxDiscreteAlarmClasses.ValueMember = "Id";
				_bsAlarmClasses.DataSource = EditHelper.IndusProtocol.Alarms.AlarmClasses;
				_bsAnalogAlarms.DataSource = EditHelper.IndusProtocol.Alarms.AnalogAlarms;
				dgvDiscreteAlarm.CellValueChanged += OnDgvDiscreteAlarm_CellValueChanged;
				dgvAnalogAlarm.Columns["txtAnalogLimit"].DefaultCellStyle.Format = "N";
				dgvAnalogAlarm.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
				dgvAnalogAlarm.DefaultValuesNeeded += OnDgvAnalogAlarm_DefaultValuesNeeded;
				dgvAnalogAlarm.CellMouseDoubleClick += OnDgvAnalogAlarm_CellMouseDoubleClick;
				dgvAnalogAlarm.CellValidating += OnDgvAnalogAlarm_CellValidating;
				dgvAnalogAlarm.CellValueChanged += OnDgvAnalogAlarm_CellValueChanged;
				dgvAnalogAlarm.RowValidating += ValidateByRow;
				dgvAnalogAlarm.UserDeletingRow += OnDgvAnalogAlarm_UserDeletingRow;
				dgvAlarmClasses.DefaultValuesNeeded += OnDgvAlarmClasses_DefaultValuesNeeded;
				dgvAlarmClasses.CellValueChanged += OnDgvAlarmClasses_CellValueChanged;
				dgvAlarmClasses.UserDeletingRow += OnDgvAlarmClasses_UserDeletingRow;
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

	private async Task<ApiResponse> OnInitializeAlarm()
	{
		ApiResponse apiResponse = null;
		if (EditHelper.IndusProtocol == null)
		{
			apiResponse = await EditHelper.OnLoadProject();
		}
		if (EditHelper.IndusProtocol != null && (apiResponse == null || (apiResponse != null && apiResponse.Success)))
		{
			EditHelper.IndusProtocol.Alarms = EditHelper.IndusProtocol.Alarms ?? new IpsAlarm();
			if (EditHelper.IndusProtocol.Alarms.AlarmClasses.Count == 0)
			{
				EditHelper.IndusProtocol.Alarms.AlarmClasses = new AlarmClass().GetAlarmClasses();
			}
		}
		return apiResponse ?? new ApiResponse
		{
			Message = "Read request successfully.",
			Success = true
		};
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
		if (EditHelper.IndusProtocol != null && EditHelper.IndusProtocol.Alarms != null)
		{
			_bsAnalogAlarms.DataSource = EditHelper.IndusProtocol.Alarms.AnalogAlarms.FindAll((AnalogAlarm analogAlarm_0) => analogAlarm_0.TagName.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || analogAlarm_0.LimitMode.GetDescription().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || analogAlarm_0.AlarmName.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || analogAlarm_0.AlarmText.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase));
		}
	}

	private void OnDgvDiscreteAlarm_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
	{
		AppHelper.DataChanged = true;
	}

	private void OnDgvAlarmClasses_DefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
	{
		if (e.Row.IsNewRow && (e.Row.Cells["AlarmClassesId"].Value == null || string.Format("{0}", e.Row.Cells["AlarmClassesId"].Value) == "0"))
		{
			e.Row.Cells["AlarmClassesId"].Value = e.Row.Index + 1;
		}
	}

	private void OnDgvAlarmClasses_UserDeletingRow(object? sender, DataGridViewRowCancelEventArgs e)
	{
		try
		{
			AlarmClass alarmClass = (AlarmClass)_bsAlarmClasses.Current;
			if (DialogResult.Yes == MessageBox.Show(this, "Do you want to remove the Alarm class(" + alarmClass.Name + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
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

	private void OnDgvAlarmClasses_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
	{
		AppHelper.DataChanged = true;
	}

	private void OnDgvAnalogAlarm_DefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
	{
		if (e.Row.IsNewRow)
		{
			if (e.Row.Cells["txtAnalogId"].Value == null || string.Format("{0}", e.Row.Cells["txtAnalogId"].Value) == "0")
			{
				e.Row.Cells["txtAnalogId"].Value = e.Row.Index + 1;
			}
			e.Row.Cells["txtAnalogTriggerTag"].Value = "<Double click>";
			if (dgvAnalogAlarm.Rows.Count > 1)
			{
				DataGridViewRow dataGridViewRow = dgvAnalogAlarm.Rows[e.Row.Index - 1];
				e.Row.Cells["cboxAnalogAlarmClass"].Value = dataGridViewRow.Cells["cboxAnalogAlarmClass"].Value;
				e.Row.Cells["cboxAnalogLimitMode"].Value = dataGridViewRow.Cells["cboxAnalogLimitMode"].Value;
				e.Row.Cells["Logging"].Value = dataGridViewRow.Cells["Logging"].Value;
			}
		}
	}

	private void OnDgvAnalogAlarm_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
	{
		try
		{
			e.Cancel = false;
			if (e.RowIndex == dgvAnalogAlarm.NewRowIndex)
			{
				return;
			}
			dgvAnalogAlarm[e.ColumnIndex, e.RowIndex].ErrorText = string.Empty;
			dgvAnalogAlarm.Rows[e.RowIndex].ErrorText = string.Empty;
			if (e.ColumnIndex == 0)
			{
				try
				{
					dgvAnalogAlarm[e.ColumnIndex, e.RowIndex].Value = decimal.Parse(e.FormattedValue.ToString());
					e.Cancel = false;
				}
				catch (Exception)
				{
					dgvAnalogAlarm[e.ColumnIndex, e.RowIndex].ErrorText = "The Id field must be numeric..";
					dgvAnalogAlarm.Rows[e.RowIndex].ErrorText = "The Id field must be numeric.";
					e.Cancel = true;
				}
			}
			else if (e.ColumnIndex == 5)
			{
				try
				{
					dgvAnalogAlarm[e.ColumnIndex, e.RowIndex].Value = decimal.Parse(e.FormattedValue.ToString());
					e.Cancel = false;
				}
				catch (Exception)
				{
					dgvAnalogAlarm[e.ColumnIndex, e.RowIndex].ErrorText = "The limit field must be numeric..";
					dgvAnalogAlarm.Rows[e.RowIndex].ErrorText = "The limit field must be numeric.";
					e.Cancel = true;
				}
			}
			AnalogAlarm analogAlarm = (AnalogAlarm)_bsAnalogAlarms[e.RowIndex];
			if (analogAlarm != null && analogAlarm.Id == 0)
			{
				analogAlarm.Id = e.RowIndex;
			}
		}
		catch (Exception ex3)
		{
			MessageBox.Show(this, ex3.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvAnalogAlarm_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
	{
		AppHelper.DataChanged = true;
	}

	private void OnDgvAnalogAlarm_CellMouseDoubleClick(object? sender, DataGridViewCellMouseEventArgs e)
	{
		DataGridViewCellMouseEventArgs dataGridViewCellMouseEventArgs_0 = e;
		try
		{
			DataGridView gridView = (DataGridView)sender;
			DataGridViewRow dataGridViewRow_0 = gridView.Rows[dataGridViewCellMouseEventArgs_0.RowIndex];
			AnalogAlarm analogAlarm = (AnalogAlarm)_bsAnalogAlarms.Current;
			if (dataGridViewCellMouseEventArgs_0.ColumnIndex != 4 || analogAlarm == null)
			{
				return;
			}
			FormTagEditor obj = new FormTagEditor(isSelectMode: true)
			{
				StartPosition = FormStartPosition.CenterParent,
				ShowInTaskbar = false
			};
			obj.OnSelectTagChanged = (EventSelectTagChanged)Delegate.Combine(obj.OnSelectTagChanged, (EventSelectTagChanged)delegate(Tag tg)
			{
				AppHelper.DataChanged = true;
				gridView.BeginEdit(selectAll: true);
				if (analogAlarm != null)
				{
					if (string.IsNullOrEmpty(analogAlarm.TagName) || analogAlarm.TagName == "<Double click>")
					{
						_bsAnalogAlarms.AddNew();
						_bsAnalogAlarms.RemoveAt(_bsAnalogAlarms.Count - 1);
					}
					analogAlarm.ChannelId = tg.ChannelId;
					analogAlarm.DeviceId = tg.DeviceId;
					analogAlarm.GroupId = tg.GroupId;
					analogAlarm.TagId = tg.Id;
					analogAlarm.TagName = tg.FullName;
					int index = gridView.Columns["txtAnalogTriggerTag"].Index;
					DataGridViewCell dataGridViewCell = dataGridViewRow_0.Cells[index];
					dataGridViewCell.ErrorText = string.Empty;
					gridView.Rows[dataGridViewCell.RowIndex].ErrorText = string.Empty;
					dataGridViewCell.Value = analogAlarm.TagName;
					if (dataGridViewCellMouseEventArgs_0.RowIndex > 0 && dataGridViewCellMouseEventArgs_0.ColumnIndex == 0)
					{
						int rowIndex = dataGridViewCellMouseEventArgs_0.RowIndex - 1;
						gridView[5, dataGridViewCellMouseEventArgs_0.RowIndex].Value = gridView[5, rowIndex].Value;
					}
				}
				return true;
			});
			obj.ShowDialog(this);
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvAnalogAlarm_UserDeletingRow(object? sender, DataGridViewRowCancelEventArgs e)
	{
		try
		{
			AnalogAlarm analogAlarm = (AnalogAlarm)_bsAnalogAlarms.Current;
			if (MessageBox.Show(this, $"Do you want to remove the alarm(Id={analogAlarm.Id})?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

	private void ValidateByRow(object? sender, DataGridViewCellCancelEventArgs e)
	{
		try
		{
			e.Cancel = false;
			if (e.RowIndex != dgvAnalogAlarm.NewRowIndex)
			{
				DataGridViewRow dataGridViewRow = dgvAnalogAlarm.Rows[e.RowIndex];
				DataGridViewCell dataGridViewCell = dataGridViewRow.Cells[dgvAnalogAlarm.Columns["txtAnalogId"].Index];
				dataGridViewCell.ErrorText = string.Empty;
				dgvAnalogAlarm.Rows[dataGridViewCell.RowIndex].ErrorText = string.Empty;
				DataGridViewCell dataGridViewCell2 = dataGridViewRow.Cells[dgvAnalogAlarm.Columns["txtAnalogAlarmName"].Index];
				dataGridViewCell2.ErrorText = string.Empty;
				dgvAnalogAlarm.Rows[dataGridViewCell2.RowIndex].ErrorText = string.Empty;
				DataGridViewCell dataGridViewCell3 = dataGridViewRow.Cells[dgvAnalogAlarm.Columns["txtAnalogAlarmText"].Index];
				dataGridViewCell3.ErrorText = string.Empty;
				dgvAnalogAlarm.Rows[dataGridViewCell3.RowIndex].ErrorText = string.Empty;
				DataGridViewCell dataGridViewCell4 = dataGridViewRow.Cells[dgvAnalogAlarm.Columns["txtAnalogLimit"].Index];
				dataGridViewCell4.ErrorText = string.Empty;
				dgvAnalogAlarm.Rows[dataGridViewCell4.RowIndex].ErrorText = string.Empty;
				DataGridViewCell dataGridViewCell5 = dataGridViewRow.Cells[dgvAnalogAlarm.Columns["cboxAnalogAlarmClass"].Index];
				dataGridViewCell5.ErrorText = string.Empty;
				dgvAnalogAlarm.Rows[dataGridViewCell5.RowIndex].ErrorText = string.Empty;
				DataGridViewCell dataGridViewCell6 = dataGridViewRow.Cells[dgvAnalogAlarm.Columns["txtAnalogTriggerTag"].Index];
				dataGridViewCell6.ErrorText = string.Empty;
				dgvAnalogAlarm.Rows[dataGridViewCell6.RowIndex].ErrorText = string.Empty;
				List<AnalogAlarm> analogAlarms = (List<AnalogAlarm>)_bsAnalogAlarms.DataSource;
				e.Cancel = !IsColumnId(analogAlarms, dgvAnalogAlarm, dataGridViewCell) || !IsAnalogAlarmName(analogAlarms, dgvAnalogAlarm, dataGridViewCell2) || !IsAnalogAlarmName(analogAlarms, dgvAnalogAlarm, dataGridViewCell2) || !IsAnalogAlarmText(analogAlarms, dgvAnalogAlarm, dataGridViewCell3) || !IsAnalogAlarmClass(analogAlarms, dgvAnalogAlarm, dataGridViewCell5) || !IsLimit(analogAlarms, dgvAnalogAlarm, dataGridViewCell4) || !IsTagName(analogAlarms, dgvAnalogAlarm, dataGridViewCell6, dataGridViewCell4);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private bool IsColumnId(List<AnalogAlarm> analogAlarms, DataGridView gridView, DataGridViewCell cell)
	{
		
		if (cell.Value != null && (cell.Value == null || !string.IsNullOrEmpty(cell.Value.ToString())))
		{
			if (analogAlarms != null && analogAlarms.Count > 0 && analogAlarms.Count((AnalogAlarm analogAlarm_0) => analogAlarm_0.Id == int.Parse($"{cell.Value}")) > 1)
			{
				cell.ErrorText = $"This Id={cell.Value} already exists.";
				gridView.Rows[cell.RowIndex].ErrorText = $"This Id={cell.Value} already exists.";
				return false;
			}
			return true;
		}
		cell.ErrorText = "Please enter Id.";
		gridView.Rows[cell.RowIndex].ErrorText = "Please enter Id.";
		return false;
	}

	private bool IsAnalogAlarmName(List<AnalogAlarm> analogAlarms, DataGridView gridView, DataGridViewCell cell)
	{
		
		if (cell.Value != null && (cell.Value == null || !string.IsNullOrEmpty(cell.Value.ToString())))
		{
			if (analogAlarms != null && analogAlarms.Count > 0 && analogAlarms.Count((AnalogAlarm analogAlarm_0) => analogAlarm_0.AlarmName == $"{cell.Value}") > 1)
			{
				cell.ErrorText = $"This name({cell.Value}) already exists.";
				gridView.Rows[cell.RowIndex].ErrorText = $"This name({cell.Value}) already exists.";
				return false;
			}
			return true;
		}
		cell.ErrorText = "Please enter name.";
		gridView.Rows[cell.RowIndex].ErrorText = "Please enter name.";
		return false;
	}

	private bool IsAnalogAlarmText(List<AnalogAlarm> analogAlarms, DataGridView gridView, DataGridViewCell cell)
	{
		if (cell.Value != null && (cell.Value == null || !string.IsNullOrEmpty(cell.Value.ToString())))
		{
			return true;
		}
		cell.ErrorText = "Please enter alarm text.";
		gridView.Rows[cell.RowIndex].ErrorText = "Please enter alarm text.";
		return false;
	}

	private bool IsAnalogAlarmClass(List<AnalogAlarm> analogAlarms, DataGridView gridView, DataGridViewCell cell)
	{
		if (cell.Value != null && (cell.Value == null || !(cell.Value.ToString() == "0")))
		{
			return true;
		}
		cell.ErrorText = "Please select alarm classes.";
		gridView.Rows[cell.RowIndex].ErrorText = "Please select alarm classes.";
		return false;
	}

	private bool IsLimit(List<AnalogAlarm> analogAlarms, DataGridView gridView, DataGridViewCell cell)
	{
		if (cell.Value != null && (cell.Value == null || !string.IsNullOrEmpty(cell.Value.ToString())))
		{
			return true;
		}
		cell.ErrorText = "Please enter limit.";
		gridView.Rows[cell.RowIndex].ErrorText = "Please enter limit.";
		return false;
	}

	private bool IsTagName(List<AnalogAlarm> analogAlarms, DataGridView gridView, DataGridViewCell tagNameCell, DataGridViewCell limitCell)
	{
		
		
		tagNameCell.ErrorText = string.Empty;
		gridView.Rows[tagNameCell.RowIndex].ErrorText = string.Empty;
		if (tagNameCell.Value != null && (tagNameCell.Value == null || (!string.IsNullOrEmpty(tagNameCell.Value.ToString()) && !"<Double click>".Equals(tagNameCell.Value.ToString()))))
		{
			if (analogAlarms != null && analogAlarms.Count > 0 && analogAlarms.Count((AnalogAlarm analogAlarm_0) => analogAlarm_0.TagName == $"{tagNameCell.Value}" && $"{analogAlarm_0.LimitValue}" == $"{limitCell.Value}") > 1)
			{
				tagNameCell.ErrorText = $"This tag name already exists with limit = {limitCell.Value}.";
				gridView.Rows[tagNameCell.RowIndex].ErrorText = $"This tag name already exists with limit = {limitCell.Value}.";
				return false;
			}
			return true;
		}
		tagNameCell.ErrorText = "Please enter a tag name.";
		gridView.Rows[tagNameCell.RowIndex].ErrorText = "Please enter a tag name";
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
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle12 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle13 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle14 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle15 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle16 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle17 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle18 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle19 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle20 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle21 = new System.Windows.Forms.DataGridViewCellStyle();
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle22 = new System.Windows.Forms.DataGridViewCellStyle();
		this.toolStrip1 = new System.Windows.Forms.ToolStrip();
		this.lblSearchBox = new System.Windows.Forms.ToolStripLabel();
		this.txtSearchBox = new System.Windows.Forms.ToolStripTextBox();
		this.btnClearSearchBox = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
		this.lblTotalTags = new System.Windows.Forms.ToolStripLabel();
		this.tcAlarm = new System.Windows.Forms.TabControl();
		this.plAnalogAlarms = new System.Windows.Forms.TabPage();
		this.dgvAnalogAlarm = new NetStudio.IPS.Controls.DataGrid();
		this.plDiscreteAlarms = new System.Windows.Forms.TabPage();
		this.dgvDiscreteAlarm = new NetStudio.IPS.Controls.DataGrid();
		this.txtDiscreteId = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtDiscreteAlarmName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtDiscreteAlarmText = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.cboxDiscreteAlarmClasses = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.txtDiscreteTriggerTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.chkDiscreteTriggerBit = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.plControllerAlarms = new System.Windows.Forms.TabPage();
		this.plAlarmClasses = new System.Windows.Forms.TabPage();
		this.dgvAlarmClasses = new NetStudio.IPS.Controls.DataGrid();
		this.DisplayName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.AlarmClassesId = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.AlarmClassName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.StatusMachine = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.EmailAddress = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.BackgroundIncoming = new NetStudio.IPS.Controls.DataGridViewColorPickerColumn();
		this.TextColorIncoming = new NetStudio.IPS.Controls.DataGridViewColorPickerColumn();
		this.AcknowledgedColorIncoming = new NetStudio.IPS.Controls.DataGridViewColorPickerColumn();
		this.BackgroundOutcoming = new NetStudio.IPS.Controls.DataGridViewColorPickerColumn();
		this.TextColorOutComing = new NetStudio.IPS.Controls.DataGridViewColorPickerColumn();
		this.AcknowledgedColorOutcoming = new NetStudio.IPS.Controls.DataGridViewColorPickerColumn();
		this.txtAnalogId = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtAnalogAlarmName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtAnalogAlarmText = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.cboxAnalogAlarmClass = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.txtAnalogTriggerTag = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtAnalogLimit = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.cboxAnalogLimitMode = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.DTime = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.AlarmStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.Logging = new System.Windows.Forms.DataGridViewCheckBoxColumn();
		this.toolStrip1.SuspendLayout();
		this.tcAlarm.SuspendLayout();
		this.plAnalogAlarms.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvAnalogAlarm).BeginInit();
		this.plDiscreteAlarms.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvDiscreteAlarm).BeginInit();
		this.plAlarmClasses.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvAlarmClasses).BeginInit();
		base.SuspendLayout();
		this.toolStrip1.BackColor = System.Drawing.Color.Snow;
		this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[5] { this.lblSearchBox, this.txtSearchBox, this.btnClearSearchBox, this.toolStripSeparator2, this.lblTotalTags });
		this.toolStrip1.Location = new System.Drawing.Point(0, 0);
		this.toolStrip1.Name = "toolStrip1";
		this.toolStrip1.Size = new System.Drawing.Size(957, 25);
		this.toolStrip1.TabIndex = 1;
		this.toolStrip1.Text = "toolStrip1";
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
		this.lblTotalTags.Visible = false;
		this.tcAlarm.Controls.Add(this.plAnalogAlarms);
		this.tcAlarm.Controls.Add(this.plDiscreteAlarms);
		this.tcAlarm.Controls.Add(this.plControllerAlarms);
		this.tcAlarm.Controls.Add(this.plAlarmClasses);
		this.tcAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tcAlarm.Location = new System.Drawing.Point(0, 25);
		this.tcAlarm.Name = "tcAlarm";
		this.tcAlarm.SelectedIndex = 0;
		this.tcAlarm.Size = new System.Drawing.Size(957, 570);
		this.tcAlarm.TabIndex = 2;
		this.plAnalogAlarms.Controls.Add(this.dgvAnalogAlarm);
		this.plAnalogAlarms.Location = new System.Drawing.Point(4, 24);
		this.plAnalogAlarms.Name = "plAnalogAlarms";
		this.plAnalogAlarms.Padding = new System.Windows.Forms.Padding(3);
		this.plAnalogAlarms.Size = new System.Drawing.Size(949, 542);
		this.plAnalogAlarms.TabIndex = 1;
		this.plAnalogAlarms.Text = "Analog alarms";
		this.plAnalogAlarms.UseVisualStyleBackColor = true;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.DarkGray;
		this.dgvAnalogAlarm.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dgvAnalogAlarm.BackgroundColor = System.Drawing.Color.LightGray;
		this.dgvAnalogAlarm.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvAnalogAlarm.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
		this.dgvAnalogAlarm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvAnalogAlarm.Columns.AddRange(this.txtAnalogId, this.txtAnalogAlarmName, this.txtAnalogAlarmText, this.cboxAnalogAlarmClass, this.txtAnalogTriggerTag, this.txtAnalogLimit, this.cboxAnalogLimitMode, this.DTime, this.AlarmStatus, this.Logging);
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dgvAnalogAlarm.DefaultCellStyle = dataGridViewCellStyle3;
		this.dgvAnalogAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dgvAnalogAlarm.Location = new System.Drawing.Point(3, 3);
		this.dgvAnalogAlarm.MultiSelect = false;
		this.dgvAnalogAlarm.Name = "dgvAnalogAlarm";
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvAnalogAlarm.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
		this.dgvAnalogAlarm.RowHeadersWidth = 53;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.LightGray;
		this.dgvAnalogAlarm.RowsDefaultCellStyle = dataGridViewCellStyle5;
		this.dgvAnalogAlarm.RowTemplate.Height = 25;
		this.dgvAnalogAlarm.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dgvAnalogAlarm.Size = new System.Drawing.Size(943, 536);
		this.dgvAnalogAlarm.TabIndex = 6;
		this.dgvAnalogAlarm.VirtualMode = true;
		this.plDiscreteAlarms.Controls.Add(this.dgvDiscreteAlarm);
		this.plDiscreteAlarms.Location = new System.Drawing.Point(4, 24);
		this.plDiscreteAlarms.Name = "plDiscreteAlarms";
		this.plDiscreteAlarms.Padding = new System.Windows.Forms.Padding(3);
		this.plDiscreteAlarms.Size = new System.Drawing.Size(949, 542);
		this.plDiscreteAlarms.TabIndex = 0;
		this.plDiscreteAlarms.Text = "Discrete alarms";
		this.plDiscreteAlarms.UseVisualStyleBackColor = true;
		dataGridViewCellStyle6.BackColor = System.Drawing.Color.DarkGray;
		this.dgvDiscreteAlarm.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle6;
		this.dgvDiscreteAlarm.BackgroundColor = System.Drawing.Color.LightGray;
		this.dgvDiscreteAlarm.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle7.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle7.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle7.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle7.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle7.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle7.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle7.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvDiscreteAlarm.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle7;
		this.dgvDiscreteAlarm.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvDiscreteAlarm.Columns.AddRange(this.txtDiscreteId, this.txtDiscreteAlarmName, this.txtDiscreteAlarmText, this.cboxDiscreteAlarmClasses, this.txtDiscreteTriggerTag, this.chkDiscreteTriggerBit);
		dataGridViewCellStyle8.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle8.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle8.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle8.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle8.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle8.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle8.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dgvDiscreteAlarm.DefaultCellStyle = dataGridViewCellStyle8;
		this.dgvDiscreteAlarm.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dgvDiscreteAlarm.Location = new System.Drawing.Point(3, 3);
		this.dgvDiscreteAlarm.MultiSelect = false;
		this.dgvDiscreteAlarm.Name = "dgvDiscreteAlarm";
		dataGridViewCellStyle9.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle9.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle9.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle9.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle9.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle9.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvDiscreteAlarm.RowHeadersDefaultCellStyle = dataGridViewCellStyle9;
		this.dgvDiscreteAlarm.RowHeadersWidth = 53;
		dataGridViewCellStyle10.BackColor = System.Drawing.Color.LightGray;
		this.dgvDiscreteAlarm.RowsDefaultCellStyle = dataGridViewCellStyle10;
		this.dgvDiscreteAlarm.RowTemplate.Height = 25;
		this.dgvDiscreteAlarm.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dgvDiscreteAlarm.Size = new System.Drawing.Size(943, 536);
		this.dgvDiscreteAlarm.TabIndex = 7;
		this.dgvDiscreteAlarm.VirtualMode = true;
		this.txtDiscreteId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtDiscreteId.DataPropertyName = "Id";
		this.txtDiscreteId.HeaderText = "Id";
		this.txtDiscreteId.MinimumWidth = 60;
		this.txtDiscreteId.Name = "txtDiscreteId";
		this.txtDiscreteId.Width = 60;
		this.txtDiscreteAlarmName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtDiscreteAlarmName.DataPropertyName = "AlarmName";
		this.txtDiscreteAlarmName.HeaderText = "Name";
		this.txtDiscreteAlarmName.MinimumWidth = 150;
		this.txtDiscreteAlarmName.Name = "txtDiscreteAlarmName";
		this.txtDiscreteAlarmName.Width = 150;
		this.txtDiscreteAlarmText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.txtDiscreteAlarmText.DataPropertyName = "AlarmText";
		this.txtDiscreteAlarmText.HeaderText = "Alarm text";
		this.txtDiscreteAlarmText.Name = "txtDiscreteAlarmText";
		this.cboxDiscreteAlarmClasses.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.cboxDiscreteAlarmClasses.DataPropertyName = "AlarmClassId";
		this.cboxDiscreteAlarmClasses.DisplayStyleForCurrentCellOnly = true;
		this.cboxDiscreteAlarmClasses.HeaderText = "Alarm classes";
		this.cboxDiscreteAlarmClasses.MinimumWidth = 125;
		this.cboxDiscreteAlarmClasses.Name = "cboxDiscreteAlarmClasses";
		this.cboxDiscreteAlarmClasses.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.cboxDiscreteAlarmClasses.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.cboxDiscreteAlarmClasses.Width = 125;
		this.txtDiscreteTriggerTag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtDiscreteTriggerTag.DataPropertyName = "TagName";
		this.txtDiscreteTriggerTag.HeaderText = "Trigger tag";
		this.txtDiscreteTriggerTag.MinimumWidth = 100;
		this.txtDiscreteTriggerTag.Name = "txtDiscreteTriggerTag";
		this.txtDiscreteTriggerTag.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.chkDiscreteTriggerBit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.chkDiscreteTriggerBit.DataPropertyName = "Limit";
		this.chkDiscreteTriggerBit.HeaderText = "Trigger bit";
		this.chkDiscreteTriggerBit.MinimumWidth = 80;
		this.chkDiscreteTriggerBit.Name = "chkDiscreteTriggerBit";
		this.chkDiscreteTriggerBit.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.chkDiscreteTriggerBit.Width = 85;
		this.plControllerAlarms.Location = new System.Drawing.Point(4, 24);
		this.plControllerAlarms.Name = "plControllerAlarms";
		this.plControllerAlarms.Padding = new System.Windows.Forms.Padding(3);
		this.plControllerAlarms.Size = new System.Drawing.Size(949, 542);
		this.plControllerAlarms.TabIndex = 3;
		this.plControllerAlarms.Text = "Controller alarms";
		this.plControllerAlarms.UseVisualStyleBackColor = true;
		this.plAlarmClasses.Controls.Add(this.dgvAlarmClasses);
		this.plAlarmClasses.Location = new System.Drawing.Point(4, 24);
		this.plAlarmClasses.Name = "plAlarmClasses";
		this.plAlarmClasses.Padding = new System.Windows.Forms.Padding(3);
		this.plAlarmClasses.Size = new System.Drawing.Size(949, 542);
		this.plAlarmClasses.TabIndex = 2;
		this.plAlarmClasses.Text = "Alarm classes";
		this.plAlarmClasses.UseVisualStyleBackColor = true;
		dataGridViewCellStyle11.BackColor = System.Drawing.Color.DarkGray;
		this.dgvAlarmClasses.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle11;
		this.dgvAlarmClasses.BackgroundColor = System.Drawing.Color.LightGray;
		this.dgvAlarmClasses.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle12.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle12.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle12.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle12.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle12.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle12.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle12.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvAlarmClasses.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle12;
		this.dgvAlarmClasses.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvAlarmClasses.Columns.AddRange(this.DisplayName, this.AlarmClassesId, this.AlarmClassName, this.StatusMachine, this.EmailAddress, this.BackgroundIncoming, this.TextColorIncoming, this.AcknowledgedColorIncoming, this.BackgroundOutcoming, this.TextColorOutComing, this.AcknowledgedColorOutcoming);
		dataGridViewCellStyle13.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle13.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle13.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle13.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle13.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle13.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle13.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dgvAlarmClasses.DefaultCellStyle = dataGridViewCellStyle13;
		this.dgvAlarmClasses.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dgvAlarmClasses.Location = new System.Drawing.Point(3, 3);
		this.dgvAlarmClasses.MultiSelect = false;
		this.dgvAlarmClasses.Name = "dgvAlarmClasses";
		dataGridViewCellStyle14.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle14.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle14.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle14.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle14.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle14.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvAlarmClasses.RowHeadersDefaultCellStyle = dataGridViewCellStyle14;
		this.dgvAlarmClasses.RowHeadersWidth = 53;
		dataGridViewCellStyle15.BackColor = System.Drawing.Color.LightGray;
		this.dgvAlarmClasses.RowsDefaultCellStyle = dataGridViewCellStyle15;
		this.dgvAlarmClasses.RowTemplate.Height = 25;
		this.dgvAlarmClasses.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dgvAlarmClasses.Size = new System.Drawing.Size(943, 536);
		this.dgvAlarmClasses.TabIndex = 6;
		this.dgvAlarmClasses.VirtualMode = true;
		this.DisplayName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.DisplayName.DataPropertyName = "DisplayName";
		this.DisplayName.HeaderText = "Display name";
		this.DisplayName.MinimumWidth = 110;
		this.DisplayName.Name = "DisplayName";
		this.DisplayName.Width = 110;
		this.AlarmClassesId.DataPropertyName = "Id";
		this.AlarmClassesId.HeaderText = "Id";
		this.AlarmClassesId.Name = "AlarmClassesId";
		this.AlarmClassesId.Visible = false;
		this.AlarmClassName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.AlarmClassName.DataPropertyName = "Name";
		this.AlarmClassName.HeaderText = "Name";
		this.AlarmClassName.MinimumWidth = 100;
		this.AlarmClassName.Name = "AlarmClassName";
		this.StatusMachine.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.StatusMachine.DataPropertyName = "StatusMachine";
		this.StatusMachine.HeaderText = "Status machine";
		this.StatusMachine.MinimumWidth = 125;
		this.StatusMachine.Name = "StatusMachine";
		this.StatusMachine.Width = 125;
		this.EmailAddress.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.EmailAddress.DataPropertyName = "EmailAddress";
		this.EmailAddress.HeaderText = "Email address";
		this.EmailAddress.MinimumWidth = 125;
		this.EmailAddress.Name = "EmailAddress";
		this.EmailAddress.Width = 125;
		this.BackgroundIncoming.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.BackgroundIncoming.DataPropertyName = "BackgroundIncoming";
		dataGridViewCellStyle16.ForeColor = System.Drawing.Color.Transparent;
		this.BackgroundIncoming.DefaultCellStyle = dataGridViewCellStyle16;
		this.BackgroundIncoming.HeaderText = "Background incoming";
		this.BackgroundIncoming.MinimumWidth = 165;
		this.BackgroundIncoming.Name = "BackgroundIncoming";
		this.BackgroundIncoming.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.BackgroundIncoming.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.BackgroundIncoming.Width = 165;
		this.TextColorIncoming.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.TextColorIncoming.DataPropertyName = "TextColorIncoming";
		dataGridViewCellStyle17.ForeColor = System.Drawing.Color.Transparent;
		this.TextColorIncoming.DefaultCellStyle = dataGridViewCellStyle17;
		this.TextColorIncoming.HeaderText = "Text incoming";
		this.TextColorIncoming.MinimumWidth = 165;
		this.TextColorIncoming.Name = "TextColorIncoming";
		this.TextColorIncoming.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.TextColorIncoming.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.TextColorIncoming.Width = 165;
		this.AcknowledgedColorIncoming.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.AcknowledgedColorIncoming.DataPropertyName = "AcknowledgedColorIncoming";
		dataGridViewCellStyle18.ForeColor = System.Drawing.Color.Transparent;
		this.AcknowledgedColorIncoming.DefaultCellStyle = dataGridViewCellStyle18;
		this.AcknowledgedColorIncoming.HeaderText = "Acknowledged incoming";
		this.AcknowledgedColorIncoming.MinimumWidth = 200;
		this.AcknowledgedColorIncoming.Name = "AcknowledgedColorIncoming";
		this.AcknowledgedColorIncoming.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.AcknowledgedColorIncoming.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.AcknowledgedColorIncoming.Width = 200;
		this.BackgroundOutcoming.DataPropertyName = "BackgroundOutcoming";
		dataGridViewCellStyle19.ForeColor = System.Drawing.Color.Transparent;
		this.BackgroundOutcoming.DefaultCellStyle = dataGridViewCellStyle19;
		this.BackgroundOutcoming.HeaderText = "Background outcoming";
		this.BackgroundOutcoming.MinimumWidth = 165;
		this.BackgroundOutcoming.Name = "BackgroundOutcoming";
		this.BackgroundOutcoming.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.BackgroundOutcoming.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.BackgroundOutcoming.Width = 165;
		this.TextColorOutComing.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.TextColorOutComing.DataPropertyName = "TextColorOutComing";
		dataGridViewCellStyle20.ForeColor = System.Drawing.Color.Transparent;
		this.TextColorOutComing.DefaultCellStyle = dataGridViewCellStyle20;
		this.TextColorOutComing.HeaderText = "Text outcoming";
		this.TextColorOutComing.MinimumWidth = 170;
		this.TextColorOutComing.Name = "TextColorOutComing";
		this.TextColorOutComing.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.TextColorOutComing.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.TextColorOutComing.Width = 170;
		this.AcknowledgedColorOutcoming.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.AcknowledgedColorOutcoming.DataPropertyName = "AcknowledgedColorOutcoming";
		dataGridViewCellStyle21.ForeColor = System.Drawing.Color.Transparent;
		this.AcknowledgedColorOutcoming.DefaultCellStyle = dataGridViewCellStyle21;
		this.AcknowledgedColorOutcoming.HeaderText = "Acknowledged outcoming";
		this.AcknowledgedColorOutcoming.MinimumWidth = 210;
		this.AcknowledgedColorOutcoming.Name = "AcknowledgedColorOutcoming";
		this.AcknowledgedColorOutcoming.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.AcknowledgedColorOutcoming.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.txtAnalogId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtAnalogId.DataPropertyName = "Id";
		this.txtAnalogId.HeaderText = "Id";
		this.txtAnalogId.MinimumWidth = 60;
		this.txtAnalogId.Name = "txtAnalogId";
		this.txtAnalogId.Width = 60;
		this.txtAnalogAlarmName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtAnalogAlarmName.DataPropertyName = "AlarmName";
		this.txtAnalogAlarmName.HeaderText = "Name";
		this.txtAnalogAlarmName.MinimumWidth = 150;
		this.txtAnalogAlarmName.Name = "txtAnalogAlarmName";
		this.txtAnalogAlarmName.Width = 150;
		this.txtAnalogAlarmText.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.txtAnalogAlarmText.DataPropertyName = "AlarmText";
		this.txtAnalogAlarmText.HeaderText = "Alarm text";
		this.txtAnalogAlarmText.Name = "txtAnalogAlarmText";
		this.cboxAnalogAlarmClass.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.cboxAnalogAlarmClass.DataPropertyName = "AlarmClassesId";
		this.cboxAnalogAlarmClass.DisplayStyleForCurrentCellOnly = true;
		this.cboxAnalogAlarmClass.HeaderText = "Alarm classes";
		this.cboxAnalogAlarmClass.MinimumWidth = 125;
		this.cboxAnalogAlarmClass.Name = "cboxAnalogAlarmClass";
		this.cboxAnalogAlarmClass.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.cboxAnalogAlarmClass.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.cboxAnalogAlarmClass.Width = 125;
		this.txtAnalogTriggerTag.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtAnalogTriggerTag.DataPropertyName = "TagName";
		this.txtAnalogTriggerTag.HeaderText = "Trigger tag";
		this.txtAnalogTriggerTag.MinimumWidth = 250;
		this.txtAnalogTriggerTag.Name = "txtAnalogTriggerTag";
		this.txtAnalogTriggerTag.Width = 250;
		this.txtAnalogLimit.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.txtAnalogLimit.DataPropertyName = "LimitValue";
		dataGridViewCellStyle22.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleRight;
		this.txtAnalogLimit.DefaultCellStyle = dataGridViewCellStyle22;
		this.txtAnalogLimit.HeaderText = "Limit";
		this.txtAnalogLimit.MinimumWidth = 80;
		this.txtAnalogLimit.Name = "txtAnalogLimit";
		this.txtAnalogLimit.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.txtAnalogLimit.Width = 80;
		this.cboxAnalogLimitMode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.cboxAnalogLimitMode.DataPropertyName = "LimitMode";
		this.cboxAnalogLimitMode.DisplayStyleForCurrentCellOnly = true;
		this.cboxAnalogLimitMode.HeaderText = "Limit mode";
		this.cboxAnalogLimitMode.MinimumWidth = 100;
		this.cboxAnalogLimitMode.Name = "cboxAnalogLimitMode";
		this.cboxAnalogLimitMode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.cboxAnalogLimitMode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.DTime.DataPropertyName = "DTime";
		this.DTime.HeaderText = "DTime";
		this.DTime.Name = "DTime";
		this.DTime.Visible = false;
		this.AlarmStatus.DataPropertyName = "AlarmStatus";
		this.AlarmStatus.HeaderText = "AlarmStatus";
		this.AlarmStatus.Name = "AlarmStatus";
		this.AlarmStatus.Visible = false;
		this.Logging.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.ColumnHeader;
		this.Logging.DataPropertyName = "Logging";
		this.Logging.HeaderText = "Logging";
		this.Logging.Name = "Logging";
		this.Logging.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.Logging.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.Logging.Width = 76;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(957, 595);
		base.Controls.Add(this.tcAlarm);
		base.Controls.Add(this.toolStrip1);
		base.Name = "FormAlarms";
		this.Text = "Alarms";
		base.Load += new System.EventHandler(FormAlarms_Load);
		this.toolStrip1.ResumeLayout(false);
		this.toolStrip1.PerformLayout();
		this.tcAlarm.ResumeLayout(false);
		this.plAnalogAlarms.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dgvAnalogAlarm).EndInit();
		this.plDiscreteAlarms.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dgvDiscreteAlarm).EndInit();
		this.plAlarmClasses.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dgvAlarmClasses).EndInit();
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

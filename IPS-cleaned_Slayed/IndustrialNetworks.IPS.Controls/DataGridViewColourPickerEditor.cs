using System;
using System.Windows.Forms;

namespace NetStudio.IPS.Controls;

public class DataGridViewColourPickerEditor : ColorPicker, IDataGridViewEditingControl
{
	private DataGridView _editingControlDataGridView;

	private object _editingControlFormattedValue;

	private int _editingControlRowIndex;

	private bool _editingControlValueChanged;

	public DataGridView EditingControlDataGridView
	{
		get
		{
			return _editingControlDataGridView;
		}
		set
		{
			_editingControlDataGridView = value;
		}
	}

	public object EditingControlFormattedValue
	{
		get
		{
			return _editingControlFormattedValue;
		}
		set
		{
			_editingControlFormattedValue = value;
		}
	}

	public int EditingControlRowIndex
	{
		get
		{
			return _editingControlRowIndex;
		}
		set
		{
			_editingControlRowIndex = value;
		}
	}

	public bool EditingControlValueChanged
	{
		get
		{
			return _editingControlValueChanged;
		}
		set
		{
			_editingControlValueChanged = value;
		}
	}

	public Cursor EditingPanelCursor => Cursors.Default;

	public bool RepositionEditingControlOnValueChange => true;

	public void ApplyCellStyleToEditingControl(DataGridViewCellStyle dataGridViewCellStyle)
	{
		base.SelectedIndexChanged += DataGridViewFADateTimePickerEditor_SelectedIndexChanged;
	}

	private void DataGridViewFADateTimePickerEditor_SelectedIndexChanged(object sender, EventArgs e)
	{
		EditingControlValueChanged = true;
		EditingControlFormattedValue = (byte)8;
		if (EditingControlValueChanged)
		{
			EditingControlDataGridView.NotifyCurrentCellDirty(dirty: true);
		}
	}

	public bool EditingControlWantsInputKey(Keys keyData, bool dataGridViewWantsInputKey)
	{
		return true;
	}

	public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts context)
	{
		if (base.SelectedItem == null)
		{
			return 0;
		}
		return ((byte)((MyColour)base.SelectedItem).Colour.ToKnownColor()).ToString();
	}

	public void PrepareEditingControlForEdit(bool selectAll)
	{
	}
}

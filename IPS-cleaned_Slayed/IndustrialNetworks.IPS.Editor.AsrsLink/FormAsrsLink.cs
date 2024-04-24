using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using NetStudio.Common;
using NetStudio.Common.AsrsLink;
using NetStudio.Common.Manager;
using NetStudio.DriverComm;
using NetStudio.DriverComm.Models;
using NetStudio.IPS.Controls;
using NetStudio.IPS.Local;
using NetStudio.IPS.Properties;

namespace NetStudio.IPS.Editor.AsrsLink;

public class FormAsrsLink : Form
{
	private AsrsServer? _Server;

	private BindingSource _bsAsrsLink;

	private SqlRequestInfo? _RequestInfo;

	private const char ESC = '\u001b';

	private TreeNode? serverNode;

	private TreeNode? databaseNode;

	private TreeNode? tableNode;

	private TreeNode? selectedNode;

	private int selectedNodeLevel;

	private AsrsTable? _Table;

	private bool keyESC;

	private IContainer components;

	private ToolStrip toolBar;

	private ToolStripLabel lblInfo;

	private ToolStripLabel lblSearchBox;

	private ToolStripTextBox txtSearchBox;

	private ToolStripButton btnClearSearchBox;

	private ToolStripSeparator toolStripSeparator4;

	private ToolStripLabel lblTotalName;

	private ToolStripLabel lblTotalTags;

	private ToolStripLabel toolStripLabel1;

	private ToolStripSeparator GroupLineHorizontal;

	private ToolStripLabel lblGroupName;

	private ToolStripLabel lblGroupTags;

	private ToolStripLabel lblGroupUnit;

	private ToolStripSeparator GroupLineVertical;

	private SplitContainer splitContainer1;

	private DataGrid dgvAsrsLink;

	private TreeView treeViewMain;

	private ImageList imageCollection;

	private ContextMenuStrip cmRight;

	private ContextMenuStrip cmLeft;

	private ToolStripMenuItem mnAsrsServer;

	private ToolStripMenuItem mnAsrsTable;

	private ToolStripSeparator toolStripSeparator1;

	private ToolStripMenuItem mnDelete;

	private ToolStripMenuItem mnDeleteAsrsRow;

	private DataGridViewComboBoxColumn cboxColumnName;

	private DataGridViewComboBoxColumn cboxColumnId;

	private DataGridViewTextBoxColumn columnValueOfId;

	private DataGridViewComboBoxColumn cboxOperatingMode;

	private DataGridViewComboBoxColumn cboxTrigger;

	private DataGridViewTextBoxColumn TagName;

	private DataGridViewTextBoxColumn txtId;

	public FormAsrsLink()
	{
		InitializeComponent();
		_bsAsrsLink = new BindingSource();
		dgvAsrsLink.DataSource = _bsAsrsLink;
	}

	private async Task<ApiResponse> OnInitializeAsrsLink()
	{
		ApiResponse apiResponse = null;
		if (EditHelper.IndusProtocol == null)
		{
			apiResponse = await EditHelper.OnLoadProject();
		}
		if (EditHelper.IndusProtocol != null && (apiResponse == null || (apiResponse != null && apiResponse.Success)))
		{
			EditHelper.IndusProtocol.AsrsServer = EditHelper.IndusProtocol.AsrsServer ?? new AsrsServer();
			_Server = EditHelper.IndusProtocol.AsrsServer;
			_RequestInfo = new SqlRequestInfo
			{
				ServerName = EditHelper.IndusProtocol.AsrsServer.ServerName,
				DatabaseName = EditHelper.IndusProtocol.AsrsServer.DatabaseName,
				Login = EditHelper.IndusProtocol.AsrsServer.Login,
				Password = EditHelper.IndusProtocol.AsrsServer.Password
			};
			if (EditHelper.IndusProtocol.AsrsServer.Tables.Count > 0)
			{
				_Table = _Server.Tables[0];
				lblInfo.Text = $"{_Server.ServerName}/{_Server.DatabaseName}/{_Table.Name}";
				_RequestInfo.TableName = _Table.Name;
				dgvAsrsLink.ReadOnly = false;
				dgvAsrsLink.AllowUserToAddRows = !dgvAsrsLink.ReadOnly;
				dgvAsrsLink.AllowUserToDeleteRows = !dgvAsrsLink.ReadOnly;
				if (GetColumnsInfo())
				{
					_bsAsrsLink.DataSource = _Table.Rows;
				}
			}
			else
			{
				lblInfo.Text = _Server.ServerName + "/" + _Server.DatabaseName;
			}
		}
		return apiResponse ?? new ApiResponse
		{
			Message = "Read request successfully.",
			Success = true
		};
	}

	private async void FormAsrsLink_Load(object sender, EventArgs e)
	{
		try
		{
			await WaitFormManager.ShowAsync(this, "Loading...");
			dgvAsrsLink.DefaultValuesNeeded += OnDgvAsrsLinkDefaultValuesNeeded;
			dgvAsrsLink.CellMouseDoubleClick += dgvAsrsLink_CellMouseDoubleClick;
			dgvAsrsLink.RowValidating += ValidateByRow;
			dgvAsrsLink.UserDeletingRow += OnDgvAsrsLinkUserDeletingRow;
			dgvAsrsLink.CellValueChanged += OnDgvAsrsLink_CellValueChanged;
			cboxOperatingMode.DataSource = Extensions.GetDictionary<OperatingMode>().ToList();
			cboxOperatingMode.DisplayMember = "Value";
			cboxOperatingMode.ValueMember = "Key";
			cboxTrigger.DataSource = Extensions.GetDictionary<AsrsTrigger>().ToList();
			cboxTrigger.DisplayMember = "Value";
			cboxTrigger.ValueMember = "Key";
			ApiResponse apiResponse = await OnInitializeAsrsLink();
			if (apiResponse.Success)
			{
				LoadTreeList();
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
		finally
		{
			treeViewMain.ExpandAll();
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
		if (EditHelper.IndusProtocol != null && EditHelper.IndusProtocol.AsrsServer != null && _Table != null)
		{
			_bsAsrsLink.DataSource = _Table.Rows.FindAll((AsrsRow asrsRow_0) => asrsRow_0.ColumnName.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || asrsRow_0.ColumnId.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || asrsRow_0.ValueOfId.ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || asrsRow_0.Mode.GetDescription().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || asrsRow_0.Trigger.GetDescription().ToString().Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase) || asrsRow_0.TagName.Contains(txtSearchBox.Text, StringComparison.OrdinalIgnoreCase));
		}
	}

	private void treeViewMain_AfterSelect(object sender, TreeViewEventArgs e)
	{
		try
		{
			serverNode = null;
			tableNode = null;
			TreeView treeView = (TreeView)sender;
			selectedNode = treeView.SelectedNode;
			selectedNodeLevel = treeView.SelectedNode.Level;
			switch (selectedNodeLevel)
			{
			default:
				serverNode = selectedNode;
				mnAsrsServer.Text = "Edit: AS/RS Server";
				mnAsrsServer.Visible = true;
				mnAsrsTable.Visible = false;
				mnDelete.Visible = false;
				dgvAsrsLink.ReadOnly = true;
				if (serverNode != null)
				{
					lblInfo.Text = serverNode.Text ?? "";
				}
				_bsAsrsLink.Clear();
				break;
			case 1:
				databaseNode = selectedNode;
				serverNode = databaseNode.Parent;
				mnAsrsServer.Text = "Edit: AS/RS Server";
				mnAsrsTable.Text = "Add: AS/RS Table";
				mnAsrsServer.Visible = true;
				mnAsrsTable.Visible = true;
				mnDelete.Visible = true;
				dgvAsrsLink.ReadOnly = true;
				if (databaseNode != null)
				{
					lblInfo.Text = serverNode.Text + "/" + databaseNode.Text;
				}
				_bsAsrsLink.Clear();
				break;
			case 2:
				tableNode = selectedNode;
				databaseNode = tableNode.Parent;
				serverNode = databaseNode.Parent;
				mnAsrsTable.Text = "Edit: AS/RS Table";
				mnAsrsServer.Visible = false;
				mnAsrsTable.Visible = true;
				mnDelete.Visible = true;
				dgvAsrsLink.ReadOnly = false;
				if (tableNode == null || _RequestInfo == null)
				{
					break;
				}
				lblInfo.Text = $"{serverNode.Text}/{databaseNode.Text}/{tableNode.Text}";
				_RequestInfo.TableName = tableNode.Text;
				_bsAsrsLink.DataSource = new List<AsrsRow>();
				if (GetColumnsInfo())
				{
					int int_ = int.Parse(tableNode.Tag.ToString());
					_Table = EditHelper.GetAsrsTable(int_);
					if (_Table != null)
					{
						_bsAsrsLink.DataSource = _Table.Rows;
					}
				}
				break;
			}
			dgvAsrsLink.AllowUserToAddRows = !dgvAsrsLink.ReadOnly;
			dgvAsrsLink.AllowUserToDeleteRows = !dgvAsrsLink.ReadOnly;
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private bool GetColumnsInfo()
	{
		if (_RequestInfo != null)
		{
			ApiResponse columnsInfo = ClientHelper.Editor.GetColumnsInfo(_RequestInfo);
			if (columnsInfo.Success)
			{
				cboxColumnId.DataSource = columnsInfo.Data;
				cboxColumnId.DisplayMember = "ColumnName";
				cboxColumnId.ValueMember = "ColumnName";
				cboxColumnName.DataSource = columnsInfo.Data;
				cboxColumnName.DisplayMember = "ColumnName";
				cboxColumnName.ValueMember = "ColumnName";
			}
			return columnsInfo.Success;
		}
		return false;
	}

	private async Task<bool> LoadAsrsInfoAsync()
	{
		if (_RequestInfo == null)
		{
			return false;
		}
		ApiResponse apiResponse = await ClientHelper.Editor.GetColumnsInfoAsync(_RequestInfo);
		if (apiResponse.Success)
		{
			cboxColumnId.DataSource = apiResponse.Data;
			cboxColumnId.DisplayMember = "ColumnName";
			cboxColumnId.ValueMember = "ColumnName";
			cboxColumnName.DataSource = apiResponse.Data;
			cboxColumnName.DisplayMember = "ColumnName";
			cboxColumnName.ValueMember = "ColumnName";
		}
		return apiResponse.Success;
	}

	public void LoadTreeList()
	{
		try
		{
			int value = 0;
			serverNode = null;
			databaseNode = null;
			selectedNode = null;
			treeViewMain.Nodes.Clear();
			treeViewMain.ImageList = imageCollection;
			if (EditHelper.IndusProtocol == null || EditHelper.IndusProtocol.AsrsServer == null)
			{
				return;
			}
			AsrsServer asrsServer = EditHelper.IndusProtocol.AsrsServer;
			_RequestInfo = new SqlRequestInfo
			{
				ServerName = asrsServer.ServerName,
				DatabaseName = asrsServer.DatabaseName,
				Login = asrsServer.Login,
				Password = asrsServer.Password
			};
			serverNode = new TreeNode(asrsServer.ServerName);
			treeViewMain.Nodes.Add(serverNode);
			if (string.IsNullOrEmpty(asrsServer.DatabaseName))
			{
				return;
			}
			databaseNode = new TreeNode(asrsServer.DatabaseName)
			{
				ImageIndex = 1,
				SelectedImageIndex = 1
			};
			serverNode.Nodes.Add(databaseNode);
			List<AsrsTable> tables = asrsServer.Tables;
			if (tables == null || tables.Count <= 0)
			{
				return;
			}
			foreach (AsrsTable item in tables)
			{
				TreeNode node = new TreeNode(item.Name)
				{
					ImageIndex = 2,
					SelectedImageIndex = 2,
					Tag = item.Id
				};
				databaseNode.Nodes.Add(node);
			}
			if (_bsAsrsLink.DataSource == null)
			{
				_bsAsrsLink.DataSource = Array.Empty<Tag>();
			}
			lblTotalTags.Text = $"{value}";
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnAsrsServer_Click(object sender, EventArgs e)
	{
		try
		{
			if (EditHelper.IndusProtocol == null)
			{
				return;
			}
			if (EditHelper.IndusProtocol.AsrsServer != null)
			{
				_Server = (AsrsServer)EditHelper.IndusProtocol.AsrsServer.Clone();
			}
			_Server = _Server ?? new AsrsServer();
			FormAsrsServer formAsrsServer = new FormAsrsServer(_Server)
			{
				WindowState = FormWindowState.Normal,
				StartPosition = FormStartPosition.CenterParent
			};
			if (formAsrsServer.ShowDialog(base.Parent) != DialogResult.OK || formAsrsServer.Tag == null)
			{
				return;
			}
			AppHelper.DataChanged = true;
			AsrsServer asrsServer = (AsrsServer)formAsrsServer.Tag;
			if (EditHelper.IndusProtocol.AsrsServer == null)
			{
				return;
			}
			EditHelper.IndusProtocol.AsrsServer.ServerName = asrsServer.ServerName;
			EditHelper.IndusProtocol.AsrsServer.DatabaseName = asrsServer.DatabaseName;
			EditHelper.IndusProtocol.AsrsServer.Login = asrsServer.Login;
			EditHelper.IndusProtocol.AsrsServer.Password = asrsServer.Password;
			_RequestInfo = new SqlRequestInfo
			{
				ServerName = asrsServer.ServerName,
				DatabaseName = asrsServer.DatabaseName,
				Login = asrsServer.Login,
				Password = asrsServer.Password
			};
			if (serverNode != null)
			{
				serverNode.Text = asrsServer.ServerName;
				if (databaseNode == null)
				{
					TreeNode node = new TreeNode(asrsServer.DatabaseName)
					{
						ImageIndex = 1,
						SelectedImageIndex = 1,
						Tag = asrsServer.Id
					};
					serverNode.Nodes.Add(node);
					serverNode.Expand();
				}
				else
				{
					databaseNode.Text = asrsServer.DatabaseName;
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void mnAsrsTable_Click(object sender, EventArgs e)
	{
		try
		{
			if (EditHelper.IndusProtocol == null)
			{
				return;
			}
			AsrsTable asrsTable = null;
			int num = selectedNodeLevel;
			if (num != 1 && num == 2)
			{
				if (tableNode != null)
				{
					if (tableNode.Tag == null)
					{
						tableNode.Tag = 0;
					}
					AsrsTable asrsTable2 = EditHelper.GetAsrsTable(int.Parse(tableNode.Tag.ToString()));
					if (asrsTable2 != null)
					{
						asrsTable = (AsrsTable)asrsTable2.Clone();
					}
				}
			}
			else
			{
				asrsTable = new AsrsTable();
			}
			asrsTable = asrsTable ?? new AsrsTable();
			FormAsrsTable formAsrsTable = new FormAsrsTable(asrsTable)
			{
				WindowState = FormWindowState.Normal,
				StartPosition = FormStartPosition.CenterParent
			};
			if (formAsrsTable.ShowDialog() != DialogResult.OK)
			{
				return;
			}
			AsrsTable asrsTable3 = (AsrsTable)formAsrsTable.Tag;
			if (asrsTable3 == null)
			{
				return;
			}
			num = selectedNodeLevel;
			if (num != 1 && num == 2)
			{
				if (tableNode != null)
				{
					tableNode.Text = asrsTable3.Name;
				}
				EditHelper.EditAsrsTable(asrsTable3);
			}
			else
			{
				EditHelper.AddAsrsTable(asrsTable3);
				if (databaseNode != null)
				{
					databaseNode.Nodes.Add(new TreeNode(asrsTable3.Name)
					{
						ImageIndex = 2,
						SelectedImageIndex = 2,
						Tag = asrsTable3.Id
					});
					databaseNode.Expand();
				}
			}
			_Table = asrsTable;
			AppHelper.DataChanged = true;
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
			if (selectedNode == null)
			{
				return;
			}
			switch (selectedNodeLevel)
			{
			default:
				throw new InvalidOperationException();
			case 1:
				if (databaseNode != null && DialogResult.Yes == MessageBox.Show(this, "Do you want to delete all tables?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					EditHelper.RemoveAsrsTableAll();
					selectedNode.Nodes.Clear();
					AppHelper.DataChanged = true;
				}
				break;
			case 2:
				if (serverNode != null && databaseNode != null && tableNode != null && tableNode.Tag != null && DialogResult.Yes == MessageBox.Show(this, "Do you want to delete the table(" + tableNode.Text + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
				{
					EditHelper.RemoveAsrsTable(int.Parse(tableNode.Tag.ToString()));
					databaseNode.Nodes.Remove(tableNode);
					AppHelper.DataChanged = true;
				}
				break;
			case 0:
				break;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvAsrsLink_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
	{
		AppHelper.DataChanged = true;
	}

	private void OnDgvAsrsLinkDefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
	{
		if (e.Row.IsNewRow)
		{
			if (e.Row.Cells["txtId"].Value == null || string.Format("{0}", e.Row.Cells["txtId"].Value) == "0")
			{
				e.Row.Cells["txtId"].Value = e.Row.Index + 1;
			}
			e.Row.Cells["TagName"].Value = "<Double click>";
			if (dgvAsrsLink.Rows.Count > 1)
			{
				DataGridViewRow dataGridViewRow = dgvAsrsLink.Rows[e.Row.Index - 1];
				e.Row.Cells["cboxColumnId"].Value = dataGridViewRow.Cells["cboxColumnId"].Value;
				e.Row.Cells["columnValueOfId"].Value = dataGridViewRow.Cells["columnValueOfId"].Value;
				e.Row.Cells["cboxOperatingMode"].Value = dataGridViewRow.Cells["cboxOperatingMode"].Value;
				e.Row.Cells["cboxTrigger"].Value = dataGridViewRow.Cells["cboxTrigger"].Value;
			}
		}
	}

	private void dgvAsrsLink_CellMouseDoubleClick(object? sender, DataGridViewCellMouseEventArgs e)
	{
		try
		{
			DataGridView gridView = (DataGridView)sender;
			DataGridViewRow dataGridViewRow_0 = gridView.Rows[e.RowIndex];
			AsrsRow asrsRowTag = (AsrsRow)_bsAsrsLink.Current;
			if (e.RowIndex < 0 || asrsRowTag == null || e.ColumnIndex != 5)
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
				if (asrsRowTag != null)
				{
					AppHelper.DataChanged = true;
					asrsRowTag.TagName = tg.FullName;
					asrsRowTag.ChannelId = tg.ChannelId;
					asrsRowTag.DeviceId = tg.DeviceId;
					asrsRowTag.GroupId = tg.GroupId;
					asrsRowTag.TagId = tg.Id;
					asrsRowTag.TagName = tg.FullName;
					dataGridViewRow_0.Cells[gridView.Columns["TagName"].Index].ErrorText = string.Empty;
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

	private void mnDeleteAsrsRow_Click(object sender, EventArgs e)
	{
		try
		{
			AsrsRow asrsRow = (AsrsRow)_bsAsrsLink.Current;
			if (asrsRow != null && MessageBox.Show(this, "Do you want to remove the tag(" + asrsRow.TagName + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
			{
				_bsAsrsLink.RemoveCurrent();
				AppHelper.DataChanged = true;
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void OnDgvAsrsLinkUserDeletingRow(object? sender, DataGridViewRowCancelEventArgs e)
	{
		try
		{
			AsrsRow asrsRow = (AsrsRow)_bsAsrsLink.Current;
			if (MessageBox.Show(this, "Do you want to remove the tag(" + asrsRow.TagName + ")?", "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
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

	private void OnDgvAsrsLinkKeyPress(object? sender, KeyPressEventArgs e)
	{
		try
		{
			if (keyESC = e.KeyChar == '\u001b' && _bsAsrsLink.Count < 2)
			{
				_bsAsrsLink.ResetBindings(metadataChanged: true);
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void ValidateByRow(object? sender, DataGridViewCellCancelEventArgs e)
	{
		e.Cancel = false;
		if (e.RowIndex != dgvAsrsLink.NewRowIndex)
		{
			AsrsRow asrsRow = (AsrsRow)_bsAsrsLink[e.RowIndex];
			DataGridViewRow dataGridViewRow = dgvAsrsLink.Rows[e.RowIndex];
			DataGridViewCell dataGridViewCell = dataGridViewRow.Cells[dgvAsrsLink.Columns["cboxColumnId"].Index];
			dataGridViewCell.ErrorText = string.Empty;
			dgvAsrsLink.Rows[dataGridViewCell.RowIndex].ErrorText = string.Empty;
			DataGridViewCell dataGridViewCell2 = dataGridViewRow.Cells[dgvAsrsLink.Columns["cboxColumnName"].Index];
			dataGridViewCell2.ErrorText = string.Empty;
			dgvAsrsLink.Rows[dataGridViewCell2.RowIndex].ErrorText = string.Empty;
			DataGridViewCell dataGridViewCell3 = dataGridViewRow.Cells[dgvAsrsLink.Columns["TagName"].Index];
			dataGridViewCell3.ErrorText = string.Empty;
			dgvAsrsLink.Rows[dataGridViewCell3.RowIndex].ErrorText = string.Empty;
			e.Cancel = !IsTagName(asrsRow, dataGridViewCell3) || !IsColumnName(asrsRow, dataGridViewCell2) || !IsColumnId(asrsRow, dataGridViewCell);
		}
	}

	private bool IsColumnId(AsrsRow asrsRow, DataGridViewCell cell)
	{
		if (cell.Value != null && (cell.Value == null || (!string.IsNullOrEmpty(cell.Value.ToString()) && !"<Double click>".Equals(cell.Value.ToString()))))
		{
			return true;
		}
		cell.ErrorText = "Please select a column id.";
		dgvAsrsLink.Rows[cell.RowIndex].ErrorText = "Please select a column id.";
		return false;
	}

	private bool IsColumnName(AsrsRow asrsRow, DataGridViewCell cell)
	{
		
		 
		if (cell.Value != null && (cell.Value == null || (!string.IsNullOrEmpty(cell.Value.ToString()) && !"<Double click>".Equals(cell.Value.ToString()))))
		{
			List<AsrsRow> list = (List<AsrsRow>)_bsAsrsLink.DataSource;
			if (list != null && list.Count > 0 && list.Count((AsrsRow asrsRow_0) => asrsRow_0.ColumnName == $"{cell.Value}" && asrsRow_0.Mode == asrsRow.Mode) > 1)
			{
				cell.ErrorText = "The column name already exists.";
				dgvAsrsLink.Rows[cell.RowIndex].ErrorText = "The column name already exists.";
				return false;
			}
			return true;
		}
		cell.ErrorText = "Please select a column name.";
		dgvAsrsLink.Rows[cell.RowIndex].ErrorText = "Please select a column name.";
		return false;
	}

	private bool IsTagName(AsrsRow asrsRow, DataGridViewCell cell)
	{
		
		 
		if (cell.Value != null && (cell.Value == null || (!string.IsNullOrEmpty(cell.Value.ToString()) && !"<Double click>".Equals(cell.Value.ToString()))))
		{
			List<AsrsRow> list = (List<AsrsRow>)_bsAsrsLink.DataSource;
			if (list != null && list.Count > 0 && list.Count((AsrsRow asrsRow_0) => asrsRow_0.TagName == $"{cell.Value}" && asrsRow_0.Mode == asrsRow.Mode) > 1)
			{
				cell.ErrorText = "The tag name already exists.";
				dgvAsrsLink.Rows[cell.RowIndex].ErrorText = "The tag name already exists.";
				return false;
			}
			return true;
		}
		cell.ErrorText = "Please enter a tag name.";
		dgvAsrsLink.Rows[cell.RowIndex].ErrorText = "Please enter a tag name.";
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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NetStudio.IPS.Editor.AsrsLink.FormAsrsLink));
		this.toolBar = new System.Windows.Forms.ToolStrip();
		this.lblInfo = new System.Windows.Forms.ToolStripLabel();
		this.lblSearchBox = new System.Windows.Forms.ToolStripLabel();
		this.txtSearchBox = new System.Windows.Forms.ToolStripTextBox();
		this.btnClearSearchBox = new System.Windows.Forms.ToolStripButton();
		this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
		this.lblTotalName = new System.Windows.Forms.ToolStripLabel();
		this.lblTotalTags = new System.Windows.Forms.ToolStripLabel();
		this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
		this.GroupLineHorizontal = new System.Windows.Forms.ToolStripSeparator();
		this.lblGroupName = new System.Windows.Forms.ToolStripLabel();
		this.lblGroupTags = new System.Windows.Forms.ToolStripLabel();
		this.lblGroupUnit = new System.Windows.Forms.ToolStripLabel();
		this.GroupLineVertical = new System.Windows.Forms.ToolStripSeparator();
		this.splitContainer1 = new System.Windows.Forms.SplitContainer();
		this.dgvAsrsLink = new NetStudio.IPS.Controls.DataGrid();
		this.cmLeft = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnDeleteAsrsRow = new System.Windows.Forms.ToolStripMenuItem();
		this.treeViewMain = new System.Windows.Forms.TreeView();
		this.cmRight = new System.Windows.Forms.ContextMenuStrip(this.components);
		this.mnAsrsServer = new System.Windows.Forms.ToolStripMenuItem();
		this.mnAsrsTable = new System.Windows.Forms.ToolStripMenuItem();
		this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
		this.mnDelete = new System.Windows.Forms.ToolStripMenuItem();
		this.imageCollection = new System.Windows.Forms.ImageList(this.components);
		this.cboxColumnName = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.cboxColumnId = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.columnValueOfId = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.cboxOperatingMode = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.cboxTrigger = new System.Windows.Forms.DataGridViewComboBoxColumn();
		this.TagName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.txtId = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.toolBar.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).BeginInit();
		this.splitContainer1.Panel1.SuspendLayout();
		this.splitContainer1.Panel2.SuspendLayout();
		this.splitContainer1.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.dgvAsrsLink).BeginInit();
		this.cmLeft.SuspendLayout();
		this.cmRight.SuspendLayout();
		base.SuspendLayout();
		this.toolBar.BackColor = System.Drawing.Color.Snow;
		this.toolBar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
		this.toolBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[13]
		{
			this.lblInfo, this.lblSearchBox, this.txtSearchBox, this.btnClearSearchBox, this.toolStripSeparator4, this.lblTotalName, this.lblTotalTags, this.toolStripLabel1, this.GroupLineHorizontal, this.lblGroupName,
			this.lblGroupTags, this.lblGroupUnit, this.GroupLineVertical
		});
		this.toolBar.Location = new System.Drawing.Point(0, 0);
		this.toolBar.Name = "toolBar";
		this.toolBar.Size = new System.Drawing.Size(954, 25);
		this.toolBar.TabIndex = 3;
		this.toolBar.Text = "Tool bar";
		this.lblInfo.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
		this.lblInfo.Name = "lblInfo";
		this.lblInfo.Size = new System.Drawing.Size(70, 22);
		this.lblInfo.Text = "Information";
		this.lblSearchBox.Name = "lblSearchBox";
		this.lblSearchBox.Size = new System.Drawing.Size(45, 22);
		this.lblSearchBox.Text = "Search:";
		this.txtSearchBox.BackColor = System.Drawing.Color.White;
		this.txtSearchBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
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
		this.toolStripSeparator4.Name = "toolStripSeparator4";
		this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
		this.lblTotalName.Name = "lblTotalName";
		this.lblTotalName.Size = new System.Drawing.Size(35, 22);
		this.lblTotalName.Text = "Total:";
		this.lblTotalName.Visible = false;
		this.lblTotalTags.ForeColor = System.Drawing.Color.Blue;
		this.lblTotalTags.Name = "lblTotalTags";
		this.lblTotalTags.Size = new System.Drawing.Size(13, 22);
		this.lblTotalTags.Text = "0";
		this.lblTotalTags.Visible = false;
		this.toolStripLabel1.ForeColor = System.Drawing.Color.DarkSlateGray;
		this.toolStripLabel1.Name = "toolStripLabel1";
		this.toolStripLabel1.Size = new System.Drawing.Size(29, 22);
		this.toolStripLabel1.Text = "tags";
		this.toolStripLabel1.Visible = false;
		this.GroupLineHorizontal.Name = "GroupLineHorizontal";
		this.GroupLineHorizontal.Size = new System.Drawing.Size(6, 25);
		this.GroupLineHorizontal.Visible = false;
		this.lblGroupName.Name = "lblGroupName";
		this.lblGroupName.Size = new System.Drawing.Size(43, 22);
		this.lblGroupName.Text = "Group:";
		this.lblGroupName.Visible = false;
		this.lblGroupTags.ForeColor = System.Drawing.Color.Crimson;
		this.lblGroupTags.Name = "lblGroupTags";
		this.lblGroupTags.Size = new System.Drawing.Size(13, 22);
		this.lblGroupTags.Text = "0";
		this.lblGroupTags.Visible = false;
		this.lblGroupUnit.ForeColor = System.Drawing.Color.DarkSlateGray;
		this.lblGroupUnit.Name = "lblGroupUnit";
		this.lblGroupUnit.Size = new System.Drawing.Size(29, 22);
		this.lblGroupUnit.Text = "tags";
		this.lblGroupUnit.Visible = false;
		this.GroupLineVertical.Name = "GroupLineVertical";
		this.GroupLineVertical.Size = new System.Drawing.Size(6, 25);
		this.GroupLineVertical.Visible = false;
		this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.splitContainer1.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
		this.splitContainer1.IsSplitterFixed = true;
		this.splitContainer1.Location = new System.Drawing.Point(0, 25);
		this.splitContainer1.Name = "splitContainer1";
		this.splitContainer1.Panel1.Controls.Add(this.dgvAsrsLink);
		this.splitContainer1.Panel2.Controls.Add(this.treeViewMain);
		this.splitContainer1.Size = new System.Drawing.Size(954, 551);
		this.splitContainer1.SplitterDistance = 697;
		this.splitContainer1.TabIndex = 5;
		this.dgvAsrsLink.AllowUserToAddRows = false;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.DarkGray;
		this.dgvAsrsLink.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle;
		this.dgvAsrsLink.BackgroundColor = System.Drawing.Color.LightGray;
		this.dgvAsrsLink.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
		dataGridViewCellStyle2.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
		dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
		dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvAsrsLink.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
		this.dgvAsrsLink.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.dgvAsrsLink.Columns.AddRange(this.cboxColumnName, this.cboxColumnId, this.columnValueOfId, this.cboxOperatingMode, this.cboxTrigger, this.TagName, this.txtId);
		this.dgvAsrsLink.ContextMenuStrip = this.cmLeft;
		dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle3.BackColor = System.Drawing.SystemColors.Window;
		dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
		dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.dgvAsrsLink.DefaultCellStyle = dataGridViewCellStyle3;
		this.dgvAsrsLink.Dock = System.Windows.Forms.DockStyle.Fill;
		this.dgvAsrsLink.Location = new System.Drawing.Point(0, 0);
		this.dgvAsrsLink.MultiSelect = false;
		this.dgvAsrsLink.Name = "dgvAsrsLink";
		dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle4.BackColor = System.Drawing.Color.Black;
		dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
		dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.WindowText;
		dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
		this.dgvAsrsLink.RowHeadersDefaultCellStyle = dataGridViewCellStyle4;
		this.dgvAsrsLink.RowHeadersWidth = 53;
		dataGridViewCellStyle5.BackColor = System.Drawing.Color.LightGray;
		this.dgvAsrsLink.RowsDefaultCellStyle = dataGridViewCellStyle5;
		this.dgvAsrsLink.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.dgvAsrsLink.Size = new System.Drawing.Size(697, 551);
		this.dgvAsrsLink.TabIndex = 3;
		this.dgvAsrsLink.VirtualMode = true;
		this.cmLeft.Items.AddRange(new System.Windows.Forms.ToolStripItem[1] { this.mnDeleteAsrsRow });
		this.cmLeft.Name = "contextMenuStrip1";
		this.cmLeft.Size = new System.Drawing.Size(108, 26);
		this.mnDeleteAsrsRow.Image = NetStudio.IPS.Properties.Resources.Resources_512_Delete;
		this.mnDeleteAsrsRow.Name = "mnDeleteAsrsRow";
		this.mnDeleteAsrsRow.Size = new System.Drawing.Size(107, 22);
		this.mnDeleteAsrsRow.Text = "&Delete";
		this.mnDeleteAsrsRow.Click += new System.EventHandler(mnDeleteAsrsRow_Click);
		this.treeViewMain.ContextMenuStrip = this.cmRight;
		this.treeViewMain.Dock = System.Windows.Forms.DockStyle.Fill;
		this.treeViewMain.Location = new System.Drawing.Point(0, 0);
		this.treeViewMain.Name = "treeViewMain";
		this.treeViewMain.Size = new System.Drawing.Size(253, 551);
		this.treeViewMain.TabIndex = 0;
		this.treeViewMain.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(treeViewMain_AfterSelect);
		this.cmRight.Items.AddRange(new System.Windows.Forms.ToolStripItem[4] { this.mnAsrsServer, this.mnAsrsTable, this.toolStripSeparator1, this.mnDelete });
		this.cmRight.Name = "contextMenuStrip1";
		this.cmRight.Size = new System.Drawing.Size(142, 76);
		this.mnAsrsServer.Image = NetStudio.IPS.Properties.Resources.Resources_512_sql_server;
		this.mnAsrsServer.Name = "mnAsrsServer";
		this.mnAsrsServer.Size = new System.Drawing.Size(141, 22);
		this.mnAsrsServer.Text = "AS/RS Server";
		this.mnAsrsServer.Click += new System.EventHandler(mnAsrsServer_Click);
		this.mnAsrsTable.Image = NetStudio.IPS.Properties.Resources.Resources_512_group;
		this.mnAsrsTable.Name = "mnAsrsTable";
		this.mnAsrsTable.Size = new System.Drawing.Size(141, 22);
		this.mnAsrsTable.Text = "AS/RS Table";
		this.mnAsrsTable.Click += new System.EventHandler(mnAsrsTable_Click);
		this.toolStripSeparator1.Name = "toolStripSeparator1";
		this.toolStripSeparator1.Size = new System.Drawing.Size(138, 6);
		this.mnDelete.Image = NetStudio.IPS.Properties.Resources.Resources_512_Delete;
		this.mnDelete.Name = "mnDelete";
		this.mnDelete.Size = new System.Drawing.Size(141, 22);
		this.mnDelete.Text = "&Delete";
		this.mnDelete.Click += new System.EventHandler(mnDelete_Click);
		this.imageCollection.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
		this.imageCollection.ImageStream = (System.Windows.Forms.ImageListStreamer)resources.GetObject("imageCollection.ImageStream");
		this.imageCollection.TransparentColor = System.Drawing.Color.Transparent;
		this.imageCollection.Images.SetKeyName(0, "sql-server.png");
		this.imageCollection.Images.SetKeyName(1, "data_block.png");
		this.imageCollection.Images.SetKeyName(2, "group.png");
		this.cboxColumnName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.cboxColumnName.DataPropertyName = "ColumnName";
		this.cboxColumnName.DisplayStyleForCurrentCellOnly = true;
		this.cboxColumnName.HeaderText = "Column Name";
		this.cboxColumnName.MinimumWidth = 160;
		this.cboxColumnName.Name = "cboxColumnName";
		this.cboxColumnName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.cboxColumnName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.cboxColumnName.Width = 160;
		this.cboxColumnId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.cboxColumnId.DataPropertyName = "ColumnId";
		this.cboxColumnId.DisplayStyleForCurrentCellOnly = true;
		this.cboxColumnId.HeaderText = "Column Id";
		this.cboxColumnId.MinimumWidth = 160;
		this.cboxColumnId.Name = "cboxColumnId";
		this.cboxColumnId.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.cboxColumnId.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.cboxColumnId.Width = 160;
		this.columnValueOfId.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.columnValueOfId.DataPropertyName = "ValueOfId";
		this.columnValueOfId.HeaderText = "Value Of Column Id";
		this.columnValueOfId.MinimumWidth = 140;
		this.columnValueOfId.Name = "columnValueOfId";
		this.columnValueOfId.Width = 140;
		this.cboxOperatingMode.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.cboxOperatingMode.DataPropertyName = "Mode";
		this.cboxOperatingMode.DisplayStyleForCurrentCellOnly = true;
		this.cboxOperatingMode.HeaderText = "Operating Mode";
		this.cboxOperatingMode.MinimumWidth = 160;
		this.cboxOperatingMode.Name = "cboxOperatingMode";
		this.cboxOperatingMode.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.cboxOperatingMode.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.cboxOperatingMode.Width = 160;
		this.cboxTrigger.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells;
		this.cboxTrigger.DataPropertyName = "Trigger";
		this.cboxTrigger.DisplayStyleForCurrentCellOnly = true;
		this.cboxTrigger.HeaderText = "Trigger Type";
		this.cboxTrigger.MinimumWidth = 160;
		this.cboxTrigger.Name = "cboxTrigger";
		this.cboxTrigger.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.cboxTrigger.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
		this.cboxTrigger.Width = 160;
		this.TagName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.TagName.DataPropertyName = "TagName";
		this.TagName.HeaderText = "Tag Name";
		this.TagName.MinimumWidth = 200;
		this.TagName.Name = "TagName";
		this.TagName.Resizable = System.Windows.Forms.DataGridViewTriState.True;
		this.TagName.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
		this.txtId.DataPropertyName = "Id";
		this.txtId.HeaderText = "Id";
		this.txtId.Name = "txtId";
		this.txtId.Visible = false;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		base.ClientSize = new System.Drawing.Size(954, 576);
		base.Controls.Add(this.splitContainer1);
		base.Controls.Add(this.toolBar);
		base.Name = "FormAsrsLink";
		this.Text = "AS/RS Link";
		base.Load += new System.EventHandler(FormAsrsLink_Load);
		this.toolBar.ResumeLayout(false);
		this.toolBar.PerformLayout();
		this.splitContainer1.Panel1.ResumeLayout(false);
		this.splitContainer1.Panel2.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.splitContainer1).EndInit();
		this.splitContainer1.ResumeLayout(false);
		((System.ComponentModel.ISupportInitialize)this.dgvAsrsLink).EndInit();
		this.cmLeft.ResumeLayout(false);
		this.cmRight.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}

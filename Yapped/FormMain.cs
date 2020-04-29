using SoulsFormats;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections.Concurrent;
using CellType = SoulsFormats.PARAM.CellType;
using DefType = SoulsFormats.PARAMDEF.DefType;
using GameType = Yapped.GameMode.GameType;

namespace Yapped
{
    public partial class FormMain : Form
    {
        private static Properties.Settings settings = Properties.Settings.Default;

        private string regulationPath;
        private IBinder regulation;
        private bool encrypted;
        private BindingSource rowSource;
        private Dictionary<string, (int Row, int Cell)> dgvIndices;
        private string lastFindRowPattern, lastFindFieldPattern;

        private string namesDir;
        private string paramsDir;
        [Obsolete]
        private Dictionary<string, PARAM.Layout> layouts;
        private Dictionary<BinderFile, ParamWrapper> fileWrapperCaches;
        private Encoding encoding;

        private const long MemoryUsedMaxSize = 1 << 29;

        public FormMain()
        {
            InitializeComponent();
            fileWrapperCaches = new Dictionary<BinderFile, ParamWrapper>();
            regulation = null;
            rowSource = new BindingSource();
            dgvIndices = new Dictionary<string, (int Row, int Cell)>();
            dgvRows.DataSource = rowSource;
            dgvParams.AutoGenerateColumns = false;
            dgvRows.AutoGenerateColumns = false;
            dgvCells.AutoGenerateColumns = false;
            lastFindRowPattern = "";
            encoding = Encoding.Default;
        }

        private void FormMain_Load(object sender, EventArgs e)
        {
            Text = "黑暗之魂系列游戏参数文件编辑工具 v" + Application.ProductVersion + "（允哥汉化修正版）";

            Location = settings.WindowLocation;
            if (settings.WindowSize.Width >= MinimumSize.Width && settings.WindowSize.Height >= MinimumSize.Height)
                Size = settings.WindowSize;
            if (settings.WindowMaximized)
                WindowState = FormWindowState.Maximized;

            toolStripComboBoxGame.ComboBox.DisplayMember = "Name";
            toolStripComboBoxGame.Items.AddRange(GameMode.Modes);
            var game = (GameType)Enum.Parse(typeof(GameType), settings.GameType);
            toolStripComboBoxGame.SelectedIndex = Array.FindIndex(GameMode.Modes, m => m.Game == game);
            if (toolStripComboBoxGame.SelectedIndex == -1)
                toolStripComboBoxGame.SelectedIndex = 0;

            regulationPath = settings.RegulationPath;
            hideUnusedParamsToolStripMenuItem.Checked = settings.HideUnusedParams;
            verifyDeletionsToolStripMenuItem.Checked = settings.VerifyRowDeletion;
            splitContainer1.SplitterDistance = settings.SplitterDistance1;
            splitContainer2.SplitterDistance = settings.SplitterDistance2;

            LoadParams();

            foreach (Match match in Regex.Matches(settings.DGVIndices, @"[^,]+"))
            {
                var indices = match.Value.Split(':');
                dgvIndices[indices[0]] = (int.Parse(indices[1]), int.Parse(indices[2]));
            }

            if (settings.SelectedParam >= dgvParams.Rows.Count)
                settings.SelectedParam = 0;

            if (dgvParams.Rows.Count <= 0) return;
            dgvParams.ClearSelection();
            dgvParams.Rows[settings.SelectedParam].Selected = true;
            dgvParams.CurrentCell = dgvParams.SelectedCells[0];
        }

        private void FormMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            settings.WindowMaximized = WindowState == FormWindowState.Maximized;
            if (WindowState == FormWindowState.Normal)
            {
                settings.WindowLocation = Location;
                settings.WindowSize = Size;
            }
            else
            {
                settings.WindowLocation = RestoreBounds.Location;
                settings.WindowSize = RestoreBounds.Size;
            }

            settings.GameType = ((GameMode)toolStripComboBoxGame.SelectedItem).Game.ToString();
            settings.RegulationPath = regulationPath;
            settings.HideUnusedParams = hideUnusedParamsToolStripMenuItem.Checked;
            settings.VerifyRowDeletion = verifyDeletionsToolStripMenuItem.Checked;
            settings.SplitterDistance2 = splitContainer2.SplitterDistance;
            settings.SplitterDistance1 = splitContainer1.SplitterDistance;

            if (dgvParams.SelectedCells.Count > 0)
                settings.SelectedParam = dgvParams.SelectedCells[0].RowIndex;

            // Force saving the dgv indices
            dgvParams.ClearSelection();
            var indices = dgvIndices.Keys.Select(key => $"{key}:{dgvIndices[key].Row}:{dgvIndices[key].Cell}").ToList();
            settings.DGVIndices = string.Join(",", indices);
        }

        private void LoadParams()
        {
            var resDir = GetResRoot();
            namesDir = $@"{resDir}\Names";
            paramsDir = $@"{resDir}\Params";
            if (!Directory.Exists(namesDir))
                Directory.CreateDirectory(namesDir);
            if (!Directory.Exists(paramsDir))
                Directory.CreateDirectory(paramsDir);
            layouts = Util.LoadLayouts($@"{resDir}\Layouts");
            var paramInfo = ParamInfo.ReadParamInfo($@"{resDir}\ParamInfo.xml");
            var gameMode = (GameMode)toolStripComboBoxGame.SelectedItem;
            var result = Util.LoadParams(regulationPath, paramInfo, layouts, fileWrapperCaches, gameMode, hideUnusedParamsToolStripMenuItem.Checked);

            if (result == null)
            {
                exportToolStripMenuItem.Enabled = false;
            }
            else
            {
                encrypted = result.Encrypted;
                regulation = result.ParamBND;
                exportToolStripMenuItem.Enabled = encrypted;
                foreach (var wrapper in result.ParamWrappers.Where(wrapper => !dgvIndices.ContainsKey(wrapper.Name)))
                {
                    dgvIndices[wrapper.Name] = (0, 0);
                }
                dgvParams.DataSource = result.ParamWrappers;
                toolStripStatusLabel1.Text = regulationPath;

                foreach (DataGridViewRow row in dgvParams.Rows)
                {
                    var wrapper = (ParamWrapper)row.DataBoundItem;
                    if (wrapper.Error)
                        row.Cells[0].Style.BackColor = Color.Pink;
                }
            }
        }

        #region File Menu
        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ofdRegulation.FileName = regulationPath;
            if (ofdRegulation.ShowDialog() != DialogResult.OK) return;
            regulationPath = ofdRegulation.FileName;
            LoadParams();
            ResourceUtil.ClearMemory(MemoryUsedMaxSize);
            SystemSounds.Asterisk.Play();
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var entry in fileWrapperCaches)
                entry.Key.Bytes = entry.Value.Param.Write();

            var gameMode = (GameMode)toolStripComboBoxGame.SelectedItem;
            if (!File.Exists(regulationPath + ".bak"))
                File.Copy(regulationPath, regulationPath + ".bak");

            if (encrypted)
            {
                if (gameMode.Game == GameType.DarkSouls2)
                    Util.EncryptDS2Regulation(regulationPath, regulation as BND4);
                else if (gameMode.Game == GameType.DarkSouls3)
                    SFUtil.EncryptDS3Regulation(regulationPath, regulation as BND4);
                else
                    Util.ShowError("只支持黑魂2和黑魂3的加密文件！");
            }
            else
            {
                if (regulation is BND3 bnd3)
                    bnd3.Write(regulationPath);
                else if (regulation is BND4 bnd4)
                    bnd4.Write(regulationPath);
            }
            SystemSounds.Asterisk.Play();
        }

        private void RestoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (File.Exists(regulationPath + ".bak"))
            {
                DialogResult choice = MessageBox.Show("Are you sure you want to restore the backup?",
                    "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (choice == DialogResult.Yes)
                {
                    try
                    {
                        File.Copy(regulationPath + ".bak", regulationPath, true);
                        LoadParams();
                        SystemSounds.Asterisk.Play();
                    }
                    catch (Exception ex)
                    {
                        Util.ShowError($"Failed to restore backup\r\n\r\n{regulationPath}.bak\r\n\r\n{ex}");
                    }
                }
            }
            else
            {
                Util.ShowError($"There is no backup to restore at:\r\n\r\n{regulationPath}.bak");
            }
        }

        private void ExploreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Process.Start(Path.GetDirectoryName(regulationPath));
            }
            catch
            {
                SystemSounds.Hand.Play();
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var bnd4 = regulation as BND4;
            fbdExport.SelectedPath = Path.GetDirectoryName(regulationPath);
            if (fbdExport.ShowDialog() == DialogResult.OK)
            {
                BND4 paramBND = new BND4
                {
                    BigEndian = false,
                    Compression = DCX.Type.DCX_DFLT_10000_44_9,
                    Files = regulation.Files.Where(f => f.Name.EndsWith(".param")).ToList()
                };

                BND4 stayBND = new BND4
                {
                    BigEndian = false,
                    Compression = DCX.Type.DCX_DFLT_10000_44_9,
                    Files = regulation.Files.Where(f => f.Name.EndsWith(".stayparam")).ToList()
                };

                string dir = fbdExport.SelectedPath;
                try
                {
                    paramBND.Write($@"{dir}\gameparam_dlc2.parambnd.dcx");
                    stayBND.Write($@"{dir}\stayparam.parambnd.dcx");
                }
                catch (Exception ex)
                {
                    Util.ShowError($"Failed to write exported parambnds.\r\n\r\n{ex}");
                }
            }
        }
        #endregion

        #region Edit Menu
        private void AddRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateRow("Add a new row...");
        }

        private void DuplicateRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvRows.SelectedCells.Count == 0)
            {
                Util.ShowError("You can't duplicate a row without one selected!");
                return;
            }

            int index = dgvRows.SelectedCells[0].RowIndex;
            ParamWrapper wrapper = (ParamWrapper)rowSource.DataSource;
            PARAM.Row oldRow = wrapper.Rows[index];
            PARAM.Row newRow;
            if ((newRow = CreateRow("Duplicate a row...")) != null)
            {
                for (int i = 0; i < oldRow.Cells.Count; i++)
                {
                    newRow.Cells[i].Value = oldRow.Cells[i].Value;
                }
            }
        }

        private PARAM.Row CreateRow(string prompt)
        {
            if (rowSource.DataSource == null)
            {
                Util.ShowError("You can't create a row with no param selected!");
                return null;
            }

            PARAM.Row result = null;
            var newRowForm = new FormNewRow(prompt);
            if (newRowForm.ShowDialog() == DialogResult.OK)
            {
                var id = newRowForm.ResultID;
                string name = newRowForm.ResultName;
                ParamWrapper paramWrapper = (ParamWrapper)rowSource.DataSource;
                if (paramWrapper.Rows.Any(row => row.ID == id))
                {
                    Util.ShowError($"A row with this ID already exists: {id}");
                }
                else
                {
                    result = new PARAM.Row(id, name, paramWrapper.Paramdef);
                    rowSource.Add(result);
                    paramWrapper.Rows.Sort((r1, r2) => r1.ID.CompareTo(r2.ID));

                    int index = paramWrapper.Rows.FindIndex(row => ReferenceEquals(row, result));
                    int displayedRows = dgvRows.DisplayedRowCount(false);
                    dgvRows.FirstDisplayedScrollingRowIndex = Math.Max(0, index - displayedRows / 2);
                    dgvRows.ClearSelection();
                    dgvRows.Rows[index].Selected = true;
                    dgvRows.Refresh();
                }
            }
            return result;
        }

        private void DeleteRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgvRows.SelectedCells.Count > 0)
            {
                DialogResult choice = DialogResult.Yes;
                if (verifyDeletionsToolStripMenuItem.Checked)
                    choice = MessageBox.Show("Are you sure you want to delete this row?",
                        "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (choice == DialogResult.Yes)
                {
                    int rowIndex = dgvRows.SelectedCells[0].RowIndex;
                    rowSource.RemoveAt(rowIndex);

                    // If you remove a row it automatically selects the next one, but if you remove the last row
                    // it doesn't automatically select the previous one
                    if (rowIndex == dgvRows.RowCount)
                    {
                        if (dgvRows.RowCount > 0)
                            dgvRows.Rows[dgvRows.RowCount - 1].Selected = true;
                        else
                            dgvCells.DataSource = null;
                    }
                }
            }
        }

        private void ImportNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var replace = MessageBox.Show("如果行已经有名称，是否覆盖它？\r\n单击“是”覆盖现有名称。\r\n单击“否”跳过现有名称。", "导入 Names", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            var paramFiles = ((List<ParamWrapper>)dgvParams.DataSource).Where(paramFile => File.Exists($@"{namesDir}\{paramFile.Name}.csv")).ToArray();
            Parallel.ForEach(Partitioner.Create(0, paramFiles.Length), (tuple, state) =>
            {
                var (i, size) = tuple;
                while (i < size)
                {
                    var paramFile = paramFiles[i++];
                    var path = $@"{namesDir}\{paramFile.Name}.csv";
                    var code = FileEncodingUtil.GetEncoding(path);
                    var records = File.ReadAllLines(path, code);
                    var names = new Dictionary<long, string>();
                    foreach (var record in records)
                    {
                        var fields = Regex.Split(record, ",");
                        if (2 != fields.Length || ((fields[1] = fields[1].Trim()) == "?")) continue;
                        var id = long.Parse(fields[0]);
                        names[id] = fields[1];
                    }

                    foreach (var row in paramFile.Rows.Where(row =>
                        names.ContainsKey(row.ID) && (replace || string.IsNullOrEmpty(row.Name))))
                        row.Name = names[row.ID];
                    names.Clear();
                    encoding = code;
                }
            });
            dgvRows.Refresh();
            SystemSounds.Asterisk.Play();
        }

        private void ImportParamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var paramFiles = (List<ParamWrapper>)dgvParams.DataSource;
            foreach (var paramFile in paramFiles.Where(paramFile => File.Exists($@"{paramsDir}\{paramFile.Name}.csv") && layouts.ContainsKey(paramFile.Param.ParamType)))
            {
                var path = $@"{paramsDir}\{paramFile.Name}.csv";
                var code = FileEncodingUtil.GetEncoding(path);
                var records = File.ReadAllLines(path, code);
                if (2 > records.Length) return;
                var layout = layouts[paramFile.Param.ParamType];
                var rowsCache = new Dictionary<long, PARAM.Row>();
                foreach (var row in paramFile.Rows)
                    rowsCache[row.ID] = row;
                var cellsCount = layout.Count(entry => entry.Type != CellType.dummy8);
                Parallel.ForEach(Partitioner.Create(1, records.Length), (tuple, state) =>
                {
                    var (i, size) = tuple;
                    while (i < size)
                    {
                        var fields = Regex.Split(records[i++], ",");
                        if (cellsCount + 2 != fields.Length) continue;
                        fields[1] = "?" == (fields[1] = fields[1].Trim()) ? "" : fields[1];
                        var id = Convert.ToInt32(fields[0]);
                        PARAM.Row row;
                        if (rowsCache.ContainsKey(id))
                        {
                            row = rowsCache[id];
                            row.Name = fields[1];
                        }
                        else
                        {
                            row = new PARAM.Row(id, fields[1], paramFile.Paramdef);
                            rowsCache[id] = row;
                            paramFile.Rows.Add(row);
                        }
                        for (int j = 0, k = 0; j < row.Cells.Count; j++)
                        {
                            var cell = row.Cells[j];
                            if (cell.Def.DisplayType == DefType.dummy8) continue;
                            ParamUtil.CastCellValue(cell, fields[2 + k++]);
                        }
                    }
                });
                rowsCache.Clear();
                encoding = code;
                paramFile.Rows.Sort((r1, r2) => r1.ID.CompareTo(r2.ID));
            }
            dgvRows.Refresh();
            dgvCells.Refresh();
            ResourceUtil.ClearMemory(MemoryUsedMaxSize);
            SystemSounds.Asterisk.Play();
        }

        private void ExportNamesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var paramFiles = (List<ParamWrapper>)dgvParams.DataSource;
            Parallel.ForEach(Partitioner.Create(0, paramFiles.Count), (tuple, state) =>
            {
                var (i, size) = tuple;
                while (i < size)
                {
                    var paramFile = paramFiles[i++];
                    var sb = new StringBuilder();
                    foreach (var row in paramFile.Param.Rows.Where(row => !string.IsNullOrEmpty(row.Name)))
                        sb.AppendLine($"{row.ID},{row.Name}");
                    try
                    {
                        File.WriteAllText($@"{namesDir}\{paramFile.Name}.csv", sb.ToString(), encoding);
                    }
                    catch (Exception ex)
                    {
                        Util.ShowError($"导出Names到文件{paramFile.Name}.csv失败！\r\n\r\n{ex}");
                    }
                    finally
                    {
                        sb.Clear();
                    }
                }
            });
            SystemSounds.Asterisk.Play();
        }

        private void ExportParamsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var paramFiles = (List<ParamWrapper>)dgvParams.DataSource;
            Parallel.ForEach(Partitioner.Create(0, paramFiles.Count), (tuple, state) =>
            {
                var (i, size) = tuple;
                while (i < size)
                {
                    var paramFile = paramFiles[i++];
                    var sb = new StringBuilder("ID,Name");
                    foreach (var entry in layouts[paramFile.Param.ParamType].Where(entry => entry.Type != CellType.dummy8))
                        sb.Append($",{entry.Name}({entry.Type})");
                    sb.AppendLine();
                    foreach (var row in paramFile.Param.Rows)
                    {
                        var name = string.IsNullOrEmpty(row.Name) ? "?" : row.Name;
                        sb.Append($"{row.ID},{name}");
                        var cells = row.Cells.Where(cell => cell.Def.DisplayType != DefType.dummy8).ToList();
                        foreach (var value in cells.Select(cell =>
                            1 == cell.Def.BitSize ? Convert.ToBoolean(cell.Value) : cell.Value))
                            sb.Append($",{value}");
                        sb.AppendLine();
                    }
                    try
                    {
                        File.WriteAllText($@"{paramsDir}\{paramFile.Name}.csv", sb.ToString(), encoding);
                    }
                    catch (Exception ex)
                    {
                        Util.ShowError($"导出Params到文件失败: {paramFile.Name}.csv\r\n\r\n{ex}");
                    }
                    finally
                    {
                        sb.Clear();
                    }
                }
            });
            ResourceUtil.ClearMemory(MemoryUsedMaxSize);
            SystemSounds.Asterisk.Play();
        }

        private void FindRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var findForm = new FormFind("Find row with name...");
            if (findForm.ShowDialog() == DialogResult.OK)
            {
                FindRow(findForm.ResultPattern);
            }
        }

        private void FindNextRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindRow(lastFindRowPattern);
        }

        private void FindRow(string pattern)
        {
            if (rowSource.DataSource == null)
            {
                Util.ShowError("You can't search for a row when there are no rows!");
                return;
            }

            int startIndex = dgvRows.SelectedCells.Count > 0 ? dgvRows.SelectedCells[0].RowIndex + 1 : 0;
            List<PARAM.Row> rows = ((ParamWrapper)rowSource.DataSource).Rows;
            int index = -1;

            for (int i = 0; i < rows.Count; i++)
            {
                if ((rows[(startIndex + i) % rows.Count].Name ?? "").ToLower().Contains(pattern.ToLower()))
                {
                    index = (startIndex + i) % rows.Count;
                    break;
                }
            }

            if (index != -1)
            {
                int displayedRows = dgvRows.DisplayedRowCount(false);
                dgvRows.FirstDisplayedScrollingRowIndex = Math.Max(0, index - displayedRows / 2);
                dgvRows.ClearSelection();
                dgvRows.Rows[index].Selected = true;
                lastFindRowPattern = pattern;
            }
            else
            {
                Util.ShowError($"No row found matching: {pattern}");
            }
        }

        private void GotoRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var gotoForm = new FormGoto();
            if (gotoForm.ShowDialog() == DialogResult.OK)
            {
                if (rowSource.DataSource == null)
                {
                    Util.ShowError("You can't goto a row when there are no rows!");
                    return;
                }

                long id = gotoForm.ResultID;
                List<PARAM.Row> rows = ((ParamWrapper)rowSource.DataSource).Rows;
                int index = rows.FindIndex(row => row.ID == id);

                if (index != -1)
                {
                    int displayedRows = dgvRows.DisplayedRowCount(false);
                    dgvRows.FirstDisplayedScrollingRowIndex = Math.Max(0, index - displayedRows / 2);
                    dgvRows.ClearSelection();
                    dgvRows.Rows[index].Selected = true;
                }
                else
                {
                    Util.ShowError($"Row ID not found: {id}");
                }
            }
        }

        private void FindFieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var findForm = new FormFind("Find field with name...");
            if (findForm.ShowDialog() == DialogResult.OK)
            {
                FindField(findForm.ResultPattern);
            }
        }

        private void FindNextFieldToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FindField(lastFindFieldPattern);
        }

        private void FindField(string pattern)
        {
            if (dgvCells.DataSource == null)
            {
                Util.ShowError("You can't search for a field when there are no fields!");
                return;
            }

            int startIndex = dgvCells.SelectedCells.Count > 0 ? dgvCells.SelectedCells[0].RowIndex + 1 : 0;
            var cells = (PARAM.Cell[])dgvCells.DataSource;
            int index = -1;

            for (int i = 0; i < cells.Length; i++)
            {
                if ((cells[(startIndex + i) % cells.Length].Def.DisplayName ?? "").ToLower().Contains(pattern.ToLower()))
                {
                    index = (startIndex + i) % cells.Length;
                    break;
                }
            }

            if (index != -1)
            {
                int displayedRows = dgvCells.DisplayedRowCount(false);
                dgvCells.FirstDisplayedScrollingRowIndex = Math.Max(0, index - displayedRows / 2);
                dgvCells.ClearSelection();
                dgvCells.Rows[index].Selected = true;
                lastFindFieldPattern = pattern;
            }
            else
            {
                Util.ShowError($"No field found matching: {pattern}");
            }
        }
        #endregion

        #region dgvParams
        private void DgvParams_SelectionChanged(object sender, EventArgs e)
        {
            if (rowSource.DataSource != null)
            {
                ParamWrapper paramFile = (ParamWrapper)rowSource.DataSource;
                (int Row, int Cell) indices = (0, 0);

                if (dgvRows.SelectedCells.Count > 0)
                    indices.Row = dgvRows.SelectedCells[0].RowIndex;
                else if (dgvRows.FirstDisplayedScrollingRowIndex >= 0)
                    indices.Row = dgvRows.FirstDisplayedScrollingRowIndex;

                if (dgvCells.FirstDisplayedScrollingRowIndex >= 0)
                    indices.Cell = dgvCells.FirstDisplayedScrollingRowIndex;

                dgvIndices[paramFile.Name] = indices;
            }

            rowSource.DataSource = null;
            dgvCells.DataSource = null;
            if (dgvParams.SelectedCells.Count > 0)
            {
                ParamWrapper paramFile = (ParamWrapper)dgvParams.SelectedCells[0].OwningRow.DataBoundItem;
                // Yes, I need to set this again every time because it gets cleared out when you null the DataSource for some stupid reason
                rowSource.DataMember = "Rows";
                rowSource.DataSource = paramFile;
                (int Row, int Cell) indices = dgvIndices[paramFile.Name];

                if (indices.Row >= dgvRows.RowCount)
                    indices.Row = dgvRows.RowCount - 1;

                if (indices.Row < 0)
                    indices.Row = 0;

                dgvIndices[paramFile.Name] = indices;
                dgvRows.ClearSelection();
                if (dgvRows.RowCount > 0)
                {
                    dgvRows.FirstDisplayedScrollingRowIndex = indices.Row;
                    dgvRows.Rows[indices.Row].Selected = true;
                }
            }
        }

        private void DgvParams_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                var paramWrapper = (ParamWrapper)dgvParams.Rows[e.RowIndex].DataBoundItem;
                e.ToolTipText = paramWrapper.Description;
            }
        }
        #endregion

        #region dgvRows
        private void DgvRows_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRows.SelectedCells.Count > 0)
            {
                ParamWrapper paramFile = (ParamWrapper)dgvParams.SelectedCells[0].OwningRow.DataBoundItem;
                (int Row, int Cell) indices = dgvIndices[paramFile.Name];
                if (dgvCells.FirstDisplayedScrollingRowIndex >= 0)
                    indices.Cell = dgvCells.FirstDisplayedScrollingRowIndex;

                PARAM.Row row = (PARAM.Row)dgvRows.SelectedCells[0].OwningRow.DataBoundItem;
                dgvCells.DataSource = row.Cells.Where(cell => cell.Def.DisplayType != DefType.dummy8).ToArray();

                if (indices.Cell >= dgvCells.RowCount)
                    indices.Cell = dgvCells.RowCount - 1;

                if (indices.Cell < 0)
                    indices.Cell = 0;

                dgvIndices[paramFile.Name] = indices;
                if (dgvCells.RowCount > 0)
                    dgvCells.FirstDisplayedScrollingRowIndex = indices.Cell;
            }
        }

        private void DgvRows_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.ColumnIndex == 0)
            {
                bool parsed = int.TryParse((string)e.FormattedValue, out int value);
                if (!parsed || value < 0)
                {
                    Util.ShowError("Row ID must be a positive integer.\r\nEnter a valid number or press Esc to cancel.");
                    e.Cancel = true;
                }
            }
        }
        #endregion

        #region dgvCells
        private void DgvCells_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvCells.Rows)
            {
                var cell = (PARAM.Cell)row.DataBoundItem;
                var paramWrapper = (ParamWrapper)dgvParams.SelectedCells[0].OwningRow.DataBoundItem;
                if (0 < paramWrapper.Layout.Enums?.Count)
                {
                    var items = paramWrapper.Layout.Enums[cell.Def.InternalType];
                    if (items.Any(i => i.Value.Equals(cell.Value)))
                    {
                        row.Cells[2] = new DataGridViewComboBoxCell
                        {
                            DataSource = items,
                            DisplayMember = "Name",
                            ValueMember = "Value",
                            ValueType = cell.Value.GetType()
                        };
                    }
                }
                else if (1 == cell.Def.BitSize)
                {
                    row.Cells[2] = new DataGridViewCheckBoxCell();
                }
                else
                {
                    row.Cells[2].ValueType = cell.Value.GetType();
                }
            }
        }

        private void DgvCells_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            var row = dgvCells.Rows[e.RowIndex];
            var cell = (PARAM.Cell)row.DataBoundItem;
            switch (e.ColumnIndex)
            {
                case 0:
                    e.Value = Util.GetValueByNameExpr(cell, dgvCellsTypeCol.DataPropertyName);
                    break;
                case 1:
                    e.Value = Util.GetValueByNameExpr(cell, dgvCellsNameCol.DataPropertyName);
                    break;
                case 2:
                    if (!(row.Cells[2] is DataGridViewComboBoxCell || 1 == cell.Def.BitSize))
                    {
                        if (cell.Def.DisplayType == DefType.u8)
                        {
                            e.Value = $"0x{e.Value:X2}";
                            e.FormattingApplied = true;
                        }
                        else if (cell.Def.DisplayType == DefType.u16)
                        {
                            e.Value = $"0x{e.Value:X4}";
                            e.FormattingApplied = true;
                        }
                        else if (cell.Def.DisplayType == DefType.u32)
                        {
                            e.Value = $"0x{e.Value:X8}";
                            e.FormattingApplied = true;
                        }
                    }
                    break;
            }
        }

        private void DgvCells_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            var row = dgvCells.Rows[e.RowIndex];
            var cell = (PARAM.Cell)row.DataBoundItem;
            if (e.ColumnIndex < 2 || row.Cells[e.ColumnIndex] is DataGridViewComboBoxCell || 1 == cell.Def.BitSize)
                return;

            try
            {
                if (cell.Def.DisplayType == DefType.u8)
                    Convert.ToByte((string)e.FormattedValue, 16);
                else if (cell.Def.DisplayType == DefType.u16)
                    Convert.ToUInt16((string)e.FormattedValue, 16);
                else if (cell.Def.DisplayType == DefType.u32)
                    Convert.ToUInt32((string)e.FormattedValue, 16);
            }
            catch
            {
                e.Cancel = true;
                dgvCells.EditingPanel.BackColor = Color.Pink;
                if (null != dgvCells.EditingControl)
                    dgvCells.EditingControl.BackColor = Color.Pink;
                SystemSounds.Hand.Play();
            }
        }

        private void DgvCells_CellParsing(object sender, DataGridViewCellParsingEventArgs e)
        {
            var row = dgvCells.Rows[e.RowIndex];
            var cell = (PARAM.Cell)row.DataBoundItem;
            if (e.ColumnIndex < 2 || row.Cells[e.ColumnIndex] is DataGridViewComboBoxCell)
                return;

            if (1 == cell.Def.BitSize)
            {
                e.Value = (byte)((bool)e.Value ? 1 : 0);
                e.ParsingApplied = true;
            }
            else if (cell.Def.DisplayType == DefType.u8)
            {
                e.Value = Convert.ToByte((string)e.Value, 16);
                e.ParsingApplied = true;
            }
            else if (cell.Def.DisplayType == DefType.u16)
            {
                e.Value = Convert.ToUInt16((string)e.Value, 16);
                e.ParsingApplied = true;
            }
            else if (cell.Def.DisplayType == DefType.u32)
            {
                e.Value = Convert.ToUInt32((string)e.Value, 16);
                e.ParsingApplied = true;
            }
        }

        private void DgvCells_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
            dgvCells.EditingPanel.BackColor = Color.Pink;
            if (dgvCells.EditingControl != null)
                dgvCells.EditingControl.BackColor = Color.Pink;
            SystemSounds.Hand.Play();
        }

        private void DgvCells_CellToolTipTextNeeded(object sender, DataGridViewCellToolTipTextNeededEventArgs e)
        {
            if (e.RowIndex >= 0 && e.ColumnIndex == 1)
            {
                var cell = (PARAM.Cell)dgvCells.Rows[e.RowIndex].DataBoundItem;
                e.ToolTipText = cell.Def.Description;
            }
        }
        #endregion

        private string GetResRoot()
        {
            var gameMode = (GameMode)toolStripComboBoxGame.SelectedItem;
#if DEBUG
            return $@"..\..\..\..\dist\res\{gameMode.Directory}";
#else
            return $@"res\{gameMode.Directory}";
#endif
        }
    }
}

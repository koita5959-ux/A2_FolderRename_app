using DesktopKit.Common;
using System.Windows.Forms;
using System.Drawing;

namespace DesktopKit.FolderRename
{
    public class MainForm : BaseForm
    {
        private Button btnSelectFolder = null!;
        private TextBox txtFolderPath = null!;
        private Label lblFileType = null!;
        private ComboBox cmbFileType = null!;
        private RadioButton rbWordSequence = null!;
        private RadioButton rbReplace = null!;
        private Panel panelWordSequence = null!;
        private TextBox txtPrefix = null!;
        private NumericUpDown nudStart = null!;
        private NumericUpDown nudDigits = null!;
        private Panel panelReplace = null!;
        private TextBox txtSearch = null!;
        private TextBox txtReplaceWith = null!;
        private Button btnPreview = null!;
        private DataGridView dgvPreview = null!;
        private Button btnExecute = null!;
        private Button btnUndo = null!;
        private Button btnSelectAll = null!;
        private Button btnDeselectAll = null!;

        private readonly RenameEngine _engine = new();
        private readonly RenameHistory _history = new();
        private List<(string oldName, string newName)>? _currentPlan;
        private bool _previewShown;

        public MainForm()
        {
            ComponentName = "FolderRename";
            LoadIcon();
            InitializeControls();
            WireEvents();
            UpdateMethodPanels();
        }

        private void LoadIcon()
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "FolderRename.ico");
            if (File.Exists(iconPath))
                Icon = new Icon(iconPath);
        }

        private void InitializeControls()
        {
            // --- 上部パネル: フォルダ選択 + フィルタ + リネーム方式 ---
            var topPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 195,
                Padding = new Padding(10, 10, 10, 5)
            };

            // フォルダ選択行
            btnSelectFolder = new Button
            {
                Text = "フォルダを選択",
                Location = new Point(10, 8),
                Size = new Size(120, 28)
            };

            txtFolderPath = new TextBox
            {
                ReadOnly = true,
                Location = new Point(140, 10),
                Size = new Size(620, 23),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            // ファイル形式フィルタ
            lblFileType = new Label
            {
                Text = "ファイル形式:",
                Location = new Point(10, 45),
                AutoSize = true
            };

            cmbFileType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(110, 42),
                Size = new Size(150, 23)
            };
            cmbFileType.Items.AddRange(new object[] { "全ファイル", ".jpg", ".png", ".xlsx", ".docx", ".txt", ".csv" });
            cmbFileType.SelectedIndex = 0;

            // 方式A: ワード＋シーケンス
            rbWordSequence = new RadioButton
            {
                Text = "ワード＋シーケンス",
                Location = new Point(10, 75),
                AutoSize = true,
                Checked = true
            };

            panelWordSequence = new Panel
            {
                Location = new Point(30, 98),
                Size = new Size(730, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblPrefix = new Label { Text = "接頭辞:", Location = new Point(0, 5), AutoSize = true };
            txtPrefix = new TextBox { Location = new Point(60, 2), Size = new Size(150, 23) };
            var lblStart = new Label { Text = "開始:", Location = new Point(225, 5), AutoSize = true };
            nudStart = new NumericUpDown { Value = 1, Minimum = 0, Maximum = 99999, Location = new Point(270, 2), Size = new Size(60, 23) };
            var lblDigits = new Label { Text = "桁:", Location = new Point(345, 5), AutoSize = true };
            nudDigits = new NumericUpDown { Value = 3, Minimum = 1, Maximum = 10, Location = new Point(375, 2), Size = new Size(60, 23) };

            panelWordSequence.Controls.AddRange(new Control[] { lblPrefix, txtPrefix, lblStart, nudStart, lblDigits, nudDigits });

            // 方式B: 部分置き換え
            rbReplace = new RadioButton
            {
                Text = "部分置き換え",
                Location = new Point(10, 132),
                AutoSize = true
            };

            panelReplace = new Panel
            {
                Location = new Point(30, 155),
                Size = new Size(730, 30),
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
            };

            var lblSearch = new Label { Text = "検索:", Location = new Point(0, 5), AutoSize = true };
            txtSearch = new TextBox { Location = new Point(50, 2), Size = new Size(150, 23) };
            var lblReplaceWith = new Label { Text = "置換:", Location = new Point(215, 5), AutoSize = true };
            txtReplaceWith = new TextBox { Location = new Point(265, 2), Size = new Size(150, 23) };

            panelReplace.Controls.AddRange(new Control[] { lblSearch, txtSearch, lblReplaceWith, txtReplaceWith });

            topPanel.Controls.AddRange(new Control[]
            {
                btnSelectFolder, txtFolderPath, lblFileType, cmbFileType,
                rbWordSequence, panelWordSequence, rbReplace, panelReplace
            });

            // --- プレビューボタン（右揃え） ---
            var previewPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35
            };

            btnSelectAll = new Button
            {
                Text = "全選択",
                Size = new Size(80, 28),
                Location = new Point(10, 3)
            };

            btnDeselectAll = new Button
            {
                Text = "全解除",
                Size = new Size(80, 28),
                Location = new Point(95, 3)
            };

            btnPreview = new Button
            {
                Text = "プレビュー",
                Size = new Size(100, 28),
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnPreview.Location = new Point(previewPanel.ClientSize.Width - btnPreview.Width - 10, 3);

            previewPanel.Controls.Add(btnSelectAll);
            previewPanel.Controls.Add(btnDeselectAll);
            previewPanel.Controls.Add(btnPreview);

            // --- 中央: DataGridView ---
            dgvPreview = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            var chkCol = new DataGridViewCheckBoxColumn
            {
                Name = "Select",
                HeaderText = "対象",
                Width = 50,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                FalseValue = false,
                TrueValue = true
            };
            dgvPreview.Columns.Add(chkCol);
            dgvPreview.Columns.Add("Before", "変更前");
            dgvPreview.Columns["Before"]!.ReadOnly = true;
            dgvPreview.Columns.Add("After", "変更後");
            dgvPreview.Columns["After"]!.ReadOnly = true;

            // --- 下部パネル: 実行 + 元に戻す（中央揃え） ---
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                Padding = new Padding(10, 5, 10, 5)
            };

            btnExecute = new Button
            {
                Text = "実行",
                Size = new Size(100, 28),
                Enabled = false
            };

            btnUndo = new Button
            {
                Text = "元に戻す",
                Size = new Size(100, 28),
                Enabled = false
            };

            bottomPanel.Controls.AddRange(new Control[] { btnExecute, btnUndo });
            bottomPanel.Resize += (s, e) => CenterBottomButtons(bottomPanel);
            bottomPanel.Layout += (s, e) => CenterBottomButtons(bottomPanel);

            // --- フォームに追加 ---
            Controls.Add(dgvPreview);
            Controls.Add(previewPanel);
            Controls.Add(topPanel);
            Controls.Add(bottomPanel);
        }

        private void CenterBottomButtons(Panel panel)
        {
            int totalWidth = btnExecute.Width + 10 + btnUndo.Width;
            int startX = (panel.ClientSize.Width - totalWidth) / 2;
            btnExecute.Location = new Point(startX, 8);
            btnUndo.Location = new Point(startX + btnExecute.Width + 10, 8);
        }

        private void WireEvents()
        {
            btnSelectFolder.Click += BtnSelectFolder_Click;
            cmbFileType.SelectedIndexChanged += CmbFileType_SelectedIndexChanged;
            rbWordSequence.CheckedChanged += (s, e) => UpdateMethodPanels();
            rbReplace.CheckedChanged += (s, e) => UpdateMethodPanels();
            btnSelectAll.Click += (s, e) => SetAllCheckboxes(true);
            btnDeselectAll.Click += (s, e) => SetAllCheckboxes(false);
            btnPreview.Click += BtnPreview_Click;
            btnExecute.Click += BtnExecute_Click;
            btnUndo.Click += BtnUndo_Click;
        }

        private void UpdateMethodPanels()
        {
            bool isSequence = rbWordSequence.Checked;
            panelWordSequence.Enabled = isSequence;
            panelReplace.Enabled = !isSequence;
            _previewShown = false;
            _currentPlan = null;
            btnExecute.Enabled = false;
        }

        private void BtnSelectFolder_Click(object? sender, EventArgs e)
        {
            var path = FileDialogHelper.SelectFolder("リネーム対象のフォルダを選択してください");
            if (path == null) return;

            txtFolderPath.Text = path;
            _previewShown = false;
            _currentPlan = null;
            btnExecute.Enabled = false;
            LoadFileList();
        }

        private void CmbFileType_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFolderPath.Text))
            {
                _previewShown = false;
                _currentPlan = null;
                btnExecute.Enabled = false;
                LoadFileList();
            }
        }

        private void LoadFileList()
        {
            dgvPreview.Rows.Clear();
            string folder = txtFolderPath.Text;
            if (!Directory.Exists(folder)) return;

            string filter = cmbFileType.SelectedItem?.ToString() ?? "全ファイル";
            var files = Directory.GetFiles(folder)
                .Select(Path.GetFileName)
                .Where(f => f != null)
                .Cast<string>()
                .Where(f => filter == "全ファイル" || f.EndsWith(filter, StringComparison.OrdinalIgnoreCase))
                .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var file in files)
            {
                dgvPreview.Rows.Add(true, file, "");
            }

            StatusHelper.ShowInfo(StatusLabel, $"{files.Count}件のファイルを検出しました");
        }

        private List<string> GetCurrentFileList()
        {
            var files = new List<string>();
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                var val = row.Cells["Before"].Value?.ToString();
                if (!string.IsNullOrEmpty(val))
                    files.Add(val);
            }
            return files;
        }

        private List<string> GetCheckedFileList()
        {
            var files = new List<string>();
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                bool isChecked = row.Cells["Select"].Value is true;
                var val = row.Cells["Before"].Value?.ToString();
                if (isChecked && !string.IsNullOrEmpty(val))
                    files.Add(val);
            }
            return files;
        }

        private List<string> GetUncheckedFileList()
        {
            var files = new List<string>();
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                bool isChecked = row.Cells["Select"].Value is true;
                var val = row.Cells["Before"].Value?.ToString();
                if (!isChecked && !string.IsNullOrEmpty(val))
                    files.Add(val);
            }
            return files;
        }

        private void SetAllCheckboxes(bool value)
        {
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                row.Cells["Select"].Value = value;
            }
            dgvPreview.RefreshEdit();
        }

        private void BtnPreview_Click(object? sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtFolderPath.Text))
            {
                MessageBox.Show("フォルダを選択してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var checkedFiles = GetCheckedFileList();
            if (checkedFiles.Count == 0)
            {
                MessageBox.Show("対象ファイルが選択されていません。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<(string oldName, string newName)> plan;

            if (rbWordSequence.Checked)
            {
                plan = _engine.PreviewSequence(checkedFiles, txtPrefix.Text, (int)nudStart.Value, (int)nudDigits.Value);
            }
            else
            {
                if (string.IsNullOrEmpty(txtSearch.Text))
                {
                    MessageBox.Show("検索文字列を入力してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                plan = _engine.PreviewReplace(checkedFiles, txtSearch.Text, txtReplaceWith.Text);
            }

            // 重複チェック（改修C）
            var uncheckedFiles = GetUncheckedFileList();
            var duplicates = _engine.FindDuplicates(plan, uncheckedFiles);
            if (duplicates.Count > 0)
            {
                string dupList = string.Join("\n", duplicates.Select(d => $"  {d.newName}"));
                MessageBox.Show(
                    $"以下のリネーム先ファイル名が既存ファイルと重複しています：\n\n{dupList}\n\n設定を変更してください。",
                    "重複エラー", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // プレビュー結果をDataGridViewに反映
            // まず全行のAfter列をクリア
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                row.Cells["After"].Value = "";
            }

            // チェック済み行にのみリネーム後ファイル名を表示
            int planIndex = 0;
            foreach (DataGridViewRow row in dgvPreview.Rows)
            {
                bool isChecked = row.Cells["Select"].Value is true;
                if (isChecked && planIndex < plan.Count)
                {
                    row.Cells["After"].Value = plan[planIndex].newName;
                    planIndex++;
                }
            }

            _previewShown = true;
            _currentPlan = plan;
            btnExecute.Enabled = true;
            StatusHelper.ShowInfo(StatusLabel, $"{plan.Count}件のリネームをプレビュー中");
        }

        private void BtnExecute_Click(object? sender, EventArgs e)
        {
            if (!_previewShown || _currentPlan == null)
            {
                MessageBox.Show("先にプレビューを確認してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int renameCount = _currentPlan.Count(p => p.oldName != p.newName);
            var result = MessageBox.Show(
                $"{renameCount}件のファイルをリネームします。よろしいですか？",
                "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            string folder = txtFolderPath.Text;
            _history.Save(folder, _currentPlan);

            _engine.Execute(folder, _currentPlan, (current, total) =>
            {
                StatusHelper.ShowInfo(StatusLabel, $"{current}/{total} リネーム中…");
                Application.DoEvents();
            });

            // 完了後: ファイル一覧を再読み込みして最新状態を反映
            _previewShown = false;
            _currentPlan = null;
            btnExecute.Enabled = false;
            btnUndo.Enabled = true;
            LoadFileList();
            StatusHelper.ShowSuccess(StatusLabel, $"{renameCount}件のリネームが完了しました");
        }

        private void BtnUndo_Click(object? sender, EventArgs e)
        {
            if (!_history.CanUndo) return;

            var result = MessageBox.Show(
                "リネームを元に戻します。よろしいですか？",
                "確認", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            _history.Undo();
            _history.Clear();
            btnUndo.Enabled = false;

            // 元のファイル名で一覧を再取得
            LoadFileList();
            StatusHelper.ShowSuccess(StatusLabel, "リネームを元に戻しました");
        }
    }
}

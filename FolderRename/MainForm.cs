using DesktopKit.Common;
using System.Windows.Forms;
using System.Drawing;

namespace DesktopKit.FolderRename
{
    /// <summary>
    /// FolderRename（一括リネームツール）のメインフォーム。
    /// </summary>
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

        /// <summary>
        /// MainFormのコンストラクタ。
        /// </summary>
        public MainForm()
        {
            ComponentName = "FolderRename";
            InitializeControls();
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

            // --- プレビューボタン ---
            var previewPanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 35
            };

            btnPreview = new Button
            {
                Text = "プレビュー",
                Location = new Point(10, 3),
                Size = new Size(100, 28)
            };

            previewPanel.Controls.Add(btnPreview);

            // --- 中央: DataGridView ---
            dgvPreview = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            dgvPreview.Columns.Add("Before", "変更前");
            dgvPreview.Columns.Add("After", "変更後");

            // --- 下部パネル: 実行 + 元に戻す ---
            var bottomPanel = new Panel
            {
                Dock = DockStyle.Bottom,
                Height = 45,
                Padding = new Padding(10, 5, 10, 5)
            };

            btnExecute = new Button
            {
                Text = "実行",
                Location = new Point(10, 8),
                Size = new Size(100, 28),
                Enabled = false
            };

            btnUndo = new Button
            {
                Text = "元に戻す",
                Location = new Point(120, 8),
                Size = new Size(100, 28),
                Enabled = false
            };

            bottomPanel.Controls.AddRange(new Control[] { btnExecute, btnUndo });

            // --- フォームに追加 ---
            Controls.Add(dgvPreview);
            Controls.Add(previewPanel);
            Controls.Add(topPanel);
            Controls.Add(bottomPanel);
        }
    }
}

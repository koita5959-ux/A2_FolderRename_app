# 制作指示書：FolderRename v1.02

---

## 概要

FolderRename v1.01 に対して、以下3点の改修を一括で実施する。

| ID | 改修内容 | 規模 |
|---|---|---|
| A | 接頭辞なし（空白）でのシーケンスNoリネームを許可 | 小 |
| B | チェックボックスによるファイル選択式リネーム | 中 |
| C | リネーム先ファイル名の重複チェック（安全機構） | 中 |

3点は密接に関連するため、1フェーズで一括実装する。

---

## 前提

- 本指示書に記載されたファイル名・クラス名・メソッド名を厳守すること
- Common（BaseForm, StatusHelper, FileDialogHelper, AppSettings）は変更しない
- RenameEngine.cs、RenameHistory.cs、MainForm.cs が改修対象
- Program.cs、csproj、Directory.Build.props は変更なし
- ビルドが通ること、既存機能が壊れていないことを確認すること

---

## 改修A：接頭辞なしシーケンスNo

### 対象ファイル
- MainForm.cs

### 改修内容

`BtnPreview_Click` メソッド内の接頭辞バリデーションを削除する。

**現行コード（311-315行目）：**
```csharp
if (string.IsNullOrEmpty(txtPrefix.Text))
{
    MessageBox.Show("接頭辞を入力してください。", "確認", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    return;
}
```

**改修後：** このif文を丸ごと削除する。

### 理由

RenameEngine.PreviewSequence は prefix に空文字が渡されても正しく動作する。
`$"{prefix}{seq}{ext}"` は prefix="" なら `"001.jpg"` を生成する。
つまりEngine側の改修は不要で、UIのバリデーションを外すだけで機能が成立する。

### 動作確認ポイント
- 接頭辞を空欄にしてプレビュー → `001.jpg`, `002.jpg` ... と表示されること
- 接頭辞を入力してプレビュー → 従来通り `aaa001.jpg` のように動作すること

---

## 改修B：チェックボックスによるファイル選択式リネーム

### 対象ファイル
- MainForm.cs
- RenameEngine.cs

### 改修内容

#### B-1. DataGridViewにチェックボックス列を追加（MainForm.cs）

`InitializeControls` メソッド内、DataGridViewの列定義を以下に変更する。

**現行：**
```csharp
dgvPreview.Columns.Add("Before", "変更前");
dgvPreview.Columns.Add("After", "変更後");
```

**改修後：**
```csharp
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
dgvPreview.Columns.Add("After", "変更後");
```

列順は「対象（チェック）」「変更前」「変更後」の3列とする。

#### B-2. 「全選択／全解除」ボタンの追加（MainForm.cs）

DataGridView上部のプレビューボタンと同じパネル（`previewPanel`）に、「全選択」「全解除」ボタンを追加する。

**フィールド追加：**
```csharp
private Button btnSelectAll = null!;
private Button btnDeselectAll = null!;
```

**InitializeControls内、previewPanelのボタン配置：**
```csharp
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

previewPanel.Controls.Add(btnSelectAll);
previewPanel.Controls.Add(btnDeselectAll);
```

配置：左端に「全選択」「全解除」、右端に「プレビュー」（既存）。

**WireEventsにイベント追加：**
```csharp
btnSelectAll.Click += (s, e) => SetAllCheckboxes(true);
btnDeselectAll.Click += (s, e) => SetAllCheckboxes(false);
```

**SetAllCheckboxesメソッドを追加：**
```csharp
private void SetAllCheckboxes(bool value)
{
    foreach (DataGridViewRow row in dgvPreview.Rows)
    {
        row.Cells["Select"].Value = value;
    }
    dgvPreview.RefreshEdit();
}
```

#### B-3. フォルダ読み込み時のチェックボックス初期値（MainForm.cs）

`LoadFileList` メソッド内、行追加部分を変更する。

**現行：**
```csharp
foreach (var file in files)
{
    dgvPreview.Rows.Add(file, "");
}
```

**改修後：**
```csharp
foreach (var file in files)
{
    dgvPreview.Rows.Add(true, file, "");
}
```

初期状態は全チェックON。利用者は外したいものだけ外す運用とする。

#### B-4. チェック済みファイルのみ取得するメソッド変更（MainForm.cs）

`GetCurrentFileList` メソッドを改修する。

**現行：**
```csharp
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
```

**改修後：**
```csharp
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
```

`GetCurrentFileList` は既存のまま残す（全ファイル一覧取得に引き続き使う）。
`GetCheckedFileList` を新規追加し、プレビュー・実行時はこちらを使う。

#### B-5. プレビュー処理の改修（MainForm.cs）

`BtnPreview_Click` メソッドを改修する。

**変更点：**

1. ファイル取得を `GetCurrentFileList()` → `GetCheckedFileList()` に変更
2. チェック0件のバリデーションを追加
3. プレビュー結果の表示を、チェック済み行にのみ反映する

**改修後のBtnPreview_Click全体：**
```csharp
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

    // 重複チェック（改修C参照）
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
```

#### B-6. 未チェックファイル一覧の取得メソッド追加（MainForm.cs）

重複チェック（改修C）で使用する。

```csharp
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
```

#### B-7. 実行処理の改修（MainForm.cs）

`BtnExecute_Click` 内の完了後処理で、チェック状態を考慮する。

**改修後のBtnExecute_Click全体：**
```csharp
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
```

**注意：** 現行ではリネーム後に手動で変更前列を書き換えていたが、改修後は `LoadFileList()` で再読み込みする方式に変更する。これによりチェックボックスの状態もリセットされ（全ON）、フォルダの実際のファイル状態と一覧が確実に一致する。

#### B-8. シーケンスNoの採番仕様

チェックされたファイルだけで詰めて連番を振る。

例：10ファイル中3つだけチェック、開始1、桁3の場合
→ チェックしたファイルに対して 001, 002, 003 を割り当てる。
チェックを外したファイルはリネーム対象外（変更後列は空欄）。

RenameEngine.PreviewSequence は引数として受け取った files リストに対して連番を振る設計のため、呼び出し側（MainForm）がチェック済みファイルだけを渡せば自動的にこの仕様が実現される。Engine側の改修は不要。

---

## 改修C：リネーム先ファイル名の重複チェック

### 対象ファイル
- RenameEngine.cs

### 改修内容

#### C-1. FindDuplicatesメソッドの追加（RenameEngine.cs）

```csharp
/// <summary>
/// リネームプランの中で、リネーム先ファイル名が以下のいずれかと重複するものを検出する。
/// 1. プラン内での重複（2つ以上のファイルが同じリネーム先になる）
/// 2. リネーム対象外ファイルとの重複（チェックを外したファイルと名前が被る）
/// </summary>
/// <param name="plan">リネームプラン</param>
/// <param name="excludedFiles">リネーム対象外のファイル名一覧</param>
/// <returns>重複が検出されたリネームエントリのリスト</returns>
public List<(string oldName, string newName)> FindDuplicates(
    List<(string oldName, string newName)> plan,
    List<string> excludedFiles)
{
    var duplicates = new List<(string oldName, string newName)>();
    var newNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    var excludedSet = new HashSet<string>(excludedFiles, StringComparer.OrdinalIgnoreCase);

    foreach (var entry in plan)
    {
        // リネーム対象外ファイルとの重複チェック
        if (excludedSet.Contains(entry.newName))
        {
            duplicates.Add(entry);
            continue;
        }

        // プラン内での重複チェック（同じnewNameが2回以上出現）
        if (!newNames.Add(entry.newName))
        {
            duplicates.Add(entry);
        }
    }

    return duplicates;
}
```

### 動作仕様

- プレビュー実行時に `FindDuplicates` を呼び出す（改修B-5参照）
- 重複が1件でも検出された場合、エラーダイアログを表示し、プレビューを確定しない
- 実行ボタンは有効化されず、利用者は設定を変更して再度プレビューする必要がある
- 大文字小文字を区別しない（Windowsのファイルシステム仕様に準拠）

### 重複検出の対象

| ケース | 例 | 検出 |
|---|---|---|
| プラン内で同名 | A.jpg→001.jpg, B.jpg→001.jpg | する |
| 対象外ファイルと同名 | チェック外のC.jpgがあり、A.jpg→C.jpg | する |
| 自分自身と同名 | A.jpg→A.jpg（変化なし） | しない（oldName==newNameは改修不要、Executeで既にスキップしている） |

---

## バージョン更新

### 対象ファイル
- build.bat
- setup.iss

### build.bat
バージョン文字列を `1.01` → `1.02` に変更する（2箇所）。

### setup.iss
`AppVersion` と `OutputBaseFilename` のバージョンを `1.01` → `1.02` に変更する。

---

## 改修対象ファイル一覧（まとめ）

| ファイル | 改修 |
|---|---|
| MainForm.cs | A, B-1〜B-7 |
| RenameEngine.cs | C-1 |
| RenameHistory.cs | 変更なし |
| Program.cs | 変更なし |
| Common/* | 変更なし |
| build.bat | バージョン番号更新 |
| setup.iss | バージョン番号更新 |

---

## 把握レポート要求事項

ClaudeCodeは実装前に、以下を把握レポートとして出力すること。

1. 改修A・B・Cそれぞれについて、何をするのかを自分の言葉で説明
2. DataGridViewの列構成（改修後）
3. プレビュー→実行の処理フロー（チェックボックス・重複チェック含む）
4. 改修しないファイルの一覧と、改修しない理由
5. 指示書と異なる実装が必要と判断した場合、その理由

---

## 品質確認ポイント

- [ ] ビルドエラー0件
- [ ] 接頭辞空欄でプレビュー・実行が正常動作すること
- [ ] 接頭辞入力時の従来動作が維持されていること
- [ ] チェックボックスの初期状態が全ONであること
- [ ] 全選択・全解除ボタンが正常動作すること
- [ ] チェック済みファイルのみがプレビュー・実行の対象になること
- [ ] シーケンスNoはチェック済みファイルだけで詰めて連番になること
- [ ] リネーム先ファイル名がプラン内で重複した場合、エラーダイアログが出ること
- [ ] リネーム先ファイル名がチェック外ファイルと重複した場合、エラーダイアログが出ること
- [ ] 重複検出時に実行ボタンが有効化されないこと
- [ ] 部分置き換えモードでも重複チェックが機能すること
- [ ] 元に戻す機能が従来通り動作すること
- [ ] 実行完了後、ファイル一覧が再読み込みされ最新状態が反映されること

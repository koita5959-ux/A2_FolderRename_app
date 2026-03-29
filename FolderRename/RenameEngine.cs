namespace DesktopKit.FolderRename
{
    public class RenameEngine
    {
        public List<(string oldName, string newName)> PreviewSequence(
            List<string> files, string prefix, int startNumber, int digits)
        {
            var result = new List<(string oldName, string newName)>();
            for (int i = 0; i < files.Count; i++)
            {
                string ext = Path.GetExtension(files[i]);
                string seq = (startNumber + i).ToString().PadLeft(digits, '0');
                result.Add((files[i], $"{prefix}{seq}{ext}"));
            }
            return result;
        }

        public List<(string oldName, string newName)> PreviewReplace(
            List<string> files, string search, string replace)
        {
            var result = new List<(string oldName, string newName)>();
            foreach (var file in files)
            {
                string newName = file.Contains(search)
                    ? file.Replace(search, replace)
                    : file;
                result.Add((file, newName));
            }
            return result;
        }

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

        public void Execute(string folderPath, List<(string oldName, string newName)> plan,
            Action<int, int>? onProgress = null)
        {
            for (int i = 0; i < plan.Count; i++)
            {
                var (oldName, newName) = plan[i];
                if (oldName != newName)
                {
                    string srcPath = Path.Combine(folderPath, oldName);
                    string dstPath = Path.Combine(folderPath, newName);
                    File.Move(srcPath, dstPath);
                }
                onProgress?.Invoke(i + 1, plan.Count);
            }
        }
    }
}

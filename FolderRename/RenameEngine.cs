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

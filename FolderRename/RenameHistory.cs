namespace DesktopKit.FolderRename
{
    public class RenameHistory
    {
        private string? _folderPath;
        private List<(string oldName, string newName)>? _plan;

        public bool CanUndo => _plan != null;

        public void Save(string folderPath, List<(string oldName, string newName)> plan)
        {
            _folderPath = folderPath;
            _plan = new List<(string oldName, string newName)>(plan);
        }

        public void Undo()
        {
            if (_folderPath == null || _plan == null)
                return;

            for (int i = _plan.Count - 1; i >= 0; i--)
            {
                var (oldName, newName) = _plan[i];
                if (oldName != newName)
                {
                    string srcPath = Path.Combine(_folderPath, newName);
                    string dstPath = Path.Combine(_folderPath, oldName);
                    File.Move(srcPath, dstPath);
                }
            }
        }

        public void Clear()
        {
            _folderPath = null;
            _plan = null;
        }
    }
}

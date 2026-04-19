using System.Collections.ObjectModel;

namespace Analyzer.Models
{
    public abstract class ExplorerItem
    {
        public string Name { get; set; }
    }

    public class FolderItem : ExplorerItem
    {
        public ObservableCollection<ExplorerItem> Children { get; set; } = new ObservableCollection<ExplorerItem>();
    }

    public class SessionItem : ExplorerItem
    {
        public string FilePath { get; set; }
    }
}

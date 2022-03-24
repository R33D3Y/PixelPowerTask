using System.Collections.ObjectModel;

namespace PixelPowerTask.Models
{
	public class FolderModel : DirectoryModel
	{
		public ObservableCollection<DirectoryModel> DirectoryModelCollection { get; set; }

		public FolderModel(string name, string path, ObservableCollection<DirectoryModel> directoryModelCollection) : base(name, path)
		{
			Name = name;
			Path = path;
			DirectoryModelCollection = directoryModelCollection;
		}
	}
}
namespace PixelPowerTask.Models
{
	public class FileModel : DirectoryModel
	{
		public FileModel(string name, string path) : base(name, path)
		{
			Name = name;
			Path = path;
		}
	}
}
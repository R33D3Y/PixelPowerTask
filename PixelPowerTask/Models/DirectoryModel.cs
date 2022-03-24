namespace PixelPowerTask.Models
{
	public class DirectoryModel
	{
		public string Name { get; set; }

		public string Path { get; set; }

		public DirectoryModel(string name, string path)
		{
			Name = name;
			Path = path;
		}
	}
}
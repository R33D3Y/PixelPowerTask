using PixelPowerTask.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PixelPowerTask
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		#region Member Variables

		/// <summary>
		/// Log Prefix
		/// </summary>
		private string LogPrefix => nameof(MainWindow);

		/// <summary>
		/// Cancellation token source for asyc operation
		/// </summary>
		public CancellationTokenSource? CancellationTokenSource;

		/// <summary>
		/// Files, folders and drives found
		/// </summary>
		private int items = 0;

		/// <summary>
		/// Files, folders and drives unable to access/read/connect
		/// </summary>
		private int itemsUnavailable = 0;

		#endregion Member Variables

		#region Properties

		/// <summary>
		/// ObservableCollection of DirectoryModel bound to TreeView
		/// </summary>
		public ObservableCollection<DirectoryModel> DriveCollection { get; set; } = new ObservableCollection<DirectoryModel>();

		private bool startButtonEnabled = true;

		/// <summary>
		/// Start button enabled property
		/// </summary>
		public bool StartButtonEnabled
		{
			get => startButtonEnabled;
			set
			{
				if (startButtonEnabled != value)
				{
					startButtonEnabled = value;
					OnPropertyChanged();
				}
			}
		}

		private bool cancelButtonEnabled = false;

		/// <summary>
		/// Cancel button enabled property
		/// </summary>
		public bool CancelButtonEnabled
		{
			get => cancelButtonEnabled;
			set
			{
				if (cancelButtonEnabled != value)
				{
					cancelButtonEnabled = value;
					OnPropertyChanged();
				}
			}
		}

		/// <summary>
		/// Items found property
		/// </summary>
		public string ItemsFound => $"Total items found {items}, items unavailable {itemsUnavailable}.";

		#endregion Properties

		#region Constructor

		/// <summary>
		/// Constructor
		/// </summary>
		public MainWindow()
		{
			InitializeComponent();
		}

		#endregion Constructor

		#region Event Handlers

		/// <summary>
		/// Start button click event handler
		/// </summary>
		private async void StartButton_Click(object sender, RoutedEventArgs e)
		{
			// Show cancel button
			StartButtonEnabled = false;
			CancelButtonEnabled = true;

			// Reset properties
			DriveCollection.Clear();
			CancellationTokenSource = new CancellationTokenSource();
			items = 0;
			itemsUnavailable = 0;

			foreach (DriveInfo drive in DriveInfo.GetDrives())
			{
				try
				{
					items++;
					OnPropertyChanged(nameof(ItemsFound));

					DriveCollection.Add(new FolderModel(drive.Name, drive.Name, await GetDirectoryAsync(CancellationTokenSource, drive.Name)));
				}
				catch (IOException)
				{
					itemsUnavailable++;
					OnPropertyChanged(nameof(ItemsFound));

					Debug.WriteLine($"{LogPrefix} {nameof(StartButton_Click)} Error: Drive unavailable");
				}
			}

			// Show start button
			CancelButtonEnabled = false;
			StartButtonEnabled = true;
		}

		/// <summary>
		/// Cancel button click event handler
		/// </summary>
		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			if (CancellationTokenSource is not null)
			{
				CancellationTokenSource.Cancel();
			}

			// Show start button
			CancelButtonEnabled = false;
			StartButtonEnabled = true;
		}

		#endregion Event Handlers

		#region Helper Methods

		/// <summary>
		/// Get directory recursively async
		/// </summary>
		/// <param name="cancellationTokenSource">Cancellation token source</param>
		/// <param name="path">Path to search</param>
		/// <returns>ObservableCollection of DirectoryModels founds</returns>
		private async Task<ObservableCollection<DirectoryModel>> GetDirectoryAsync(CancellationTokenSource cancellationTokenSource, string path)
		{
			// Operation cancelled
			if (cancellationTokenSource.IsCancellationRequested)
			{
				Debug.WriteLine($"{LogPrefix} {nameof(GetDirectoryAsync)}: Operation cancelled before iteration");
				return new ObservableCollection<DirectoryModel>();
			}

			ObservableCollection<DirectoryModel> directoryModelCollection = new ObservableCollection<DirectoryModel>();

			await Task.Run(async () =>
			{
				try
				{
					foreach (string folder in Directory.GetDirectories($"{path}"))
					{
						// Operation cancelled
						if (cancellationTokenSource.IsCancellationRequested)
						{
							Debug.WriteLine($"{LogPrefix} {nameof(GetDirectoryAsync)}: Operation cancelled at {nameof(Directory.GetDirectories)}");
							return;
						}

						items++;
						OnPropertyChanged(nameof(ItemsFound));

						directoryModelCollection.Add(new FolderModel(GetName(folder), folder, await GetDirectoryAsync(cancellationTokenSource, folder)));
					}

					foreach (string file in Directory.GetFiles($"{path}"))
					{
						// Operation cancelled
						if (cancellationTokenSource.IsCancellationRequested)
						{
							Debug.WriteLine($"{LogPrefix} {nameof(GetDirectoryAsync)}: Operation cancelled at {nameof(Directory.GetFiles)}");
							return;
						}

						items++;
						OnPropertyChanged(nameof(ItemsFound));

						directoryModelCollection.Add(new FileModel(GetName(file), file));
					}
				}
				catch (DirectoryNotFoundException)
				{
					itemsUnavailable++;
					OnPropertyChanged(nameof(ItemsFound));

					Debug.WriteLine($"{LogPrefix} {nameof(GetDirectoryAsync)} Error: Suspected folder shortcut");
				}
				catch (UnauthorizedAccessException)
				{
					itemsUnavailable++;
					OnPropertyChanged(nameof(ItemsFound));

					Debug.WriteLine($"{LogPrefix} {nameof(GetDirectoryAsync)} Error: Permission denied");
				}
			});

			return directoryModelCollection;
		}

		/// <summary>
		/// Gets name of path
		/// </summary>
		/// <param name="path">Path to get name from</param>
		/// <returns>Name of file/folder</returns>
		private static string GetName(string path) => path.Split('\\').Last();

		#endregion Helper Methods

		#region INotifyPropertyChanged

		/// <summary>
		/// Property changed event handler
		/// </summary>
		public event PropertyChangedEventHandler? PropertyChanged;

		/// <summary>
		/// The CallerMemberName attribute that is applied to the optional propertyName parameter causes the property name of the caller to be substituted as an argument
		/// </summary>
		/// <param name="propertyName"></param>
		private void OnPropertyChanged([CallerMemberName] string propertyName = "")
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion INotifyPropertyChanged
	}
}
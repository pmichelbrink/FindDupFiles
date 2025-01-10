using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FindDupFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ObservableCollection<MatchingFile> MatchingFiles { get; set; } = new ObservableCollection<MatchingFile>();
        private List<string> _files = new List<string>();
        private int _numMatchingStartingChars = 10;
        public MainWindow()
        {
            InitializeComponent();
            txtNumMatchingStartingChars.Text = _numMatchingStartingChars.ToString();
        }
        private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            var item = sender as ListViewItem;
            if (item != null && item.IsSelected)
            {
                MatchingFile matchingFile = item.DataContext as MatchingFile;
                string argument = "/select, \"" + matchingFile.Path + "\"";

                System.Diagnostics.Process.Start("explorer.exe", argument);
            }
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            _files.Clear();
            MatchingFiles.Clear();

            if (!Int32.TryParse(txtNumMatchingStartingChars.Text, out _numMatchingStartingChars))
            {
                MessageBox.Show("Enter a valid Number of Matching Starting Characters.");
                return;
            }

            FindAllFiles(txtRootFolderPath.Text);
            FindDupFiles(txtRootFolderPath.Text);
        }

        private void FindDupFiles(string path)
        {
            var matches = _files.GroupBy(x => Path.GetFileName(x).Substring(0, _numMatchingStartingChars))
                  .Where(x => x.Count() > 1)
                  .OrderByDescending(x => x.Count());

            if (!matches.Any())
                return;
   //.First();

            var test = _files.GroupBy(x => Path.GetFileName(x).Substring(0, _numMatchingStartingChars)).OrderByDescending(x => x.Count()).ToList();
            foreach (var match in matches)
            {
                foreach (var item in match.ToList())
                {
                    FileInfo fileInfo = new FileInfo(item);
                    if (!Directory.Exists(item))
                    {
                        MatchingFiles.Add(new MatchingFile() { Path = fileInfo.FullName, Size = ConvertBytesToMegabytes(fileInfo.Length) });
                    }
                }
            }
        }
        private void FindAllFiles(string path)
        {
            Directory.GetFiles(path).ToList().ForEach(file =>
            {
                FileAttributes attr = File.GetAttributes(file);

                if (Path.GetFileName(file).Length <= _numMatchingStartingChars)
                    return;
                    
                _files.Add(file);
            });

            Directory.GetDirectories(path).ToList().ForEach(dir => FindAllFiles(dir));
        }
        static string ConvertBytesToMegabytes(long bytes)
        {
            return ((bytes / 1024f) / 1024f).ToString("0.00");
        }
    }
    public class MatchingFile
    {
        public string Path { get; set; }
        public string Size { get; set; }
    }
}
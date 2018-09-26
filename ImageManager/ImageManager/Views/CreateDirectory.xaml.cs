using FileManagerLib.File.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ImageManager.Views
{
    /// <summary>
    /// CreateDirectory.xaml の相互作用ロジック
    /// </summary>
    public partial class CreateDirectory : Window
    {

        public CreateDirectory(string currentDirectory, JsonFileManager fileManager, Action<string> action)
        {
            InitializeComponent();

            var model = new Models.CreateDirectoryModel(currentDirectory, fileManager, action);
            var vm = new ViewModels.CreateDirectoryViewModel(this, model);
            DataContext = vm;
        }
    }
}

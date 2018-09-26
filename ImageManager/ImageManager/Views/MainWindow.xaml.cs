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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace ImageManager.Views
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {

        public static Dispatcher CurrentDispacher
        {
            get;
            private set;
        }

        public MainWindow()
        {
            InitializeComponent();

            MainWindow.CurrentDispacher = this.Dispatcher;

            var model = new Models.MainWindowModel();
            var vm = new ViewModels.MainWindowViewModel(this, model);
            DataContext = vm;
        }
    }
}

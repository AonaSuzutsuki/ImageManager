using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using ImageManager.Models;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ImageManager.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {

        #region Fields
        private readonly MainWindowModel model;
        #endregion

        public MainWindowViewModel(Window view, MainWindowModel model) : base(view, model)
        {

            this.model = model;

            #region Initialize Events
            CreateArchiveBtClicked = new DelegateCommand(CreateArchiveBt_Clicked);
            OpenArchiveBtClicked = new DelegateCommand(OpenArchiveBt_Clicked);
            FileCloseBtClicked = new DelegateCommand(FileCloseBt_Clicked);
            DeleteCacheBtClicked = new DelegateCommand(DeleteCacheBt_Clicked);

            BackBtClicked = new DelegateCommand(BackBt_Clicked);
            ForwardBtClicked = new DelegateCommand(ForwardBt_Clicked);

            AddFileBtClicked = new DelegateCommand(AddFileBt_Clicked);
            AddFilesInDirectoryBtClicked = new DelegateCommand(AddFilesInDirectoryBt_Clicked);
            CreateDirectoryBtClicked = new DelegateCommand(CreateDirectoryBt_Clicked);
            ExtractFilesOnDirClicked = new DelegateCommand(ExtractFilesOnDir_Clicked);

            ListBoxDoubleClicked = new DelegateCommand<FileDirectoryItem>(ListBox_DoubleClicked);
            ListBoxSelectionChanged = new DelegateCommand<FileDirectoryItem>(ListBox_SelectionChanged);
            #endregion

            #region Initialize Properties
            BackBtIsEnabled = model.ToReactivePropertyAsSynchronized(m => m.CanBack);
            ForwardBtIsEnabled = model.ToReactivePropertyAsSynchronized(m => m.CanForward);
            PathText = model.ToReactivePropertyAsSynchronized(m => m.PathText);
            FileDirectoryItems = model.ToReactivePropertyAsSynchronized(m => m.FileDirectoryItems);
            UnderMessageLabelText = model.ToReactivePropertyAsSynchronized(m => m.UnderMessageLabelText);
            #endregion

            DoLoaded();
        }

        #region Properties
        public ReactiveProperty<bool> BackBtIsEnabled { get; set; }
        public ReactiveProperty<bool> ForwardBtIsEnabled { get; set; }
        public ReactiveProperty<string> PathText { get; set; }

        public ReactiveProperty<ObservableCollection<FileDirectoryItem>> FileDirectoryItems { get; set; }
        public FileDirectoryItem SelectedItem { get; set; }

        public ReactiveProperty<string> UnderMessageLabelText { get; set; }
        #endregion

        #region Event Properties
        public ICommand CreateArchiveBtClicked { get; set; }
        public ICommand OpenArchiveBtClicked { get; set; }
        public ICommand FileCloseBtClicked { get; set; }
        public ICommand DeleteCacheBtClicked { get; set; }

        public ICommand BackBtClicked { get; set; }
        public ICommand ForwardBtClicked { get; set; }

        public ICommand AddFileBtClicked { get; set; }
        public ICommand AddFilesInDirectoryBtClicked { get; set; }
        public ICommand CreateDirectoryBtClicked { get; set; }
        public ICommand ExtractFilesOnDirClicked { get; set; }

        public ICommand ListBoxDoubleClicked { get; set; }
        public ICommand ListBoxSelectionChanged { get; set; }
        #endregion

        #region Event Methods
        protected override void MainWindow_Loaded()
        {
        }
        protected override void MainWindow_Closing()
        {
            model.Dispose();
        }

        public void CreateArchiveBt_Clicked()
        {
            model.MakeNewArchive();
        }
        public void OpenArchiveBt_Clicked()
        {
            model.OpenArchive();
        }
        public void FileCloseBt_Clicked()
        {
            model.Close();
        }
        private void DeleteCacheBt_Clicked()
        {
            model.RemakeThumbnail();
        }

        public void BackBt_Clicked()
        {
            model.BackDirectory();
        }
        public void ForwardBt_Clicked()
        {
            model.ForwardDirectory();
        }

        public void AddFileBt_Clicked()
        {
            model.AddFile();
        }
        public void AddFilesInDirectoryBt_Clicked()
        {
            model.AddFilesInDirectory();
        }
        public void CreateDirectoryBt_Clicked()
        {
            model.MakeDirectory();
        }
        public void ExtractFilesOnDir_Clicked()
        {
            model.ExtractFilesOnDirectory();
        }

        public void ListBox_DoubleClicked(FileDirectoryItem arg)
        {
            if (arg == null)
                return;

            if (arg.IsDirectory)
            {
                model.MoveDirectory(arg.Text);
            }
        }
        public void ListBox_SelectionChanged(FileDirectoryItem arg)
        {
            if (arg == null)
                return;

            Console.WriteLine(arg);
        }
        #endregion
    }
}

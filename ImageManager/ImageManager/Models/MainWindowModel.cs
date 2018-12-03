using CommonExtensionLib.Extensions;
using CommonStyleLib.File;
using CommonStyleLib.Models;
using FileManagerLib.File;
using FileManagerLib.File.Json;
using FileManagerLib.MimeType;
using FileManagerLib.Path;
using ImageManager.Thumbnail;
using ImageManager.Views;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace ImageManager.Models
{
    public class MainWindowModel : ModelBase, IDisposable
    {
        #region Fields
        private JsonFileManager fileManager;
        private ThumbnailManager thumbnailManager;
        private PathItem pathItem;
        private Stack<PathItem> pathItemsForForward;

        private bool isBusy = false;
        private bool isOpened = false;
        private bool isOpenedAndFileSelected = false;
        private bool isCancelRequested = false;
        private BoolCollector boolCollector;

        private ObservableCollection<FileDirectoryItem> fileDirectoryItems = new ObservableCollection<FileDirectoryItem>();
        private string pathText;
        private string underMessageLabelText;
        
        private bool canBack = false;
        private bool canForward = false;
        #endregion

        #region Properties
        public ObservableCollection<FileDirectoryItem> FileDirectoryItems
        {
            get => fileDirectoryItems;
            set => SetProperty(ref fileDirectoryItems, value);
        }
        
        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

        public bool IsOpened
        {
            get => isOpened;
            set => SetProperty(ref isOpened, value);
        }

        public bool IsOpenedAndFileSelected
        {
            get => isOpenedAndFileSelected && isOpened;
            set => SetProperty(ref isOpenedAndFileSelected, value);
        }

        public string PathText
        {
            get => pathText;
            set => SetProperty(ref pathText, value);
        }

        public string UnderMessageLabelText
        {
            get => underMessageLabelText;
            set => SetProperty(ref underMessageLabelText, value);
        }

        public bool CanBack
        {
            get => canBack;
            set => SetProperty(ref canBack, value);
        }
        public bool CanForward
        {
            get => canForward;
            set => SetProperty(ref canForward, value);
        }
        #endregion
        

        public MainWindowModel()
        {
        }


        public void MakeNewArchive()
        {
            var fileName = FileSelector.GetFilePath(CommonCoreLib.AppInfo.GetAppPath(), "DATファイル (*.dat)|*.dat|すべてのファイル(*.*)|*.*", "", FileSelector.FileSelectorType.Write);
            if (fileName != null)
            {
                fileManager = new JsonFileManager(fileName, true, true);
                Initialize();

                UnderMessageLabelText = "{0}を読み込みました。".FormatString(fileName);
            }
        }

        public void OpenArchive()
        {
            var fileName = FileSelector.GetFilePath(CommonCoreLib.AppInfo.GetAppPath(), "DATファイル (*.dat)|*.dat|すべてのファイル(*.*)|*.*", "", FileSelector.FileSelectorType.Read);
            if (fileName != null)
            {
                fileManager = new JsonFileManager(fileName, false, true);
                Initialize();
                DrawItems("/");

                UnderMessageLabelText = "{0}を読み込みました。".FormatString(fileName);
            }
        }

        public void Initialize()
        {
            isCancelRequested = false;
            IsOpened = true;

            thumbnailManager = new ThumbnailManager();
            pathItem = new PathItem();
            pathItemsForForward = new Stack<PathItem>();

            fileManager.VacuumProgress += FileManager_VacuumProgress;
            fileManager.WriteIntoResourceProgress += FileManager_WriteIntoResourceProgress;
        }

        private void FileManager_WriteIntoResourceProgress(object sender, JsonResourceManager.ReadWriteProgressEventArgs eventArgs)
        {
            UnderMessageLabelText = "書き込み中 {0}/{1} ({2}%)".FormatString(eventArgs.CompletedNumber, eventArgs.FullNumber, eventArgs.Percentage);
        }

        public void RemakeThumbnail()
        {
            thumbnailManager?.Dispose();
            var thumbPath = ThumbnailManager.ThumbnailChachePath;
            if (File.Exists(thumbPath))
                File.Delete(thumbPath);
            if (IsOpened)
                thumbnailManager = new ThumbnailManager();
        }

        public void RebuildData()
        {
            IsBusy = true;
            Task.Factory.StartNew(() =>
            {
                fileManager.DataVacuum();
                IsBusy = false;
            });
        }
        

        public void DrawItems(string path)
        {
            var dirs = fileManager.GetDirectories(path);
            var files = fileManager.GetFiles(path);
            
            boolCollector = new BoolCollector();
            var fileDirectoryItems = new ObservableCollection<FileDirectoryItem>();

            Action drawListAct = () =>
            {
                foreach (var dir in dirs)
                {
                    if (isCancelRequested)
                        break;

                    var item = new FileDirectoryItem
                    {
                        Id = dir.Id,
                        IsDirectory = true,
                        Text = dir.Name,
                    };
                    fileDirectoryItems.Add(item);
                }
                foreach (var file in files)
                {
                    if (isCancelRequested)
                        break;

                    string mime = string.Empty;
                    if (file.Additional != null && file.Additional.ContainsKey("MimeType"))
                        mime = file.Additional["MimeType"];

                    var item = new FileDirectoryItem
                    {
                        Id = file.Id,
                        IsDirectory = false,
                        Text = file.Name,
                        Hash = file.Hash,
                        Mimetype = mime,
                    };
                    fileDirectoryItems.Add(item);
                }
                FileDirectoryItems = fileDirectoryItems;
            };
            var task = Task.Factory.StartNew(drawListAct).ContinueWith((t) =>
            {
                boolCollector.ChangeBool(drawListAct, true);
            });

            Action drawImageAct = () =>
            {
                task.Wait();
                foreach (var file in FileDirectoryItems)
                {
                    if (isCancelRequested)
                        break;

                    if (!file.IsDirectory)
                    {
                        if (MimeTypeMap.IsImage(file.Mimetype))
                        {

                            lock (fileManager)
                            {
                                BitmapImage image = null;
                                lock (thumbnailManager)
                                    image = thumbnailManager.GetThumbnail(file.Hash, () => fileManager.GetBytes(file.Id));
                                if (image != null)
                                {
                                    image.Freeze();
                                    file.ImageSource = image;
                                }
                            }
                        }
                    }
                }
                GC.Collect();
            };
            var imageTask = Task.Factory.StartNew(drawImageAct).ContinueWith((t) =>
            {
                boolCollector.ChangeBool(drawImageAct, true);
            });

            boolCollector.ChangeBool(drawListAct, false);
            boolCollector.ChangeBool(drawImageAct, false);

            PathText = path;

            CanBack = pathItem?.GetLast() != null;
            var cnt = pathItemsForForward?.Count;
            CanForward = cnt != null && cnt > 0;
        }

        public void MoveDirectory(string nextPath, bool isFromTextBox = false)
        {
            if (isFromTextBox)
                pathItemsForForward.Push(PathSplitter.SplitPath(pathItem.ToString()));

            pathItem.AddPath(nextPath);
            DrawItems(pathItem.ToString());
        }

        public void BackDirectory()
        {
            if (CanBack)
            {
                pathItemsForForward.Push(PathSplitter.SplitPath(pathItem.ToString()));
                pathItem.Pop();
                DrawItems(pathItem.ToString());
            }
        }

        public void ForwardDirectory()
        {
            if (CanForward)
            {
                var localPathItem = pathItemsForForward?.Pop();
                if (localPathItem != null)
                {
                    pathItem = localPathItem;
                    DrawItems(localPathItem.ToString());
                }
            }
        }

        #region Read
        public void FileDoubleClicked(FileDirectoryItem fileDirectoryItem)
        {
            if (fileDirectoryItem.Mimetype.Equals("text/plain"))
            {
                var bytes = fileManager.GetBytes(fileDirectoryItem.Id);
                Console.WriteLine(Encoding.UTF8.GetString(bytes));
            }
        }
        #endregion

        #region Create
        public void MakeDirectory()
        {
            var currentDirectory = pathItem.ToString();
            var createDirectory = new CreateDirectory(currentDirectory, fileManager, cd => DrawItems(currentDirectory));
            createDirectory.Show();
        }

        public void AddFile()
        {
            var files = FileSelector.GetFilePaths(CommonCoreLib.AppInfo.GetAppPath(), "すべてのファイル(*.*)|*.*", "");
            if (files != null)
            {
                IsBusy = true;
                var currentDirectory = pathItem.ToString();
                Task.Factory.StartNew(() =>
                {
                    fileManager.CreateFiles(currentDirectory, files);
                    MainWindow.CurrentDispacher.Invoke(() => DrawItems(currentDirectory));
                    IsBusy = false;
                });
            }
        }

        public void AddFilesInDirectory()
        {
            var open = new CommonOpenFileDialog()
            {
                IsFolderPicker = true,
                Multiselect = true
            };
            
            if (open.ShowDialog() == CommonFileDialogResult.Ok)
            {
                IsBusy = true;
                var fileNames = open.FileNames;
                var currentDirectory = pathItem.ToString();
                var task = Task.Factory.StartNew(() =>
                {
                    string dirPath = "/";
                    if (!currentDirectory.Equals("/"))
                        dirPath = "{0}/".FormatString(currentDirectory);
                    fileManager.CreateFilesOnDirectories(dirPath, fileNames.ToArray());
                    MainWindow.CurrentDispacher.Invoke(() => DrawItems(currentDirectory));
                    IsBusy = false;
                });
            }
        }
        #endregion

        #region Delete
        public void Delete(List<FileDirectoryItem> fileDirectoryItems)
        {
            foreach (var item in fileDirectoryItems.Select((v, i) => new { i, v }))
            {
                if (item.v.IsDirectory)
                    fileManager.DeleteDirectory(item.v.Id);
                else
                    fileManager.DeleteFile(item.v.Id);
                DeleteProgress(
                    new JsonResourceManager.ReadWriteProgressEventArgs(item.i + 1, fileDirectoryItems.Count, item.v.Text, true)
                );
                FileDirectoryItems.Remove(item.v);
            }
            //var currentDirectory = pathItem.ToString();
            //DrawItems(currentDirectory);
        }
        #endregion

        #region Extract
        public void ExtractSelectedFiles(List<FileDirectoryItem> fileDirectoryItems)
        {
            var open = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };

            if (open.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var dirPath = open.FileName;
                //if (!Directory.Exists(dirPath))
                //    dirPath = Path.GetDirectoryName(dirPath);

                var currentDirectory = pathItem.ToString();
                currentDirectory = currentDirectory.Equals("/") ? "" : currentDirectory;
                if (fileDirectoryItems.Count > 0)
                {

                    Task.Factory.StartNew(() => {
                        var index = 1;
                        var cnt = 0;

                        foreach (var item in fileDirectoryItems)
                        {
                            if (item.IsDirectory)
                                cnt += fileManager.GetAllFilesCount("{0}/{1}".FormatString(currentDirectory, item.Text));
                            else
                                cnt++;
                        }

                        foreach (var item in fileDirectoryItems)
                        {
                            void act(string currentFilePath, bool isComplete)
                            {
                                WriteIntoResourceProgress(
                                    new JsonResourceManager.ReadWriteProgressEventArgs(index, cnt, currentFilePath, isComplete)
                                );
                                index++;
                            }

                            if (item.IsDirectory)
                                fileManager.WriteToDir("{0}/{1}".FormatString(currentDirectory, item.Text), "{0}/{1}".FormatString(dirPath, item.Text), act);
                            else
                                fileManager.WriteToFile("{0}/{1}".FormatString(currentDirectory, item.Text), "{0}/{1}".FormatString(dirPath, item.Text), act);
                        }
                    });
                }
            }
        }
        #endregion


        #region EventMethods
        private void FileManager_VacuumProgress(object sender, JsonResourceManager.ReadWriteProgressEventArgs eventArgs)
        {
            UnderMessageLabelText = "再構成中 {0}/{1} ({2}%)".FormatString(eventArgs.CompletedNumber, eventArgs.FullNumber, eventArgs.Percentage);
        }

        private void DeleteProgress(JsonResourceManager.ReadWriteProgressEventArgs eventArgs)
        {
            UnderMessageLabelText = "削除中 {0}/{1} ({2}%)".FormatString(eventArgs.CompletedNumber, eventArgs.FullNumber, eventArgs.Percentage);
        }
        private void WriteToFilesProgress(JsonResourceManager.ReadWriteProgressEventArgs eventArgs)
        {
            UnderMessageLabelText = "書き込み中 {0}/{1} ({2}%)".FormatString(eventArgs.CompletedNumber, eventArgs.FullNumber, eventArgs.Percentage);
        }
        private void WriteIntoResourceProgress(JsonResourceManager.ReadWriteProgressEventArgs eventArgs)
        {
            UnderMessageLabelText = "読み込み中 {0}/{1} ({2}%)".FormatString(eventArgs.CompletedNumber, eventArgs.FullNumber, eventArgs.Percentage);
        }
        #endregion


        #region Dispose
        public void Close()
        {
            var fileName = fileManager?.FilePath;
            Dispose();
            FileDirectoryItems = new ObservableCollection<FileDirectoryItem>();
            UnderMessageLabelText = "{0}を閉じました。".FormatString(fileName);
        }

        public void Dispose()
        {
            isCancelRequested = true;
            IsOpened = false;

            if (fileManager != null)
            {
                Task.Factory.StartNew(() =>
                {
                    if (boolCollector != null)
                        while (!boolCollector.Value) ;
                    lock (fileManager)
                        fileManager.Dispose();
                    if (thumbnailManager != null)
                        lock (thumbnailManager)
                            thumbnailManager.Dispose();
                });
            }
        }
        #endregion
    }
}

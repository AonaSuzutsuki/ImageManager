using CommonExtensionLib.Extensions;
using CommonStyleLib.ExMessageBox;
using CommonStyleLib.File;
using CommonStyleLib.Models;
using FileManagerLib.Filer.Json;
using FileManagerLib.MimeType;
using FileManagerLib.Path;
using ImageManager.ImageLoader;
using ImageManager.Thumbnail;
using ImageManager.Views;
using Microsoft.WindowsAPICodePack.Dialogs;
using Microsoft.WindowsAPICodePack.Dialogs.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace ImageManager.Models
{
    public class MainWindowModel : ModelBase, IDisposable
    {
        #region Fields
        private readonly Window window;

        private JsonFileManager fileManager;
        private ThumbnailManager thumbnailManager;
        private PathItem pathItem;
        private Stack<PathItem> pathItemsForForward;
        
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
        

        public MainWindowModel(Window window)
        {
            this.window = window;
        }


        public void MakeNewArchive()
        {
            var fileName = FileSelector.GetFilePath(CommonStyleLib.AppInfo.GetAppPath(), "DATファイル (*.dat)|*.dat|すべてのファイル(*.*)|*.*", "", FileSelector.FileSelectorType.Write);
            if (fileName != null)
            {
                fileManager = new JsonFileManager(fileName, true, true);
                Initialize();

                UnderMessageLabelText = "{0}を読み込みました。".FormatString(fileName);
            }
        }

        public void OpenArchive()
        {
            var fileName = FileSelector.GetFilePath(CommonStyleLib.AppInfo.GetAppPath(), "DATファイル (*.dat)|*.dat|すべてのファイル(*.*)|*.*", "", FileSelector.FileSelectorType.Read);
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
            
            thumbnailManager = new ThumbnailManager();
            pathItem = new PathItem();
            pathItemsForForward = new Stack<PathItem>();

            fileManager.WriteToFilesProgress += FileManager_WriteToFilesProgress;
            fileManager.WriteIntoResourceProgress += FileManager_WriteIntoResourceProgress; ;
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
                                //byte[] bytes = fileManager.GetBytes(file.Id);
                                //using (var stream = new MemoryStream(bytes))
                                //{
                                //    var image = ImageConverter.GetBitmapImage(stream);
                                //    image.Freeze();
                                //    if (image != null)
                                //        file.SetImageSourceAndCache(file.Hash, image, window);
                                //}
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


        #region Create
        public void MakeDirectory()
        {
            var currentDirectory = pathItem.ToString();
            var createDirectory = new CreateDirectory(currentDirectory, fileManager, cd => DrawItems(currentDirectory));
            createDirectory.Show();
        }

        public void AddFile()
        {
            var files = FileSelector.GetFilePaths(CommonStyleLib.AppInfo.GetAppPath(), "すべてのファイル(*.*)|*.*", "");

            var currentDirectory = pathItem.ToString();
            foreach (var file in files)
            {
                var filename = Path.GetFileName(file);
                fileManager.CreateFile(filename, currentDirectory, file);
            }
        }

        public void AddFilesInDirectory()
        {
            var open = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };
            
            if (open.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var fileName = open.FileName;

                var currentDirectory = pathItem.ToString();
                var task = Task.Factory.StartNew(() =>
                {
                    fileManager.CreateFiles(currentDirectory, fileName);

                    window.Dispatcher.Invoke(() => DrawItems(currentDirectory));
                });
            }
        }
        #endregion

        #region Extract
        public void ExtractFilesOnDirectory()
        {
            var open = new CommonOpenFileDialog()
            {
                IsFolderPicker = true
            };

            if (open.ShowDialog() == CommonFileDialogResult.Ok)
            {
                var fileName = open.FileName;

                var currentDirectory = pathItem.ToString();
                Task.Factory.StartNew(() => fileManager.WriteToDir(currentDirectory, fileName));
            }
        }
        #endregion


        #region EventMethods
        private void FileManager_WriteToFilesProgress(object sender, AbstractJsonResourceManager.ReadWriteProgressEventArgs eventArgs)
        {
            UnderMessageLabelText = "書き込み中 {0}/{1} ({2}%)".FormatString(eventArgs.CompletedNumber, eventArgs.FullNumber, eventArgs.Percentage);
        }

        private void FileManager_WriteIntoResourceProgress(object sender, AbstractJsonResourceManager.ReadWriteProgressEventArgs eventArgs)
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

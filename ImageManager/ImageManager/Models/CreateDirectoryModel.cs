using CommonExtensionLib.Extensions;
using CommonStyleLib.Models;
using FileManagerLib.File.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Models
{
    public class CreateDirectoryModel : ModelBase
    {

        #region Fields
        private string directoryNameText = string.Empty;
        #endregion

        #region Properties
        public JsonFileManager FileManager { get; }
        public string CurrentDirectory { get; }
        public Action<string> Action { get; }
        #endregion


        public CreateDirectoryModel(string currentDirectory, JsonFileManager fileManager, Action<string> action)
        {
            FileManager = fileManager;
            CurrentDirectory = currentDirectory;
            Action = action;
        }


        public void CreateDirectory(string directoryName)
        {
            var fullPath = "{0}/{1}".FormatString(CurrentDirectory, directoryName);
            FileManager.CreateDirectory(fullPath);
            Action?.Invoke(CurrentDirectory);
        }
    }
}

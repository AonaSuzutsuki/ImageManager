using CommonStyleLib.Models;
using CommonStyleLib.ViewModels;
using ImageManager.Models;
using Prism.Commands;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ImageManager.ViewModels
{
    public class CreateDirectoryViewModel : ViewModelBase
    {

        #region Fields
        private readonly CreateDirectoryModel model;
        #endregion

        public CreateDirectoryViewModel(Window view, CreateDirectoryModel model) : base(view, model)
        {

            #region Initialize Properties
            #endregion

            #region Initialize Event
            OkBtClicked = new DelegateCommand(OkBt_Clicked);
            #endregion

            this.model = model;
        }

        #region Properties
        public string DirectoryNameText { get; set; }
        #endregion

        #region Event Properties
        public ICommand OkBtClicked { get; set; }
        #endregion

        #region Event Methods
        public void OkBt_Clicked()
        {
            model.CreateDirectory(DirectoryNameText);
            view.Close();
        }
        #endregion
    }
}

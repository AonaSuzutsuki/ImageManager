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
    public class MainWindowViewModel : ViewModelBase
    {

        #region Fields
        private readonly MainWindowModel model;
        #endregion

        public MainWindowViewModel(Window view, MainWindowModel model) : base(view, model)
        {

            this.model = model;

            #region Initialize Events
            TestClicked = new DelegateCommand(Test_Clicked);
            #endregion

            #region Initialize Properties
            TestText = model.ToReactivePropertyAsSynchronized(m => m.TestText);
            #endregion

            DoLoaded();
        }

        #region Properties
        public ReactiveProperty<string> TestText { get; set; }
        #endregion

        #region Event Properties
        public ICommand TestClicked { get; set; }
        #endregion

        #region Event Methods
        private void Test_Clicked()
        {
            model.TestClicked();
        }
        #endregion
    }
}

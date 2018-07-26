using CommonStyleLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageManager.Models
{
    public class MainWindowModel : ModelBase
    {
        #region Fields
        private string testText;
        #endregion

        #region Properties
        public string TestText
        {
            get => testText;
            set => SetProperty(ref testText, value);
        }
        #endregion


        public void TestClicked()
        {
            TestText = "Test";
        }
    }
}

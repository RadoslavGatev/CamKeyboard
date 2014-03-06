using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CamKeyboard.UI.ViewModel
{
    abstract class BaseViewModel : INotifyPropertyChanged
    {
        private string displayName;

        public BaseViewModel(string displayName)
        {
            this.DisplayName = displayName;
        }

        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
            protected set
            {
                this.displayName = value;
                this.OnPropertyChanged("DisplayName");
            }
        }

        protected void OnPropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                var propertyChangedEventArgs = new PropertyChangedEventArgs(propertyName);
                this.PropertyChanged(this, propertyChangedEventArgs);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}

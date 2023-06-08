using System.ComponentModel;
using System.Runtime.CompilerServices;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.ViewModel
{
	public class BaseViewModel : INotifyPropertyChanged
	{
		public Settings Settings => Settings.Current;

        private bool _isBusy;
		public bool IsBusy
		{
			get => _isBusy;
            set
			{
				_isBusy = value;
				OnPropertyChanged();
				OnPropertyChanged("SectionEnabled");
			}
		}

		public bool SectionEnabled => !IsBusy;


        #region INotifyPropertyChanged implementation

		public event PropertyChangedEventHandler PropertyChanged;

		public void OnPropertyChanged([CallerMemberName]string name = "")
		{
			var changed = PropertyChanged;
			if (changed == null)
				return;
			changed(this, new PropertyChangedEventArgs(name));
		}

		#endregion
	}
}


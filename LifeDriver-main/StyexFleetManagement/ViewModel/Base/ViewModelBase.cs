using System.Threading.Tasks;

namespace StyexFleetManagement.ViewModel.Base
{
    public abstract class ViewModelBase : ExtendedBindableObject
    {
        private bool _isBusy;

        public bool IsBusy
        {
            get => _isBusy;

            set
            {
                _isBusy = value;
                RaisePropertyChanged(() => IsBusy);
            }
        }

        public virtual Task InitializeAsync(object navigationData)
        {
            return Task.FromResult(false);
        }

        /// <summary>
        /// Called when the view model is disappearing. View Model clean-up should be performed here.
        /// </summary>
        public virtual void OnDisappearing()
        { }
    }
}

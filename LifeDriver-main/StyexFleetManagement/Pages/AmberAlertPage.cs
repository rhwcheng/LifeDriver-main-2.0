using StyexFleetManagement.Resx;
using StyexFleetManagement.ViewModel;
using StyexFleetManagement.ViewModel.Base;

namespace StyexFleetManagement.Pages
{
    public class AmberAlertPage : CountdownTimerPage
    {
        public override ICountdownTimerViewModel ViewModel => ViewModelLocator.Resolve<IAmberAlertViewModel>();

        public AmberAlertPage()
        {
            Title = AppResources.amber_alert_title;
        }
    }
}
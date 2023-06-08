using StyexFleetManagement.Enums;

namespace StyexFleetManagement.ViewModel
{
    public class AmberAlertViewModel : CountdownTimerViewModel, IAmberAlertViewModel
    {
        public AmberAlertViewModel()
        {
            TimerType = TimerType.AmberAlert;
        }
    }
}
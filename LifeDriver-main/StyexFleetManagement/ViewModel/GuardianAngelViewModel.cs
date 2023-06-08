using StyexFleetManagement.Enums;

namespace StyexFleetManagement.ViewModel
{
    public class GuardianAngelViewModel : CountdownTimerViewModel, IGuardianAngelViewModel
    {
        public GuardianAngelViewModel()
        {
            TimerType = TimerType.GuardianAngel;
        }
    }
}
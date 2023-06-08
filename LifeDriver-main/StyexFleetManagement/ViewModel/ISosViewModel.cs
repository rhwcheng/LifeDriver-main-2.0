using System;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.ViewModel
{
    public interface ISosViewModel
    {
        Command DriverIdPreviewCommand { get; set; }
        Command SosCommand { get; set; }
        Command CovidHotlineCommand { get; set; }
        Command CaptureDriverIdCommand { get; }
        ImageSource DriverIdImageSource { get; set; }
        Task CheckIn_OnClicked(object sender, EventArgs e);
        Task CheckOut_OnClicked(object sender, EventArgs e);
        Task DriverId_OnClicked(object sender, EventArgs e);
        //Task DriverId_OnClicked(object sender, EventArgs e);
    }
}
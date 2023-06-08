using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Shiny;
using Shiny.BluetoothLE;
using Xamarin.Forms;

namespace StyexFleetManagement
{
    public class Startup : ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.UseBleClient();
        }
    }
    public class BleClientDelegate : BleDelegate, IShinyStartupTask
    {


        public override Task OnAdapterStateChanged(AccessState state)
        {
            if (state == AccessState.Disabled)
                MessagingCenter.Send(this.GetType(), "BLE State", "Turn on Bluetooth");
            return Task.CompletedTask;
        }


        public override Task OnConnected(IPeripheral peripheral)
        {
            MessagingCenter.Send(this.GetType(), "BluetoothLE Device Connected", $"{peripheral.Name} has connected");
            return Task.CompletedTask;
        }


        //public override Task OnScanResult(ScanResult result)
        //{
        //    // we only want this to run in the background
        //    return base.OnScanResult(result);
        //}


        public void Start()
        {

        }
    }
}
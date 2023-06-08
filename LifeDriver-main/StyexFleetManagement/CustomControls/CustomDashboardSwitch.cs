using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Runtime.CompilerServices;
using StyexFleetManagement.Models;

namespace StyexFleetManagement.CustomControls
{
    public class CustomDashboardSwitch : Switch
    {
        public DashboardReport Report { get; set; }
    }
}

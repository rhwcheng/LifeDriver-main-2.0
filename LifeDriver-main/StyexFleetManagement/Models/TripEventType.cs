using System;
using System.Collections.Generic;
using System.Text;

namespace StyexFleetManagement.Models
{
    public enum TripEventType
    {
        PeriodicPosition = 1,
        PolledPosition = 2,
        Tachograph = 9,
        TripStart = 122,
        TripEnd = 123,
        DriverRegistered = 220,
        TripSummary = 232
    }
}

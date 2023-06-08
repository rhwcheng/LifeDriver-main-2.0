namespace StyexFleetManagement.Models
{
    public enum EventType
    {
        DEVICE_UNPLUGGED = 245,
        IDLE = 52,
        CONSOLIDATED_FUEL_DATA = 242,
        TRIP_SHUTDOWN = 123,
        TRIP_STARTUP = 122,
        MAINS_LOW = 215,
        FLEET_EXCEPTION = 0,
        UBI_EXCEPTION = 1,
        FuelTheft = 239,
        Fuel = 172,
        //PeriodicPosition = 1, TO FIX: SAME AS UBI EXVEPTION?
        PolledPosition = 2,
        Tachograph = 9,
        DriverRegistered = 220,
        TripSummary = 232
    }
}

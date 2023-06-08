using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StyexFleetManagement.Abstractions
{
    public interface IStoreManager
    {
        bool IsInitialized { get; }
        ITripStore TripStore { get; }
        //IPhotoStore PhotoStore { get; }
        //IUserStore UserStore { get; }
        //IHubIOTStore IOTHubStore { get; }
        //IPOIStore POIStore { get; }
        //ITripPointStore TripPointStore { get; }
        Task<bool> SyncAllAsync(bool syncUserSpecific);
        Task DropEverythingAsync();
        Task InitializeAsync();
    }
}

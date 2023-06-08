using Firebase.Database;
using Firebase.Database.Query;
using StyexFleetManagement.Salus.Models;
using System.Threading.Tasks;

namespace StyexFleetManagement.Salus.Services
{
    public class CredentialManagementService
    {
        private const string ChildName = "Credentials";

        private readonly FirebaseClient _firebase = new FirebaseClient("https://styex-fleet-management.firebaseio.com/");

        public async Task Add(CredentialUpdateRequest request)
        {
            await _firebase
                .Child(ChildName)
                .PostAsync(request);
        }
    }
}
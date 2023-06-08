using System;

namespace StyexFleetManagement.Salus.Models
{
    public class CredentialUpdateRequest
    {
        public Guid CredentialId { get; set; } = Guid.NewGuid();
        public string Username { get; set; }
        public string Password { get; set; }
        public CredentialProvider CredentialProvider { get; set; }
    }
}
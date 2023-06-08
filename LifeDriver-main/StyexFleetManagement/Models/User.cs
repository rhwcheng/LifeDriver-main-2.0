using System;
using Newtonsoft.Json;

namespace StyexFleetManagement.Models
{
	public class User
	{
        [JsonProperty("UtcLastModified")]
		public string UtcLastModified { get; set; }


		[JsonProperty("Id")]
		public Guid Id{ get; set; }


		[JsonProperty("Description")]
		public string Description { get; set; }



		[JsonProperty("Email")]
		public string Email { get; set; }

	
		[JsonProperty("FirstName")]
		public string FirstName { get; set; }



		[JsonProperty("LastName")]
		public string LastName { get; set; }


	}
}


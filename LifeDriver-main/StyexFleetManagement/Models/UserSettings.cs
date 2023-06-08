using System;

namespace StyexFleetManagement.Models
{
	public class UserSettings
	{
		public Guid Id { get; set;}
		public string Description { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Email { get; set; }
		public string UserName { get; set; }
		public string PhoneHome { get; set; }
		public string PhoneMobile { get; set; }
		public string PhoneOffice { get; set; }
		public string PasswordHint { get; set; }
		public int UtcOffset { get; set; }
		public string LanguageCode { get; set; }
		public Guid UnitOfMeasureDistanceId { get; set; }
		public Guid UnitOfMeasureFluidId { get; set; }
		public int UtcDaylightSavingOffset { get; set; }
		public DateTime UtcDaylightSavingStart { get; set; }
		public DateTime UtcDaylightSavingEnd { get; set; }
		public Guid SecurityGroupId { get; set; }
		public string CurrencyCode { get; set; }
		public string CurrencySymbol { get; set; }
	}
}


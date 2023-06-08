using System;
using StyexFleetManagement.Resx;

namespace StyexFleetManagement.Models
{
	public class FluidMeasurementUnit
	{
		private const string LITRE_ID = "01F4A7F9-55B1-4B6D-A65A-EB85F48FDFB8";
		private const string US_GALLON_ID = "4CA70ADB-882B-4607-942A-CAF7AD229557";
		private const string IMPERIAL_GALLON_ID = "5DB2BBB6-7805-4522-BF49-E0DEC4667E2A";

		private string id;

		public FluidMeasurementUnit(string id)
		{
			this.id = id;
		}

		public static string GetAbbreviation(Guid _id)
		{
			string id = _id.ToString();
			if (id.ToUpperInvariant().Equals(LITRE_ID))
			{
				return AppResources.volume_litres_abbr;
			}
			if (id.ToUpperInvariant().Equals(US_GALLON_ID))
			{
				return AppResources.volume_us_gallons_abbr;
			}
			if (id.ToUpperInvariant().Equals(IMPERIAL_GALLON_ID))
			{
				return AppResources.volume_imperial_gallons_abbr;
			}
			return string.Empty;
		}

		public static string GetDescription(Guid _id)
		{
			string id = _id.ToString();

			if (id.ToUpperInvariant().Equals(LITRE_ID))
			{
				return AppResources.volume_litres;
			}
			if (id.ToUpperInvariant().Equals(US_GALLON_ID))
			{
				return AppResources.volume_us_gallons;
			}
			if (id.ToUpperInvariant().Equals(IMPERIAL_GALLON_ID))
			{
				return AppResources.volume_imperial_gallons;
			}
			return string.Empty;
		}
	}
}


using System;
using StyexFleetManagement.Resx;

namespace StyexFleetManagement.Models
{
	public class DistanceMeasurementUnit
	{
		private const string KILOMETRE_ID = "A54104B8-2A37-471F-92DB-3F98C60B966D";
		private const string INTERNATIONAL_MILE_ID = "68672268-8C6F-4C53-896F-2D0B6A253C83";
		private const string NAUTICAL_MILE_ID = "B4DB5CF8-3648-47F3-8D23-9837270B2FAC";

		private string id;

		public DistanceMeasurementUnit(string id)
		{
			this.id = id;
		}

		public bool equals(string unit)
		{
			return id.Equals(unit);
		}

		public static string GetAbbreviation(Guid _id)
		{
			string id = _id.ToString();

			if (id.ToUpperInvariant().Equals(KILOMETRE_ID))
			{
				return AppResources.distance_km_abbr;
			}
			if (id.ToUpperInvariant().Equals(INTERNATIONAL_MILE_ID))
			{
				return AppResources.distance_miles_abbr;
			}
			if (id.ToUpperInvariant().Equals(NAUTICAL_MILE_ID))
			{
				return AppResources.distance_nautical_miles_abbr;
			}
			return string.Empty;
		}

		public static string GetDescription(Guid _id)
		{
			string id = _id.ToString();

			if (id.ToUpperInvariant().Equals(KILOMETRE_ID))
			{
				return AppResources.distance_km;
			}
			if (id.ToUpperInvariant().Equals(INTERNATIONAL_MILE_ID))
			{
				return AppResources.distance_miles;
			}
			if (id.ToUpperInvariant().Equals(NAUTICAL_MILE_ID))
			{
				return AppResources.distance_nautical_miles;
			}
			return string.Empty;
		}

		public static string GetSpeedAbbreviation(Guid _id)
		{
			string id = _id.ToString();

			if (id.ToUpperInvariant().Equals(KILOMETRE_ID))
			{
				return AppResources.speed_km_h_abbr;
			}
			if (id.ToUpperInvariant().Equals(INTERNATIONAL_MILE_ID))
			{
				return AppResources.speed_mph_abbr;
			}
			if (id.ToUpperInvariant().Equals(NAUTICAL_MILE_ID))
			{
				return AppResources.speed_mph_abbr;
			}
			return string.Empty;
		}
	}
}


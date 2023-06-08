using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace StyexFleetManagement.Models
{
	public class LastKnownPositions
	{
		public int Page { get; set; }
		public int PageSize { get; set; }
		public int TotalResults { get; set; }
		public List<UnitLocation> Items { get; set; }
		public bool HasMoreResults { get; set; }
    }

	public class UnitLocation
	{
		public string LocalTimestamp { get; set; }
		public Guid Id { get; set; }
		public string Description { get; set; }
		public string UnitId { get; set; }
		public List<float> Position { get; set; }
		public string Location { get; set; }

        public string DateAsString => GetLocalTimestamp().ToString("dd MMMMM yyyy HH:mm tt");

        public DateTime GetLocalTimestamp()
		{
			string dateString = LocalTimestamp;
			if (dateString.Length > 19)
			{
				dateString = dateString.Substring(0, 19);
			}

			CultureInfo provider = CultureInfo.InvariantCulture;

			return DateTime.ParseExact(dateString, "yyyy-MM-dd'T'HH:mm:ss", provider);

		}

		public DateTime GetUtcTimestamp()
		{
			DateTime timestamp = GetLocalTimestamp();
			timestamp = timestamp.ToUniversalTime();


			string pattern = "([\\+-])(\\d+):(\\d+)";

			var r = new Regex(pattern);

			Match m = r.Match(LocalTimestamp);

			//Matcher m = p.matcher(LocalTimestamp);
			if (m.Success)
			{
				if (m.Groups[1].Equals("+"))
				{
					timestamp = timestamp.AddHours(-1*(int.Parse(m.Groups[2].ToString())));
					//timestamp = timestamp.minusHours(Integer.parseInt(m.group(2)));

					timestamp = timestamp.AddMinutes(-1*(int.Parse(m.Groups[3].ToString())));

					//timestamp = timestamp.minusMinutes(Integer.parseInt(m.group(3)));
				}
				else if (m.Groups[1].Equals("-"))
				{

					timestamp = timestamp.AddHours(int.Parse(m.Groups[2].ToString()));


					timestamp = timestamp.AddMinutes(int.Parse(m.Groups[3].ToString()));
					//timestamp = timestamp.plusHours(Integer.parseInt(m.group(2)));
					//timestamp = timestamp.plusMinutes(Integer.parseInt(m.group(3)));
				}
			}
			return timestamp;
		}
	}
}


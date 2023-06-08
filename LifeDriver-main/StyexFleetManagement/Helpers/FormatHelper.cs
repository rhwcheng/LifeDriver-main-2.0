using System;
using StyexFleetManagement.Models;
using StyexFleetManagement.Resx;
using Xamarin.Forms;

namespace StyexFleetManagement.Helpers
{
	public static class FormatHelper
	{
		public static FormattedString FormatVolume(string volume)
		{
			var fs = new FormattedString();

			fs.Spans.Add(new Span { Text = volume, FontSize = 16, FontAttributes = FontAttributes.Bold });
			fs.Spans.Add(new Span { Text = " " });

			fs.Spans.Add(new Span { Text = App.GetFluidAbbreviation(), FontSize = 11 });

			return fs;
		}

		internal static FormattedString FormatDistance(string distance, int fontSize = 16)
		{
			var fs = new FormattedString();

			fs.Spans.Add(new Span { Text = distance, FontSize = fontSize, FontAttributes = FontAttributes.Bold });
			fs.Spans.Add(new Span { Text = " "});
			fs.Spans.Add(new Span { Text = App.GetDistanceAbbreviation(), FontSize = 11 });

			return fs;
		}

		internal static FormattedString FormatConsumption(string consumption)
		{
			var fs = new FormattedString();

			fs.Spans.Add(new Span { Text = consumption, FontSize = 16, FontAttributes = FontAttributes.Bold });
			fs.Spans.Add(new Span { Text = " " });
			fs.Spans.Add(new Span { Text = App.GetConsumptionAbbreviation(), FontSize = 11 });

			return fs;
		}

		internal static FormattedString FormatCost(string cost)
		{
			var fs = new FormattedString();

			fs.Spans.Add(new Span { Text = cost, FontSize = 16, FontAttributes = FontAttributes.Bold });
			fs.Spans.Add(new Span { Text = " " });
			fs.Spans.Add(new Span { Text = Settings.Current.Currency, FontSize = 11 });

			return fs;
		}

		internal static FormattedString FormatSpeed(string speed)
		{
			var fs = new FormattedString();

			fs.Spans.Add(new Span { Text = speed, FontSize = 16, FontAttributes = FontAttributes.Bold });
			fs.Spans.Add(new Span { Text = " " });
			fs.Spans.Add(new Span { Text = App.GetDistanceAbbreviation() + "/h", FontSize = 11 });

			return fs;
		}
        public static string ToShortForm(TimeSpan t)
        {
            string shortForm = "";
            if (t.Hours > 0)
            {
                shortForm += $"{t.Hours.ToString()}h";
            }
            if (t.Minutes > 0)
            {
                shortForm += $"{t.Minutes.ToString()}m";
            }
            if (t.Seconds > 0 || string.IsNullOrEmpty(shortForm))
            {
                shortForm += $"{t.Seconds.ToString()}s";
            }
            return shortForm;
        }

        internal static FormattedString FormatTime(string time, int fontSize = 16)
        {
            var fs = new FormattedString();

            fs.Spans.Add(new Span { Text = time, FontSize = fontSize, FontAttributes = FontAttributes.Bold });
            fs.Spans.Add(new Span { Text = " " });
            fs.Spans.Add(new Span { Text = "min", FontSize = 11 });

            return fs;
        }

        internal static FormattedString FormatAlerts(string alerts, int fontSize = 16)
        {
            var fs = new FormattedString();

            fs.Spans.Add(new Span { Text = alerts, FontSize = fontSize, FontAttributes = FontAttributes.Bold });
            fs.Spans.Add(new Span { Text = " " });
            fs.Spans.Add(new Span { Text = AppResources.alerts, FontSize = 11 });

            return fs;
        }
    }
}


using System;
using System.Globalization;
using StyexFleetManagement.Models;
using Xamarin.Forms;

namespace StyexFleetManagement.Services
{
	public class DateHelper


	{
		/*
	 * Formats date and time to user preferred format If date is today, returns only time
	 */
		public static string dateTimeToString(DateTime dateTime)
		{
			if (dateTime.ToLocalTime().Equals(DateTime.Now.ToLocalTime()))
			{
				// Today - show only time
				return dateTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
			}
			else if (dateTime.AddDays(1).ToLocalTime().Equals(DateTime.Now.ToLocalTime()))
			{
				// Yesterday - show "Yesterday" and time
				return "Yesterday" + " "
					+ dateTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern);
			}
			else
			{
				// Before yesterday - show only date
				return dateTime.ToString(CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
			}
		}

		public static string durationToString(long seconds)
		{
			TimeSpan span = TimeSpan.FromSeconds(seconds);
			return $"{(int) span.TotalHours}h{span.Minutes}min";
		}

		public static DateTime GetDateRangeStartDate(ReportDateRange dateRange)
		{
			DateTime today = DateTime.Now.ToLocalTime().Date;//.toDateTimeAtStartOfDay();
			switch (dateRange)
			{
                case ReportDateRange.TODAY:
                    return DateTime.Today.ToLocalTime();
				case ReportDateRange.THIS_MONTH:
					return today.AddDays(-1 * (today.Day - 1));
				case ReportDateRange.PREVIOUS_MONTH:
					return today.AddDays(-1 * (today.Day - 1)).AddMonths(-1);
                case ReportDateRange.LAST_SEVEN_DAYS:
                    return today.AddDays(-7);

                default:
					return DateTime.MinValue;
			}
		}

		public static DateTime GetDateRangeEndDate(ReportDateRange dateRange)
		{
			DateTime todayEndTime = DateTime.Now.Date.AddDays(1).ToLocalTime().AddTicks(-1);

			switch (dateRange)
			{
                case ReportDateRange.TODAY:
                    return todayEndTime;
				case ReportDateRange.THIS_MONTH:
					return todayEndTime;
				case ReportDateRange.PREVIOUS_MONTH:
					return todayEndTime.AddDays(-1 * (todayEndTime.Day));
                case ReportDateRange.LAST_SEVEN_DAYS:
                    return todayEndTime;
                default:
					return DateTime.MinValue;
			}

		}

		public static FormattedString FormatTime(int seconds)
		{
			TimeSpan span = TimeSpan.FromSeconds(seconds);
			var fs = new FormattedString();
			var hours = (int)span.TotalHours;
			var minutes = (int)span.Minutes;
			if (hours != 0)
			{
				fs.Spans.Add(new Span { Text = hours.ToString(), FontSize = 16, FontAttributes = FontAttributes.Bold });
				fs.Spans.Add(new Span { Text = "h", FontSize = 11 });
			}
			fs.Spans.Add(new Span { Text = minutes.ToString(), FontSize = 16, FontAttributes = FontAttributes.Bold });
			fs.Spans.Add(new Span { Text = "min", FontSize = 11 });

			return fs;
		}

		public static DateTime GetGroupByPeriodStartDate(ReportDateRange date, DateTime startDate, DateTime endDate)
		{
			switch (GetGroupByPeriod(date))
			{
				case GroupByPeriod.byday:
					if (date == ReportDateRange.CUSTOM)
					{
						DateTime thirtyDaysAgo = endDate.AddDays(-29).Date;
						if (startDate > thirtyDaysAgo)
						{
							return startDate;
						}
						return thirtyDaysAgo;
					}
					return endDate.AddDays(-29).Date;
				case GroupByPeriod.byweek:
					return endDate.AddDays(-8).AddDays(1).Date;
				case GroupByPeriod.bymonth:
					return endDate.AddDays(-(endDate.Day - 1)).AddDays(-11).Date;
			}
			return startDate;
		}

		public static GroupByPeriod GetGroupByPeriod(ReportDateRange dateRange)
		{
			switch (dateRange)
			{
                case ReportDateRange.TODAY:
                case ReportDateRange.LAST_SEVEN_DAYS:
                    return GroupByPeriod.byday;
                case ReportDateRange.THIS_MONTH:
				case ReportDateRange.PREVIOUS_MONTH:
					return GroupByPeriod.byday;
				default:
					return GroupByPeriod.byweek;

			}
		}

		public static string GetPeriodLabel(DateTime periodDate, GroupByPeriod period)
		{
			switch (period)
			{
				case GroupByPeriod.byday:
					return periodDate.ToString("dd");
				case GroupByPeriod.bymonth:
					return periodDate.ToString("MMM");
				default:
					return periodDate.ToString("dd-MMM");
			}
		}

		public static int GetNumberOfDays(DateTime startDate, DateTime endDate)
		{
			return (int) Math.Round(((endDate - startDate).TotalDays));

		}
	}

}

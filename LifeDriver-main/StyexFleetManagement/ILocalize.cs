using System;
using System.Globalization;

namespace StyexFleetManagement
{
	public interface ILocalize
	{
		CultureInfo GetCurrentCultureInfo();
	}
}


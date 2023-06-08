using System;
using Foundation;

[assembly: Xamarin.Forms.Dependency(typeof(StyexFleetManagement.iOS.Services.InfoService))]
namespace StyexFleetManagement.iOS.Services
{
	public class InfoService : IInfoService
	{
		
		public string AppVersionCode
		{
			get
			{
				return NSBundle.MainBundle.InfoDictionary["CFBundleShortVersionString"].ToString();
			}
		}

	}
}


using System;
using Xamarin.Forms;

[assembly: Xamarin.Forms.Dependency(typeof(StyexFleetManagement.Droid.Services.InfoService))]
namespace StyexFleetManagement.Droid.Services
{
	public class InfoService : IInfoService
	{
		public string AppVersionCode
		{
			get
			{
				var context = Forms.Context;
				return context.PackageManager.GetPackageInfo(context.PackageName, 0).VersionCode.ToString();
			}
		}
	}
}


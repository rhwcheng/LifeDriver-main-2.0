using System;
using StyexFleetManagement.Services;
using Xamarin.Forms;

namespace StyexFleetManagement
{
	public partial class HomePage : ContentPage
	{
		public HomePage ()
		{
			InitializeComponent ();
		}

		async void OnLogoutButtonClicked (object sender, EventArgs e)
		{
			DependencyService.Get<ICredentialsService> ().DeleteCredentials ();
			Navigation.InsertPageBefore (new LoginPage (), this);
			await Navigation.PopAsync ();
		}
	}
}

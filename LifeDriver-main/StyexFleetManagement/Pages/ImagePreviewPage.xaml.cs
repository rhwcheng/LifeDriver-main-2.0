using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using System;
using System.IO;
using StyexFleetManagement.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ImagePreviewPage : PopupPage
    {
        private ImageSource _imageSource;

        public ImageSource ImageSource
        {
            set
            {
                _imageSource = value;
                PreviewImage.Source = value;
            }
        }

        public ImagePreviewPage()
        {
            BindingContext = this;

            InitializeComponent();
            SetUpPadding();
            SetUpGestureRecognizers();
        }

        private void SetUpPadding()
        {
            HasSystemPadding = true;
            var sidePadding = Device.Idiom == TargetIdiom.Phone ? 10 : (App.ScreenWidth) / 5;
            Padding = new Thickness(sidePadding, 10, sidePadding, 10);
        }

        private void SetUpGestureRecognizers()
        {
            var closeGestureRecognizer = new TapGestureRecognizer();
            closeGestureRecognizer.Tapped += CloseGestureRecognizer_Tapped;

            Content.GestureRecognizers.Add(closeGestureRecognizer);
        }

        private static async void CloseGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAsync();
        }
    }
}
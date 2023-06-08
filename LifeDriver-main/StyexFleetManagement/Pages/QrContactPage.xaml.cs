using System;
using System.IO;
using FFImageLoading.Forms;
using QRCoder;
using Rg.Plugins.Popup.Extensions;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using StyexFleetManagement.Models;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace StyexFleetManagement.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QrContactPage : PopupPage
    {
        public QrContactPage()
        {
            InitializeComponent();
            SetUpPadding();
            SetUpGestureRecognizers();
            LoadQrCode();
        }

        private void LoadQrCode()
        {
            var generator = new PayloadGenerator.ContactData(PayloadGenerator.ContactData.ContactOutputType.VCard3, Settings.Current.SalusUserFirstName, Settings.Current.SalusUserLastName, phone: Settings.Current.SalusUserPhoneNumber, email: Settings.Current.SalusUserEmailAddress);
            var payload = generator.ToString();
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(20);

            QrCodeImage.Source = ImageSource.FromStream(() => new MemoryStream(qrCodeBytes));
        }

        private void SetUpPadding()
        {
            HasSystemPadding = true;
            double sidePadding = 0;
            if (Device.Idiom == TargetIdiom.Phone)
            {
                sidePadding = 10;
            }
            else
            {
                sidePadding = (App.ScreenWidth) / 5;
            }

            Padding = new Thickness(sidePadding, 10, sidePadding, 10);
        }

        private void SetUpGestureRecognizers()
        {
            var closeGestureRecognizer = new TapGestureRecognizer();
            closeGestureRecognizer.Tapped += async (sender, args) => await PopupNavigation.Instance.PopAsync();

            CloseButton.GestureRecognizers.Add(closeGestureRecognizer);
        }
    }
}
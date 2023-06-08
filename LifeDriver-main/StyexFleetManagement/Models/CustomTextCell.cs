using Xamarin.Forms;

namespace StyexFleetManagement.Models
{
    public class CustomTextCell : ViewCell
    {
        Label descriptionLabel;

        public static readonly BindableProperty DescriptionProperty =
    BindableProperty.Create("Description", typeof(string), typeof(CustomTextCell), "");

        public string Description
        {
            get => (string)GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        public CustomTextCell()
        {
            descriptionLabel = new Label();
            descriptionLabel.SetBinding(Label.TextProperty, "Description");

            View = descriptionLabel;
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();

            if (BindingContext != null)
            {
                descriptionLabel.Text = Description;
                
            }
        }
    }
}

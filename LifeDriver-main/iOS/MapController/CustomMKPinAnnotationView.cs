using MapKit;
using UIKit;

namespace StyexFleetManagement.iOS
{
	class CustomMKPinAnnotationView : MKAnnotationView
	{
		public string Id { get; set; }
		public string Url { get; set; }



		public CustomMKPinAnnotationView(IMKAnnotation annotation, string id): base(annotation, id)
		{
			
		}

        public CustomMKPinAnnotationView(IMKAnnotation annotation, string id, string excpetion) : base(annotation, id)
        {
            var uiView = new UIView();
            var line1 = new UITextView();
            var line2 = new UITextView();

            line1.Text = "Title";
            line2.Text = string.Format("Exception: {0}", excpetion);

            uiView.AddSubviews(line1, line2);

            base.DetailCalloutAccessoryView = uiView;
        }

    }
}
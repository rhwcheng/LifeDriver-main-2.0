using System;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class GestureFrame : Frame
    {
        public event EventHandler SwipeDown;
        public event EventHandler SwipeTop;
        public event EventHandler SwipeLeft;
        public event EventHandler SwipeRight;

        public void OnSwipeDown()
        {
            if (SwipeDown != null)
                SwipeDown(this, null);
        }

        public void OnSwipeTop()
        {
            if (SwipeTop != null)
                SwipeTop(this, null);
        }

        public void OnSwipeLeft()
        {
            if (SwipeLeft != null)
                SwipeLeft(this, null);
        }

        public void OnSwipeRight()
        {
            if (SwipeRight != null)
                SwipeRight(this, null);
        }
    }
}
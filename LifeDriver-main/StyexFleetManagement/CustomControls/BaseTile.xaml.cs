using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public partial class BaseTile : ContentView
    {
        public BaseTile()
        {
            InitializeComponent();
        }
            
        protected void SetTitle(string title){
            this.titleLabel.Text = title;
        }

        protected void SetImage(string imageSource){
            this.headerImage.Source = imageSource;
        }

        protected StackLayout ContentStack(){
            return contentStack;
        }
    }
}

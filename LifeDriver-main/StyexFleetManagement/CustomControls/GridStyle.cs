using StyexFleetManagement.Statics;
using Syncfusion.SfDataGrid.XForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace StyexFleetManagement.CustomControls
{
    public class GridStyle : DataGridStyle
    {
        public override GridLinesVisibility GetGridLinesVisibility()
        {
            return GridLinesVisibility.Vertical;
        }

        public override Color GetBordercolor()
        {
            return Color.Black;
        }

        public override Color GetHeaderBackgroundColor()
        {
            return Color.FromHex("#F5F5F5");
        }

        public override Color GetSelectionBackgroundColor()
        {
            return Color.FromHex("#81C784");
        }

        public override Color GetCaptionSummaryRowBackgroundColor()
        {
            return Color.FromHex("#FAFAFA");
        }

        public override Color GetCaptionSummaryRowForeGroundColor()
        {
            return Color.FromRgb(0, 0, 0);
        }
    }

    
}

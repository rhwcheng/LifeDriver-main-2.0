using System;
using Xamarin.Forms;
using System.Threading.Tasks;
namespace StyexFleetManagement.Pages
{
    public interface ISearchPage
    {
		Task OnSearchSelection(object sender, SelectedItemChangedEventArgs e);
    }
}

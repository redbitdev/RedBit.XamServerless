using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace RedBit.XamServerless
{
	public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();
            this.BindingContext = new MainPageViewModel();
            MessagingCenter.Subscribe<Mobile.Core.ViewModelBase, string[]>(this, "DisplayAlert", (sender, values) => {
                DisplayAlert(values[0], values[1], "Ok");
            });

        }
    }
}

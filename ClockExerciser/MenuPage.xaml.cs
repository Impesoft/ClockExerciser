using ClockExerciser.Helpers;
using ClockExerciser.ViewModels;
using Syncfusion.Maui.Gauges;

namespace ClockExerciser
{
    public partial class MenuPage : ContentPage
    {
        public MenuPage()
        {
            InitializeComponent();
            BindingContext = ServiceHelper.GetRequiredService<MenuViewModel>();
        }

        private void HourAxis_LabelCreated(object sender, LabelCreatedEventArgs e)
        {
            if (e.Text == "0")
            {
                e.Text = "12";
            }
        }
    }
}

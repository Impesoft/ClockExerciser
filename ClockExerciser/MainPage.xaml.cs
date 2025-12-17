using ClockExerciser.Helpers;
using ClockExerciser.ViewModels;
using Syncfusion.Maui.Gauges;

namespace ClockExerciser
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
            BindingContext = ServiceHelper.GetRequiredService<GameViewModel>();
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

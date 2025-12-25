using ClockExerciser.Helpers;
using ClockExerciser.ViewModels;
using Syncfusion.Maui.Gauges;

namespace ClockExerciser
{
    public partial class GamePage : ContentPage
    {
        public GamePage()
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

        private void HourSlider_DragCompleted(object sender, EventArgs e)
        {
            if (sender is Slider slider)
            {
                // Snap to half-hour increments (0.5 steps)
                double snappedValue = Math.Round(slider.Value * 2) / 2.0;
                slider.Value = snappedValue;
            }
        }
    }
}


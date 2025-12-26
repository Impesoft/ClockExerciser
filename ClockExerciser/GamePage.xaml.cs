using ClockExerciser.Helpers;
using ClockExerciser.ViewModels;
using Syncfusion.Maui.Gauges;

namespace ClockExerciser
{
    public partial class GamePage : ContentPage
    {
        private GameViewModel? _viewModel;
        
        public GamePage()
        {
            InitializeComponent();
            _viewModel = ServiceHelper.GetRequiredService<GameViewModel>();
            BindingContext = _viewModel;
        }
        
        protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
        {
            base.OnNavigatedFrom(args);
            
            // Dispose the ViewModel to stop the timer when truly navigating away
            // This prevents timer ticks on a disposed view
            _viewModel?.Dispose();
            _viewModel = null;
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


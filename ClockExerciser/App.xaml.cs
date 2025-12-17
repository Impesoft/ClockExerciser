using ClockExerciser.Services;

namespace ClockExerciser
{
    public partial class App : Application
    {
        private readonly LocalizationService _localizationService;

        public App(LocalizationService localizationService)
        {
            InitializeComponent();
            _localizationService = localizationService;
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(new AppShell());
        }
    }
}
using ClockExerciser.Services;
using System.Diagnostics;

namespace ClockExerciser
{
    public partial class App : Application
    {
        private readonly LocalizationService _localizationService;

        public App(LocalizationService localizationService)
        {
            // Add global exception handlers BEFORE InitializeComponent
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
            TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
            
            try
            {
                InitializeComponent();
                _localizationService = localizationService;
                Debug.WriteLine("✅ App initialized successfully");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ FATAL: App initialization failed: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to see the error
            }
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            try
            {
                Debug.WriteLine("✅ Creating main window");
                return new Window(new AppShell());
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ FATAL: Window creation failed: {ex.Message}");
                Debug.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                Debug.WriteLine($"❌ UNHANDLED EXCEPTION: {exception.Message}");
                Debug.WriteLine($"Stack trace: {exception.StackTrace}");
                Debug.WriteLine($"Inner exception: {exception.InnerException?.Message}");
            }
        }

        private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
        {
            Debug.WriteLine($"❌ UNOBSERVED TASK EXCEPTION: {e.Exception.Message}");
            Debug.WriteLine($"Stack trace: {e.Exception.StackTrace}");
            e.SetObserved(); // Prevent crash, but log it
        }
    }
}
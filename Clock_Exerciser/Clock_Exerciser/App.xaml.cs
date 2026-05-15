namespace Clock_Exerciser;

public partial class App : Application
{
    public App(IServiceProvider serviceProvider)
    {
        InitializeComponent();
        ServiceProvider = serviceProvider;
    }

    public IServiceProvider ServiceProvider { get; }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var mainPage = ServiceProvider.GetRequiredService<MainPage>();
        return new Window(mainPage) { Title = "Clock Exerciser" };
    }
}

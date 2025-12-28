using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using ClockExerciser.Models;
using ClockExerciser.Services;
using Microsoft.Maui.Controls;

namespace ClockExerciser.ViewModels;

public sealed class MenuViewModel : INotifyPropertyChanged
{
    private readonly LocalizationService _localizationService;
    private readonly IGameStateService _gameStateService;
    private LanguageOption? _selectedLanguage;
    private DifficultyOption? _selectedDifficulty;

    public MenuViewModel(LocalizationService localizationService, IGameStateService gameStateService)
    {
        _localizationService = localizationService;
        _gameStateService = gameStateService;
        _localizationService.CultureChanged += (_, _) => OnCultureChanged();

        Languages = new ObservableCollection<LanguageOption>
        {
            new("English", "en-US"),
            new("Nederlands", "nl-NL")
        };

        // Initialize difficulties - will be populated with localized names on culture change
        Difficulties = new ObservableCollection<DifficultyOption>();
        UpdateDifficulties();

        ClockToTimeCommand = new Command(async () => await NavigateToGame(GameMode.ClockToTime));
        TimeToClockCommand = new Command(async () => await NavigateToGame(GameMode.TimeToClock));
        RandomModeCommand = new Command(async () => await NavigateToGame(GameMode.Random));

        UpdateCultureDependentData();
        
        // Set selected language to match current culture from service
        SelectedLanguage = Languages.FirstOrDefault(l => l.Culture.Name == _localizationService.CurrentCulture.Name) 
                          ?? Languages.First();
        
        // Set selected difficulty to match current difficulty from service
        SelectedDifficulty = Difficulties.FirstOrDefault(d => d.Level == _gameStateService.CurrentDifficulty)
                          ?? Difficulties.First();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<LanguageOption> Languages { get; }
    public ObservableCollection<DifficultyOption> Difficulties { get; }

    public LanguageOption? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value) && value is not null)
            {
                _localizationService.SetCulture(value.Culture);
            }
        }
    }

    public DifficultyOption? SelectedDifficulty
    {
        get => _selectedDifficulty;
        set
        {
            if (SetProperty(ref _selectedDifficulty, value) && value is not null)
            {
                _gameStateService.CurrentDifficulty = value.Level;
            }
        }
    }

    public Command ClockToTimeCommand { get; }
    public Command TimeToClockCommand { get; }
    public Command RandomModeCommand { get; }

    public string AppTitle => _localizationService.GetString("AppTitle");
    public string MenuTitle => _localizationService.GetString("MenuTitle");
    public string MenuSubtitle => _localizationService.GetString("MenuSubtitle");
    public string ClockToTimeButton => _localizationService.GetString("ModeClockToTime");
    public string TimeToClockButton => _localizationService.GetString("ModeTimeToClock");
    public string RandomModeButton => _localizationService.GetString("ModeRandom");
    public string LanguageLabel => _localizationService.GetString("LanguageLabel");
    public string DifficultyLabel => _localizationService.GetString("DifficultyLabel");

    private async Task NavigateToGame(GameMode mode)
    {
        await Shell.Current.GoToAsync("///game", new Dictionary<string, object>
        {
            { "mode", mode }
        });
    }

    private void UpdateCultureDependentData()
    {
        OnPropertyChanged(nameof(AppTitle));
        OnPropertyChanged(nameof(MenuTitle));
        OnPropertyChanged(nameof(MenuSubtitle));
        OnPropertyChanged(nameof(ClockToTimeButton));
        OnPropertyChanged(nameof(TimeToClockButton));
        OnPropertyChanged(nameof(RandomModeButton));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(DifficultyLabel));
        UpdateDifficulties(); // Refresh difficulty names when culture changes
    }

    private void UpdateDifficulties()
    {
        var currentLevel = _selectedDifficulty?.Level ?? DifficultyLevel.Normal;
        
        Difficulties.Clear();
        Difficulties.Add(new DifficultyOption(_localizationService.GetString("DifficultyBeginner"), DifficultyLevel.Beginner));
        Difficulties.Add(new DifficultyOption(_localizationService.GetString("DifficultyNormal"), DifficultyLevel.Normal));
        Difficulties.Add(new DifficultyOption(_localizationService.GetString("DifficultyAdvanced"), DifficultyLevel.Advanced));

        // Restore selection
        SelectedDifficulty = Difficulties.FirstOrDefault(d => d.Level == currentLevel)
                          ?? Difficulties.First();
    }

    private void OnCultureChanged()
    {
        UpdateCultureDependentData();
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}

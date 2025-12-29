using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using ClockExerciser.Models;
using ClockExerciser.Services;

namespace ClockExerciser.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private readonly LocalizationService _localizationService;
    private readonly ISettingsService _settingsService;
    private readonly ITextToSpeechService _ttsService;
    
    private LanguageOption? _selectedLanguage;
    private RegionOption? _selectedRegion;
    private bool _voiceOutputEnabled;
    private bool _voiceInputEnabled;

    public SettingsViewModel(
        LocalizationService localizationService,
        ISettingsService settingsService,
        ITextToSpeechService ttsService)
    {
        _localizationService = localizationService;
        _settingsService = settingsService;
        _ttsService = ttsService;
        
        _localizationService.CultureChanged += (_, _) => OnCultureChanged();

        AvailableLanguages = new ObservableCollection<LanguageOption>
        {
            new("English", "en-US"),
            new("Nederlands", "nl-NL")
        };

        AvailableRegions = new ObservableCollection<RegionOption>();

        TestVoiceCommand = new Command(async () => await TestVoiceAsync());

        InitializeAsync();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<LanguageOption> AvailableLanguages { get; }
    public ObservableCollection<RegionOption> AvailableRegions { get; }

    public LanguageOption? SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            if (SetProperty(ref _selectedLanguage, value) && value is not null)
            {
                _localizationService.SetCulture(value.Culture);
                UpdateAvailableRegions();
            }
        }
    }

    public RegionOption? SelectedRegion
    {
        get => _selectedRegion;
        set
        {
            if (SetProperty(ref _selectedRegion, value) && value is not null)
            {
                SavePreferredLocaleAsync(value.LocaleCode);
            }
        }
    }

    public bool VoiceOutputEnabled
    {
        get => _voiceOutputEnabled;
        set
        {
            if (SetProperty(ref _voiceOutputEnabled, value))
            {
                _settingsService.SetVoiceOutputEnabledAsync(value);
            }
        }
    }

    public bool VoiceInputEnabled
    {
        get => _voiceInputEnabled;
        set
        {
            if (SetProperty(ref _voiceInputEnabled, value))
            {
                _settingsService.SetVoiceInputEnabledAsync(value);
            }
        }
    }

    public ICommand TestVoiceCommand { get; }

    // Localized strings
    public string SettingsTitle => _localizationService.GetString("SettingsTitle");
    public string LanguageLabel => _localizationService.GetString("LanguageLabel");
    public string VoiceOutputLabel => _localizationService.GetString("VoiceOutputLabel");
    public string VoiceOutputDescription => _localizationService.GetString("VoiceOutputDescription");
    public string VoiceRegionLabel => _localizationService.GetString("VoiceRegionLabel");
    public string TestVoiceButtonText => _localizationService.GetString("TestVoiceButtonText");
    public string VoiceInputLabel => _localizationService.GetString("VoiceInputLabel");
    public string VoiceInputDescription => _localizationService.GetString("VoiceInputDescription");

    private async void InitializeAsync()
    {
        // Load settings
        _voiceOutputEnabled = await _settingsService.GetVoiceOutputEnabledAsync();
        _voiceInputEnabled = await _settingsService.GetVoiceInputEnabledAsync();

        OnPropertyChanged(nameof(VoiceOutputEnabled));
        OnPropertyChanged(nameof(VoiceInputEnabled));

        // Set selected language to match current culture
        _selectedLanguage = AvailableLanguages.FirstOrDefault(l => 
            l.Culture.Name == _localizationService.CurrentCulture.Name) 
            ?? AvailableLanguages.First();
        OnPropertyChanged(nameof(SelectedLanguage));

        UpdateAvailableRegions();
    }

    private async void UpdateAvailableRegions()
    {
        AvailableRegions.Clear();

        var currentLanguage = _localizationService.CurrentCulture.TwoLetterISOLanguageName;

        if (currentLanguage == "en")
        {
            AvailableRegions.Add(new RegionOption 
            { 
                Code = "US", 
                LocaleCode = "en-US", 
                DisplayName = "???? American" 
            });
            AvailableRegions.Add(new RegionOption 
            { 
                Code = "GB", 
                LocaleCode = "en-GB", 
                DisplayName = "???? British" 
            });
        }
        else if (currentLanguage == "nl")
        {
            AvailableRegions.Add(new RegionOption 
            { 
                Code = "NL", 
                LocaleCode = "nl-NL", 
                DisplayName = "???? Nederlands" 
            });
            AvailableRegions.Add(new RegionOption 
            { 
                Code = "BE", 
                LocaleCode = "nl-BE", 
                DisplayName = "???? Vlaams" 
            });
        }

        // Load saved preference and select it
        var preferredLocale = await _settingsService.GetPreferredLocaleAsync(currentLanguage);
        _selectedRegion = AvailableRegions.FirstOrDefault(r => r.LocaleCode == preferredLocale)
                         ?? AvailableRegions.FirstOrDefault();
        OnPropertyChanged(nameof(SelectedRegion));
    }

    private async void SavePreferredLocaleAsync(string localeCode)
    {
        var currentLanguage = _localizationService.CurrentCulture.TwoLetterISOLanguageName;
        await _settingsService.SetPreferredLocaleAsync(currentLanguage, localeCode);
    }

    private async Task TestVoiceAsync()
    {
        if (!VoiceOutputEnabled || SelectedRegion == null)
            return;

        var testPhrase = _localizationService.GetString("TestVoicePhrase");
        await _ttsService.SpeakAsync(testPhrase, SelectedRegion.LocaleCode);
    }

    private void OnCultureChanged()
    {
        OnPropertyChanged(nameof(SettingsTitle));
        OnPropertyChanged(nameof(LanguageLabel));
        OnPropertyChanged(nameof(VoiceOutputLabel));
        OnPropertyChanged(nameof(VoiceOutputDescription));
        OnPropertyChanged(nameof(VoiceRegionLabel));
        OnPropertyChanged(nameof(TestVoiceButtonText));
        OnPropertyChanged(nameof(VoiceInputLabel));
        OnPropertyChanged(nameof(VoiceInputDescription));
        
        UpdateAvailableRegions();
    }

    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

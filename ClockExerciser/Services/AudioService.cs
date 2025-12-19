using Plugin.Maui.Audio;

namespace ClockExerciser.Services;

/// <summary>
/// Audio service for playing feedback sounds using Plugin.Maui.Audio
/// </summary>
public class AudioService : IAudioService
{
    private readonly IAudioManager _audioManager;
    private IAudioPlayer? _successPlayer;
    private IAudioPlayer? _errorPlayer;

    public AudioService(IAudioManager audioManager)
    {
        _audioManager = audioManager ?? throw new ArgumentNullException(nameof(audioManager));
    }

    public async Task PlaySuccessSound()
    {
        try
        {
            // Dispose previous player if exists
            _successPlayer?.Dispose();

            // Load and play success sound
            using var successStream = await FileSystem.OpenAppPackageFileAsync("success.mp3");
            if (successStream == null)
            {
                System.Diagnostics.Debug.WriteLine("Success sound file not found");
                return;
            }
            
            _successPlayer = _audioManager.CreatePlayer(successStream);
            if (_successPlayer == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to create success audio player");
                return;
            }
            
            _successPlayer.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error playing success sound: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }

    public async Task PlayErrorSound()
    {
        try
        {
            // Dispose previous player if exists
            _errorPlayer?.Dispose();

            // Load and play error sound
            using var errorStream = await FileSystem.OpenAppPackageFileAsync("error.mp3");
            if (errorStream == null)
            {
                System.Diagnostics.Debug.WriteLine("Error sound file not found");
                return;
            }
            
            _errorPlayer = _audioManager.CreatePlayer(errorStream);
            if (_errorPlayer == null)
            {
                System.Diagnostics.Debug.WriteLine("Failed to create error audio player");
                return;
            }
            
            _errorPlayer.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error playing error sound: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
        }
    }
}

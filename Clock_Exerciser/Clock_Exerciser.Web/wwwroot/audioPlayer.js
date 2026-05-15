// Audio player module for Blazor Web
export function playAudio(audioPath) {
    const audio = new Audio(audioPath);
    audio.volume = 0.5; // 50% volume
    audio.play().catch(error => {
        console.error('Audio playback failed:', error);
    });
}

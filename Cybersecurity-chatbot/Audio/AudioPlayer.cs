using System;
using System.IO;
using System.Media;
using System.Windows;

namespace CybersecurityChatbot.Audio;

/// <summary>
/// Handles WAV audio playback for the voice greeting.
/// Compatible with both the Part 1 console app and the Part 2 WPF GUI.
/// </summary>
public class AudioPlayer
{
    // Relative path from the output directory to the WAV file
    private readonly string _greetingPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory, "greeting.wav");

    /// <summary>
    /// Plays the greeting WAV file asynchronously.
    /// Silently skips if the file is not found (so the app still runs without it).
    /// </summary>
    public void PlayGreeting()
    {
        try
        {
            if (File.Exists(_greetingPath))
            {
                using var player = new SoundPlayer(_greetingPath);
                player.Play(); // async — does not block the UI thread
            }
            // No warning shown if file is missing — the app works without audio
        }
        catch (Exception ex)
        {
            // Log to Debug output only; never crash the app over missing audio
            System.Diagnostics.Debug.WriteLine($"[AudioPlayer] Could not play greeting: {ex.Message}");
        }
    }
}
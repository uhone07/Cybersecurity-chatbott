using System;
using System.IO;
using System.Threading;

namespace CybersecurityChatbot.Audio
{
    /// <summary>
    /// Handles playback of the WAV voice greeting on application launch.
    /// Uses reflection to avoid a compile-time dependency on System.Windows.Extensions
    /// (which contains System.Media.SoundPlayer on netcore/net5+). Falls back to a
    /// MinimalAudioPlayer when SoundPlayer is not available at runtime.
    /// </summary>
    public class AudioPlayer
    {
        private const string WAV_FILE_NAME = "greeting.wav";

        /// <summary>
        /// Attempts to play the WAV greeting file.
        /// Degrades gracefully with a console message if the file is missing.
        /// </summary>
        public void PlayGreeting()
        {
            try
            {
                string wavPath = GetWavPath();

                if (!File.Exists(wavPath))
                {
                    PrintAudioWarning(wavPath);
                    return;
                }

                // Try to use System.Media.SoundPlayer via reflection so we don't need
                // a compile-time reference to System.Windows.Extensions.
                // Type name and assembly based on the diagnostic message.
                try
                {
                    var spType = Type.GetType("System.Media.SoundPlayer, System.Windows.Extensions");
                    if (spType != null)
                    {
                        // Constructor SoundPlayer(string)
                        var ctor = spType.GetConstructor(new[] { typeof(string) });
                        if (ctor != null)
                        {
                            var player = ctor.Invoke(new object[] { wavPath });
                            var playMethod = spType.GetMethod("Play", Type.EmptyTypes);
                            playMethod?.Invoke(player, null);

                            // Try to dispose if available (non-critical)
                            var disposeMethod = spType.GetMethod("Dispose");
                            disposeMethod?.Invoke(player, null);
                            return;
                        }
                    }
                }
                catch
                {
                    // Reflection failed — fall through to MinimalAudioPlayer fallback.
                }

                // Fallback: simple beep-based greeting that is safe across platforms.
                var minimal = new MinimalAudioPlayer();
                minimal.PlayGreeting();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                Console.WriteLine($"\n  [Audio] Could not play greeting: {ex.Message}");
                Console.ResetColor();
            }
        }

        // ─── Helpers ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Resolves the full path to the WAV file.
        /// </summary>
        private static string GetWavPath()
        {
            string exeDir = AppDomain.CurrentDomain.BaseDirectory;
            string exeDirPath = Path.Combine(exeDir, WAV_FILE_NAME);
            if (File.Exists(exeDirPath)) return exeDirPath;

            return Path.Combine(Directory.GetCurrentDirectory(), WAV_FILE_NAME);
        }

        /// <summary>
        /// Prints a friendly warning when the WAV file cannot be located.
        /// </summary>
        private static void PrintAudioWarning(string expectedPath)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine();
            Console.WriteLine("  ─────────────────────────────────────────────────────");
            Console.WriteLine("  [Audio Notice]");
            Console.WriteLine($"  greeting.wav not found at: {expectedPath}");
            Console.WriteLine("  Add your recorded WAV file to the project directory");
            Console.WriteLine("  and set 'Copy to Output Directory' to 'Copy if newer'.");
            Console.WriteLine("  ─────────────────────────────────────────────────────");
            Console.ResetColor();
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Minimal AudioPlayer implementation to satisfy usages in ConsoleUI.
    /// Provides a small greeting sound sequence. Non-fatal on platforms
    /// that do not support Console.Beep.
    /// </summary>
    public class MinimalAudioPlayer
    {
        public MinimalAudioPlayer()
        {
            // Placeholder constructor for future expansion (e.g., audio device init).
        }

        /// <summary>
        /// Plays a short greeting. Swallows exceptions so non-Windows platforms
        /// or environments without a console sound device do not crash the app.
        /// </summary>
        public void PlayGreeting()
        {
            try
            {
                // Play a short, non-blocking beep sequence on a background thread.
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    try
                    {
                        Console.Beep(700, 120);
                        Thread.Sleep(60);
                        Console.Beep(900, 120);
                    }
                    catch
                    {
                       
                    }
                });
            }
            catch
            {
                
            }
        }
    }
}

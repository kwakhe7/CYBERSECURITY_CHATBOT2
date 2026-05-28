using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace ChatbotPart2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        // Show a splash screen for 2.5 seconds, then open the main window.
        private async void Application_Startup(object sender, StartupEventArgs e)
        {
            var splash = new SplashWindow();
            // Show the splash window centered on screen
            splash.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            splash.Show();

            // Play startup audio (create a tiny test WAV if no file exists).
            try
            {
                var soundPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "safenet_greeting.wav");
                // EnsureTestGreeting returns true when file exists or was created successfully
                if (AudioPlayer.EnsureTestGreeting(soundPath))
                {
                    // Play asynchronously so UI is not blocked
                    AudioPlayer.PlayAsync(soundPath);
                }
            }
            catch
            {
                // ignore audio errors
            }

            // Wait 2.5 seconds (2500 ms). Adjust to 2000-3000 ms as required.
            await Task.Delay(2500);

            var main = new MainWindow();
            // set as application's main window so shutdown behavior is correct
            this.MainWindow = main;
            main.Show();
            main.Activate();

            // Close splash once main has shown
            splash.Close();
        }
    }

}

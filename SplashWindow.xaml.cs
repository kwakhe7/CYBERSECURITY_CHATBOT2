using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Reflection;

namespace ChatbotPart2
{
    /// <summary>
    /// Interaction logic for SplashWindow.xaml
    /// </summary>
    public partial class SplashWindow : Window
    {
        // Preferred filename to look for first (can be changed to any desired filename)
        private const string PreferredLogoFileName = "logo_png.png";
        public SplashWindow()
        {
            InitializeComponent();
            LoadLogoAtRuntime();
        }

        private void LoadLogoAtRuntime()
        {
            try
            {
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;

                // Prefer the explicitly requested filename first
                var preferredPaths = new[]
                {
                    Path.Combine(baseDir, "Assets", PreferredLogoFileName),
                    Path.Combine(baseDir, PreferredLogoFileName)
                };

                foreach (var fp in preferredPaths)
                {
                    if (File.Exists(fp))
                    {
                        SetImageFromPath(fp);
                        return;
                    }
                }

                // If preferred file not found, fall back to a small set of known filenames
                string[] fallbackCandidates = new[]
                {
                    Path.Combine(baseDir, "Assets", "spider_logo.png"),
                    Path.Combine(baseDir, "Assets", "spider_logo.jpg"),
                    Path.Combine(baseDir, "spider_logo.png"),
                };

                foreach (var fp in fallbackCandidates)
                {
                    if (File.Exists(fp))
                    {
                        SetImageFromPath(fp);
                        return;
                    }
                }

                // Try pack resources, preferring the preferred name
                string[] packCandidates = new[]
                {
                    $"pack://application:,,,/Assets/{PreferredLogoFileName}",
                    "pack://application:,,,/Assets/spider_logo.png",
                    $"pack://application:,,,/{PreferredLogoFileName}"
                };

                foreach (var uriStr in packCandidates)
                {
                    try
                    {
                        var uri = new Uri(uriStr, UriKind.Absolute);
                        var bmp = new BitmapImage(uri);
                        var imgCtrl = this.FindName("LogoImage") as System.Windows.Controls.Image;
                        if (imgCtrl != null) imgCtrl.Source = bmp;
                        return;
                    }
                    catch
                    {
                        // ignore and try next
                    }
                }

                // Finally try embedded resources containing the preferred token
                var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
                var names = asm.GetManifestResourceNames();
                string found = null;
                foreach (var n in names)
                {
                    if (n.IndexOf(Path.GetFileNameWithoutExtension(PreferredLogoFileName), StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        found = n;
                        break;
                    }
                }

                if (found != null)
                {
                    using var s = asm.GetManifestResourceStream(found);
                    if (s != null)
                    {
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.StreamSource = s;
                        bmp.EndInit();
                        bmp.Freeze();
                        // Try to find the image control by either known name.
                        var imgCtrl3 = this.FindName("logo_png") as System.Windows.Controls.Image
                                      ?? this.FindName("LogoImage") as System.Windows.Controls.Image;
                        if (imgCtrl3 != null) imgCtrl3.Source = bmp;
                        return;
                    }
                }
            }
            catch
            {
                // ignore failures and leave image blank
            }
        }

        private void SetImageFromPath(string path)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.UriSource = new Uri(path, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            // Try known control names (legacy and new)
            var imgCtrl = this.FindName("logo_png") as System.Windows.Controls.Image
                         ?? this.FindName("LogoImage") as System.Windows.Controls.Image;
            if (imgCtrl != null) imgCtrl.Source = bmp;
        }
    }
}

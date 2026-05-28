using System;
using System.Windows;
using System.Windows.Input;

namespace ChatbotPart2
{
    public partial class MainWindow : Window
    {
        private ChatBot bot;

        public MainWindow()
        {
            InitializeComponent();

            bot = new ChatBot("SAFENET");

            LoadAsciiArt();
            PlayGreeting();

            // Prompt the user for their name on first start so replies can be personalized
            bot.PromptForName();
            AppendMessage("BOT", "Hi — what's your name? (Just type your first name)");
        }

        // SEND BUTTON
        private void SendButton_Click(object sender, RoutedEventArgs e)
        {
            SendMessage();
        }

        // ENTER KEY SUPPORT
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                SendMessage();
            }
        }

        // CORE MESSAGE HANDLER
        private void SendMessage()
        {
            string input = InputTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            AppendMessage("YOU", input);

            string response = bot.GetResponse(input);

            if (string.IsNullOrWhiteSpace(response))
            {
                response = "I'm not sure I understand. Try: password, phishing, malware, privacy, or safe browsing.";
            }

            AppendMessage("BOT", response);

            InputTextBox.Clear();

            ScrollToBottom();
        }

        // DISPLAY MESSAGE
        private void AppendMessage(string sender, string message)
        {
            ChatTextBlock.Text += $"{sender}: {message}\n\n";
        }

        // AUTO SCROLL
        private void ScrollToBottom()
        {
            ChatScrollViewer.ScrollToEnd();
        }

        // ASCII (you already have it in XAML, so this is optional)
        private void LoadAsciiArt()
        {
            // Not needed since ASCII is inside XAML already
        }

        // AUDIO GREETING
        private void PlayGreeting()
        {
            try
            {
                string path = System.IO.Path.Combine(AppContext.BaseDirectory, "greeting.wav");
                AudioPlayer.PlaySync(path);
            }
            catch
            {
                // ignore errors
            }
        }
    }
}
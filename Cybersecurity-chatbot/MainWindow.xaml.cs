using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbot.Chat;
using CybersecurityChatbot.Audio;

namespace CybersecurityChatbot;

public partial class MainWindow : Window
{
    private readonly ChatBot _bot = new ChatBot();
    private readonly AudioPlayer _audio = new AudioPlayer();
    private bool _nameCollected = false;
    private const string PlaceholderText = "Type a message or ask about cybersecurity topics...";

    public MainWindow()
    {
        InitializeComponent();
        Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _audio.PlayGreeting();
        AddBotMessage(
            "🛡️ Welcome to the Cybersecurity Awareness Bot!\n\n" +
            "I'm here to help you stay safe in the digital world.\n" +
            "I can answer questions about passwords, phishing, privacy, scams, malware, and much more.\n\n" +
            "Before we begin — what's your name?");
        TxtInput.Text = PlaceholderText;
        TxtInput.Foreground = new SolidColorBrush(Color.FromRgb(0x8B, 0x94, 0x9E));
    }

    private void TxtInput_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            e.Handled = true;
            ProcessInput();
        }
    }

    private void BtnSend_Click(object sender, RoutedEventArgs e)
    {
        ProcessInput();
    }

    private void TxtInput_GotFocus(object sender, RoutedEventArgs e)
    {
        if (TxtInput.Text == PlaceholderText)
        {
            TxtInput.Text = string.Empty;
            TxtInput.Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3));
        }
    }

    private void TxtInput_LostFocus(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TxtInput.Text))
        {
            TxtInput.Text = PlaceholderText;
            TxtInput.Foreground = new SolidColorBrush(Color.FromRgb(0x8B, 0x94, 0x9E));
        }
    }

    private void ProcessInput()
    {
        string text = TxtInput.Text.Trim();
        if (string.IsNullOrWhiteSpace(text) || text == PlaceholderText)
            return;

        TxtInput.Text = string.Empty;
        TxtInput.Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3));

        AddUserMessage(text);

        if (!_nameCollected)
        {
            _bot.SetUserName(text);
            _nameCollected = true;
            UpdateMemoryPanel();
            AddBotMessage(
                $"Nice to meet you, {_bot.GetUserName()}! 👋\n\n" +
                "You can ask me about any cybersecurity topic, or use the quick-topic chips on the left.\n" +
                "Try typing something like:\n" +
                "  • 'Tell me about phishing'\n" +
                "  • 'I'm worried about online scams'\n" +
                "  • 'I'm interested in privacy'\n" +
                "  • 'What's your purpose?'");
            return;
        }

        string response = _bot.GetResponse(text);
        AddBotMessage(response);
        UpdateMemoryPanel();
    }

    private void ChipButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button btn && btn.Tag is string topic)
        {
            TxtInput.Text = topic;
            TxtInput.Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3));
            ProcessInput();
        }
    }

    private void BtnClear_Click(object sender, RoutedEventArgs e)
    {
        ChatPanel.Children.Clear();
        AddBotMessage("Chat cleared. How can I help you with cybersecurity today?");
    }

    private void AddUserMessage(string text)
    {
        var container = new Grid { Margin = new Thickness(60, 4, 0, 4) };
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var bubble = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x1F, 0x3A, 0x5F)),
            CornerRadius = new CornerRadius(16, 16, 4, 16),
            Padding = new Thickness(14, 10, 14, 10),
            MaxWidth = 520,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        Grid.SetColumn(bubble, 1);

        var tb = new TextBlock
        {
            Text = text,
            Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20
        };
        bubble.Child = tb;
        container.Children.Add(bubble);
        ChatPanel.Children.Add(container);
        ScrollToBottom();
    }

    private void AddBotMessage(string text)
    {
        var container = new Grid { Margin = new Thickness(0, 4, 60, 4) };
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var avatar = new Border
        {
            Width = 32,
            Height = 32,
            CornerRadius = new CornerRadius(16),
            Background = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x9C)),
            Margin = new Thickness(0, 0, 10, 0),
            VerticalAlignment = VerticalAlignment.Top
        };
        var avatarText = new TextBlock
        {
            Text = "🛡",
            FontSize = 16,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        avatar.Child = avatarText;
        Grid.SetColumn(avatar, 0);

        var bubble = new Border
        {
            Background = new SolidColorBrush(Color.FromRgb(0x1A, 0x23, 0x32)),
            CornerRadius = new CornerRadius(4, 16, 16, 16),
            Padding = new Thickness(14, 10, 14, 10),
            MaxWidth = 560,
            HorizontalAlignment = HorizontalAlignment.Left,
            BorderBrush = new SolidColorBrush(Color.FromRgb(0x30, 0x36, 0x3D)),
            BorderThickness = new Thickness(1)
        };
        Grid.SetColumn(bubble, 1);

        var stack = new StackPanel();
        stack.Children.Add(bubble);
        var ts = new TextBlock
        {
            Text = DateTime.Now.ToString("HH:mm"),
            FontSize = 10,
            Foreground = new SolidColorBrush(Color.FromRgb(0x8B, 0x94, 0x9E)),
            Margin = new Thickness(4, 2, 0, 0)
        };
        stack.Children.Add(ts);
        Grid.SetColumn(stack, 1);

        var tb = BuildRichTextBlock(text);
        bubble.Child = tb;

        container.Children.Add(avatar);
        container.Children.Add(stack);
        ChatPanel.Children.Add(container);
        ScrollToBottom();
    }

    private TextBlock BuildRichTextBlock(string text)
    {
        var tb = new TextBlock
        {
            Foreground = new SolidColorBrush(Color.FromRgb(0xE6, 0xED, 0xF3)),
            FontSize = 13,
            TextWrapping = TextWrapping.Wrap,
            LineHeight = 20
        };

        var lines = text.Split('\n');
        for (int l = 0; l < lines.Length; l++)
        {
            if (l > 0) tb.Inlines.Add(new LineBreak());
            var parts = lines[l].Split(new[] { "**" }, StringSplitOptions.None);
            bool isBold = false;
            foreach (var part in parts)
            {
                if (isBold)
                {
                    tb.Inlines.Add(new Run(part)
                    {
                        FontWeight = FontWeights.Bold,
                        Foreground = new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x9C))
                    });
                }
                else
                {
                    tb.Inlines.Add(new Run(part));
                }
                isBold = !isBold;
            }
        }
        return tb;
    }

    private void ScrollToBottom()
    {
        ChatScrollViewer.UpdateLayout();
        ChatScrollViewer.ScrollToEnd();
    }

    private void UpdateMemoryPanel()
    {
        string name = _bot.GetUserName();
        TxtMemoryName.Text = string.IsNullOrEmpty(name) ? "Not set" : name;

        string topic = _bot.GetFavouriteTopic();
        TxtMemoryTopic.Text = string.IsNullOrEmpty(topic) ? "Not set" : topic;

        if (!string.IsNullOrEmpty(name))
            TxtGreeting.Text = $"Hello, {name}! Ask me anything about cybersecurity.";
    }
}
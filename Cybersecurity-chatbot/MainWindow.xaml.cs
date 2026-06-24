using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using CybersecurityChatbot.Audio;
using CybersecurityChatbot.Chat;
using CybersecurityChatbot.Data;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot.UI
{
    public partial class MainWindow : Window
    {
        private readonly ChatBot _bot = new ChatBot();
        private readonly AudioPlayer _audio = new AudioPlayer();
        private readonly DatabaseHelper _db = new DatabaseHelper();

        private bool _nameCollected = false;
        private const string Placeholder = "Type a message, or try 'start quiz', 'show my tasks', 'add a task to...'";

        // Quiz state
        private List<QuizQuestion> _questions = new List<QuizQuestion>();
        private int _currentQuestion = 0;
        private int _score = 0;
        private bool _answered = false;
        private bool _showingAll = false;  // for activity log "show more"

        public MainWindow()
        {
            InitializeComponent();

            // Wire NLP delegate actions so ChatBot can trigger UI changes
            _bot.OnNavigateToTasks = () => Dispatcher.Invoke(NavigateToTasks);
            _bot.OnNavigateToQuiz = () => Dispatcher.Invoke(NavigateToQuiz);
            _bot.OnShowActivityLog = () => Dispatcher.Invoke(NavigateToLog);
            _bot.OnAddTaskRequested = (title, desc, reminder) =>
                Dispatcher.Invoke(() => AddTaskToDb(title, desc, reminder));

            Loaded += OnLoaded;
        }

        // ── Startup ───────────────────────────────────────────────────────

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _db.InitialiseDatabase();
            _audio.PlayGreeting();
            ActivityLog.Add("Application started");

            SetChatPlaceholder();
            AddBotMessage(
                "🛡️ Welcome to the Cybersecurity Awareness Bot!\n\n" +
                "I can help with cybersecurity tips, manage your tasks, run a quiz, " +
                "and log everything you do.\n\n" +
                "Before we begin — what's your name?");
        }

        // ── Navigation ────────────────────────────────────────────────────

        private void NavTab_Checked(object sender, RoutedEventArgs e)
        {
            if (PanelChat == null) return; // guard before fully initialised
            PanelChat.Visibility = TabChat.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PanelTasks.Visibility = TabTasks.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PanelQuiz.Visibility = TabQuiz.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
            PanelLog.Visibility = TabLog.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;

            if (TabTasks.IsChecked == true) RefreshTaskList();
            if (TabLog.IsChecked == true) RefreshLog(10);
        }

        private void NavigateToTasks() { TabTasks.IsChecked = true; }
        private void NavigateToQuiz() { TabQuiz.IsChecked = true; }
        private void NavigateToLog() { TabLog.IsChecked = true; }

        // ── CHAT ─────────────────────────────────────────────────────────

        private void TxtChatInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) { e.Handled = true; ProcessChatInput(); }
        }

        private void BtnSend_Click(object sender, RoutedEventArgs e) => ProcessChatInput();

        private void TxtChatInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (TxtChatInput.Text == Placeholder)
            {
                TxtChatInput.Text = string.Empty;
                TxtChatInput.Foreground = Brush(0xE6, 0xED, 0xF3);
            }
        }

        private void TxtChatInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtChatInput.Text)) SetChatPlaceholder();
        }

        private void SetChatPlaceholder()
        {
            TxtChatInput.Text = Placeholder;
            TxtChatInput.Foreground = Brush(0x8B, 0x94, 0x9E);
        }

        private void ProcessChatInput()
        {
            string text = TxtChatInput.Text.Trim();
            if (string.IsNullOrWhiteSpace(text) || text == Placeholder) return;

            TxtChatInput.Text = string.Empty;
            TxtChatInput.Foreground = Brush(0xE6, 0xED, 0xF3);

            AddUserMessage(text);
            ActivityLog.Add($"User said: \"{text}\"");

            if (!_nameCollected)
            {
                _bot.SetUserName(text);
                _nameCollected = true;
                UpdateMemoryPanel();
                ActivityLog.Add($"User name set: {_bot.GetUserName()}");
                AddBotMessage(
                    $"Nice to meet you, {_bot.GetUserName()}! 👋\n\n" +
                    "Here's what I can do:\n" +
                    "  💬 Answer cybersecurity questions\n" +
                    "  📋 Add and manage tasks (try: 'add a task to enable 2FA')\n" +
                    "  🎮 Quiz you on cybersecurity (try: 'start quiz')\n" +
                    "  📜 Show your activity log (try: 'show activity log')\n\n" +
                    "What would you like to do?");
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
                TxtChatInput.Text = topic;
                TxtChatInput.Foreground = Brush(0xE6, 0xED, 0xF3);
                ProcessChatInput();
            }
        }

        // ── TASKS ─────────────────────────────────────────────────────────

        private void BtnAddTask_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtTaskTitle.Text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                MessageBox.Show("Please enter a task title.", "Missing Title",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string desc = TxtTaskDesc.Text.Trim();
            DateTime? reminder = DpReminder.SelectedDate;
            AddTaskToDb(title, desc, reminder);

            TxtTaskTitle.Text = string.Empty;
            TxtTaskDesc.Text = string.Empty;
            DpReminder.SelectedDate = null;
        }

        private void AddTaskToDb(string title, string desc, DateTime? reminder)
        {
            var task = new TaskItem
            {
                Title = title,
                Description = desc,
                ReminderDate = reminder,
                CreatedAt = DateTime.Now
            };
            _db.AddTask(task);
            ActivityLog.Add($"Task added: '{title}'" +
                (reminder.HasValue ? $" (reminder: {reminder.Value:dd MMM yyyy})" : ""));
            RefreshTaskList();
        }

        private void RefreshTaskList()
        {
            TaskListPanel.Children.Clear();
            var tasks = _db.GetAllTasks();

            if (tasks.Count == 0)
            {
                TaskListPanel.Children.Add(new TextBlock
                {
                    Text = "No tasks yet. Add your first cybersecurity task above!",
                    Foreground = Brush(0x8B, 0x94, 0x9E),
                    FontSize = 14,
                    Margin = new Thickness(0, 20, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            foreach (var task in tasks)
            {
                var card = new Border
                {
                    Background = Brush(0x16, 0x1B, 0x22),
                    BorderBrush = Brush(0x30, 0x36, 0x3D),
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(8),
                    Padding = new Thickness(16, 12, 16, 12),
                    Margin = new Thickness(0, 0, 0, 10)
                };

                var row = new Grid();
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

                // Left: task info
                var info = new StackPanel();
                var titleRow = new StackPanel { Orientation = Orientation.Horizontal };
                titleRow.Children.Add(new TextBlock
                {
                    Text = task.StatusIcon + "  ",
                    FontSize = 16,
                    VerticalAlignment = VerticalAlignment.Center
                });
                titleRow.Children.Add(new TextBlock
                {
                    Text = task.Title,
                    FontSize = 14,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = task.IsCompleted
                                        ? Brush(0x8B, 0x94, 0x9E)
                                        : Brush(0xE6, 0xED, 0xF3),
                    TextDecorations = task.IsCompleted ? TextDecorations.Strikethrough : null,
                    VerticalAlignment = VerticalAlignment.Center
                });
                info.Children.Add(titleRow);

                if (!string.IsNullOrWhiteSpace(task.Description))
                    info.Children.Add(new TextBlock
                    {
                        Text = task.Description,
                        FontSize = 12,
                        Foreground = Brush(0x8B, 0x94, 0x9E),
                        Margin = new Thickness(24, 2, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    });

                info.Children.Add(new TextBlock
                {
                    Text = task.ReminderText,
                    FontSize = 11,
                    Foreground = Brush(0xFF, 0x7B, 0x54),
                    Margin = new Thickness(24, 4, 0, 0)
                });

                info.Children.Add(new TextBlock
                {
                    Text = $"Added: {task.CreatedAt:dd MMM yyyy HH:mm}",
                    FontSize = 10,
                    Foreground = Brush(0x58, 0x59, 0x69),
                    Margin = new Thickness(24, 2, 0, 0)
                });

                Grid.SetColumn(info, 0);

                // Right: action buttons
                var buttons = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                if (!task.IsCompleted)
                {
                    var btnDone = new Button
                    {
                        Content = "✅ Done",
                        Tag = task.Id,
                        Margin = new Thickness(0, 0, 8, 0),
                        Padding = new Thickness(10, 6, 10, 6),
                        Background = Brush(0x00, 0xAA, 0x66),
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        BorderThickness = new Thickness(0),
                        Cursor = Cursors.Hand
                    };
                    btnDone.Click += BtnMarkDone_Click;
                    buttons.Children.Add(btnDone);
                }

                var btnDelete = new Button
                {
                    Content = "🗑️",
                    Tag = task.Id,
                    Padding = new Thickness(10, 6, 10, 6),
                    Background = Brush(0x44, 0x00, 0x00),
                    Foreground = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Cursor = Cursors.Hand
                };
                btnDelete.Click += BtnDeleteTask_Click;
                buttons.Children.Add(btnDelete);

                Grid.SetColumn(buttons, 1);
                row.Children.Add(info);
                row.Children.Add(buttons);
                card.Child = row;
                TaskListPanel.Children.Add(card);
            }
        }

        private void BtnMarkDone_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                _db.MarkCompleted(id);
                ActivityLog.Add($"Task marked as completed (ID: {id})");
                RefreshTaskList();
            }
        }

        private void BtnDeleteTask_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is int id)
            {
                var result = MessageBox.Show("Delete this task?", "Confirm Delete",
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    _db.DeleteTask(id);
                    ActivityLog.Add($"Task deleted (ID: {id})");
                    RefreshTaskList();
                }
            }
        }

        // ── QUIZ ─────────────────────────────────────────────────────────

        private void BtnStartQuiz_Click(object sender, RoutedEventArgs e)
        {
            InitialiseQuestions();
            _currentQuestion = 0;
            _score = 0;
            _answered = false;

            BtnStartQuiz.Visibility = Visibility.Collapsed;
            BtnNextQuestion.Visibility = Visibility.Visible;
            BtnRestartQuiz.Visibility = Visibility.Collapsed;

            ActivityLog.Add("Quiz started");
            ShowQuestion();
        }

        private void BtnNextQuestion_Click(object sender, RoutedEventArgs e)
        {
            if (!_answered)
            {
                MessageBox.Show("Please select an answer before continuing.",
                    "No Answer", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            _currentQuestion++;
            _answered = false;
            if (_currentQuestion < _questions.Count)
                ShowQuestion();
            else
                ShowQuizResults();
        }

        private void BtnRestartQuiz_Click(object sender, RoutedEventArgs e)
        {
            BtnStartQuiz.Visibility = Visibility.Visible;
            BtnNextQuestion.Visibility = Visibility.Collapsed;
            BtnRestartQuiz.Visibility = Visibility.Collapsed;
            QuizPanel.Children.Clear();
            TxtQuizScore.Text = "Score: 0 / 0";
            TxtQuizProgress.Text = "Question 0 of 12";
            TxtQuizSubtitle.Text = "Test your cybersecurity knowledge!";
        }

        private void ShowQuestion()
        {
            QuizPanel.Children.Clear();
            var q = _questions[_currentQuestion];

            TxtQuizProgress.Text = $"Question {_currentQuestion + 1} of {_questions.Count}";
            TxtQuizScore.Text = $"Score: {_score} / {_currentQuestion}";

            // Question text
            QuizPanel.Children.Add(new Border
            {
                Background = Brush(0x16, 0x1B, 0x22),
                BorderBrush = Brush(0x00, 0xFF, 0x9C),
                BorderThickness = new Thickness(0, 0, 0, 2),
                Padding = new Thickness(20, 16, 20, 16),
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Margin = new Thickness(0, 0, 0, 2),
                Child = new TextBlock
                {
                    Text = $"Q{_currentQuestion + 1}: {q.Question}",
                    FontSize = 16,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = Brush(0xE6, 0xED, 0xF3),
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 24
                }
            });

            // Answer options
            for (int i = 0; i < q.Options.Count; i++)
            {
                int index = i;
                var optBtn = new Button
                {
                    Content = $"{(char)('A' + i)})  {q.Options[i]}",
                    Tag = index,
                    HorizontalContentAlignment = HorizontalAlignment.Left,
                    Padding = new Thickness(16, 12, 16, 12),
                    Margin = new Thickness(0, 0, 0, 6),
                    Background = Brush(0x21, 0x26, 0x2D),
                    Foreground = Brush(0xE6, 0xED, 0xF3),
                    BorderBrush = Brush(0x30, 0x36, 0x3D),
                    BorderThickness = new Thickness(1),
                    FontSize = 13,
                    Cursor = Cursors.Hand
                };
                optBtn.Click += OptionBtn_Click;
                QuizPanel.Children.Add(optBtn);
            }
        }

        private void OptionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_answered) return;
            _answered = true;

            var q = _questions[_currentQuestion];
            int chosen = (int)((Button)sender).Tag;
            bool correct = chosen == q.CorrectIndex;

            if (correct) _score++;

            // Colour all buttons
            int btnIndex = 0;
            foreach (UIElement child in QuizPanel.Children)
            {
                if (child is Button btn && btn.Tag is int idx)
                {
                    if (idx == q.CorrectIndex)
                        btn.Background = Brush(0x00, 0x88, 0x44);   // green = correct
                    else if (idx == chosen && !correct)
                        btn.Background = Brush(0x88, 0x00, 0x00);   // red   = wrong choice
                    btn.IsEnabled = false;
                    btnIndex++;
                }
            }

            TxtQuizScore.Text = $"Score: {_score} / {_currentQuestion + 1}";

            // Feedback box
            QuizPanel.Children.Add(new Border
            {
                Background = correct ? Brush(0x00, 0x33, 0x22) : Brush(0x33, 0x00, 0x00),
                BorderBrush = correct ? Brush(0x00, 0xFF, 0x9C) : Brush(0xFF, 0x44, 0x44),
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(16, 12, 16, 12),
                Margin = new Thickness(0, 10, 0, 0),
                Child = new TextBlock
                {
                    Text = (correct ? "✅ Correct! " : "❌ Incorrect. ") + q.Explanation,
                    Foreground = correct ? Brush(0x00, 0xFF, 0x9C) : Brush(0xFF, 0x88, 0x88),
                    FontSize = 13,
                    TextWrapping = TextWrapping.Wrap,
                    LineHeight = 20
                }
            });

            ActivityLog.Add($"Quiz Q{_currentQuestion + 1}: {(correct ? "Correct" : "Incorrect")}");
        }

        private void ShowQuizResults()
        {
            QuizPanel.Children.Clear();
            BtnNextQuestion.Visibility = Visibility.Collapsed;
            BtnRestartQuiz.Visibility = Visibility.Visible;

            double pct = (double)_score / _questions.Count * 100;
            string grade = pct >= 80 ? "🏆 Cybersecurity Pro!"
                           : pct >= 60 ? "👍 Good effort! Keep learning."
                                       : "📚 Keep studying to stay safe online!";
            string colour = pct >= 80 ? "#00FF9C" : pct >= 60 ? "#58A6FF" : "#FF7B54";

            TxtQuizScore.Text = $"Final Score: {_score} / {_questions.Count}";
            TxtQuizProgress.Text = $"{pct:F0}% — {grade}";

            QuizPanel.Children.Add(new Border
            {
                Background = Brush(0x16, 0x1B, 0x22),
                BorderBrush = (SolidColorBrush)new BrushConverter().ConvertFrom(colour)!,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(12),
                Padding = new Thickness(30, 24, 30, 24),
                Margin = new Thickness(0, 20, 0, 0),
                Child = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Children =
                    {
                        new TextBlock
                        {
                            Text                = "Quiz Complete!",
                            FontSize            = 24,
                            FontWeight          = FontWeights.Bold,
                            Foreground          = (SolidColorBrush)new BrushConverter().ConvertFrom(colour)!,
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin              = new Thickness(0, 0, 0, 10)
                        },
                        new TextBlock
                        {
                            Text                = $"{_score} out of {_questions.Count} correct ({pct:F0}%)",
                            FontSize            = 18,
                            Foreground          = Brush(0xE6, 0xED, 0xF3),
                            HorizontalAlignment = HorizontalAlignment.Center,
                            Margin              = new Thickness(0, 0, 0, 10)
                        },
                        new TextBlock
                        {
                            Text                = grade,
                            FontSize            = 16,
                            Foreground          = Brush(0x8B, 0x94, 0x9E),
                            HorizontalAlignment = HorizontalAlignment.Center
                        }
                    }
                }
            });

            ActivityLog.Add($"Quiz completed — Score: {_score}/{_questions.Count} ({pct:F0}%)");
        }

        private void InitialiseQuestions()
        {
            _questions = new List<QuizQuestion>
            {
                new QuizQuestion
                {
                    Question     = "What should you do if you receive an email asking for your password?",
                    Options      = new List<string> { "Reply with your password", "Delete the email", "Report it as phishing", "Ignore it" },
                    CorrectIndex = 2,
                    Explanation  = "Reporting phishing emails helps protect you and others. Legitimate organisations never ask for passwords via email."
                },
                new QuizQuestion
                {
                    Question     = "True or False: Using the same password for multiple accounts is safe as long as it is strong.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "False. If one account is breached, all accounts with the same password are at risk."
                },
                new QuizQuestion
                {
                    Question     = "What does 2FA stand for?",
                    Options      = new List<string> { "Two-Factor Authentication", "Two-File Access", "Triple Firewall Activation", "Timed Frequency Access" },
                    CorrectIndex = 0,
                    Explanation  = "Two-Factor Authentication adds a second layer of security beyond just your password."
                },
                new QuizQuestion
                {
                    Question     = "Which of the following is a sign of a phishing website?",
                    Options      = new List<string> { "HTTPS in the URL", "A padlock icon", "Spelling errors and urgent warnings", "A privacy policy page" },
                    CorrectIndex = 2,
                    Explanation  = "Phishing sites often use urgent language and contain spelling or grammar errors to trick users."
                },
                new QuizQuestion
                {
                    Question     = "True or False: Public Wi-Fi networks are generally safe for online banking.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "False. Public Wi-Fi is unsecured and attackers can intercept your data. Use a VPN or mobile data for banking."
                },
                new QuizQuestion
                {
                    Question     = "What is social engineering?",
                    Options      = new List<string> { "Building social media apps", "Manipulating people into revealing confidential info", "Engineering social networks", "Hacking social media accounts" },
                    CorrectIndex = 1,
                    Explanation  = "Social engineering exploits human psychology rather than technical vulnerabilities to gain access to systems or data."
                },
                new QuizQuestion
                {
                    Question     = "Which password is the strongest?",
                    Options      = new List<string> { "password123", "John1990", "Tr@ff1c!L1ght#99", "qwerty" },
                    CorrectIndex = 2,
                    Explanation  = "A strong password uses a mix of upper/lowercase letters, numbers, and symbols with no personal information."
                },
                new QuizQuestion
                {
                    Question     = "True or False: Antivirus software alone is enough to protect you from all cyber threats.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "False. Antivirus is one layer of defence. You also need strong passwords, 2FA, safe browsing habits, and regular updates."
                },
                new QuizQuestion
                {
                    Question     = "What is ransomware?",
                    Options      = new List<string> { "Software that speeds up your PC", "Malware that encrypts files and demands payment", "A type of firewall", "An email spam filter" },
                    CorrectIndex = 1,
                    Explanation  = "Ransomware encrypts your files and demands payment for the decryption key. Regular backups are your best defence."
                },
                new QuizQuestion
                {
                    Question     = "Which of the following best protects your privacy on social media?",
                    Options      = new List<string> { "Accepting all friend requests", "Making your profile public", "Regularly reviewing your privacy settings", "Sharing your location always" },
                    CorrectIndex = 2,
                    Explanation  = "Regularly reviewing privacy settings limits what strangers can see and reduces your risk of targeted attacks."
                },
                new QuizQuestion
                {
                    Question     = "True or False: You should click links in SMS messages from unknown numbers to verify them.",
                    Options      = new List<string> { "True", "False" },
                    CorrectIndex = 1,
                    Explanation  = "False. SMS phishing (smishing) uses fake links to steal information. Go directly to the official website instead."
                },
                new QuizQuestion
                {
                    Question     = "What is the safest way to store your passwords?",
                    Options      = new List<string> { "Write them in a notebook", "Save them in a browser only", "Use a reputable password manager", "Use the same password everywhere" },
                    CorrectIndex = 2,
                    Explanation  = "A reputable password manager generates and stores unique, strong passwords securely for all your accounts."
                }
            };
        }

        // ── ACTIVITY LOG ─────────────────────────────────────────────────

        private void RefreshLog(int count)
        {
            LogPanel.Children.Clear();
            var entries = _showingAll ? ActivityLog.GetAll() : ActivityLog.GetRecent(count);

            TxtLogSubtitle.Text = _showingAll
                ? $"Showing all {ActivityLog.TotalCount} actions"
                : $"Showing last {Math.Min(count, ActivityLog.TotalCount)} actions";
            TxtLogTotal.Text = $"Total actions recorded: {ActivityLog.TotalCount}";

            if (entries.Count == 0)
            {
                LogPanel.Children.Add(new TextBlock
                {
                    Text = "No activity recorded yet. Start chatting, add tasks, or take the quiz!",
                    Foreground = Brush(0x8B, 0x94, 0x9E),
                    FontSize = 14,
                    Margin = new Thickness(0, 20, 0, 0),
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                return;
            }

            for (int i = 0; i < entries.Count; i++)
            {
                var row = new Border
                {
                    Background = i % 2 == 0 ? Brush(0x16, 0x1B, 0x22) : Brush(0x0D, 0x11, 0x17),
                    BorderBrush = Brush(0x30, 0x36, 0x3D),
                    BorderThickness = new Thickness(0, 0, 0, 1),
                    Padding = new Thickness(16, 10, 16, 10)
                };
                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                sp.Children.Add(new TextBlock
                {
                    Text = $"{i + 1}.",
                    FontSize = 12,
                    Foreground = Brush(0x58, 0xA6, 0xFF),
                    Margin = new Thickness(0, 0, 12, 0),
                    Width = 28,
                    VerticalAlignment = VerticalAlignment.Center
                });
                sp.Children.Add(new TextBlock
                {
                    Text = entries[i],
                    FontSize = 12,
                    Foreground = Brush(0xE6, 0xED, 0xF3),
                    TextWrapping = TextWrapping.Wrap,
                    VerticalAlignment = VerticalAlignment.Center
                });
                row.Child = sp;
                LogPanel.Children.Add(row);
            }
        }

        private void BtnShowMoreLog_Click(object sender, RoutedEventArgs e)
        {
            _showingAll = !_showingAll;
            ((Button)sender).Content = _showingAll ? "Show Less" : "Show More";
            RefreshLog(10);
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            // ActivityLog is static so we re-initialise it
            var result = MessageBox.Show("Clear all activity log entries?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                RefreshLog(10);
        }

        // ── Chat bubble rendering ─────────────────────────────────────────

        private void AddUserMessage(string text)
        {
            var container = new Grid { Margin = new Thickness(60, 4, 0, 4) };
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            container.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var bubble = new Border
            {
                Background = Brush(0x1F, 0x3A, 0x5F),
                CornerRadius = new CornerRadius(16, 16, 4, 16),
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 520,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            Grid.SetColumn(bubble, 1);
            bubble.Child = new TextBlock
            {
                Text = text,
                Foreground = Brush(0xE6, 0xED, 0xF3),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };
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
                Background = Brush(0x00, 0xFF, 0x9C),
                Margin = new Thickness(0, 0, 10, 0),
                VerticalAlignment = VerticalAlignment.Top,
                Child = new TextBlock
                {
                    Text = "🛡",
                    FontSize = 16,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                }
            };
            Grid.SetColumn(avatar, 0);

            var bubble = new Border
            {
                Background = Brush(0x1A, 0x23, 0x32),
                CornerRadius = new CornerRadius(4, 16, 16, 16),
                Padding = new Thickness(14, 10, 14, 10),
                MaxWidth = 580,
                HorizontalAlignment = HorizontalAlignment.Left,
                BorderBrush = Brush(0x30, 0x36, 0x3D),
                BorderThickness = new Thickness(1),
                Child = BuildRichText(text)
            };

            var stack = new StackPanel();
            stack.Children.Add(bubble);
            stack.Children.Add(new TextBlock
            {
                Text = DateTime.Now.ToString("HH:mm"),
                FontSize = 10,
                Foreground = Brush(0x8B, 0x94, 0x9E),
                Margin = new Thickness(4, 2, 0, 0)
            });
            Grid.SetColumn(stack, 1);

            container.Children.Add(avatar);
            container.Children.Add(stack);
            ChatPanel.Children.Add(container);
            ScrollToBottom();
        }

        private TextBlock BuildRichText(string text)
        {
            var tb = new TextBlock
            {
                Foreground = Brush(0xE6, 0xED, 0xF3),
                FontSize = 13,
                TextWrapping = TextWrapping.Wrap,
                LineHeight = 20
            };
            var lines = text.Split('\n');
            for (int l = 0; l < lines.Length; l++)
            {
                if (l > 0) tb.Inlines.Add(new LineBreak());
                var parts = lines[l].Split(new[] { "**" }, StringSplitOptions.None);
                bool bold = false;
                foreach (var part in parts)
                {
                    tb.Inlines.Add(bold
                        ? new Run(part) { FontWeight = FontWeights.Bold, Foreground = Brush(0x00, 0xFF, 0x9C) }
                        : new Run(part));
                    bold = !bold;
                }
            }
            return tb;
        }

        private void ScrollToBottom()
        {
            ChatScrollViewer.UpdateLayout();
            ChatScrollViewer.ScrollToEnd();
        }

        // ── Memory panel ──────────────────────────────────────────────────

        private void UpdateMemoryPanel()
        {
            string name = _bot.GetUserName();
            string topic = _bot.GetFavouriteTopic();
            TxtMemoryName.Text = string.IsNullOrEmpty(name) ? "Not set" : name;
            TxtMemoryTopic.Text = string.IsNullOrEmpty(topic) ? "Not set" : topic;
            if (!string.IsNullOrEmpty(name))
                TxtGreeting.Text = $"Hello, {name}! Protecting South Africa, one tip at a time.";
        }

        // ── Utility ───────────────────────────────────────────────────────

        private static SolidColorBrush Brush(byte r, byte g, byte b)
            => new SolidColorBrush(Color.FromRgb(r, g, b));
    }
}
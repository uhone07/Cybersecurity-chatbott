using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CybersecurityChatbot.Data;

namespace CybersecurityChatbot.Chat
{
    /// <summary>
    /// Part 3 ChatBot — extends Part 2 with NLP simulation.
    /// NLP detects intent (add task, start quiz, show log, set reminder)
    /// from naturally worded user input using keyword + regex matching.
    /// </summary>
    public class ChatBot
    {
        // ── Memory ────────────────────────────────────────────────────────
        private string _userName = string.Empty;
        private string _favouriteTopic = string.Empty;
        private string _lastTopic = string.Empty;
        private int _followUpCount = 0;

        private readonly Random _rng = new Random();

        // ── Delegates for cross-tab NLP actions ──────────────────────────
        // The MainWindow subscribes to these so ChatBot can trigger
        // tab switches and actions without directly referencing the UI.
        public Action? OnNavigateToTasks { get; set; }
        public Action? OnNavigateToQuiz { get; set; }
        public Action? OnShowActivityLog { get; set; }
        public Action<string, string, DateTime?>? OnAddTaskRequested { get; set; }

        // ── Keyword responses (same as Part 2) ───────────────────────────
        private readonly Dictionary<string, List<string>> _keywordResponses =
            new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
            {
                ["password"] = new List<string>
            {
                "Use a strong password with at least 12 characters combining upper and lowercase letters, numbers, and symbols.",
                "Never reuse passwords across different sites — if one account is breached all reused accounts are at risk.",
                "Consider using a reputable password manager like Bitwarden or KeePass to generate and store unique passwords.",
                "Enable two-factor authentication (2FA) wherever possible — it adds a critical second layer beyond just your password.",
                "Avoid using personal details like birthdays or pet names in your passwords — these are easily guessed by attackers."
            },
                ["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                "Always hover over links before clicking — verify the URL matches the legitimate website.",
                "Look for red flags: urgency like Act now!, grammar mistakes, and suspicious sender addresses.",
                "Legitimate companies will never ask for your password via email or SMS.",
                "When in doubt go directly to the company's official website instead of clicking any link in the email."
            },
                ["privacy"] = new List<string>
            {
                "Review your social media privacy settings regularly — limit what strangers can see about you.",
                "Be mindful of what personal information you share online; even small details can be used in social engineering attacks.",
                "Use a VPN on public Wi-Fi networks to protect your data from eavesdroppers.",
                "Read app permissions carefully before granting access — many apps request more data than they need.",
                "Enable end-to-end encryption in your messaging apps for private conversations."
            },
                ["scam"] = new List<string>
            {
                "If an offer seems too good to be true it probably is. Verify before you trust.",
                "Romance scams are on the rise in South Africa — never send money to someone you have not met in person.",
                "Verify any unexpected prize winnings directly with the official organisation — do not pay any fees upfront.",
                "SARS will never contact you via SMS asking for payment. When in doubt call SARS directly.",
                "Report suspected scams to the South African Fraud Prevention Service (SAFPS) at 0860 101 248."
            },
                ["malware"] = new List<string>
            {
                "Keep your operating system and software updated — patches often fix security vulnerabilities exploited by malware.",
                "Only download software from official or trusted sources and avoid cracked or pirated software.",
                "Use a reputable antivirus solution and run regular scans on your device.",
                "Be cautious with USB drives from unknown sources — they can introduce malware automatically.",
                "Ransomware can encrypt your files; regular backups offline or in the cloud are your best defence."
            },
                ["browsing"] = new List<string>
            {
                "Look for HTTPS and a padlock icon in your browser's address bar before entering any personal data.",
                "Use a privacy-focused browser extension like uBlock Origin to block malicious ads and trackers.",
                "Avoid using public computers for sensitive tasks like online banking.",
                "Clear your browser cache and cookies periodically to reduce tracking.",
                "Be cautious of browser pop-ups claiming your device is infected — these are often scareware."
            },
                ["2fa"] = new List<string>
            {
                "Two-factor authentication prevents attackers from accessing your account even if they have your password.",
                "Prefer an authenticator app like Google Authenticator or Authy over SMS-based 2FA which can be intercepted.",
                "Enable 2FA on all critical accounts: email, banking, and social media first.",
                "Store your 2FA backup codes in a safe offline location in case you lose access to your device.",
                "Hardware security keys like YubiKey offer the strongest form of two-factor authentication available."
            },
                ["social engineering"] = new List<string>
            {
                "Social engineering manipulates people rather than systems — always verify identities before sharing information.",
                "Be suspicious of unsolicited phone calls claiming to be IT support or your bank.",
                "Attackers may use information from your public social media profiles to appear legitimate.",
                "Establish a verbal code word with family members to verify identity during emergency calls.",
                "Always confirm requests for sensitive data through a separate trusted communication channel."
            }
            };

        // ── Sentiment map ─────────────────────────────────────────────────
        private readonly Dictionary<string, string> _sentimentPrefixes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["worried"] = "It's completely understandable to feel that way — cybersecurity threats are real and growing. Let me help put your mind at ease. ",
                ["scared"] = "Your concern is valid and actually the first step toward being safer online. Here's something that will help: ",
                ["anxious"] = "Take a deep breath — you're already taking the right step by learning. Here's a practical tip: ",
                ["frustrated"] = "I understand the frustration — cybersecurity can feel overwhelming. Let's break it down simply. ",
                ["confused"] = "No worries at all — these concepts can be tricky. Let me explain it as clearly as possible. ",
                ["curious"] = "Great curiosity — that's exactly the mindset that keeps you safe! Here's something interesting: ",
                ["nervous"] = "Being cautious is smart! Let me share something that should make you feel more confident: ",
                ["overwhelmed"] = "Let's take it one step at a time. Here's the most important thing to know right now: "
            };

        // ── Follow-up triggers ────────────────────────────────────────────
        private readonly List<string> _followUpTriggers = new List<string>
        {
            "tell me more", "more", "another tip", "give me another",
            "explain more", "continue", "go on", "more info",
            "more details", "keep going", "what else", "next", "another one"
        };

        // ── General small-talk ────────────────────────────────────────────
        private readonly Dictionary<string, string> _generalResponses =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["how are you"] = "I'm running at full capacity and ready to help you stay safe online!",
                ["what is your purpose"] = "I'm your Cybersecurity Awareness Assistant — here to educate you on staying safe in the digital world.",
                ["what can i ask you about"] = "You can ask me about:\n  🔑 Password safety\n  🎣 Phishing\n  🛡️ Privacy\n  💰 Scams\n  🦠 Malware\n  🔒 2FA\n  🌐 Safe browsing\n  🎭 Social engineering\n\nOr try:\n  📋 'show my tasks'\n  🎮 'start quiz'\n  📜 'show activity log'",
                ["hello"] = "Hello! I'm your Cybersecurity Awareness Assistant. What topic would you like to explore?",
                ["hi"] = "Hi! Great to see you. Ask me anything about staying safe online!",
                ["thank you"] = "You're very welcome! Staying informed is your best defence.",
                ["thanks"] = "Happy to help! Any other questions?",
                ["bye"] = "Stay safe online! Remember: think before you click. Goodbye! 👋",
                ["goodbye"] = "Take care and stay cyber-safe! Come back anytime. 👋",
                ["who are you"] = "I'm the Cybersecurity Awareness Bot — your digital safety assistant.",
                ["help"] = "Type any cybersecurity topic, or try 'start quiz', 'show my tasks', or 'show activity log'."
            };

        // ── NLP intent patterns ───────────────────────────────────────────
        // Each pattern maps to an intent string
        private readonly List<(Regex Pattern, string Intent)> _nlpPatterns =
            new List<(Regex, string)>
        {
            (new Regex(@"\b(add|create|new|set up)\b.*(task|reminder|todo)\b",          RegexOptions.IgnoreCase), "add_task"),
            (new Regex(@"\b(remind me|set.?a.?reminder|reminder.?for)\b",               RegexOptions.IgnoreCase), "add_task"),
            (new Regex(@"\b(show|view|list|see|display|what).*(task|todo|reminder)\b",  RegexOptions.IgnoreCase), "view_tasks"),
            (new Regex(@"\b(start|play|begin|launch|open).*(quiz|game|test)\b",         RegexOptions.IgnoreCase), "start_quiz"),
            (new Regex(@"\b(show|view|see|display|what).*(log|history|done|actions)\b", RegexOptions.IgnoreCase), "show_log"),
            (new Regex(@"\bwhat.*(have you done|did you do)\b",                         RegexOptions.IgnoreCase), "show_log"),
            (new Regex(@"\b(enable|setup|set up|activate)\s+2fa\b",                     RegexOptions.IgnoreCase), "suggest_2fa_task"),
            (new Regex(@"\b(update|change).*(password)\b",                              RegexOptions.IgnoreCase), "suggest_password_task"),
            (new Regex(@"\b(review|check).*(privacy|settings)\b",                       RegexOptions.IgnoreCase), "suggest_privacy_task"),
        };

        // ─────────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────────

        public void SetUserName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _userName = name.Trim();
        }

        public string GetUserName() => _userName;
        public string GetFavouriteTopic() => _favouriteTopic;

        public string GetResponse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "I didn't catch that. Could you please type your question?";

            string raw = userInput.Trim();
            string input = raw.ToLower();

            // 1. NLP intent detection — checked FIRST so natural phrasing works
            string? nlpResponse = TryHandleNlpIntent(raw);
            if (nlpResponse != null)
                return nlpResponse;

            // 2. Sentiment detection
            string sentimentPrefix = DetectSentiment(input, out string cleaned);
            input = cleaned;

            // 3. Follow-up detection
            if (IsFollowUp(input) && !string.IsNullOrEmpty(_lastTopic))
                return sentimentPrefix + GetNextTipForTopic(_lastTopic);

            // 4. Memory: "I'm interested in X"
            string? memoryResponse = TrySetFavouriteTopic(input);
            if (memoryResponse != null)
                return sentimentPrefix + memoryResponse;

            // 5. General small-talk
            foreach (var kv in _generalResponses)
                if (input.Contains(kv.Key))
                    return sentimentPrefix + PersonaliseResponse(kv.Value);

            // 6. Cybersecurity keyword
            string? keyword = FindKeyword(input);
            if (keyword != null)
            {
                _lastTopic = keyword;
                _followUpCount = 0;
                string tip = GetRandomTip(keyword);
                string recall = BuildRecallLine(keyword);
                ActivityLog.Add($"Keyword response given for topic: {keyword}");
                return sentimentPrefix + recall + tip +
                       "\n\n💡 Type 'tell me more' for another tip on this topic.";
            }

            // 7. Fallback
            return "I'm not sure I understand that. Could you rephrase?\n" +
                   "Try: password, phishing, privacy, scam, malware, 2fa, browsing, " +
                   "social engineering, 'start quiz', 'show my tasks', or 'show activity log'.";
        }

        // ─────────────────────────────────────────────────────────────────
        //  NLP Intent Handler
        // ─────────────────────────────────────────────────────────────────

        private string? TryHandleNlpIntent(string raw)
        {
            foreach (var (pattern, intent) in _nlpPatterns)
            {
                if (!pattern.IsMatch(raw)) continue;

                switch (intent)
                {
                    case "add_task":
                        // Try to extract a task title from the input
                        string title = ExtractTaskTitle(raw);
                        DateTime? reminder = ExtractReminderDate(raw);
                        OnAddTaskRequested?.Invoke(
                            title,
                            $"Task created via chat: {title}",
                            reminder);
                        ActivityLog.Add($"NLP: Task added via chat — '{title}'" +
                            (reminder.HasValue ? $" (reminder: {reminder.Value:dd MMM yyyy})" : ""));
                        string reminderMsg = reminder.HasValue
                            ? $" I've set a reminder for {reminder.Value:dd MMM yyyy}."
                            : " No reminder was set — you can add one in the Tasks tab.";
                        return $"✅ Task added: '{title}'.{reminderMsg}\n\n" +
                               "Switch to the 📋 Tasks tab to view and manage all your tasks.";

                    case "view_tasks":
                        OnNavigateToTasks?.Invoke();
                        ActivityLog.Add("NLP: User navigated to Tasks tab via chat");
                        return "Opening your 📋 Tasks tab now! You can view, complete, or delete tasks there.";

                    case "start_quiz":
                        OnNavigateToQuiz?.Invoke();
                        ActivityLog.Add("NLP: User started quiz via chat command");
                        return "Opening the 🎮 Quiz tab now! Test your cybersecurity knowledge!";

                    case "show_log":
                        OnShowActivityLog?.Invoke();
                        ActivityLog.Add("NLP: User requested activity log");
                        var recent = ActivityLog.GetRecent(10);
                        if (recent.Count == 0)
                            return "No activity recorded yet. Start chatting, add tasks, or take the quiz!";
                        return "📜 Here are your recent actions:\n\n" +
                               string.Join("\n", recent.Select((e, i) => $"{i + 1}. {e}"));

                    case "suggest_2fa_task":
                        OnAddTaskRequested?.Invoke(
                            "Enable two-factor authentication",
                            "Set up 2FA on your most important accounts: email, banking, social media.",
                            DateTime.Now.AddDays(3));
                        ActivityLog.Add("NLP: Suggested 2FA task added automatically");
                        return "✅ I've added a task: 'Enable two-factor authentication' with a reminder in 3 days.\n\n" +
                               "Check the 📋 Tasks tab to manage it.";

                    case "suggest_password_task":
                        OnAddTaskRequested?.Invoke(
                            "Update my passwords",
                            "Review and update passwords for all important accounts. Use a password manager.",
                            DateTime.Now.AddDays(7));
                        ActivityLog.Add("NLP: Suggested password update task added automatically");
                        return "✅ I've added a task: 'Update my passwords' with a reminder in 7 days.\n\n" +
                               "Check the 📋 Tasks tab to manage it.";

                    case "suggest_privacy_task":
                        OnAddTaskRequested?.Invoke(
                            "Review privacy settings",
                            "Check privacy settings on social media and app accounts to limit data exposure.",
                            DateTime.Now.AddDays(5));
                        ActivityLog.Add("NLP: Suggested privacy review task added automatically");
                        return "✅ I've added a task: 'Review privacy settings' with a reminder in 5 days.\n\n" +
                               "Check the 📋 Tasks tab to manage it.";
                }
            }
            return null;
        }

        // ─────────────────────────────────────────────────────────────────
        //  NLP Helpers
        // ─────────────────────────────────────────────────────────────────

        /// <summary>
        /// Extracts a task title from natural language.
        /// e.g. "Add a task to enable 2FA" → "Enable 2FA"
        /// </summary>
        private string ExtractTaskTitle(string input)
        {
            // Remove common command prefixes
            var prefixes = new[]
            {
                "add a task to", "add task to", "create a task to", "create task to",
                "add a reminder to", "set a reminder to", "remind me to",
                "add a task", "create a task", "new task"
            };
            string lower = input.ToLower();
            foreach (var prefix in prefixes)
            {
                if (lower.Contains(prefix))
                {
                    int idx = lower.IndexOf(prefix) + prefix.Length;
                    string title = input.Substring(idx).Trim().TrimEnd('.', '!', '?');
                    if (!string.IsNullOrWhiteSpace(title))
                        return char.ToUpper(title[0]) + title.Substring(1);
                }
            }
            // Fallback: use the whole input trimmed
            return input.Length > 60 ? input.Substring(0, 60) + "..." : input;
        }

        /// <summary>
        /// Extracts a reminder date from phrases like:
        /// "in 3 days", "tomorrow", "in 1 week", "in 2 weeks"
        /// </summary>
        private DateTime? ExtractReminderDate(string input)
        {
            string lower = input.ToLower();

            if (lower.Contains("tomorrow"))
                return DateTime.Now.AddDays(1);

            var daysMatch = Regex.Match(lower, @"in\s+(\d+)\s+day");
            if (daysMatch.Success && int.TryParse(daysMatch.Groups[1].Value, out int days))
                return DateTime.Now.AddDays(days);

            var weeksMatch = Regex.Match(lower, @"in\s+(\d+)\s+week");
            if (weeksMatch.Success && int.TryParse(weeksMatch.Groups[1].Value, out int weeks))
                return DateTime.Now.AddDays(weeks * 7);

            if (lower.Contains("next week"))
                return DateTime.Now.AddDays(7);

            return null;
        }

        // ─────────────────────────────────────────────────────────────────
        //  Part 2 helpers (unchanged)
        // ─────────────────────────────────────────────────────────────────

        private string DetectSentiment(string input, out string cleaned)
        {
            cleaned = input;
            foreach (var kv in _sentimentPrefixes)
            {
                if (input.Contains(kv.Key))
                {
                    cleaned = input.Replace(kv.Key, "").Trim();
                    return kv.Value;
                }
            }
            return string.Empty;
        }

        private bool IsFollowUp(string input)
            => _followUpTriggers.Any(t => input.Contains(t));

        private string GetNextTipForTopic(string keyword)
        {
            if (!_keywordResponses.ContainsKey(keyword))
                return "I don't have more details on that topic right now.";
            var tips = _keywordResponses[keyword];
            _followUpCount = (_followUpCount + 1) % tips.Count;
            return tips[_followUpCount] + "\n\n💡 Type 'tell me more' for yet another tip!";
        }

        private string? TrySetFavouriteTopic(string input)
        {
            var phrases = new[] { "interested in", "like to learn about",
                                  "want to know about", "favourite topic is", "care about" };
            foreach (var phrase in phrases)
            {
                if (!input.Contains(phrase)) continue;
                int idx = input.IndexOf(phrase, StringComparison.OrdinalIgnoreCase) + phrase.Length;
                string possible = input.Substring(idx).Trim().TrimEnd('.', '!', '?');
                string? matched = FindKeywordInText(possible);
                if (matched != null)
                {
                    _favouriteTopic = matched;
                    string name = string.IsNullOrEmpty(_userName) ? "there" : _userName;
                    ActivityLog.Add($"Memory updated: favourite topic set to '{matched}'");
                    return $"Great, {name}! I'll remember that you're interested in {matched}.\n\n" +
                           $"Here's a tip to get you started:\n{GetRandomTip(matched)}\n\n" +
                           $"💡 Type 'tell me more' for additional {matched} tips anytime.";
                }
                else
                {
                    _favouriteTopic = possible;
                    return $"Got it! I'll remember you're interested in '{possible}'.";
                }
            }
            return null;
        }

        private string? FindKeyword(string input)
        {
            if (input.Contains("social engineering")) return "social engineering";
            if (input.Contains("safe browsing") || input.Contains("browsing")) return "browsing";
            if (input.Contains("2fa") || input.Contains("two factor") ||
                input.Contains("two-factor") || input.Contains("multi factor")) return "2fa";
            foreach (var key in _keywordResponses.Keys)
                if (input.Contains(key)) return key;
            return null;
        }

        private string? FindKeywordInText(string text)
        {
            if (text.Contains("social engineering")) return "social engineering";
            if (text.Contains("browsing")) return "browsing";
            if (text.Contains("2fa") || text.Contains("two factor")) return "2fa";
            foreach (var key in _keywordResponses.Keys)
                if (text.Contains(key)) return key;
            return null;
        }

        private string GetRandomTip(string keyword)
        {
            var tips = _keywordResponses[keyword];
            return tips[_rng.Next(tips.Count)];
        }

        private string BuildRecallLine(string keyword)
        {
            if (!string.IsNullOrEmpty(_favouriteTopic) &&
                keyword.Equals(_favouriteTopic, StringComparison.OrdinalIgnoreCase))
            {
                string name = string.IsNullOrEmpty(_userName) ? "you" : _userName;
                return $"As someone interested in {_favouriteTopic}, {name}, here's a relevant tip:\n\n";
            }
            return string.Empty;
        }

        private string PersonaliseResponse(string response)
        {
            if (!string.IsNullOrEmpty(_userName) && !response.Contains(_userName))
                return $"{_userName}, {char.ToLower(response[0])}{response.Substring(1)}";
            return response;
        }
    }
}
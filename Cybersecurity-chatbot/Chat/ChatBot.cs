using System;
using System.Collections.Generic;
using System.Linq;

namespace CybersecurityChatbot.Chat
{
    /// <summary>
    /// Core chatbot logic for Part 2:
    /// - Keyword recognition (password, phishing, privacy, scam, malware, etc.)
    /// - Random responses for varied interaction
    /// - Conversation flow (follow-up: "tell me more", "explain more")
    /// - Memory and recall (name, favourite topic)
    /// - Sentiment detection (worried, curious, frustrated, confused)
    /// - Error handling for unknown inputs
    /// </summary>
    public class ChatBot
    {
        // ── Memory ────────────────────────────────────────────────────────────
        private string _userName = string.Empty;
        private string _favouriteTopic = string.Empty;
        private string _lastTopic = string.Empty;          // tracks current topic for follow-ups
        private int _followUpCount = 0;                    // how many tips given on current topic

        // ── Random source ────────────────────────────────────────────────────
        private readonly Random _rng = new Random();

        // ── Keyword → multiple responses (for random selection) ──────────────
        private readonly Dictionary<string, List<string>> _keywordResponses = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            ["password"] = new List<string>
            {
                "Use a strong password with at least 12 characters, combining upper and lowercase letters, numbers, and symbols.",
                "Never reuse passwords across different sites — if one account is breached, all reused accounts are at risk.",
                "Consider using a reputable password manager like Bitwarden or KeePass to generate and store unique passwords.",
                "Enable two-factor authentication (2FA) wherever possible — it adds a critical second layer beyond just your password.",
                "Avoid using personal details (birthdays, pet names) in your passwords — these are easily guessed by attackers."
            },
            ["phishing"] = new List<string>
            {
                "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organisations.",
                "Always hover over links before clicking — verify the URL matches the legitimate website.",
                "Look for red flags: urgency ('Act now!'), grammar mistakes, and suspicious sender addresses.",
                "Legitimate companies will never ask for your password via email or SMS.",
                "When in doubt, go directly to the company's official website instead of clicking any link in the email."
            },
            ["privacy"] = new List<string>
            {
                "Review your social media privacy settings regularly — limit what strangers can see about you.",
                "Be mindful of what personal information you share online; even small details can be used in social engineering attacks.",
                "Use a VPN on public Wi-Fi networks to protect your data from eavesdroppers.",
                "Read app permissions carefully before granting access — many apps request more data than they need.",
                "Enable end-to-end encryption in your messaging apps (e.g., Signal or WhatsApp) for private conversations."
            },
            ["scam"] = new List<string>
            {
                "If an offer seems too good to be true, it probably is. Verify before you trust.",
                "Romance scams are on the rise in South Africa — never send money to someone you haven't met in person.",
                "Verify any unexpected 'prize winnings' directly with the official organisation — do not pay any fees upfront.",
                "SARS will never contact you via SMS asking for payment. When in doubt, call SARS directly.",
                "Report suspected scams to the South African Fraud Prevention Service (SAFPS) at 0860 101 248."
            },
            ["malware"] = new List<string>
            {
                "Keep your operating system and software updated — patches often fix security vulnerabilities exploited by malware.",
                "Only download software from official or trusted sources, and avoid cracked/pirated software.",
                "Use a reputable antivirus solution and run regular scans on your device.",
                "Be cautious with USB drives from unknown sources — they can introduce malware automatically.",
                "Ransomware can encrypt your files for payment; regular backups (offline or cloud) are your best defence."
            },
            ["browsing"] = new List<string>
            {
                "Look for 'HTTPS' and a padlock icon in your browser's address bar before entering any personal data.",
                "Use a privacy-focused browser extension like uBlock Origin to block malicious ads and trackers.",
                "Avoid using public computers for sensitive tasks like online banking.",
                "Clear your browser cache and cookies periodically to reduce tracking.",
                "Be cautious of browser pop-ups claiming your device is infected — these are often scareware."
            },
            ["2fa"] = new List<string>
            {
                "Two-factor authentication (2FA) prevents attackers from accessing your account even if they have your password.",
                "Prefer an authenticator app (e.g., Google Authenticator, Authy) over SMS-based 2FA, which can be intercepted.",
                "Enable 2FA on all critical accounts: email, banking, and social media first.",
                "Store your 2FA backup codes in a safe, offline location in case you lose access to your device.",
                "Hardware security keys (e.g., YubiKey) offer the strongest form of two-factor authentication available."
            },
            ["social engineering"] = new List<string>
            {
                "Social engineering manipulates people rather than systems — always verify identities before sharing information.",
                "Be suspicious of unsolicited phone calls claiming to be IT support or your bank.",
                "Attackers may use information from your public social media profiles to appear legitimate — limit what you share.",
                "Establish a verbal code word with family members to verify identity during emergency calls.",
                "Always confirm requests for sensitive data through a separate, trusted communication channel."
            }
        };

        // ── Sentiment keywords and their adjusted responses ───────────────────
        private readonly Dictionary<string, string> _sentimentPrefixes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
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

        // ── Follow-up trigger phrases ─────────────────────────────────────────
        private readonly List<string> _followUpTriggers = new List<string>
        {
            "tell me more", "more", "another tip", "give me another", "explain more",
            "continue", "go on", "more info", "more details", "keep going", "and then",
            "what else", "next", "another one"
        };

        // ── General / small-talk responses ───────────────────────────────────
        private readonly Dictionary<string, string> _generalResponses = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["how are you"] = "I'm running at full capacity and ready to help you stay safe online! How can I assist you today?",
            ["what is your purpose"] = "I'm your Cybersecurity Awareness Assistant! I'm here to educate you on staying safe in the digital world — covering topics like passwords, phishing, scams, privacy, and more.",
            ["what can i ask you about"] = "You can ask me about:\n  🔑 Password safety\n  🎣 Phishing attacks\n  🛡️ Privacy protection\n  🦠 Malware & viruses\n  🔒 Two-factor authentication (2FA)\n  🌐 Safe browsing\n  🎭 Social engineering\n  💰 Online scams\n\nJust type any topic and I'll guide you!",
            ["hello"] = "Hello there! I'm your Cybersecurity Awareness Assistant. What cybersecurity topic would you like to explore today?",
            ["hi"] = "Hi! Great to see you. Ask me anything about staying safe online!",
            ["thank you"] = "You're very welcome! Staying informed is your best defence. Is there anything else you'd like to know?",
            ["thanks"] = "Happy to help! Cybersecurity awareness is key to staying safe. Any other questions?",
            ["bye"] = "Stay safe online! Remember: think before you click. Goodbye! 👋",
            ["goodbye"] = "Take care and stay cyber-safe! Come back anytime you have questions. 👋",
            ["who are you"] = "I'm the Cybersecurity Awareness Bot — a virtual assistant designed to help South African citizens stay safe in the digital world.",
            ["help"] = "Type any cybersecurity topic to get started — for example: 'password', 'phishing', 'scam', 'privacy', 'malware', '2fa', or 'safe browsing'."
        };

        // ─────────────────────────────────────────────────────────────────────
        //  Public API
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>Sets or updates the user's remembered name.</summary>
        public void SetUserName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                _userName = name.Trim();
        }

        /// <summary>Returns the remembered user name (empty if not yet set).</summary>
        public string GetUserName() => _userName;

        /// <summary>Returns the remembered favourite topic (empty if not yet set).</summary>
        public string GetFavouriteTopic() => _favouriteTopic;

        /// <summary>
        /// Main entry point: takes raw user input and returns the chatbot's response.
        /// </summary>
        public string GetResponse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "I didn't catch that. Could you please type your question?";

            string input = userInput.Trim().ToLower();

            // 1. Detect & strip sentiment prefix, then build an empathetic opener
            string sentimentPrefix = DetectSentiment(input, out string cleanedInput);
            input = cleanedInput;

            // 2. Check for follow-up requests
            if (IsFollowUp(input) && !string.IsNullOrEmpty(_lastTopic))
                return sentimentPrefix + GetNextTipForTopic(_lastTopic);

            // 3. Check for memory-setting phrases ("I'm interested in …")
            string memoryResponse = TrySetFavouriteTopic(input);
            if (memoryResponse != null)
                return sentimentPrefix + memoryResponse;

            // 4. Check general / small-talk responses (exact & contains)
            foreach (var kv in _generalResponses)
            {
                if (input.Contains(kv.Key))
                    return sentimentPrefix + PersonaliseResponse(kv.Value);
            }

            // 5. Keyword recognition — find the best matching cybersecurity keyword
            string matchedKeyword = FindKeyword(input);
            if (matchedKeyword != null)
            {
                _lastTopic = matchedKeyword;
                _followUpCount = 0;
                string tip = GetRandomTip(matchedKeyword);
                string recall = BuildRecallLine(matchedKeyword);
                return sentimentPrefix + recall + tip + "\n\n💡 Type 'tell me more' for another tip on this topic.";
            }

            // 6. Default / fallback response
            _lastTopic = string.Empty;
            return "I'm not sure I understand that. Could you rephrase?\n" +
                   "You can ask about: password, phishing, privacy, scam, malware, 2fa, browsing, or social engineering.";
        }

        // ─────────────────────────────────────────────────────────────────────
        //  Private helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Detects a sentiment keyword at the start of input.
        /// Returns the empathetic prefix and the cleaned input (sentiment word removed).
        /// </summary>
        private string DetectSentiment(string input, out string cleaned)
        {
            cleaned = input;
            foreach (var kv in _sentimentPrefixes)
            {
                if (input.Contains(kv.Key))
                {
                    // Remove the sentiment word so keyword detection still works
                    cleaned = input.Replace(kv.Key, "").Trim();
                    return kv.Value;
                }
            }
            return string.Empty;
        }

        /// <summary>Returns true if the input is a follow-up request.</summary>
        private bool IsFollowUp(string input)
        {
            return _followUpTriggers.Any(trigger => input.Contains(trigger));
        }

        /// <summary>
        /// Cycles through tips for the last topic so the user always gets a new one.
        /// </summary>
        private string GetNextTipForTopic(string keyword)
        {
            if (!_keywordResponses.ContainsKey(keyword))
                return "I don't have more details on that topic right now. Try asking about another cybersecurity topic!";

            var tips = _keywordResponses[keyword];
            _followUpCount = (_followUpCount + 1) % tips.Count;
            return tips[_followUpCount] + "\n\n💡 Type 'tell me more' for yet another tip!";
        }

        /// <summary>
        /// Detects "I'm interested in X" or "my favourite topic is X" and stores it.
        /// </summary>
        private string TrySetFavouriteTopic(string input)
        {
            var interestPhrases = new[] { "interested in", "like to learn about", "want to know about", "favourite topic is", "care about" };

            foreach (var phrase in interestPhrases)
            {
                if (input.Contains(phrase))
                {
                    // Extract the topic after the phrase
                    int idx = input.IndexOf(phrase, StringComparison.OrdinalIgnoreCase) + phrase.Length;
                    string possibleTopic = input.Substring(idx).Trim().TrimEnd('.', '!', '?');

                    // See if it maps to a known keyword
                    string matched = FindKeywordInText(possibleTopic);
                    if (matched != null)
                    {
                        _favouriteTopic = matched;
                        string name = string.IsNullOrEmpty(_userName) ? "there" : _userName;
                        return $"Great, {name}! I'll remember that you're interested in **{matched}**. " +
                               $"It's a crucial part of staying safe online.\n\n" +
                               $"Here's a tip to get you started: {GetRandomTip(matched)}\n\n" +
                               $"💡 Type 'tell me more' for additional {matched} tips anytime.";
                    }
                    else
                    {
                        _favouriteTopic = possibleTopic;
                        return $"Got it! I'll remember you're interested in '{possibleTopic}'. " +
                               "Feel free to ask me anything about cybersecurity!";
                    }
                }
            }
            return null;
        }

        /// <summary>Finds the first matching cybersecurity keyword in the full input string.</summary>
        private string FindKeyword(string input)
        {
            // Check multi-word keywords first
            if (input.Contains("social engineering")) return "social engineering";
            if (input.Contains("safe browsing") || input.Contains("browsing")) return "browsing";
            if (input.Contains("2fa") || input.Contains("two factor") || input.Contains("two-factor") || input.Contains("multi factor")) return "2fa";

            foreach (var key in _keywordResponses.Keys)
            {
                if (input.Contains(key))
                    return key;
            }
            return null;
        }

        /// <summary>Same as FindKeyword but operates on a substring (for topic extraction).</summary>
        private string FindKeywordInText(string text)
        {
            if (text.Contains("social engineering")) return "social engineering";
            if (text.Contains("browsing")) return "browsing";
            if (text.Contains("2fa") || text.Contains("two factor")) return "2fa";

            foreach (var key in _keywordResponses.Keys)
            {
                if (text.Contains(key))
                    return key;
            }
            return null;
        }

        /// <summary>Returns a random tip for the given keyword.</summary>
        private string GetRandomTip(string keyword)
        {
            var tips = _keywordResponses[keyword];
            return tips[_rng.Next(tips.Count)];
        }

        /// <summary>
        /// If the current topic matches the user's remembered favourite topic,
        /// adds a personalised recall line.
        /// </summary>
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

        /// <summary>Inserts the user's name into generic responses where appropriate.</summary>
        private string PersonaliseResponse(string response)
        {
            if (!string.IsNullOrEmpty(_userName) && !response.Contains(_userName))
                return $"{_userName}, {char.ToLower(response[0])}{response.Substring(1)}";
            return response;
        }
    }
}
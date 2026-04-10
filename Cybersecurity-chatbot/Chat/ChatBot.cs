using System;
using System.Collections.Generic;

namespace CybersecurityChatbot.Chat
{
    
    /// Core chatbot logic. Stores the user's name, manages keyword-based
    /// responses, and handles invalid/unrecognised input gracefully.
   
    public class ChatBot
    {
        private string _userName = "User";

      

        /// Maps cybersecurity keywords to educational responses.
      
        private readonly Dictionary<string, string> _keywordResponses
            = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {
                "password",
                "🔑 Password Safety Tips:\n" +
                "  • Use at least 12 characters mixing letters, numbers, and symbols.\n" +
                "  • Never reuse passwords across different accounts.\n" +
                "  • Consider using a reputable password manager like Bitwarden.\n" +
                "  • Avoid using personal information such as birthdays or names.\n" +
                "  • Change passwords immediately if a breach is suspected."
            },
            {
                "phishing",
                "🎣 How to Spot Phishing Attacks:\n" +
                "  • Be suspicious of urgent emails asking you to 'act now'.\n" +
                "  • Check the sender's email address carefully for slight misspellings.\n" +
                "  • Hover over links before clicking to preview the real destination URL.\n" +
                "  • Legitimate organisations will NEVER ask for your password via email.\n" +
                "  • When in doubt, contact the organisation directly through their official site."
            },
            {
                "browsing",
                "🌐 Safe Browsing Practices:\n" +
                "  • Always verify a site uses HTTPS (look for the padlock icon).\n" +
                "  • Avoid clicking on pop-up advertisements or unknown download prompts.\n" +
                "  • Use a reputable browser with built-in phishing and malware protection.\n" +
                "  • Clear your cookies and cache regularly.\n" +
                "  • Use a VPN when connecting on public Wi-Fi networks."
            },
            {
                "malware",
                "🦠 Protecting Against Malware:\n" +
                "  • Keep your operating system and software updated at all times.\n" +
                "  • Install a trusted antivirus/antimalware program and keep it updated.\n" +
                "  • Never download software from unofficial or untrusted sources.\n" +
                "  • Be cautious of USB drives from unknown sources — they can carry malware.\n" +
                "  • Regularly back up your files to an external drive or cloud storage."
            },
            {
                "two-factor",
                "🔐 Two-Factor Authentication (2FA):\n" +
                "  • 2FA adds a second layer of security beyond just your password.\n" +
                "  • Enable 2FA on all important accounts: email, banking, and social media.\n" +
                "  • Use an authenticator app (e.g. Google Authenticator) rather than SMS.\n" +
                "  • Even if someone steals your password, 2FA keeps your account protected."
            },
            {
                "2fa",
                "🔐 Two-Factor Authentication (2FA):\n" +
                "  • 2FA adds a second layer of security beyond just your password.\n" +
                "  • Enable 2FA on all important accounts: email, banking, and social media.\n" +
                "  • Use an authenticator app (e.g. Google Authenticator) rather than SMS.\n" +
                "  • Even if someone steals your password, 2FA keeps your account protected."
            },
            {
                "social engineering",
                "🎭 Recognising Social Engineering:\n" +
                "  • Social engineers manipulate people into revealing confidential information.\n" +
                "  • Be wary of unsolicited phone calls claiming to be from IT support or your bank.\n" +
                "  • Never share your OTP (one-time password) with anyone — not even bank staff.\n" +
                "  • Verify the identity of anyone requesting sensitive information.\n" +
                "  • Trust your instincts: if something feels wrong, it probably is."
            },
            {
                "privacy",
                "🛡️ Protecting Your Online Privacy:\n" +
                "  • Review the privacy settings on all your social media accounts.\n" +
                "  • Limit the personal information you share publicly online.\n" +
                "  • Read app permissions carefully before installation.\n" +
                "  • Use private/incognito browsing when using shared devices.\n" +
                "  • Opt out of data collection and targeted advertising where possible."
            },
            {
                "scam",
                "⚠️  Common Online Scams to Watch Out For:\n" +
                "  • 'You've won a prize!' — If you didn't enter a competition, you didn't win.\n" +
                "  • Advance-fee fraud: promises of large sums in exchange for an upfront payment.\n" +
                "  • Romance scams: online relationships that lead to requests for money.\n" +
                "  • Fake job offers requiring you to pay a registration fee upfront.\n" +
                "  • Report scams to the South African Police Service (SAPS) or SABRIC."
            },
            {
                "how are you",
                "😊 All systems operational, Agent! I'm fully online and ready\n" +
                "  to help you navigate the world of cybersecurity safely."
            },
            {
                "purpose",
                "🎯 My mission is to educate South African citizens about cybersecurity threats.\n" +
                "  I cover phishing, malware, password safety, privacy, and much more.\n" +
                "  Think of me as your personal digital intelligence assistant!"
            },
            {
                "what can i ask",
                "💬 You can request intel on:\n" +
                "  [+] Password safety\n" +
                "  [+] Phishing attacks\n" +
                "  [+] Safe browsing\n" +
                "  [+] Malware protection\n" +
                "  [+] Two-factor authentication (2FA)\n" +
                "  [+] Social engineering\n" +
                "  [+] Online privacy\n" +
                "  [+] Online scams\n\n" +
                "  Just type naturally — I'll decode your request!"
            },
            {
                "help",
                "💬 You can request intel on:\n" +
                "  [+] Password safety\n" +
                "  [+] Phishing attacks\n" +
                "  [+] Safe browsing\n" +
                "  [+] Malware protection\n" +
                "  [+] Two-factor authentication (2FA)\n" +
                "  [+] Social engineering\n" +
                "  [+] Online privacy\n" +
                "  [+] Online scams\n\n" +
                "  Just type naturally — I'll decode your request!"
            },
            {
                "hello",
                "👋 Agent acknowledged. How can I assist with your cybersecurity briefing today?"
            },
            {
                "hi",
                "👋 Good to have you online, Agent. What cybersecurity topic can I help with?"
            },
            {
                "thank",
                "😊 Mission accepted with gratitude! Staying informed is your strongest weapon."
            }
        };

        
        /// Sets the user's name for personalised responses.
       
        public void SetUserName(string name)
        {
            _userName = name;
        }

      
        /// Analyses user input, matches keywords, and returns a response.
       
        public string GetResponse(string userInput)
        {
            if (string.IsNullOrWhiteSpace(userInput))
                return "No input detected. Please enter a query, Agent.";

            string lowerInput = userInput.ToLower().Trim();

            // Check multi-word phrases first
            if (lowerInput.Contains("social engineering"))
                return _keywordResponses["social engineering"];

            if (lowerInput.Contains("two-factor") || lowerInput.Contains("two factor")
                || lowerInput.Contains("2fa"))
                return _keywordResponses["two-factor"];

            if (lowerInput.Contains("what can i ask") || lowerInput.Contains("what can you do"))
                return _keywordResponses["what can i ask"];

            if (lowerInput.Contains("how are you"))
                return _keywordResponses["how are you"];

            // Check single keywords
            foreach (KeyValuePair<string, string> entry in _keywordResponses)
            {
                if (lowerInput.Contains(entry.Key.ToLower()))
                    return entry.Value;
            }

            // Default unrecognised response
            return GetDefaultResponse();
        }

        

        
        /// Returns a randomised default message for unrecognised input.
       
        private string GetDefaultResponse()
        {
            string[] defaults = {
                $"Intel not found, Agent {_userName}. Could you rephrase your query?\n" +
                "  Type 'help' to see available briefing topics.",

                "Classification unknown. Try asking about phishing, passwords,\n" +
                "  malware, or safe browsing — those are within my database.",

                $"Query unrecognised, Agent {_userName}.\n" +
                "  Try: 'What is phishing?' or 'How do I create a safe password?'"
            };

            Random rng = new Random();
            return defaults[rng.Next(defaults.Length)];
        }
    }
}

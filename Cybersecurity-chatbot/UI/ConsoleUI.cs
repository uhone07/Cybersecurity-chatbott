using System;
using System.Threading;
using CybersecurityChatbot.Audio;
using CybersecurityChatbot.Chat;

namespace Cybersecurity_chatbot.UI
{
    
    public class ConsoleUI
    {
        // ─── Colour Scheme ───────────────────────────────────────────────────────
        private const ConsoleColor HEADER_COLOR = ConsoleColor.Green;
        private const ConsoleColor BOT_COLOR = ConsoleColor.Cyan;
        private const ConsoleColor USER_COLOR = ConsoleColor.Yellow;
        private const ConsoleColor ACCENT_COLOR = ConsoleColor.White;
        private const ConsoleColor ERROR_COLOR = ConsoleColor.Red;
        private const ConsoleColor BORDER_COLOR = ConsoleColor.DarkGreen;
        private const ConsoleColor PROMPT_COLOR = ConsoleColor.Gray;

        private readonly ChatBot _bot;
        private readonly AudioPlayer _audioPlayer;
        private string _userName = "User";

        public ConsoleUI()
        {
            _bot = new ChatBot();
            _audioPlayer = new AudioPlayer();
        }


        
        /// Starts the chatbot: plays greeting audio, shows ASCII art,
        /// greets user, then enters the main conversation loop.
       
        public void Start()
        {
            Console.Title = "Cybersecurity Awareness Chatbot";
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            _audioPlayer.PlayGreeting();

            DisplayAsciiArt();
            DisplayWelcomeBanner();
            AskForUserName();
            DisplayHelp();
            RunConversationLoop();
        }

        private void DisplayAsciiArt()
        {
            Console.Clear();
            PrintBorder('*', 70);
            Console.WriteLine();

            // Shield art
            PrintColored(@"                        /\   /\                        ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                       /  \ /  \                       ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                      / ,--V--. \                      ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                     / /  | |  \ \                     ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                    / /   | |   \ \                    ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                   | |  .-'-'.  | |                   ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                   | | (  | |  ) | |                   ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                   | |  '-._.-'  | |                   ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                    \ \   | |   / /                    ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                     \ \  | |  / /                     ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                      \  \| |/  /                      ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                       \       /                        ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                        \    /                         ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                         \  /                          ", HEADER_COLOR);
            Console.WriteLine();
            PrintColored(@"                          \/                           ", HEADER_COLOR);
            Console.WriteLine();
            Console.WriteLine();

            

            PrintColored(@"   ██████╗██╗   ██╗██████╗ ███████╗██████╗ ", ACCENT_COLOR);
            Console.WriteLine();
            PrintColored(@"  ██╔════╝╚██╗ ██╔╝██╔══██╗██╔════╝██╔══██╗", ACCENT_COLOR);
            Console.WriteLine();
            PrintColored(@"  ██║      ╚████╔╝ ██████╔╝█████╗  ██████╔╝", ACCENT_COLOR);
            Console.WriteLine();
            PrintColored(@"  ██║       ╚██╔╝  ██╔══██╗██╔══╝  ██╔══██╗", ACCENT_COLOR);
            Console.WriteLine();
            PrintColored(@"  ╚██████╗   ██║   ██████╔╝███████╗██║  ██║", ACCENT_COLOR);
            Console.WriteLine();
            PrintColored(@"   ╚═════╝   ╚═╝   ╚═════╝ ╚══════╝╚═╝  ╚═╝", ACCENT_COLOR);
            Console.WriteLine();
            PrintColored(@"       CYBER  🔒", ACCENT_COLOR);





            PrintColored(@"    ===  CYBERSECURITY AWARENESS ASSISTANT  ===   ", ConsoleColor.Cyan);
            Console.WriteLine();
            PrintColored(@"    [ Protecting South African Citizens ]     ", ConsoleColor.DarkCyan);
            Console.WriteLine();
            Console.WriteLine();

            PrintBorder('*', 70);
            Thread.Sleep(600);
        }

        

       
        /// Prints the initial welcome message after ASCII art is shown.
        
        private void DisplayWelcomeBanner()
        {
            Console.WriteLine();
            TypeLine("  >> Initialising Cybersecurity Awareness Bot...", BOT_COLOR, 25);
            Thread.Sleep(400);
            TypeLine("  >> Systems online. Welcome, citizen!", BOT_COLOR, 25);
            TypeLine("  >> Your digital safety is our mission.", BOT_COLOR, 20);
            Console.WriteLine();
            Thread.Sleep(300);
        }

     

        
        /// Prompts the user for their name and validates it is not empty.
        
        private void AskForUserName()
        {
            PrintBorder('-', 70);
            BotSay("Agent identification required. Please enter your name:");

            string input = PromptUser();

            while (string.IsNullOrWhiteSpace(input))
            {
                PrintColored("  [!] Invalid entry. A name is required to proceed.\n", ERROR_COLOR);
                input = PromptUser();
            }

            _userName = input.Trim();
            _bot.SetUserName(_userName);

            Console.WriteLine();
            TypeLine($"  >> Identity confirmed. Welcome aboard, Agent {_userName}!", BOT_COLOR, 25);
            TypeLine("  >> I am CyberBot — your cybersecurity intelligence assistant.", BOT_COLOR, 20);
            PrintBorder('-', 70);
            Console.WriteLine();
        }

       

       
        /// Displays the list of available topics.
       
        private void DisplayHelp()
        {
            PrintColored("  [INTEL DATABASE] — Topics available for briefing:\n\n", ACCENT_COLOR);

            string[] topics = {
                "password safety",        "phishing",
                "safe browsing",          "malware",
                "two-factor authentication (2FA)",
                "social engineering",     "privacy",
                "scams",                  "how are you",
                "what is your purpose",   "what can I ask you about"
            };

            foreach (string topic in topics)
            {
                PrintColored($"    [+] {topic}\n", HEADER_COLOR);
                Thread.Sleep(50);
            }

            Console.WriteLine();
            PrintColored("  Type 'exit' or 'quit' to end the session.\n", PROMPT_COLOR);
            PrintBorder('*', 70);
            Console.WriteLine();
        }

        

        
        /// Runs the main conversation loop until the user exits.
       
        private void RunConversationLoop()
        {
            while (true)
            {
                string userInput = PromptUser();

                if (IsExitCommand(userInput))
                {
                    Farewell();
                    break;
                }

                if (string.IsNullOrWhiteSpace(userInput))
                {
                    PrintColored("  [!] No input detected. Please enter a query.\n\n", ERROR_COLOR);
                    continue;
                }

                string response = _bot.GetResponse(userInput);
                Console.WriteLine();
                BotSay(response);
                PrintBorder('-', 70);
                Console.WriteLine();
            }
        }

       
       
        /// Displays the input prompt and reads user input.
        
        private string PromptUser()
        {
            PrintColored($"  [Agent {_userName}] >> ", USER_COLOR);
            Console.ForegroundColor = PROMPT_COLOR;
            string input = Console.ReadLine() ?? string.Empty;
            Console.ResetColor();
            return input;
        }

        /// Prints a bot message with a typing-style delay.
       
        private void BotSay(string message)
        {
            PrintColored("  [CyberBot] >> ", BOT_COLOR);
            TypeLine(message, ACCENT_COLOR, 18);
            Console.WriteLine();
        }

        
        /// Prints text character-by-character to simulate a typing effect.
        
        private static void TypeLine(string text, ConsoleColor color, int delayMs = 20)
        {
            Console.ForegroundColor = color;
            foreach (char c in text)
            {
                Console.Write(c);
                Thread.Sleep(delayMs);
            }
            Console.ResetColor();
            Console.WriteLine();
        }

        
        /// Prints coloured text without a typing delay.
       
        private static void PrintColored(string text, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }

        
        /// Prints a horizontal border line.
      
        private static void PrintBorder(char borderChar, int width)
        {
            Console.ForegroundColor = BORDER_COLOR;
            Console.WriteLine(new string(borderChar, width));
            Console.ResetColor();
        }

       
        /// Returns true if the user typed an exit command.
        
        private static bool IsExitCommand(string input)
        {
            string lower = input.Trim().ToLower();
            return lower == "exit" || lower == "quit" || lower == "bye";
        }


        /// Displays a farewell message before closing.
       
        private void Farewell()
        {
            Console.WriteLine();
            PrintBorder('*', 70);
            TypeLine($"  >> Session terminated. Stay vigilant, Agent {_userName}.", BOT_COLOR, 25);
            TypeLine("  >> Remember: Your first line of defence is awareness. 🔒", HEADER_COLOR, 25);
            PrintBorder('*', 70);
            Thread.Sleep(1500);
        }
    }
}

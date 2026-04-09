using Cybersecurity_chatbot.UI;
using System;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Entry point for the Cybersecurity Awareness Chatbot application.
    /// Initialises the console interface and starts the chatbot session.
    /// </summary>
    class Program
    {
        static void Main(string[] args)
        {
            ConsoleUI ui = new ConsoleUI();
            ui.Start();
        }
    }
}

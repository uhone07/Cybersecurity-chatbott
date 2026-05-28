# Cybersecurity Awareness Bot — Part 2

## Student Information
- *Module:* Programming 2A (PROG6221)
- *Assessment:* Portfolio of Evidence (POE) — Part 2

---

## Project Overview
This is Part 2 of the Cybersecurity Awareness Chatbot. The console application from Part 1
has been upgraded to a full *WPF (Windows Presentation Foundation)* graphical user interface
with advanced chatbot features including keyword recognition, sentiment detection, memory,
and dynamic responses.

---

## What's New in Part 2

| Feature | Description |
|---|---|
| *WPF GUI* | Dark-themed graphical interface replacing the console |
| *Keyword Recognition* | Recognises 8 cybersecurity topics and gives targeted responses |
| *Random Responses* | Randomly selects from 5 tips per topic for varied interaction |
| *Conversation Flow* | Type "tell me more" to continue on the current topic |
| *Memory and Recall* | Remembers your name and favourite topic across the conversation |
| *Sentiment Detection* | Detects emotions (worried, frustrated, curious, etc.) and responds empathetically |
| *Voice Greeting* | WAV audio plays on startup (carried over from Part 1) |
| *ASCII Art* | Cybersecurity logo displayed in the sidebar (carried over from Part 1) |
| *Error Handling* | Default responses for unknown or invalid inputs |


Cybersecurity-chatbot/
├── App.xaml
├── App.xaml.cs
├── Cybersecurity-chatbot.csproj
├── Program.cs
├── greeting.wav
├── Audio/
│   └── AudioPlayer.cs
├── Chat/
│   └── ChatBot.cs
└── UI/
├── ConsoleUI.cs
├── MainWindow.xaml
└── MainWindow.xaml.cs

## How to Run

### Requirements
- Windows 10 or 11
- Visual Studio 2022
- .NET 8.0 SDK
- No additional NuGet packages required

### Steps
1. Clone the repository:
2.git clone https://github.com/YOUR-USERNAME/Cybersecurity-chatbot.git



2. Open Cybersecurity-chatbot.sln in Visual Studio 2022
3. Ensure the startup project is set to Cybersecurity-chatbot
4. Press F5 or click Start to run
5. The WPF window will launch with a voice greeting

---

## How to Use

### Starting the Chat
- When the app launches the bot will greet you and ask for your name
- Type your name and press Enter or click Send
- The sidebar will update with your name

### Asking Questions
Type any of the following topics to get a cybersecurity tip:
- password — password safety advice
- phishing — how to spot phishing attacks
- privacy — protecting your personal data
- scam — avoiding online scams
- malware — malware and virus protection
- 2fa — two-factor authentication
- browsing — safe browsing habits
- social engineering — recognising manipulation attacks

### Conversation Flow
- Type tell me more or another tip to get a new tip on the same topic
- Type what can I ask you about to see all available topics

### Memory Feature
Tell the bot your favourite topic by typing:
I'm interested in privacy
The bot will remember this and personalise future responses

### Sentiment Detection
Express how you feel and the bot will respond empathetically:
- I'm worried about scams
- I'm confused about 2fa
- I'm frustrated with passwords

### Quick Topic Chips
Click any chip button in the left sidebar to instantly ask about that topic

---

## Features Demonstrated

### Keyword Recognition
The chatbot uses string.Contains() to detect cybersecurity keywords in user input.
8 topics are supported with targeted relevant responses for each.

### Random Responses
Each topic has 5 different tips stored in a List of strings.
The bot uses Random.Next() to select a different one each time
keeping the conversation varied and engaging.

### Conversation Flow
Follow-up phrases like tell me more, explain more, another tip, and what else
are detected using a list of trigger strings. The bot cycles through tips for the
current topic without restarting the conversation.

### Memory and Recall
- The user's name is captured on the first message and stored in the ChatBot class
- When the user says I'm interested in a topic that topic is stored as their favourite
- The sidebar panel updates live to show what the bot remembers
- Later responses on the favourite topic include a personalised recall line

### Sentiment Detection
8 emotional keywords are mapped to empathetic response prefixes:
worried, scared, anxious, frustrated, confused, curious, nervous, overwhelmed
The sentiment prefix is prepended to the relevant tip automatically.
The user does not need to re-enter their question.

---

## GitHub CI/CD
- GitHub Actions workflow is configured for automatic build checks on every push
- See .github/workflows/ for the CI configuration
- A green check mark confirms each commit passes the build

---

## GitHub Releases
- v1.0 — Part 1 console application
- v2.0 — Part 2 WPF GUI with all advanced features

---

## Video Presentation
Part 2 video walkthrough (unlisted YouTube link):
INSERT YOUR YOUTUBE LINK HERE

The video covers:
- Overview of the WPF GUI design
- Live demonstration of all Part 2 features
- Code walkthrough of ChatBot.cs, MainWindow.xaml and MainWindow.xaml.cs
- Explanation of keyword recognition, random responses, memory, sentiment detection
  and conversation flow logic

---

## References
Pieterse, H. 2021. The Cyber Threat Landscape in South Africa: A 10-Year Review.
The African Journal of Information and Communication, 28(28).
doi: https://doi.org/10.23962/10539/32213

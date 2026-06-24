# Cybersecurity Awareness Bot — Part 3 (Final POE)

## Student Information
- **Module:** Programming 2A (PROG6221)
- **Assessment:** Portfolio of Evidence (POE) — Part 3 Final Submission

---

## Project Overview
This is the final part of the Cybersecurity Awareness Chatbot project.
Building on Parts 1 and 2, this submission adds a Task Assistant with MySQL
database integration, a Cybersecurity Quiz game, NLP simulation, and an
Activity Log — all within a single tabbed WPF GUI application.

---

## What Was Built Across All Three Parts

### Part 1 — Console Application
- Voice greeting using WAV audio playback
- ASCII art cybersecurity logo
- Personalised user greeting using the user's name
- Basic cybersecurity response system
- Input validation and error handling
- Enhanced console UI with colour formatting

### Part 2 — WPF GUI
- Full dark-themed graphical user interface
- Keyword recognition for 8 cybersecurity topics
- Random responses with 5 tips per topic
- Conversation flow with follow-up detection
- Memory and recall system for name and favourite topic
- Sentiment detection with empathetic responses

### Part 3 — Advanced Features (Final POE)
- Task Assistant with MySQL database integration
- Cybersecurity Mini-Game Quiz with 12 questions
- NLP simulation using regex and keyword detection
- Activity Log recording all chatbot actions
- All parts integrated into one tabbed WPF application

---

## Project Structure

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

├── Data/

│   ├── DatabaseHelper.cs

│   └── ActivityLog.cs

├── Models/

│   ├── TaskItem.cs

│   └── QuizQuestion.cs

└── UI/

├── ConsoleUI.cs

├── MainWindow.xaml

└── MainWindow.xaml.cs
---

## Requirements

- Windows 10 or 11
- Visual Studio 2022
- .NET 8.0 SDK
- MySQL Server (local instance)
- MySQL Workbench (optional, for verification)
- NuGet Package: MySql.Data 9.3.0

---

## Database Setup

1. Open MySQL Workbench and connect using your root credentials
2. Run the following query:
CREATE DATABASE IF NOT EXISTS cybersecurity_bot;
3. The application will automatically create the cyber_tasks table
   on first launch — no further setup is required

### Database Table Structure

Table: cyber_tasks

id            INT AUTO_INCREMENT PRIMARY KEY
title         VARCHAR(255)
description   TEXT
is_completed  TINYINT(1) DEFAULT 0
reminder_date DATETIME NULL
created_at    DATETIME

---

## How to Run

1. Clone the repository:
2. Open Cybersecurity-chatbot.sln in Visual Studio 2022
3. Open DatabaseHelper.cs and confirm the password matches your MySQL setup
4. Open Package Manager Console and run:
Install-Package MySql.Data
5. Press F5 to run the application
6. The WPF window will launch with a voice greeting and ask for your name

---

## How to Use

### Chat Tab
Type any cybersecurity topic or natural language command:

Cybersecurity topics:
- password — password safety tips
- phishing — how to spot phishing attacks
- privacy — protecting personal data
- scam — avoiding online scams
- malware — malware and virus protection
- 2fa — two-factor authentication
- browsing — safe browsing habits
- social engineering — recognising manipulation

Conversation features:
- Type tell me more to get another tip on the same topic
- Type I am interested in privacy to save your favourite topic
- Express emotions like I am worried about scams for empathetic responses
- Type what can I ask you about to see all available topics

NLP commands (natural language):
- add a task to enable 2FA
- remind me to update my password in 3 days
- start quiz
- show my tasks
- show activity log
- what have you done for me

### Tasks Tab
- Fill in the task title and optional description
- Select an optional reminder date using the date picker
- Click Add Task to save to the MySQL database
- Click Done to mark a task as completed
- Click the bin icon to delete a task

### Quiz Tab
- Click Start Quiz to begin
- Answer each of the 12 cybersecurity questions
- Get immediate feedback after each answer
- View your final score and rating at the end
- Click Restart to play again

### Activity Log Tab
- View the last 10 actions recorded by the chatbot
- Click Show More to see the full history
- All actions are timestamped

---

## Features Demonstrated

### Task Assistant with Database Integration
Tasks are stored in a MySQL database using the MySql.Data NuGet package.
The DatabaseHelper class handles all CRUD operations:
- AddTask inserts a new task with title, description, and optional reminder
- GetAllTasks retrieves all tasks ordered by creation date
- MarkCompleted updates the is_completed field to 1
- DeleteTask removes the task from the database

### Cybersecurity Quiz
12 questions covering phishing, passwords, malware, social engineering,
safe browsing, privacy, 2FA, and ransomware.
Mix of multiple choice and true or false formats.
Immediate feedback with explanation after each answer.
Final score with performance rating.

### NLP Simulation
The ChatBot class uses System.Text.RegularExpressions to detect user intent
from naturally worded input. Nine regex patterns cover intents including:
- add task or reminder
- view tasks
- start quiz
- show activity log
- suggest specific cybersecurity tasks like enabling 2FA

Examples of natural language the bot understands:
- Add a task to review my privacy settings
- Remind me to change my password in 7 days
- Can you start the quiz
- What have you done for me

### Activity Log
A static ActivityLog class stores up to 50 timestamped entries in memory.
Every significant action is logged including:
- Tasks added, completed, or deleted
- Quiz started and completed with score
- NLP commands recognised and executed
- Keywords responded to
- User name and favourite topic set

---

## GitHub Releases

- v1.0 — Part 1 console application with voice greeting and ASCII art
- v2.0 — Part 2 WPF GUI with keyword recognition, memory, and sentiment detection
- v3.0 — Part 3 final POE with tasks, quiz, NLP, and activity log

---

## GitHub Actions CI
- Automated build check runs on every push
- See .github/workflows/ for the workflow configuration
- Green check mark confirms each commit passes the build

---

## Video Presentation
Final POE video walkthrough (unlisted YouTube link):

The video covers:
- Full demonstration of the Chat tab including NLP commands
- Tasks tab demonstration with MySQL database verification
- Quiz tab walkthrough showing questions, feedback, and final score
- Activity Log demonstration showing tracked actions
- Code walkthrough of ChatBot.cs, DatabaseHelper.cs, ActivityLog.cs,
  MainWindow.xaml, and MainWindow.xaml.cs
- Explanation of NLP regex patterns and intent detection logic
- MySQL Workbench showing data saved in the cyber_tasks table

---

## References
Pieterse, H. 2021. The Cyber Threat Landscape in South Africa: A 10-Year Review.
The African Journal of Information and Communication, 28(28).
doi: https://doi.org/10.23962/10539/32213

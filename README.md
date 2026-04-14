# CyberGuard — Cybersecurity Awareness Assistant

**Module:** PROG6221 — Programming 2A  
**Assessment:** Portfolio of Evidence — Part 1  
**Institution:** The Independent Institute of Education (IIE)  
**Target Framework:** .NET Framework 4.8  

---

## What Is This?

CyberGuard is a C# console application built as a cybersecurity education tool for South African citizens. It simulates a conversation with a knowledgeable assistant that teaches users how to protect themselves against common digital threats including phishing, malware, online scams, and social engineering.

---

## How to Run It

### What You Need
- Windows PC
- Visual Studio 2022
- .NET Framework 4.8

### Steps
1. Clone this repository to your machine
2. Open `CybersecurityChatbot.csproj` in Visual Studio 2022
3. Add your `greeting.wav` file to the project root (see WAV setup below)
4. Press **F5** to build and run

### Adding the WAV Greeting
1. Record yourself saying the welcome message (e.g. using Windows Voice Recorder)
2. Save/convert the file as `greeting.wav`
3. Add it to the project in Visual Studio via right-click → Add → Existing Item
4. Click the file in Solution Explorer → press F4 → set **Copy to Output Directory** to **Copy if newer**

---

## Project Structure

```
CybersecurityChatbot/
│
├── Program.cs                   ← Launches the application
├── UI/ConsoleUI.cs              ← Display logic, ASCII art, colours, typing effect
├── Chat/ChatBot.cs              ← Response engine and keyword matching
├── Audio/AudioPlayer.cs         ← WAV greeting playback
├── greeting.wav                 ← Your recorded voice greeting
├── CybersecurityChatbot.csproj  ← Project configuration
├── .github/workflows/ci.yml     ← Automated build check on every push
└── README.md                    ← You are here
```

---

## What It Can Do

Type any of these topics into the chat:

| Topic | What You Will Learn |
|---|---|
| `password` | How to create and manage strong passwords |
| `phishing` | How to identify and avoid phishing emails |
| `browsing` | Safe habits for browsing the internet |
| `malware` | How to protect your device from malicious software |
| `two-factor` or `2fa` | Why and how to enable two-factor authentication |
| `social engineering` | Psychological manipulation tactics used by attackers |
| `privacy` | How to protect your personal data online |
| `scam` | Common online scams targeting South Africans |
| `help` | Shows the full topic list |
| `exit` | Ends the session |

---

## Code Design

The project is split into four focused classes:

- **Program.cs** — entry point only, no logic
- **ConsoleUI** — all visual output: ASCII art, colour scheme, typing effect, borders, input/output loop
- **ChatBot** — keyword dictionary, response matching, input validation, default fallback messages
- **AudioPlayer** — WAV file location, playback via `System.Media.SoundPlayer`, graceful error handling

---

## Continuous Integration

This project uses **GitHub Actions** to automatically build the code on every push. The workflow file lives at `.github/workflows/ci.yml`.

### CI Build Screenshot

<img width="731" height="338" alt="CI Workflow" src="https://github.com/user-attachments/assets/6813cc52-89ae-4016-b6f7-99398d7638a9" />


## Commit History

| Commit | Message |
|---|---|
| 1 | `Initial commit: project structure, folders, and namespace setup` |
| 2 | `Added AudioPlayer with WAV playback and missing-file handling` |
| 3 | `Added ConsoleUI with shield ASCII art and colour-coded output` |
| 4 | `Implemented ChatBot keyword dictionary and response system` |
| 5 | `Added input validation, typing effect, and default responses` |
| 6 | `Added GitHub Actions CI workflow, README, and final polish` |

---

## References

Pieterse, H. 2021. The Cyber Threat Landscape in South Africa: A 10-Year Review. *The African Journal of Information and Communication*, 28(28). doi: https://doi.org/10.23962/10539/32213. Available at: https://www.scielo.org.za/scielo.php?pid=S2077-72132021000200003&script=sci_arttext [Accessed 16 February 2026].

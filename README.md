# CYBERSECURITY_CHATBOT2

Initialized the SAFENET cybersecurity chatbot project using C# and WPF in Visual Studio. Set up the overall application structure, configured namespaces, created the MainWindow and SplashWindow, and prepared the base architecture for chatbot interactions and UI development

Designed and implemented the main chatbot user interface with a modern dark cybersecurity theme. Added ASCII art branding, chat display functionality, ScrollViewer support, styled input controls, Send button interactions, and keyboard Enter key support to improve user experience.


Developed the custom splash screen system with dynamic runtime logo loading. Added support for loading images from Assets folders, application directories, and embedded resources while also implementing fallback handling to prevent crashes if logo files are missing.

Added audio greeting functionality through a reusable AudioPlayer utility class. Implemented synchronous and asynchronous WAV playback methods, included exception handling for audio errors, and created automatic test greeting WAV generation for improved startup interaction.

Implemented the core chatbot response engine with keyword-based cybersecurity topic detection. Added multiple cybersecurity awareness categories including phishing, malware, passwords, ransomware, privacy, backups, MFA, scams, social engineering, and IoT security together with randomized responses for more natural conversations.

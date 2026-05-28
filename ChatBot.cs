using System;
using System.Collections.Generic;
using System.Linq;

namespace ChatbotPart2
{
    public class ChatBot
    {
        private readonly string _name;
        private string? _userName;
        private bool _expectingName;
        private readonly Random _rng;
        // Random number generator used to pick different replies so the
        // chatbot doesn't sound repetitive. Think of this as rolling a die
        // to choose one of several helpful tips.
        
        // Response pools for varied replies
        private readonly List<string> _phishingTips = new()
        {
            "Be cautious of emails asking for personal information. Scammers often disguise themselves as trusted organizations.",
            "Check the sender's email address carefully and hover over links to verify destinations before clicking.",
            "Look for spelling/grammar mistakes and unexpected urgency — these are common signals of phishing.",
            "When in doubt, contact the organization via a known channel rather than replying to the email."
        };

        private readonly List<string> _passwordTips = new()
        {
            "Use a long, unique password and enable multi-factor authentication where possible.",
            "Consider a reputable password manager to generate and store unique passwords for each account.",
            "Avoid reusing passwords across sites — a breach at one site can compromise others."
        };

        // Each of the above lists is a small set of friendly tips about the
        // topic named by the variable. The bot will pick one at random when
        // the user asks about that subject.

        private readonly List<string> _malwareTips = new()
        {
            "Keep software updated, avoid pirated software, and run reputable antivirus software.",
            "Only install apps from trusted sources and review requested permissions before installing.",
            "Back up important data and isolate infected devices to reduce the impact of malware."
        };

        private readonly List<string> _ransomwareTips = new()
        {
            "Ransomware often spreads through malicious email attachments — don't open unexpected files and keep backups offline.",
            "Keep systems and backups patched, and test restores regularly so you can recover without paying a ransom.",
            "Limit administrative privileges and segment networks to reduce the blast radius of ransomware." 
        };

        private readonly List<string> _privacyTips = new()
        {
            "Review app permissions and disable access to data/features that aren't needed.",
            "Use privacy-focused browser settings and limit third-party cookie tracking where possible.",
            "Be cautious sharing personal information on social media — malicious actors harvest data for targeted scams."
        };

        private readonly List<string> _browsingTips = new()
        {
            "Use HTTPS sites, avoid suspicious downloads, and consider a browser with tracking protection.",
            "Keep your browser and extensions updated and remove extensions you don't recognize or need.",
            "Consider using an ad/tracker blocker and avoid clicking unexpected pop-ups or download prompts."
        };

        private readonly List<string> _mfaTips = new()
        {
            "Enable multi-factor authentication (MFA) on important accounts — use an authenticator app or hardware key when possible.",
            "Avoid SMS-based codes when stronger options (authenticator apps or hardware tokens) are available.",
            "MFA greatly reduces the risk from stolen passwords — enable it wherever the service supports it." 
        };

        private readonly List<string> _backupTips = new()
        {
            "Keep regular, tested backups and store at least one copy offline or in a different network location.",
            "Use versioned backups so you can recover from accidental deletion or ransomware without losing all history.",
            "Encrypt sensitive backups and monitor backup jobs for failures." 
        };

        private readonly List<string> _socialEngineeringTips = new()
        {
            "Be skeptical of unexpected requests for information or urgent asks — verify identities through an independent channel.",
            "Don't reveal sensitive details over the phone or chat unless you initiated the contact and verified the recipient.",
            "Train staff to recognize common social-engineering tactics like pretexting, baiting, and tailgating." 
        };

        private readonly List<string> _scamTips = new()
        {
            "If an offer sounds too good to be true, it probably is — research and verify before engaging.",
            "Don't transfer money to unknown parties and verify requests for payment through official channels.",
            "Report scams to the relevant platform or authority so others can be warned." 
        };

        private readonly List<string> _iotTips = new()
        {
            "Change default passwords on IoT devices and keep their firmware updated.",
            "Place IoT devices on a separate network or VLAN from sensitive systems.",
            "Disable unnecessary services and use network-level protections where available." 
        };
        // Simple memory store for a user's favourite topic (could be extended)
        private string? _favoriteTopic;

        // Constructor: create a chatbot instance. "name" is the bot's name
        // (what the bot will call itself). We also initialise memory fields
        // and a random number generator used for varied replies.
        public ChatBot(string name)
        {
            _name = name;
            _userName = null; // we don't know the user's name yet
            _expectingName = false; // not currently waiting for a name
            _rng = new Random(); // used to select random tips from the lists
        }

        // The sentiment analyzer above is intentionally tiny and rule-based.
        // It scans words in the user's input and increases or decreases a
        // score based on a small list of positive and negative keywords.
        // The final score decides whether the message is treated as
        // Positive, Negative or Neutral. This is not a full NLP system; it
        // is a pragmatic approach that works well enough for simple
        // adjustments to the bot's tone.

        // Call this when you want the bot to ask the user for their name
        public void PromptForName()
        {
            _expectingName = true;
        }

        // Returns true if the bot is currently waiting for the user's name
        public bool IsExpectingName => _expectingName && string.IsNullOrWhiteSpace(_userName);

        // Simple heuristic to decide whether a short input looks like a name
        private bool IsProbableName(string rawInput)
        {
            if (string.IsNullOrWhiteSpace(rawInput)) return false;
            var trimmed = rawInput.Trim();
            // avoid questions
            var lower = trimmed.ToLowerInvariant();
            if (lower.StartsWith("what") || lower.StartsWith("who") || lower.StartsWith("where") || lower.Contains("?"))
                return false;

            // token count 1-3 and tokens contain letters/hyphen/apostrophe only
            var tokens = trimmed.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length < 1 || tokens.Length > 3) return false;

            foreach (var t in tokens)
            {
                foreach (var c in t)
                {
                    if (!(char.IsLetter(c) || c == '-' || c == '\''))
                        return false;
                }
            }

            return true;
        }

        // The code above is a simple check to see whether a short phrase
        // is likely a person's name. It prevents the bot from treating
        // questions or long sentences as names.

        // High level sentiment categories
        public enum Sentiment
        {
            Negative,
            Neutral,
            Positive
        }

        // Sentiment is a tiny way to describe the user's mood based on
        // key words in what they said: positive (happy/helpful),
        // negative (upset/problem), or neutral.

        // Rich response that includes detected sentiment and emotion label
        public class ChatResponse
        {
            public string Text { get; set; } = string.Empty;
            public Sentiment SentimentScore { get; set; }
            public string Emotion { get; set; } = string.Empty;
        }

        // ChatResponse is a small container used when the caller wants
        // not just the text reply but also a note about the detected mood
        // and an emotion label. Think of it as a package: the message plus
        // a couple of helpful tags describing how the user seems to feel.

        // Main conversation entry point.
        // This method receives what the user typed and returns the chatbot's
        // reply as a single text string. The method does a few things in
        // order:
        // 1. Ignore empty input.
        // 2. If the bot previously asked for the user's name, try to capture it.
        // 3. Check whether the user says they are "interested in X" and
        //    remember that as their favourite topic.
        // 4. Detect the user's sentiment (rough mood) and a short emotion tag.
        // 5. Look for topic keywords (password, phishing, malware, etc.) and
        //    pick a helpful tip from the corresponding list.
        // 6. Adjust the tone of the reply depending on sentiment and then
        //    personalise by mentioning the user's name or favourite topic if
        //    we already remembered them.
        //
        // The method is intentionally simple so it is easy to understand and
        // extend. It returns null for empty input.
        public string? GetResponse(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;

            var raw = input.Trim();
            var s = raw.ToLowerInvariant();

            // If the bot is expecting a name, handle that first
            if (_expectingName && string.IsNullOrWhiteSpace(_userName))
            {
                if (IsProbableName(raw))
                {
                    // Store a cleaned up name (capitalize first letters)
                    _userName = string.Join(" ", raw.Split(' ', StringSplitOptions.RemoveEmptyEntries).Select(p => char.ToUpperInvariant(p[0]) + p.Substring(1)));
                    _expectingName = false;
                    return $"Nice to meet you, {_userName}! I'm {_name}. How can I help you today?";
                }
                else
                {
                    return "I didn't catch your name  could you tell me your first name?";
                }
            }

            // Check if the user is telling us a favourite topic to remember
            if (TryExtractFavoriteTopic(s, out var favTopic))
            {
                _favoriteTopic = favTopic;
                // Acknowledge and give a short tip from that topic
                var pool = GetPoolForTopic(favTopic);
                var tip = pool != null ? PickRandom(pool) : string.Empty;
                // Say we will remember the user's interest. We use the
                // canonical topic name (favTopic) which is a short, readable
                // label such as "privacy" or "phishing".
                return $"Great — I'll remember that you're interested in {favTopic}. {tip}";
            }

            // Detect sentiment and emotion first
            var sentiment = AnalyzeSentiment(s);
            var emotion = DetectEmotionLabel(s);

            // Base advice depending on topic
            string baseReply = null;
            string detectedTopic = null;
            if (s.Contains("password"))
            {
                baseReply = PickRandom(_passwordTips);
                detectedTopic = "passwords";
            }
            else if (s.Contains("mfa") || s.Contains("two-factor") || s.Contains("two factor") || s.Contains("2fa"))
            {
                baseReply = PickRandom(_mfaTips);
                detectedTopic = "mfa";
            }
            else if (s.Contains("phish") || s.Contains("email"))
            {
                baseReply = PickRandom(_phishingTips);
                detectedTopic = "phishing";
            }
            else if (s.Contains("ransom") || s.Contains("ransomware"))
            {
                baseReply = PickRandom(_ransomwareTips);
                detectedTopic = "ransomware";
            }
            else if (s.Contains("malware") || s.Contains("virus"))
            {
                baseReply = PickRandom(_malwareTips);
                detectedTopic = "malware";
            }
            else if ((s.Contains("social") && (s.Contains("engineer") || s.Contains("engineering"))) || s.Contains("social-engineering"))
            {
                baseReply = PickRandom(_socialEngineeringTips);
                detectedTopic = "social-engineering";
            }
            else if (s.Contains("scam") || s.Contains("scams"))
            {
                baseReply = PickRandom(_scamTips);
                detectedTopic = "scams";
            }
            else if (s.Contains("backup") || s.Contains("back up") || s.Contains("backups"))
            {
                baseReply = PickRandom(_backupTips);
                detectedTopic = "backups";
            }
            else if (s.Contains("privacy"))
            {
                baseReply = PickRandom(_privacyTips);
                detectedTopic = "privacy";
            }
            else if (s.Contains("browse") || s.Contains("safe browsing") || s.Contains("https"))
            {
                baseReply = PickRandom(_browsingTips);
                detectedTopic = "browsing";
            }
            else if (s.Contains("iot") || s.Contains("device") || s.Contains("smart") )
            {
                baseReply = PickRandom(_iotTips);
                detectedTopic = "iot";
            }

            // If no topic matched, provide a friendly fallback that adapts to sentiment
            if (baseReply == null)
            {
                if (sentiment == Sentiment.Negative)
                    baseReply = "I'm sorry you're having trouble — tell me more and I'll do my best to help.";
                else if (sentiment == Sentiment.Positive)
                    baseReply = "Great! I'm glad to hear that. How can I help further?";
                else
                    baseReply = "I'm here to help — what would you like to know about cybersecurity?";
            }

            // Adjust tone according to detected sentiment
            var adjusted = AdjustTone(baseReply, sentiment, emotion);

            // Personalize responses: mention user's favourite topic and their name when appropriate
            string finalReply = adjusted;
            if (!string.IsNullOrWhiteSpace(_favoriteTopic) && !string.IsNullOrWhiteSpace(detectedTopic) && _favoriteTopic == detectedTopic)
            {
                finalReply = $"As someone interested in {_favoriteTopic}, {finalReply}";
            }
            if (!string.IsNullOrWhiteSpace(_userName))
            {
                finalReply = $"{_userName}, {finalReply}";
            }

            return finalReply;
        }

        // New richer API that returns sentiment metadata alongside the text
        public ChatResponse GetResponseWithSentiment(string input)
        {
            // This method is a convenience: it returns the same text reply as
            // GetResponse but also includes the sentiment and emotion labels
            // so the caller (for example, the UI) can display them or use
            // them for analytics or different visuals.
            var text = GetResponse(input);
            var s = input?.ToLowerInvariant() ?? string.Empty;
            var sentiment = AnalyzeSentiment(s);
            var emotion = DetectEmotionLabel(s);

            return new ChatResponse
            {
                Text = text ?? string.Empty,
                SentimentScore = sentiment,
                Emotion = emotion
            };
        }

        // Very small rule-based sentiment analyzer. Good enough for simple UX changes.
        private Sentiment AnalyzeSentiment(string lowerInput)
        {
            if (string.IsNullOrWhiteSpace(lowerInput)) return Sentiment.Neutral;

            int score = 0;

            // Positive/negative seed lists — keep small and conservative for offline use
            var positive = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "thank", "thanks", "great", "good", "awesome", "love", "fantastic", "happy", "ok", "okay", "nice", "helpful"
            };

            var negative = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "angry", "upset", "hate", "frustrat", "frustration", "sad", "terrible", "bad", "worried", "scared", "afraid", "panic", "problem", "issue"
            };

            // Simple token scan
            var tokens = lowerInput.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in tokens)
            {
                if (positive.Any(p => t.Contains(p))) score += 1;
                if (negative.Any(n => t.Contains(n))) score -= 1;
            }

            if (score > 0) return Sentiment.Positive;
            if (score < 0) return Sentiment.Negative;
            return Sentiment.Neutral;
        }

        // Map some keywords to an emotion label for more natural replies
        private string DetectEmotionLabel(string lowerInput)
        {
            if (string.IsNullOrWhiteSpace(lowerInput)) return string.Empty;

            if (lowerInput.Contains("angry") || lowerInput.Contains("furious") || lowerInput.Contains("rage")) return "angry";
            if (lowerInput.Contains("frustrat") || lowerInput.Contains("annoy") || lowerInput.Contains("hate")) return "frustrated";
            if (lowerInput.Contains("sad") || lowerInput.Contains("upset") || lowerInput.Contains("depress")) return "sad";
            if (lowerInput.Contains("scared") || lowerInput.Contains("afraid") || lowerInput.Contains("worried") || lowerInput.Contains("panic")) return "anxious";
            if (lowerInput.Contains("thank") || lowerInput.Contains("great") || lowerInput.Contains("love") || lowerInput.Contains("happy")) return "happy";

            return string.Empty;
        }

        // DetectEmotionLabel looks for a few words that map to a simple
        // emotion label such as "angry" or "happy". The label is used to
        // make replies feel more personal (for example: "I'm sorry you're
        // feeling frustrated"). This is a small and conservative mapping
        // intended for UX polish rather than detailed emotion analysis.

        // Adjust a base reply to match sentiment and optionally reference the emotion label
        private string AdjustTone(string baseReply, Sentiment sentiment, string emotionLabel)
        {
            if (string.IsNullOrWhiteSpace(baseReply)) return baseReply;

            // AdjustTone modifies the start of the reply to match the user's
            // mood. For negative sentiment we add a short empathic phrase;
            // for positive sentiment we add a congratulatory phrase. This
            // keeps the conversation feeling more natural.
            switch (sentiment)
            {
                case Sentiment.Negative:
                    var empathic = "I" + "'m sorry you\'re feeling"; // avoid contraction parsing issues
                    if (!string.IsNullOrWhiteSpace(emotionLabel)) empathic += $" {emotionLabel}";
                    empathic += ". " ;
                    empathic += "If you want, I can walk you through the steps to fix this.";
                    return empathic + " " + baseReply;

                case Sentiment.Positive:
                    var praise = "That's great to hear! ";
                    return praise + baseReply;

                default:
                    return baseReply;
            }
        }

        // Helper to pick a random entry from a list
        private string PickRandom(IList<string> options)
        {
            if (options == null || options.Count == 0) return string.Empty;
            return options[_rng.Next(options.Count)];
        }

        // PickRandom chooses one string from the provided list. If you think
        // of the list as a stack of tip-cards, this method picks one card at
        // random. This is what makes the bot's replies varied across turns.

        // Try to extract a favourite topic when the user expresses interest
        private bool TryExtractFavoriteTopic(string input, out string? topic)
        {
            topic = null;
            if (string.IsNullOrWhiteSpace(input)) return false;

            // Mapping of canonical topic -> keywords
            var mapping = new Dictionary<string, string[]>()
            {
                { "privacy", new[] { "privacy" } },
                { "phishing", new[] { "phish", "email" } },
                { "passwords", new[] { "password", "passwords" } },
                { "malware", new[] { "malware", "virus" } },
                { "ransomware", new[] { "ransom", "ransomware" } },
                { "browsing", new[] { "browse", "browsing", "https", "browser" } },
                { "mfa", new[] { "mfa", "two-factor", "two factor", "2fa" } },
                { "backups", new[] { "backup", "back up", "backups" } },
                { "social-engineering", new[] { "social engineer", "social-engineering", "social engineering" } },
                { "scams", new[] { "scam", "scams" } },
                { "iot", new[] { "iot", "device", "smart" } }
            };

            // Simple trigger phrases that indicate the user is stating an interest
            // (we look for plain-language clues such as "I'm interested" or "I like")
            var trigger = input.Contains("interested") || input.Contains("i like") || input.Contains("i'm into") || input.Contains("im into") || input.Contains("favorite") || input.Contains("favourite") || input.StartsWith("i like") || input.StartsWith("i'm interested") || input.StartsWith("i am interested");

            foreach (var kvp in mapping)
            {
                if (kvp.Value.Any(k => input.Contains(k)))
                {
                    if (trigger)
                    {
                        topic = kvp.Key;
                        return true;
                    }
                }
            }

            return false;
        }

        // Return the response pool for a canonical topic
        private IList<string>? GetPoolForTopic(string topic)
        {
            return topic switch
            {
                "privacy" => _privacyTips,
                "phishing" => _phishingTips,
                "passwords" => _passwordTips,
                "malware" => _malwareTips,
                "ransomware" => _ransomwareTips,
                "browsing" => _browsingTips,
                "mfa" => _mfaTips,
                "backups" => _backupTips,
                "social-engineering" => _socialEngineeringTips,
                "scams" => _scamTips,
                "iot" => _iotTips,
                _ => null,
            };
        }
    }
}
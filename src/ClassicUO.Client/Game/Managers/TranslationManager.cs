using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using ClassicUO.Configuration;

namespace ClassicUO.Game.Managers
{
    internal static class TranslationManager
    {
        private static readonly Dictionary<string, Dictionary<string, string>> _dictionary = new()
        {
            ["es"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"hello", "hola"},
                {"world", "mundo"},
                {"yes", "sí"},
                {"no", "no"}
            },
            ["fr"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"hello", "bonjour"},
                {"world", "monde"},
                {"yes", "oui"},
                {"no", "non"}
            },
            ["de"] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                {"hello", "hallo"},
                {"world", "welt"},
                {"yes", "ja"},
                {"no", "nein"}
            }
        };

        private static readonly HttpClient _http = new HttpClient();

        public static string Translate(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            string lang = ProfileManager.CurrentProfile?.TranslationLanguage ?? "es";

            if (string.Equals(lang, "en", StringComparison.OrdinalIgnoreCase))
            {
                return text;
            }

            // First try local dictionary
            if (_dictionary.TryGetValue(lang, out var langDict) && langDict.TryGetValue(text, out string translated))
            {
                return translated;
            }

            // Fallback to external API if possible
            try
            {
                var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=auto|{lang}";
                string json = _http.GetStringAsync(url).GetAwaiter().GetResult();
                using JsonDocument doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("responseData", out JsonElement rd) &&
                    rd.TryGetProperty("translatedText", out JsonElement tt))
                {
                    translated = tt.GetString();
                    if (!string.IsNullOrEmpty(translated))
                    {
                        return translated;
                    }
                }
            }
            catch
            {
                // ignored - fallback to original text
            }

            return text;
        }
    }
}


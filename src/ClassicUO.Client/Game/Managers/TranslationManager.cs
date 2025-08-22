using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using ClassicUO.Configuration;

namespace ClassicUO.Game.Managers
{
    internal static class TranslationManager
    {
        private static readonly HttpClient _client = new HttpClient();
        private static readonly Dictionary<string, string> _cache = new Dictionary<string, string>();

        public static string Translate(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            if (ProfileManager.CurrentProfile == null || !ProfileManager.CurrentProfile.TranslateChatMessages)
                return text;

            if (_cache.TryGetValue(text, out var cached))
                return cached;

            try
            {
                var url = $"https://api.mymemory.translated.net/get?q={Uri.EscapeDataString(text)}&langpair=en|es";
                var json = _client.GetStringAsync(url).GetAwaiter().GetResult();
                using var doc = JsonDocument.Parse(json);
                var translated = doc.RootElement.GetProperty("responseData").GetProperty("translatedText").GetString();

                if (!string.IsNullOrEmpty(translated))
                {
                    _cache[text] = translated;
                    return translated;
                }
            }
            catch
            {
                // ignore errors and fall back to original text
            }

            return text;
        }
    }
}

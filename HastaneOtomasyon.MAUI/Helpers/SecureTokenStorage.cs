namespace HastaneOtomasyon.MAUI.Helpers
{
    /// <summary>
    /// Mac Catalyst derlemesinde Keychain (SecureStorage) yerine Preferences kullanilir.
    /// </summary>
    public static class SecureTokenStorage
    {
        private static bool UsePreferences =>
#if MACCATALYST
            true;
#else
            DeviceInfo.Platform == DevicePlatform.MacCatalyst;
#endif

        public static Task SetAsync(string key, string value)
        {
            if (UsePreferences)
            {
                Preferences.Default.Set(key, value);
                return Task.CompletedTask;
            }

            return SecureStorage.Default.SetAsync(key, value);
        }

        public static Task<string?> GetAsync(string key)
        {
            if (UsePreferences)
            {
                return Task.FromResult(Preferences.Default.Get(key, (string?)null));
            }

            return SecureStorage.Default.GetAsync(key);
        }

        public static void Remove(string key)
        {
            if (UsePreferences)
            {
                Preferences.Default.Remove(key);
                return;
            }

            SecureStorage.Default.Remove(key);
        }
    }
}

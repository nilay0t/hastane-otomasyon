using System.Net.Http.Headers;

namespace HastaneOtomasyon.MAUI.Helpers
{
    public static class HttpClientHelper
    {
        private const string TokenKey = "auth_token";

        public static async Task SaveTokenAsync(string token)
        {
            await SecureTokenStorage.SetAsync(TokenKey, token);
        }

        public static async Task<string?> GetTokenAsync()
        {
            return await SecureTokenStorage.GetAsync(TokenKey);
        }

        public static void RemoveToken()
        {
            SecureTokenStorage.Remove(TokenKey);
        }

        
        public static async Task SetAuthHeaderAsync(HttpClient client)
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                client.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }

       
        public static void ClearAuthHeader(HttpClient client)
        {
            client.DefaultRequestHeaders.Authorization = null;
        }
    }
}
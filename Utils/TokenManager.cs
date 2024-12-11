namespace ChatApp.Utils
{
    public class TokenManager
    {
        private static readonly Dictionary<string, TokenInfo> TOKEN_MANAGER = new Dictionary<string, TokenInfo>();
        private static SemaphoreSlim mutex = new SemaphoreSlim(1, 1);
        public static TokenInfo getTokenInfoByUser(string userName)
        {
            mutex.Wait();

            try
            {
                if (TOKEN_MANAGER.Values != null && TOKEN_MANAGER.Values.Count > 0)
                {
                    foreach (var token in TOKEN_MANAGER.Values)
                    {
                        if (token.UserName.Equals(userName))
                        {
                            return token;
                        }
                    }
                }

                return null;
            }
            finally
            {
                mutex.Release();
            }
        }

        public static TokenInfo getTokenInfoByToken(string token)
        {
            mutex.Wait();

            try
            {
                if (!string.IsNullOrEmpty(token) && TOKEN_MANAGER.ContainsKey(token))
                {
                    return TOKEN_MANAGER[token];
                }
            }
            finally
            {
                mutex.Release();
            }

            return null;
        }

        public static void addToken(TokenInfo tokenInfo)
        {
            mutex.Wait();

            try
            {
                if (tokenInfo != null)
                {
                    if (TOKEN_MANAGER.ContainsKey(tokenInfo.Token))
                    {
                        TOKEN_MANAGER.Remove(tokenInfo.Token);
                    }

                    TOKEN_MANAGER.Add(tokenInfo.Token, tokenInfo);
                }
            }
            finally
            {
                mutex.Release();
            }
        }

        public static void removeToken(string token)
        {
            mutex.Wait();

            try
            {
                if (!string.IsNullOrEmpty(token))
                {
                    if (TOKEN_MANAGER.ContainsKey(token))
                    {
                        TOKEN_MANAGER.Remove(token);
                    }
                }
            }
            finally
            {
                mutex.Release();
            }
        }

        public static List<TokenInfo> clearToken()
        {
            mutex.Wait();
            var lstCleared = new List<TokenInfo>();

            try
            {
                var tokenKeys = TOKEN_MANAGER.Keys;
                var now = DateTime.Now;

                foreach (var key in tokenKeys)
                {
                    var value = TOKEN_MANAGER[key];

                    if(value.IsExpired())
                    {
                        TOKEN_MANAGER.Remove(key);
                        lstCleared.Add(value);
                    }    
                }
            }
            finally
            {
                mutex.Release();
            }

            return lstCleared;
        }
    }
}
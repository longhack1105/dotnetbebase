using TWChatAppApiMaster.Models;

namespace TWChatAppApiMaster.SecurityManagers
{
    public class TokenStore
    {
        private static List<TokenModel> _tokens = new List<TokenModel>();
        public static void AddToken(TokenModel token)
        {
            _tokens.Add(token);
        }

        public static void UpdateToken(TokenModel token)
        {
            var data = GetBySessionUuid(token.SessionUuid);
            if (data != null)
            {
                _tokens.Remove(data);
                _tokens.Add(token);
            }
        }

        public static List<string>? ClearToken()
        {
            var lstCleared = _tokens.Where(x => x.TimeExpiredRefresh < DateTime.Now).ToList();
            _tokens.RemoveAll(x => lstCleared.Contains(x));

            return lstCleared.Select(x => x.SessionUuid).ToList();
        }

        public static TokenModel? GetByToken(string accessToken)
        {
            return _tokens.Find(t => t.AccessToken == accessToken);
        }

        public static TokenModel? GetByRefreshToken(string refreshToken)
        {
            return _tokens.Find(t => t.RefreshToken == refreshToken);
        }

        public static TokenModel? GetBySessionUuid(string sessionUuid)
        {
            return _tokens.Find(t => t.SessionUuid == sessionUuid);
        }
    }
}

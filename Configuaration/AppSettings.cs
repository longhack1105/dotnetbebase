namespace ChatApp.Configuaration
{
    public class Database
    {
        public string QuanLyTrungTam { get; set; } = string.Empty;
    }
    public class JwtTokenSettings
    {
        public string ValidAudience { get; set; } = string.Empty;
        public string ValidIssuer { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public double TokenValidityTime { get; set; } = 1;
        public double RefreshTokenExpirationTime { get; set; } = 30;
    }
    public class AppSettings
    {
        public Database Database { get; set; }
        public string DPS_CERT { get; set; }
        public string ANSender { get; set; } = string.Empty;
        public string UploadPath { get; set; }
        public string ContentRootPath { get; set; }
        public JwtTokenSettings TokenSettings { get; set; } = new JwtTokenSettings();
        public string KeyHash { get; set; } = string.Empty;
    }
}

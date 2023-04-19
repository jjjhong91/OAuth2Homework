namespace OAuth2Homework
{
    public class AppSettings
    {
        public LineLoginSettings LineLogin { get; set; }
        public LineNotifySettings LineNotify { get; set; }
    }



    public class Auth2Settings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUrl { get; set; }
    }

    public class LineLoginSettings : Auth2Settings
    {
    }

    public class LineNotifySettings : Auth2Settings
    {
    }
}
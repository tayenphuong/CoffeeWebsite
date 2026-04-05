namespace WebBanNuocMVC.DesignPatterns.Adapter
{
    public class TelegramNotificationAdapter : INotificationAdapter
    {
        private readonly HttpClient _httpClient;
        private const string BotToken = "8382921586:AAGMttI9pQrhWUmFep2fQxpEInsAUpoB6Fo";

        public TelegramNotificationAdapter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task SendAsync(string to, string subject, string body)
        {
            // 'to' ở đây chính là Chat ID của người dùng
            var message = $"<b>{subject}</b>\n{body}";
            var url = $"https://api.telegram.org/bot{BotToken}/sendMessage?chat_id=7631953076&text={Uri.EscapeDataString(message)}&parse_mode=HTML" +
                $"";

            await _httpClient.GetAsync(url);
        }
    }
}

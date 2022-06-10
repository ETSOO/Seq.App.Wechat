using com.etsoo.WeiXinService;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System.Net.Http.Json;
using System.Web;

namespace Seq.App.Wechat
{
    /// <summary>
    /// Wechat app
    /// 微信主程序
    /// </summary>
    [SeqApp("Wechat App", Description = "Publish messages to Wechat service account. Go https://github.com/ETSOO/Seq.App.Wechat for setup help.")]
    public class WechatApp : SeqApp, ISubscribeToAsync<LogEventData>
    {
        const string Service = "Seq";

        private DateTime lastRunAt;

        protected DateTime LastRunAt
        {
            get { lock (this) return lastRunAt; }
            set { lock (this) lastRunAt = value; }
        }

        [SeqAppSetting(
            IsOptional = false,
            DisplayName = "Token(s)",
            InputType = SettingInputType.Password,
            HelpText = "Follow the WeChat service account 'etsoo2004', click the menu to obtain it. Multiple tokens are separated by semicolons.")]
        public string? Tokens { get; set; }

        /// <summary>
        /// Method is called each time a log event is processed by SEQ.
        /// 每次SEQ处理一个日志事件时，都会调用此方法。
        /// </summary>
        /// <param name="evt">Event</param>
        public async Task OnAsync(Event<LogEventData> evt)
        {
            // 忽略三分钟内的重复记录
            var ts = DateTime.Now - LastRunAt;
            if (ts.TotalMinutes < 3) return;

            // 更新时间
            LastRunAt = DateTime.Now;

            // 发送
            await SendAsync(evt);
        }

        private async Task SendAsync(Event<LogEventData> evt)
        {
            // 验证数据
            if (string.IsNullOrEmpty(Tokens)) return;

            // 发送的数据
            var message = evt.Data.RenderedMessage;
            if (message.Length > 512) message = message[..512];
            var data = new LogAlertDto
            {
                Tokens = Tokens.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(token => token.Trim()).ToArray(),
                Service = Service,
                Id = evt.Id,
                Level = evt.Data.Level.ToString(),
                Message = message,
                Datetime = evt.Data.LocalTimestamp.LocalDateTime
            };

            // 哈希
            var (json, signature) = await ServiceUtils.SerializeAsync(data);

            using var client = new HttpClient();
            var response = await client.PostAsJsonAsync("https://wechatapi.etsoo.com/api/Service/LogAlert/" + HttpUtility.UrlEncode(signature), new StreamContent(json));
            if (!response.IsSuccessStatusCode)
            {
                // 如果失败如何记录日志？
            }
        }
    }
}

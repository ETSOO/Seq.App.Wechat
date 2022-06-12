using com.etsoo.WeiXinService;
using Seq.Apps;
using Seq.Apps.LogEvents;

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
            if (string.IsNullOrEmpty(Tokens))
            {
                return;
            }

            var tokenItems = Tokens.Split(';', StringSplitOptions.RemoveEmptyEntries).Select(token => token.Trim()).ToArray();
            var tokenCount = tokenItems.Length;
            if (tokenCount == 0)
            {
                Log.Warning("Seq.App.Wechat failed - no token");
                return;
            }

            // 发送的数据
            var data = new LogAlertDto
            {
                Tokens = tokenItems,
                Service = Service,
                Id = evt.Id,
                Level = evt.Data.Level.ToString(),
                Message = evt.Data.RenderedMessage,
                Datetime = evt.Data.LocalTimestamp
            };

            // 发送
            using var client = new HttpClient();
            var response = await ServiceUtils.SendLogAlertAsync(data, client);
            if (!response.IsSuccessStatusCode)
            {
                // 记录日志
                var code = (int)response.StatusCode;
                var status = response.StatusCode.ToString();
                var content = await response.Content.ReadAsStringAsync();
                Log.Warning("Seq.App.Wechat failed - {status} ({code}), {tokenCount}, {content}", status, code, tokenCount, content);
            }
        }
    }
}

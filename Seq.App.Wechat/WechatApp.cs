using Seq.Apps;
using Seq.Apps.LogEvents;
using System.Net;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace Seq.App.Wechat
{
    /// <summary>
    /// Main App
    /// 主程序
    /// </summary>
    [SeqApp("Wechat App", Description = "Publish messages to Wechat official account")]
    public class WechatApp : SeqApp, ISubscribeTo<LogEventData>
    {
        // Event queue
        // 事件队列
        static readonly BufferBlock<Event<LogEventData>> queue = new ();

        // Open id
        // 微信服务号用户编号
        static string? openId;

        /// <summary>
        /// Sending count
        /// 发送计数
        /// </summary>
        public static int Count = 0;

        // Static constructor
        // 静态构造函数
        static WechatApp()
        {
            Task.Run(async () =>
            {
                while(true)
                {
                    // No open id
                    if (string.IsNullOrEmpty(openId))
                        continue;

                    // Available item
                    var item = await queue.ReceiveAsync();
                    if (item != null)
                    {
                        // Send it
                        await SendAsync(item);
                    }
                }
            });
        }

        // Send
        // 发送
        static async Task SendAsync(Event<LogEventData> evt)
        {
            // 发送邮件测试
            using var client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential("alert.etsoo@gmail.com", "Etsoo@2004");

            using var message = new MailMessage("alert.etsoo@gmail.com", "xz@etsoo.com", "测试邮件", evt.Data.RenderedMessage);
            await client.SendMailAsync(message);

            // Add to the count
            Interlocked.Increment(ref Count);
        }

        [SeqAppSetting(
            IsOptional = false,
            DisplayName = "用户编号",
            HelpText = "识别用户身份，可以多个 Seq 程序共用同一编号")]
        public string? OpenId
        {
            get { return openId; }
            set { openId = value; }
        }

        /// <summary>
        /// Method is called each time a log event is processed by SEQ.
        /// 每次SEQ处理一个日志事件时，都会调用此方法。
        /// </summary>
        /// <param name="evt">Event</param>
        public void On(Event<LogEventData> evt)
        {
            Task.Run(async () =>
            {
                await queue.SendAsync(evt);
            });
        }
    }
}

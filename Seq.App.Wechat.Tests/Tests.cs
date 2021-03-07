using NUnit.Framework;
using Seq.Apps;
using Seq.Apps.LogEvents;
using System;
using System.Threading;

namespace Seq.App.Wechat.Tests
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void LifecycleTest()
        {
            // Arrange
            var data1 = new Event<LogEventData>(Guid.NewGuid().ToString(), 10, DateTime.UtcNow, new LogEventData
            {
                Exception = null,
                Level = LogEventLevel.Fatal,
                LocalTimestamp = DateTime.UtcNow,
                MessageTemplate = "Some text 1",
                RenderedMessage = "Some text 1",
                Properties = null
            });

            // Act
            var app = new WechatApp();
            app.OpenId = Guid.NewGuid().ToString();

            // First time
            app.On(data1);

            var data2 = new Event<LogEventData>(Guid.NewGuid().ToString(), 10, DateTime.UtcNow, new LogEventData
            {
                Exception = null,
                Level = LogEventLevel.Fatal,
                LocalTimestamp = DateTime.UtcNow,
                MessageTemplate = "Some text 2",
                RenderedMessage = "Some text 2",
                Properties = null
            });

            // Second time
            app.On(data2);

            // Sleep 12 seconds to wait the results
            Thread.Sleep(12000);

            // Assert
            Assert.IsTrue(WechatApp.Count == 2);
        }
    }
}
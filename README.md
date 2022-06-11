# Seq.App.Wechat
ETSOO's Seq App for message publish to Wechat service account.
亿速思维开发的用于消息发布到微信服务帐户的 Seq App 。
快速提醒，提高运维反应速度和效率，3分钟提醒一次，避免信息轰炸。

## 关注微信服务号 etsoo2004
![服务号二维码](https://cn.etsoo.com/qrcode.jpg "服务号二维码")

从菜单的“服务”=>“访问令牌”，获取授权码，有效期为一年。

## Seq 配置
- 从官网下载安装 Seq：https://datalust.co/seq
- 进入管理后台：http://localhost:5341/
- 从“Settings" => "Apps"，"INSTALL FROM NUGET", "Package id" 输入 "Seq.App.Wechat"，点按钮 "INSTALL"。
- 回到 "Apps" 界面，点 "ADD INSTANCE"，勾选 "Title" 下方的 "Stream incoming events"，并选择要推送的事件类型。
- 在 "Token(s)" 中输入刚才获取的令牌（注意不要包括说明的中文文字），多个令牌之间用分号隔开。

## Serilog + Seq
- 在线帮助：https://docs.datalust.co/docs/using-serilog
- appsettings.json 配置示例 
```json
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"
    },
    "Enrich": [
      "FromLogContext",
      "WithExceptionDetails"
    ],
    "WriteTo": [
      {
        "Name": "Seq",
        "Args": { "serverUrl": "http://localhost:5341" }
      }
    ]
  }
```
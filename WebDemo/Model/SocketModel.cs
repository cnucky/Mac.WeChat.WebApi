using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Web;
using WebDemo.WeChat;

namespace WebDemo.Model
{
    public class SocketModel
    {
        public string action { get; set; }
        public string context { get; set; }
    }

    public class DicSocket {
        public WebSocket socket;
        public XzyWeChatThread weChatThread;
    }
}
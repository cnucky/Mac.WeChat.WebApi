using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDemo.Model
{
    public class ApiServerMsg
    {
        public bool Success { get; set; }

        public Object Context { get; set; }

        public string ErrContext { get; set; }
    }

    public class BaseModel {
        /// <summary>
        /// 创建websocket的uuid唯一标识
        /// </summary>
        public string uuid { get; set; }
    }

    /// <summary>
    /// 发送文字消息实体类
    /// </summary>
    public class SendTextModel: BaseModel
    {
        /// <summary>
        /// 微信ID
        /// </summary>
        public string wxid { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string text { get; set; }
    }

    /// <summary>
    /// 发送图片消息实体类
    /// </summary>
    public class SendImgModel : BaseModel
    {
        /// <summary>
        /// 微信ID
        /// </summary>
        public string wxid { get; set; }

        /// <summary>
        /// 消息内容
        /// </summary>
        public string base64 { get; set; }
    }

    /// <summary>
    /// 发送图片消息实体类
    /// </summary>
    public class SendAppModel : BaseModel
    {
        /// <summary>
        /// 微信ID
        /// </summary>
        public string wxid { get; set; }

        public string appid { get; set; }

        public string sdkver { get; set; }

        public string title { get; set; }

        public string des { get; set; }

        public string url { get; set; }

        public string thumburl { get; set; }
    }

    public class GroupCreatModel : BaseModel
    {
        /// <summary>
        /// 好友wxid ["wxid_aaa","wxid_bbb"]
        /// </summary>
        public string users { get; set; }
    }

    public class GroupQuickModel : BaseModel
    {
        /// <summary>
        /// 群id
        /// </summary>
        public string chatroomid { get; set; }
    }
}
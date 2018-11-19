using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebDemo.Model
{
    public class SocketStart62 {
        public string username { get; set; }
        public string password { get; set; }
        public string str62 { get; set; }
    }

    public class ApiServerMsg
    {
       /// <summary>
       /// 是否成功标记
       /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 成功返回内容
        /// </summary>
        public Object Context { get; set; }

        /// <summary>
        /// 失败返回内容
        /// </summary>
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
    /// 发送文字消息实体类
    /// </summary>
    public class SendMassModel : BaseModel
    {
        /// <summary>
        /// 用户名json数组 ["AB1","AC2","AD3"]
        /// </summary>
        public string wxids { get; set; }

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

        /// <summary>
        /// 标题
        /// </summary>
        public string title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string des { get; set; }

        /// <summary>
        /// 链接地址
        /// </summary>
        public string url { get; set; }

        /// <summary>
        /// 图片地址
        /// </summary>
        public string thumburl { get; set; }
    }

    public class GroupCreatModel : BaseModel
    {
        /// <summary>
        /// 好友wxid ["wxid_aaa","wxid_bbb"]，不小于3人且包含自己
        /// </summary>
        public string users { get; set; }
    }

    /// <summary>
    /// 群信息
    /// </summary>
    public class GroupModel : BaseModel
    {
        /// <summary>
        /// 群id
        /// </summary>
        public string chatroomid { get; set; }
    }

    public class GroupUserModel: GroupModel
    {
        /// <summary>
        /// 用户微信id集合
        /// </summary>
        public string user { get; set; }
    }

    /// <summary>
    /// 群信息
    /// </summary>
    public class GroupNameModel : GroupModel
    {
        /// <summary>
        /// 群名称
        /// </summary>
        public string name { get; set; }
    }

    /// <summary>
    /// 群信息
    /// </summary>
    public class GroupAnnouncementModel : GroupModel
    {
        /// <summary>
        /// 群公告
        /// </summary>
        public string context { get; set; }
    }


    public class FansGetNearModel : BaseModel {
        public float lat { get; set; }
        public float lng { get; set; }
    }

    public class FansSearchModel : BaseModel
    {
        /// <summary>
        /// 搜索条件  QQ号 手机号 微信号
        /// </summary>
        public string search { get; set; }    
    }

    public class FansAddModel : BaseModel
    {
        public string v1 { get; set; }

        public string v2 { get; set; }

        /// <summary>
        /// 1   -通过QQ好友添加--可以
        /// 2   -通过搜索邮箱--可加但无提示
        /// 3   -通过微信号搜索--可以
        /// 5   -通过朋友验证消息-可加但无提示
        /// 7   -通过朋友验证消息(可回复)-可加但无提示
        /// 12  -来自QQ好友--可以
        /// 13  -通过手机通讯录添加--可以
        /// 14  -通过群来源--no
        /// 15  -通过搜索手机号--可以
        /// 16  -通过朋友验证消息-可加但无提示
        /// 17  -通过名片分享--no
        /// 18  -通过附近的人--可以(貌似只需要v1就够了)
        /// 22  -通过摇一摇打招呼方式--可以
        /// 25  -通过漂流瓶---no
        /// 30  -通过二维码方式--可以
        /// </summary>
        public int type { get; set; }

        /// <summary>
        /// 打招呼语句
        /// </summary>
        public string hellotext { get; set; }
    }

    public class ContactModel : BaseModel
    {
        /// <summary>
        /// 微信id
        /// </summary>
        public string wxid { get; set; }
    }

    public class ContactRemarkModel : ContactModel
    {
        /// <summary>
        /// 备注
        /// </summary>
        public string remark { get; set; }
    }

    public class SnsModel : BaseModel
    {
        /// <summary>
        /// 朋友圈id
        /// </summary>
        public string snsid { get; set; }
    }

    public class SnsComment : SnsModel
    {
        /// <summary>
        /// 评论内容
        /// </summary>
        public string context { get; set; }

        /// <summary>
        /// 回复id
        /// </summary>
        public int replyid { get; set; }
    }

    public class SnsUserModel : SnsModel
    {
        /// <summary>
        /// wxid
        /// </summary>
        public string wxid { get; set; }
    }

    public class SnsSendTextModel : BaseModel
    {
        /// <summary>
        /// 朋友圈文字内容
        /// </summary>
        public string text { get; set; }
    }

    public class SnsSendImageTextModel : SnsSendTextModel
    {
        /// <summary>
        /// 朋友圈图片 base64 数组 不超过9张
        /// </summary>
        public List<string> base64list { get; set; }
    }

    public class GhModel : BaseModel
    {
        /// <summary>
        /// 公众号id  gh_xxxxxxx
        /// </summary>
        public string ghid { get; set; }
    }

    public class GhSubscriptionCommandModel : GhModel
    {
        /// <summary>
        /// 公众号uin
        /// </summary>
        public string uin { get; set; }

        /// <summary>
        /// 公众号key
        /// </summary>
        public string key { get; set; }
    }

    public class GhRequestUrl : GhSubscriptionCommandModel {
        /// <summary>
        /// 阅读链接地址
        /// </summary>
        public string url { get; set; }
    }

    public class GhSearchModel : BaseModel
    {
        /// <summary>
        /// 公众号名称
        /// </summary>
        public string name { get; set; }
    }

    public class LabelModel: BaseModel
    {
        /// <summary>
        /// 标签id
        /// </summary>
        public string labelid { get; set; }
    }

    public class LabelSetModel : LabelModel
    {
        /// <summary>
        /// wxid
        /// </summary>
        public string wxid { get; set; }
    }

    public class LabelAddModel : BaseModel
    {
        /// <summary>
        /// 标签名称
        /// </summary>
        public string name { get; set; }
    }

    public class FavModel : BaseModel
    {
        /// <summary>
        /// 收藏key
        /// </summary>
        public string favkey { get; set; }
    }

    public class FavAddModel : BaseModel
    {
        /// <summary>
        /// <favitem type="1"><desc>我这辈子最佩服的只有两个人一个是群主，还有一个，就是免死[抱拳][抱拳][抱拳]</desc><ctrlflag>127</ctrlflag><source sourcetype="1" sourceid="5247349643135372738"><fromusr>wxid_j7rwo75glpzw22</fromusr><tousr>7459655793@chatroom</tousr><createtime>1474781339</createtime><msgid>5247349643135372738</msgid></source></favitem>
        /// </summary>
        public string favObject { get; set; }
    }

    public class FavSelectModel : BaseModel
    {
        /// <summary>
        /// 收藏id
        /// </summary>
        public string favid { get; set; }
    }

    public class UserSetWxidModel : BaseModel
    {
        /// <summary>
        /// wxid
        /// </summary>
        public string wxid { get; set; }
    }

    public class UserSetUserInfoModel : BaseModel
    {
        /// <summary>
        /// 昵称
        /// </summary>
        public string nickname { get; set; }

        /// <summary>
        /// 签名
        /// </summary>
        public string sign { get; set; }

        /// <summary>
        /// 性别 0/1
        /// </summary>
        public int sex { get; set; }

        /// <summary>
        /// 国籍 CN
        /// </summary>
        public string country { get; set; }

        /// <summary>
        /// 省份 guangdong
        /// </summary>
        public string provincia { get; set; }

        /// <summary>
        /// 市 guangzhou
        /// </summary>
        public string city { get; set; }
    }
}
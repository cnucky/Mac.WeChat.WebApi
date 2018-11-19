using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace WebDemo.Util
{
    public static class EUtils
    {
        #region 易语言 Utils 处理中文乱码
        /// <summary>
        /// 发送文字朋友圈
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="str"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int ESendSNS(int wxuser, string str);

        /// <summary>
        /// 发送朋友圈图片
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="xml"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int ESendSNSImage(int wxuser, string xml, string context);

        /// <summary>
        /// 设置群公告
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="wxid"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int ESetChatroomAnnouncement(int wxuser, string wxid, string context);

        /// <summary>
        /// 设置群名称
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="wxid"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int ESetChatroomName(int wxuser, string wxid, string name);

        /// <summary>
        /// 发送名片
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="wxid"></param>
        /// <param name="fromwxid"></param>
        /// <param name="caption"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int EShareCarde(int wxuser, string wxid, string fromwxid, string caption);

        /// <summary>
        /// 朋友圈评论
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="wxid"></param>
        /// <param name="snsid"></param>
        /// <param name="context"></param>
        /// <param name="replyid"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int ESnsComment(int wxuser, string wxid, string snsid, string context, int replyid);

        /// <summary>
        /// 添加用户
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="type"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int EAddUser(int wxuser, string v1, string v2, int type, string context);

        /// <summary>
        /// 设置备注
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="wxid"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int ESetUserRemark(int wxuser, string wxid, string context);

        /// <summary>
        /// 打招呼
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="v1"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int ESayHello(int wxuser, string v1, string context);

        /// <summary>
        /// 添加标签
        /// </summary>
        /// <param name="wxuser"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern int EAddContactLabel(int wxuser, string context);

        /// <summary>
        /// 16进制转字符串
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern string EStrToHex(string context);

        /// <summary>
        /// 字符串转16进制
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        [DllImport("EUtils.dll")]
        public static extern string EHexToStr(string context);
        #endregion
    }
}

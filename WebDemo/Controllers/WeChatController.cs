using WebDemo.Model;
using WebDemo.WeChat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.WebSockets;
using Newtonsoft.Json;
using System.Drawing;
using WebDemo.Util;

namespace WebDemo.Controllers
{
    /// <summary>
    /// 微信模块
    /// </summary>
    [RoutePrefix("api/wechat")]
    [Error]
    public class WeChatController : ApiController
    {
        private static Dictionary<string, DicSocket> _dicSockets = new Dictionary<string, DicSocket>();

        /// <summary>
        /// 创建websocket连接
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("socket/connect")]
        public HttpResponseMessage Connect()
        {
            HttpContext.Current.AcceptWebSocketRequest(ProcessRequest); //在服务器端接受Web Socket请求，传入的函数作为Web Socket的处理函数，待Web Socket建立后该函数会被调用，在该函数中可以对Web Socket进行消息收发
            return Request.CreateResponse(HttpStatusCode.SwitchingProtocols); //构造同意切换至Web Socket的Response.
        }

        /// <summary>
        /// websocket监听
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task ProcessRequest(AspNetWebSocketContext context)
        {
            var socket = context.WebSocket;
            string uuid = context.QueryString["uuid"].ToString();
            XzyWeChatThread xzy = null;
            DicSocket dicSocket = new DicSocket()
            {
                socket = socket,
                weChatThread = xzy
            };
            if (_dicSockets.ContainsKey(uuid))
            {
                try
                {
                    await _dicSockets[uuid].socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);//如果client发起close请求，对client进行ack
                }
                catch (Exception ex)
                {
                    LogServer.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "socketErr:" + ex.Message);
                }
            }
            _dicSockets.Add(uuid, dicSocket);
            while (true)
            {
                var buffer = new ArraySegment<byte>(new byte[1024]);
                var receivedResult = await socket.ReceiveAsync(buffer, CancellationToken.None);//对web socket进行异步接收数据
                if (receivedResult.MessageType == WebSocketMessageType.Close)
                {
                    try
                    {
                        await socket.CloseAsync(WebSocketCloseStatus.Empty, string.Empty, CancellationToken.None);//如果client发起close请求，对client进行ack
                    }
                    catch (Exception ex)
                    {
                        LogServer.Info(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "socketErr:" + ex.Message);
                    }
                    break;
                }
                if (socket.State == System.Net.WebSockets.WebSocketState.Open)
                {
                    string recvMsg = Encoding.UTF8.GetString(buffer.Array, 0, receivedResult.Count);
                    SocketModel model = JsonConvert.DeserializeObject<SocketModel>(recvMsg);
                    switch (model.action.ToLower())
                    {
                        case "start"://创建socket
                            await Task.Factory.StartNew(() =>
                            {
                                xzy = new XzyWeChatThread(socket);
                            });
                            break;
                    }
                }
            }
        }


        #region 发送消息

        /// <summary>
        /// 发送文字消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("msg/sendtext")]
        public IHttpActionResult SendText(SendTextModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try {
                if (_dicSockets.ContainsKey(model.uuid)) {
                    var res=_dicSockets[model.uuid].weChatThread.Wx_SendMsg(model.wxid, model.text);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else{
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }
              
            } catch (Exception e) {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }    
        }

        /// <summary>
        /// 发送图片消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("msg/sendimg")]
        public IHttpActionResult SendImg(SendImgModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    Image img = ConvertUtils.GetImageFromBase64(model.base64);
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SendImg(model.wxid, img);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 发送链接消息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("msg/sendapp")]
        public IHttpActionResult SendApp(SendAppModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {

                    string xml = App.AppMsgXml.
               Replace("$appid$", model.appid).
                Replace("$sdkver$", model.sdkver).
                 Replace("$title$", model.title).
                  Replace("$des$", model.des).
                   Replace("$url$", model.url).
                    Replace("$thumburl$", model.thumburl);
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SendAppMsg(model.wxid, xml);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        #endregion 发送消息

        #region 群模块

        /// <summary>
        /// 创建群，好友微信id ["wxid_aaa","wxid_bbb"] 必须大于3人且包含自己
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("group/creat")]
        public IHttpActionResult GroupCreat(GroupCreatModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_CreateChatRoom(model.users);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 退出群
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("group/quick")]
        public IHttpActionResult GroupQuick(GroupModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_QuitChatRoom(model.chatroomid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 获取群成员资料
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("group/getmember")]
        public IHttpActionResult GroupGetMember(GroupModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_GetChatRoomMember(model.chatroomid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 添加群成员 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("group/addmember")]
        public IHttpActionResult GroupAddMember(GroupUserModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_AddChatRoomMember(model.chatroomid,model.user);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 邀请群成员
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("group/invitemember")]
        public IHttpActionResult GroupInviteMember(GroupUserModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_InviteChatRoomMember(model.chatroomid, model.user);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 踢出群成员
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("group/delmember")]
        public IHttpActionResult GroupDelMember(GroupUserModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_DeleteChatRoomMember(model.chatroomid, model.user);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 修改群名称
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("group/updatename")]
        public IHttpActionResult GroupUpdateName(GroupNameModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SetChatroomName(model.chatroomid, model.name);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 修改群公告
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("group/updateannouncement")]
        public IHttpActionResult GroupUpdateAnnouncement(GroupAnnouncementModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SetChatroomAnnouncement(model.chatroomid, model.context);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        #endregion 群模块

        #region 加粉引流
        /// <summary>
        /// 查看附近的人
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("fans/getnear")]
        public IHttpActionResult FansGetNear(FansGetNearModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_GetPeopleNearby(model.lat, model.lng);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 搜索用户信息，支持手机号 微信号 qq号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("fans/search")]
        public IHttpActionResult FansSearch(FansSearchModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SearchContact(model.search);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 添加粉丝
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("fans/add")]
        public IHttpActionResult FansAdd(FansAddModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_AddUser(model.v1,model.v2,model.type,model.hellotext);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }
        #endregion 加粉引流

        #region 好友模块

        /// <summary>
        /// 获取好友详情
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("contact/get")]
        public IHttpActionResult ContactGet(ContactModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_GetContact(model.wxid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 设置好友备注
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("contact/setremark")]
        public IHttpActionResult ContactSetRemark(ContactRemarkModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SetUserRemark(model.wxid,model.remark);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }


        /// <summary>
        /// 删除好友
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("contact/delete")]
        public IHttpActionResult ContactDelete(ContactModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_DeleteUser(model.wxid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        #endregion 好友模块

        #region 朋友圈模块

        /// <summary>
        /// 发送文字朋友圈
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sns/sendtext")]
        public IHttpActionResult SnsSendText(SnsSendTextModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SendMoment(model.text);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 发送图文朋友圈
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sns/sendimgtext")]
        public IHttpActionResult SnsSendImageText(SnsSendImageTextModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SendMoment(model.text,model.base64list);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 获取朋友圈，snsid第一次传空，后面传最后一条id进行翻页查看
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sns/gettimeline")]
        public IHttpActionResult SnsGetTimeLine(SnsModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SnsTimeline(model.snsid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 查看指定好友朋友圈,snsid 第一次传空，后面传最后一条id进行翻页查看
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sns/getuserpage")]
        public IHttpActionResult SnsGetUserPage(SnsUserModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SnsUserPage(model.wxid,model.snsid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 获取朋友圈详情
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sns/objectdetail")]
        public IHttpActionResult SnsObjectDetail(SnsModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SnsObjectDetail(model.snsid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 朋友圈评论
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("sns/comment")]
        public IHttpActionResult SnsComment(SnsComment model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SnsComment(model.snsid,model.context,model.replyid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        #endregion 朋友圈模块

        #region  公众号

        /// <summary>
        /// 关注公众号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("gh/follow")]
        public IHttpActionResult GhFlower(GhModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_AddUser(model.ghid, "", 0, "");
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 搜索公众号
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("gh/search")]
        public IHttpActionResult GhSearch(GhSearchModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_WebSearch(model.name);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 执行公众号菜单
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("gh/subscriptioncommand")]
        public IHttpActionResult SubscriptionCommand(GhSubscriptionCommandModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SubscriptionCommand(model.ghid,uint.Parse(model.uin),model.key);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }

        /// <summary>
        /// 阅读链接
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("gh/requesturl")]
        public IHttpActionResult RequestUrl(GhRequestUrl model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_RequestUrl(model.url, model.uin, model.key);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }

        }


        #endregion 公众号

        #region 标签
        /// <summary>
        /// 获取所有标签
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("label/get")]
        public IHttpActionResult LabelGet(BaseModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_GetContactLabelList();
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 设置标签
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("label/set")]
        public IHttpActionResult LabelSet(LabelSetModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SetContactLabel(model.wxid,model.labelid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 创建标签
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("label/add")]
        public IHttpActionResult LabelAdd(LabelAddModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_AddContactLabel(model.name);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 删除标签
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("label/delete")]
        public IHttpActionResult LabelDelete(LabelModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_DeleteContactLabel(model.labelid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }


        #endregion 标签

        #region 收藏

        /// <summary>
        /// 同步收藏
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("fav/sync")]
        public IHttpActionResult FavSync(FavModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_FavSync(model.favkey);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 添加收藏
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("fav/add")]
        public IHttpActionResult FavAdd(FavAddModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_FavAddItem(model.favObject);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 查看收藏
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("fav/select")]
        public IHttpActionResult FavSelect(FavSelectModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_FavGetItem(model.favid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }


        #endregion 收藏

        #region 个人信息
        /// <summary>
        /// 设置wxid
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("user/setwxid")]
        public IHttpActionResult UserSetWxid(UserSetWxidModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SetWeChatID(model.wxid);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        /// <summary>
        /// 设置个人信息
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("user/setuserinfo")]
        public IHttpActionResult UserSetUserInfo(UserSetUserInfoModel model)
        {
            ApiServerMsg result = new ApiServerMsg();
            try
            {
                if (_dicSockets.ContainsKey(model.uuid))
                {
                    var res = _dicSockets[model.uuid].weChatThread.Wx_SetUserInfo(model.nickname,model.sign,model.sex,model.country,model.provincia,model.city);
                    result.Success = true;
                    result.Context = res;
                    return Ok(result);
                }
                else
                {
                    result.Success = false;
                    result.Context = "不存在该websocket连接";
                    return Ok(result);
                }

            }
            catch (Exception e)
            {
                result.Success = false;
                result.ErrContext = e.Message;
                return Ok(result);
            }
        }

        #endregion 个人信息
    }
}
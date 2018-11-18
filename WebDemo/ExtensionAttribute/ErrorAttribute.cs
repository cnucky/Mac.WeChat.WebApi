using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Filters;

namespace WebDemo
{
    public class ErrorAttribute: ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK);

            response.Content = new StringContent(Newtonsoft.Json.JsonConvert.SerializeObject((new ApiServerMsg() { Success = true, ErrContext = "服务崩啦，异常信息：" + actionExecutedContext.Exception.Message + " 详情查看日志" })), System.Text.Encoding.UTF8, "application/json");
            actionExecutedContext.Response = response;

            var ex = actionExecutedContext.Exception;
            LogServer.Error(ex.Message + "--" + ex.Source + "--" + ex.StackTrace);
            base.OnException(actionExecutedContext);
        }
    }
}
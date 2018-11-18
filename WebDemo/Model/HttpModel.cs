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
        public string uuid { get; set; }
    }

    public class SendTextModel: BaseModel
    {
        public string wxid { get; set; }

        public string text { get; set; }
    }
}
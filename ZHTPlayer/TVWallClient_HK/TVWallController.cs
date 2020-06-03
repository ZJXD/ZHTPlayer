using Model;
using Model.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace TVWallClient.UI
{
    /// <summary>
    /// 电视墙控制器
    /// </summary>
    public class TVWallController : ApiController
    {
        public delegate ResponseData PlayHandler(object sender, MessageArgs e);
        public static event PlayHandler PartEvent;

        public static ResponseData InFunction(string comMessage)
        {
            var messageArg = new MessageArgs(comMessage);
            return PartEvent?.Invoke(null, messageArg);         // 触发事件，执行所有注册过的函数
        }

        [HttpGet]
        public object OperateTV(string command)
        {
            try
            {
                ResponseData result = InFunction(command);
                return result;
            }
            catch (Exception e)
            {
                Logger.Default.Error($"执行：{command}，出现错误", e);
                return new ResponseData { Code = 1, MessageStr = "操作失败！" };
            }
        }
    }

    public class MessageArgs : EventArgs
    {
        public MessageArgs(string comMessage)
        {
            this.ComMessage = comMessage;
        }

        public string ComMessage { get; set; }

    }
}

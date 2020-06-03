using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.SelfHost;

namespace TVWallClient.UI
{
    public class InitConfig
    {
        public static HttpSelfHostConfiguration InitSelfHostConfig(string baseAddress)
        {
            // 配置 http 服务的路由
            HttpSelfHostConfiguration config = new HttpSelfHostConfiguration(baseAddress);
            config.Routes.MapHttpRoute(
                "API Default", "api/{controller}/{id}",
                new { id = RouteParameter.Optional }
            );

            // 设置跨域
            var cors = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(cors);

            config.Formatters
               .XmlFormatter.SupportedMediaTypes.Clear();

            // 默认返回 json
            config.Formatters
                .JsonFormatter.MediaTypeMappings.Add(
                new QueryStringMapping("datatype", "json", "application/json"));

            // 返回格式选择
            config.Formatters
                .XmlFormatter.MediaTypeMappings.Add(
                new QueryStringMapping("datatype", "xml", "application/xml"));

            // json 序列化设置
            config.Formatters
                .JsonFormatter.SerializerSettings = new
                Newtonsoft.Json.JsonSerializerSettings()
                {
                    //NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore,
                    DateFormatString = "yyyy-MM-dd HH:mm:ss"        // 设置时间日期格式化
                };
            return config;
        }
    }
}

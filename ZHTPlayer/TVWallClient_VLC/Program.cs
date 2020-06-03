using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.SelfHost;
using System.Windows.Forms;

namespace TVWallClient.UI
{
    static class Program
    {

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            string baseAddress = string.Format("http://{0}:{1}/",
                   //System.Configuration.ConfigurationManager.AppSettings.Get("Domain"),
                   "0.0.0.0",
                   System.Configuration.ConfigurationManager.AppSettings.Get("APIPort"));
            using (var server = new HttpSelfHostServer(InitConfig.InitSelfHostConfig(baseAddress)))
            {
                server.OpenAsync().Wait();
                Console.WriteLine(String.Format("host 已启动：{0}", baseAddress));
                App app = new App();
                app.Run();
            }
        }
    }
}

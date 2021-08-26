using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Diagnostics;
using OeipCommon.Http;
using Owin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(WinLiveManage.Startup))]
namespace WinLiveManage
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            app.Use<IndexMiddleware>();
            app.UseErrorPage(new ErrorPageOptions { SourceCodeLineCount = 20 });
            app.UseWelcomePage("/Welcome");
            var config = new HubConfiguration
            {
                EnableJSONP = true,
                EnableDetailedErrors = true,
                EnableJavaScriptProxies = true
            };
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR(config);
            //app.Map("/signalr", map =>
            //{
            //    map.UseCors(CorsOptions.AllowAll)
            //       .RunSignalR(config);
            //});
        }
    }
}

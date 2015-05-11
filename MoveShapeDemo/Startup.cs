using System;
using Microsoft.AspNet.SignalR;
using Microsoft.Owin;
using Newtonsoft.Json;
using Owin;

[assembly: OwinStartup(typeof(MoveShapeDemo.Startup))]
namespace MoveShapeDemo
{
    public class Startup
    {

        private static readonly Lazy<JsonSerializer> JsonSerializerFactory = new Lazy<JsonSerializer>(GetJsonSerializer);
        private static JsonSerializer GetJsonSerializer()
        {
            return new JsonSerializer
            {
                ContractResolver = new FilteredCamelCasePropertyNamesContractResolver
                {
                    // 1) Register all types in specified assemblies:
                    AssembliesToInclude =
                 {
                     typeof (Startup).Assembly
                 },
                    // 2) Register individual types:
                    //TypesToInclude =
                    //                {
                    //                    typeof(Hubs.Message),
                    //                }
                }
            };
        }

        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();

            GlobalHost.DependencyResolver.Register(
                typeof (JsonSerializer),
                () => JsonSerializerFactory.Value);
        }
    }
}

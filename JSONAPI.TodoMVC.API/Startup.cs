using System.Web.Http;
using JSONAPI.Core;
using JSONAPI.Payload;
using JSONAPI.TodoMVC.API.Models;
using Owin;
using PluralizationService = JSONAPI.Core.PluralizationService;

namespace JSONAPI.TodoMVC.API
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            var httpConfig = GetWebApiConfiguration();
            app.UseWebApi(httpConfig);
        }

        private static HttpConfiguration GetWebApiConfiguration()
        {
            var pluralizationService = new PluralizationService();
            pluralizationService.AddMapping("todo", "todos");
            var modelManager = new ResourceTypeRegistry(pluralizationService);
            modelManager.RegisterResourceType(typeof (Todo));

            var httpConfig = new HttpConfiguration();

            var linkConventions = new DefaultLinkConventions();

            // Configure JSON API
            new JsonApiConfiguration(modelManager, linkConventions)
                .Apply(httpConfig);

            // Web API routes
            httpConfig.Routes.MapHttpRoute("DefaultApi", "{controller}/{id}", new { id = RouteParameter.Optional });

            return httpConfig;
        }
    }
}
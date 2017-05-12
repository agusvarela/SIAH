using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.OData.Builder;
using System.Web.Http.OData.Extensions;
using SIAH.Models.Insumos;
namespace SIAH
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
           
            ODataConventionModelBuilder builder = new ODataConventionModelBuilder();
            builder.EntitySet<Insumo>("InsumosAPI");
            builder.EntitySet<TipoInsumo>("TiposInsumo");
            config.Routes.MapODataServiceRoute("odata", "odata", builder.GetEdmModel());
            config.MapHttpAttributeRoutes();

            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional }
            );
        }
    }
}

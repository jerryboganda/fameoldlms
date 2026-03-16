using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Swashbuckle.Swagger;
using System.Web.Http.Description;

namespace First_Aid_Made_Easy.Filters
{
    public class JzSwaggerControllerFilter : IDocumentFilter
    {
        public void Apply(SwaggerDocument swaggerDoc, SchemaRegistry schemaRegistry, IApiExplorer apiExplorer)
        {
            //var allowedControllers = new HashSet<string> { "StudentDataV2Controller", "StudentBankV2Controller" };

            //// Filter paths by controller names
            //swaggerDoc.paths = swaggerDoc.paths
            //    .Where(p => allowedControllers.Contains(p.Key.Split('/')[1]))
            //    .ToDictionary(p => p.Key, p => p.Value);
        }
    }

}
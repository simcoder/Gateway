using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GOC.ApiGateway.Controllers
{
    public class BaseController<T> : Controller where T : class
    {
        protected ILogger Logger;

        public BaseController(ILoggerFactory loggerFactory)
        {
            Logger = loggerFactory.CreateLogger<T>();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Model;
using Redis.OM;
using LoggerSpace;

namespace Task1.Controllers
{
    public class HomeController : Controller
    {
        private DataWork? dwork = null;
        private Log log = new(true, true);
        private ISystemSettings? settings = null;
        public HomeController(IServiceProvider servprovider)
        {
            var provider = servprovider.GetService<RedisConnectionProvider>();            
            settings = GetConfig();
            dwork = new(provider, log, settings);            
        }       

        [HttpPost]
        public async Task<ActionResult?> GetData([FromBody] FilterRequest request)
        {
            if (dwork == null || request==null) return null;
            
            var r = await dwork.GetItemsInfoExt(request, request.iPage, request.N);

            return Json((r!=null)? r.items:null);
        }

        [HttpPost]
        public async Task<ActionResult?> SaveItems([FromBody] Items[] list)
        {
            if (dwork == null || list==null) return null;
            var r = await dwork.SaveItems(list.ToArray());
            return Json(r);
        }
      
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error");//(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        private static SystemSettings GetConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                                          .AddJsonFile("appsettings.json")
                                          .AddEnvironmentVariables()
                                          .Build();
            SystemSettings settings = config.GetRequiredSection("SystemSettings").Get<SystemSettings>();
            return settings;
        }
    }
}
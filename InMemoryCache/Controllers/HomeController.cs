using InMemoryCache.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace InMemoryCache.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IMemoryCache _memoryCache;

        public HomeController(ILogger<HomeController> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public IActionResult SetValueSimple()
        {
            if (string.IsNullOrEmpty(_memoryCache.Get<string>("date")))
            {
                _memoryCache.Set<string>("date", DateTime.Now.ToString());
            }

            return Content("data cached");
        }

        public IActionResult SetValue()
        {
            if (!_memoryCache.TryGetValue("date", out string? dateCache))
            {
                MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();
                options.AbsoluteExpiration = DateTime.Now.AddMinutes(1); //1 dk sonra her türlü ramden silinir
                options.SlidingExpiration = TimeSpan.FromSeconds(10); // 10 sn içinde bu veriye eriþilirse eriþim süresi bi 10 saniye daha uzayacak, 10 sn hiç eriþilmezse ramden silinecek
                options.Priority = CacheItemPriority.Low;//ram dolarsa öncelikle silinecekler arasýnda yer alýr

                options.RegisterPostEvictionCallback((key, value, reason, state) => //ramden silinme durumunda çalýþacak metot
                {
                    _memoryCache.Set<string>("callback", $"{key} --> {value} => sebep : {reason} <> durum: {state}");
                });

                _memoryCache.Set<string>("date", DateTime.Now.ToString(), options);
            }
            else
            {
                return Content("data already cached : " + dateCache);
            }

            return Content("data cached");
        }

        public IActionResult SetObject()
        {
            _memoryCache.Set<UserObj>("user:1", new UserObj { Id = 1, Age = 33, FullName = "Selahattin Akan" });

            return Content("data cached");
        }

        public IActionResult GetValue()
        {
            string cacheData = _memoryCache.Get<string>("date")!;
            return Content("cached data: " + cacheData);
        }

        public IActionResult GetOrCreate()
        {
            //eðer cache yoksa cacheliyor, varsa deðeri okuyor
            string? cacheData = _memoryCache.GetOrCreate<string>("date", entry =>
            {
                return DateTime.Now.ToString();
            });
            return Content("cached data: " + cacheData);
        }

        public IActionResult GetCallbackValue()
        {
            string callback = _memoryCache.Get<string>("callback")!;
            return Content("callback data: " + callback);
        }

        public IActionResult GetObject()
        {
            UserObj? user = _memoryCache.Get<UserObj>("user:1") as UserObj;
            if (user == null) return Content("not found");
            return Content($"Id:{user.Id} Age:{user.Age} FullName:{user.FullName}");
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
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

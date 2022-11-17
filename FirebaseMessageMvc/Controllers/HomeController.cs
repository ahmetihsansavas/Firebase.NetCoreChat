using FirebaseMessageMvc.Models;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Diagnostics;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.SignalR;

namespace FirebaseMessageMvc.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        static private Dictionary<string, string> ChatList = new Dictionary<string, string>();
        IFirebaseConfig config = new FirebaseConfig { AuthSecret = "beywhu1T09RyfX4ZIfx2uXQArGFgq1EBBLjTM5Ok", 
            BasePath = "https://deneme-caa59-default-rtdb.europe-west1.firebasedatabase.app/" };
        IFirebaseClient client;
        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            client = new FireSharp.FirebaseClient(config);
            FirebaseResponse response = client.Get("Messages");
            dynamic data = JsonConvert.DeserializeObject<dynamic>(response.Body);
            var list = new List<Message>();
            foreach (var item in data)
            {
                list.Add(JsonConvert.DeserializeObject<Message>(((JProperty)item).Value.ToString()));
            }
            return View(list);
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

        public IActionResult Login() {
            return View(new LoginModel());
        }
        [HttpPost]
        public IActionResult Login(string UserName)
        {
            if (!ChatList.Any(cl => cl.Value.ToUpper() == UserName.ToUpper()) && UserName.Trim() != "")
            {
                ChatList.Add(DateTime.Now.ToString(), UserName);
                HttpContext.Session.SetString("UserName", UserName);
                return RedirectToAction("CreateMessage", "Home");
            }
            else
            {
                return Error();
            }
        }

        public IActionResult CreateMessage() {
            var userName = HttpContext.Session.GetString("UserName");
            return View(new Message() { UserName=userName});
        }
        [HttpPost]
        public JsonResult CreateMessage(string Data) {
            try
            {
            var userName = HttpContext.Session.GetString("UserName");
            var message = new Message() { Data = Data, UserName = userName };
            AddMessageToFirebase(message);
            }
            catch (Exception ex)
            {

               ModelState.AddModelError(string.Empty, ex.Message);
            }
        
           return Json(new Message());
        }

        private void AddMessageToFirebase(Message message)
        {
            client = new FireSharp.FirebaseClient(config);
            var data = message;
            PushResponse response = client.Push("Messages/", data);
            data.MessageId = response.Result.name;
            SetResponse setResponse = client.Set("Messages/" + data.MessageId, data);
        }
    }

    public class Chat : Hub
    {
        public Task SendMessage(string user, string message)
        {
            return Clients.All.SendAsync("ReceiveMessage", user, message);
        }
    }
}
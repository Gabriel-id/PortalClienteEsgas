using Microsoft.AspNetCore.Mvc;
using PortalCliente.Dtos;

namespace PortalCliente.Controllers
{
    public class BaseController : Controller
    {
        public void SendToastMessage(string message, string type)
        {
             this.TempData["toastMessage"] = $"{message}||{type}";
        }

        public LoggedUser GetUser()
        {
            return new LoggedUser
            {
                Name = User.Claims.First(p => p.Type == "UserName").Value,
                ClientCode = User.Claims.First(p => p.Type == "ClientCode").Value,
            };
        }
    }
}

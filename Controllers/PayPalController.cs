using Microsoft.AspNetCore.Mvc;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PayPalController : Controller
    {
        private readonly IConfiguration _configuration;
        public PayPalController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
    }
}

using Microsoft.AspNetCore.Mvc;

namespace EventManagementServer.Controllers
{
    [ApiController]
    [ApiVersion("1.0")]
    [ControllerName("PayPal")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PayPalController : Controller
    {
        private readonly IConfiguration _configuration;
        public PayPalController(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        
    }
}

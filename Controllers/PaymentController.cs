using EventManagementServer.Data;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using Stripe.Checkout;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.RateLimiting;

namespace EventManagementServer.Controllers
{
    [Route("api/payment")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly EventDbContext _dbContext;

        //Constructor
        public PaymentController(IConfiguration configuration, EventDbContext eventDbContext)
        {
            _configuration = configuration;
            _dbContext = eventDbContext;
        }

        [HttpPost("create-checkout-session")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<IActionResult> CreateCheckoutSessionAsync([FromBody] PaymentRequest request)
        {
            // ✅ Thiết lập API Key của Stripe
            var secretKey = _configuration.GetValue<string>("Stripe:SecretKey");
            if (string.IsNullOrEmpty(secretKey))
            {
                return StatusCode(500, "Stripe API key is missing");
            }

            StripeConfiguration.ApiKey = secretKey;

            var domain = "http://localhost:3000";
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Event Ticket"
                        },
                        UnitAmount = (long)(request.Amount * 100)
                    },
                    Quantity = 1
                }
            },
                Mode = "payment",
                SuccessUrl = $"{domain}/success?session_id={{CHECKOUT_SESSION_ID}}&registrationId={request.RegistrationId}",
                CancelUrl = $"{domain}/cancel",
                Metadata = new Dictionary<string, string>
                {
                    { "registrationId", request.RegistrationId.ToString() }
                }
            };

            var service = new SessionService();
            Session session = service.Create(options);

            await UpdatePaymentDate(request.RegistrationId);

            return Ok(new { sessionId = session.Id, url = session.Url });
        }
        



        [HttpPost("webhook")]
        [EnableRateLimiting("FixedWindowLimiter")]
        //Xử lý webhook từ Stripe
        public async Task<IActionResult> HandleWebhook()
        {
            //Đọc thông tin từ request
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            //Xác thực webhook
            var stripeEvent = EventUtility.ConstructEvent(
                json, Request.Headers["Stripe-Signature"], _configuration["Stripe:WebhookSecret"]
            );

            //Xử lý event
            if (stripeEvent.Type == "checkout.session.completed")
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                if (session != null && session.Metadata.TryGetValue("registrationId", out var registrationIdStr))
                {
                    if (int.TryParse(registrationIdStr, out var registrationId))
                    {
                        await UpdatePaymentDate(registrationId);
                        // Log để debug
                        Console.WriteLine($"Updated payment date for registration {registrationId}");
                    }
                    else
                    {
                        // Log để debug
                        Console.WriteLine($"Failed to parse registrationId: {registrationIdStr}");
                    }
                }
                else
                {
                    // Log để debug
                    Console.WriteLine("No registrationId found in metadata");
                }
            }

            return Ok();
        }

        //Hàm cập nhật payment date
        private async Task UpdatePaymentDate(int registrationId)
{
    var registration = await _dbContext.Registrations.FirstOrDefaultAsync(r => r.RegistrationID == registrationId);
    if (registration != null)
    {
        registration.PaymentDate = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        Console.WriteLine($"Updated payment date for registration {registrationId} to {DateTime.UtcNow}");
    }
    else
    {
        Console.WriteLine($"Registration with ID {registrationId} not found");
    }
}
    }
}

public class PaymentRequest
{
    public int RegistrationId { get; set; }
    public decimal Amount { get; set; }
}

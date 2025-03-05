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
        //Tạo checkout session với thông tin sản phẩm và giá trị thanh toán
        public IActionResult CreateCheckoutSession([FromBody] PaymentRequest request)
        {
            var domain = "https://your-website.com"; //domain của website khi user thanh toán thành công sẽ được chuyển đến
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" }, //Chỉ chấp nhận thanh toán bằng thẻ
                LineItems = new List<SessionLineItemOptions>
            {
                new SessionLineItemOptions
                {
                    //Thông tin sản phẩm và giá trị thanh toán
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd", //Loại tiền tệ
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = "Event Ticket" //Tên sản phẩm
                        },
                        UnitAmount = (long)(request.Amount * 100) //Giá trị thanh toán
                    },
                    Quantity = 1
                }
            },
                Mode = "payment", //Chế độ thanh toán
                SuccessUrl = $"{domain}/success?session_id={{CHECKOUT_SESSION_ID}}&registrationId={request.RegistrationId}", //Link khi thanh toán thành công
                CancelUrl = $"{domain}/cancel" //Link khi hủy thanh toán
            };

            //Tạo checkout session
            var service = new SessionService();

            //Lưu thông tin registrationId vào metadata của session
            Session session = service.Create(options);

            //Trả về thông tin session
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
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session; //Lấy thông tin session
                if (session != null)
                {
                    var registrationId = int.Parse(session.Metadata["registrationId"]);
                    await UpdatePaymentDate(registrationId); //Gọi hàm cập nhật payment date
                }
            }

            return Ok();
        }

        //Hàm cập nhật payment date
        private async Task UpdatePaymentDate(int registrationId)
        {
            //Tìm registration theo registrationId
            var registration = await _dbContext.Registrations.FirstOrDefaultAsync(r => r.RegistrationID == registrationId);

            //Nếu tìm thấy registration thì cập nhật payment date
            if (registration != null)
            {
                registration.PaymentDate = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync();
            }
        }
    }
}

public class PaymentRequest
{
    public int RegistrationId { get; set; }
    public decimal Amount { get; set; }
}

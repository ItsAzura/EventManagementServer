using EventManagementServer.Data;
using EventManagementServer.Interface;
using EventManagementServer.Repositories;
using EventManagementServer.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Đăng ký Dịch vụ để kết nối với database
builder.Services.AddDbContext<EventDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
});

//Đăng ký dịch vụ xác thực
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration.GetValue<string>("AppSettings:Issuer"),
            ValidateAudience = true,
            ValidAudience = builder.Configuration.GetValue<string>("AppSettings:Audience"),
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["AppSettings:Token"]!)),
            ValidateIssuerSigningKey = true
        };
    });

//Đăng ký dịch vụ AuthService
builder.Services.AddScoped<IAuthService, AuthService>();

//Đăng ký dịch vụ CategoryRepository
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

//Đăng ký dịch vụ CommentRepository
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

//Đăng ký dịch vụ EventRepository
builder.Services.AddScoped<IEventAreaRepository, EventAreaRepository>();

//Đăng ký dịch vụ EventCategoryRepository
builder.Services.AddScoped<IEventCategoryRepository, EventCategoryRepository>();

//Đăng ký dịch vụ NotificationHub
builder.Services.AddSignalR();

//Đăng ký dịch vụ Cors
builder.Services.AddCors(options =>
{
    options.AddPolicy("WebApp", policyBuiler =>
    {
        policyBuiler.WithOrigins("http://localhost:3000") //domain của web app chưa thay
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

//Đăng ký dịch vụ RateLimiter
builder.Services.AddRateLimiter(
    //Sử dụng ConcurrencyLimiter 
    RateLimiterOptions => RateLimiterOptions.AddFixedWindowLimiter("FixedWindowLimiter",
    options => {
        options.PermitLimit = 5; //Số lượng request tối đa mà một user có thể thực hiện trong 1 thời điểm
        options.Window = TimeSpan.FromSeconds(10); //Thời gian giới hạn
        options.QueueLimit = 10; //Số lượng request tối đa mà một user có thể thực hiện trong 1 khoảng thời gian
        options.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst; //Xử lý request theo thứ tự cũ nhất trước
        options.AutoReplenishment = true; //Tự động cung cấp thêm request khi hết hạn
    }
 ));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Sử dụng Cors
app.UseCors("WebApp");

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.UseRateLimiter();

app.Run();

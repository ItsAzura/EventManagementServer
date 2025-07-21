using EventManagementServer.Data;
using EventManagementServer.Interface;
using EventManagementServer.Repositories;
using EventManagementServer.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Text.Json.Serialization;
using EventManagementServer.Validator;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Formatting.Compact;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using EventManagementServer.Helpers;
using DotNetEnv;
;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiVersioning(options =>
{
    options.ReportApiVersions = true;
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new QueryStringApiVersionReader("api-version"),
        new HeaderApiVersionReader("X-Version"),
        new MediaTypeApiVersionReader("X-Version"));
});

// ✅ Cấu hình API Explorer để hiển thị nhiều version trên Swagger
builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Định dạng version (v1, v2, ...)
    options.SubstituteApiVersionInUrl = true; // Thay thế {version} trong URL
});

// ✅ Cấu hình Swagger
builder.Services.AddSwaggerGen(options =>
{
    var provider = builder.Services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerDoc(description.GroupName, new OpenApiInfo
        {
            Title = $"My API {description.ApiVersion}",
            Version = description.ApiVersion.ToString()
        });
    }

    // Hỗ trợ truyền version qua Header hoặc Query String
    options.OperationFilter<SwaggerDefaultValues>();
});

builder.Services.AddControllers();

//Dùng Serilog để ghi log
builder.Host.UseSerilog((context, config) =>
{
    config.MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information)
        .Enrich.FromLogContext()
        .Enrich.WithEnvironmentName()
        .Enrich.WithProcessId()
        .Enrich.WithProcessName()
        .Enrich.WithThreadId()
        .WriteTo.Debug(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.Console(outputTemplate: "{Timestamp:HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
        .WriteTo.File("Logs/log.txt", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, buffered: true)
        .WriteTo.File(new CompactJsonFormatter(), "Logs/log.json", rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true, buffered: true);
});

builder.Services.AddControllers();
builder.Services.AddValidatorsFromAssemblyContaining<UserDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<UpdateUserDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<TicketDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RoleDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrationDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<RegistrationDetailDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<NotificationDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EventDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EventCategoryDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<EventAreaDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ContactResponseDtoValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<ContactRequestDtoValidator>();

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

//Đăng ký dịch vụ RoleRepository
builder.Services.AddScoped<IRoleRepository, RoleRepository>();

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

builder.Services.AddScoped<ITicketRepository, TicketRepository>();

builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

//Đăng ký dịch vụ EmailService
builder.Services.AddSingleton<EmailService>();

Env.Load();

var googleClientId = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_ID");
var googleClientSecret = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_SECRET");

var app = builder.Build();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

        foreach (var description in provider.ApiVersionDescriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", $"My API {description.ApiVersion}");
        }
    });


app.UseHttpsRedirection();

//Sử dụng Cors
app.UseCors("WebApp");

app.UseAuthorization();

app.MapControllers();

app.MapHub<NotificationHub>("/notificationHub");

app.UseRateLimiter();

app.UseSerilogRequestLogging();

app.Run();

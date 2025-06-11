using Azure.Storage.Blobs;
using log4net.Config;
using log4net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SCCMS.API.Services;
using SCCMS.Domain.Services.Implements;
using SCCMS.Domain.Services.Interfaces;
using SCCMS.Infrastucture.Context;
using SCCMS.Infrastucture.Entities;
using SCCMS.Infrastucture.UnitOfWork;
using System.Reflection;
using System.Text;
using Utility;
using SCCMS.Domain.Hubs;
using Microsoft.AspNetCore.SignalR;
using SCCMS.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// **1. Thêm các dịch vụ cần thiết vào DI Container**

// Thêm Controllers
builder.Services.AddControllers();

// Cấu hình Swagger với hỗ trợ JWT Authentication
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Định nghĩa Security Scheme cho JWT
    options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
    {
        Description =
            "JWT Authorization header using the Bearer scheme. \r\n\r\n " +
            "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
            "Example: \"Bearer 12345abcdef\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = JwtBearerDefaults.AuthenticationScheme,
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT"
    });

    // Áp dụng Security Requirement cho Swagger
    options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
           new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = JwtBearerDefaults.AuthenticationScheme
                            },
                Scheme = "bearer",
                Name = JwtBearerDefaults.AuthenticationScheme,
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Thêm HttpContextAccessor
builder.Services.AddHttpContextAccessor();

// Cấu hình DbContext với SQL Server
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultSQLConnection"));
});

// Cấu hình Log4Net
var logDirectory = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logDirectory))
{
    Directory.CreateDirectory(logDirectory);
}

var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

// Đăng ký các Repository và Service
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IStudentService, StudentService>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IVolunteerCourseService, VolunteerCourseService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISupervisorService, SupervisorService>();
builder.Services.AddScoped<IStudentGroupService, StudentGroupService>();
builder.Services.AddScoped<ApiResponse>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddTransient<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddTransient<IEmailService, EmailService>();
builder.Services.AddScoped<IStudentApplicationService, StudentApplicationService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IStudentGroupAssignmentService, StudentGroupAssignmentService>();
builder.Services.AddScoped<ApiResponse>();
builder.Services.AddScoped<INightShiftAssignmentService, NightShiftAssignmentService>();
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<ApiResponse>();
builder.Services.AddScoped<IVolunteerTeamService, VolunteerTeamService>();
builder.Services.AddScoped<IVolunteerCourseService, VolunteerCourseService>();
// Nếu AttendanceService nằm trong SCCMS.Domain.Services.Implements
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<INightShiftService, NightShiftService>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IStaffFreeTimeService, StaffFreeTimeService>();
builder.Services.AddHostedService<ReportStatusUpdaterService>();

//builder.Services.AddScoped<IAttendanceService, AttendanceService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped(typeof(ILoggerService<>), typeof(Log4NetLoggerService<>));
builder.Services.AddHostedService<NotificationCleanupService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped(typeof(ILoggerService<>), typeof(Log4NetLoggerService<>));
builder.Services.AddHostedService<NotificationCleanupService>();
builder.Services.AddHostedService<CourseStatusUpdaterService>();

builder.Services.AddHostedService<StudentReportGeneratorService>();
// Đăng ký SignalR
builder.Services.AddSignalR();

// Đăng ký Custom IUserIdProvider
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();

// Đăng ký Notification Service
builder.Services.AddScoped<INotificationService, NotificationService>();

// Đăng ký AutoMapper
builder.Services.AddAutoMapper(typeof(SCCMS.Domain.Mapping.AutoMapper));

// **4. Cấu hình JWT Authentication**
var key = builder.Configuration.GetValue<string>("ApiSettings:Secret");
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Đặt thành true trong môi trường sản xuất
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };

    // Thêm sự kiện để hỗ trợ SignalR
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Nếu yêu cầu đến từ SignalR hub
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/api/notificationHub"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        },
        OnTokenValidated = async context =>
        {
            var userIdClaim = context.Principal?.FindFirst("userId");
            if (userIdClaim == null)
            {
                context.Fail("Unauthorized");
                return;
            }

            if (!int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Fail("Unauthorized");
                return;
            }

            // Lấy AppDbContext từ DI
            var dbContext = context.HttpContext.RequestServices.GetRequiredService<AppDbContext>();
            var user = await dbContext.Users.FindAsync(userId);
            if (user == null || user.Status == UserStatus.DeActive)
            {
                context.Fail("Unauthorized");
            }
        }
    };
});

// Đăng ký HttpClient
builder.Services.AddHttpClient();

// Cấu hình CORS với chính sách có tên
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials()
            .SetIsOriginAllowed(origin => true) // Cho phép tất cả origin
            .WithExposedHeaders("Content-Disposition"); // Expose thêm header này
    });
});

// Cấu hình Blob Storage
builder.Services.AddSingleton(u => new BlobServiceClient(
    builder.Configuration.GetConnectionString("StorageAccount")));
builder.Services.AddSingleton<IBlobService, BlobService>();

var app = builder.Build();

// **8. Cấu hình Middleware Pipeline**

// Cấu hình Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API v1");
        c.RoutePrefix = string.Empty; // Swagger ở root
    });
}

// Cấu hình HTTPS Redirection
app.UseHttpsRedirection();

// Cấu hình CORS trước khi sử dụng Authentication và Authorization
app.UseCors("CorsPolicy");

// Cấu hình Authentication và Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map SignalR Hubs sau khi đã áp dụng CORS và Authentication
app.MapHub<NotificationHub>("/api/notificationHub"); // **Map tại đường dẫn này**


// Đặt MaxRequestBodySize không giới hạn (nếu cần)
app.Use(async (context, next) =>
{
    context.Features.Get<IHttpMaxRequestBodySizeFeature>().MaxRequestBodySize = null;
    await next.Invoke();
});

// Map Controllers
app.MapControllers();

// Run the app
app.Run();

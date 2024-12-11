using ApiBuyerMorgan.Repositories;
using ChatApp.Configuaration;
using ChatApp.Extensions;
using ChatApp.Socket;
using ChatApp.Timers;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.FileProviders;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;
using Tư.Configurations;
using TWChatAppApiMaster.Configurations;
using TWChatAppApiMaster.Middleware;
using TWChatAppApiMaster.Middlewares;
using TWChatAppApiMaster.Repositories;
using TWChatAppApiMaster.Timers;
using TWChatAppApiMaster.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerConfiguration("ChatApp", "v1");
builder.Services.AddJwtAuthentication();
builder.Services.AddApiVersioning(options =>
{
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.ReportApiVersions = true; // Hiển thị phiên bản API trong tiêu đề phản hồi
});

builder.Services.AddVersionedApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV"; // Định dạng cho tên phiên bản
    options.SubstituteApiVersionInUrl = true; // Cho phép thay thế phiên bản trong URL
});

var appSettings = builder.Configuration.GetSection("AppSettings");
builder.Services.Configure<AppSettings>(appSettings);
GlobalSettings.IncludeConfig(appSettings.Get<AppSettings>());
builder.Services.ConfigureDbContext(GlobalSettings.AppSettings.Database.ChatAppDatabase);

MailService.Init(GlobalSettings.AppSettings.ANSender);

builder.Services.AddHostedService<TimerProcessMessageDb>();

/*if (!Directory.Exists(GlobalSettings.AppSettings.UploadPath))
{
    Directory.CreateDirectory(GlobalSettings.AppSettings.UploadPath);
}*/
builder.Services.AddCors(x => x.AddPolicy("CorsPolicy", p =>
{
    p.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
}));

// For socket services
builder.Services.AddTransient<ConnectionManager>();
builder.Services.AddSingleton<ChatHandler>();

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("AutoDeleteMessageJob");
    q.AddJob<AutoDeleteMessageJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("AutoDeleteMessageJob-trigger")
        .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(3, 0))
        //.WithSimpleSchedule(schedule => schedule
        //    .WithIntervalInMinutes(1) // Lặp lại mỗi 1 phút
        //    .RepeatForever() // Lặp lại không ngừng
        //)
    );
});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

// Configure FirebaseApp
FirebaseApp.Create(new AppOptions
{
    Credential = GoogleCredential.FromFile("firebase-cloud-message.json")
});

builder.Services.AddScoped<ILogTimingRepository, LogTimingRepository>();
builder.Services.AddScoped<ISessionRepository, SessionRepository>();

var app = builder.Build();

// Set up Swagger UI (if needed, typically done early)
app.ConfigurationSwaggerUI();
app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    var provider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

    foreach (var description in provider.ApiVersionDescriptions)
    {
        options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
    }

    // Optional: Set the Swagger UI at the root path
    options.RoutePrefix = string.Empty;
});

// Enable CORS
app.UseCors("CorsPolicy");
app.UseCors(x =>
{
    x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
});

// Serve static files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
           Path.Combine(builder.Environment.ContentRootPath, "Images")),
    RequestPath = "/Images"
});

// Enable WebSockets before authentication and authorization
var webSocketOptions = new WebSocketOptions()
{
    KeepAliveInterval = TimeSpan.FromSeconds(120),
    ReceiveBufferSize = 100 * 1024
};
app.UseWebSockets(webSocketOptions);

// Authentication and Authorization should be placed before handling requests
app.UseAuthentication();
app.UseAuthorization();

// Map controller routes
app.MapControllers();

// Custom middlewares (order matters)
var handler = app.Services.GetRequiredService<ChatHandler>();
app.UseMiddleware<WebSocketMiddleware>(handler);
ChatHandler.handleInstance = handler;


// app.UseMiddleware<SecretKeyMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();
app.UseMiddleware<SessionValidationMiddleware>();

// Run the application
app.Run();
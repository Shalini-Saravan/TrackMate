using BlazorServerAppWithIdentity.Api.Hubs;
using BlazorServerAppWithIdentity.Api.Services;
using BlazorServerAppWithIdentity.Api.Services.BackgroundServices;
using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using Newtonsoft.Json.Serialization;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddServerSideBlazor();
//builder.WebHost.UseUrls("http://10.164.42.30:7035");
var Configuration = builder.Configuration;

//connection to mongo db ATLAS
string connectionUri = Configuration.GetValue<string>("ConnectionStrings:MongoDBConnection");
var settings = MongoClientSettings.FromConnectionString(connectionUri);
settings.ServerApi = new ServerApi(ServerApiVersion.V1);
var client = new MongoClient(settings);
builder.Services.AddSingleton<IMongoClient>(client);

//registering Identity Service for Authentication and Authorization purposes
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
     options.SignIn.RequireConfirmedEmail = false)
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
    (
        connectionUri, "IdentityAuthDb"
    );

// Add services to the container.
builder.Services.AddSingleton<MachineService>();
builder.Services.AddSingleton<MachineUsageService>();
builder.Services.AddSingleton<NotificationService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<RunsLogService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddHostedService<MachineTimeout>();
builder.Services.AddHostedService<PipelineNotification>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddControllersWithViews().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore).
    AddNewtonsoftJson(options => options.SerializerSettings.ContractResolver = new DefaultContractResolver());

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = false,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Configuration["JwtIssuer"],
        ValidAudience = Configuration["JwtAudience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JwtSecurityKey"]))
    };
});
var app = builder.Build();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapBlazorHub();
    endpoints.MapHub<BlazorHub>("/blazorHub");
});
app.Run();

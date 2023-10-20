using BlazorServerAppWithIdentity.Models;
using BlazorServerAppWithIdentity.Services;
using BlazorServerAppWithIdentity.Hubs;
using Microsoft.Extensions.Hosting;
using Blazored.LocalStorage;
using System.Net;

var builder = WebApplication.CreateBuilder(args);
//string hostUrl = "https://" + System.Environment.MachineName + ":7035";
string hostUrl = "http://api:443";
//builder.WebHost.UseUrls("https://inrd-shsarava.apac.corp.natinst.com:1035");

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
var Configuration = builder.Configuration;

//connection to mongo db ATLAS
string connectionUri = Configuration.GetValue<string>("ConnectionStrings:MongoDBConnection");

builder.Services.AddAuthentication();
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
     options.SignIn.RequireConfirmedEmail = false)
    .AddMongoDbStores<ApplicationUser, ApplicationRole, Guid>
    (
        connectionUri, "IdentityAuthDb"
    );

builder.Services.AddHttpContextAccessor();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<MachineService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MachineUsageService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddScoped<RunsLogService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<AzureService>();
builder.Services.AddHostedService<MachineBackgroundService>();

builder.Services.AddCors(c =>
{
    c.AddPolicy("AllowOrigin", options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
builder.Services.AddHttpClient<MachineService>(client =>
{
    client.BaseAddress = new Uri(hostUrl);
});
builder.Services.AddHttpClient<MachineUsageService>(client =>
{
    client.BaseAddress = new Uri(hostUrl);
});
builder.Services.AddHttpClient<UserService>(client =>
{
    client.BaseAddress = new Uri(hostUrl);
});
builder.Services.AddHttpClient<AccountService>(client =>
{
    client.BaseAddress = new Uri(hostUrl);
});
builder.Services.AddHttpClient<RunsLogService>(client =>
{
    client.BaseAddress = new Uri(hostUrl);
});
builder.Services.AddHttpClient<NotificationService>(client =>
{
    client.BaseAddress = new Uri(hostUrl);
});
builder.Services.AddHttpClient<SubscriptionService>(client =>
{
    client.BaseAddress = new Uri(hostUrl);
});
builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddSignalR();
var app = builder.Build();
app.UseCors(options => options.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.UseEndpoints(endpoints =>
{
    endpoints.MapBlazorHub();
    endpoints.MapHub<BlazorHub>("/blazorHub");
    endpoints.MapFallbackToPage("/_Host");

});
app.Run();
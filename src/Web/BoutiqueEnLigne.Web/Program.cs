using BoutiqueEnLigne.Web.Configuration;
using BoutiqueEnLigne.Web.Services;
using BoutiqueEnLigne.Web.Authentication;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddSession(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<AccessTokenHandler>();
builder.Services.Configure<BackendApiOptions>(
    builder.Configuration.GetSection(BackendApiOptions.SectionName));
builder.Services.AddHttpClient<IProductApiClient, ProductApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<BackendApiOptions>>()
        .Value;
    client.BaseAddress = new Uri(options.BaseUrl);
}).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<IUserApiClient, UserApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<BackendApiOptions>>()
        .Value;
    client.BaseAddress = new Uri(options.BaseUrl);
}).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<IOrderApiClient, OrderApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<BackendApiOptions>>()
        .Value;
    client.BaseAddress = new Uri(options.BaseUrl);
}).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<IPaymentApiClient, PaymentApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<BackendApiOptions>>()
        .Value;
    client.BaseAddress = new Uri(options.BaseUrl);
}).AddHttpMessageHandler<AccessTokenHandler>();
builder.Services.AddHttpClient<ICartApiClient, CartApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<Microsoft.Extensions.Options.IOptions<BackendApiOptions>>()
        .Value;
    client.BaseAddress = new Uri(options.BaseUrl);
}).AddHttpMessageHandler<AccessTokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

var disableHttpsRedirection = builder.Configuration.GetValue<bool>("DisableHttpsRedirection");
if (!disableHttpsRedirection)
{
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

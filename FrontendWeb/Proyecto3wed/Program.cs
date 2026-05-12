using System.Net.Http.Headers;
using Microsoft.Extensions.Options;
using Proyecto3wed.Options;
using Proyecto3wed.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<BibliotecaApiOptions>(builder.Configuration.GetSection(BibliotecaApiOptions.SectionName));
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<BibliotecaApiClient>()
    .ConfigureHttpClient((sp, client) =>
    {
        var baseUrl = sp.GetRequiredService<IOptions<BibliotecaApiOptions>>().Value.BaseUrl.TrimEnd('/');
        client.BaseAddress = new Uri(baseUrl + "/");
        client.DefaultRequestHeaders.Accept.Clear();
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .ConfigurePrimaryHttpMessageHandler(sp =>
    {
        var env = sp.GetRequiredService<IHostEnvironment>();
        var handler = new HttpClientHandler();
        if (env.IsDevelopment())
        {
            handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        }

        return handler;
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Catalogo}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();

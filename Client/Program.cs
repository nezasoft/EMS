using Blazored.LocalStorage;
using Client;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Contracts;
using ClientLibrary.Services.Implementation;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Popups;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddTransient<CustomHttpHandler>();
/*builder.Services.AddHttpClient("SystemApiClient", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Apis:SystemApi:BaseUrl"]!);
});*/
//Extend and add Http Handler
builder.Services.AddHttpClient("SystemApiClient",client =>
{
    client.BaseAddress = new Uri("https://localhost:7161/");
}).AddHttpMessageHandler<CustomHttpHandler>();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7161/") });
builder.Services.AddAuthorizationCore();
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<GetHttpClient>();
builder.Services.AddScoped<LocalStorageService>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IUserAccountService, UserAccountService>();
//REQUIRED for Syncfusion
builder.Services.AddLocalization();
builder.Services.AddSyncfusionBlazor();
builder.Services.AddScoped<SfDialogService>();
await builder.Build().RunAsync();

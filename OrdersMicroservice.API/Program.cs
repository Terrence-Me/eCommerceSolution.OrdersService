using BusinessLogiclayer.HttpClient;
using BusinessLogiclayer.Policies;
using eCommerce.OrderMicorservice.API.Middleware;
using eCommerce.OrderMicorservice.BusinessLogicLayer;
using eCommerce.OrderMicorservice.DataAccessLayer;
using FluentValidation.AspNetCore;
using Polly;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("http://localhost:4200")
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();
builder.Services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();

//var usersMicroserviceName = builder.Configuration["UsersMicroserviceName"];
//var usersMicroservicePort = builder.Configuration["UsersMicroservicePort"];
//Console.WriteLine($"http://{usersMicroserviceName}:{usersMicroservicePort}");

builder.Services.AddHttpClient<UsersMicroserviceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var usersMicroserviceName = configuration["UsersMicroserviceName"];
    var usersMicroservicePort = configuration["UsersMicroservicePort"];
    client.BaseAddress = new Uri($"http://{usersMicroserviceName}:{usersMicroservicePort}");
})
.AddPolicyHandler((serviceProvider, request) =>
{
    var policies = serviceProvider.GetRequiredService<IUsersMicroservicePolicies>();
    var retryPolicy = policies.GetRetryPolicy();
    var circuitBreakerPolicy = policies.GetCircuitBreakerPolicy();
    var timeoutPolicy = policies.GetTimeoutPolicy();
    return Policy.WrapAsync(retryPolicy, circuitBreakerPolicy, timeoutPolicy);

});

//builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
//{
//    client.BaseAddress = new Uri($"http://{builder.Configuration["UsersMicroserviceName"]}:{builder.Configuration["UsersMicroservicePort"]}");
//}).AddPolicyHandler(builder.Services.BuildServiceProvider().GetRequiredService<IUsersMicroservicePolicies>().GetRetryPolicy());

//var productsMicroserviceName = builder.Configuration["ProductsMicroserviceName"];
//var uproductsMicroservicePort = builder.Configuration["ProductsMicroservicePort"];
//Console.WriteLine($"http://{productsMicroserviceName}:{uproductsMicroservicePort}");
builder.Services.AddHttpClient<ProductsMicroserviceClient>((serviceProvider, client) =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var usersMicroserviceName = configuration["ProductsMicroserviceName"];
    var usersMicroservicePort = configuration["ProductsMicroservicePort"];
    client.BaseAddress = new Uri($"http://{builder.Configuration["ProductsMicroserviceName"]}:{builder.Configuration["ProductsMicroservicePort"]}");
})
.AddPolicyHandler((serviceProvider, request) =>
{
    var policies = serviceProvider.GetRequiredService<IProductsMicroservicePolicies>();
    var fallbackPolicy = policies.GetFallbackPolicy();
    return Policy.WrapAsync(fallbackPolicy);

});


var app = builder.Build();

// Add ExceptionHandlingMiddleware to the pipeline
app.UseExceptionHandlingMiddleware();

app.UseRouting();

app.UseCors();

app.UseSwagger();
app.UseSwaggerUI();

//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

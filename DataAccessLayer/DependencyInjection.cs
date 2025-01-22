using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace eCommerce.OrderMicorservice.DataAccessLayer;
public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        string connectionStringTemplate = configuration.GetConnectionString("MongoDB")!;
        string connectionString = connectionStringTemplate
          .Replace("$MONGO_HOST", Environment.GetEnvironmentVariable("MONGODB_HOST"))
          .Replace("$MONGO_PORT", Environment.GetEnvironmentVariable("MONGODB_PORT"));
        Console.WriteLine($"[DEBUG] Actual MongoDB connection string: {connectionString}");
        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

        services.AddScoped<IMongoDatabase>(provider =>
        {
            IMongoClient client = provider.GetRequiredService<IMongoClient>();
            return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE"));
        });

        services.AddScoped<IOrdersRepository, OrdersRepository>();

        return services;
    }
}

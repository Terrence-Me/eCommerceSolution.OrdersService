using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BusinessLogiclayer.RabbitMQ;
public class RabbitMQProductNameUpdateConsumer : IRabbitMQProductNameUpdateConsumer
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQProductNameUpdateConsumer> _logger;

    public RabbitMQProductNameUpdateConsumer(IConfiguration configuration, ILogger<RabbitMQProductNameUpdateConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;

        Console.WriteLine($"RabbitMQ_HostName: {_configuration["RabbitMQ_HostName"]}");
        Console.WriteLine($"RabbitMQ_UserName: {_configuration["RabbitMQ_UserName"]}");
        Console.WriteLine($"RabbitMQ_Password: {_configuration["RabbitMQ_Password"]}");
        Console.WriteLine($"RabbitMQ_Port: {_configuration["RabbitMQ_Port"]}");

        string hostName = _configuration["RabbitMQ_HostName"]!;
        string userName = _configuration["RabbitMQ_UserName"]!;
        string password = _configuration["RabbitMQ_Password"]!;
        string port = _configuration["RabbitMQ_Port"]!;

        ConnectionFactory connectionFactory = new ConnectionFactory()
        {
            HostName = hostName,
            UserName = userName,
            Password = password,
            Port = Convert.ToInt32(port)
        };
        _connection = connectionFactory.CreateConnection();

        _channel = _connection.CreateModel();
    }


    public void Consume()
    {
        string routingKey = "product.update.name";
        string queueName = "orders.product.update.name.queue";

        //Create exchange
        string exchangeName = _configuration["RABBITMQ_PRODUCTS_Exchange"]!;
        _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true);

        // Create message queue
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        // Bind the queue to the exchange
        _channel.QueueBind(queue: queueName, exchange: exchangeName, routingKey: routingKey);

        EventingBasicConsumer consumer = new EventingBasicConsumer(_channel);

        consumer.Received += (sender, args) =>
        {
            byte[] body = args.Body.ToArray();
            string message = Encoding.UTF8.GetString(body);

            if (message != null)
            {
                ProductNameUpdateMessage? updateProductNameMessage = JsonSerializer.Deserialize<ProductNameUpdateMessage>(message);
                Console.WriteLine($" [x] Received {message}");

                _logger.LogInformation($"Product name updated: {updateProductNameMessage?.ProductId}, New name: {updateProductNameMessage?.NewName} ");
            }

        };

        _channel.BasicConsume(queue: queueName, consumer: consumer, autoAck: true);

    }

    public void Dispose()
    {
        _channel.Dispose();
        _connection.Dispose();
    }
}

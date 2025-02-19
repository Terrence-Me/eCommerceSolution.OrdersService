using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace BusinessLogiclayer.RabbitMQ;
public class RabbitMQProductDeletionConsumer : IRabbitMQProductDeletionConsumer
{
    private readonly IConfiguration _configuration;
    private readonly IModel _channel;
    private readonly IConnection _connection;
    private readonly ILogger<RabbitMQProductDeletionConsumer> _logger;

    public RabbitMQProductDeletionConsumer(IConfiguration configuration, ILogger<RabbitMQProductDeletionConsumer> logger)
    {
        _configuration = configuration;
        _logger = logger;

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
        string routingKey = "product.delete";
        string queueName = "orders.product.delete.queue";

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
                ProductDeletionMessage? productDeletionMessage = JsonSerializer.Deserialize<ProductDeletionMessage>(message);
                Console.WriteLine($" [x] Received {message}");

                _logger.LogInformation($"Product deleted: {productDeletionMessage?.ProductId}, Product name: {productDeletionMessage?.ProductName} ");
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

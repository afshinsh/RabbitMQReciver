using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQReciver
{
    public class TicketConsumer : BackgroundService
    {
        protected readonly IModel _channel;
        private readonly IConnection _connection;
        private string _consumerTag;
        private readonly string QUEUE_NAME = "ticketQueue2";
        public TicketConsumer()
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = "localhost",
                UserName = "guest",
                Password = "guest"
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            DeclareExchange();
            DeclareQueue();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceived;
            _consumerTag = _channel.BasicConsume(queue: QUEUE_NAME, autoAck: true, consumer);

            return Task.CompletedTask;
        }

        public void OnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            var body = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<Ticket>(body);
        }

        protected void DeclareQueue()
        {
            _channel.QueueDeclare(queue: QUEUE_NAME
                    , durable: true
                    , exclusive: false
                    , autoDelete: false
                    , arguments: null);

            _channel.QueueBind(QUEUE_NAME, QUEUE_NAME, "");
        }

        protected void DeclareExchange()
        {
            _channel.ExchangeDeclare(exchange: QUEUE_NAME
                , type: "direct"
                , durable: true
                , autoDelete: false
                , arguments: null);
        }
    }
}

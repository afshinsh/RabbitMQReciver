using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQReciver
{
    public abstract class RabbitConsumer<T> : RabbitBase
    {
        private string _consumerTag;
        public RabbitConsumer(IConfiguration configration, string sectionName) : base(configration, sectionName)
        {
        }

        public void OnMessageReceived(object sender, BasicDeliverEventArgs args)
        {
            var body = Encoding.UTF8.GetString(args.Body.ToArray());
            var message = JsonSerializer.Deserialize<T>(body);
            HandleMessage(message);
        }

        public abstract void HandleMessage(T message);

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DeclareExchange();
            DeclareQueue();

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += OnMessageReceived;
            _consumerTag = _channel.BasicConsume(queue: "ticketQueue", autoAck: true, consumer);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.BasicCancel(_consumerTag);

            return Task.CompletedTask;
        }
    }
}

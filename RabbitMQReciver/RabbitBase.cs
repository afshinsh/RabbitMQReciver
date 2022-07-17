using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System;

namespace RabbitMQReciver
{
    public abstract class RabbitBase :  IDisposable
    {
        protected readonly IModel _channel;
        private IConnection _connection;
        private const string HOST_NAME = "rabbitmq://localhost/";

        public RabbitBase(IConfiguration configuration, string sectionName)
        {
            var connectionFactory = new ConnectionFactory()
            {
                HostName = HOST_NAME,
                UserName = "guest",
                Password = "guest"
            };

            _connection = connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();
        }

        protected void DeclareQueue()
        {
            _channel.QueueDeclare(queue: "ticketQueue"
                    , durable: true
                    , exclusive: false
                    , autoDelete: false
                    , arguments: null);

            _channel.QueueBind("ticketQueue", "ticketQueue", "");
        }

        protected void DeclareExchange()
        {
            _channel.ExchangeDeclare(exchange: "ticketQueue"
                , type: "fanout"
                , durable: true
                , autoDelete: false
                , arguments: null);
        }

        public void Dispose()
        {
            _channel.Dispose();
            _connection.Dispose();
        }
    }
}

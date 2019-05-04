using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HistoryService.DB;
using HistoryService.Models;
using HistoryService.OptionModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Prometheus;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HistoryService.Services
{
    public class RabbitMqService : BackgroundService
    {
        private readonly ILogger<RabbitMqService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _channel;
        private readonly IConnection _connection;
        private readonly RabbitMqOptions _rabbitMqOptions;



        public RabbitMqService(ILogger<RabbitMqService> logger, IServiceProvider serviceProvider, IOptionsMonitor<RabbitMqOptions> rabbitMqOptions)
        {
            _rabbitMqOptions = rabbitMqOptions.CurrentValue;
            _logger = logger;
            _serviceProvider = serviceProvider;
            (_channel,_connection) = CreateRabbitMqChannel();
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMq Service is starting.");

            cancellationToken.ThrowIfCancellationRequested();
            DeclareQueueAndExchange();

            var consumer = RegisterRabbitMqConsumer(_channel);
            _channel.BasicConsume(_rabbitMqOptions.QueueName, false, consumer);

            return Task.CompletedTask;
        }

        private void DeclareQueueAndExchange()
        {
            _channel.ExchangeDeclare(_rabbitMqOptions.ExchangeName, ExchangeType.Direct);
            _channel.QueueDeclare(_rabbitMqOptions.QueueName, false, false, false, null);
            _channel.QueueBind(_rabbitMqOptions.QueueName, _rabbitMqOptions.ExchangeName, _rabbitMqOptions.RoutingKey, null);
        }

        private EventingBasicConsumer RegisterRabbitMqConsumer(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var historyMessage = JsonConvert.DeserializeObject<HistoryMessage>(message);
                try
                {
                    await Handle(historyMessage);
                    channel.BasicAck(ea.DeliveryTag, false);
                }
                catch (Exception e)
                {
                    channel.BasicNack(ea.DeliveryTag, false, true);
                }
                
            };
            return consumer;
        }

        private async Task Handle(HistoryMessage historyMessage)
        {
            await InsertIntoDatabase(historyMessage);
            // Counter event received
            PrometheusMetrics.EventsReceived.WithLabels(historyMessage.Event).Inc();
        }

        private async Task InsertIntoDatabase(HistoryMessage historyMessage)
        {
            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<HistoryContext>())
            {
                var eventObj = await context.Events.FirstOrDefaultAsync(x => x.Title == historyMessage.Event) ?? new Event { Title = historyMessage.Event };
                var history = new History
                    { Event = eventObj, EventMessage = historyMessage.EventMessage, User = historyMessage.User, Timestamp = historyMessage.Timestamp};
                context.TaxHistories.Add(history);
                await context.SaveChangesAsync();
            }
        }

        private (IModel channel, IConnection connection) CreateRabbitMqChannel()
        {
            var factory = new ConnectionFactory
            {
                HostName = _rabbitMqOptions.HostName,
                UserName = _rabbitMqOptions.User,
                Password = _rabbitMqOptions.Password,
                VirtualHost = _rabbitMqOptions.VirtualHost
            };
            var connection = factory.CreateConnection();
            var channel = _connection.CreateModel();
            return (channel, connection);
        }

        public override void Dispose()
        {
            _channel.Close();
            _connection.Close();
            base.Dispose();
        }
    }
}
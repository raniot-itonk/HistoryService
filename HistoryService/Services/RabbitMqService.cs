using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HistoryService.DB;
using HistoryService.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace HistoryService.Services
{
    public class RabbitMqService : BackgroundService
    {
        private readonly ILogger<RabbitMqService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IModel _channel;

        public RabbitMqService(ILogger<RabbitMqService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _channel = CreateRabbitMqChannel();
        }

        protected override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("RabbitMq Service is starting.");

            cancellationToken.ThrowIfCancellationRequested();

            _channel.QueueDeclare("hello",false, false, false, null);

            var consumer = RegisterRabbitMqConsumer(_channel);
            _channel.BasicConsume("hello", true, consumer);

            return Task.CompletedTask;
        }

        private EventingBasicConsumer RegisterRabbitMqConsumer(IModel channel)
        {
            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body);
                var historyMessage = JsonConvert.DeserializeObject<HistoryMessage>(message);
                await Handle(historyMessage);
            };
            return consumer;
        }

        private async Task Handle(HistoryMessage historyMessage)
        {
            Console.WriteLine(historyMessage.Event);

            using (var scope = _serviceProvider.CreateScope())
            using (var context = scope.ServiceProvider.GetRequiredService<HistoryContext>())
            {
                var eventObj = await context.Events.FirstOrDefaultAsync(x => x.Title == historyMessage.Event) ?? new Event { Title = historyMessage.Event };
                var history = new History { Event = eventObj, EventMessage = historyMessage.EventMessage, User = historyMessage.User };
                context.TaxHistories.Add(history);
                await context.SaveChangesAsync();
            }
        }

        private IModel CreateRabbitMqChannel()
        {
            var factory = new ConnectionFactory
            {
                HostName = "localhost"
            };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();
            return channel;
        }
    }
}
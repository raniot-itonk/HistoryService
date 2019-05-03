using System.Threading.Tasks;
using HistoryService.DB;
using Microsoft.EntityFrameworkCore;
using Prometheus;

namespace HistoryService
{
    public class PrometheusMetrics
    {
        public static readonly Counter EventsReceived = Metrics.CreateCounter("EventsReceived", "Events received in the history service",
            new CounterConfiguration
            {
                LabelNames = new[] { "Event" }
            });



        public static async Task Setup(HistoryContext context)
        {
            var events = await context.Events.ToListAsync();
            events.ForEach(x => EventsReceived.WithLabels(x.Title).Inc(0));
        }
    }
}

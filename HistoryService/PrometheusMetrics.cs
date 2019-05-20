using System.Linq;
using HistoryService.DB;
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


        public static void Setup(HistoryContext context)
        {
            var events = context.Events.ToList();
            events.ForEach(x => EventsReceived.WithLabels(x.Title).Inc(0));
        }
    }
}

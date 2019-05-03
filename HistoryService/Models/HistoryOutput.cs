using System;
using System.Collections.Generic;
using HistoryService.DB;

namespace HistoryService.Models
{
    public class HistoryOutput
    {
        public string Event { get; set; }
        public string EventMessage { get; set; }

        public static List<HistoryOutput> GetHistoryOutputList(List<History> histories)
        {
            var historyOutputs = new List<HistoryOutput>();
            histories.ForEach(x => historyOutputs.Add(new HistoryOutput{Event = x.Event.Title, EventMessage = x.EventMessage}));
            return historyOutputs;
        }
    }
}

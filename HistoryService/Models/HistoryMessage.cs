﻿using System;

namespace HistoryService.Models
{
    public class HistoryMessage
    {
        public Guid User { get; set; }
        public string Event { get; set; }
        public string EventMessage { get; set; }
    }
}

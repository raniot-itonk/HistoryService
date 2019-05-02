using System;
using Microsoft.EntityFrameworkCore;

namespace HistoryService.DB
{
    public class HistoryContext : DbContext
    {
        public HistoryContext(DbContextOptions<HistoryContext> options)
            : base(options)
        {
        }
        public DbSet<History> TaxHistories { get; set; }
        public DbSet<Event> Events { get; set; }
    }

    public class History
    {
        public long Id { get; set; }
        public Guid User { get; set; }
        public Event Event { get; set; }
        public string EventMessage { get; set; }
    }

    public class Event
    {
        public long Id { get; set; }
        public string Title { get; set; }
    }
}

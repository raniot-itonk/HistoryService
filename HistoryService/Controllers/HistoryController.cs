using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HistoryService.DB;
using HistoryService.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HistoryService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HistoryController : ControllerBase
    {
        private readonly ILogger<HistoryController> _logger;
        private readonly HistoryContext _context;

        public HistoryController(ILogger<HistoryController> logger, HistoryContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<HistoryOutput>>> Get(Guid user)
        {
            var histories = await _context.TaxHistories.Where(history => history.User == user).ToListAsync();
            var historyOutputList = HistoryOutput.GetHistoryOutputList(histories);
            return historyOutputList;
        }
    }
}
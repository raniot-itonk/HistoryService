using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace HistoryService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TaxRateController : ControllerBase
    {

        public TaxRateController()
        {
        }

        [HttpGet]
        public ActionResult<double> Get()
        {
            return Ok("Ok");
        }
    }
}
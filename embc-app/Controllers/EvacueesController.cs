using Gov.Jag.Embc.Public.DataInterfaces;
using Gov.Jag.Embc.Public.ViewModels.Search;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Gov.Jag.Embc.Public.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    public class EvacueesController : Controller
    {
        private readonly IDataInterface dataInterface;

        public EvacueesController(IDataInterface dataInterface)
        {
            this.dataInterface = dataInterface;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] EvacueeSearchQueryParameters query)
        {
            var evacuees = await dataInterface.GetPaginatedEvacueesAsync(query);
            return Json(evacuees);
        }
    }
}

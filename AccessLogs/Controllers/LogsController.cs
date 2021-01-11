using System.Threading;
using System.Threading.Tasks;
using AccessLogs.Data;
using AccessLogs.Service.Models;
using Microsoft.AspNetCore.Mvc;

namespace AccessLogs.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly AccessLogsDataManagement _accessLogsDataManagement;

        public LogsController(AccessLogsDataManagement accessLogsDataManagement)
        {
            _accessLogsDataManagement = accessLogsDataManagement;
        }

        [HttpGet("top-clients")]
        public async Task<IActionResult> GetTopClients([FromQuery] GetTopClientsRequest getTopClientsRequest, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var topClients = await _accessLogsDataManagement.GetTopClients(
                getTopClientsRequest.TopN,
                getTopClientsRequest.From,
                getTopClientsRequest.To,
                cancellationToken);

            return Ok(topClients);
        }
    }
}

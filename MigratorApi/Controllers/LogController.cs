using BusinessRulesEngine;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace MigratorApi.Controllers
{
    [ApiController]
    public class LogController: Controller
    {
        private readonly BatchManager _batchManager = new BatchManager();

        [HttpGet]
        [Route("api/Log/GetBatches")]
        public string GetBatches()
        {
            var batchs = _batchManager.GetBatches();
            var jsonResult = JsonConvert.SerializeObject(batchs);
            return jsonResult;
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using BusinessRulesManager.Entities;

namespace MigratorApi.Controllers
{
    [ApiController]
    public class BrController : Controller
    {
        private readonly BusinessRulesManager.BusinessRulesManager _bRulesManager = new BusinessRulesManager.BusinessRulesManager();
        [HttpGet]
        [Route("api/BR/GetBusinessRules")]
        public string GetBusinessRules(string origin)
        {
            var bRs = _bRulesManager.GetBusinessRules(origin);
            var jsonResult = JsonConvert.SerializeObject(bRs);
            return jsonResult;
        }

        [HttpPost]
        [Route("api/BR/SaveBusinessRules")]
        public string SaveBusinessRules([FromBody] List<BusinessRuleEntity> businessRules)
        {
            var bRs = _bRulesManager.GetBusinessRules("Source");
            var jsonResult = JsonConvert.SerializeObject(bRs);
            return jsonResult;
        }
    }
}

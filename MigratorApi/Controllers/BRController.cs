using BusinessRulesManager;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BusinessRulesManager.Entities;

namespace MigratorApi.Controllers
{
    [ApiController]
    public class BRController : Controller
    {
        private readonly BusinessRulesManager.BusinessRulesManager _BRulesManager = new BusinessRulesManager.BusinessRulesManager();
        [HttpGet]
        [Route("api/BR/GetBusinessRules")]
        public string GetBusinessRules()
        {
            var BRs = _BRulesManager.GetBusinessRules();
            var jsonResult = JsonConvert.SerializeObject(BRs);
            return jsonResult;
        }

        [HttpPost]
        [Route("api/BR/SaveBusinessRules")]
        public string SaveBusinessRules([FromBody] List<BusinessRuleEntity> BusinessRules)
        {
            var BRs = _BRulesManager.GetBusinessRules();
            var jsonResult = JsonConvert.SerializeObject(BRs);
            return jsonResult;
        }
    }
}

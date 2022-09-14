using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections.Generic;
using BusinessRulesManager.Entities;
using BusinessRulesEngine.Entities;

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
        
        [HttpGet]
        [Route("api/BR/GetInActiveBusinessRules")]
        public string GetInActiveBusinessRules(string origin)
        {
            var bRs = _bRulesManager.GetInActiveBusinessRules(origin);
            var jsonResult = JsonConvert.SerializeObject(bRs);
            return jsonResult;
        }

        [HttpGet]
        [Route("api/BR/GetRelationRules")]
        public string GetRelationRules()
        {
            var bRs = _bRulesManager.GetRelationRules();
            var jsonResult = JsonConvert.SerializeObject(bRs);
            return jsonResult;
        }

        [HttpPost]
        [Route("api/BR/SaveBusinessRules")]
        public string SaveBusinessRules([FromBody] List<BusinessRuleEntity> businessRules)
        {
            _bRulesManager.SaveBusinessRules(businessRules);
            var jsonResult = "Ok";
            return jsonResult;
        }

        [HttpPost]
        [Route("api/BR/SaveRelationRules")]
        public string SaveRelationRules([FromBody] List<RelationRuleObject> relationRules)
        {
            _bRulesManager.SaveRelationRules(relationRules);
            var jsonResult = "Ok";
            return jsonResult;
        }
    }
}

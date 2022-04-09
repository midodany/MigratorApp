using BusinessRulesEngine;
using Microsoft.AspNetCore.Mvc;

namespace MigratorApi.Controllers
{
    [ApiController]
    public class MigratorController : Controller
    {
        private readonly Extractor.Extractor _extractor = new Extractor.Extractor();
        private readonly DataMigrator.DataMigrator _dataMigrator = new DataMigrator.DataMigrator();

        [HttpGet]
        [Route("api/Migrator/RunMigration")]
        public void RunMigration()
        {
            BatchManager batchManager = new BatchManager();

            _extractor.StartExtractor();
            _dataMigrator.StartDataMigrator(batchManager.CreateNewBatch());
        }
    }
}
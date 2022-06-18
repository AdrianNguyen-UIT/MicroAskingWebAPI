using MicroAskingWebApi.Clients;
using MicroAskingWebApi.Models;
using MicroAskingWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDBDataAccess.DataAccess;
using MongoDBDataAccess.Models;
using System.Net;
using System.Net.Sockets;

namespace MicroAskingWebApi.Controllers
{
    [ApiController]
    [Route("/api.micro-asking.ask")]
    public class MicroAskingController : ControllerBase
    {
        private readonly LuceneSearcher _luceneSearcher;
        private readonly SocketService _socketClient;
        private readonly string _connectionString;
        private readonly string _sepToken;
        private readonly int _limtTopResults;

        public MicroAskingController(ISingletonAService luceneService, ISingletonBService socketService, IConfiguration configuration)
        {
            _luceneSearcher = (LuceneSearcher)luceneService;
            _socketClient = (SocketService)socketService;
            _connectionString = configuration.GetValue<string>("ConnectionStrings:Default");
            _sepToken = configuration.GetValue<string>("CustomToken:Sep");
            _limtTopResults = configuration.GetValue<int>("TopResultsLimit");
        }

        [HttpPost]
        [Route("save")]
        public async Task<ActionResult<Document[]>> SaveDocs(Document[] docs)
        {
            if (docs == null)
                return NotFound();
            await _luceneSearcher.SaveDocs(docs);
            return Ok();
        }

        [HttpGet]
        [Route("ask/{question}")]
        public async Task<ActionResult<ResultDBModel>> Ask(string question)
        {
            if (question[question.Length - 1] != '?') question += "?";
            if (string.IsNullOrWhiteSpace(question))
                return BadRequest("Invalid question!: " + question);
            ResultDataAccess db = new ResultDataAccess(_connectionString);
            var cachedResult = await db.GetResultByQuestion(question);

            if (cachedResult.Count != 0)
            {
                var cached = cachedResult[0];
                cached.Count++;
                await db.UpdateResult(cached);
                return Ok(cached);
            }

            QAInput input = await _luceneSearcher.Search(question);
            Result[] results = await _socketClient.Send(input, _sepToken);

            _luceneSearcher.SaveQuestion(question);
            ResultDBModel resultDBModel = new() { Question = question, Count = 1 };

            foreach (var result in results)
            {
                resultDBModel.Answers.Add(new AnswerDBModel()
                {
                    Domain = result.Domain,
                    Answer = result.Answer,
                });
            }
            await db.CreateResult(resultDBModel);
            return Ok(resultDBModel);
        }

        [HttpGet]
        [Route("hint/{hint}")]
        public async Task<ActionResult<string[]>> Hint(string hint)
        {
            if (string.IsNullOrWhiteSpace(hint))
                return BadRequest("Invalid!");

            string[] results = await _luceneSearcher.SearchForHints(hint);
            return Ok(results);
        }

        [HttpGet]
        [Route("statistics/get-top-questions")]
        public async Task<ActionResult<ResultDBModel[]>> GetTopQuestion()
        {
            ResultDataAccess db = new ResultDataAccess(_connectionString);
            var results = await db.GetTopResults(_limtTopResults);
            return Ok(results.ToArray());
        }
    }
}

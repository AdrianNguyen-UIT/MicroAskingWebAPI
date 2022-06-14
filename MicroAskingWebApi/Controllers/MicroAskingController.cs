using BlingFire;
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
        private readonly IConfiguration _configuration;
        private readonly ulong _toIdsTokenizer;
        private readonly ulong _toWordsTokenizer;

        public MicroAskingController(ISingletonService singletonService, IConfiguration configuration)
        {
            _luceneSearcher = (LuceneSearcher)singletonService;
            _configuration = configuration;
            _toIdsTokenizer = BlingFireUtils.LoadModel(_configuration.GetValue<string>("Tokenizer:ToIdsPath"));
            _toWordsTokenizer = BlingFireUtils.LoadModel(_configuration.GetValue<string>("Tokenizer:ToWordsPath"));
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
        public async Task<ActionResult<ResultDBModel>> Ask(string question, [FromServices] InferenceHttpClient client)
        {
            if (string.IsNullOrWhiteSpace(question))
                return BadRequest("Invalid question!: " + question);
            ResultDataAccess db = new ResultDataAccess(_configuration.GetValue<string>("ConnectionStrings:Default"));
            var cachedResult = await db.GetResultByQuestion(question);

            if (cachedResult.Count != 0)
            {
                var cached = cachedResult[0];
                cached.Count++;
                await db.UpdateResult(cached);
                return Ok(cached);
            }

            QAInput input = await _luceneSearcher.Search(question);
            //Result[] results = await client.Inference(input);

            int port = 13000;
            IPAddress localAddr = IPAddress.Parse("127.0.0.1");


            var bytesCollection = input.GetBytes();
            Result[] results = new Result[bytesCollection.Length];
            Console.WriteLine(bytesCollection.Length);
            for (int index = 0; index < bytesCollection.Length; index++)
            {
                TcpClient aswpClient = new TcpClient();
                await aswpClient.ConnectAsync(localAddr, port);
                NetworkStream networkStream = aswpClient.GetStream();

                var byt = bytesCollection[index];
                await networkStream.WriteAsync(byt);

                byte[] data = new byte[byt.Length];

                int bytes = await networkStream.ReadAsync(data);
                //string responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);


                // Process respone here

                results[index] = new Result();
                results[index].Domain = input.Contexts[index].Domain;
                results[index].Context = input.Contexts[index].Content;
                networkStream.Close();
                aswpClient.Close();
            }

            //_luceneSearcher.SaveQuestion(question);
            ResultDBModel resultDBModel = new() { Question = question, Count = 1 };

            //foreach (var result in results)
            //{
            //    resultDBModel.Answers.Add(new AnswerDBModel()
            //    { 
            //        Score_Start = result.Score_Start,
            //        Score_End = result.Score_End,
            //        Score = result.Score,
            //        Domain = result.Domain,
            //        Answer = result.Answer,
            //        Context = result.Context
            //    });
            //}
            //await db.CreateResult(resultDBModel);
            return Ok(resultDBModel);
        }

        private int ToInt32(object value, int int32)
        {
            throw new NotImplementedException();
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
    }
}

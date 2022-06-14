using Lucene.Net.Analysis;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers.Classic;
using Lucene.Net.Search;
using Lucene.Net.Store;
using Lucene.Net.Util;
using LuceneDirectory = Lucene.Net.Store.Directory;

namespace MicroAskingWebApi.Services
{
    public class LuceneSearcher : ISingletonService
    {
        private readonly LuceneVersion _luceneVersion;
        private readonly Analyzer _standardAnalyzer;
        private readonly IndexWriter _contextWriter;
        private readonly IndexWriter _questionWriter;
        private readonly int _queryCount;
        private readonly IConfiguration _configuration;

        public LuceneSearcher(IConfiguration configuration)
        {
            _configuration = configuration;
            _queryCount = configuration.GetValue<int>("LuceneSettings:QueryCount");

            _luceneVersion = LuceneVersion.LUCENE_48;
            _standardAnalyzer = new StandardAnalyzer(_luceneVersion);

            //Create context writer
            string _contextIndexName = configuration.GetValue<string>("LuceneSettings:ContextIndexPath");
            _contextWriter = CreateIndexWriter(Path.Combine(Environment.CurrentDirectory, _contextIndexName));

            //Create question writer
            string _questionIndexName = configuration.GetValue<string>("LuceneSettings:QuestionIndexPath");
            _questionWriter = CreateIndexWriter(Path.Combine(Environment.CurrentDirectory, _questionIndexName));
        }

        private IndexWriter CreateIndexWriter(string indexName)
        {
            string indexPath = Path.Combine(Environment.CurrentDirectory, indexName);
            LuceneDirectory contextIndexDir = FSDirectory.Open(indexPath);
            IndexWriterConfig contextIndexConfig = new(_luceneVersion, _standardAnalyzer)
            {
                OpenMode = OpenMode.CREATE_OR_APPEND
            };

            return new IndexWriter(contextIndexDir, contextIndexConfig);
        }

        public async Task SaveDocs(Models.Document[] docs)
        {
            List<Task> tasks = new List<Task>();
            foreach (var doc in docs)
            {
                tasks.Add(AddDoc(doc));
            }

            await Task.WhenAll(tasks);
            if (_contextWriter != null)
            {
                _contextWriter.Commit();
                Console.WriteLine("Saved " + docs.Length + " Lucene Documents");
            }

            await Task.CompletedTask;
        }

        public void SaveQuestion(string question)
        {
            Document doc = new()
                {
                    new TextField("question", question, Field.Store.YES)
                };

            if (_questionWriter != null)
            {
                _questionWriter.AddDocument(doc);
                _contextWriter.Commit();
                Console.WriteLine("Saved " + question + " Lucene Documents");
            }
        }

        public async Task<string[]> SearchForHints(string hint)
        {
            List<string> hints = new List<string>();
            if (_questionWriter == null)
            {
                await Task.CompletedTask;
                return hints.ToArray();
            }

            using DirectoryReader reader = _questionWriter.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);
            
            QueryParser parser = new QueryParser(_luceneVersion, "question", _standardAnalyzer);
            Query query = parser.Parse(hint);
            TopDocs topDocs = searcher.Search(query, n: _queryCount);

            Console.WriteLine($"Matching results: {topDocs.TotalHits}");

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                Document resultDoc = searcher.Doc(scoreDoc.Doc);
                hints.Add(resultDoc.Get("question"));
            }

            await Task.CompletedTask;
            return hints.ToArray();
        }


        public async Task<Models.QAInput> Search(string question)
        {
            Models.QAInput qaInput = new Models.QAInput();
            qaInput.Question = question;

            if (_contextWriter == null)
            {
                await Task.CompletedTask;
                return qaInput;
            }

            using DirectoryReader reader = _contextWriter.GetReader(applyAllDeletes: true);
            IndexSearcher searcher = new IndexSearcher(reader);

            MultiFieldQueryParser parser = new MultiFieldQueryParser(_luceneVersion, new string[] { Models.Document.ContentKey, Models.Document.TitleKey }, _standardAnalyzer);
            Query query = parser.Parse(question);
            TopDocs topDocs = searcher.Search(query, n: _queryCount);

            Console.WriteLine($"Matching results: {topDocs.TotalHits}");

            foreach (var scoreDoc in topDocs.ScoreDocs)
            {
                Document resultDoc = searcher.Doc(scoreDoc.Doc);

                string content = resultDoc.Get(Models.Document.ContentKey);
                string domain = resultDoc.Get(Models.Document.DomainKey);
                qaInput.Contexts.Add(new Models.Context { Content = content, Domain = domain });
            }

            await Task.CompletedTask;
            return qaInput;
        }

        private async Task AddDoc(Models.Document doc)
        {
            Document ldoc = new()
                {
                    new TextField(Models.Document.TitleKey, doc.Title, Field.Store.YES),
                    new TextField(Models.Document.ContentKey, doc.Content, Field.Store.YES),
                    new StringField(Models.Document.DomainKey, doc.Domain, Field.Store.YES)
                };

            if (_contextWriter != null)
            {
                _contextWriter.AddDocument(ldoc);
            }

            await Task.CompletedTask;
        }
    }
}

using System.Runtime.InteropServices;

namespace MicroAskingWebApi.Models
{
    public class Context
    {
        public string Content { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
    }

    public class QAInput
    {
        public string Question { get; set; } = string.Empty;

        public List<Context> Contexts { get; set; } = new List<Context>();

        public string[] GetCombinedStrings(string sepToken)
        {
            string[] results = new string[Contexts.Count];

            for (int index = 0; index < results.Length; index++)
            {
                var context = Contexts[index].Content;
                if (!string.IsNullOrWhiteSpace(context))
                {
                    results[index] = Question + sepToken + context;
                }
            }

            return results;
        }
    }
}

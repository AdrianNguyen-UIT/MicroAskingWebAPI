namespace MicroAskingWebApi.Models
{
    public class Document
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;

        public static string TitleKey => "title";
        public static string ContentKey => "content";
        public static string DomainKey => "domain";
    }
}

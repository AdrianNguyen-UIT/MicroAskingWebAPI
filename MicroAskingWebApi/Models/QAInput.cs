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

        public byte[][] GetBytes()
        {
            List<byte> questionBytes = new List<byte>() { 0 };
            questionBytes.AddRange(System.Text.Encoding.UTF8.GetBytes(Question).ToList());
            questionBytes.Add(2);
            List<byte[]> bytes = new List<byte[]>();
            foreach (var context in Contexts)
            {
                List<byte> data = new List<byte>(questionBytes);
                data.AddRange(System.Text.Encoding.UTF8.GetBytes(context.Content).ToList());
                data.Add(2);
                bytes.Add(data.ToArray());
            }

            return bytes.ToArray();
        }
    }
}

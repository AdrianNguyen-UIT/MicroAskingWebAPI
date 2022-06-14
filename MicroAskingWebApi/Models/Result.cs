using Microsoft.Net.Http.Headers;
using System.Text.Json;

namespace MicroAskingWebApi.Models
{
    public class Result : IComparable<Result>
    {
        public float Score_Start { get; set; } = 0.0f;
        public float Score_End { get; set; } = 0.0f;
        public string Answer { get; set; } = string.Empty;
        public string Domain { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public float Score => (Score_Start + Score_End) / 2.0f;

        int IComparable<Result>.CompareTo(Result? other)
        {
            if (other == null) return 1;
            return Score.CompareTo(other.Score) * -1;
        }
    }
}

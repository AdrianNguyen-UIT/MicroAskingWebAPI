using MicroAskingWebApi.Models;

namespace MicroAskingWebApi.Clients
{
    public class InferenceHttpClient
    {
        private readonly HttpClient _client;
        private readonly string _inferenceUri;

        public InferenceHttpClient(HttpClient client, IConfiguration configuration)
        {
            _client = client;
            _inferenceUri = configuration.GetValue<string>("URLs:Inference:Routes:Post");
            _client.BaseAddress = new Uri(configuration.GetValue<string>("URLs:Inference:Base"));
        }

        public async Task<Result[]> Inference(QAInput input)
        {
            var response = await _client.PostAsJsonAsync(_inferenceUri, input);
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadFromJsonAsync<Result[]>();
                if (responseContent != null)
                    Array.Sort(responseContent);
                else
                    responseContent = Array.Empty<Result>();
                return responseContent;
            }
            else
            {
                return Array.Empty<Result>();
            }
        }
    }
}

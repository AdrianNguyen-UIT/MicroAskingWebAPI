using System.Net;
using System.Net.Sockets;
using MicroAskingWebApi.Models;
namespace MicroAskingWebApi.Services
{
    public class SocketService : ISingletonBService
    {
        private readonly int _port;
        private readonly IPAddress _ipAddress;

        public SocketService(IConfiguration configuration)
        {
            _port = configuration.GetValue<int>("SocketServer:Port");
            _ipAddress = IPAddress.Parse(configuration.GetValue<string>("SocketServer:IpAddress"));
        }

        public async Task<Result[]> Send(QAInput input, string sepToken)
        {
            string[] stringData = input.GetCombinedStrings(sepToken);
            List<Result> results = new List<Result>();
            for (int index = 0; index < stringData.Length; index++)
            {
                TcpClient aswpClient = new TcpClient();
                await aswpClient.ConnectAsync(_ipAddress, _port);
                NetworkStream networkStream = aswpClient.GetStream();

                byte[] stringDataInBytes = System.Text.Encoding.UTF8.GetBytes(stringData[index]);
                await networkStream.WriteAsync(stringDataInBytes, 0, stringDataInBytes.Length);

                byte[] data = new byte[stringDataInBytes.Length];
                int bytes = await networkStream.ReadAsync(data);
                string responseData = System.Text.Encoding.UTF8.GetString(data, 0, bytes).Replace("\0", "");

                if (!string.IsNullOrWhiteSpace(responseData))
                {
                    results.Add(new Result
                    {
                        Answer = responseData,
                        Domain = input.Contexts[index].Domain
                    });
                }
                networkStream.Close();
                aswpClient.Close();
            }

            return results.ToArray();
        }
    }
}

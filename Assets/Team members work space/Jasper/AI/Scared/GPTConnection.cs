using System.Linq;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Responses;
using UnityEngine;

namespace Jasper_AI
{
    public class GPTConnection : MonoBehaviour
    {
        public delegate void GotResponse(string response);

        public event GotResponse OnGotResponse;

        public async Task GetResponse(string prompt = "say anything")
        {
            // OpenAIClient api = new OpenAIClient("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");
            // Response response = await api.ResponsesEndpoint.CreateModelResponseAsync(prompt);
            // OnGotResponse?.Invoke(response.Output.LastOrDefault()?.ToString());
        }
    }
}

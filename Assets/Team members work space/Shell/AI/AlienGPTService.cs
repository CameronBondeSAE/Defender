using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Shell_AI
{
    public enum GPTDecision
    {
        Attack,
        Wait,
        Unknown
    }

    public class AlienGPTService : MonoBehaviour
    {
        [Tooltip("REMOVE BEFORE SUBMISSION")]
        public string apiKey;

        const string apiUrl = "https://api.openai.com/v1/chat/completions";

        public async Task<GPTDecision> QueryDecisionAsync(string context)
        {
            string prompt =
                "Respond with ATTACK or WAIT only.\n\n" + context;

            string json = JsonUtility.ToJson(new GPTRequest
            {
                model = "gpt-3.5-turbo",
                messages = new[]
                {
                    new GPTMessage
                    {
                        role = "user",
                        content = prompt
                    }
                }
            });

            using UnityWebRequest req =
                new UnityWebRequest(apiUrl, "POST");

            req.uploadHandler =
                new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler =
                new DownloadHandlerBuffer();

            req.SetRequestHeader(
                "Authorization",
                $"Bearer {apiKey}"
            );
            req.SetRequestHeader(
                "Content-Type",
                "application/json"
            );

            var op = req.SendWebRequest();
            while (!op.isDone)
                await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
                return GPTDecision.Unknown;

            if (req.downloadHandler.text.Contains("ATTACK"))
                return GPTDecision.Attack;

            return GPTDecision.Wait;
        }

        [Serializable]
        class GPTRequest
        {
            public string model;
            public GPTMessage[] messages;
        }

        [Serializable]
        class GPTMessage
        {
            public string role;
            public string content;
        }
    }
}

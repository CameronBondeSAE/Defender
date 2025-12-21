using OpenAI;
using OpenAI.Chat;
using OpenAI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

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
        [TextArea(5, 10)]
        public string systemMessage =
            "You are an alien commander in a game. " +
            "Only reply with ATTACK or WAIT.";

        [Tooltip("REMOVE BEFORE SUBMISSION")]
        public string apiKey;

        OpenAIClient api;
        List<Message> messages = new();

        void Awake()
        {
            api = new OpenAIClient(apiKey);
            messages.Add(new Message(Role.System, systemMessage));
        }

        public async Task<GPTDecision> QueryDecisionAsync(string context)
        {
            messages.Add(new Message(Role.User, context));

            ChatRequest request = new ChatRequest(
                messages,
                Model.GPT4oMini
            );

            ChatResponse response =
                await api.ChatEndpoint.GetCompletionAsync(request);

            string reply =
                response.FirstChoice.Message.Content
                .ToString()
                .ToUpper();

            Debug.Log("GPT raw reply: " + reply);

            messages.Add(new Message(Role.Assistant, reply));

            if (reply.Contains("ATTACK")) return GPTDecision.Attack;
            if (reply.Contains("WAIT")) return GPTDecision.Wait;

            return GPTDecision.Unknown;
        }
    }
}
 
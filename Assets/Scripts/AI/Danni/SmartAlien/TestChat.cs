using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using OpenAI;
using OpenAI.Chat;

public class TestChat : MonoBehaviour
{
    private async void Start()
    {
        await TestChatAsync();
    }

    private async Task TestChatAsync()
    {
        try
        {
            var api = new OpenAIClient();

            var messages = new List<Message>
            {
                new Message(Role.System, "You are a friendly NPC in a video game."),
                new Message(Role.User, "Say a friendly greeting in one short sentence.")
            };

            var request  = new ChatRequest(messages, model: "gpt-4o-mini");
            var response = await api.ChatEndpoint.GetCompletionAsync(request);

            var reply = response.FirstChoice.Message.Content;
            Debug.Log($"OpenAI npc says: {reply}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"OpenAI test failed: {ex}");
        }
    }
}

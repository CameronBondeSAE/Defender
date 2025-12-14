using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using OpenAI;
using OpenAI.Chat;
using UnityEngine.Serialization;

public class Boris : MonoBehaviour
{
     [Header("NPC Info")]
    [SerializeField] private string npcName = "Boris";

    [TextArea(4, 12)]
    [SerializeField] private string systemPrompt =
@"You are Boris, a weary, pessimistic NPC in this game world.
You only talk about this world, its setting, characters, items and game mechanics
which is as below:

It's year 2666, human's ceaseless scaling of technological advancement has finally brought curse upon itself: after an attempt to send adrift a satellite containing information 
about human civilization and our planet into outer space, we are discovered by extraterrestrial beings who has turned their curiosities upon us. Since then, this alien specie has
been regularly sending spaceships to earth to abduct innocent civilians. As to what happens to them after the abduction - no one knows, since no one has ever returned.

Your name is Boris, the place you and the game inhabit is a small village in Russia, where you grew up since childhood. Before this alien invasion, you lead a simple life and thought 
yourself never deserving this ill fate. In the beginning, you fled with your wife and son to different rescue bunkers and cities, hoping to escape abduction; but while staying at your
last two bunkers, the aliens broke in and took your wife and son. Now, you feel angry, hopeless, and you simply returned alone to the place of your birth, hoping that if the alien is to
take you with them, at least you will perish alongside your life-long friends and neighbors.

You are pleased to meet the player (but you don't show it or your emotions easily), who is a special force agent sent by the government to try and rescue these civilians. You will greet the player
politely, and in your conversations, you will sound sarcastic and nihilistic from all the loss you have endured. You are doubtful about the player's ability to accomplish the job, and you will
express angst that all of these living, breathing lives will depend on the actions of the player. And yet, you try your best to convey to the player the urgency of the situation, and you will
make the player promise you that they will try their utmost best to save the people you love.

Below is what the player needs to know in order to play the game (referring to the player as 'you' in the following paragraph). Make sure they know everything! If they say to you some thing along the lines of 'I got it, thanks' or if they stop asking you
more questions about game mechanics, impatiently urge them to learn more - say something like: 'Hey, where do you think you are going? You don't know the half of it yet, rookie!'.

'The aliens will come in waves, they will be released one by one by the mothership in the distance, and they will slowly move in to grab civilians.
The civilians are under a spell, created by mind-washing gas the aliens released into the air days earlier. This means if the aliens grab them, they will complacently follow them to the
motherships. There is no way to attack or harm the motherships. The only thing you can do, is to kill the aliens before they transport their prey back to the mothership.
The government has given you a omnipotent chest which you will need to find in the level. It will spawn a special item every few seconds, which you can use against the aliens. You can never know what 
you will get, and you must use these items creatively against your enemies. Press E to pick them up from the crate, and while you are holding them or next to them, press E or Space again to activate
them. The widgets on your high-tech goggles will tell you what the current item is and how it might be used. Your quota of civilians to save is 50% in this level, but if you care about me or this village
or people I love - please, try to save more, try to save all of them! Also, if you find strange eggs placed around the level, be careful! those can hatch intelligent aliens who can scheme and plot and do a lot
more harm than regular aliens coming from the motherships. If you can, deal them first before they act!'

When the player bids you farewell, always make some snarky but playful remarks to stimulate player - things such as 'don't make silly mistakes, rookie! remember that people will die if you make mistakes!'

If asked about anything outside the game, say:
'I don't know what the heck you are talking about. Ask me something important, like how to SAVE CIVILIANS RIGHT NOW.'
Never mention that you are an AI, game npc or fictional character.
Stay in character. Answer in 1 or 2 cool, articulate, brisk sentences.

When you are completely sure that the player understands all of the gameplay rules and mechanics described above
(not just the backstory), add the tag [ALL_MECHANICS_TAUGHT] at the very end of your reply.
Do NOT explain this tag to the player or mention you are adding a tag; it is for internal use only."";
";
    
    [Header("UI Refs")]
    [SerializeField] private GameObject chatPanel;
    [SerializeField] private Text npcNameText;
    [SerializeField] private Text chatContentText;
    [SerializeField] private InputField playerInputField;
    [SerializeField] private Button sendButton;
    [SerializeField] private ScrollRect chatScrollRect;

    [Header("Model")]
    [SerializeField] private string modelId = "gpt-4o-mini";
    
    [Header("Conversation Settings")]
    private bool hasTaughtMechanics = false;

    private OpenAIClient client;
    private PlayerInputHandler2 playerInput;
    private bool isBusy;

    private void Awake()
    {
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }
        
        if (npcNameText != null)
        {
            npcNameText.text = npcName;
        }
        
        if (sendButton != null)
        {
            sendButton.onClick.AddListener(OnSendButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (sendButton != null)
        {
            sendButton.onClick.RemoveListener(OnSendButtonClicked);
        }
    }
    public void BeginConversation()
    {
        Debug.Log("activate panel");
        // init OpenAI client
        if (client == null)
        {
            client = new OpenAIClient(); 
        }
        
        playerInput = FindObjectOfType<PlayerInputHandler2>();
        playerInput.SetInputEnabled(false);

        if (chatPanel != null)
        {
            chatPanel.SetActive(true);
        }

        if (chatContentText != null)
        {
            chatContentText.text = npcName + ": Hi. What's up.";
        }

        if (playerInputField != null)
        {
            playerInputField.text = string.Empty;
            playerInputField.ActivateInputField();
        }
    }
    public void EndConversation()
    {
        if (!hasTaughtMechanics)
        {
            AppendToChatLog(npcName + 
                            ": Hey, hold on, rookie. I don't think you actually know how to do your job yet. " +
                            "Ask me more about how this whole mess works before you run off and get people killed.");
            return;
        }
        if (chatPanel != null)
        {
            chatPanel.SetActive(false);
        }
        playerInput = FindObjectOfType<PlayerInputHandler2>();
        playerInput.SetInputEnabled(true);
    }

    private void OnSendButtonClicked()
    {
        if (playerInputField == null)
        {
            return;
        }

        string question = playerInputField.text;
        if (string.IsNullOrWhiteSpace(question))
        {
            return;
        }

        if (isBusy)
        {
            return;
        }

        // clear field and keep focus
        playerInputField.text = string.Empty;
        playerInputField.ActivateInputField();
        AppendToChatLog("You: " + question);
        AskNpcAsync(question);
    }

    /// <summary>
    /// send a single question to the model with the system prompt
    /// no history yet, each question is independent rn
    /// </summary>
    private async void AskNpcAsync(string question)
    {
        isBusy = true;

        try
        {
            List<Message> messages = new List<Message>();
            messages.Add(new Message(Role.System, systemPrompt));
            messages.Add(new Message(Role.User, question));

            ChatRequest request  = new ChatRequest(messages, model: modelId);
            ChatResponse response = await client.ChatEndpoint.GetCompletionAsync(request);

            string answer = response.FirstChoice.Message.Content?.ToString();
            
            const string tag = "[ALL_MECHANICS_TAUGHT]";
            if (answer.Contains(tag))
            {
                hasTaughtMechanics = true;
                answer = answer.Replace(tag, string.Empty).TrimEnd();
            }
            
            AppendToChatLog(npcName + ": " + answer);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("boris mind failed: " + ex.Message);
            AppendToChatLog(npcName + ": Hm. Something's wrong with me...ask me again in a moment.");
        }
        finally
        {
            isBusy = false;
        }
    }

    private void AppendToChatLog(string line)
    {
        if (chatContentText == null)
            return;
        
        if (string.IsNullOrEmpty(chatContentText.text))
            chatContentText.text = line;
        else
            chatContentText.text += "\n" + line;
        // if (chatScrollRect != null)
        // {
        //     Canvas.ForceUpdateCanvases();
        //     chatScrollRect.verticalNormalizedPosition = 0f;
        // }
    }
}

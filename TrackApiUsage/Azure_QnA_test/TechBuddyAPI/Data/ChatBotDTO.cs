using Azure.AI.Language.QuestionAnswering;

namespace TechBuddyAPI.Data
{
    public class ChatBotDTO
    {
        public class AnswerWithpromptDTO
        {
            public int ClassID { get; set; } = 2;
            public string? BotAnswer { get; set; }

            public KnowledgeBaseAnswerPrompt? BotPrompt { get; set; }
        }

        public class AnswerDTO
        {
            public int ClassID { get; set; } = 1;
            public string? BotAnswer { get; set; }

        }

        public class WrongAnswerDTO
        {
            public int ClassID { get; set; } = 3;
            public string? BotAnswer { get; set; }
            public string? BotAnswerSecond { get; set; }
        }

    }

}

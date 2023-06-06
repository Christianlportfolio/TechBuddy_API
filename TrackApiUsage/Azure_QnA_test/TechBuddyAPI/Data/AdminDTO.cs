using Azure.AI.Language.QuestionAnswering;

namespace TechBuddyAPI.Data
{
    public class AdminDTO
    {
        public class AnswerAdminDTO
        {
            public int ClassID { get; set; } = 1;
            public IReadOnlyList<string> Questions { get; set; }
            //public IReadOnlyDictionary<string, string> Metadatas { get; set; }
            public string Answer { get; set; } = "string";
            public double? Confidence { get; set; } = 0;
            public int? QnaId { get; set; } = 0;
            public string Source { get; set; } = "string";
        }

        public class AnswerAdminWithPromptDTO
        {
            public int ClassID { get; set; } = 2;
            public IReadOnlyList<string> Questions { get; set; }
            //public IReadOnlyDictionary<string, string> Metadatas { get; set; }
            public string Answer { get; set; } = "string";
            public double? Confidence { get; set; } = 0;
            public int? QnaId { get; set; } = 0;
            public string Source { get; set; } = "string";
            public bool? IsContextOnly { get; set; } = true;
            public int? DisplayOrder { get; set; } = 0;
            public int? PromptQnaId { get; set; } = 0;
            public string DisplayText { get; set; } = "string";
        }

        public class NoAnswerAdminDTO
        {
            public int ClassID { get; set; } = 3;
            public string Answer { get; set; } = "string";
        }

    }



}
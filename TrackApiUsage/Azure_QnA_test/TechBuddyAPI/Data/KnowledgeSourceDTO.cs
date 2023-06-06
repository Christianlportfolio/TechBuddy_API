namespace TechBuddyAPI.Data
{
    public class KnowledgeSourceDTO
    {
        //public KnowledgeSourceDTO(string question, string answer) 
        //{
        //    Question = question;
        //    Answer = answer;    
        //} 
        public class AddQnADTO
        {
            public string Question { get; set; }
            public string Answer { get; set; }
        }

        public class AddQnADTOWithPrompt
        {
            public string Question { get; set; }
            public string Answer { get; set; }

            public string DisplayText { get; set; }
        }


        public class UpdateQnAQuestionDTO
        {
            public int id { get; set; }
            public string Question { get; set; }
        }

        public class UpdateQnADTO
        {
            public int id { get; set; }
            public string Answer { get; set; }

            public string Question { get; set; }
        }

        public class AddKnowledgeBaseSourceDTO
        {
            public string DisplayName { get; set; }
            public string source { get; set; }

            public string sourceUri { get; set;}

            public string sourceKind { get; set;}

        }




    }
}

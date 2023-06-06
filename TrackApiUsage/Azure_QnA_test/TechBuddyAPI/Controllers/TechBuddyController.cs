using Azure;
using Azure.AI.Language.QuestionAnswering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using TechBuddyAPI.Data;
using TechBuddyAPI.Models;


namespace TechBuddyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TechBuddyController : ControllerBase
    {
        private readonly TechBuddyContext _context;
        private readonly QuestionAnsweringClient client;
        private readonly QuestionAnsweringProject project;
        private readonly IConfiguration _config;

        private string projectName = "TechBuddy";
        private string deploymentName = "production";

        public TechBuddyController(TechBuddyContext context, IConfiguration config)
        {
            _config = config;
            Uri endpoint = new Uri("https://techbuddydk.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new AzureKeyCredential(config["Secrets:AzureKeyCredential"]);

            client = new QuestionAnsweringClient(endpoint, credential);

            project = new QuestionAnsweringProject(projectName, deploymentName);
            _context = context;
        }


        [HttpGet("{question}")]
        public async Task<ActionResult<Response<AnswersResult>>> GetAnswer(string question)
        {

            List<ChatBotDTO.AnswerWithpromptDTO> answersWithPrompt = new List<ChatBotDTO.AnswerWithpromptDTO>();
            List<ChatBotDTO.AnswerDTO> answer = new List<ChatBotDTO.AnswerDTO>();
            List<ChatBotDTO.WrongAnswerDTO> wrongAnswer = new List<ChatBotDTO.WrongAnswerDTO>();

            TechBuddyController controller = new TechBuddyController(this._context, this._config);
            Response<AnswersResult> response = await controller.client.GetAnswersAsync($"{question}", project);


            for (int i = 0; i < response.Value.Answers.Count; i++)
            {
                if (response.Value.Answers[i].Confidence > 0.5)
                {
                    try
                    {
                        if (response.Value.Answers[i].Dialog == null)
                        {
                            if (response.Value.Answers[i].Answer == "Øv... intet svar fundet :(")
                            {
                                wrongAnswer.Add(new ChatBotDTO.WrongAnswerDTO()
                                {
                                    BotAnswer = response.Value.Answers[i].Answer,
                                    BotAnswerSecond = "Prøv at omformulere dit spørgsmål eller tryk på hjælp",
                                }); 
                                return Ok(wrongAnswer);
                            }
                        }
                        else if (response.Value.Answers[i].Dialog.Prompts.IsNullOrEmpty())
                        {
                            answer.Add(new ChatBotDTO.AnswerDTO()
                            {
                                BotAnswer = response.Value.Answers[i].Answer

                            });
                            return Ok(answer);
                        }
                        else
                        {
                            answersWithPrompt.Add(new ChatBotDTO.AnswerWithpromptDTO()
                            {
                                BotAnswer = response.Value.Answers[i].Answer,
                                BotPrompt = response.Value.Answers[i].Dialog.Prompts[i]

                            });
                            return Ok(answersWithPrompt);

                        }
                    }

                    catch (Exception e)
                    {
                        return Conflict(e.Message);
                    }
                    //answers.Add(new Answer()
                    //        {
                    //            BotAnswer = response.Value.Answers[i].Answer,
                    //            BotPrompt = response.Value.Answers[i].Dialog.Prompts[i]

                    //        });
                }

                wrongAnswer.Add(new ChatBotDTO.WrongAnswerDTO()
                {
                    BotAnswer = "Øv... intet svar fundet :(",
                    BotAnswerSecond = "Prøv at omformulere dit spørgsmål eller tryk på hjælp",
                }); ;
                return Ok(wrongAnswer);

                //return null;

            }
            return Ok(null);

        }




        // CREATE
        [HttpPost("createCustomerQuestionForm")]
        public async Task<IActionResult> CreateForm([FromBody] CustomerQuestion customerQuestion)
        {
            try
            {
                customerQuestion.SubmitDate = DateTime.Now;
                customerQuestion.IsChecked = false;
                _context.CustomerQuestion.Add(customerQuestion);
                await _context.SaveChangesAsync();
            }

            catch (Exception e)
            {
                return Conflict(e.Message);
            }
            return Ok(customerQuestion);
        }











    }
}

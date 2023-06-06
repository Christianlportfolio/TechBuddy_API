using Azure;
using Azure.AI.Language.QuestionAnswering;
using Azure.AI.Language.QuestionAnswering.Authoring;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;
using TechBuddyAPI.Data;
using TechBuddyAPI.Models;
using static TechBuddyAPI.Data.KnowledgeSourceDTO;

namespace TechBuddyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashBoardController : ControllerBase
    {
        private readonly TechBuddyContext _context;
        private readonly QuestionAnsweringClient client;
        private readonly QuestionAnsweringAuthoringClient authroingClient;
        private readonly QuestionAnsweringProject project;
        private readonly IConfiguration _config;
        

        private readonly string projectName = "TechBuddy";
        private readonly string deploymentName = "production";
        public DashBoardController(TechBuddyContext context, IConfiguration config)
        {
            _config = config;
            Uri endpoint = new Uri("https://techbuddydk.cognitiveservices.azure.com/");
            AzureKeyCredential credential = new AzureKeyCredential(config["Secrets:AzureKeyCredential"]);

            client = new QuestionAnsweringClient(endpoint, credential);
            authroingClient = new QuestionAnsweringAuthoringClient(endpoint, credential);

            project = new QuestionAnsweringProject(projectName, deploymentName);
            _context = context;
        }

        [HttpGet("admin/{question}")]
        public async Task<ActionResult<List<AdminDTO.AnswerAdminDTO>>> GetAnswerAdmin(string question)
        {
            List<AdminDTO.AnswerAdminDTO> answerAdminDTOs = new List<AdminDTO.AnswerAdminDTO>();
            List<AdminDTO.AnswerAdminWithPromptDTO> answerAdminWithPromptDTOs = new List<AdminDTO.AnswerAdminWithPromptDTO>();
            List<AdminDTO.NoAnswerAdminDTO> noAnswerAdminDTOs = new List<AdminDTO.NoAnswerAdminDTO>();
            DashBoardController controller = new DashBoardController(this._context, this._config);
            Response<AnswersResult> response = await controller.client.GetAnswersAsync($"{question}", project);

            for (int i = 0; i < response.Value.Answers.Count; i++)
            {
                try
                {
                    if (response.Value.Answers[i].Answer == "Øv... intet svar fundet :(")
                    {
                        noAnswerAdminDTOs.Add(new AdminDTO.NoAnswerAdminDTO()
                        {
                            Answer = response.Value.Answers[i].Answer,
                        });
                        return Ok(noAnswerAdminDTOs);

                    }
                    else if (response.Value.Answers[i].Dialog.Prompts.IsNullOrEmpty())
                    {
                        answerAdminDTOs.Add(new AdminDTO.AnswerAdminDTO()
                        {
                            Questions = response.Value.Answers[i].Questions,
                            //Metadatas = response.Value.Answers[i].Metadata,
                            Answer = response.Value.Answers[i].Answer,
                            Confidence = response.Value.Answers[i].Confidence,
                            QnaId = response.Value.Answers[i].QnaId,
                            Source = response.Value.Answers[i].Source

                            //text = response.Value.Answers[i].ShortAnswer.Text,
                            //confidence = response.Value.Answers[i].ShortAnswer.Confidence,
                            //offset = response.Value.Answers[i].ShortAnswer.Offset,
                            //length = response.Value.Answers[i].ShortAnswer.Length,
                        });
                        return Ok(answerAdminDTOs);
                    }
                    else
                    {
                        answerAdminWithPromptDTOs.Add(new AdminDTO.AnswerAdminWithPromptDTO()
                        {
                            Questions = response.Value.Answers[i].Questions,
                            //Metadatas = response.Value.Answers[i].Metadata,
                            Answer = response.Value.Answers[i].Answer,
                            Confidence = response.Value.Answers[i].Confidence,
                            QnaId = response.Value.Answers[i].QnaId,
                            Source = response.Value.Answers[i].Source,
                            IsContextOnly = response.Value.Answers[i].Dialog.IsContextOnly,
                            DisplayOrder = response.Value.Answers[i].Dialog.Prompts[i].DisplayOrder,
                            PromptQnaId = response.Value.Answers[i].Dialog.Prompts[i].QnaId,
                            DisplayText = response.Value.Answers[i].Dialog.Prompts[i].DisplayText
                        });
                        return Ok(answerAdminWithPromptDTOs);

                    }
                }
                catch (Exception e)
                {
                    return Conflict(e.Message);
                }
            }
            return Ok(null);
        }




        // GET ALL
        [HttpGet("GetAllCustomerQuestionFormData")]
        public async Task<ActionResult<List<CustomerQuestion>>> GetAllFormData()
        {
            try
            {
                var customerQuestion = _context.CustomerQuestion;
                return Ok(customerQuestion);
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }
        }


        //Delete
        [HttpDelete("DeleteCustomerQuestionFormData/{id}")]
        public async Task<IActionResult> DeleteFormData(int id)
        {
            try
            {
                var formData = await _context.CustomerQuestion.FindAsync(id);
                if (formData == null)
                {
                    return NotFound();
                }

                _context.CustomerQuestion.Remove(formData);
                await _context.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }
            return NoContent();
        }

        
        [HttpGet("FormIsChecked/{id}")]
        public async Task<ActionResult<CustomerQuestion>> IsChecked(int id)
        {
            try
            {
                var formData = await _context.CustomerQuestion.FindAsync(id);
                if (formData == null)
                {
                    return NotFound();
                }
                if (formData.IsChecked == false)
                {
                    formData.IsChecked = true;
                }
                else
                    formData.IsChecked = false;
                await _context.SaveChangesAsync();
                return Ok(formData);
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }
        }


        //[HttpGet("GetCustomerQuestionFormDataFromID/{id}")]
        //public async Task<ActionResult<CustomerQuestion>> GetFormDataFromId(int id)
        //{
        //    try
        //    {
        //        var formData = await _context.CustomerQuestion.FindAsync(id);
        //        if (formData == null)
        //        {
        //            return NotFound();
        //        }
        //        return Ok(formData);
        //    }
        //    catch (Exception e)
        //    {
        //        return Conflict(e.Message);
        //    }
        //}

        
        [HttpGet("GetAllQnAData")]

        public async Task<ActionResult<List<JsonElement>>> getAllQnAData()
        {
            List<JsonElement> jsonElements = new List<JsonElement>();
            await foreach (var data in authroingClient.GetQnasAsync(projectName))
            {
                JsonElement result = JsonDocument.Parse(data.ToStream()).RootElement;
                jsonElements.Add(result);
            }
            return Ok(jsonElements);

        }

        //Add QnA pair

        [HttpPost("AddQnAPair")]

        public async Task<ActionResult> AddQnAPair([FromBody] KnowledgeSourceDTO.AddQnADTO addQnADTO)
        {
            try
            {
                RequestContent updateQnasRequestContent = RequestContent.Create(
    new[] {
                    new {
                            op = "add",
                            value = new
                            {
                                questions = new[]
                                    {
                                        addQnADTO.Question
                                    },
                                answer = addQnADTO.Answer
                            }
                        }
    });
                Operation<Pageable<BinaryData>> updateQnasOperation = authroingClient.UpdateQnas(WaitUntil.Completed, projectName, updateQnasRequestContent);
            }
            catch (Exception e)
            {
                return Conflict(e.Message);

            }
            return Ok($"Har tilføjet spørgsmålet {addQnADTO.Question} med svaret {addQnADTO.Answer}");
        }


        //Add QnA pair with prompt OBS statuskode 200, men virker ikke?

        [HttpPost("AddQnAPairWithPrompt")]

        public async Task<ActionResult> AddQnAPairWithPrompt([FromBody] KnowledgeSourceDTO.AddQnADTOWithPrompt addQnADTOWithPrompt)
        {
            try
            {
                RequestContent updateQnasRequestContent = RequestContent.Create(
    new[] {
                    new {
                            op = "add",
                            value = new
                            {
                                questions = new[]
                                    {
                                        addQnADTOWithPrompt.Question
                                    },
                                answer = addQnADTOWithPrompt.Answer,

                                 dialog = new {
                                     isContextOnly = false,
                                       prompts = new[] {
                                                        new {
                                                            displayOrder = 0,
                                                            qnaId = 42,
                                                            displayText = addQnADTOWithPrompt.DisplayText,
                                                            }
                                                       },
                                              },
                            }
                        }
    });
                Operation<Pageable<BinaryData>> updateQnasOperation = authroingClient.UpdateQnas(WaitUntil.Completed, projectName, updateQnasRequestContent);
            }
            catch (Exception e)
            {
                return Conflict(e.Message);

            }
            return Ok($"Har tilføjet spørgsmålet {addQnADTOWithPrompt.Question} med svaret {addQnADTOWithPrompt.Answer} og prompt {addQnADTOWithPrompt.DisplayText}");
        }


        // Update QnA pair 
        [HttpPost("UpdateQnAPair")]
        public async Task<ActionResult> UpdateQnAPair([FromBody] KnowledgeSourceDTO.UpdateQnADTO updateQnADTO)
        {
            try
            {
                RequestContent updateQnasRequestContent = RequestContent.Create(
    new[] {
                    new {
                            op = "replace",
                            value = new
                            {
                                id = updateQnADTO.id,
                                questions = new[]
                                {
                                  updateQnADTO.Question
                                },
                                answer = updateQnADTO.Answer

                            }
                        }
    });
                Operation<Pageable<BinaryData>> updateQnasOperation = authroingClient.UpdateQnas(WaitUntil.Completed, projectName, updateQnasRequestContent);
            }
            catch (Exception e)
            {
                return Conflict(e.Message);

            }
            return Ok($"Har opdateret QnA {updateQnADTO.id} med spørgsmålet {updateQnADTO.Question} eller svaret {updateQnADTO.Answer}");
        }






        // Delete Qna pair

        [HttpDelete("DeleteQnAPair/{id}")]

        public async Task<ActionResult> DeleteQnAPair(int id)
        {
            try
            {
                RequestContent deleteQnasRequestContent = RequestContent.Create(
    new[] {
                    new {
                            op = "delete",
                            value = new
                            {
                                id = id,
                            }
                        }
    });
                Operation<Pageable<BinaryData>> deleteQnasOperation = authroingClient.UpdateQnas(WaitUntil.Completed, projectName, deleteQnasRequestContent);

            }
            catch (Exception e)
            {
                return Conflict(e.Message);

            }

            return Ok($"QnA med id {id} er blevet slettet fra KB");
        }


        //Adding a knowledge base source

        [HttpPost("AddKnowledgeBaseSource")]
        public async Task<ActionResult> AddKnowledgeBaseSource([FromBody] KnowledgeSourceDTO.AddKnowledgeBaseSourceDTO addKnowledgeBaseSourceDTO)
        {
            try
            {
                RequestContent updateSourcesRequestContent = RequestContent.Create(
new[] {
        new {
                op = "add",
                value = new
                {
                    displayName = addKnowledgeBaseSourceDTO.DisplayName,
                    source = addKnowledgeBaseSourceDTO.source,
                    sourceUri = addKnowledgeBaseSourceDTO.sourceUri,
                    sourceKind = addKnowledgeBaseSourceDTO.sourceKind,
                    refresh = false
                }
            }
});
                Operation<Pageable<BinaryData>> updateSourcesOperation = authroingClient.UpdateSources(WaitUntil.Completed, projectName, updateSourcesRequestContent);
            }
            catch (Exception e)
            {
                return Conflict(e.Message);

            }
            return Ok(null);

        }



        //Deleting a knowledge base source

        [HttpPost("DeleteKnowledgeBaseSource")]
        public async Task<ActionResult> DeleteKnowledgeBaseSource([FromBody] KnowledgeSourceDTO.AddKnowledgeBaseSourceDTO addKnowledgeBaseSourceDTO)
        {
            try
            {
                RequestContent updateSourcesRequestContent = RequestContent.Create(
new[] {
        new {
                op = "delete",
                value = new
                {
                    displayName = addKnowledgeBaseSourceDTO.DisplayName,
                    source = addKnowledgeBaseSourceDTO.source,
                    sourceUri = addKnowledgeBaseSourceDTO.sourceUri,
                    sourceKind = addKnowledgeBaseSourceDTO.sourceKind,
                    refresh = false
                }
            }
});
                Operation<Pageable<BinaryData>> updateSourcesOperation = authroingClient.UpdateSources(WaitUntil.Completed, projectName, updateSourcesRequestContent);
            }
            catch (Exception e)
            {
                return Conflict(e.Message);

            }
            return Ok(null);

        }

        //Updating a knowledge base source

        //Deploy project

        [HttpGet("DeployProject")]
        public async Task<ActionResult> DeployProject()
        {
            try
            {
                var operation =  authroingClient.DeployProject(WaitUntil.Completed, projectName, deploymentName);
                BinaryData data =  operation.WaitForCompletion();
                JsonElement result = JsonDocument.Parse(data.ToStream()).RootElement;
                string deploymentNameResult = result.GetProperty("deploymentName").ToString();
                string lastDeployedDateTime = result.GetProperty("lastDeployedDateTime").ToString();

                return Ok($"{deploymentNameResult} + lastDeployedDateTime: {lastDeployedDateTime}");
            }
            catch (Exception e)
            {
                return Conflict(e.Message);
            }

        }






    }
}

using Azure.AI.Language.QuestionAnswering;
using Azure;
using Azure.Core;
using Azure.AI.Language.QuestionAnswering.Authoring;


//download Azure.AI.Language.QuestionAnswering

Uri endpoint = new Uri("https://techbuddydk.cognitiveservices.azure.com/");
AzureKeyCredential credential = new AzureKeyCredential("");

QuestionAnsweringClient client = new QuestionAnsweringClient(endpoint, credential);


string projectName = "TechBuddy";
string deploymentName = "production";
//QuestionAnsweringProject project = new QuestionAnsweringProject(projectName, deploymentName);
//Response<AnswersResult> response = await client.GetAnswersAsync("Hvor meget strøm er et normalt elforbrug?", project);

//foreach (KnowledgeBaseAnswer _answer in response.Value.Answers)
//{
//    Console.WriteLine($"({_answer.Confidence:P2}) {_answer.Answer}");
//    Console.WriteLine($"Source: {_answer.Source}");
//    Console.WriteLine($"QnaId: {_answer.QnaId}");

//    foreach (KnowledgeBaseAnswerPrompt promt in _answer.Dialog.Prompts)
//    {
//        Console.WriteLine($"Promt ID:{promt.QnaId}");
//        Console.WriteLine($"Promt DisplayOrder:{promt.DisplayOrder}");
//        Console.WriteLine($"Promt DisplayText:{promt.DisplayText}");
//    }
//}



QuestionAnsweringAuthoringClient _client = new QuestionAnsweringAuthoringClient(endpoint, credential);

//string question = "test of update qna";
//string answer = "it works great";
//RequestContent updateQnasRequestContent = RequestContent.Create(
//    new[] {
//        new {
//                op = "add",
//                value = new
//                {
//                    questions = new[]
//                        {
//                            question
//                        },
//                    answer = answer
//                }
//            }
//    });

//Operation<AsyncPageable<BinaryData>> updateQnasOperation = await _client.UpdateQnasAsync(WaitUntil.Completed, projectName, updateQnasRequestContent);

var updateQnasOperation = _client.GetQnasAsync(projectName);
// QnAs can be retrieved as follows
//AsyncPageable<BinaryData> qnas = updateQnasOperation.Value;

//Console.WriteLine("Qnas: ");
//await foreach (var qna in qnas)
//{
//    Console.WriteLine(qna);
//}


Console.WriteLine("Qnas: ");
await foreach (var qna in updateQnasOperation)
{
    Console.WriteLine(qna);
}

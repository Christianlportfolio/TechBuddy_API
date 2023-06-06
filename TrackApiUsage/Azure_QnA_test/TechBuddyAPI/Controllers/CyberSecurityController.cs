using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace TechBuddyAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CyberSecurityController : ControllerBase
    {
        string filename = "RateLimiterLog.txt";
        List<string> logs = new List<string>();

        [HttpGet("ratelimitlog")]
        public async Task<ActionResult<List<string>>> GetRateLimitLog()
        {
            try
            {
                using (StreamReader reader = new StreamReader(filename))
                {

                    string line;

                    if (reader.ReadLine() == null)
                    {
                        logs.Add("no_logs");
                    }
                    else
                    {
                        while ((line = reader.ReadLine()) != null)
                        {
                            logs.Add(line);
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok(logs);
        }
    }
}

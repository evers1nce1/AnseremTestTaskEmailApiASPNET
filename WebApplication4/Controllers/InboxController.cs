using Microsoft.AspNetCore.Mvc;
using WebApplication4.Models;
using WebApplication4.Services;

namespace WebApplication4.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class InboxController : ControllerBase
    {
        private readonly IMessageProcessor _processor;

        public InboxController(IMessageProcessor processor)
        {
            _processor = processor;
        }

        [HttpPost("message")]
        public ActionResult<Message> GetNewMessage([FromBody] Message message)
        {
            return Ok(_processor.ProcessMessage(message)); 
        }
    }
}

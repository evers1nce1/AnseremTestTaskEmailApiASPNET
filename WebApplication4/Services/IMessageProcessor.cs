using WebApplication4.Models;
namespace WebApplication4.Services
{
    public interface IMessageProcessor
    {
        public Message ProcessMessage(Message message);
    }
}

using ChatService.Data;
using ChatService.Models;
using ChatService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System;

namespace ChatService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly RabbitMqProducerService _rabbitMqService;

        public MessagesController(AppDbContext context, RabbitMqProducerService rabbitMqService)
        {
            _context = context;
            _rabbitMqService = rabbitMqService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageRequest request)
        {
            try
            {
                // Сохраняем сообщение в БД
                var message = new Message
                {
                    SenderId = request.SenderId,
                    ReceiverId = request.ReceiverId,
                    Content = request.Content
                };

                _context.Messages.Add(message);
                await _context.SaveChangesAsync();

                // Отправляем в RabbitMQ для real-time рассылки
                var rabbitMessage = $"{request.SenderId}:{request.ReceiverId}:{request.Content}";
                _rabbitMqService.SendMessage(rabbitMessage);

                return Ok(new
                {
                    messageId = message.Id,
                    timestamp = message.Timestamp
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetMessageHistory(int userId, [FromQuery] int otherUserId = 0)
        {
            try
            {
                var query = _context.Messages
                    .Where(m => m.SenderId == userId || m.ReceiverId == userId);

                if (otherUserId > 0)
                {
                    query = query.Where(m =>
                        (m.SenderId == userId && m.ReceiverId == otherUserId) ||
                        (m.SenderId == otherUserId && m.ReceiverId == userId));
                }

                var messages = await query
                    .OrderBy(m => m.Timestamp)
                    .Take(50)
                    .ToListAsync();

                return Ok(messages);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpGet("health")]
        public IActionResult Health()
        {
            var dbStatus = _context.Database.CanConnect() ? "Connected" : "Disconnected";
            return Ok(new
            {
                status = "Chat service is running",
                database = dbStatus,
                timestamp = DateTime.UtcNow
            });
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            return Ok(new
            {
                message = "ChatService API is working!",
                endpoints = new[]
                {
                    "POST /api/messages/send",
                    "GET /api/messages/history/{userId}",
                    "GET /api/messages/health",
                    "GET /api/messages/test"
                }
            });
        }
    }

    public class SendMessageRequest
    {
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}
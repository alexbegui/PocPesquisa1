using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Gestao.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendMessage(string message)
        {
            try
            {
                if (Clients?.All == null)
                {
                    _logger.LogError("Clients or Clients.All is null");
                    return;
                }

                await Clients.All.SendAsync("ReceiveMessage", message, cancellationToken: Context.ConnectionAborted);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error sending message: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task SendToGroup(string groupName, string message)
        {
            try
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
                await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", message, cancellationToken: Context.ConnectionAborted);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error in SendToGroup: {ex.Message}");
                throw;
            }
        }
    }
}
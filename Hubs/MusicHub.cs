using Microsoft.AspNetCore.SignalR;

namespace MusicPortalLaLaFa.Hubs
{
    public class MusicHub : Hub
    {
        public async Task SendNotification(string message)
        {
            // Отправляем сообщение всем подключенным клиентам
            await Clients.All.SendAsync("ReceiveNotification", message);
        }
    }
}

using Microsoft.AspNetCore.SignalR;

namespace TransferProject.Hubs;

public class ExampleTypeSafeHub : Hub<IExampleTypeSafeHub>
{
    private static int ConnectedClientCount = 0;

    public async Task BroadCastMessageToAllClient(string message)
    {
        await Clients.All.ReceiveMessageAllClients(message);
    }


    public async Task BroadCastMessageToCallerClient(string message) //todo :ui tarafindan tetiklenecek ve bu method da gidip clienti tetikleyecek (herhangi bir sey olabilir)
    {
        await Clients.Caller.ReceiveMessageForCallerClient(message+" direkt serverdan geldi");
    }

    public async Task BroadCastMessageToOtherClients(string message)
    {
        await Clients.Others.ReceiveMessageForOtherClients(message+ " kendisi disinda herkese gitti");
    }

    public async Task BroadCastMessageToSpecificClient(string connectionId, string message)
    {
        await Clients.Client(connectionId).ReceiveMessageForSpecificClient(message);
    }

    public async IAsyncEnumerable<string> BroadCastFromHubToClient()
    {
        foreach (var item in Enumerable.Range(1,20).ToList())
        {
            await Task.Delay(1000);
            yield return $"{item}. data";
        }
    }

    public override async Task OnConnectedAsync()
    {
        ConnectedClientCount++;
        await Clients.All.ReceiveClientCount(ConnectedClientCount);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        ConnectedClientCount--;
        await Clients.All.ReceiveClientCount(ConnectedClientCount);
        await base.OnDisconnectedAsync(exception);
    }
}
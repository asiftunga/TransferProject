namespace TransferProject.Hubs;

public interface IExampleTypeSafeHub
{
    Task ReceiveMessageAllClients(string message);

    Task ReceiveClientCount(int count);

    Task ReceiveMessageForCallerClient(string message);

    Task ReceiveMessageForOtherClients(string message);

    Task ReceiveMessageForSpecificClient(string message);
}
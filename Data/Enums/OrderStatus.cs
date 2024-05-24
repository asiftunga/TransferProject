namespace MiniApp1Api.Data.Enums;

public enum OrderStatus
{
    Unknown = 0,
    WaitingForMoneyTransfer = 1,
    MonayTransferSucceded = 2,
    OrderCompleted = 3,
    OrderCanceled = 4,
}
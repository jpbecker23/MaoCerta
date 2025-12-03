namespace MaoCerta.Domain.Enums
{
    /// <summary>
    /// Represents the status of a service request
    /// </summary>
    public enum ServiceStatus
    {
        Pending = 0,
        Accepted = 1,
        Rejected = 2,
        InProgress = 3,
        Completed = 4,
        Cancelled = 5
    }
}

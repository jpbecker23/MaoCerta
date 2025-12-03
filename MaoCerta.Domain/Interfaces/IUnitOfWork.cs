using MaoCerta.Domain.Entities;

namespace MaoCerta.Domain.Interfaces
{
    /// <summary>
    /// Unit of Work pattern interface for managing transactions
    /// Ensures data consistency across multiple repository operations
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Client> Clients { get; }
        IRepository<Professional> Professionals { get; }
        IRepository<Category> Categories { get; }
        IRepository<ServiceRequest> ServiceRequests { get; }
        IRepository<Review> Reviews { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}

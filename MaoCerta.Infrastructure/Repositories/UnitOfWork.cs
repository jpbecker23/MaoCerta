using Microsoft.EntityFrameworkCore.Storage;
using MaoCerta.Domain.Interfaces;
using MaoCerta.Infrastructure.Data;

namespace MaoCerta.Infrastructure.Repositories
{
    /// <summary>
    /// Unit of Work implementation for managing transactions
    /// Ensures data consistency across multiple repository operations
    /// </summary>
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private IDbContextTransaction? _transaction;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            Clients = new Repository<Domain.Entities.Client>(_context);
            Professionals = new Repository<Domain.Entities.Professional>(_context);
            Categories = new Repository<Domain.Entities.Category>(_context);
            ServiceRequests = new Repository<Domain.Entities.ServiceRequest>(_context);
            Reviews = new Repository<Domain.Entities.Review>(_context);
        }

        public IRepository<Domain.Entities.Client> Clients { get; }
        public IRepository<Domain.Entities.Professional> Professionals { get; }
        public IRepository<Domain.Entities.Category> Categories { get; }
        public IRepository<Domain.Entities.ServiceRequest> ServiceRequests { get; }
        public IRepository<Domain.Entities.Review> Reviews { get; }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.CommitAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public async Task RollbackTransactionAsync()
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync();
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}

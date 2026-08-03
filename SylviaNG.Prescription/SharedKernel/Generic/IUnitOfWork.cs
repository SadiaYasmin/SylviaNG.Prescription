using SylviaNG.Prescription.Infrastructure.Data;

namespace SylviaNG.Prescription.SharedKernel.Generic
{
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync();

        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();

        ApplicationDBContext Context { get; }
    }
}

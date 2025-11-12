namespace studyez_backend.Core.Interfaces.Repositories
{
    public interface IUnitOfWork
    {
        /// <summary>
        /// Saves all changes made in this context to the database asynchronously.
        /// </summary>
        /// <param name="ct"></param>
        /// <returns></returns>
        Task<int> SaveChangesAsync(CancellationToken ct = default);
    }
}

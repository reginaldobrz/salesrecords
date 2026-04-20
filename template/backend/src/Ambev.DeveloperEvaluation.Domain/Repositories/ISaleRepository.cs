using Ambev.DeveloperEvaluation.Domain.Entities;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

/// <summary>
/// Repository interface for Sale aggregate operations.
/// </summary>
public interface ISaleRepository
{
    /// <summary>Creates a new sale record and returns the persisted entity.</summary>
    Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a sale by its unique identifier, including items.</summary>
    Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a sale by its sequential sale number, including items.</summary>
    Task<Sale?> GetByNumberAsync(int saleNumber, CancellationToken cancellationToken = default);

    /// <summary>Persists changes to an existing sale.</summary>
    Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a sale record by its identifier.
    /// </summary>
    /// <returns>True if the record was found and deleted; false otherwise.</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns a paginated list of all sales (including items) and the total count.
    /// </summary>
    /// <param name="page">1-based page number.</param>
    /// <param name="size">Number of records per page.</param>
    Task<(IEnumerable<Sale> Items, int TotalCount)> GetAllAsync(
        int page,
        int size,
        CancellationToken cancellationToken = default);
}

using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

/// <summary>
/// EF Core implementation of <see cref="ISaleRepository"/>.
/// </summary>
public class SaleRepository : ISaleRepository
{
    private readonly DefaultContext _context;

    public SaleRepository(DefaultContext context)
    {
        _context = context;
    }

    public async Task<Sale> CreateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        await _context.Sales.AddAsync(sale, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<Sale?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sale?> GetByNumberAsync(int saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.Items)
            .FirstOrDefaultAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<Sale> UpdateAsync(Sale sale, CancellationToken cancellationToken = default)
    {
        // Detach any currently tracked Sale or SaleItem entities that conflict
        var trackedSale = _context.ChangeTracker.Entries<Sale>()
            .FirstOrDefault(e => e.Entity.Id == sale.Id);
        if (trackedSale != null)
            trackedSale.State = EntityState.Detached;

        foreach (var itemEntry in _context.ChangeTracker.Entries<SaleItem>()
            .Where(e => e.Entity.SaleId == sale.Id).ToList())
        {
            itemEntry.State = EntityState.Detached;
        }

        // Load existing item IDs from store to determine which are new vs existing
        var existingItemIds = await _context.SaleItems
            .Where(si => si.SaleId == sale.Id)
            .Select(si => si.Id)
            .ToListAsync(cancellationToken);

        var currentItemIds = sale.Items.Select(i => i.Id).ToHashSet();

        // Remove stale items
        var staleIds = existingItemIds.Where(id => !currentItemIds.Contains(id)).ToList();
        if (staleIds.Count > 0)
        {
            var staleItems = await _context.SaleItems
                .Where(si => staleIds.Contains(si.Id))
                .ToListAsync(cancellationToken);
            _context.SaleItems.RemoveRange(staleItems);
        }

        // Mark sale header as modified
        _context.Entry(sale).State = EntityState.Modified;

        // For each item: add if new, modify if existing
        foreach (var item in sale.Items)
        {
            if (existingItemIds.Contains(item.Id))
                _context.Entry(item).State = EntityState.Modified;
            else
                _context.Entry(item).State = EntityState.Added;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return sale;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);
        if (sale is null)
            return false;

        _context.Sales.Remove(sale);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<(IEnumerable<Sale> Items, int TotalCount)> GetAllAsync(
        int page,
        int size,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include(s => s.Items)
            .OrderByDescending(s => s.SaleDate)
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}

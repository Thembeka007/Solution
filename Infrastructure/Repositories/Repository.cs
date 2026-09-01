using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using SharedKernel.Interfaces;

namespace Infrastructure.Repositories;

public class Repository<T>
    : IRepository<T>
    where T : class
{
    protected readonly SportsBettingDbContext
        _context;

    protected readonly DbSet<T> _dbSet;

    public Repository(
        SportsBettingDbContext context)
    {
        _context = context;

        _dbSet = context.Set<T>();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        return await _dbSet.FindAsync(id);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);

        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        _dbSet.Update(entity);

        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await GetByIdAsync(id);

        if (entity == null)
            return;

        _dbSet.Remove(entity);

        await _context.SaveChangesAsync();
    }
}
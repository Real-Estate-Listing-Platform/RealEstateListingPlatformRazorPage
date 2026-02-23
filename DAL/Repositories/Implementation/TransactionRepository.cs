using DAL.Models;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementation;

public class TransactionRepository : ITransactionRepository
{
    private readonly RealEstateListingPlatformContext _context;

    public TransactionRepository(RealEstateListingPlatformContext context)
    {
        _context = context;
    }

    public async Task<List<Transaction>> GetAllTransactionsAsync()
    {
        return await _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Package)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetUserTransactionsAsync(Guid userId)
    {
        return await _context.Transactions
            .Include(t => t.Package)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction?> GetTransactionByIdAsync(Guid id)
    {
        return await _context.Transactions.FindAsync(id);
    }

    public async Task<Transaction?> GetTransactionWithDetailsAsync(Guid id)
    {
        return await _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Package)
            .Include(t => t.UserPackages)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Transaction> CreateTransactionAsync(Transaction transaction)
    {
        transaction.Id = Guid.NewGuid();
        transaction.CreatedAt = DateTime.UtcNow;

        await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task UpdateTransactionAsync(Transaction transaction)
    {
        _context.Transactions.Update(transaction);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Transaction>> GetPendingTransactionsAsync()
    {
        return await _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Package)
            .Where(t => t.Status == "Pending")
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByStatusAsync(string status)
    {
        return await _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Package)
            .Where(t => t.Status == status)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Package)
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<Transaction?> GetTransactionByPayOSOrderCodeAsync(long orderCode)
    {
        return await _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Package)
            .FirstOrDefaultAsync(t => t.PayOSOrderCode == orderCode);
    }
}

using DAL.Models;

namespace DAL.Repositories;

public interface ITransactionRepository
{
    Task<List<Transaction>> GetAllTransactionsAsync();
    Task<List<Transaction>> GetUserTransactionsAsync(Guid userId);
    Task<Transaction?> GetTransactionByIdAsync(Guid id);
    Task<Transaction?> GetTransactionWithDetailsAsync(Guid id);
    Task<Transaction> CreateTransactionAsync(Transaction transaction);
    Task UpdateTransactionAsync(Transaction transaction);
    Task<List<Transaction>> GetPendingTransactionsAsync();
    Task<List<Transaction>> GetTransactionsByStatusAsync(string status);
    Task<List<Transaction>> GetTransactionsByDateRangeAsync(DateTime startDate, DateTime endDate);
    Task<Transaction?> GetTransactionByPayOSOrderCodeAsync(long orderCode);
}

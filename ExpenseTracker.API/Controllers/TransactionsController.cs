    using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;
using ExpenseTracker.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions(
        
            [FromQuery] string? shortBy,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
         {
            int currentUserId = GetCurrentUserId();

                

            var query = _context.Transactions
                .Include(t => t.Category)
                .Where(t => t.UserId == currentUserId)
                .AsQueryable();

            if (shortBy == "dateDesc")
                query = query.OrderByDescending(t => t.Date);
            else if (shortBy == "amountDesc")            
                query = query.OrderByDescending(t => t.Amount);          
            else
                query = query.OrderByDescending(t => t.Date);

            var totalRecords = await query.CountAsync();


            var transactions = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(t => new TransactionDto
                {
                    Id = t.Id,
                    Amount= t.Amount,
                    Description = t.Description,
                    Date = t.Date,
                    CategoryName = t.Category != null ? t.Category.Name : "Kategori Yok",
                    Type = t.Type,

                }) 
                .ToListAsync();
            return Ok(new
            {
                TotalRecords = totalRecords,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                Data = transactions
            });
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction(CreateTransactionDto transactionDto)
        {
            var transaction = new Transaction
            {
                Amount = transactionDto.Amount,
                Description = transactionDto.Description,
                Type = (TransactionType)transactionDto.Type,
                CategoryId = transactionDto.CategoryId,
                Date = DateTime.Now,

                UserId = GetCurrentUserId()
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return Ok(transaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, Transaction transaction)
        {
             int currentUserId = GetCurrentUserId();
             var existingTransaction = _context.Transactions.FirstOrDefault(t => t.Id == id);    

             if (existingTransaction == null)
             {
                return NotFound("Güncellenmek istenen harcama bulunamadı!");
             }

             if (existingTransaction.UserId != currentUserId)
             {
                return StatusCode(403, "Güvenlik İhlali: Bu harcamayı güncelleme yetkiniz yok!");
             }

             existingTransaction.Amount = transaction.Amount;
             existingTransaction.Description = transaction.Description;  
             existingTransaction.Type = (TransactionType)transaction.Type;
             existingTransaction.CategoryId = transaction.CategoryId;
             existingTransaction.Date = DateTime.Now;

            _context.Transactions.Update(existingTransaction);
            await _context.SaveChangesAsync();

            return Ok("Harcama başarıyla güncellendi!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            int currentUserId = GetCurrentUserId();

            var transaction = _context.Transactions.FirstOrDefault(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound("Silinmek istenen harcama bulunamadı!");
            }

            if (transaction.UserId != currentUserId)
            {
                return StatusCode(403, "Güvenlik İhlali: Bu harcamayı silme yetkiniz yok!");
            }

            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();

            return Ok("Harcama başarıyla silindi!");
        }

        [HttpGet("total")]
        [Authorize]
        public async Task<IActionResult> GetTotalTransactions()
        {
            var UserIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(UserIdString))
            {
                return BadRequest("Kullanıcı kimliği bulunamadı.");
            }
            var userId = int.Parse(UserIdString);

            var total = await _context.Transactions
                .Where(t => t.UserId == userId)
                .SumAsync(t => t.Amount);   

            return Ok(new { TotalAmount = total });
        }   
        

            [HttpGet("category/{categoryId}")]
            [Authorize]
        public async Task<IActionResult> GetTransactionsByCategory(int categoryId)
        {
         var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                return BadRequest("Kullanıcı kimliği bulunamadı.");
            }

            var userId = int.Parse(userIdString);

            var transactions = await _context.Transactions
                .Where(t => t.UserId == userId && t.CategoryId == categoryId)
                .Include(t => t.Category)
                .ToListAsync();

            return Ok(new { transactions });
        }



        private int GetCurrentUserId()
        {
            var UserIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(UserIdString!);    
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            int userId = GetCurrentUserId();

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound("Kullanıcı bulunamadı!");
            }

            var totalIncome = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Income)
                .SumAsync(t => t.Amount);

            var totalExpenses = await _context.Transactions
                .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
                .SumAsync(t => t.Amount);

           var summary = new DashboardSummaryDto
           {
               TotalIncome = totalIncome,
               TotalExpenses = totalExpenses,
               CurrentBalance = totalIncome - totalExpenses,
               MonthlyBudget = user.MonthlyBudget,
               BudgetUsagePercentage = 0,
               BudgetWarningMessage = "Bütçe belirlenmedi."
           };
            
            if (user.MonthlyBudget.HasValue && user.MonthlyBudget.Value > 0)
            {
                summary.BudgetUsagePercentage = Math.Round((totalExpenses / user.MonthlyBudget.Value) * 100, 2); 

                if (summary.BudgetUsagePercentage >= 100)
                {
                    summary.BudgetWarningMessage = "🚨 Kritik Uyarı: Aylık bütçenizi tamamen aştınız!";
                }
                else if (summary.BudgetUsagePercentage >= 80)
                {
                    summary.BudgetWarningMessage = "⚠️ Dikkat: Aylık bütçenizin %80'inden fazlasını kullandınız.";
                }
                else
                {
                    summary.BudgetWarningMessage = "✅ Harika! Bütçe hedefleriniz doğrultusunda ilerliyorsunuz.";
                }
            }

            return(Ok(summary));
        }
    }
    
}


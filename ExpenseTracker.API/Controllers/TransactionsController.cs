using ExpenseTracker.API.Data;
using ExpenseTracker.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography.X509Certificates;

namespace ExpenseTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context; 

        public TransactionsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetTransactions()
        {
            var transactions = await _context.Transactions
                                                          .Include(t => t.Category)
                                                          .ToListAsync();
            return Ok(transactions);
        }

        [HttpPost]
        public async Task<IActionResult> AddTransaction(Transaction transaction)
        {
            if (transaction.Date == default)
            {
                transaction.Date = DateTime.UtcNow;
            }

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();
            return Ok(transaction);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTransaction(int id, Transaction transaction)
        {
            if (id != transaction.Id)
            {
                return BadRequest("Güncellenmek istenen harcama ID'si uyuşmuyor!");
            }

            _context.Transactions.Update(transaction);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch(DbUpdateConcurrencyException)
            {
                if (!_context.Transactions.Any(t => t.Id == id))
                {
                    return NotFound("Güncellenmek istenen harcama bulunamadı!");
                }
                throw;
            }

            return Ok("Harcama başarıyla güncellendi!");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
                
            if (transaction == null)
            {
                return NotFound("Silinmek istenen harcama bulunamadı!");
            }
            _context.Transactions.Remove(transaction);
            await _context.SaveChangesAsync();
            return Ok("Harcama başarıyla silindi!");
        }
       
    }
}

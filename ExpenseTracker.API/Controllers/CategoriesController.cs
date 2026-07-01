using ExpenseTracker.API.Data;
using ExpenseTracker.API.DTOs;
using ExpenseTracker.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.API.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return Ok(categories);
        }

        [HttpPost]
        public async Task<IActionResult> AddCategory([FromBody] CreateCategoryDto categoryDto)
        {
            if (string.IsNullOrWhiteSpace(categoryDto.CategoryName))
            {
                return BadRequest("Kategori adı boş olamaz.");
            }

            var category = new Category
            {
                Name = categoryDto.CategoryName
                // Eğer kategoriler kullanıcıya özel tutuluyorsa buraya UserId atamasını da eklemeliyiz.
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            // Frontend'in yeni kategoriyi listeye ekleyebilmesi için ID'siyle birlikte geri dönüyoruz
            return Ok(category);
        }
    }
}

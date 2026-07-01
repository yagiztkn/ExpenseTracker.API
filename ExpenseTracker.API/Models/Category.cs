 using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExpenseTracker.API.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        [StringLength(50)]
        public string? Name { get; set; }
        
        [JsonIgnore]
        public List<Transaction>? Transactions { get; set; }

    }
}

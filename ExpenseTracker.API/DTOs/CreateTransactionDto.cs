using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.DTOs
{
    public class CreateTransactionDto
    {
        [Required(ErrorMessage = "Harcama Tutarı Girmek Zorunludur!")]
        [Range(0.01, 1000000, ErrorMessage = "Harcama Tutarı 0'dan büyük olmalıdır!")]
        public decimal Amount { get; set; } 


        [MaxLength(100, ErrorMessage = "Harcama Açıklaması 100 karakterden fazla olamaz!")] 
        public string? Description { get; set; }


        [Required(ErrorMessage = "Harcama Türü Seçmek Zorunludur!")]
        public int Type { get; set; }


        [Required(ErrorMessage = "Harcama Kategorisi Seçmek Zorunludur!")]
        public int CategoryId { get; set; }
    }
}

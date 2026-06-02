using System.ComponentModel.DataAnnotations;

namespace Project.Models
{
    public class TransactionType
    {
        [Key] 
        public int TransactionTypeId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(10)]
        public string Direction { get; set; } = string.Empty;

        public int BalanceEffect { get; set; }

        public bool IsActive { get; set; } = true;

        public ICollection<AccountTransaction> AccountTransactions { get; set; } = new List<AccountTransaction>();
    }
}
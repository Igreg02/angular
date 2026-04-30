using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RubricaSemplice.Api.Models
{
    [Table("Interests")]
    public class Interest
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Nome { get; set; } = string.Empty;

        // Con identity l'id utente è string
        [Required]
        public string UserId { get; set; } = string.Empty;

        // collegamento all'utente
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
    }
}
using System.ComponentModel.DataAnnotations;

namespace ForFIll.Models
{
    public class Product
    {

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Please enter a valid price")]
        public decimal Price { get; set; }

        [Required]
        public string Category { get; set; }

    }
}

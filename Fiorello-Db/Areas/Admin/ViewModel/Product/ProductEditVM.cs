using System.ComponentModel.DataAnnotations;

namespace Fiorello_Db.Areas.Admin.ViewModel.Product
{
    public class ProductEditVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "This input can't be empty")]
        [StringLength(20, ErrorMessage = "Length must be max 20")]
        public string Name { get; set; }

        public string Description { get; set; }
        public float Price { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public List<ProductImageVM> Images { get; set; }
        public List<IFormFile> NewImages { get; set; }

    }
}

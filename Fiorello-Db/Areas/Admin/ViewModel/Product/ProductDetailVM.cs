namespace Fiorello_Db.Areas.Admin.ViewModel.Product
{
    public class ProductDetailVM
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float Price { get; set; }
        public string Category { get; set; }
        public List<ProductImageVM> Images { get; set; }


    }
}

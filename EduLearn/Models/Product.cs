namespace EduLearn.Models
{

    public class Product
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Version { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }
        public List<string> Features { get; set; }
        public string MainImage { get; set; }
        public List<RelatedProductViewModel> RelatedProducts { get; set; }
    }
    public class RelatedProductViewModel
    {
		public int Id { get; set; }
		public string Title { get; set; }
        public string Version { get; set; }
        public string Price { get; set; }
        public string Description { get; set; }
        public List<string> Features { get; set; }
        public string MainImage { get; set; }
    }
}

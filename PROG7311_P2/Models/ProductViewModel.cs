using System.ComponentModel.DataAnnotations;

namespace PROG7311_P2.Models
{
    public class ProductViewModel
    {
        public int? Id { get; set; }
        public string ProductName { get; set; }

        //display quantity as an integer using data annotations
        [DisplayFormat(DataFormatString = "{0:0.##}")]
        public decimal Quantity { get; set; }

        public decimal Price { get; set; }


        [DisplayFormat(DataFormatString = "{0:dd-MM-yyyy}")]
        public DateTime DateSupplied { get; set; }

        public string Email { get; set; }

        public string TypeName { get; set; }
    }
}

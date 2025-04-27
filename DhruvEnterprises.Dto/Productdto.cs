using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace DhruvEnterprises.Dto
{
    public class Productdto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ImageUrl { get; set; }
        public int AlreadyInCard { get; set; }
        public List<string> CategoryIds { get; set; }
        public string CategoryName { get; set; }
        public string FullDescription { get; set; }
        public bool ShowOnHomepage { get; set; }
        public int StockQuantity { get; set; }
        public bool DisplayStockAvailability { get; set; }
        public int OrderMinimumQuantity { get; set; }
        public int OrderMaximumQuantity { get; set; }
        public int cartquantity { get; set; }
        [Display(Name= "Selling Price")]
        public decimal Price { get; set; }

        [Display(Name = "Voucher Cost")]
        public decimal ProductCost { get; set; }
        public int DisplayOrder { get; set; }
        public bool Published { get; set; }
        public bool Deleted { get; set; }
        // public string ImageURL { get; set; }
    }
    public class ProductImagedto
    {
       
        public int Id { get; set; }
        public Nullable<int> PId { get; set; }
        public string ImageUrl { get; set; }
        public HttpPostedFileBase FileAttach { get; set; }
        public Nullable<int> DisplayOrder { get; set; }
     
     
    }
    public class Itemdto
    {
        public int Id { get; set; }
        public Product Product { get; set; }
     
        public Productdto Products { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string ImageUrl { get; set; }
        public string ProductName { get; set; }
        public int CheakStatus { get; set; }
   
    }
    public class Cartdto
    {

        public int Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> ProductId { get; set; }
        public string ProductName { get; set; }
        public Nullable<System.DateTime> Addeddate { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; }
        public int ProductUnitMapId { get; set; }
    }
    public class ProductStockdto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PId { get; set; }
        public int OrderId { get; set; }
        public int Quantity { get; set; }
        public bool IsPullOut { get; set; }
        public string Remark { get; set; }
 
    }
}

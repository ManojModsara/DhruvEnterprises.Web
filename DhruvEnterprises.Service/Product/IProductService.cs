using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DhruvEnterprises.Service
{
    public interface IProductService
    {
        KeyValuePair<int, List<Product>> GetProducts(DataTableServerSide searchModel, int VendorId = 0);

        KeyValuePair<int, List<ProductImage>> GetProductImages(DataTableServerSide searchModel, int Id);
        Product GetProductByIdAsync(int productId);
        Cart GetCartByProductId(int id);
        Order Saves(Order adminRole);
        OrderDetail SaveOrderDetails(OrderDetail adminRole);
        ProductImage GetProductByIdAsyncimg(int productId);
 
        ProductImage GetProductImageByIdAsync(int imageId);
        ICollection<Product> GetProductUserList(int? Id);
        ICollection<Product> GetProductList();
        Cart GetCartById(int id);
        Cart delete(Cart carts);
        List<Cart> GetCartList(int UserId);
        Cart GetCartUserProduct(int UserId, int ProductId);
      
        Cart Saves(Cart adminRole);
        ICollection<Product> GetRelaitedProductList(int? CId, int? PCId);
        ICollection<Product> GetProductListSearch(DataTableServerSide searchModel, int? Id, string BrandId);
        Product Save(Product product);

        ProductImage GetImageById(int ProductId, int ColorId);
        ProductImage Save(ProductImage productImage);
        List<ProductImage> GetProductImageListByColorId(int Id, int Colorid);
        List<ProductImage> GetProductImageListByUnitId(int Id, int AttributeId);
        List<ProductImage> GetProductImageListById(int Id);

        ProductImage GetProductImageById(int Id);
        List<Product> GetProductSearch(int pageSize, string search = null, int page = 0, int? category = null);
        List<Product> GetProductByBrand(int pageSize, int? Id = null, int page = 0, int? category = null);
        List<Product> GetProductselections(int pageSize, int? Id = null, int page = 0, int? category = null);

    }
}

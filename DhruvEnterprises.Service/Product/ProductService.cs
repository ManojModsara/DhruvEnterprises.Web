using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Repo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace DhruvEnterprises.Service
{
    public class ProductService : IProductService 
    {
        #region "Fields"
        private IRepository<Product> repoProduct;
        private IRepository<ProductImage> repoProductImage;
        private IRepository<Cart> repoCart;
        private IRepository<Order> repoOrder;
        private IRepository<OrderDetail> repoOrderDetail;

        #endregion

        #region "Cosntructor"
        public ProductService(
            IRepository<Product> _repoProduct,
            IRepository<ProductImage> _repoProductImage, 
            IRepository<Order> _repoOrder,
            IRepository<OrderDetail> _repoOrderDetail,
            IRepository<Cart> _repoCart


           )
        {
            this.repoProduct = _repoProduct;
            this.repoProductImage = _repoProductImage;
            this.repoCart = _repoCart;
            this.repoOrder = _repoOrder;
            this.repoOrderDetail = _repoOrderDetail;

        }

        #endregion

        #region Action

        public KeyValuePair<int, List<Product>> GetProducts(DataTableServerSide searchModel,int VendorId=0)
        {
            var predicate = Core.CustomPredicate.BuildPredicate<Product>(searchModel, new Type[] { typeof(Product) });

            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Product> results = repoProduct
                .Query()
               .Filter(predicate)
                //.Filter(predicate.And(a => a.IsActive && a.RoleName != Enum.GetName(typeof(RoleType),1)))
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(Product) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<Product>> resultResponse = new KeyValuePair<int, List<Product>>(totalCount, results);

            return resultResponse;
        }


        public OrderDetail SaveOrderDetails(OrderDetail adminRole)
        {
            if (adminRole.Id == 0)
            {
                repoOrderDetail.Insert(adminRole);
            }
            else
            {
                repoOrderDetail.Update(adminRole);
            }
            return adminRole;
        }
        public List<Cart> GetCartList(int UserId)
        {
            return repoCart.Query().Include(x => x.Product).Filter(x => x.UserId == UserId).Get().ToList();
        }
        public Product GetProductByIdAsync(int productId)
        {
            return repoProduct.FindById(productId);
        }
        public Cart GetCartUserProduct(int UserId, int ProductId)
        {
            return repoCart.Query().Filter(x => x.UserId == UserId && x.ProductId == ProductId).Get().FirstOrDefault();
        }
        public Cart Saves(Cart adminRole)
        {
            if (adminRole.Id == 0)
            {
                adminRole.Addeddate = DateTime.Now;
                repoCart.Insert(adminRole);
            }
            else
            {
                repoCart.Update(adminRole);
            }
            return adminRole;
        }
        public Order Saves(Order adminRole)
        {
     
            if (adminRole.Id == 0)
            {
                repoOrder.Insert(adminRole);
            }
            else
            {
                repoOrder.Update(adminRole);
            }
            return adminRole;
        }
        public KeyValuePair<int, List<ProductImage>> GetProductImages(DataTableServerSide searchModel,int ProductId)
        {
            var predicate = Core.CustomPredicate.BuildPredicate<ProductImage>(searchModel, new Type[] { typeof(ProductImage) });
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);
            predicate = ProductId == 0 ? predicate : predicate.And(x => x.PId == ProductId);
            List<ProductImage> results = repoProductImage
                .Query()
               .Filter(predicate)
                //.Filter(predicate.And(a => a.IsActive && a.RoleName != Enum.GetName(typeof(RoleType),1)))
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(ProductImage) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();

            KeyValuePair<int, List<ProductImage>> resultResponse = new KeyValuePair<int, List<ProductImage>>(totalCount, results);

            return resultResponse;
        }
        public Cart GetCartByProductId(int id)
        {
            return repoCart.Query().Get().FirstOrDefault(x => x.ProductId == id);
        }
        public Cart GetCartById(int id)
        {
            return repoCart.FindById(id);
        }
        public Cart delete(Cart carts)
        {
            repoCart.Delete(carts);
            return carts;
        }
        public ProductImage GetProductByIdAsyncimg(int productId)
        {
            return repoProductImage.FindById(productId);
        }
   
        public ProductImage GetProductImageByIdAsync(int imageId)
        {
            return repoProductImage.FindById(imageId);
        }
        public List<Product> GetProductSearch(int pageSize, string search = null, int page = 0, int? category = null )
        {
            List<Product> catalogPager = new List<Product>();
            using (var onlineShopContext = new DhruvEnterprisesDBEntities())
            {
                var products = onlineShopContext.Products.Where(x => (category == null || category == 0 ) && (search == null || x.Name.Contains(search)) && x.ShowOnHomepage == true).OrderBy(x => x.Id).ToList();
                var pageProducts = products.Skip(pageSize * page).Take(pageSize);
                catalogPager = products;// AutoMapper.Mapper.Map<IEnumerable<Product>>(pageProducts);
            }
            return catalogPager;
        }
        public List<Product> GetProductByBrand(int pageSize, int? Id = null, int page = 0, int? category = null)
        {
            List<Product> catalogPager = new List<Product>();
            using (var onlineShopContext = new DhruvEnterprisesDBEntities())
            {
                var products = onlineShopContext.Products.Where(x => (category == null || category == 0 ) && (Id == null) && x.ShowOnHomepage == true).OrderBy(x => x.Id).ToList();
                var pageProducts = products.Skip(pageSize * page).Take(pageSize);
                catalogPager = products;// AutoMapper.Mapper.Map<IEnumerable<Product>>(pageProducts);
            }
            return catalogPager;
        }
        public List<Product> GetProductselections(int pageSize, int? Id = null, int page = 0, int? category = null)
        {
            List<Product> catalogPager = new List<Product>();
            using (var onlineShopContext = new DhruvEnterprisesDBEntities())
            {
                var products = onlineShopContext.Products.Where(x => (category == null || category == 0 ) && (Id == null) && x.ShowOnHomepage == true).OrderBy(x => x.Id).ToList();
                var pageProducts = products.Skip(pageSize * page).Take(pageSize);
                catalogPager = products;// AutoMapper.Mapper.Map<IEnumerable<Product>>(pageProducts);
            }
            return catalogPager;
        }
        public ProductImage GetProductImageById(int Id)
        {
            return repoProductImage.Query().Get().Where(x => x.PId == Id && x.ImageUrl != null).LastOrDefault();

        }
     
        public ICollection<Product> GetProductUserList(int? Id)
        {
            return repoProduct.Query().Get().Where(x => (x.Published == true)).ToList();
        }

        public ProductImage GetImageById(int ProductId, int ColorId)
        {
            return repoProductImage.Query().Get().Where(x => x.PId == ProductId).FirstOrDefault();
        }
        public List<ProductImage> GetProductImageListById(int Id)
        {
            return repoProductImage.Query().Get().Where(x => x.PId == Id).OrderBy(x=>x.DisplayOrder).ToList();
        }
        public List<ProductImage> GetProductImageListByColorId(int Id, int Colorid)
        {
            return repoProductImage.Query().Get().Where(x => x.PId == Id ).Take(5).OrderBy(x => x.DisplayOrder).ToList();
        }

        public List<ProductImage> GetProductImageListByUnitId(int Id, int AttributeId)
        {
            return repoProductImage.Query().Get().Where(x => x.PId == Id ).Take(5).OrderBy(x => x.DisplayOrder).ToList();
        }
        public ICollection<Product> GetProductList()
        {

            return repoProduct.Query().Get().Where(x => x.Published == true && x.ShowOnHomepage == true).ToList();
        }
        public ICollection<Product> GetRelaitedProductList(int? CId , int? PCId)
        {
            return repoProduct.Query().Get().Where(x =>  x.ShowOnHomepage == true).ToList();
            //    return repoProduct.Query().Get().Where(x=>x.Product_Category_Mapping..Where(y=>y.CategoryId==Id)).ToList();
        }
        public ICollection<Product> GetProductListSearch(DataTableServerSide searchModel,int? CategoryId,string BrandId)
        {
            FilterData filterData = new FilterData();
            searchModel.filterdata = filterData;
            var predicate = Core.CustomPredicate.BuildPredicate<Product>(searchModel, new Type[] { typeof(Product) });
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Product> results = repoProduct
                .Query().Filter(predicate)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(Product) }))
                .GetPage(page, searchModel.length, out totalCount).
               ToList();


            return results;
        }
   
      
        public Product Save(Product product)
        {
         
            if (product.Id == 0)
            {
        
                repoProduct.Insert(product);
            }
            else
            {
                repoProduct.Update(product);
            }
            return product;
        }
        public ProductImage Save(ProductImage productImage)
        {
            productImage.UpdateDate = DateTime.Now;
            if (productImage.Id == 0)
            {
                productImage.AddedDate = DateTime.Now;
                repoProductImage.Insert(productImage);
            }
            else
            {
                repoProductImage.Update(productImage);
            }
            return productImage;
        }


        public Product GetProduct(int id)
        {
            return repoProduct.FindById(id);
        }
        public KeyValuePair<int, List<Product>> GetOutofStockProduct(DataTableServerSide searchModel)
        {
            var fdata = searchModel.filterdata;
            int totalCount;
            int page = searchModel.start == 0 ? 1 : (Convert.ToInt32(Decimal.Floor(Convert.ToDecimal(searchModel.start) / searchModel.length)) + 1);

            List<Product> results = repoProduct
                .Query().Filter(x=>x.StockQuantity ==0)
                .CustomOrderBy(u => u.OrderBy(searchModel, new Type[] { typeof(Product), typeof(User) }))
                .GetPage(page, searchModel.length, out totalCount)
                .ToList();
            KeyValuePair<int, List<Product>> resultResponse = new KeyValuePair<int, List<Product>>(totalCount, results);

            return resultResponse;

        }

     
        #endregion
    }
}

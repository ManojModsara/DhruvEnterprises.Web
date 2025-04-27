using Newtonsoft.Json;
using DhruvEnterprises.Core;
using DhruvEnterprises.Data;
using DhruvEnterprises.Dto;
using DhruvEnterprises.Service;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace DhruvEnterprises.Web.Controllers
{
    public class ProductController : BaseController
    {
        DhruvEnterprisesDBEntities db = new DhruvEnterprisesDBEntities();

        private readonly IPackageService packageService;
        private readonly IUserService userService;
        private IProductService productService;
        public ActionAllowedDto action;
        SqlConnection con;

        public ProductController(IProductService _ProductService,
            IUserService _userService,
            IActivityLogService _activityLogService,
            IRoleService roleService,
            IPackageService _packageService) : base(_activityLogService, roleService)
        {
            this.packageService = _packageService;
            this.userService = _userService;
            this.productService = _ProductService;

            this.action = new ActionAllowedDto();

        }
        // GET: Product
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult GetProducts(DataTableServerSide model)
        {
            ViewBag.actionAllowed = action = ActionAllowed("Product", CurrentUser.RoleId);
            int uid = CurrentUser.IsAdminRole ? 0 : CurrentUser.UserID;
            KeyValuePair<int, List<Product>> products = productService.GetProducts(model, uid);

            return Json(new
            {
                draw = model.draw,
                recordsTotal = products.Key,
                recordsFiltered = products.Key,
                data = products.Value.Where(x => (CurrentUser.IsAdminRole ? true : (x.Id != CurrentUser.RoleId && x.Id != 1))).Select(c => new List<object> {
                     (action.AllowEdit? DataTableButton.EditButton(Url.Action( "createedit", "Product",new { id = c.Id })):string.Empty)+"&nbsp"+
                     (action.AllowEdit? DataTableButton.ImgButton(Url.Action( "AddImage", "Product",new { id = c.Id })):string.Empty)+"&nbsp"+
                   (action.AllowEdit?  DataTableButton.PlusButton2(Url.Action( "AddProductStock","Product", new { id = c.Id }),"modal-add-edit-addapiwallet","Add Product Stock"):string.Empty)
                   ,
                    c.Id,
                    productService.GetProductImageById(c.Id)?.ImageUrl??"/Images/NoImage.png",
                    c.Name,
                    c.StockQuantity,
                    (Math.Round(c.Price,2)).ToString("0.00"),
                    (Math.Round(c.ProductCost,2)).ToString("0.00"),
                    c.Published
                   })
            }, JsonRequestBehavior.AllowGet);
        }
        public ActionResult CreateEdit(int? id)
        {
            Productdto model = new Productdto();
            //UpdateActivity("Product Create/Edit REQUEST", "GET:Product/CreateEdit/" + id, "ProductId=" + id);
            action = ActionAllowed("Product", CurrentUser.RoleId, id.HasValue ? 3 : 2);

            if (id.HasValue && id.Value > 0)
            {
                Product product = productService.GetProductByIdAsync(id.Value);
                model.Id = product.Id;
                model.Name = product.Name;
                model.StockQuantity = product.StockQuantity;
                model.FullDescription = product.FullDescription;
                model.Deleted = product.Deleted;
                model.Published = product.Published;
                model.DisplayOrder = product.DisplayOrder;
                model.Price = product.Price;
                model.ProductCost = product.ProductCost;
                model.DisplayStockAvailability = product.DisplayStockAvailability;
                model.ShowOnHomepage = product.ShowOnHomepage;

                //model.OrderMinimumQuantity = product.OrderMinimumQuantity;
                //model.OrderMaximumQuantity = product.OrderMaximumQuantity;
                if (model.CategoryName != null)
                {
                    model.CategoryIds = model.CategoryName.Replace(" ", "").Split(',').ToList();
                }
            }
            return View("createedit", model);

        }

        [HttpPost]
        public ActionResult CreateEdit(Productdto model, FormCollection FC)
        {
            string message = string.Empty;
            action = ActionAllowed("Product", CurrentUser.RoleId, model.Id > 0 ? 3 : 2);
            try
            {
                Product product = productService.GetProductByIdAsync(model.Id) ?? new Product();
                product.Id = model.Id;

                product.Name = FirstLetterToUpper(model.Name);
                product.FullDescription = model.FullDescription;
                product.ShowOnHomepage = true;
                product.DisplayOrder = model.DisplayOrder;
                product.DisplayStockAvailability = model.DisplayStockAvailability;
                product.Deleted = model.Deleted;
                product.StockQuantity = model.StockQuantity;
                product.ProductCost = model.ProductCost;
                product.Published = model.Published;
                if (model.Id > 0)
                {
                    product.StockQuantity = model.Id == 0 ? 10 : model.StockQuantity;
                }
                product.DisplayOrder = model.DisplayOrder;
                if (model.Price == 0)
                {
                    ShowErrorMessage("Error!", "Please Enter Price!", false);
                }
                else
                {
                    product.Price = model.Price;
                }

                product.ShowOnHomepage = model.ShowOnHomepage;


                //if (model.OrderMinimumQuantity > model.OrderMaximumQuantity)
                //{
                //    product.OrderMaximumQuantity = model.OrderMinimumQuantity;
                //}
                //else
                //{
                //    product.OrderMaximumQuantity = model.OrderMaximumQuantity == 0 ? 1 : model.OrderMaximumQuantity;
                //}
                //product.OrderMinimumQuantity = model.OrderMinimumQuantity == 0 ? 1 : model.OrderMinimumQuantity;

                productService.Save(product);
                ShowSuccessMessage("Success!", "Product has been saved", false);

            }
            catch (Exception Ex)
            {
                var msg = Ex.GetBaseException().ToString();
                if (msg.Contains("UNIQUE KEY"))
                {
                    message = "Product already exist.";
                    ShowErrorMessage("Error!", message, false);
                }
                else
                {
                    message = "An internal error found during to process your requested data!";
                    ShowErrorMessage("Error!", message, false);
                }
            }
            return RedirectToAction("Index");
        }
        public string FirstLetterToUpper(string str)
        {
            if (str == null)
                return null;

            if (str.Length > 1)
                return char.ToUpper(str[0]) + str.Substring(1);

            return str.ToUpper();
        }
        public bool Active(int id)
        {
            ViewBag.actionAllowed = action = ActionAllowed("Product", CurrentUser.RoleId, 3);
            string message = string.Empty;
            try
            {
                var adminUser = productService.GetProductByIdAsync(id);
                adminUser.Published = !adminUser.Published;
                return productService.Save(adminUser).Published;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public ActionResult AddImage(int? PId, int? Id)
        {
            ProductImagedto model = new ProductImagedto();
            model.PId = Id;
            if (Id.HasValue && Id.Value > 0)
            {
                ProductImage productImage = productService.GetProductImageByIdAsync(Id.Value) ?? new ProductImage();
                model.Id = productImage.Id;
                model.ImageUrl = productImage?.ImageUrl;

                model.PId = productImage.PId;
                model.DisplayOrder = productImage.DisplayOrder;
            }
            return View("AddImage", model);
        }
        [HttpPost]
        public ActionResult AddImage(ProductImagedto model, FormCollection FC)
        {
            string message = string.Empty;
            ProductImage PrdImage = new ProductImage();
            try
            {
                if (ModelState.IsValid)
                {
                    string filename = null;
                    string response = "";
                    if (model.FileAttach != null)
                    {
                        int i = model.FileAttach.ContentLength;
                        if (i < 768000)
                        {
                            FileUpdoad(model.FileAttach, ref filename, ref response);
                        }
                        else
                        {
                            FileUpdoad(model.FileAttach, ref filename, ref response);
                        }
                    }
                    ProductImage productImage = new ProductImage();

                    productImage.ImageUrl = filename ?? productImage.ImageUrl;
                    productImage.PId = model.Id;

                    productImage.DisplayOrder = model.DisplayOrder ?? 0;
                    productImage.AddedById = model.Id > 0 ? productImage.AddedById : CurrentUser.UserID;

                    productService.Save(productImage);
                    ShowSuccessMessage("Success!", "Image has been saved", false);
                }
            }
            catch (Exception Ex)
            {
                LogException(Ex);
                message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error!", message, false);

            }
            return RedirectToAction("Index", "Product");
        }
        public void FileUpdoad(HttpPostedFileBase file, ref string _FileName, ref string Response)
        {
            string ext = System.IO.Path.GetExtension(file.FileName);
            int maxlength = 1024 * 512 * 1; // 1 MB;
            if (ext.ToUpper().Trim() == ".jpg" || ext.ToUpper().Trim() == ".png" || ext.ToUpper().Trim() == ".jpeg")
            {
                Response = "Please choose only .jpg, .png ,.jpeg image types!";
            }
            else
            {
                if (file != null && file.ContentLength > 0)
                {
                    byte[] upload = new byte[file.ContentLength];
                    file.InputStream.Read(upload, 0, file.ContentLength);
                    string Name = DateTime.Now.ToString("yyyyMMddHHmmss");
                    _FileName = Name + Path.GetFileName(file.FileName);
                    var _path = Path.Combine(Server.MapPath("~/img/product/"), _FileName);
                    Stream strm = file.InputStream;
                    //Compressimage(strm, _path, _FileName);

                    file.SaveAs(_path);

                    _FileName = "/img/product/" + _FileName;
                }
                Response = "FileUpload Successfull";
            }
        }
        public ActionResult AddProductStock(int? id)
        {
            int Productid = id ?? 0;
            //UpdateActivity("Add/Edit ApiWallet ", "Get:ApiWallet/AddEdit");
            ViewBag.actionAllowed = action = ActionAllowed("Product", CurrentUser.RoleId, 2);
            ProductStockdto model = new ProductStockdto();
            ViewBag.ProductList = productService.GetProductUserList(CurrentUser.UserID);

            model.PId = Productid;
            string dtime = DateTime.Now.ToString("yyyyMMddHHmmssfff");
            model.Remark = string.IsNullOrWhiteSpace(model.Remark) ? dtime : model.Remark;
            return PartialView("_AddProductStock", model);
        }

        [HttpPost]
        public ActionResult AddProductStock(ProductStockdto model, FormCollection FC)
        {
            ViewBag.actionAllowed = action = ActionAllowed("Product", CurrentUser.RoleId, 2);

            model.Remark = !string.IsNullOrEmpty(model.Remark) ? model.Remark : "NA";
            string error = "0";
            try
            {
                string dtime = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                model.Remark = string.IsNullOrWhiteSpace(model.Remark) ? dtime : model.Remark;
                if (model.PId <= 0)
                {
                    ShowErrorMessage("Error!", "Invalid ProductId", false);
                }
                else if (model.Quantity <= 0 || model.Quantity > 10000000 || model.Quantity > 10000000 || model.Quantity > 10000000)
                {
                    ProductStockdto dt = new ProductStockdto();
                    dt.Quantity = 0;
                    ShowErrorMessage("Error!", "Please Enter valid Quantity!", false);
                }
                else
                {
                    Product prd = productService.GetProductByIdAsync(model.PId) ?? new Product();
                    if (model.Quantity >= prd.OrderMinimumQuantity)
                    {


                        using (SqlConnection con = new SqlConnection(LIBS.SiteKey.SqlConn))
                        {

                            SqlCommand cmd = new SqlCommand("sp_AddProductStock", con);
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ProductId", model.PId);
                            cmd.Parameters.AddWithValue("@UserId", model.UserId >= 0 ? CurrentUser.UserID : model.UserId);
                            cmd.Parameters.AddWithValue("@Remark", model.Remark);
                            cmd.Parameters.AddWithValue("@AddedById", CurrentUser.UserID);
                            cmd.Parameters.AddWithValue("@Quantity", model.Quantity);
                            cmd.Parameters.AddWithValue("@IsPullOut", model.IsPullOut ? "Yes" : "No");
                            cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            con.Open();
                            cmd.ExecuteNonQuery();
                            con.Close();

                            error = Convert.ToString(cmd.Parameters["@error"].Value);

                        }
                        if (error == "0")
                        {
                            ShowSuccessMessage("Success!", "Product Quantity has been updated", false);
                        }

                        else if (error == "2")
                        {
                            ShowErrorMessage("Error!", "Duplicate Cheque/Ref number!", false);
                        }

                        else
                        {
                            ShowErrorMessage("OOPS!", "Something Went Wrong!", false);
                        }
                    }
                    else
                    {
                        ShowErrorMessage("Error!", "Please Enter Higher Quantity From Minimum Order Quantity!", false);
                    }
                }
            }

            catch (Exception ex)
            {
                string message = "An internal error found during to process your requested data!";
                ShowErrorMessage("Error -Vendor Wallet!", message, false);
                LogException(ex);
            }
            return RedirectToAction("Index", "Product");
        }

        public ActionResult ProductList()
        {
            ICollection<Productdto> Category = new List<Productdto>();

            var data = productService.GetProductList().Where(x => x.Published = true).Take(100).ToList();


            foreach (var data1 in data)
            {
                var ProductPrice = decimal.Round(data1.Price, 2);
                Productdto productdto = new Productdto
                {
                    Id = data1.Id,
                    Name = data1.Name.Length > 30 ? data1.Name.Substring(0, 27) + "..." : data1.Name,
                    Price = Convert.ToDecimal(data1.Price),
                    ImageUrl = productService.GetProductImageById(data1.Id)?.ImageUrl ?? "/Images/NoImage.png",
                    StockQuantity = data1.StockQuantity,
                    cartquantity = productService.GetCartUserProduct(CurrentUser.UserID, data1.Id)?.Quantity ?? 0

                };

                Category.Add(productdto);
            }
            if (Category.Count() >= 1)
            {
                ViewBag.ProductMaxPrice = Category.OrderByDescending(x => x.Price).FirstOrDefault().Price;
                ViewBag.ProductnMInPrice = Category.OrderBy(x => x.Price).FirstOrDefault().Price;

            }
            else
            {
                ViewBag.NotFound = 1;
            }

            return View(Category);

        }
        [HttpPost]
        public JsonResult AddToCart(int? Quantity, int productId, string url, string Imageurl, int? ProductAttributeID, int? WishlistId)
        {
            dynamic response = new ExpandoObject();
            Product product = productService.GetProductByIdAsync(productId);
            Imageurl = productService.GetProductImageListById(productId).FirstOrDefault() == null ? "~/Images/NoImage.png" : productService.GetProductImageListById(productId).FirstOrDefault().ImageUrl;
            if (CurrentUser == null)
            {
                TempData["Url"] = url;
                response.Message = "Anonymous User";
                response.Token = "3";
                //return RedirectToAction("Index", "Account");
            }

            else
            {
                if (ProductAttributeID != 0 && ProductAttributeID != null)
                {
                    var cart = productService.GetCartList(CurrentUser.UserID).Where(x => x.ProductId == productId).ToList();

                    if (cart.Count == 0)
                    {
                        Cart cart1 = new Cart
                        {
                            UserId = CurrentUser.UserID,
                            ProductId = productId,
                            Addeddate = DateTime.Now,
                            Quantity = Quantity ?? 1,
                            ImageUrl = Imageurl,
                            CheakStatus = 2
                        };
                        productService.Saves(cart1);
                        var cartlist1 = productService.GetCartList(CurrentUser.UserID).ToList();
                        response.Message = "Saved";
                        response.Token = "1";
                        response.notification = cartlist1.Count;
                    }
                    else
                    {
                        var currentQuantity = product.StockQuantity - cart.FirstOrDefault().Quantity;
                        if (currentQuantity >= Quantity)
                        {
                            Cart carts = cart.FirstOrDefault() ?? new Cart();
                            carts.UserId = CurrentUser.UserID;
                            carts.ProductId = productId;
                            carts.Addeddate = DateTime.Now;
                            carts.Quantity = carts.Id == 0 ? Quantity ?? 1 : carts.Quantity + Quantity ?? 1;
                            carts.ImageUrl = Imageurl;
                            carts.CheakStatus = 2;
                            productService.Saves(carts);
                            var cartlist = productService.GetCartList(CurrentUser.UserID).ToList();
                            response.Message = "Saved";
                            response.Token = "1";
                            response.notification = cartlist.Count; /*cartlist.Count();*/

                        }
                        else
                        {
                            response.Message = "Not Saved";
                            response.Token = "4";
                        }

                    }

                }

                else
                {
                    Cart carts = productService.GetCartUserProduct(CurrentUser.UserID, productId) ?? new Cart();
                    var currentQuantity = product.StockQuantity - carts.Quantity;
                    if (currentQuantity >= Quantity)
                    {
                        carts.UserId = CurrentUser.UserID;
                        carts.ProductId = productId;
                        carts.Addeddate = DateTime.Now;
                        carts.Quantity = carts.Id == 0 ? Quantity ?? 1 : carts.Quantity + Quantity ?? 1;
                        carts.ImageUrl = Imageurl;
                        carts.CheakStatus = 2;
                        productService.Saves(carts);
                        var cartlist = productService.GetCartList(CurrentUser.UserID).ToList();
                        response.Message = "Saved";
                        response.Token = "1";
                        response.notification = cartlist.Count; /*cartlist.Count();*/
                    }
                    else
                    {
                        response.Message = "Not Saved";
                        response.Token = "4";
                    }
                }
            }
            var serelizeResponse = JsonConvert.SerializeObject(response);
            return Json(serelizeResponse, JsonRequestBehavior.AllowGet);
        }

        public ActionResult ViewDecreaseQty(int Id, string url, int? IndexCart)
        {
            string ReturnUrl = Request.UrlReferrer.AbsoluteUri.ToString();
            List<Itemdto> itemdtos = new List<Itemdto>();
            Cart carts = new Cart();
            if (IndexCart == 1)
            {
                carts = productService.GetCartUserProduct(CurrentUser.UserID, Id);
            }
            else
            {
                carts = productService.GetCartUserProduct(CurrentUser.UserID, Id) ?? new Cart();
            }

            if (carts?.Quantity == 1)
            {
                productService.delete(carts);
                return Json(2, JsonRequestBehavior.AllowGet);
            }
            else
            {
                if (carts?.Quantity <= carts?.Product?.StockQuantity)
                {
                    carts.Quantity = carts.Quantity - 1;
                    productService.Saves(carts);
                    var cartlist1 = productService.GetCartList(CurrentUser.UserID).Where(x => x.CheakStatus == 2).ToList();
                    foreach (var da in cartlist1)
                    {

                        var product = productService.GetProductByIdAsync(da.ProductId ?? 0);
                        Productdto productdto = new Productdto
                        {
                            Id = da.Id
                        };
                        Itemdto itemdto = new Itemdto
                        {
                            ImageUrl = da.ImageUrl,
                            Quantity = da.Quantity,

                            Products = productdto,
                            ProductId = product.Id,
                            UnitPrice = product.Price,
                            Product = product,
                            Id = da.Id,
                            CheakStatus = 2


                        };
                        itemdtos.Add(itemdto);


                    }
                    if (IndexCart == 1)
                    {
                        return Json(2, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {

                        return PartialView("_ProductDetailsTable", itemdtos);
                    }
                }
                else
                {
                    ShowErrorMessage("Error!", "Cannot less from the  quantity", false);
                    return Json(1, JsonRequestBehavior.AllowGet);

                }

            }
        }

        public ActionResult VAddToCart(int Id, int? IndexCart)
        {
            dynamic response = new ExpandoObject();
            Cart cart2 = new Cart();
            List<Itemdto> itemdtos = new List<Itemdto>();
            if (CurrentUser == null)
            {
                response.Message = "Anonymous User";
                response.Token = "3";
                return RedirectToAction("Index", "Account");
            }
            else
            {
                if (IndexCart == 1)
                {
                    cart2 = productService.GetCartUserProduct(CurrentUser.UserID, Id);
                }
                else
                {
                    cart2 = productService.GetCartUserProduct(CurrentUser.UserID, Id);
                }
                if (cart2 == null)
                {
                    ShowErrorMessage("Error!", "Please first add the product", true);
                    return Json(0, JsonRequestBehavior.AllowGet);
                }
                if (cart2?.Quantity < cart2.Product.StockQuantity)
                {
                    cart2.Quantity += 1;
                    productService.Saves(cart2);
                    var cartlist1 = productService.GetCartList(CurrentUser.UserID).Where(x => x.CheakStatus == 2).ToList();


                    foreach (var da in cartlist1)
                    {
                        if (da.ProductAttributeId == null)
                        {
                            var product = productService.GetProductByIdAsync(da.ProductId ?? 0);
                            Productdto productdto = new Productdto
                            {
                                Id = da.Id
                            };
                            Itemdto itemdto = new Itemdto
                            {
                                ImageUrl = da.ImageUrl,
                                Quantity = da.Quantity,

                                Products = productdto,
                                ProductId = product.Id,
                                UnitPrice = product.Price,
                                Product = product,
                                Id = da.Id,
                                CheakStatus = 2


                            };
                            itemdtos.Add(itemdto);
                        }

                    }
                    if (IndexCart == 1)
                    {
                        return Json(2, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return PartialView("_ProductDetailsTable", itemdtos);
                    }

                }
                else
                {
                    ShowErrorMessage("Error!", "Cannot Add quantity from the StockQuantity", true);
                    return Json(1, JsonRequestBehavior.AllowGet);

                }
            }
        }
        public ActionResult ViewCart(int? id)
        {
            if (CurrentUser == null)
            {
                return RedirectToAction("Index", "Account");
            }


            List<Itemdto> itemdtos = new List<Itemdto>();
            DataTable dt = ShowCartData(CurrentUser?.UserID ?? 0);
            for (int i = 0; i < dt.Rows.Count; i++)
            {

                Itemdto productdto = new Itemdto
                {
                    Id = Convert.ToInt32(dt.Rows[i]["id"]),
                    ImageUrl = dt.Rows[i]["imageurl"].ToString(),
                    Quantity = Convert.ToInt32(dt.Rows[i]["quantity"]),
                    ProductId = Convert.ToInt32(dt.Rows[i]["productid"]),
                    CheakStatus = Convert.ToInt32(dt.Rows[i]["cheakstatus"]),
                    ProductName = Convert.ToString(dt.Rows[i]["name"]),
                    UnitPrice = Convert.ToDecimal(dt.Rows[i]["price"])
                };
                itemdtos.Add(productdto);

            }

            var cartval = productService.GetCartList(CurrentUser.UserID);
            List<Cart> cart = cartval.ToList();
            foreach (var da in cart)
            {
                if (da.Quantity > 0)
                {
                    da.CheakStatus = 2;
                }
                else
                {
                    da.CheakStatus = 1;
                }
                productService.Saves(da);
            }
            ViewBag.Item = itemdtos;
            return View(itemdtos);
        }

        private void Connection()
        {
            string constr = ConfigurationManager.ConnectionStrings["sqlconn"].ConnectionString;
            con = new SqlConnection(constr);
        }
        public DataTable ShowCartData(int userid)
        {
            try
            {
                Connection();
                using (SqlCommand cmd = new SqlCommand("ShowCart", con))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Userid", userid);
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    cmd.ExecuteNonQuery();
                    con.Close();
                    return dt;
                }
            }

            catch (Exception ex)
            {
                LIBS.Common.LogException(ex);
                return null; ;

            }
        }
        [HttpPost]
        public ActionResult DecreaseQty(int Id, string url, int? IndexCart)
        {
            var carts = productService.GetCartUserProduct(CurrentUser.UserID, Id);

            if (carts?.Quantity == 1)
            {
                productService.delete(carts);
                return Json(0, JsonRequestBehavior.AllowGet);
            }
            else
            {
                carts.Quantity = carts.Quantity - 1;
                productService.Saves(carts);
                var cartlist1 = productService.GetCartUserProduct(CurrentUser.UserID, Id);
                return Json(cartlist1.Quantity, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public ActionResult IncreaseQTY(int Id, int? Cartquantity)
        {
            var cartlist1 = productService.GetCartUserProduct(CurrentUser.UserID, Id);
            if (CurrentUser == null)
            {
                return RedirectToAction("Index", "Account");
            }
            else
            {
                if (Cartquantity == 0)
                {
                    productService.delete(cartlist1);
                    return Json(0, JsonRequestBehavior.AllowGet);
                }
                else if (Cartquantity < cartlist1.Product.StockQuantity)
                {
                    cartlist1.Quantity = (int)Cartquantity;
                    productService.Saves(cartlist1);
                    var cartlist2 = productService.GetCartUserProduct(CurrentUser.UserID, Id);
                    return Json(cartlist2.Quantity, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(01, JsonRequestBehavior.AllowGet);
                }
            }
        }
        #region product order
        [HttpPost]
        public JsonResult Ordervoucher()
        {
            string log = "start Purchase voucher";
            List<Itemdto> itemdtos = new List<Itemdto>();
            DataTable dt = ShowCartData(CurrentUser?.UserID ?? 0);
            var cartlist = productService.GetCartList(CurrentUser.UserID).Where(x => x.CheakStatus == 2).ToList();
            var txnled = packageService.GetuserIdCl_Bal(CurrentUser.UserID);
            log += "get user CL_Bal = " + txnled.CL_Bal;
            var orderid = 0;
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                Itemdto productdto = new Itemdto
                {
                    Id = Convert.ToInt32(dt.Rows[i]["id"]),
                    Quantity = Convert.ToInt32(dt.Rows[i]["quantity"]),
                    ProductId = Convert.ToInt32(dt.Rows[i]["productid"]),
                    ProductName = Convert.ToString(dt.Rows[i]["name"]),
                    UnitPrice = Convert.ToDecimal(dt.Rows[i]["price"])
                };
                itemdtos.Add(productdto);
            }
            decimal totalamount = 0;

            foreach (var item in itemdtos)
            {
                if (item.Quantity > 0)
                {
                    totalamount += item.Quantity * decimal.Round(Convert.ToDecimal(item.UnitPrice), 4);
                }
            }
            log += " total amount cart product " + totalamount;
            if (totalamount < txnled?.CL_Bal)
            {

                if (cartlist != null)
                {
                    List<Cart> cart = (List<Cart>)cartlist;
                    Random generator = new Random();
                    String randomref = generator.Next(0, 1000000).ToString("D6");
                    Order order = new Order
                    {
                        UserId = CurrentUser.UserID,
                        Amount = totalamount,
                        RequestTime = DateTime.Now,
                        OurRefId = randomref
                    };
                    productService.Saves(order);
                    log += " order create order id = " + order.Id;
                    orderid = order.Id;
                    foreach (var item in cart)
                    {
                        OrderDetail orderDetail = new OrderDetail();
                        orderDetail = new OrderDetail
                        {
                            ProductId = item.Product.Id,
                            OrderId = order.Id,
                            UnitPrice = item.Product.Price,
                            Quantity = item.Quantity
                        };
                        Product productdd = productService.GetProductByIdAsync(item.Product.Id);// update product quantity 

                        productdd.StockQuantity = productdd.StockQuantity - item.Quantity;
                        log += " update   product quantity ";
                        productService.Save(productdd);
                        productService.SaveOrderDetails(orderDetail);
                        log += " save product Detail ";
                        Cart cart1 = productService.GetCartById(item.Id);
                        productService.delete(cart1);
                    }
                }
                try
                {
                    if (orderid > 0)
                    {
                        log += " Sp_UpdateTxnLadger order id = " + orderid;

                        Connection();
                        using (SqlCommand cmd = new SqlCommand("Sp_UpdateTxnLadger", con))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@UserId", CurrentUser.UserID);
                            cmd.Parameters.AddWithValue("@Amount", totalamount);
                            cmd.Parameters.AddWithValue("@Orderid", orderid);
                            cmd.Parameters.AddWithValue("@Remark", "Purchase voucher");
                            cmd.Parameters.AddWithValue("@WalletType", 1);
                            cmd.Parameters.AddWithValue("@AddedById", CurrentUser.UserID);
                            cmd.Parameters.Add("@error", SqlDbType.VarChar, 100).Direction = ParameterDirection.Output;
                            con.Open();
                            int i = cmd.ExecuteNonQuery();
                            con.Close();
                            int Error = Convert.ToInt32(cmd.Parameters["@Error"].Value);

                        }
                    }
                    else
                    {
                        ShowErrorMessage("Error!", "something went wrong order not Create", false);
                        LogActivity(log);
                        return Json(2, JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {

                    LIBS.Common.LogException(ex);
                }
            }
            else
            {
                ShowErrorMessage("Error!", "Insufficient Balance", false);
                return Json(3, JsonRequestBehavior.AllowGet);
            }
            LogActivity(log);
            return Json(1, JsonRequestBehavior.AllowGet);
        }
        #endregion

    }
}
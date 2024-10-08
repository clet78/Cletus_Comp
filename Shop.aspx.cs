using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Front_End.ServiceReference1;

namespace Front_End
{
    public partial class Shop : System.Web.UI.Page
    {
        Service1Client SC = new Service1Client();

        protected void Page_Load(object sender, EventArgs e)
        {
            string categoryParam = Request.QueryString["Category"];
            string saveParam = Request.QueryString["Save"];
            string sortParam = Request.QueryString["sort"];

            dynamic filteredProducts = null;

            if (!string.IsNullOrEmpty(categoryParam))
            {
                // Check if the category is an integer (for numerical categories)
                if (int.TryParse(categoryParam, out int categoryId))
                {
                    filteredProducts = SC.GetProductByCategory(categoryId);
                }
            }
            else
            {
                // Fetch all products from the service
                dynamic product = SC.GetProducts();
                if (product != null)
                {
                    filteredProducts = new List<ProductDTO>(product);
                }
            }

            // Apply filtering based on the Save parameter
            if (!string.IsNullOrEmpty(saveParam) && int.TryParse(saveParam, out int save))
            {
                filteredProducts = FilterProductsBySave(filteredProducts, save);
            }

            // Sort the products if a sort parameter is provided
            if (!string.IsNullOrEmpty(sortParam))
            {
                SortProducts(filteredProducts, sortParam);
            }

            // Generate HTML output for the products
            StringBuilder sb = new StringBuilder();
            sb.Append("<div class='row'>"); // Create a row for Bootstrap grid

            foreach (ProductDTO p in filteredProducts)
            {
                sb.Append("<div class='col-lg-4 col-md-6 col-sm-12 pb-1'>"); // Use col-lg-4 for 3 in a row
                sb.Append("<div class='product-item bg-light mb-4'>");
                sb.Append("<div class='product-img position-relative overflow-hidden'>");
                sb.Append("<img class='img-fluid w-100' src='" + p.Image + "' alt=''>");
                sb.Append("<div class='product-action'>");
                sb.Append("<a class='btn btn-outline-dark btn-square' href='Shop.aspx?AddToCart=" + p.ProductID + "'><i class='fa fa-shopping-cart'></i></a>");
                sb.Append("<a class='btn btn-outline-dark btn-square' href=''><i class='far fa-heart'></i></a>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("<div class='text-center py-4'>");
                sb.Append("<a class='h6 text-decoration-none text-truncate' href=''>" + p.Name + "</a>");
                sb.Append("<div class='d-flex align-items-center justify-content-center mt-2'>");
                if (p.DiscountedPrice != null)
                {
                    sb.Append("<h5>R" + p.DiscountedPrice + "</h5><h6 class='text-muted ml-2'><del>R" + p.Price + "</del></h6>");
                }
                else
                {
                    sb.Append("<h5>R" + p.Price + "</h5>");
                }
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("</div>");
                sb.Append("</div>"); // Close the product item div
            }

            sb.Append("</div>"); // Close the row div
            Prods.InnerHtml = sb.ToString(); // Set the generated HTML to the Prods section
            Prods2.InnerHtml = sb.ToString(); // Set the generated HTML to the Prods section
            Prods3.InnerHtml = sb.ToString(); // Set the generated HTML to the Prods section
        }


        private void SortProducts(List<ProductDTO> products, string sortParam)
        {
            switch (sortParam)
            {
                case "price_asc":
                    products.Sort((a, b) => a.Price.CompareTo(b.Price));
                    break;
                case "price_desc":
                    products.Sort((a, b) => b.Price.CompareTo(a.Price));
                    break;
                case "name_asc":
                    products.Sort((a, b) => string.Compare(a.Name, b.Name));
                    break;
                case "name_desc":
                    products.Sort((a, b) => string.Compare(b.Name, a.Name));
                    break;
            }
        }

        private List<ProductDTO> FilterProductsBySave(List<ProductDTO> products, int save)
        {
            List<ProductDTO> filteredProducts = new List<ProductDTO>();

            foreach (ProductDTO p in products)
            {
                if (save == 1 && p.DiscountedPrice != null)
                {
                    filteredProducts.Add(p); // Discounted products
                }
                else if (save == 2 && p.DiscountedPrice == null)
                {
                    filteredProducts.Add(p); // Non-discounted products
                }
            }

            return filteredProducts;
        }
        //Adding product to the session-based SHopping Cart
        protected void Page_LoadComplete(object sender, EventArgs e)
        {
            try
            {
                if (Request.QueryString["AddToCart"] != null)
                {
                    int productId = int.Parse(Request.QueryString["AddToCart"]);
                    AddToCart(productId);
                }
            }
            catch (Exception ex)
            {
                // Log the error and show a user-friendly message
                Response.Write("Error: " + ex.Message);
            }
        }

        //adding to cart
        protected void AddToCart(int productId)
        {
            try
            {
                // Check if the user is logged in and userID exists in session
                if (Session["UserID"] == null)
                {
                    throw new Exception("User not logged in.");
                }

                int userId = (int)Session["UserID"]; // Fetch user ID from session

                // Get the selected product using the service
                var selectedProduct = SC.GetProductDTO(productId);
                if (selectedProduct == null)
                {
                    throw new Exception("Selected product is not available.");
                }

                // Retrieve or create cart ID for the current user
                int cartId = GetCartID();
                decimal totalCartAmount = CalculateCartTotal(cartId);

                // Add/update the cart in the backend
                bool cartUpdated = SC.addToCart(userId, totalCartAmount);
                if (!cartUpdated)
                {
                    throw new Exception("Unable to update the cart in the database.");
                }

                // Add/update the cart item
                AddOrUpdateCartItem(cartId, selectedProduct);

                // Update cart session
                Session["CartID"] = cartId;

                // Redirect to the cart page
                Response.Redirect("Cart.aspx", false);
            }
            catch (Exception ex)
            {
                // Log error and show it to the user
                Response.Write("Error: " + ex.Message);
            }
        }

        private decimal CalculateCartTotal(int cartId)
        {
            List<CartItem> cart = (List<CartItem>)Session["Cart"] ?? new List<CartItem>();
            return cart.Sum(c => c.Price * c.Quantity); // Calculate total
        }

        protected int GetCartID()
        {
            // Check if CartID exists in session
            if (Session["CartID"] != null)
            {
                return (int)Session["CartID"];
            }

            if (Session["UserID"] == null)
            {
                throw new Exception("UserID not found in session.");
            }

            int userId = (int)Session["UserID"];
            var existingCart = SC.GetCartForUser(userId);

            if (existingCart != null && existingCart.cartID > 0)
            {
                Session["CartID"] = existingCart.cartID;
                Session["CartCreatedAt"] = existingCart.createdAt;
                return existingCart.cartID;
            }

            // If no cart exists, create one
            bool isCartCreated = SC.addToCart(userId, 0.0m);
            if (!isCartCreated)
            {
                throw new Exception("Failed to create cart.");
            }

            var newCart = SC.GetCartForUser(userId);
            if (newCart == null || newCart.cartID <= 0)
            {
                throw new Exception("Error retrieving the new cart.");
            }

            Session["CartID"] = newCart.cartID;
            Session["CartCreatedAt"] = newCart.createdAt;
            return newCart.cartID;
        }

        private void AddOrUpdateCartItem(int cartId, ProductDTO selectedProduct)
        {
            List<CartItem> cart = (List<CartItem>)Session["Cart"] ?? new List<CartItem>();

            CartItem existingItem = cart.Find(c => c.ProductID == selectedProduct.ProductID);

            if (existingItem != null)
            {
                existingItem.Quantity++;
                if (!SC.addCartItems(cartId, existingItem.ProductID, existingItem.Quantity, existingItem.Price))
                {
                    throw new Exception("Failed to update cart item.");
                }
            }
            else
            {
                CartItem newItem = new CartItem
                {
                    CartID = cartId,
                    ProductID = selectedProduct.ProductID,
                    ProductName = selectedProduct.Name,
                    Price = selectedProduct.Price,
                    Quantity = 1
                };

                cart.Add(newItem);

                if (!SC.addCartItems(cartId, newItem.ProductID, newItem.Quantity, newItem.Price))
                {
                    throw new Exception("Failed to add new item to cart.");
                }
            }

            Session["Cart"] = cart; // Update session with cart
        }


    }
}

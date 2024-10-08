using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Front_End.ServiceReference1;

namespace Front_End
{
    public partial class Cart : System.Web.UI.Page
    {
        Service1Client sc = new Service1Client();
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["LoggedIn"] != null)
            {
                if (!IsPostBack)
                {
                    BindCartItems();
                    RemoveItemFromCart(); // Call to remove item if requested
                }
            }
            else
            {
                Response.Redirect("Login.aspx");
            }

        }
        private void RemoveItemFromCart()
        {
            // Check if there is a "Remove" parameter in the query string
            if (Request.QueryString["Remove"] != null)
            {
                int productIdToRemove;
                if (int.TryParse(Request.QueryString["Remove"], out productIdToRemove))
                {
                    List<CartItem> cart = (List<CartItem>)Session["Cart"];
                    if (cart != null)
                    {
                        // Find the item to remove
                        var itemToRemove = cart.FirstOrDefault(item => item.ProductID == productIdToRemove);
                        if (itemToRemove != null)
                        {
                            cart.Remove(itemToRemove); // Remove the item from the cart
                            Session["Cart"] = cart; // Update the session with the new cart
                        }
                    }
                }
            }
        }

        protected void BindCartItems()
        {
            List<CartItem> cart = (List<CartItem>)Session["Cart"];
            string display = null;
            if (cart != null && cart.Count > 0)
            {
                display = "<div class='col-lg-8 table-responsive mb-5'>";
                display += "<table class='table table-light table-borderless table-hover text-center mb-0'>";
                display += "<thead class='thead-dark'>";
                display += "<tr><th>Products</th><th>Price</th><th>Quantity</th><th>Total</th><th>Remove</th></tr>";
                display += "</thead>";
                display += "<tbody class='align-middle'>";

                decimal grandTotal = 0;

                decimal SubTotal = 0;
                decimal shippingFee = 0;
                decimal shipping = 0;
                decimal vat = 0;
                foreach (CartItem c in cart)
                {
                    decimal Total = c.Price * c.Quantity;
                    SubTotal += Total;
                    vat = Math.Round(SubTotal * 0.07m, 2);
                    shippingFee = SubTotal * 0.3m;
                    shipping = Math.Round(shippingFee, 2);
                    grandTotal = Math.Round(shippingFee + SubTotal + vat, 2);
                    Session["Total"] = grandTotal.ToString("F2");
                    Session["Vat"] = vat;
                    display += "<tr>";
                    display += $"<td class='align-middle'><img src='{c.Image}' alt='' style='width: 50px;'>{c.ProductName}</td>";
                    display += $"<td class='align-middle'>R{c.Price}</td>";
                    display += $"<td class='align-middle'>{c.Quantity}</td>";
                    
                    //display += "<div class='input-group quantity mx-auto' style='width: 100px;'>";
                    //display += "<div class='input-group-btn'>";
                    //display += $"<a href='Cart.aspx?Minus={c.ProductID}' class='btn btn-sm btn-primary btn-minus'><i class='fa fa-minus'></i></a>";
                    //display += "</div>";
                    //display += "<input type='text' class='form-control form-control-sm bg-secondary border-0 text-center' value='1'>";
                    //display += "<div class='input-group-btn'>";
                    //display += $"<a href='Cart.aspx?Plus={c.ProductID}' class='btn btn-sm btn-primary btn-plus'><i class='fa fa-plus'></i></a>";
                    //display += "</div>";
                    //display += "</div>";
                    display += $"<td class='align-middle'>R{Total}</td>";
                    display += $"<td class='align-middle'><a href='Cart.aspx?Remove={c.ProductID}' class='btn btn-danger btn-sm'>Remove</a></td>";
                    //Use asp:TextBox to trigger postback on quantity change
                    display += $"<td><asp:TextBox ID='txtQuantity_{c.ProductID}' runat='server' Text='{c.Quantity}' AutoPostBack='true' OnTextChanged='Quantity_Changed'/></td>";
                    display += "</tr>";
                }
                display += " </tbody>";
                display += "</table>";
                display += "</div>";

                Cart_Tables.Controls.Add(new Literal { Text = display });
                display = null;

                //display += "<div class='col-lg-4'>";
                //display += "<h5 class='section-title position-relative text-uppercase mb-3'><span class='bg-secondary pr-3'>Cart Summary</span></h5>";
                //display += "<div class='bg-light p-30 mb-5'>";
                display += "<div class='border-bottom pb-2'>";
                display += "<div class='d-flex justify-content-between mb-3'>";
                display += "<h6>Subtotal</h6>";
                display += $"<h6>R{SubTotal}</h6>";
                display += "</div>";
                display += "<div class='d-flex justify-content-between'>";
                display += "<h6 class='font-weight-medium'>Shipping</h6>";
                display += $"<h6 class='font-weight-medium'>R{shipping}</h6>";
                display += "</div>";
                display += "</div>";
                display += "<div class='pt-2'>";
                display += "<div class='d-flex justify-content-between mt-2'>";
                display += "<h5>Grand Total</h5>";
                display += $"<h5>R{grandTotal}</h5>";
                display += "</div>";
                //display += "<asp:Button ID='btnCheckout' class='btn btn-block btn-primary font-weight-bold my-3 py-3' Text='Proceed To Checout' runat='server' Enabled='false' OnClick='btnCheckout_Click'/>";
                //display += "</div>";
                display += "</div>";
                //display += "</div>";

                Cart_Item.Controls.Add(new Literal { Text = display });


                //Enable Checkout button
                btnCheckout.Enabled = true;

            }
            else
            {
                Cart_Tables.Controls.Add(new Literal { Text = "<p>Your shopping cart is empty!</p>" });
                btnCheckout.Enabled = false;

            }
        }
        // Handle quantity change for updating the cart
        protected void Quantity_Changed(object sender, EventArgs e)
        {
            TextBox txtBox = (TextBox)sender;
            string productIdString = txtBox.ID.Replace("txtQuantity_", "");
            int productId = int.Parse(productIdString);
            int newQuantity = int.Parse(txtBox.Text);
            Session["ProdID"] = productId;
            Session["Quantity"] = newQuantity;
            UpdateCartQuantity(productId, newQuantity);
        }
        // Method to update cart quantity and rebind the cart
        protected void UpdateCartQuantity(int productId, int quantity)
        {
            List<CartItem> cart = (List<CartItem>)Session["Cart"];
            CartItem item = cart.Find(c => c.ProductID == productId);
            if (item != null)
            {
                item.Quantity = quantity;
            }
            //Session["ProdID"] = productId;
            //Session["Quantity"] = quantity;
            // Save updated cart back to session
            Session["Cart"] = cart;

            // Refresh the cart display
            BindCartItems();
        }

        //Proceed to checkout and view invoice.
        protected void btnCheckout_Click(object sender, EventArgs e)
        {
            // Retrieve cart and session values, and ensure they are valid
            List<CartItem> cart = (List<CartItem>)Session["Cart"];
            if (cart == null || cart.Count == 0)
            {
                throw new Exception("Your cart is empty.");
                
            }

            if (Session["Total"] == null || Session["Vat"] == null || Session["UserID"] == null)
            {
                throw new Exception("Session values missing.");
               
            }

            decimal Total = Convert.ToDecimal(Session["Total"]);
            decimal vat = Convert.ToDecimal(Session["Vat"]);
            int userID = (int)Session["UserID"];

            try
            {
                // Add invoice to the database
                bool IsCreated = sc.addToInvoice(userID, Total, vat);
                if (!IsCreated)
                {
                    throw new Exception("Error: Could not create invoice.");
                }

                // Retrieve the newly created invoice
                var invoice = sc.GetInvoiceId(userID);
                if (invoice == null)
                {
                    throw new Exception("Error: Invoice not found after creation.");
                }

                int invoiceID = invoice.InvoiceID;

                // Iterate through cart items and add them as invoice items
                foreach (CartItem c in cart)
                {
             
                    int prodID = c.ProductID;
                    int quantity = c.Quantity;

                    // Get product details
                    var prod = sc.GetProductDTO(prodID);
                    if (prod == null)
                    {
                        throw new Exception($"Error: Product with ID {prodID} does not exist.");
                    }

                    // Add the product as an invoice item
                    bool IsAdded = sc.addInvoiceItems(invoiceID, prodID, quantity, prod.Price);
                    if (!IsAdded)
                    {
                        throw new Exception($"Error: Could not add invoice item for Product ID {prodID}.");
                    }
                }
                cart.Clear();
                Response.Redirect("Invoices.aspx", false);
                Context.ApplicationInstance.CompleteRequest(); // Optional, to bypass any further page lifecycle processing
                
            }
            catch (Exception ex)
            {
                // Log error, then redirect to an error page (logging can be done with a logging framework)
                throw new Exception("Error adding invoice and invoice items: " + ex.Message);
                // Optionally log the exception details for debugging (could use ex.GetBaseException() for root cause)
                // Redirect to an error page with a user-friendly message

            }
        }
    }
    //public class InvoiceItemDTO
    //{
    //    public int InvoiceID { get; set; }
    //    public int ProductID { get; set; }
    //    public int Quantity { get; set; }
    //    public decimal UnitPrice { get; set; }
    //}
    public class CartItem
    {
        public int ProductID { get; set; }
        public int CartID { get; set; }
        public string ProductName { get; set; }
        public string Image { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal Total
        {
            get { return Price * Quantity; }
        }
    }
}
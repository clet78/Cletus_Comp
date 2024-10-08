﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace Back_End
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        bool Register(string Username, string Email, string Password);

        [OperationContract]
        int Login(string Email, string Password);

        [OperationContract]
        List<ProductDTO> GetProducts();

        [OperationContract]
        Product GetProduct(int ID);

        [OperationContract]
        Customer1 GetUserByEmail(string email);

        [OperationContract]
        List<ProductDTO> GetProductByCategory(int CatID);
        [OperationContract]
        bool addToCart(int userID, decimal totalAmount);
        [OperationContract]
        bool addCartItems(int cartID, int prodID, int quantity, decimal price);
        [OperationContract]
        CartDTO GetCartForUser(int userID);
        [OperationContract]
        ProductDTO GetProductDTO(int ID);
        [OperationContract]
        bool addToInvoice(int userID, decimal Total, decimal vatAmount);
        [OperationContract]
        bool addInvoiceItems(int InvoiceID, int ProdID, int Quantity, decimal uPrice);
        [OperationContract]
        Invoice GetInvoiceId(int userID);
       
    }
}

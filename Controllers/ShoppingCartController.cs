using CafateriaSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using PayPal.Api;

namespace CafateriaSystem.Controllers
{
    public class ShoppingCartController : Controller
    {
        CafeteriaDBEntity db = new CafeteriaDBEntity();
        private string strCart = "Cart";
        decimal cost;

        // GET: ShoppingCart
        public ActionResult CheckOut()
        {
            return View();
        }

        public ActionResult Failure()
        {
            return View();
        }


        public ActionResult OrderNow(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (Session[strCart] == null)
            {
                List<Cart> lsCart = new List<Cart>
                {
                  new Cart(db.Menus.Find(id),1)
                };

                Session[strCart] = lsCart;
            }
            else
            {
                List<Cart> lsCart = (List<Cart>)Session[strCart];
                int check = IsExistingCheck(id);
                if (check == -1)
                    lsCart.Add(new Cart(db.Menus.Find(id), 1));
                else
                    lsCart[check].Quantity++;

                Session[strCart] = lsCart;
            }
            return View("CheckOut");
        }
        public int IsExistingCheck(int? id)
        {
            List<Cart> lsCart = (List<Cart>)Session[strCart];
            for (int i = 0; i < lsCart.Count; i++)
            {

                if (lsCart[i].Menu.menu_id == id)
                    return i;
            }
            return -1;
        }

        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            int check = IsExistingCheck(id);
            List<Cart> lsCart = (List<Cart>)Session[strCart];
            lsCart.RemoveAt(check);
            return View("CheckOut");

        }

        public ActionResult UpdateCart(FormCollection obj, string deliver, string delivercost, string userName, string userEmail, string userPhone, string location)
        {
            string[] quantities = obj.GetValues("quantity");
            List<Cart> lstCart = (List<Cart>)Session[strCart];
            for (int i = 0; i < lstCart.Count; i++)
            {
                lstCart[i].Quantity = Convert.ToInt32(quantities[i]);

            }
            Session[strCart] = lstCart;
            ViewBag.Method = deliver;
            ViewBag.Cost = delivercost;
            Session["UserEmail"] = userEmail;
            Session["UserName"] = userName;
            Session["UserPhone"] = userPhone;
            Session["UserLocation"] = location;
            return View("CheckOut");
        }

        public int PlaceOrder(int userid, string delivery)
        {
            List<Cart> lstCart = (List<Cart>)Session[strCart];
            CafateriaSystem.Models.Order order = new CafateriaSystem.Models.Order();
            {
                order.order_date = DateTime.Today;
                
                order.order_time = DateTime.Now.TimeOfDay;
                
                order.customer_id = userid;
                
                order.deliver_method = delivery;
                
                order.order_status = "Pending";
               
            };
            db.Orders.Add(order);
            db.SaveChanges();

            foreach (Cart cart in lstCart)
            {
                OrderDetail orderDetail = new OrderDetail();
                {
                    orderDetail.order_id = order.order_id;
                    orderDetail.menu_id = cart.Menu.menu_id;
                    orderDetail.item_name = cart.Menu.name;
                    orderDetail.item_quantity = cart.Quantity;
                };
                db.OrderDetails.Add(orderDetail);
                db.SaveChanges();

                var result = db.Menus.SingleOrDefault(m => m.menu_id == cart.Menu.menu_id);
                if (result != null)
                {
                    result.quantity -= cart.Quantity;
                    db.SaveChanges();
                }
            }
            //Session.Remove(strCart);
            return order.order_id;
        }

        //globals string
        public ActionResult PaymentWithPaypal(string Cancel = null)
        {
            int userid = Convert.ToInt32(Request.Params["userid"]);
            string deliver = Request.Params["deliver"];
            cost = Convert.ToDecimal(Request.Params["cost"]);
            //getting the apiContext
            APIContext apiContext = PaypalConfiguration.GetAPIContext();
            
            try
            {
                //A resource representing a Payer that funds a payment Payment Method as paypal
                //Payer Id will be returned when payment proceeds or click to pay
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    //this section will be executed first because PayerID doesn't exist
                    //it is returned by the create function call of the payment class

                    // Creating a payment
                    // baseURL is the url on which paypal sendsback the data.
                    string baseURI = Request.Url.Scheme + "://" + Request.Url.Authority +
                                "/ShoppingCart/PaymentWithPayPal?";

                    //here we are generating guid for storing the paymentID received in session
                    //which will be used in the payment execution

                    
                    var guid = PlaceOrder(userid,deliver).ToString(); //orderID
                    Session["orderID"] = guid;

                    //CreatePayment function gives us the payment approval url
                    //on which payer is redirected for paypal account payment

                    var createdPayment = this.CreatePayment(apiContext, baseURI + "guid=" + guid);

                    //get links returned from paypal in response to Create function call

                    var links = createdPayment.links.GetEnumerator();

                    string paypalRedirectUrl = null;

                    while (links.MoveNext())
                    {
                        Links lnk = links.Current;

                        if (lnk.rel.ToLower().Trim().Equals("approval_url")) 
                        {
                            //saving the payapalredirect URL to which user will be redirected for payment
                            paypalRedirectUrl = lnk.href;
                        }
                    }

                    // saving the paymentID in the key guid
                    Session.Add(guid, createdPayment.id);
                    
                    return Redirect(paypalRedirectUrl);
                }
                else
                {
                    // This function exectues after receving all parameters for the payment

                    var guid = Request.Params["guid"];
                    var payerid = Request.Params["payerId"];
                    var executedPayment = ExecutePayment(apiContext, payerId, Session[guid] as string);

                    //If executed payment failed then we will show payment failure message to user
                    if (executedPayment.state.ToLower() != "approved")
                    {
                        return View("Failure");
                    }
                }
            }
            catch (Exception)
            {
                return View("Failure");
            }

            //on successful payment, show success page to user.
            Session.Remove(strCart);

            return View("Success");
        }

        private PayPal.Api.Payment payment;
        private PayPal.Api.Payment ExecutePayment(APIContext apiContext, string payerId, string paymentId)
        {
            var paymentExecution = new PaymentExecution() { payer_id = payerId };
            this.payment = new PayPal.Api.Payment() { id = paymentId };
            return this.payment.Execute(apiContext, paymentExecution);
        }

        private PayPal.Api.Payment CreatePayment(APIContext apiContext, string redirectUrl)
        {

            //create itemlist and add item objects to it
            var itemList = new ItemList() { items = new List<Item>() };

            //Adding Item Details like name, currency, price etc
            List<Cart> listCarts = (List<Cart>)Session[strCart];
            foreach (var cart in listCarts)
            {
                itemList.items.Add(new Item()
                {
                    name = cart.Menu.name,
                    currency = "USD",
                    price = cart.Menu.price.ToString(),
                    quantity = cart.Quantity.ToString()
                });
            }

            var payer = new Payer() { payment_method = "paypal" };

            // Configure Redirect Urls here with RedirectUrls object
            var redirUrls = new RedirectUrls()
            {
                cancel_url = redirectUrl + "&Cancel=true",
                return_url = redirectUrl
            };

            var subtotal = listCarts.Sum(x => x.Quantity * x.Menu.price);
            var billServChgs = subtotal * 4 / 100;
            var billtax = (subtotal + billServChgs) * 8 / 100;
            var fee = billServChgs + billtax + cost;
            

            // Adding Tax, shipping and Subtotal details
            var details = new Details()
            {
                tax=fee.ToString(),
                shipping="0",
                subtotal = subtotal.ToString()
            };

            //Final amount with details
            var amount = new Amount()
            {
                currency = "USD",
                total = (Convert.ToDouble(details.subtotal) + Convert.ToDouble(details.tax) + Convert.ToDouble(details.shipping)).ToString(),
                details = details
                
            };

            TempData["Payment"] = amount.total;

            var transactionList = new List<Transaction>();
            // Adding description about the transaction
            transactionList.Add(new Transaction()
            {
                description = "ABC CAFE",
                invoice_number = Convert.ToString((new Random()).Next(100000)), //Generate an confirmation No
                amount = amount,
                item_list = itemList
            });

            this.payment = new PayPal.Api.Payment()
            {
                intent = "sale",
                payer = payer,
                transactions = transactionList,
                redirect_urls = redirUrls
            };

            // Create a payment using a APIContext
            return this.payment.Create(apiContext);
        }
    }
}
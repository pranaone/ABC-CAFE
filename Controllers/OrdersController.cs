using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using CafateriaSystem.Models;
using PagedList;

namespace CafateriaSystem.Controllers
{
    public class OrdersController : Controller
    {
        private CafeteriaDBEntity db = new CafeteriaDBEntity();

        // GET: Orders
        public ActionResult Index()
        {
            //var orders = db.Orders.Include(o => o.User);
            var orders = db.Orders.Where(y => y.order_status == "Pending" || y.order_status == "In Preparation").OrderBy(x => x.order_id);
            return View(orders.ToList());
        }

        public ActionResult ChefPanel()
        {
            var orders = db.Orders.Where(y => y.order_status == "Pending" || y.order_status == "In Preparation").OrderBy(x => x.order_id);
            return View(orders.ToList());
           
        }

        public PartialViewResult OrderListPartial(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 5;

            var orderList = db.Orders.Where(y => y.order_status == "Pending" || y.order_status == "In Preparation").OrderBy(x => x.order_id).ToPagedList(pageNumber, pageSize);
            return PartialView(orderList);

        }

        public PartialViewResult CustomerHistory(int? page, int? custid)
        {
            var pageNumber = page ?? 1;
            var pageSize = 3;

            var history = db.Orders.Where(y => y.customer_id == custid).OrderByDescending(x => x.order_id).ToPagedList(pageNumber, pageSize);
            return PartialView(history);

        }

        public PartialViewResult CustomerLastOrder(int? custid)
        {
            var orders = db.Orders.Where(x => x.customer_id == custid).OrderByDescending(y => y.order_id).FirstOrDefault();
            return PartialView(orders);

        }

        public ActionResult Prepare(int? id)
        {
            var result = db.Orders.SingleOrDefault(m => m.order_id == id);
            if (result != null)
            {
                result.order_status = "In Preparation";
                db.SaveChanges();
            }
            return View("ChefPanel");
        }

        public ActionResult Ready(int? id)
        {
            var result = db.Orders.SingleOrDefault(m => m.order_id == id);
            if (result != null)
            {
                if (result.deliver_method == "Takeaway")
                {
                    result.order_status = "Ready For Collection";
                    db.SaveChanges();
                }
                else
                {
                    result.order_status = "Awaiting Dispatch";
                    db.SaveChanges();

                    Deliver deliver = new Deliver();
                    {
                        deliver.customer_id = result.customer_id;
                        deliver.order_id = result.order_id;
                        deliver.dispatch_time = DateTime.Now.TimeOfDay;
                        deliver.deliver_status = "Pending";
                        var driver = db.Users.FirstOrDefault(d => d.user_role == "Driver" && d.status == "Available");
                        if (driver != null)
                        {
                            deliver.driver_id = driver.user_id;
                        }

                        //not available scenario to be implemented
                    }
                    db.Delivers.Add(deliver);
                    db.SaveChanges();
                }

            }
            return View("ChefPanel");
        }

        public ActionResult Cancel(int? id)
        {
            var result = db.Orders.SingleOrDefault(m => m.order_id == id);
            if (result != null)
            {
                result.order_status = "Cancelled";
                db.SaveChanges();
            }
            return View("ChefPanel");
        }

        public ActionResult CancelOrder(int id)
        {
            var result = db.Orders.SingleOrDefault(m => m.order_id == id);
            if (result != null)
            {
                if (result.order_status == "Pending")
                {
                    result.order_status = "Cancelled";
                    db.SaveChanges();
                    TempData["Message"] = "Your order has been successfully cancelled!!";
                    Payment payment = new Payment();
                    {
                        payment.payment_status = "Full Refund Required";
                    }
                    db.SaveChanges();

                }
                else if (result.order_status == "In Preparation")
                {
                    result.order_status = "Cancelled (Penalty)";
                    db.SaveChanges();
                    TempData["Message"] = "Your order has been cancelled with 50% penalty";
                    Payment payment = new Payment();
                    {
                        payment.payment_status = "50% Refund Required";
                    }
                    db.SaveChanges();
                }
                else
                {
                    TempData["Message"] = "Sorry unable to cancel - your order is ready!!";
                }
                return RedirectToAction("UserDashboard", "Users");
            }

            return RedirectToAction("UserUserDashboard", "Users");
        }

        public PartialViewResult OrderCustomerDetails(int? id)
        {
            Order order = db.Orders.Find(id);
            return PartialView(order);
        }

            public PartialViewResult OrderDetailsPartial(int? id)
        {
            Order order = db.Orders.Find(id);
            return PartialView(order);
        }

        // GET: Orders/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // GET: Orders/Create
        public ActionResult Create()
        {
            ViewBag.customer_id = new SelectList(db.Users, "user_id", "name");
            return View();
        }

        // POST: Orders/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "order_id,order_date,order_time,customer_id,deliver_method,order_status")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Orders.Add(order);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.customer_id = new SelectList(db.Users, "user_id", "name", order.customer_id);
            return View(order);
        }

        // GET: Orders/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            ViewBag.customer_id = new SelectList(db.Users, "user_id", "name", order.customer_id);
            return View(order);
        }

        // POST: Orders/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "order_id,order_date,order_time,customer_id,deliver_method,order_status")] Order order)
        {
            if (ModelState.IsValid)
            {
                db.Entry(order).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.customer_id = new SelectList(db.Users, "user_id", "name", order.customer_id);
            return View(order);
        }

        // GET: Orders/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Order order = db.Orders.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }
            return View(order);
        }

        // POST: Orders/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Order order = db.Orders.Find(id);
            db.Orders.Remove(order);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}

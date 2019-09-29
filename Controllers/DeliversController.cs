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
    public class DeliversController : Controller
    {
        private CafeteriaDBEntity db = new CafeteriaDBEntity();

        // GET: Delivers
        public ActionResult Index()
        {
            var delivers = db.Delivers.Include(d => d.Order).Include(d => d.User).Include(d => d.User1);
            return View(delivers.ToList());
        }

        public ActionResult DeliveryAgentPanel()
        {
            var delivers = db.Delivers.Where(y => y.deliver_status == "Pending" || y.deliver_status == "In Progress").OrderBy(x => x.deliver_id);
            return View(delivers.ToList());
        }

        public PartialViewResult DeliverListPartial(int? page)
        {
            var pageNumber = page ?? 1;
            var pageSize = 3;

            var deliverList = db.Delivers.Where(y => y.deliver_status == "Pending" || y.deliver_status == "In Progress").OrderBy(x => x.deliver_id).ToPagedList(pageNumber, pageSize);
            return PartialView(deliverList);
        }

        public PartialViewResult DriverDetails(int? id)
        {
            var driver = db.Users.Find(id);
            return PartialView(driver);
        }

        public PartialViewResult ManageDeliveries(string searchDelivery, int? page)
        {
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            if (!String.IsNullOrEmpty(searchDelivery))
            {
                var deliverList = db.Delivers.Where(s => s.deliver_status.Contains(searchDelivery)).OrderByDescending(x => x.deliver_id).ToPagedList(pageNumber, pageSize);
                return PartialView(deliverList);

            }

            var deliversList = db.Delivers.OrderByDescending(x => x.deliver_id).ToPagedList(pageNumber, pageSize);


            return PartialView(deliversList);
        }

        public ActionResult Pickup(int? id, int? oid)
        {
            var result = db.Delivers.SingleOrDefault(m => m.deliver_id == id);
            if (result != null)
            {
                var uid = result.driver_id;

                var driverdetail = db.Users.Where(u => u.user_id == uid).FirstOrDefault();

                var driverName = driverdetail.name;
                var driverPhone = driverdetail.phone;


                result.deliver_status = "In Progress";
                db.SaveChanges();

                var result2 = db.Orders.SingleOrDefault(m => m.order_id == oid);
                if (result2 != null)
                {
                    result2.order_status = "Order Dispatched";
                    db.SaveChanges();
                }

                var deliverydetail = db.Delivers.Where(c => c.order_id == oid && c.deliver_id == id).FirstOrDefault();

                var customerid = deliverydetail.customer_id;

                var userdetail = db.Users.Where(u => u.user_id == customerid).FirstOrDefault();

                var useremail = userdetail.email;

                var message = @String.Format("<div> Your Order has been dispatched !! <br /> Driver Details Given Below <br /> Driver Name : {0} <br /> Driver Phone : {1} <br /> </div>", driverName, driverPhone);

                EmailController email = new EmailController();

                email.SendOrderEmail(useremail, "ABC Cafe - Order Update", message);

                var result3 = db.Users.SingleOrDefault(m => m.user_id == uid);

                if (result3 != null)
                {
                    result3.status = "Un-Available";
                    db.SaveChanges();
                }
            }

            return View("DeliveryAgentPanel");
        }

        public ActionResult Delivered(int? id, int? oid)
        {
            var result = db.Delivers.SingleOrDefault(m => m.deliver_id == id);
            if (result != null)
            {
                var uid = result.driver_id;

                result.deliver_status = "Delivered";
                db.SaveChanges();

                var result2 = db.Orders.SingleOrDefault(m => m.order_id == oid);
                if (result != null)
                {
                    result2.order_status = "Delivered";
                    db.SaveChanges();
                }
                var result3 = db.Users.SingleOrDefault(m => m.user_id == uid);
                if (result3 != null)
                {
                    result3.status = "Available";
                    db.SaveChanges();
                }
                var deliverydetail = db.Delivers.Where(c => c.order_id == oid && c.deliver_id == id).FirstOrDefault();

                var customerid = deliverydetail.customer_id;

                var userdetail = db.Users.Where(u => u.user_id == customerid).FirstOrDefault();

                var useremail = userdetail.email;

                var message = @String.Format("<div> Thank you for ordering through Cafe ABC!! <br /> Have a nice day and enjoy your meal!! </div>");

                EmailController email = new EmailController();

                email.SendOrderEmail(useremail, "ABC Cafe - Enjoy!!", message);
            }

            return View("DeliveryAgentPanel");
        }


        // GET: Delivers/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deliver deliver = db.Delivers.Find(id);
            if (deliver == null)
            {
                return HttpNotFound();
            }
            return View(deliver);
        }

        // GET: Delivers/Create
        public ActionResult Create()
        {
            ViewBag.order_id = new SelectList(db.Orders, "order_id", "deliver_method");
            ViewBag.customer_id = new SelectList(db.Users, "user_id", "name");
            ViewBag.driver_id = new SelectList(db.Users, "user_id", "name");
            return View();
        }

        // POST: Delivers/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "deliver_id,order_id,customer_id,driver_id,dispatch_time,deliver_status")] Deliver deliver)
        {
            if (ModelState.IsValid)
            {
                db.Delivers.Add(deliver);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.order_id = new SelectList(db.Orders, "order_id", "deliver_method", deliver.order_id);
            ViewBag.customer_id = new SelectList(db.Users, "user_id", "name", deliver.customer_id);
            ViewBag.driver_id = new SelectList(db.Users, "user_id", "name", deliver.driver_id);
            return View(deliver);
        }

        // GET: Delivers/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deliver deliver = db.Delivers.Find(id);
            if (deliver == null)
            {
                return HttpNotFound();
            }
            ViewBag.order_id = new SelectList(db.Orders, "order_id", "deliver_method", deliver.order_id);
            ViewBag.customer_id = new SelectList(db.Users, "user_id", "name", deliver.customer_id);
            ViewBag.driver_id = new SelectList(db.Users, "user_id", "name", deliver.driver_id);
            return View(deliver);
        }

        // POST: Delivers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "deliver_id,order_id,customer_id,driver_id,dispatch_time,deliver_status")] Deliver deliver)
        {
            if (ModelState.IsValid)
            {
                db.Entry(deliver).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.order_id = new SelectList(db.Orders, "order_id", "deliver_method", deliver.order_id);
            ViewBag.customer_id = new SelectList(db.Users, "user_id", "name", deliver.customer_id);
            ViewBag.driver_id = new SelectList(db.Users, "user_id", "name", deliver.driver_id);
            return View(deliver);
        }

        // GET: Delivers/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Deliver deliver = db.Delivers.Find(id);
            if (deliver == null)
            {
                return HttpNotFound();
            }
            return View(deliver);
        }

        // POST: Delivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Deliver deliver = db.Delivers.Find(id);
            db.Delivers.Remove(deliver);
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

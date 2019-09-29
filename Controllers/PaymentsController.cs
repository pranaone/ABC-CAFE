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
    public class PaymentsController : Controller
    {
        private CafeteriaDBEntity db = new CafeteriaDBEntity();

        // GET: Payments
        public ActionResult Index()
        {
            var payments = db.Payments.Include(p => p.Order);
            return View(payments.ToList());
        }

        public void SavePayment(string billno,string payerid,int orderid,decimal bilval, string confirmno)
        {
            var payment = new Payment();
            {
                payment.bill_number = billno;
                payment.payer_id = payerid;
                payment.bill_value = bilval;
                payment.order_id = orderid;
                payment.confirmation_number = confirmno;
                payment.payment_status = "Success";

            }
            db.Payments.Add(payment);
            db.SaveChanges();
        }

        public PartialViewResult ManagePayments(string searchPayments, int? page)
        {
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            if (!String.IsNullOrEmpty(searchPayments))
            {
                var paymentList = db.Payments.Where(s => s.payment_status.Contains(searchPayments)).OrderByDescending(x=> x.payment_id).ToPagedList(pageNumber, pageSize);
                return PartialView(paymentList);

            }

            var paymentsList = db.Payments.OrderByDescending(x => x.payment_id).ToPagedList(pageNumber, pageSize);


            return PartialView(paymentsList);
        }

        // GET: Payments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        public ActionResult CustomerPaymentDetails(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Where(x => x.order_id == id).FirstOrDefault();
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // GET: Payments/Create
        public ActionResult Create()
        {
            ViewBag.order_id = new SelectList(db.Orders, "order_id", "deliver_method");
            return View();
        }

        // POST: Payments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "bill_number,payer_id,order_id,bill_value,confirmation_number,payment_id")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Payments.Add(payment);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.order_id = new SelectList(db.Orders, "order_id", "deliver_method", payment.order_id);
            return View(payment);
        }

        // GET: Payments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            ViewBag.order_id = new SelectList(db.Orders, "order_id", "deliver_method", payment.order_id);
            return View(payment);
        }

        // POST: Payments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "bill_number,payer_id,order_id,bill_value,confirmation_number,payment_id")] Payment payment)
        {
            if (ModelState.IsValid)
            {
                db.Entry(payment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.order_id = new SelectList(db.Orders, "order_id", "deliver_method", payment.order_id);
            return View(payment);
        }

        // GET: Payments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Payment payment = db.Payments.Find(id);
            if (payment == null)
            {
                return HttpNotFound();
            }
            return View(payment);
        }

        // POST: Payments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Payment payment = db.Payments.Find(id);
            db.Payments.Remove(payment);
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

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using CafateriaSystem.Models;
using PagedList;

namespace CafateriaSystem.Controllers
{
    public class UsersController : Controller
    {
        private CafeteriaDBEntity db = new CafeteriaDBEntity();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        public ActionResult UserDashboard()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }

        }

        public ActionResult ChefDashboard()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult AdminPanel()
        {
            if (Session["UserID"] != null)
            {
                return View();
            }
            else
            {
                return RedirectToAction("Login");
            }
        }

        public ActionResult AdminMenuPanel()
        {
            return View();
        }

        public ActionResult AdminUserPanel()
        {
            return View();
        }
        public ActionResult AdminDeliveriesPanel()
        {
            return View();
        }
        public ActionResult AdminPaymentsPanel()
        {
            return View();
        }

        public ActionResult Register() 
        {
            return View();
        }

        public ActionResult SignOut()
        {
            Session["UserID"] = null;
            Session["UserName"] = null;
            Session["UserEmail"] = null;
            Session["UserPhone"] = null;
            Session["UserLocation"] = null;
            return RedirectToAction("Login");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterViewModel rvm)
        {

            if (ModelState.IsValid)
            {

                var isExist = IsEmailExist(rvm.Email);
                if (isExist)
                {
                    ModelState.AddModelError("EmailExist", "Email already exist");
                    return View(rvm);
                }


                User user = new User();
                {
                    user.name = rvm.Name;
                    user.email = rvm.Email;
                    user.phone = rvm.PhoneNumber;
                    user.address = rvm.Location;
                    user.password = Crypto.HashPassword(rvm.Password);

                    
                    //user.password = rvm.Password;
                    user.user_role = "Customer";
                    user.status = "Non Staff";
                }
                using (CafeteriaDBEntity dc = new CafeteriaDBEntity())
                {
                    dc.Users.Add(user);
                    dc.SaveChanges();


                }

                return RedirectToAction("Login");
            }

            return View();
        }

        [NonAction]
        public bool IsEmailExist(string email)
        {
            using (CafeteriaDBEntity db = new CafeteriaDBEntity())
            {
                var v = db.Users.Where(a => a.email == email).FirstOrDefault();
                return v != null;
            }
        }

        public ActionResult Login(LoginViewModel login)
        {
            if (ModelState.IsValid)
            {
                using (CafeteriaDBEntity db = new CafeteriaDBEntity())
                {
                    

                    var obj = db.Users.Where(a => a.email.Equals(login.Email)).FirstOrDefault();
                    if(obj!= null)

                    {
                        var verified = Crypto.VerifyHashedPassword(obj.password, login.Password);

                        if (obj != null && verified == true)
                        {
                            Session["UserID"] = obj.user_id;
                            Session["UserName"] = obj.name.ToString();
                            Session["UserEmail"] = obj.email.ToString();
                            Session["UserPhone"] = obj.phone.ToString();
                            Session["UserLocation"] = obj.address.ToString();

                            if (obj.user_role == "Admin")
                            {
                                return RedirectToAction("AdminPanel");
                            }
                            else if (obj.user_role == "Chef")
                            {
                                return RedirectToAction("ChefPanel", "Orders");
                            }
                            else if (obj.user_role == "Driver")
                            {
                                return RedirectToAction("DeliveryAgentPanel", "Delivers");
                            }
                            else if (obj.user_role == "Customer")
                            {
                                return RedirectToAction("UserDashboard");
                            }
                            else
                            {
                                TempData["Error"] = "Invalid Login!!";
                            }
                        }
                        TempData["Error"] = "Incorrect Email or Password!!";
                        return View();

                    }
                    TempData["Error"] = "Incorrect Email or Password!!";
                    return View();

                }

            }
            
            return View();
        }

            public PartialViewResult ManageUser(string searchUser, int? page)
        {
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            if (!String.IsNullOrEmpty(searchUser))
            {
                var userList = db.Users.Where(s => s.name.Contains(searchUser) && s.user_role != "Customer").OrderByDescending(x => x.user_id).ToPagedList(pageNumber, pageSize);
                return PartialView(userList);

            }

            var usersList = db.Users.Where(s => s.user_role != "Customer").OrderByDescending(x => x.user_id).ToPagedList(pageNumber, pageSize);


            return PartialView(usersList);
        }


        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "user_id,name,password,user_role,email,phone,address,status")] User user)
        {
            if (ModelState.IsValid)
            {
                user.password = Crypto.HashPassword(user.password);
                db.Users.Add(user);
                db.SaveChanges();
                return RedirectToAction("AdminPanel");
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        public ActionResult EditUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }



        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "user_id,name,password,user_role,email,phone,address,status")] User user)
        {
            if (ModelState.IsValid)
            {
                user.password = Crypto.HashPassword(user.password);
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
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

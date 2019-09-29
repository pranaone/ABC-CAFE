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
    public class MenusController : Controller
    {
        private CafeteriaDBEntity db = new CafeteriaDBEntity();

        // GET: Menus
        public ActionResult Index()
        {
            return View(db.Menus.ToList());
        }

        public PartialViewResult ManageMenu(string searchString, int? page)
        {
            int pageSize = 3;
            int pageNumber = (page ?? 1);

            if (!String.IsNullOrEmpty(searchString))
            {
                var menuslist = db.Menus.Where(s => s.name.Contains(searchString)).OrderByDescending(x => x.menu_id).ToPagedList(pageNumber, pageSize);
                return PartialView(menuslist);

            }

            var menusList = db.Menus.OrderByDescending(x => x.menu_id).ToPagedList(pageNumber, pageSize);

           
            return PartialView(menusList);
        }

        public PartialViewResult MenuListPartial(int? page, string category)
        {
            var pageNumber = page ?? 1;
            var pageSize = 3;
            if (category == "Sri Lankan")
            {
                var menuList = db.Menus.Where(y => y.menu_category == "Sri Lankan").OrderByDescending(x => x.menu_id).ToPagedList(pageNumber, pageSize);
                return PartialView(menuList);
            }
            else if (category == "Chinese")
            {
                var menuList = db.Menus.Where(y => y.menu_category == "Chinese").OrderByDescending(x => x.menu_id).ToPagedList(pageNumber, pageSize);
                return PartialView(menuList);
            }
            else if (category == "Snacks")
            {
                var menuList = db.Menus.Where(y => y.sub_catergory == "Snacks").OrderByDescending(x => x.menu_id).ToPagedList(pageNumber, pageSize);
                return PartialView(menuList);
            }
            else if (category == "Beverages")
            {
                var menuList = db.Menus.Where(y => y.sub_catergory == "Beverages").OrderByDescending(x => x.menu_id).ToPagedList(pageNumber, pageSize);
                return PartialView(menuList);
            }
            else if (category == "Deserts")
            {
                var menuList = db.Menus.Where(y => y.sub_catergory == "Deserts").OrderByDescending(x => x.menu_id).ToPagedList(pageNumber, pageSize);
                return PartialView(menuList);
            }
            else if (category == "Main")
            {
                var menuList = db.Menus.Where(y => y.sub_catergory == "Main").OrderByDescending(x => x.menu_id).ToPagedList(pageNumber, pageSize);
                return PartialView(menuList);
            }
            else
            {
                var menuList = db.Menus.OrderByDescending(x => x.menu_id).ToPagedList(pageNumber, pageSize);
                return PartialView(menuList);
            }

        }
        public ActionResult Search (FormCollection form)
        {
            var item = Request.Form["SearchMenuName"];

            var search = db.Menus.Where(f => f.name.Contains(item)).ToList();
            return View(search);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MenuImage(HttpPostedFileBase file, int? id)
        {

            var menu = db.Menus.Where(x => x.menu_id == id).FirstOrDefault();

            // convert image stream to byte array
            byte[] image = new byte[file.ContentLength];
            file.InputStream.Read(image, 0, Convert.ToInt32(file.ContentLength));

            menu.image = image;

            // save changes to database
            db.SaveChanges();

            return RedirectToAction("AdminPanel", "Users");
        }

        public FileContentResult Image(int id)
        {

            
            var menu = db.Menus.Where(x => x.menu_id == id).FirstOrDefault();
            if (menu.image == null)
            {

                var file = Server.MapPath("~/Content/Food.jpg");
                var img = System.IO.File.ReadAllBytes(file);
                return new FileContentResult(img, MimeMapping.GetMimeMapping(file));

            }
            return new FileContentResult(menu.image, "image/jpeg");

        }



        // GET: Menus/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menus.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // GET: Menus/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Menus/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "menu_id,menu_category,sub_catergory,name,description,price,quantity,image")] Menu menu)
        {
            if (ModelState.IsValid)
            {
                db.Menus.Add(menu);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(menu);
        }

        // GET: Menus/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menus.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // POST: Menus/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "menu_id,menu_category,sub_catergory,name,description,price,quantity")] Menu menu)
        {
            if (ModelState.IsValid)
            {
                db.Entry(menu).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(menu);
        }

        // GET: Menus/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Menu menu = db.Menus.Find(id);
            if (menu == null)
            {
                return HttpNotFound();
            }
            return View(menu);
        }

        // POST: Menus/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Menu menu = db.Menus.Find(id);
            db.Menus.Remove(menu);
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

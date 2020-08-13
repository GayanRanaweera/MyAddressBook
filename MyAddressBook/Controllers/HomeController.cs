using MyAddressBook.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyAddressBook.Controllers
{
    public class HomeController : Controller
    {
        //Get: /Home/
        public ActionResult Index()
        {
            List<ContactModel> contacts = new List<ContactModel>();
            using (MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                var v = (from a in dc.Contacts
                         join b in dc.Countries on a.CountryID equals b.CountryID
                         join c in dc.States on a.StateID equals c.StateID
                         select new ContactModel
                         {
                             ContactID = a.ContactID,
                             FirstName = a.ContactPersonFname,
                             LastName = a. ContactPersonLname,
                             ContactNo1 = a.ContactNo1,
                             ContactNo2 = a.ContactNo2,
                             EmailID = a.EmailID,
                             Country = b.CountryName,
                             State = c.StateName,
                             Address = a.Address,
                             ImagePath = a.ImagePath
                         }).ToList();
                contacts = v;
            }
            return View(contacts);
        }

        public ActionResult Add()
        {
            //fetch country data
            List<Country> AllCountry = new List<Country>();
            List<State> states = new List<State>();
            // Hre MyAddressBookEntities is our DbContext
            using (MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                AllCountry = dc.Countries.OrderBy(a => a.CountryName).ToList();
                //Not need to fetch state as we dont know witch country user select here
            }

            ViewBag.Country = new SelectList(AllCountry, "CountryID", "CountryName");
            ViewBag.State = new SelectList(states, "StateID", "StateName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(Contact c, HttpPostedFileBase file)
        {
            #region // Fetch Country & state
            List<Country> allCountry = new List<Country>();
            List<State> states = new List<State>();
            using (MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                allCountry = dc.Countries.OrderBy(a => a.CountryName).ToList();
                if(c.CountryID > 0)
                {
                    states = dc.States.Where(a => a.CountryID.Equals(c.CountryID)).OrderBy(a => a.StateName).ToList();
                }
            }
            ViewBag.Country = new SelectList(allCountry, "CountryID", "CountryName", c.CountryID);
            ViewBag.State = new SelectList(states, "StateID", "StateName", c.StateID);
            #endregion
            #region// Validation file if selected
            if (file != null)
            {
                if(file.ContentLength>(512*100))    //512KB
                {
                    ModelState.AddModelError("FileErrorMessage", "File size must be withing 512KB");
                }
                string[] allowedType = new string[] { "image/png", "image/gif", "image/jpeg", "image/jpg" };
                bool isFlileTypeValid = false;
                foreach(var i in allowedType)
                {
                    if(file.ContentType == i.ToString())
                    {
                        isFlileTypeValid = true;
                        break;
                    }
                }
                if (!isFlileTypeValid)
                {
                    ModelState.AddModelError("FileErroeMessage", "Only .png, .gif, .jpg file type allowed");
                }
            }
            #endregion
            #region // Validate Model and Save to DB
            if (ModelState.IsValid)
            {
                //save here
                if(file != null)
                {
                    string savePath = Server.MapPath("~/image");
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    file.SaveAs(Path.Combine(savePath, fileName));
                    c.ImagePath = fileName;
                }
                using(MyAddressBookEntities dc = new MyAddressBookEntities())
                {   
                    dc.Contacts.Add(c);
                    dc.SaveChanges(); 
                }
                return RedirectToAction("Index");
            }
            else
            {
                return View(c);
            }
            #endregion

        }

        // Action for fetch state from jquery code
        public JsonResult GetStates(int countryID)
        {
            using (MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                // will off Lazy Loading
                var State = (from a in dc.States
                             where a.CountryID.Equals(countryID)
                              orderby a.StateName
                             select a).ToList();
                return new JsonResult { Data = State, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            }
        }

        public ActionResult View(int id)
        {   // To show contact details of a selected contact
            // Before that we have used Model, now we will extend Contact Class for add Country and State Name feild
            Contact c = null;
            c = GetContact(id);
            return View(c);
        }

        // Function for get Contact by id, Isolated as we will use this multiple time
        public Contact GetContact(int contactID)
        {
            Contact contact = null;
            using(MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                var v = (from a in dc.Contacts
                         join b in dc.Countries on a.CountryID equals b.CountryID
                         join c in dc.States on a.StateID equals c.StateID
                         where a.ContactID.Equals(contactID)
                         select new
                         {
                             a,
                             b.CountryName,
                             c.StateName
                         }).FirstOrDefault();
                if (v != null)
                {
                    contact = v.a;
                    contact.CountryName = v.CountryName;
                    contact.StateName = v.StateName;
                }
            }
            return contact;
        }

         

        public ActionResult Export()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        public ActionResult Login()
        {
            ViewBag.Message = "Your login page.";

            return View();
        }
        public ActionResult Register()
        {
            ViewBag.Message = "Your register page.";

            return View();
        }
    }
}
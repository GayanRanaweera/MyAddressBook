using MyAddressBook.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
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

        // Add
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

        // View
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

        // Edit
        public ActionResult Edit(int id)
        {
            // Fetch Contact
            Contact c = null;
            c = GetContact(id); // GetContact I have created in the previous part

            if (c == null)
            {
                return HttpNotFound("Contact Not Found!");
            }
            // Fetch Country & State
            List<Country> allCountry = new List<Country>();
            List<State> states = new List<State>();
            using (MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                allCountry = dc.Countries.OrderBy(a => a.CountryName).ToList();
                states = dc.States.Where(a => a.CountryID.Equals(c.CountryID)).OrderBy(a => a.StateName).ToList();
            }
            ViewBag.Country = new SelectList(allCountry, "CountryID", "CountryName", c.CountryID);
            ViewBag.State = new SelectList(states, "StateID", "StateName", c.StateID);
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Contact c, HttpPostedFileBase file)
        {
            #region//fetch country & state for dropdown

            List<Country> allCountry = new List<Country>();
            List<State> states = new List<State>();
            using (MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                allCountry = dc.Countries.OrderBy(a => a.CountryName).ToList();
                if (c.CountryID > 0)
                {
                    states = dc.States.Where(a => a.CountryID.Equals(c.CountryID)).OrderBy(a => a.StateName).ToList();
                }
            }
            ViewBag.Country = new SelectList(allCountry, "CountryID", "CountryName", c.CountryID);
            ViewBag.State = new SelectList(states, "StateID", "StateName", c.StateID);

            #endregion
            #region//validate file is selected
            if (file != null)
            {
                if (file.ContentLength > (512 * 1000)) // 512 KB
                {
                    ModelState.AddModelError("FileErrorMessage", "File size must be within 512KB");
                }
                string[] allowedType = new string[] { "image/png", "image/gif", "image/jpg", "image/jpeg" };
                bool isFileTypeValid = false;
                foreach (var i in allowedType)
                {
                    if (file.ContentType == i.ToString())
                    {
                        isFileTypeValid = true;
                        break;
                    }
                }
                if (!isFileTypeValid)
                {
                    ModelState.AddModelError("FileErrorMessage", "Only .png, .gif and .jpg file allowed");
                }
            }
            #endregion
            // Update Contact
            if (ModelState.IsValid)
            {
                //Update Contact
                if (file != null)
                {
                    string savePath = Server.MapPath("~/image");
                    string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    file.SaveAs(Path.Combine(savePath, fileName));
                    c.ImagePath = fileName;
                }

                using (MyAddressBookEntities dc = new MyAddressBookEntities())
                {
                    var v = dc.Contacts.Where(a => a.ContactID.Equals(c.ContactID)).FirstOrDefault();
                    if (v != null)
                    {
                        v.ContactPersonFname = c.ContactPersonFname;
                        v.ContactPersonLname = c.ContactPersonLname;
                        v.ContactNo1 = c.ContactNo1;
                        v.ContactNo2 = c.ContactNo2;
                        v.EmailID = c.EmailID;
                        v.CountryID = c.CountryID;
                        v.StateID = c.StateID;
                        v.Address = c.Address;
                        if (file != null)
                        {
                            v.ImagePath = c.ImagePath;
                        }
                    }
                    dc.SaveChanges();
                }
                return RedirectToAction("Index");
            }
            else
            {
                return View(c);
            }
        }

        // Delete
        public ActionResult Delete(int id)
        {
            // Fetch Contact
            Contact c = null;
            c = GetContact(id);
            return View(c);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Delete")] // Here Action Name is required as we can not make same signature for Get & Post Method
        public ActionResult DeletrConfirm(int id)
        {
            using (MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                var contact = dc.Contacts.Where(a => a.ContactID.Equals(id)).FirstOrDefault();
                if (contact != null)
                {
                    dc.Contacts.Remove(contact);
                    dc.SaveChanges();
                    return RedirectToAction("Index");
                }
                else
                {
                    return HttpNotFound("Contact Not Found!");
                }
            }
        }

        public ActionResult Export()
        {
            List<ContactModel> allContacts = new List<ContactModel>();
            using (MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                var v = (from a in dc.Contacts
                         join b in dc.Countries on a.CountryID equals b.CountryID
                         join c in dc.States on a.StateID equals c.StateID
                         select new ContactModel
                         {
                             ContactID = a.ContactID,
                             FirstName = a.ContactPersonFname,
                             LastName = a.ContactPersonLname,
                             ContactNo1 = a.ContactNo1,
                             ContactNo2 = a.ContactNo2,
                             EmailID = a.EmailID,
                             Country = b.CountryName,
                             State = c.StateName,
                             Address = a.Address,
                             ImagePath = a.ImagePath
                         }).ToList();
                allContacts = v;
            }
            return View(allContacts);
        }

        [HttpPost]
        [ActionName("Export")]
        public FileResult ExportData()
        {
            List<ContactModel> allContacts = new List<ContactModel>();
            using (MyAddressBookEntities dc = new MyAddressBookEntities())
            {
                var v = (from a in dc.Contacts
                         join b in dc.Countries on a.CountryID equals b.CountryID
                         join c in dc.States on a.StateID equals c.StateID
                         select new ContactModel
                         {
                             ContactID = a.ContactID,
                             FirstName = a.ContactPersonFname,
                             LastName = a.ContactPersonLname,
                             ContactNo1 = a.ContactNo1,
                             ContactNo2 = a.ContactNo2,
                             EmailID = a.EmailID,
                             Country = b.CountryName,
                             State = c.StateName,
                             Address = a.Address,
                             ImagePath = a.ImagePath
                         }).ToList();
                allContacts = v;
            }

            var grid = new WebGrid(source: allContacts, canPage: false, canSort: false);
            string exportData = grid.GetHtml(
                            tableStyle:"table table-responsive",
                            columns: grid.Columns(
                                        grid.Column("ContactID", "Contact ID"),
                                        grid.Column("FirstName", "First Name"),
                                        grid.Column("LastName", "Last Name"),
                                        grid.Column("ContactNo1", "Contact No1"),
                                        grid.Column("ContactNo2", "Contact No2"),
                                        grid.Column("EmailID", "Email ID")
                                    )
                                ).ToHtmlString();
            return File(new System.Text.UTF8Encoding().GetBytes(exportData),
                    "application/vnd.ms-excel",
                    "Contacts.xls");

        }
    }
}
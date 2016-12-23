using Library.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Library.Controllers
{
    public class BooksController : Controller
    {
        // GET: books/getbook/id
        public ActionResult GetBook(int id)
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Equals(ConfigurationManager.AppSettings["adminUserName"].ToString()))
                return View("GetBookDescriptionHistory", BookRepository.GetBook(id));

            return View("GetBookDescription", BookRepository.GetBook(id));
        }
    }
}
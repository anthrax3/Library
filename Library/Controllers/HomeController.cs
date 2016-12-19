﻿using Library.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Library.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name=="librarytesttask@gmail.com")
                return View("AdminIndex");

            if (User.Identity.IsAuthenticated)
                return View("UserIndex");

            return View("NotUserIndex");
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }
    }
}
﻿using Library.Models;
using Library.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Library.Controllers.Api
{
    public class BookHistoryController : ApiController
    {
        [HttpGet]
        public List<BookHistory> GetBookHistory(int id)
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Equals(ConfigurationManager.AppSettings["adminUserName"].ToString()))
                return TransactionRepository.getAllBookTransactions(id);
            return null;
        }
    }
}

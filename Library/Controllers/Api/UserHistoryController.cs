using Library.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.Models;

namespace Library.Controllers.Api
{
    public class UserHistoryController : ApiController
    {
        [HttpGet]
        public List<int> NotReturnedBooks(string userId)
        {
            return TransactionRepository.getNotReturnedBooksIds(userId);
        }

        [HttpPost]
        public List<UserHistory> UserHistory(string userId)
        {
            return TransactionRepository.getAllUserTransactions(userId);
        }
    }
}

using Library.Models;
using Library.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.Models;
using Library.Identity;
using System.Threading.Tasks;
using System.Threading;

namespace Library.Controllers.Api
{
    public class TransactionController : ApiController
    {
        //Post api/transaction
        [HttpPost]
        public bool TakeBook(Transaction transaction)
        {
            if (User.Identity.IsAuthenticated)
            {
                int bookQuantity = BookRepository.GetBookQuantity(transaction.BookId);
                if (bookQuantity > 0)
                {
                    BookRepository.UpdateBookQuantity(new int[] { transaction.BookId, bookQuantity - 1 });

                    new Thread(() =>
                    {
                        Thread.CurrentThread.IsBackground = true;
                        UserStore userStore = new UserStore();

                        User user = userStore.FindByIdAsync(transaction.UserId).Result;
                        user.sendMail();
                    }).Start();

                    return TransactionRepository.AddTransaction(transaction);
                }
            }
            return false;
        }

        //Put api/transaction
        [HttpPut]
        public bool ReturnBook(Transaction transaction)
        {
            if (User.Identity.IsAuthenticated)
            {
                int bookQuantity = BookRepository.GetBookQuantity(transaction.BookId);
                BookRepository.UpdateBookQuantity(new int[] { transaction.BookId, bookQuantity + 1 });
                return TransactionRepository.UpdateTransaction(transaction);
            }
            return false;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Library.Models;
using Library.Repositories;
using System.Data;
using System.Configuration;

namespace Library.Controllers.Api
{
    public class BooksController : ApiController
    {
        // GET api/books/id
        public FullBook getBooks(int id)
        {
            return BookRepository.GetBook(id);
        }

        // GET api/books
        public List<Book> getBooks()
        {
            return BookRepository.GetBooks();
        }

        //Post api/books
        [HttpPost]
        public int AddBook(FullBook fullBook) {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Equals(ConfigurationManager.AppSettings["adminUserName"].ToString()))
                return BookRepository.AddBook(fullBook);
            return -1;
        }

        //Put api/books
        [HttpPut]
        public bool AddBook(Book updateBook)
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Equals(ConfigurationManager.AppSettings["adminUserName"].ToString()))
            {
                int[] updateData = { updateBook.Id, updateBook.LeftInStock };
                return BookRepository.UpdateBookQuantity(updateData);
            }
            return false;   
        }

        //Delete api/books/id
        [HttpDelete]
        public bool DeleteBook(int id)
        {
            if (User.Identity.IsAuthenticated && User.Identity.Name.Equals(ConfigurationManager.AppSettings["adminUserName"].ToString()))
                return BookRepository.DeleteBook(id);
            return false;
        }
    }
}

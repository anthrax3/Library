using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Library.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int BookId { get; set; }
        public DateTime DateTaken { get; set; }
        public DateTime DateReturned { get; set; }
        public int Action { get; set; }
    }

    public class UserHistory
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime DateTaken { get; set; }
        public DateTime DateReturned { get; set; }
    }

    public class BookHistory
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public DateTime DateTaken { get; set; }
        public DateTime dateReturned { get; set; }
    }
}
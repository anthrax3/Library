using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Library.Models
{
        public class Book
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Authors { get; set; }
            public int LeftInStock { get; set; }
        }

}
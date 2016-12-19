using Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Library.Repositories
{
    public class BookRepository
    {
        private static readonly string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\LibraryDB.mdf;Initial Catalog=LibraryDB;Integrated Security=True;";

        public static FullBook GetBook(int id)
        {
            // Create and open the connection in a using block. This
            // ensures that all resources will be closed and disposed
            // when the code exits.

            string queryString = "SELECT Book.Id, Book.Title, Book.Description, Book.Left_In_Stock, STUFF(" +
                    "(" +
                        "SELECT ', ' + Author.Name AS[text()] " +
                            "FROM BookAuthorConnect " +
                                "LEFT JOIN Author " +
                            "ON Author.Id = BookAuthorConnect.Author_Id " +
                        "WHERE Book.Id = BookAuthorConnect.Book_Id " +
                    "ORDER BY Author.Name " +
                "FOR XML PATH('')" +
                "), 1, 2, '') AS Authors " +
                "FROM Book WHERE Book.Id=@id";

            using (SqlConnection connection =
                    new SqlConnection(connectionString))
            {
                // Create the Command and Parameter objects.
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@id", id);

                // Open the connection in a try/catch block. 
                // Create and execute the DataReader, writing the result
                // set to the console window.
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    FullBook fullBook = null;
                    if (reader.Read())
                    {
                        fullBook = new FullBook
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            Title = reader.GetString(reader.GetOrdinal("Title")),
                            Authors = reader.GetString(reader.GetOrdinal("Authors")).Replace(" ,", ","),
                            Description = reader.GetString(reader.GetOrdinal("Description")),
                            LeftInStock = reader.GetInt32(reader.GetOrdinal("Left_In_Stock")),
                        };
                    }

                    reader.Close();

                    return fullBook;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    connection.Close();
                }

                return null;
            }
        }


        public static bool BookExists(FullBook fullBook, string[] authorsArr, SqlConnection connection)
        {
            string queryGetExistingBooks = "SELECT Book.Id, Book.Title, STUFF(" +
                    "(SELECT ', ' + Author.Name AS[text()] " +
                            "FROM BookAuthorConnect " +
                                "LEFT JOIN Author " +
                            "ON Author.Id = BookAuthorConnect.Author_Id " +
                        "WHERE Book.Id = BookAuthorConnect.Book_Id " +
                    "ORDER BY Author.Name " +
                "FOR XML PATH('')), 1, 2, '') AS Authors " +
                "FROM Book WHERE Book.Title = @Title";

            SqlCommand command = new SqlCommand(queryGetExistingBooks, connection);
            command.Parameters.AddWithValue("@Title", fullBook.Title);

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                var dataTable = new DataTable();

                dataTable.Load(reader);

                reader.Close();

                List<Book> listOfBooks = dataTable.AsEnumerable().Select(m => new Book
                {
                    Id = m.Field<int>("Id"),
                    Title = m.Field<string>("Title"),
                    Authors = m.Field<string>("Authors").Replace(" ,", ","),
                }).ToList();

               
                string sortedAuthors = String.Join(", ", authorsArr);

                Console.WriteLine(sortedAuthors);

                foreach (Book book in listOfBooks)
                {
                    Console.WriteLine(book.Authors);
                    if (book.Authors.Equals(sortedAuthors) || book.Authors.Equals(sortedAuthors + " "))
                        return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return false;
        }

        public static List<Book> GetBooks()
        {
            string queryString = "SELECT Book.Id, Book.Title, Book.Left_In_Stock, STUFF(" +
                    "(" +
                        "SELECT ', ' + Author.Name AS[text()] " +
                            "FROM BookAuthorConnect " +
                                "LEFT JOIN Author " +
                            "ON Author.Id = BookAuthorConnect.Author_Id " +
                        "WHERE Book.Id = BookAuthorConnect.Book_Id " +
                    "ORDER BY Author.Name " +
                "FOR XML PATH('')" +
                "), 1, 2, '') AS Authors " +
                "FROM Book";

            using (SqlConnection connection =
                    new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    var dataTable = new DataTable();

                    dataTable.Load(reader);

                    reader.Close();

                    List<Book> listOfBooks = dataTable.AsEnumerable().Select(m => new Book
                    {
                        Id = m.Field<int>("Id"),
                        Title = m.Field<string>("Title"),
                        Authors = m.Field<string>("Authors") == null ? "" : m.Field<string>("Authors").Replace(" ,", ","),
                        LeftInStock = m.Field<int>("Left_In_Stock"),
                    }).ToList();

                    return listOfBooks;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                finally
                {
                    connection.Close();
                }

                return null;
            }
        }

        public static List<Author> GetAuthors(string[] authorsArr, SqlConnection connection)
        {
            StringBuilder queryGetExistingAuthors = new StringBuilder("SELECT * FROM Author WHERE Author.Name = @AuthorName0");

            for (int i = 1; i < authorsArr.Length; i++)
            {
                queryGetExistingAuthors.Append(" OR Author.Name = @AuthorName").Append(i);
            }

            string sqlQueryString = queryGetExistingAuthors.ToString();
            SqlCommand command = new SqlCommand(sqlQueryString, connection);

            for (int i = 0; i < authorsArr.Length; i++)
            {
                command.Parameters.AddWithValue("@AuthorName" + i, authorsArr[i]);
            }

            List<Author> listOfAuthors = null;

            try
            {
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                var dataTable = new DataTable();

                dataTable.Load(reader);

                reader.Close();

                listOfAuthors = dataTable.AsEnumerable().Select(m => new Author
                {
                    Id = m.Field<int>("Id"),
                    Name = m.Field<string>("Name"),
                }).ToList();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection.Close();
            }

            return listOfAuthors;
        }

        public static int AddBookOnly(FullBook fullBook, SqlConnection connection)
        {
            string queryInsertNewBook = "INSERT INTO Book (Title, Description, Left_In_Stock) " +
                "VALUES(@Title, @Description, @LeftInStock); SELECT CAST(scope_identity() AS int)";

            SqlCommand command = new SqlCommand(queryInsertNewBook, connection);
            command.Parameters.AddWithValue("@Title", fullBook.Title);
            command.Parameters.AddWithValue("@Description", fullBook.Description);
            command.Parameters.AddWithValue("@LeftInStock", fullBook.LeftInStock);

            Int32 bookId = 0;

            try
            {
                connection.Open();
                bookId = (Int32) command.ExecuteScalar();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return bookId;
        }

        public static Author AddAuthor(string author, SqlConnection connection)
        {
            string queryInsertNewAuthor = "INSERT INTO Author (Name) " +
                "VALUES(@AuthorName); SELECT CAST(scope_identity() AS int)";

            SqlCommand command = new SqlCommand(queryInsertNewAuthor, connection);
            command.Parameters.AddWithValue("@AuthorName", author);

            Int32 bookId = 0;
            try
            {
                connection.Open();
                bookId = (Int32)command.ExecuteScalar();
                connection.Close();

                if (bookId > 0)
                {
                    return new Author
                    {
                        Id = bookId,
                        Name = author
                    };
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            return null;
        }

        public static bool AddBookAuthorConnect(int bookId, List<Author> listOfAuthors, SqlConnection connection)
        {
            StringBuilder queryInsertNewConnection = new StringBuilder("INSERT INTO BookAuthorConnect (Book_Id, Author_Id) " +
                "VALUES(@BookId, @AuthorId0)");

            for (int i = 1; i < listOfAuthors.Count; i++)
            {
                queryInsertNewConnection.Append("; INSERT INTO BookAuthorConnect (Book_Id, Author_Id) " +
            "VALUES(@BookId, @AuthorId").Append(i).Append(")");
            }

            SqlCommand command = new SqlCommand(queryInsertNewConnection.ToString(), connection);
            command.Parameters.AddWithValue("@BookId", bookId);

            for (int i = 0; i < listOfAuthors.Count; i++)
                command.Parameters.AddWithValue("@AuthorId" + i, listOfAuthors[i].Id);

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            finally
            {
                connection.Close();
            }
            return false;
        }

        public static int AddBook(FullBook fullBook)
        {
            string[] authorsArr = new string[0];
            if (fullBook.Description == null)
                fullBook.Description = "";

            if (fullBook.Authors != null)
            {
                authorsArr = fullBook.Authors.Split(',');
                for (int i = 0; i < authorsArr.Length; i++)
                {
                    authorsArr[i] = Regex.Replace(authorsArr[i].Trim(), @"\s+", " ");
                }
                Array.Sort(authorsArr);
            }

            using (SqlConnection connection =
                    new SqlConnection(connectionString))
            {
                //Check if the book already exist in the database.
                if (BookExists(fullBook, authorsArr, connection))
                    return -1;


                
                //Check if mentioned authors already exist in the database.
                List<Author> listOfAuthors = GetAuthors(authorsArr, connection);
                if (listOfAuthors == null)
                {
                    listOfAuthors = new List<Author>();
                }

                //Add new authors to the database
                foreach (string author in authorsArr)
                {
                    bool inList = false;

                    foreach (Author authorModel in listOfAuthors)
                    {
                        if (authorModel.Name.Equals(author))
                        {
                            inList = true;
                            break;
                        }
                    }

                    if (inList)
                        continue;

                    listOfAuthors.Add(AddAuthor(author, connection));
                }

                //Insert new book to the database
                int bookId = AddBookOnly(fullBook, connection);

                //Insert connections between book and authors
                bool connectionAdded = false;
                if (bookId > 0)
                    connectionAdded = AddBookAuthorConnect(bookId, listOfAuthors, connection);

                return bookId;
            }
        }

        public static bool UpdateBookQuantity (int[] updateData)
        {
            if (updateData != null && updateData.Length == 2)
            {
                string queryString = "UPDATE Book SET Left_in_Stock = @LeftInStock WHERE Id = @Id";
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.Parameters.AddWithValue("@Id", updateData[0]);
                    command.Parameters.AddWithValue("@LeftInStock", updateData[1]);

                    try
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        return false;
                    }
                    finally
                    {
                        connection.Close();
                    }
                    return true;
                }
            }
            return false;
        }

        public static int GetBookQuantity(int bookId)
        {
            string queryString = "SELECT Book.Left_In_Stock FROM Book WHERE Id = @Id";
            int leftInStock = 0;
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@Id", bookId);
                try
                {
                    connection.Open();
                    leftInStock = (int)command.ExecuteScalar();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return 0;
                }
                finally
                {
                    connection.Close();
                }
                return leftInStock;
            }
     
        }

        public static bool DeleteBook(int id)
        {
            string queryString = "DELETE FROM Book WHERE Id = @Id";
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@Id", id);

                try
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return false;
                }
                finally
                {
                    connection.Close();
                }
                return true;
            }
        }
    }
}
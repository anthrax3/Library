using Library.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace Library.Repositories
{
    public class TransactionRepository
    {
        private static readonly string connectionString = "Data Source=(LocalDb)\\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\\LibraryDB.mdf;Initial Catalog=LibraryDB;Integrated Security=True;";

        public static bool AddTransaction(Transaction transaction)
        {
            string queryInsertNewBook = "INSERT INTO Transactions(Book_Id, User_Id, Date_Taken, Date_Returned) " +
                "VALUES(@BookId, @UserId, @DateTaken, @DateReturned)";
            SqlConnection connection = new SqlConnection(connectionString);

            Guid parsedUserId;
            if (!Guid.TryParse(transaction.UserId, out parsedUserId))
                throw new ArgumentOutOfRangeException("userId", string.Format("'{0}' is not a valid GUID.", new { transaction.UserId }));

            SqlCommand command = new SqlCommand(queryInsertNewBook, connection);
            command.Parameters.AddWithValue("@BookId", transaction.BookId);
            command.Parameters.AddWithValue("@UserId", transaction.UserId);
            command.Parameters.AddWithValue("@DateTaken", DateTime.Now);
            command.Parameters.AddWithValue("@DateReturned", new DateTime(2000,1,1));

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                connection.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            return true;
        }

        //Update only when user returns the book
        //If he somehow took several copies of the same book return the first one taken
        public static bool UpdateTransaction(Transaction transaction)
        {
            SqlConnection connection = null;
            SqlCommand command = null;

            Guid parsedUserId;
            if (!Guid.TryParse(transaction.UserId, out parsedUserId))
                throw new ArgumentOutOfRangeException("userId", string.Format("'{0}' is not a valid GUID.", new { transaction.UserId }));

            if (transaction.Id == -1)
            {
                string queryInsertNewBook = "UPDATE Transactions SET Date_Returned=@DateReturned" +
                " WHERE Id=(SELECT MIN(Id) FROM Transactions WHERE Book_Id=@BookId AND User_Id=@UserId AND Date_Returned=@DateReturnedOld)";
                connection = new SqlConnection(connectionString);
                command = new SqlCommand(queryInsertNewBook, connection);
                command.Parameters.AddWithValue("@BookId", transaction.BookId);
                command.Parameters.AddWithValue("@UserId", parsedUserId);
                command.Parameters.AddWithValue("@DateReturned", DateTime.Now);
                command.Parameters.AddWithValue("@DateReturnedOld", new DateTime(2000,1,1));
            }
            else
            {
                string queryInsertNewBook = "UPDATE Transactions SET Date_Returned=@DateReturned WHERE Id=@Id";
                connection = new SqlConnection(connectionString);
                command = new SqlCommand(queryInsertNewBook, connection);
                command.Parameters.AddWithValue("@Id", transaction.Id);
                command.Parameters.AddWithValue("@DateReturned", DateTime.Now);
            }
                        

            try
            {
                connection.Open();
                command.ExecuteNonQuery();
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return false;
            }
            finally
            {
                connection.Close();
            }
            return true;
        }

        //Check if the user took this book
        public static bool BookTaken(Transaction transaction)
        {
            string queryString = "SELECT * FROM Transactions WHERE User_Id=@UserId AND Book_Id=@BookId";

            using (SqlConnection connection =
                    new SqlConnection(connectionString))
            {
                SqlCommand command = new SqlCommand(queryString, connection);
                command.Parameters.AddWithValue("@UserId", transaction.UserId);
                command.Parameters.AddWithValue("@BookId", transaction.BookId);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    bool bookTaken = false;

                    if (reader.Read())
                        bookTaken = true;

                    reader.Close();

                    return bookTaken;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                return false;
            }
        }

        public static List<UserHistory> getAllUserTransactions(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("userId");

            Guid parsedUserId;
            if (!Guid.TryParse(userId, out parsedUserId))
                throw new ArgumentOutOfRangeException("userId", string.Format("'{0}' is not a valid GUID.", new { userId }));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlString = "SELECT Transactions.Id, Transactions.Date_Taken, Transactions.Date_returned, Book.Title " +
                "FROM Transactions " +
                "JOIN Book ON Book.Id = Transactions.Book_Id " +
                "WHERE User_Id = @UserId";

                SqlCommand command = new SqlCommand(sqlString, connection);
                command.Parameters.AddWithValue("@UserId", parsedUserId);
                //throw new Exception();
                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    var dataTable = new DataTable();

                    dataTable.Load(reader);

                    reader.Close();

                    List<UserHistory> listOfTransactions = dataTable.AsEnumerable().Select(m => new UserHistory
                    {
                        Id = m.Field<int>("Id"),
                        Title = m.Field<string>("Title"),
                        DateTaken = m.Field<DateTime>("Date_Taken"),
                        DateReturned = m.Field<DateTime>("Date_Returned"),
                    }).ToList();
                    return listOfTransactions;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return null;
            }
        }

        public static List<BookHistory> getAllBookTransactions(int bookId)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlString = "SELECT Transactions.Id, Transactions.Date_Taken, Transactions.Date_returned, Users.UserName " +
                "FROM Transactions " +
                "JOIN Users ON Users.Id = Transactions.User_Id " +
                "WHERE Book_Id = @BookId";

                SqlCommand command = new SqlCommand(sqlString, connection);
                command.Parameters.AddWithValue("@BookId", bookId);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    var dataTable = new DataTable();

                    dataTable.Load(reader);

                    reader.Close();

                    List<BookHistory> listOfTransactions = dataTable.AsEnumerable().Select(m => new BookHistory
                    {
                        Id = m.Field<int>("Id"),
                        UserName = m.Field<string>("UserName"),
                        DateTaken = m.Field<DateTime>("Date_Taken"),
                        dateReturned = m.Field<DateTime>("Date_Returned"),
                    }).ToList();
                    return listOfTransactions;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return null;
            }
        }

        public static List<int> getNotReturnedBooksIds(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentNullException("userId");

            Guid parsedUserId;
            if (!Guid.TryParse(userId, out parsedUserId))
                throw new ArgumentOutOfRangeException("userId", string.Format("'{0}' is not a valid GUID.", new { userId }));

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string sqlString = "SELECT * FROM Transactions " +
                "WHERE User_Id = @UserId AND Date_Returned = @ReturnDate";

                SqlCommand command = new SqlCommand(sqlString, connection);
                command.Parameters.AddWithValue("@UserId", userId);
                command.Parameters.AddWithValue("@ReturnDate", new DateTime(2000, 1, 1));

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    var dataTable = new DataTable();

                    dataTable.Load(reader);

                    reader.Close();

                    List<int> listOfBookIds = dataTable.AsEnumerable().Select(m => m.Field<int>("Book_Id")).ToList();
                    return listOfBookIds;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                return null;
            }
        }
    }
}
using Grocery.Core.Data.Helpers;
using Grocery.Core.Interfaces.Repositories;
using Grocery.Core.Models;
using Microsoft.Data.Sqlite;

namespace Grocery.Core.Data.Repositories
{
    public class ProductRepository : DatabaseConnection, IProductRepository
    {
        private readonly List<Product> products = [];
        public ProductRepository()
        {
            {
                CreateTable(@"CREATE TABLE IF NOT EXISTS Product (
                            [Id] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,
                            [Name] NVARCHAR(80) UNIQUE NOT NULL,
                            [Stock] INTEGER NOT NULL,
                            [Date] DATE NOT NULL,
                            [Price] DECIMAL NOT NULL)");
                List<string> insertQueries = [@"INSERT OR IGNORE INTO Product(Name, Stock, Date, Price) VALUES('Melk', 100, '2025-09-25', 0.95)",
                                          @"INSERT OR IGNORE INTO Product(Name, Stock, Date, Price) VALUES('Kaas', 50, '2025-09-30', 7.98)",
                                          @"INSERT OR IGNORE INTO Product(Name, Stock, Date, Price) VALUES('Brood', 200, '2025-09-12', 2.19)",
                                          @"INSERT OR IGNORE INTO Product(Name, Stock, Date, Price) VALUES('Cornflakes', 0, '2025-12-31', 1.48)"];
                InsertMultipleWithTransaction(insertQueries);
                GetAll();
            }
        }
        public List<Product> GetAll()
        {
            products.Clear();
            string selectQuery = "SELECT Id, Name, Stock, date(Date), Price FROM Product";
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    int id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly date = DateOnly.FromDateTime(reader.GetDateTime(3));
                    decimal price = reader.GetDecimal(4);
                    products.Add(new(id, name, stock, date, price));
                }
            }
            CloseConnection();
            return products;
        }

        public Product? Get(int id)
        {
            string selectQuery = $"SELECT Id, Name, Stock, date(Date), Price FROM Product WHERE Id = {id}";
            Product? p = null;
            OpenConnection();
            using (SqliteCommand command = new(selectQuery, Connection))
            {
                SqliteDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    int Id = reader.GetInt32(0);
                    string name = reader.GetString(1);
                    int stock = reader.GetInt32(2);
                    DateOnly date = DateOnly.FromDateTime(reader.GetDateTime(3));
                    decimal price = reader.GetDecimal(4);
                    p = (new(Id, name, stock, date, price));
                }
            }
            CloseConnection();
            return p;
        }

        public Product Add(Product item)
        {
            int recordsAffected;
            string insertQuery = $"INSERT INTO Product(Name, Stock, Date, Price) VALUES(@Name, @Stock, @Date, @Price) Returning RowId;";
            OpenConnection();
            using (SqliteCommand command = new(insertQuery, Connection))
            {
                command.Parameters.AddWithValue("Name", item.Name);
                command.Parameters.AddWithValue("Stock", item.Stock);
                command.Parameters.AddWithValue("Date", item.ShelfLife);
                command.Parameters.AddWithValue("Price", item.Price);

                //recordsAffected = command.ExecuteNonQuery();
                item.Id = Convert.ToInt32(command.ExecuteScalar());
            }
            CloseConnection();
            return item;
        }

        public Product? Delete(Product item)
        {
            string deleteQuery = $"DELETE FROM Product WHERE Id = {item.Id};";
            OpenConnection();
            Connection.ExecuteNonQuery(deleteQuery);
            CloseConnection();
            return item;
        }

        public Product? Update(Product item)
        {
            int recordsAffected;
            string updateQuery = $"UPDATE Product SET Name = @Name, Stock = @Stock, Date = @Date, Price = @Price  WHERE Id = {item.Id};";
            OpenConnection();
            using (SqliteCommand command = new(updateQuery, Connection))
            {
                command.Parameters.AddWithValue("Name", item.Name);
                command.Parameters.AddWithValue("Stock", item.Stock);
                command.Parameters.AddWithValue("Date", item.ShelfLife);
                command.Parameters.AddWithValue("Price", item.Price);

                recordsAffected = command.ExecuteNonQuery();
            }
            CloseConnection();
            return item;
        } 
        public bool NameExists(string name)
        {
            string query = "SELECT COUNT(*) FROM Product WHERE Name = @Name";
            OpenConnection();
            using (SqliteCommand command = new SqliteCommand(query, Connection))
            {
                command.Parameters.AddWithValue("@Name", name);
                var count = Convert.ToInt32(command.ExecuteScalar());
                CloseConnection();
                return count > 0;
            }
        }
    }
}

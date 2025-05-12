using ATC.DataAccess.DataConnection;
using ATC.DataAccess.Models;
using MySql.Data.MySqlClient;

namespace ATC.DataAccess.Repositories
{
    public class UserRepo
    {
        private readonly DatabaseConnection _databaseConnection;

        public UserRepo(DatabaseConnection databaseConnection)
        {
            _databaseConnection = databaseConnection;
        }

        public User GetUserByUsername(string username)
        {
            using var conn = _databaseConnection.GetConnection();
            conn.Open();

            var query = "SELECT * FROM users WHERE username = @username";
            var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@username", username);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new User(
                    reader.GetInt32("id"),
                    reader.GetString("username"),
                    reader.GetString("password_hash"),
                    reader.GetString("role")
                );
            }
            return null;
        }
        
        
    }
}

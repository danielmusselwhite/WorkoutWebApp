using Microsoft.Data.SqlClient;
using System.Threading.Tasks;

namespace WorkoutTracker.Components.Services
{
    /// <summary>
    /// Provides a service for managing user data.
    /// </summary>
    public class UserService
    {
        private readonly string _connectionString;

        /// <summary>
        /// Constructor for the UserService class.
        /// </summary>
        /// <param name="connectionString">The connection string for the SQL database.</param>
        /// <returns>A new UserService object.</returns>
        public UserService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Gets the username of the user with the specified user ID.
        /// </summary>
        /// <param name="userId">The ID of the user to get the username for.</param>
        /// <returns>The username of the user with the specified user ID.</returns>
        /// <remarks>If no user is found, the default value is "Guest".</remarks>
        public async Task<string> GetUserNameAsync(int userId)
        {
            string username = "Guest"; // Default value if no user is found

            string query = "SELECT Username FROM Users WHERE UserID = @UserID";

            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UserID", userId);

                    var result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        username = result.ToString();
                    }
                }
            }

            return username;
        }
    }
}

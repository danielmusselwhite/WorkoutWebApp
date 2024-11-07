using System.Reflection.PortableExecutable;
using System.Data.SqlClient;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Data.SqlClient; 

namespace WorkoutTracker.Components.Authentication
{
    /// <summary>
    /// Provides a simple authentication state provider for managing user authentication state.
    /// </summary>
    public class SimpleAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly IConfiguration _configuration; // Configuration object for accessing appsettings.json
        private static ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity()); // Must be Static so all calls reference the same logged in user

        /// <summary>
        /// Constructor for the SimpleAuthenticationStateProvider class.
        /// </summary>
        /// <param name="configuration">The configuration object for accessing appsettings.json.</param>
        /// <returns>A new SimpleAuthenticationStateProvider object.</returns>
        public SimpleAuthenticationStateProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Gets the current authentication state.
        /// </summary>
        /// <returns>The current authentication state.</returns>
        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        /// <summary>
        /// Checks if the user is authenticated.
        /// </summary>
        /// <returns>True if the user is authenticated, otherwise false.</returns>
        public async Task<bool> IsAuthenticated()
        {
            var authState = await GetAuthenticationStateAsync();
            var isAuthenticated = authState.User.Identity.IsAuthenticated;
            System.Diagnostics.Debug.Print($"IsAuthenticated: {isAuthenticated}");
            return isAuthenticated;
        }

        /// <summary>
        /// Logs in the user with the specified username and password.
        /// </summary>
        /// <param name="username">The username of the user to log in.</param>
        /// <param name="password">The password of the user to log in.</param>
        /// <returns>True if the user is successfully logged in, otherwise false.</returns>
        public async Task<bool> Login(string username, string password)
        {
            // Get the connection string from appsettings.json defining the SQL DB connection
            var connectionString = _configuration.GetConnectionString("ConnectionWorkoutTrackerDB");

            // Create a new SQL connection using the connection string
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync(); // Open the connection

                // Query the Users table for the user with the specified username
                var query = "SELECT UserID, PasswordHash FROM Users WHERE Username = @Username";
                using (var command = new SqlCommand(query, connection))
                {
                    // Add the username parameter to the query
                    command.Parameters.AddWithValue("@Username", username);
                    // Execute the query and read the results
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        // If the reader has rows, read the first row...
                        if (await reader.ReadAsync())
                        {
                            // Get the password hash from the reader
                            var passwordHash = reader.GetString(1);

                            // If the password is verified, create a new ClaimsIdentity for the user
                            if (VerifyPassword(password, passwordHash))
                            {
                                var identity = new ClaimsIdentity(new[]
                                {
                                    new Claim(ClaimTypes.Name, username),
                                    new Claim(ClaimTypes.NameIdentifier, reader.GetInt32(0).ToString())
                                }, "databaseAuth");

                                // Set the current user to the new ClaimsPrincipal and notify the authentication state has changed and return true
                                _currentUser = new ClaimsPrincipal(identity);
                                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
                                return true;
                            }
                        }
                    }
                }
            }

            // If the user is not successfully logged in, return false
            return false;
        }

        /// <summary>
        /// Registers a new user with the specified username and password.
        /// </summary>
        /// <param name="username">The username of the user to register.</param>
        /// <param name="password">The password of the user to register.</param>
        /// <returns>True if the user is successfully registered, otherwise false.</returns>
        public async Task<bool> Register(string username, string password)
        {
            // Get the connection string from appsettings.json defining the SQL DB connection
            var connectionString = _configuration.GetConnectionString("ConnectionWorkoutTrackerDB");

            // Create a new SQL connection using the connection string
            using (var connection = new SqlConnection(connectionString))
            {
                // Open the connection
                await connection.OpenAsync();

                // Check if the username already exists
                var checkQuery = "SELECT COUNT(*) FROM Users WHERE Username = @Username";
                using (var checkCommand = new SqlCommand(checkQuery, connection))
                {
                    // Add the username parameter to the query
                    checkCommand.Parameters.AddWithValue("@Username", username);
                    
                    // Execute the query and read the results
                    int userCount = (int)await checkCommand.ExecuteScalarAsync();

                    // If the username already exists, return false
                    if (userCount > 0)
                    {
                        return false;
                    }
                }

                // If username does not exist, insert the new user
                var insertQuery = "INSERT INTO Users (Username, PasswordHash) VALUES (@Username, @PasswordHash)";
                using (var insertCommand = new SqlCommand(insertQuery, connection))
                {
                    insertCommand.Parameters.AddWithValue("@Username", username);
                    insertCommand.Parameters.AddWithValue("@PasswordHash", BCrypt.Net.BCrypt.HashPassword(password)); // Hash the password before storing it for security
                    await insertCommand.ExecuteNonQueryAsync();
                    return true;
                }
            }
        }

        /// <summary>
        /// Logs out the current user.
        /// </summary>
        /// <returns>Task representing the logout operation.</returns>
        public async Task Logout()
        {
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }

        /// <summary>
        /// Verifies the password against the password hash.
        /// </summary>
        /// <param name="password">The password to verify.</param>
        /// <param name="passwordHash">The password hash to verify against.</param>
        /// <returns>True if the password is verified, otherwise false.</returns>
        /// <remarks>Uses BCrypt.Net to verify the password against the password hash.</remarks>
        private bool VerifyPassword(string password, string passwordHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, passwordHash);
        }

        /// <summary>
        /// Gets the current user ID.
        /// </summary>
        /// <returns>The current user ID.</returns>
        /// <remarks>Uses the NameIdentifier claim to get the user ID.</remarks>
        public int? GetCurrentUserId()
        {
            var userIdClaim = _currentUser.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }
    }
}
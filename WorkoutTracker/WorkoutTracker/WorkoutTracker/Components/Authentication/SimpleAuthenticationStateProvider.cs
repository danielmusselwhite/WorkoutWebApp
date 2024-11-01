using System.Reflection.PortableExecutable;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace WorkoutTracker.Components.Authentication
{
    /// <summary>
    /// Provides a simple authentication state provider for managing user authentication state.
    /// </summary>
    public class SimpleAuthenticationStateProvider : AuthenticationStateProvider
    {
        private static ClaimsPrincipal _currentUser = new ClaimsPrincipal(new ClaimsIdentity());
        /// <summary>
        /// Gets the current authentication state asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation. The task result contains the <see cref="AuthenticationState"/>.</returns>

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            return Task.FromResult(new AuthenticationState(_currentUser));
        }

        public async Task<bool> IsAuthenticated()
        {
            var authState = await GetAuthenticationStateAsync();
            var isAuthenticated = authState.User.Identity.IsAuthenticated;
            System.Diagnostics.Debug.Print($"IsAuthenticated: {isAuthenticated}");
            return isAuthenticated;
        }

        /// <summary>
        /// Logs in a user with the specified username and password.
        /// TODO - PLACEHOLDER - Replace this with a real authentication mechanism
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public async Task Login(string username, string password)
        {
            // Simple check for username and password
            // TODO - PLACEHOLDER - Replace this with a real authentication mechanism

            System.Diagnostics.Debug.Print($"Login attempt: Username: {username}, Password: {password}");

            if (username == "username" && password == "password")
            {
                System.Diagnostics.Debug.Print("User Successfully Logged In");
                var identity = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.Name, username),
                }, "simpleAuth");

                _currentUser = new ClaimsPrincipal(identity); // Update the current user
                NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
            }
            else
            {
                System.Diagnostics.Debug.Print("User Failed To Log In");
            }
        }

        public async Task Logout()
        {
            System.Diagnostics.Debug.Print("Attempting to LogOut");
            _currentUser = new ClaimsPrincipal(new ClaimsIdentity()); // Clear the current user
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_currentUser)));
        }
    }

}

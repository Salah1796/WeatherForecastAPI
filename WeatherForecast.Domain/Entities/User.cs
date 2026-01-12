using System.ComponentModel.DataAnnotations;
using WeatherForecast.Domain.Common;

namespace WeatherForecast.Domain.Entities
{
    /// <summary>
    /// Represents a user in the system.
    /// </summary>
    public class User : AuditableEntity
    {
        /// <summary>
        /// Creates a new user with the specified username and password hash.
        /// </summary>
        /// <param name="username">The username of the user. Cannot be null or empty.</param>
        /// <param name="passwordHash">The hashed password of the user. Cannot be null or empty.</param>
        /// <exception cref="ArgumentNullException">Thrown when username or passwordHash is null or empty.</exception>
        public User(string username, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username), "Username cannot be empty.");

            if (string.IsNullOrWhiteSpace(passwordHash))
                throw new ArgumentNullException(nameof(passwordHash), "Password hash cannot be empty.");

            Username = username;
            PasswordHash = passwordHash;
            FailedLoginAttempts = 0;
            LockoutEnd = null;
        }

        /// <summary>
        /// The unique identifier of the user.
        /// </summary>
        [Key]
        public Guid Id { get; private set; } = Guid.NewGuid();

        /// <summary>
        /// The username of the user.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// The hashed password of the user.
        /// </summary>
        public string PasswordHash { get; private set; }

        /// <summary>
        /// Gets or sets the number of consecutive failed login attempts.
        /// </summary>
        public int FailedLoginAttempts { get; private set; }

        /// <summary>
        /// Gets or sets the date and time when the account lockout ends.
        /// </summary>
        public DateTime? LockoutEnd { get; private set; }

        /// <summary>
        /// Checks if the account is currently locked out.
        /// </summary>
        /// <returns>True if the account is locked, otherwise false.</returns>
        public bool IsLockedOut()
        {
            return LockoutEnd.HasValue && LockoutEnd.Value > DateTime.UtcNow;
        }

        /// <summary>
        /// Increments the failed login attempts counter and locks the account if threshold is reached.
        /// </summary>
        /// <param name="maxAttempts">Maximum allowed failed attempts before lockout.</param>
        /// <param name="lockoutDuration">Duration of the lockout period.</param>
        public void IncrementFailedAttempts(int maxAttempts, TimeSpan lockoutDuration)
        {
            FailedLoginAttempts++;
            if (FailedLoginAttempts >= maxAttempts)
            {
                LockoutEnd = DateTime.UtcNow.Add(lockoutDuration);
            }
        }

        /// <summary>
        /// Resets the failed login attempts counter and clears any lockout.
        /// </summary>
        public void ResetFailedAttempts()
        {
            FailedLoginAttempts = 0;
            LockoutEnd = null;
        }
    }
}

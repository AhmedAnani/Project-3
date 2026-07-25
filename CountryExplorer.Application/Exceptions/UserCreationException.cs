using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Application.Exceptions;

/// <summary>
/// Thrown when user creation fails.
/// </summary>
public class UserCreationException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserCreationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public UserCreationException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="UserCreationException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public UserCreationException(string message, Exception innerException)
        : base(message, innerException) { }
}


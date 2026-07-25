using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Application.Exceptions;

/// <summary>
/// Thrown when a token is invalid, expired, or revoked.
/// </summary>
public class InvalidTokenException : AppException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTokenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InvalidTokenException(string message) : base(message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidTokenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public InvalidTokenException(string message, Exception innerException)
        : base(message, innerException) { }
}

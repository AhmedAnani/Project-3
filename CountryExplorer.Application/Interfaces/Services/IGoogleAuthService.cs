using Google.Apis.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Application.Interfaces.Services;

public interface IGoogleAuthService
{
    Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken);
}

using CountryExplorer.Application.Services.Interfaces;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CountryExplorer.Infrastructure.Services;

public class GoogleAuthService :IGoogleAuthService
{
    private readonly IConfiguration _config;

    public GoogleAuthService(IConfiguration config) => _config = config;

    public async Task<GoogleJsonWebSignature.Payload> VerifyGoogleTokenAsync(string idToken)
    {
        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { _config["Google:ClientId"]! }
        };

        return await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
    }
}

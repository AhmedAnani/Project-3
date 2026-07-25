using System;
using System.Collections.Generic;
using System.Text;

namespace CountryExplorer.Application.DTOs.Auth;

public class RefreshTokenDto
{
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsActive { get; set; }
}

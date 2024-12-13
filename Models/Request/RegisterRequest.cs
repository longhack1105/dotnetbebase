using DotnetBeBase.Models.Dtos;
using DotnetBeBase.Models.Request;
using Microsoft.AspNetCore.Mvc;

namespace DotnetBeBase.Models.Request;

public class RegisterRequest
{
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    /// <summary>
    /// 0:email - 1:phone
    /// </summary>
    public sbyte RegisterType { get; set; }
    public string Password { get; set; }

    public string FullName { get; set; }
    public string? Avatar { get; set; }
    public sbyte Role { get; set; }
}


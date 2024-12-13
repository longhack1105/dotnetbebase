using System.Net.WebSockets;

namespace DotnetBeBase.Models.Dtos
{
    public class AccountDTO
    {
        public string Username { get; set; }
        public sbyte State { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public sbyte Role { get; set; }
        public string Avatar { get; set; }
    }
}

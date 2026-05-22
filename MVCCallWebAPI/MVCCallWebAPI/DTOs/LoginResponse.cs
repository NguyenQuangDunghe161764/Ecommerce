using Microsoft.AspNetCore.Mvc;

namespace MVCCallWebAPI.DTOs
{
    public class LoginResponse
    {
        public string accessToken { get; set; }

        public string refreshToken { get; set; }

        public string userName { get; set; }

        public string role { get; set; }
    }

    public class TokenDetail
    {
        public string value { get; set; }
    }
}

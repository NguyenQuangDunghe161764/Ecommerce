using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace demoWebAPI.Services
{
    // Tiện ích build request URL và xác thực chữ ký theo chuẩn VNPay.
    public class VnPayLibrary
    {
        private readonly SortedDictionary<string, string> _requestData =
            new(StringComparer.Ordinal);

        private readonly SortedDictionary<string, string> _responseData =
            new(StringComparer.Ordinal);

        public void AddRequestData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _requestData[key] = value;
        }

        public void AddResponseData(string key, string value)
        {
            if (!string.IsNullOrEmpty(value))
                _responseData[key] = value;
        }

        // Tạo URL thanh toán đã ký
        public string CreateRequestUrl(string baseUrl, string hashSecret)
        {
            var data = new StringBuilder();
            foreach (var (key, value) in _requestData)
            {
                data.Append(WebUtility.UrlEncode(key))
                    .Append('=')
                    .Append(WebUtility.UrlEncode(value))
                    .Append('&');
            }

            var queryString = data.ToString();
            if (queryString.Length > 0)
                queryString = queryString.Remove(queryString.Length - 1);

            var secureHash = HmacSha512(hashSecret, queryString);
            return $"{baseUrl}?{queryString}&vnp_SecureHash={secureHash}";
        }

        // Xác thực chữ ký trả về từ VNPay
        public bool ValidateSignature(string inputHash, string hashSecret)
        {
            var rawData = new StringBuilder();
            foreach (var (key, value) in _responseData)
            {
                if (key == "vnp_SecureHash" || key == "vnp_SecureHashType")
                    continue;

                rawData.Append(WebUtility.UrlEncode(key))
                    .Append('=')
                    .Append(WebUtility.UrlEncode(value))
                    .Append('&');
            }

            var raw = rawData.ToString();
            if (raw.Length > 0)
                raw = raw.Remove(raw.Length - 1);

            var computed = HmacSha512(hashSecret, raw);
            return computed.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
        }

        public string? GetResponseData(string key) =>
            _responseData.TryGetValue(key, out var v) ? v : null;

        private static string HmacSha512(string key, string input)
        {
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(input);
            using var hmac = new HMACSHA512(keyBytes);
            var hash = hmac.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hash)
                sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            return sb.ToString();
        }
    }
}

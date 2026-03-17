using System.Globalization;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace WebBanNuocMVC.DesignPatterns.FactoryMethod;

public class VNPayService : IPaymentService
{
    private readonly IConfiguration _configuration;

    public VNPayService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string CreatePaymentUrl(int orderId, decimal amount, string orderInfo, string returnUrl)
    {
        var vnpayUrl = _configuration["VNPay:Url"];
        var tmnCode = _configuration["VNPay:TmnCode"];
        var hashSecret = _configuration["VNPay:HashSecret"];

        var vnpay = new VNPayLibrary();

        vnpay.AddRequestData("vnp_Version", "2.1.0");
        vnpay.AddRequestData("vnp_Command", "pay");
        vnpay.AddRequestData("vnp_TmnCode", tmnCode);
        vnpay.AddRequestData("vnp_Amount", ((long)(amount * 100)).ToString());
        vnpay.AddRequestData("vnp_CreateDate", DateTime.Now.ToString("yyyyMMddHHmmss"));
        vnpay.AddRequestData("vnp_CurrCode", "VND");
        vnpay.AddRequestData("vnp_IpAddr", "127.0.0.1");
        vnpay.AddRequestData("vnp_Locale", "vn");
        vnpay.AddRequestData("vnp_OrderInfo", orderInfo);
        vnpay.AddRequestData("vnp_OrderType", "other");
        vnpay.AddRequestData("vnp_ReturnUrl", returnUrl);
        vnpay.AddRequestData("vnp_TxnRef", orderId.ToString());

        return vnpay.CreateRequestUrl(vnpayUrl, hashSecret);
    }

    public bool ValidateCallback(Dictionary<string, string> queryParams)
    {
        var hashSecret = _configuration["VNPay:HashSecret"];
        var vnpay = new VNPayLibrary();

        foreach (var param in queryParams)
        {
            if (!string.IsNullOrEmpty(param.Value) && param.Key.StartsWith("vnp_"))
            {
                vnpay.AddResponseData(param.Key, param.Value);
            }
        }

        var vnpSecureHash = queryParams.ContainsKey("vnp_SecureHash") ? queryParams["vnp_SecureHash"] : "";
        var checkSignature = vnpay.ValidateSignature(vnpSecureHash, hashSecret);

        return checkSignature;
    }
}

public class VNPayLibrary
{
    private readonly SortedList<string, string> _requestData = new SortedList<string, string>(new VNPayCompare());
    private readonly SortedList<string, string> _responseData = new SortedList<string, string>(new VNPayCompare());

    public void AddRequestData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _requestData.Add(key, value);
        }
    }

    public void AddResponseData(string key, string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _responseData.Add(key, value);
        }
    }

    public string GetResponseData(string key)
    {
        return _responseData.TryGetValue(key, out var value) ? value : string.Empty;
    }

    public string CreateRequestUrl(string baseUrl, string vnpHashSecret)
    {
        var data = new StringBuilder();

        foreach (var kv in _requestData)
        {
            if (!string.IsNullOrEmpty(kv.Value))
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        var queryString = data.ToString();

        if (queryString.Length > 0)
        {
            queryString = queryString.Remove(queryString.Length - 1, 1);
        }

        var signData = queryString;
        var vnpSecureHash = HmacSHA512(vnpHashSecret, signData);

        return $"{baseUrl}?{queryString}&vnp_SecureHash={vnpSecureHash}";
    }

    public bool ValidateSignature(string inputHash, string secretKey)
    {
        var data = new StringBuilder();

        foreach (var kv in _responseData)
        {
            if (!string.IsNullOrEmpty(kv.Value) && kv.Key != "vnp_SecureHashType" && kv.Key != "vnp_SecureHash")
            {
                data.Append(WebUtility.UrlEncode(kv.Key) + "=" + WebUtility.UrlEncode(kv.Value) + "&");
            }
        }

        var signData = data.ToString();

        if (signData.Length > 0)
        {
            signData = signData.Remove(signData.Length - 1, 1);
        }

        var myChecksum = HmacSHA512(secretKey, signData);

        return myChecksum.Equals(inputHash, StringComparison.InvariantCultureIgnoreCase);
    }

    private string HmacSHA512(string key, string inputData)
    {
        var hash = new StringBuilder();
        var keyBytes = Encoding.UTF8.GetBytes(key);
        var inputBytes = Encoding.UTF8.GetBytes(inputData);

        using (var hmac = new HMACSHA512(keyBytes))
        {
            var hashValue = hmac.ComputeHash(inputBytes);
            foreach (var b in hashValue)
            {
                hash.Append(b.ToString("x2"));
            }
        }

        return hash.ToString();
    }
}

public class VNPayCompare : IComparer<string>
{
    public int Compare(string? x, string? y)
    {
        if (x == y) return 0;
        if (x == null) return -1;
        if (y == null) return 1;

        var vnpCompare = CompareInfo.GetCompareInfo("en-US");
        return vnpCompare.Compare(x, y, CompareOptions.Ordinal);
    }
}


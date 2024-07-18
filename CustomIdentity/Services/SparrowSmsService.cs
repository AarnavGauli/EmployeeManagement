using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

public class SparrowSmsService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public SparrowSmsService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<string> SendSms(string from, string token, string to, string text)
    {
        string parameters = $"?from={from}&to={to}&text={text}&token={token}";
        //HttpResponseMessage response = await _httpClient.GetAsync($"http://api.sparrowsms.com/v2/sms/{parameters}");
        //string msg = @"http://api.sparrowsms.com/v2/sms?from=CXNepal&to=9848580066&text=Test Message&token=v2_krziSPCh5gLoMZXKqILLIUY8Ut9.mIn9";
        string msg = @"http://api.sparrowsms.com/v2/sms?";
        msg += string.Format("from={0}&", from);//fromm
        msg += string.Format("to={0}&", to);//to
        msg += string.Format("text={0}&", text); //text
        msg += string.Format("token={0}", token);//token


        HttpResponseMessage response = await _httpClient.GetAsync(msg);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadAsStringAsync();
        }
        else
        {
            // Log the error or handle accordingly
            return "error";
        }
        return "ok";
    }
}

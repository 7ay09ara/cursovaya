using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Principal;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

HttpClient client = new HttpClient();

async Task<Token?> LoginOnServer(string? username, string? password)
{
    if(username == null || username.Length == 0 || password == null || password.Length == 0)
    {
        return null;
    }
    string request = "/login?username?="+username+"&password="+password;
    var response = client.PostAsync(request,null).Result;
    if(response.IsSuccessStatusCode)
    {
        return await response.Content.ReadFromJsonAsync<Token>();
    }
    else
    {
        return null;
    }
}
async Task<Token?> SignUpOnServer(string? username, string? password)
{
    if(username == null || username.Length == 0 || password == null || password.Length == 0)
    {
        return null;
    }
    string request = "/register?username = "+username+"&password = "+password;
    var response = client.PostAsync(request,null).Result;
    if (response.IsSuccessStatusCode){return await LoginOnServer(username,password);}
    else{return null;}
}
Token? ChangePassword(string? username, string? newPassword)
{
    if(username == null || username.Length == 0 || newPassword == null || newPassword.Length == 0)
    {
        return null;
    }
    string request = "/change-password?username ="+username+"&newPassword="+newPassword;
    var payload = new
    {
        Login = username,
        NEWPassword = newPassword
    };
    var content = new StringContent(JsonSerializer.Serialize(payload),Encoding.UTF8, "application/json");
    var response = client.PatchAsync(request,content).Result;
    if(response.IsSuccessStatusCode){return response.Content.ReadFromJsonAsync<Token>().Result;}
    else{return null;}
}
string SORT(string?username, SortRequest ints)    
{
        var json = JsonSerializer.Serialize(ints);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var url = "/sort?username = "+username;
        try
        {
            var response = client.PostAsync(url, content).Result;
            if (response.IsSuccessStatusCode)
            {
                return response.Content.ReadAsStringAsync().Result;
            }
            else
            {
                return "Data in unawiable";
            }
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine($"Ошибка при отправке запроса: {e.Message}");
            return "ERROR";
        }
    }
string? GetHistory(string? username)
{
    if(username == null || username.Length == 0)
    {
        return null;
    }
    string request = "/history?username = "+username;
    var response = client.GetAsync(request).Result;
    if(response.IsSuccessStatusCode){return response.Content.ReadAsStringAsync().Result;}
    else{return null;}
}
string? DeleteHistory(string?username)
{
    if(username == null || username.Length == 0)
    {
        return null;
    }
    string request = "/clearhistory?username="+username;
    var response = client.DeleteAsync(request).Result;
    if(response.IsSuccessStatusCode){return response.Content.ReadAsStringAsync().Result;}
    else{return "Something wrong!";}
}
string? GenerateRandomArray(string? username, int? size)
{
    if(username == null || username.Length == 0 || size == null || size == 0)
    {
        return null;
    }
    string request = "/generate?username = "+username + "&size = "+size;
    var response = client.GetAsync(request).Result;
    if (response.IsSuccessStatusCode){return response.Content.ReadAsStringAsync().Result;}
    else{return null;}
}
string? GetSubArray(string? username, int? startIndex, int? endIndex)
{
    if(username == null || username.Length == 0 || startIndex == null || startIndex == 0 || endIndex == null || endIndex == 0)
    {
        return null;
    }
    string request = "/subarray?username = "+username+"&startIndex="+startIndex+"&endIndex="+endIndex;
    var response = client.GetAsync(request).Result;
    if(response.IsSuccessStatusCode){return response.Content.ReadAsStringAsync().Result;}
    else{return null;}
}
string? GetSortedArray(string? username)
{
    if(username == null || username.Length == 0)
    {
        return null;
    }
    string request = "/sorted-array?username = "+username;
    var response = client.GetAsync(request).Result;
    if(response.IsSuccessStatusCode){return response.Content.ReadAsStringAsync().Result;}
    else{return null;}
}
string? AddToArray(string? username, int? index, int? value)
{
    if(username == null || username.Length == 0 || index == null || index == 0 || value == null || value == 0)
    {
        return null;
    }
    string request = "/add-to-array?username = "+username+"&index = "+index+"&value="+value;
    var response = client.PatchAsync(request,null).Result;
    if(response.IsSuccessStatusCode){return response.Content.ReadAsStringAsync().Result;}
    else{return null;}
}
string? DeleteArray(string? username)
{
    if(username == null || username.Length == 0)
    {
        return null;
    }
    string request = "/delete-array?username = "+username;
    var response = client.DeleteAsync(request).Result;
    if(response.IsSuccessStatusCode){return response.Content.ReadAsStringAsync().Result;}
    else{return null;}
}
const string DEFAULT_SERVER_URL = "http://localhost:5000";
Console.WriteLine("Введите URL сервера (http://localhost:5000 по умолчанию)");
string? server_url = Console.ReadLine();

if(server_url==null || server_url.Length ==0 ) server_url = DEFAULT_SERVER_URL;
client.BaseAddress = new Uri(server_url);
try
{
    int? action = null;
    bool IsAuth = false;
    while(action != 0)
    {
        if(IsAuth == false)
        {
            Console.WriteLine("Выберите действие");
            Console.WriteLine("1 - Зарегистрировать нового пользователя");
            Console.WriteLine("2 - Войти в аккаунт");
            Console.WriteLine("0 - Завершить работу приложения");
            action = Convert.ToInt32(Console.ReadLine());
            if(action == 0)
            {
                return;
            }
            else if(action == 1)
            {
                Console.WriteLine("Введите логин и пароль");
                string? username = Console.ReadLine();
                string? password = Console.ReadLine();
                Token? token = await SignUpOnServer(username,password);
                if (token == null)
                {
                    Console.WriteLine("Такой пользователь уже существует");
                    action = null;
                    IsAuth = false;
                    continue;
                }
                else
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.access_token);
                    Console.WriteLine("Успешно!");
                    action = null;
                    IsAuth = true;
                    continue;
                }
            }
            else if(action == 2)
            {
                Console.WriteLine("Введите логин и пароль");
                string? username = Console.ReadLine();
                string? password = Console.ReadLine();
                Token? token = await LoginOnServer(username,password);
                if (token == null)
                {
                    Console.WriteLine("Не удалось авторизоваться");
                    action = null;
                    IsAuth = false;
                    continue;                
                }
                else
                {
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.access_token);
                    action = null;
                    IsAuth = true;
                    Console.WriteLine("Успешно");
                    continue;
                }
            }
        }
    }
}
catch(Exception e){Console.WriteLine(e.Message);}
public struct Token
{
    public required string access_token{get;set;}
    public required string username{get;set;}
}
public struct SortRequest
{
    public int[]? Data { get; set; }
}

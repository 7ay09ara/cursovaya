using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

HttpClient client = new HttpClient();
Token? LoginOnServer(string? username, string? password)
{
    if (username == null ||username.Length == 0 || password == null || password.Length == 0)
    {
        return null;
    }
    string request = "/PS_LogIn?login=" + username + "&password=" + password;
    var response = client.PostAsync(request, null).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("Авторизация прошла успешно!");
        return response.Content.ReadFromJsonAsync<Token>().Result;
    }
    else
    {
        Console.WriteLine("Авторизация не удалась!");
        return null;
    }
}
Token? RegisterOnServer(string? username, string? password)
{
    if(username == null || username.Length == 0 || password == null || password.Length == 0)
    {
        return null;
    }
    string request = "/PS_Register?login="+ username + "&password=" + password;
    var response = client.PostAsync(request, null).Result;
    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("reg suc");
        return response.Content.ReadFromJsonAsync<Token>().Result;
    }
    else
    {
        Console.WriteLine("failed to register");
        return null;
    }
}
string SORT(int[] ints)    
{
        var json = JsonSerializer.Serialize(ints);
        var content = new StringContent(json, Encoding.Unicode);
        var url = "/PS_Body_Post";
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
            return null;
        }
    }
const string DEAFAULTSERVERURL = "http://localhost:5000";
Console.WriteLine("Введите URL сервера (http://localhost:5000 по умолчанию)");
string? server_url = Console.ReadLine();
if(server_url == null || server_url.Length == 0)
{
    server_url = DEAFAULTSERVERURL;
}
try
{
int[] ints = {7,1,6,3};
client.BaseAddress = new Uri(server_url);
Console.WriteLine("Введите логин и пароль:");
string? username = Console.ReadLine();
string? password = Console.ReadLine();
Token? token = LoginOnServer(username,password);
if (token == null)
{
    Console.WriteLine("Попробуйте ещё раз");
    return;
}
var sortedArray = SORT(ints);
if (sortedArray != null)
        {
            Console.WriteLine("Отсортированный массив:");
            foreach (var item in sortedArray)
            {
                Console.Write(item + " ");
            }
        }
}
catch(Exception exp){Console.WriteLine(exp.Message);}

public struct Token
{
    public required string access_token{get;set;}
    public required sbyte username {get;set;} 
}
public struct Task
{
    public required sbyte ints{get;set;}
}

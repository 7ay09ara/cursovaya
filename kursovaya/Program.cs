using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Добавляем аутентификацию JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "WebAppTest",
            ValidAudience = "WebAppTestAudience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("WebAppTestPasswordWebAppTestPasswordWebAppTestPassword"))
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/register", (string username, string password) =>
{
    Services.RegisterUser(username, password);
    var token = Services.GenerateJwtToken(username);
    return Results.Ok(new{Token = token});
});

// Сортировка массива (требуется аутентификация)
app.MapPost("/sort", (string username, [FromBody] SortRequest data) =>
{
    var sortedArray = Services.SortArray(username, data.Data);
    var Sorteddata = sortedArray;
    return Results.Ok("sucsessfuly sorted!");
});

// Получение истории запросов (требуется аутентификация)
app.MapGet("/history", [Authorize] (string username) =>
{
    var history = Services.GetUserHistory(username);
    return Results.Ok(new { History = history });
});
app.MapDelete("/clearhistory", (string username)=>
{
    var clearhistory = Services.DeleteArray(username);
    return Results.Ok();
}
);

// Генерация случайного массива (требуется аутентификация)
app.MapPost("/generate", [Authorize] (string username, int size) =>
{
    var randomArray = Services.GenerateRandomArray(username, size);
    return Results.Ok(new { RandomArray = randomArray });
});

// Получение JWT токена (для аутентификации)
app.MapPost("/login", (string username, string password) =>
{
    if (Services.VerifyPassword(username, password))
    {
        var token = Services.GenerateJwtToken(username);
        return Results.Ok(new { Token = token });
    }
    else{return Results.Unauthorized();}
});
app.MapPatch("/change-password", [Authorize] (string username, string newPassword) =>
{
    Services.ChangePassword(username, newPassword);
    return Results.Ok("Password changed successfully.");
});
app.MapGet("/subarray", [Authorize] (string username, int startIndex, int endIndex) =>
{
    try
    {
        var subArray = Services.GetSubArray(username, startIndex, endIndex);
        return Results.Ok(new { SubArray = subArray });
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});
app.MapGet("/sorted-array", [Authorize] (string username) =>
{
    var sortedArray = Services.GetSortedArray(username);
    return Results.Ok(new { SortedArray = sortedArray });
});
app.MapPatch("/add-to-array", [Authorize] (string username, int index, int value) =>
{
    try
    {
        Services.AddToArray(username, index, value);
        return Results.Ok("Element added successfully.");
    }
    catch (ArgumentOutOfRangeException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});
app.MapDelete("/delete-array", (string username)=>
{
    Services.DeleteArray(username);
    return Results.Ok();
});
app.Run();

// Классы для десериализации запросов
public class RegisterRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public struct SortRequest
{
    public int[]? Data { get; set; }
}

public class GenerateRequest
{
    public string? Username { get; set; }
    public int Size { get; set; }
}

public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}
public static class Services
{
    // Метод сортировки пирамидой
    public static int[] HeapSort(int[] arr)
    {
        int n = arr.Length;

        for (int i = n / 2 - 1; i >= 0; i--)
            Heapify(arr, n, i);

        for (int i = n - 1; i > 0; i--)
        {
            int temp = arr[0];
            arr[0] = arr[i];
            arr[i] = temp;

            Heapify(arr, i, 0);
        }

        return arr;
    }

    private static void Heapify(int[] arr, int n, int i)
    {
        int largest = i;
        int left = 2 * i + 1;
        int right = 2 * i + 2;

        if (left < n && arr[left] > arr[largest])
            largest = left;

        if (right < n && arr[right] > arr[largest])
            largest = right;

        if (largest != i)
        {
            int swap = arr[i];
            arr[i] = arr[largest];
            arr[largest] = swap;

            Heapify(arr, n, largest);
        }
    }
    // Генерация JWT токена
    public static string GenerateJwtToken(string username)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("WebAppTestPasswordWebAppTestPasswordWebAppTestPassword"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: "WebAppTest",
            audience: "WebAppTestAudience",
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: credentials,
            claims: new[] { new Claim(ClaimTypes.Name, username) }
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // Регистрация пользователя
    public static void RegisterUser(string username, string password)
    {
        Database.RegisterUser(username, password);
    }

    // Получение истории запросов пользователя
    public static string[] GetUserHistory(string username)
    {
        return Database.GetUserHistory(username);
    }

    // Удаление истории запросов пользователя
    public static void DeleteUserHistory(string username)
    {
        Database.DeleteUserHistory(username);
    }

    // Изменение пароля пользователя
    public static void ChangePassword(string username, string newPassword)
    {
        Database.ChangePassword(username, newPassword);
    }

    // Сохранение массива и запроса в историю
    public static void SaveArray(string username, string requestType, int[] array)
    {
        Database.SaveArray(username,requestType, array);
    }

    // Сортировка массива и сохранение в историю
    public static int[] SortArray(string username, int[] array)
    {
        var sortedArray = HeapSort(array);
        SaveArray(username,"Sort", sortedArray); // Добавляем тип запроса
        return sortedArray;
    }

    // Получение последнего сохраненного массива
    public static int[] GetSortedArray(string username)
    {
        return Database.GetArray(username);
    }

    // Добавление элемента в массив по индексу и сохранение в историю
    public static void AddToArray(string username, int index, int value)
    {
        var array = Database.GetArray(username);
        if (index < 0 || index > array.Length)
            throw new ArgumentOutOfRangeException(nameof(index));

        var newArray = new int[array.Length + 1];
        Array.Copy(array, 0, newArray, 0, index);
        newArray[index] = value;
        Array.Copy(array, index, newArray, index + 1, array.Length - index);

        SaveArray(username,"AddToArray",newArray); // Добавляем тип запроса
    }

    // Получение части массива от индекса до индекса
    public static int[] GetSubArray(string username, int startIndex, int endIndex)
    {
        var array = Database.GetArray(username);
        if (startIndex < 0 || endIndex >= array.Length || startIndex > endIndex)
            throw new ArgumentOutOfRangeException();

        var subArray = new int[endIndex - startIndex + 1];
        Array.Copy(array, startIndex, subArray, 0, subArray.Length);
        return subArray;
    }

    // Генерация случайного массива и сохранение в историю
    public static int[] GenerateRandomArray(string username, int size)
    {
        var random = new Random();
        var array = new int[size];
        for (int i = 0; i < size; i++)
        {
            array[i] = random.Next(0, 100);
        }

        SaveArray(username, "GenerateRandomArray", array); // Добавляем тип запроса
        return array;
    }

    // Удаление массива пользователя
    public static bool DeleteArray(string username)
    {
        Database.DeleteArray(username);
        return true;
    }
    public static bool VerifyPassword(string username, string password)
    {
        if(Database.VerifyPassword(username,password))
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
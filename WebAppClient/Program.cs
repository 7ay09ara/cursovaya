using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private static HttpClient client = new HttpClient();
    private static string? username = null;
    private static bool isAuthenticated = false; // Переменная для отслеживания авторизации

    static async Task Main(string[] args)
    {
        const string DEFAULT_SERVER_URL = "http://localhost:5000";
        Console.WriteLine("Введите URL сервера (http://localhost:5000 по умолчанию):");
        string? serverUrl = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(serverUrl))
        {
            serverUrl = DEFAULT_SERVER_URL;
        }

        client.BaseAddress = new Uri(serverUrl);

        try
        {
            while (true)
            {
                Console.WriteLine("Выберите действие:");
                Console.WriteLine("1 - Войти в аккаунт");
                Console.WriteLine("2 - Зарегистрировать новый аккаунт");
                Console.WriteLine("0 - Выйти");

                string input = Console.ReadLine();

                if (!int.TryParse(input, out int choice) || choice < 0 || choice > 2)
                {
                    Console.WriteLine("Неверный выбор, попробуйте снова.");
                    continue;
                }

                if (choice == 0)
                {
                    break;
                }

                switch (choice)
                {
                    case 1:
                        await Login();
                        break;
                    case 2:
                        await Signup();
                        break;
                }

                if (isAuthenticated)
                {
                    await MainMenu();
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    private static async Task Login()
    {
        Console.WriteLine("Введите логин:");
        username = Console.ReadLine();

        Console.WriteLine("Введите пароль:");
        string? password = Console.ReadLine();

        var token = await LoginOnServer(username, password);
        if (token.HasValue)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.token);
            isAuthenticated = true; // Устанавливаем флаг авторизации
            Console.WriteLine("Успешный вход!");
        }
        else
        {
            Console.WriteLine("Не удалось войти. Проверьте логин и пароль.");
        }
    }

    private static async Task Signup()
    {
        Console.WriteLine("Введите логин:");
        username = Console.ReadLine();

        Console.WriteLine("Введите пароль:");
        string? password = Console.ReadLine();

        var token = await SignupOnServer(username, password);
        if (token.HasValue)
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token.Value.token);
            isAuthenticated = true; // Устанавливаем флаг авторизации
            Console.WriteLine("Регистрация прошла успешно!");
        }
        else
        {
            Console.WriteLine("Не удалось зарегистрироваться. Возможно, пользователь уже существует.");
        }
    }

    private static async Task MainMenu()
    {
        while (true)
        {
            Console.WriteLine("Выберите действие:");
            Console.WriteLine("1 - Сортировать массив");
            Console.WriteLine("2 - История запросов");
            Console.WriteLine("3 - Сгенерировать случайный массив");
            Console.WriteLine("4 - Получить отсортированный массив");
            Console.WriteLine("5 - Добавить элемент в массив");
            Console.WriteLine("6 - Получить часть массива");
            Console.WriteLine("7 - Удалить массив");
            Console.WriteLine("8 - Сменить пароль");
            Console.WriteLine("9 - Удалить историю запросов");
            Console.WriteLine("0 - Выйти");

            string input = Console.ReadLine();

            if (!int.TryParse(input, out int choice) || choice < 0 || choice > 9)
            {
                Console.WriteLine("Неверный выбор, попробуйте снова.");
                continue;
            }

            if (choice == 0)
            {
                break;
            }

            switch (choice)
            {
                case 1:
                    await SortArray();
                    break;
                case 2:
                    await GetUserHistory();
                    break;
                case 3:
                    await GenerateRandomArray();
                    break;
                case 4:
                    await GetSortedArray();
                    break;
                case 5:
                    await AddToArray();
                    break;
                case 6:
                    await GetSubArray();
                    break;
                case 7:
                    await DeleteArray();
                    break;
                case 8:
                    await ChangePassword();
                    break;
                case 9:
                    await DeleteUserHistory();
                    break;
            }
        }
    }

    private static async Task<Token?> LoginOnServer(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var response = await client.PostAsync($"/login?username={username}&password={password}", null);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<Token>(json);
        }
        return null;
    }

    private static async Task<Token?> SignupOnServer(string? username, string? password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var response = await client.PostAsync($"/register?username={username}&password={password}", null);
        if (response.IsSuccessStatusCode)
        {
            return await LoginOnServer(username, password);
        }
        return null;
    }

    private static async Task ChangePassword()
    {
        Console.WriteLine("Введите старый пароль:");
        string? oldPassword = Console.ReadLine();

        Console.WriteLine("Введите новый пароль:");
        string? newPassword = Console.ReadLine();

        var response = await client.PatchAsync($"/change-password?username={username}&oldPassword={oldPassword}&newPassword={newPassword}", null);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Пароль успешно изменён!");
        }
        else
        {
            Console.WriteLine("Ошибка при изменении пароля.");
        }
    }

    private static async Task SortArray()
    {
        Console.WriteLine("Введите массив чисел через пробел:");
        string? input = Console.ReadLine();
        int[] data = Array.ConvertAll(input.Split(' '), int.Parse);

        var sortRequest = new SortRequest { Data = data };
        var json = JsonSerializer.Serialize(sortRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var response = await client.PostAsync($"/sort?username={username}", content);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Sorted!"+ response.StatusCode);
            //var responseJson = await response.Content.ReadAsStringAsync();
            //var sortedArray = JsonSerializer.Deserialize<SortRequest>(responseJson).Data;
            //Console.WriteLine("Отсортированный массив: " + string.Join(", ", sortedArray));
        }
        else
        {
            Console.WriteLine("Ошибка при сортировке массива: " + response.StatusCode);
        }
    }

    private static async Task GetUserHistory()
    {
        var response = await client.GetAsync($"/history?username={username}");
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var history = JsonSerializer.Deserialize<GetHistory>(responseJson).history;
            Console.WriteLine("История запросов: " + string.Join(", ", history));
        }
        else
        {
            Console.WriteLine("Ошибка при получении истории запросов: " + response.StatusCode);
        }
    }

    private static async Task GenerateRandomArray()
    {
        Console.WriteLine("Введите размер массива:");
        int size = int.Parse(Console.ReadLine());

        var response = await client.PostAsync($"/generate?username={username}&size={size}", null);
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var randomArray = JsonSerializer.Deserialize<RandomArray>(responseJson).randomArray;
            Console.WriteLine("Сгенерированный массив: " + string.Join(", ", randomArray));
        }
        else
        {
            Console.WriteLine("Ошибка при генерации массива: " + response.StatusCode);
        }
    }
    

    private static async Task GetSortedArray()
    {
        var response = await client.GetAsync($"/sorted-array?username={username}");
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var sortedArray = JsonSerializer.Deserialize<GetArrayString>(responseJson);
            Console.WriteLine("Отсортированный массив: " + string.Join(',',sortedArray.sortedArray));
        }
        else
        {
            Console.WriteLine("Ошибка при получении отсортированного массива: " + response.StatusCode);
        }
    }

    private static async Task AddToArray()
    {
        Console.WriteLine("Введите индекс для добавления:");
        int index = int.Parse(Console.ReadLine());

        Console.WriteLine("Введите значение:");
        int value = int.Parse(Console.ReadLine());

        var response = await client.PatchAsync($"/add-to-array?username={username}&index={index}&value={value}", null);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Элемент успешно добавлен.");
        }
        else
        {
            Console.WriteLine("Ошибка при добавлении элемента: " + response.StatusCode);
        }
    }

    private static async Task GetSubArray()
    {
        Console.WriteLine("Введите начальный индекс:");
        int startIndex = int.Parse(Console.ReadLine());

        Console.WriteLine("Введите конечный индекс:");
        int endIndex = int.Parse(Console.ReadLine());

        var response = await client.GetAsync($"/subarray?username={username}&startIndex={startIndex}&endIndex={endIndex}");
        if (response.IsSuccessStatusCode)
        {
            var responseJson = await response.Content.ReadAsStringAsync();
            var subArray = JsonSerializer.Deserialize<GetSubarray>(responseJson).subArray;
            Console.WriteLine("Часть массива: " + string.Join(", ", subArray));
        }
        else
        {
            Console.WriteLine("Ошибка при получении части массива: " + response.StatusCode);
        }
    }
    private static async Task DeleteUserHistory()
    {
        var response = await client.DeleteAsync("/deletehistory?username="+username);
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Успешно Удалено");
        }
        else
        {
            Console.WriteLine("Что-то пошло не так");
        }
    }

    private static async Task DeleteArray()
    {
        var response = await client.DeleteAsync($"/delete-array?username={username}");
        if (response.IsSuccessStatusCode)
        {
            Console.WriteLine("Массив успешно удалён.");
        }
        else
        {
            Console.WriteLine("Ошибка при удалении массива: " + response.StatusCode);
        }
    }
}

// Структуры для сериализации запросов
public struct Token
{
    public string token { get; set; }
    //public string username { get; set; }
}
public struct GetHistory{public string[] history{get;set;}}
public struct GetArrayString{
    public int[] sortedArray {get;set;}
}
public struct SortRequest
{
    public int[] Data { get; set; }
}
public struct GetSubarray
{
    public int[] subArray{get;set;}
}
public struct RandomArray {
    public int[] randomArray { get; set; }
}

public struct GenerateRequest
{
    public int Size { get; set; }
}

public struct AddToArrayRequest
{
    public int Index { get; set; }
    public int Value { get; set; }
}
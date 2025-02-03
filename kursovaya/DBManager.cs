using System;
using Microsoft.Data.Sqlite;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

public static class Database
{
    // Путь к базе данных
    private  static SqliteConnection? connection = null;

    public static bool Connect()
    {
        Console.WriteLine("Connected");
        try
        {
            connection = new SqliteConnection("Data Source=" + "/home/ayanami-rey/Рабочий стол/users.db");
            connection.Open();
            if (connection.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Failed");
            }
        }
        catch (Exception exp){Console.WriteLine(exp.Message);return false;}
        Console.WriteLine("Succesfully");
        return true;
    }
    public static void Disconnect()
    {
        if (connection == null)
        {
            return;
        }
        if (connection.State != System.Data.ConnectionState.Open)
        {
            return;
        }
        connection.Close();
        Console.WriteLine("Disconnected");
    }
    // Проверка подключения к базе данных
    private static void CheckConnection()
    {
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            throw new InvalidOperationException("Database connection is not open.");
        }
    }

    // Хэширование пароля
    private static string HashPassword(string password)
    {
        using (var algorithm = SHA256.Create())
        {
            var bytesHash = algorithm.ComputeHash(Encoding.Unicode.GetBytes(password));
            return Convert.ToBase64String(bytesHash); // Используем Base64 для хранения хэша
        }
    }

    // Регистрация пользователя
    public static void RegisterUser(string login, string password)
    {
        Connect();
        try
        {
            CheckConnection();
            var hashedPassword = HashPassword(password); // Хэшируем пароль
            var command = new SqliteCommand("INSERT INTO users (Login, Password) VALUES (@login, @password)", connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@password", hashedPassword);
            command.ExecuteNonQuery();
        }
        finally
        {
            Disconnect();
        }
    }

    // Проверка пароля пользователя
    public static bool VerifyPassword(string login, string password)
    {
        Connect();
        try
        {
            CheckConnection();
            var command = new SqliteCommand("SELECT Password FROM users WHERE Login = @login", connection);
            command.Parameters.AddWithValue("@login", login);
            var reader = command.ExecuteReader();

            if (reader.Read())
            {
                var storedHash = reader["Password"].ToString();
                var inputHash = HashPassword(password);
                return storedHash == inputHash; // Сравниваем хэши
            }

            return false; // Пользователь не найден
        }
        finally
        {
            Disconnect();
        }
    }

    // Изменение пароля пользователя
    public static void ChangePassword(string login, string newPassword)
    {
        Connect();
        try
        {
            CheckConnection();
            var hashedPassword = HashPassword(newPassword); // Хэшируем новый пароль
            var command = new SqliteCommand("UPDATE users SET Password = @password WHERE Login = @login", connection);
            command.Parameters.AddWithValue("@password", hashedPassword);
            command.Parameters.AddWithValue("@login", login);
            command.ExecuteNonQuery();
        }
        finally
        {
            Disconnect();
        }
    }

    // Получение истории запросов пользователя
    public static string[] GetUserHistory(string login)
    {
        Connect();
        try
        {
            CheckConnection();
            var command = new SqliteCommand("SELECT ArrayData FROM Arrays WHERE UserID = (SELECT ID FROM users WHERE Login = @login)", connection);
            command.Parameters.AddWithValue("@login", login);
            var reader = command.ExecuteReader();
            var history = new System.Collections.Generic.List<string>();
            while (reader.Read())
            {
                history.Add(reader["ArrayData"].ToString());
            }
            return history.ToArray();
        }
        finally
        {
            Disconnect();
        }
    }

    // Удаление истории запросов пользователя
    public static void DeleteUserHistory(string login)
    {
        Connect();
        try
        {
            CheckConnection();
            var command = new SqliteCommand("DELETE FROM Arrays WHERE UserID = (SELECT ID FROM users WHERE Login = @login)", connection);
            command.Parameters.AddWithValue("@login", login);
            command.ExecuteNonQuery();
        }
        finally
        {
            Disconnect();
        }
    }

    // Сохранение массива и запроса в историю
    public static void SaveArray(string login, string requestType, int[] array)
    {
        Connect();
        if (connection == null || connection.State != System.Data.ConnectionState.Open)
        {
            return;
        }
        else{
        try
        {
            CheckConnection();
            var command = new SqliteCommand("INSERT INTO Arrays (UserID, RequestData, ArrayData, ArrayID) VALUES ((SELECT ID FROM users WHERE Login = "+'"'+login+'"'+"), "+'"'+requestType+'"'+","+'"'+string.Join(",",array)+'"'+",(SELECT max(ArrayID)+1 FROM Arrays WHERE UserID = (SELECT ID FROM users WHERE Login = "+'"'+login+'"'+")))",connection);
            command.Parameters.AddWithValue("@login", login);
            command.Parameters.AddWithValue("@requestData", requestType);
            command.Parameters.AddWithValue("@arrayData", string.Join(",",array));
            command.ExecuteNonQuery();
        }
        catch(Exception e){Console.WriteLine(e.Message);}
        finally
        {
            Disconnect();
        }
        }
        
    }

    

    // Получение последнего сохраненного массива пользователя
    public static int[] GetArray(string login)
    {
        Connect();
        try
        {
            CheckConnection();
            var command = new SqliteCommand("SELECT ArrayData FROM Arrays WHERE RequestData = \"Sort\" AND ArrayID = (SELECT max(ArrayID) FROM arrays WHERE RequestData = \"Sort\" AND UserID = (SELECT ID FROM users WHERE Login = "+'"'+login+'"'+"))", connection);
            command.Parameters.AddWithValue("@login", login);
            var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return reader["ArrayData"].ToString().Split(',').Select(int.Parse).ToArray();
            }
            return new int[0];
        }
        finally
        {
            Disconnect();
        }
    }

    // Удаление массива пользователя
    public static void DeleteArray(string login)
    {
        Connect();
        try
        {
            CheckConnection();
            var command = new SqliteCommand("DELETE From arrays WHERE ArrayID = (SELECT ID FROM users where Login = "+'"'+login+'"'+") AND ArrayID = (select max(ArrayID) FROM arrays)", connection);
            command.Parameters.AddWithValue("@login", login);
            command.ExecuteNonQuery();
        }
        finally
        {
            Disconnect();
        }
    }
}
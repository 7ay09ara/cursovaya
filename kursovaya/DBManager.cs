using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

public class DBManager 
{
    private SqliteConnection? Connect = null;
    private string HashPassword(string Password)
    {
        using (var algorithm = SHA256.Create())
        {
            var bytes_hash = algorithm.ComputeHash(Encoding.Unicode.GetBytes(Password));
            return Encoding.Unicode.GetString(bytes_hash);
        }
    }
    public bool ConnectToDatabase(string path)
    {
        Console.WriteLine("Connected");
        try
        {
            Connect = new SqliteConnection("Data Source=" + path);
            Connect.Open();
            if (Connect.State != System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Failed");
            }
        }
        catch (Exception exp){Console.WriteLine(exp.Message);return false;}
        Console.WriteLine("Succesfully");
        return true;
    }
    public void DisconnectFromDB()
    {
        if (Connect == null)
        {
            return;
        }
        if (Connect.State != System.Data.ConnectionState.Open)
        {
            return;
        }
        Connect.Close();
        Console.WriteLine("Disconnected");
    }
    public bool AddUser(string Login, string Password)
    {
 
        if (Connect == null)
        {
            return false;
        }
        if (Connect.State != System.Data.ConnectionState.Open)
        {
            return false;
        }
        string Request = "INSERT INTO users (Login, Password) VALUES ('" + Login + "', '" + HashPassword(Password) + "')";
        var Command = new SqliteCommand(Request, Connect);
        int result = 0;
        try
        {
            result = Command.ExecuteNonQuery();
        }
        catch (Exception exp){Console.WriteLine(exp.Message);return false;}
        if (result == 1)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    public bool CheckUser(string Login, string Password)
    {
        if (Connect == null)
        {
            return false;
        }
        if (Connect.State != System.Data.ConnectionState.Open)
        {
            return false;
        }
        string RequestAdd = "SELECT Login,Password FROM users WHERE Login ='" + Login + "' AND Password = '" + HashPassword(Password) + "'";
        var Command = new SqliteCommand(RequestAdd, Connect);
        try
        {
            var reader = Command.ExecuteReader();
            if (reader.HasRows)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        catch (Exception exp){Console.WriteLine(exp.Message);return false;}
    }
    public bool ChangePassword(string login, string newPassword)
{
    if (Connect == null) return false;
    if (Connect.State != System.Data.ConnectionState.Open) return false;

    string request = "UPDATE user SET Password = @password WHERE Login = @login";
    using var command = new SqliteCommand(request, Connect);
    command.Parameters.AddWithValue("@login", login);
    command.Parameters.AddWithValue("@password", newPassword); // Сохраняем новый пароль без хеширования

    try
    {
        int result = command.ExecuteNonQuery();
        return result == 1; // Возвращаем true, если изменение прошло успешно
    }
    catch (Exception exp)
    {
        Console.WriteLine($"Ошибка: {exp.Message}");
        return false;
    }
}
}
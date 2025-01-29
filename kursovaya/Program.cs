using System.ComponentModel;
using System.Text.Json;
using System.Diagnostics.Eventing.Reader;
using System.Windows.Markup;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authorization;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
bool customLifeTimeValidator(DateTime? notBefore, DateTime? expires, SecurityToken securityToken, TokenValidationParameters validationParameters)
{
    if (expires == null)
    {
        return false;
    }
    return expires > DateTime.UtcNow;
}
builder.Services.AddAuthentication();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => 
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = AuthOptions.ISSUER,
        ValidateAudience = true,
        ValidAudience = AuthOptions.AUDIENCE,
        ValidateLifetime = true,
        LifetimeValidator = customLifeTimeValidator,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = AuthOptions.GetKey()
    };
});
builder.Services.AddAuthorization();
var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
PSWebAdapter PSW = new PSWebAdapter();
DBManager DBM = new DBManager();
app.MapPost("/PS_LogIn", (string login, string password) => 
{
    if (!DBM.CheckUser(login,password))
    {
        return Results.Unauthorized();
    }
    var jwt = new JwtSecurityToken
            (
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                expires: DateTime.UtcNow.AddMinutes(10),
                signingCredentials: new SigningCredentials(AuthOptions.GetKey(), SecurityAlgorithms.HmacSha256));
                var encodedToken = new JwtSecurityTokenHandler().WriteToken(jwt);
                var response = new {access_token = encodedToken, username = login};
                return Results.Ok(response);
}
);
app.MapPatch("/Update_Password", (HttpContext httpContext) => 
{
    var login = httpContext.Request.Query["login"].ToString();
    var oldPassword = httpContext.Request.Query["oldPassword"].ToString();
    var newPassword = httpContext.Request.Query["newPassword"].ToString();
    if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword))
    {
        return Results.BadRequest("Все параметры должны быть указаны.");
    }
    if (!DBM.CheckUser (login, oldPassword))
    {
        return Results.Unauthorized();
    }
    bool isPasswordChanged = DBM.ChangePassword(login, newPassword);
    
    if (!isPasswordChanged)
    {
        return Results.BadRequest("Не удалось изменить пароль.");
    }
    var jwt = new JwtSecurityToken(
        issuer: AuthOptions.ISSUER,
        audience: AuthOptions.AUDIENCE,
        expires: DateTime.UtcNow.AddMinutes(10),
        signingCredentials: new SigningCredentials(
            AuthOptions.GetKey(), SecurityAlgorithms.HmacSha256
        )
    );
    var encodedToken = new JwtSecurityTokenHandler().WriteToken(jwt);
    
    var response = new
    {
        access_token = encodedToken,
        username = login
    };
    return Results.Ok(response);
});
app.MapPost("/PS_Register", (string login, string password) => 
{
    if (DBM.AddUser(login, password))
    {
        return Results.Ok("User " + login +  " registered succesfully");
    }
    else
    {
        return Results.Problem("Failed to register user " + login);
    }
});
app.MapPost("/PS_Body_Post", [Authorize] ([FromBody] PirSor ints) => {
    PSW.sort(ints.values); 
    return ints.values;
});
const string dbpath = "/home/ayanami-rey/Рабочий стол/users.db";
if (!DBM.ConnectToDatabase(dbpath))
{
    Console.WriteLine("Failed to connect " + dbpath );
    Console.WriteLine("Shotdown");
    return;
}
app.Run();
DBM.DisconnectFromDB();
public struct PirSor
{
    public int[] values {get;set;}
}
public class AuthOptions
{
    public const string ISSUER = "WebAppTest";
    public const string AUDIENCE = "WebAppTestAudience";
    public static SymmetricSecurityKey GetKey()
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            "WebAppTestPasswordWebAppTestPasswordWebAppTestPassword"));
    }
}
public class PSWebAdapter
{
        public void sort(int[] arr)
    {
        int n = arr.Length;
        for (int i = n / 2 - 1; i >= 0; i--)
            heapify(arr, n, i);
        for (int i=n-1; i>=0; i--)
        {
            int temp = arr[0];
            arr[0] = arr[i];
            arr[i] = temp;
            heapify(arr, i, 0);
        }
    }
    void heapify(int[] arr, int n, int i)
    {
        int largest = i;

        int l = 2*i + 1;
        int r = 2*i + 2;
        if (l < n && arr[l] > arr[largest])
            largest = l;
        if (r < n && arr[r] > arr[largest])
            largest = r;
        if (largest != i)
        {
            int swap = arr[i];
            arr[i] = arr[largest];
            arr[largest] = swap;
            heapify(arr, n, largest);
        }
    }
}
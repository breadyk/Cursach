using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

RGWebAdapter rg = new RGWebAdapter();

DBManager db = new DBManager();
DBManager textdb = new DBManager();

app.MapGet("/about", () => "Check out this goofy ah cat \n:~-._                                                 _.-~::\n:.~^o._        ________---------________        _.o^~.:.:\n : ::.`?88booo~~~.::::::::...::::::::::::..~~oood88P'.::.:\n :  ::: `?88P .:::....         ........:::::. ?88P' :::. :\n  :  :::. `? .::.            . ...........:::. P' .:::. :\n   :  :::   ... ..  ...       .. .::::......::.   :::. :\n   `  :' .... ..  .:::::.     . ..:::::::....:::.  `: .'\n    :..    ____:::::::::.  . . ....:::::::::____  ... :\n   :... `:~    ^~-:::::..  .........:::::-~^    ~::.::::\n   `.::. `\\   (8)  \\b:::..::.:.:::::::d/  (8)   /'.::::'\n    ::::.  ~-._v    |b.::::::::::::::d|    v_.-~..:::::\n    `.:::::... ~~^?888b..:::::::::::d888P^~...::::::::'\n     `.::::::::::....~~~ .:::::::::~~~:::::::::::::::'\n      `..:::::::::::   .   ....::::    ::::::::::::,'\n        `. .:::::::    .      .::::.    ::::::::'.'\n          `._ .:::    .        :::::.    :::::_.'\n             `-. :    .        :::::      :,-'\n                :.   :___     .:::___   .::\n      ..--~~~~--:+::. ~~^?b..:::dP^~~.::++:--~~~~--..\n        ___....--`+:::.    `~8~'    .:::+'--....___\n      ~~   __..---`_=:: ___gd8bg___ :==_'---..__   ~~\n       -~~~  _.--~~`-.~~~~~~~~~~~~~~~,-' ~~--._ ~~~-\n          -~~            ~~~~~~~~~   _ Seal _  ~~-");
app.MapGet("/current_user", [Authorize](HttpContext context) => {
    if (context.User.Identity == null)
        return Results.BadRequest("No username");
    return Results.Ok(context.User.Identity.Name);
});

app.MapPost("/login", ([FromBody] Auth auth, HttpContext context) => {
    rg.LogIn(auth.login, auth.password, context, db);
    });
app.MapPost("/signup", (string login, string password) =>{
    if (db.AddUser(login, password)) return Results.Ok("User "+login+" registered successfully!");
    return Results.Problem("Failed to register user " + login); 
});
app.MapPost("/update_Password", ([FromBody] Auth auth, HttpContext context) => {
    if (db.UpdatePassword(auth.login, auth.password, auth.newPassword)) return Results.Ok("Password updated successfully!");
    return Results.Problem("Failed to change password for " + auth.login); 
});

const string DB_PATH = "/home/kursach/Документы/users.db";
const string TEXT_DB_PATH = "/home/kursach/Документы/texts.db";

if (!db.ConnectToDB(DB_PATH)){
    Console.WriteLine("Failed to connect to Users DB " + DB_PATH+"\nShutting down");
    return;
}
if (!textdb.ConnectToDB(TEXT_DB_PATH)){
    Console.WriteLine("Failed to connect to Texts DB " + TEXT_DB_PATH+"\nShutting down");
    return;
}

app.MapPost("/text_add", (string newtext) => {
    if (textdb.AddText(newtext)) return Results.Ok($"Added new text: {newtext}");
    else return Results.Problem("Something went wrong");
});

app.MapPatch("/text_edit", (string newtext, int textID) => {
    if(textdb.EditTextNewRC(newtext, textID)) return Results.Ok($"Edited text: {textID}");
    else return Results.Problem("Something went wrong");
});

app.MapDelete("/text_delete", (int textID) => {
    if(textdb.deleteText(textID)) return Results.Ok($"Deleted text {textID}.");
    else return Results.Problem("Something went wrong");
});

app.MapGet("/text_get_single", (int textID) => {
    string text = textdb.getSingle(textID);
    if(text != null)
    {
        return Results.Ok($"Text: {text}");
    }
    else return Results.Problem("Something went wrong");
});
app.MapGet("/text_get_all", () =>{
    List<string> textList = textdb.getAllTexts();
    if(textList != null){
        string textString = "";
        int counter = 1;
        foreach(string text in textList){
            textString+= $"\n{counter}: {text}";
            counter++;
        }
        return Results.Ok(textString);
    }
    else return Results.Problem("Something went wrong");
});
app.MapPost("/text_encrypt", (int textID) => {
    if(textdb.getSingleWithRC != null){
        var result = textdb.getSingleWithRC(textID);
        string text = result.Value.Text;
        int rows = result.Value.Rows;
        int columns = result.Value.Columns;
        string enc = Codec.Encrypt(text, rows, columns);
        textdb.EditTextOldRC(enc, textID);
        return Results.Ok($"Encrypted text {enc}");
    }
    else return Results.Problem("Something went wrong");
});
app.MapPost("/text_decrypt", (int textID) => {
    if(textdb.getSingleWithRC != null){
        var result = textdb.getSingleWithRC(textID);
        string text = result.Value.Text;
        int rows = result.Value.Rows;
        int columns = result.Value.Columns;
        string dec = Codec.Decrypt(text, rows, columns);
        textdb.EditTextOldRC(dec, textID);
        return Results.Ok($"Decrypted text {dec}");
    }
    else return Results.Problem("Something went wrong");
});


app.Run();
db.Disconnect();
textdb.Disconnect();


struct Auth{
    public string login {get; set;}
    public string password {get; set;}
    public string newPassword {get; set;}
}
struct Texts{
    public int textID {get; set;}
    public string newText {get;set;}
}
public struct RGResult{
    public RGResult(int rv){
        random_value = rv;
    }
    public int random_value {get; set;}
}


public class RGWebAdapter{
    private RandomGenerator rg = new RandomGenerator();

    public async Task<IResult> LogIn(string login, string password, HttpContext context, DBManager db){
        if (!db.CheckUser(login, password)){
            return Results.Unauthorized();
        }

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, login) };
        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return Results.Ok(/*response*/);
    }
    public IResult GetValue(){ 
        return Results.Ok(rg.GetValue());
    }
    public IResult GetValue(int lb, int ub){
        if (lb>=ub){
            return Results.Conflict("Low border can't be equal or bigger than up border");
        }
        return Results.Ok(new RGResult(rg.GetValue(lb, ub)));
    }
    
    public IResult UpdateBorder(int lb, int ub){
        if (rg.UpdateBorder(lb, ub)){
            return Results.Ok("Borders are updated");
        }
        return Results.Conflict("Cannot upgrade borders with such values");
    }
}

public class RandomGenerator
{
    private int low_border;
    private int up_border;
    private Random random;

    public RandomGenerator(int lb = 0, int ub = 10)
    {
        low_border = lb;
        up_border = ub;
        random = new Random();
    }

    public int GetValue()
    {
        return random.Next(low_border, up_border);
    }

    public int GetValue(int lb, int ub)
    {
        return random.Next(lb, ub);
    }

    public bool UpdateBorder(int lb, int ub)
    {
        if (ub <= lb){return false;}
        low_border = lb;
        up_border = ub;
        return true;
    }
}

public class Codec
{
    /// <summary>
    /// Шифрует текст методом табличной перестановки.
    /// </summary>
    /// <param name="input">Входной текст для шифрования.</param>
    /// <param name="rows">Количество строк в таблице.</param>
    /// <param name="columns">Количество столбцов в таблице.</param>
    /// <returns>Зашифрованный текст.</returns>
    public static string Encrypt(string input, int rows, int columns)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input text cannot be null or empty.");

        if (rows * columns < input.Length)
            throw new ArgumentException("Table size is too small for the input text.");

        // Заполняем таблицу по строкам
        char[,] table = new char[rows, columns];
        int index = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                table[i, j] = index < input.Length ? input[index++] : ' '; // Заполняем пробелами, если текст короче
            }
        }

        // Читаем таблицу по столбцам справа налево
        StringBuilder encryptedText = new StringBuilder();

        for (int j = columns - 1; j >= 0; j--)
        {
            for (int i = 0; i < rows; i++)
            {
                encryptedText.Append(table[i, j]);
            }
        }

        return encryptedText.ToString();
    }

    /// <summary>
    /// Расшифровывает текст, зашифрованный методом табличной перестановки.
    /// </summary>
    /// <param name="input">Зашифрованный текст.</param>
    /// <param name="rows">Количество строк в таблице.</param>
    /// <param name="columns">Количество столбцов в таблице.</param>
    /// <returns>Расшифрованный текст.</returns>
    public static string Decrypt(string input, int rows, int columns)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input text cannot be null or empty.");

        if (rows * columns < input.Length)
            throw new ArgumentException("Table size is too small for the input text.");

        // Заполняем таблицу по столбцам справа налево
        char[,] table = new char[rows, columns];
        int index = 0;

        for (int j = columns - 1; j >= 0; j--)
        {
            for (int i = 0; i < rows; i++)
            {
                table[i, j] = index < input.Length ? input[index++] : ' ';
            }
        }

        // Читаем таблицу по строкам
        StringBuilder decryptedText = new StringBuilder();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                decryptedText.Append(table[i, j]);
            }
        }

        return decryptedText.ToString().TrimEnd(); // Убираем лишние пробелы
    }
}



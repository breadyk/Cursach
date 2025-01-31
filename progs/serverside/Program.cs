using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie();
builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

AuthOptions ao = new AuthOptions();

DBManager db = new DBManager();
DBManager textdb = new DBManager();

app.MapGet("/about", () => "Check out this goofy ah cat \n:~-._                                                 _.-~::\n:.~^o._        ________---------________        _.o^~.:.:\n : ::.`?88booo~~~.::::::::...::::::::::::..~~oood88P'.::.:\n :  ::: `?88P .:::....         ........:::::. ?88P' :::. :\n  :  :::. `? .::.            . ...........:::. P' .:::. :\n   :  :::   ... ..  ...       .. .::::......::.   :::. :\n   `  :' .... ..  .:::::.     . ..:::::::....:::.  `: .'\n    :..    ____:::::::::.  . . ....:::::::::____  ... :\n   :... `:~    ^~-:::::..  .........:::::-~^    ~::.::::\n   `.::. `\\   (8)  \\b:::..::.:.:::::::d/  (8)   /'.::::'\n    ::::.  ~-._v    |b.::::::::::::::d|    v_.-~..:::::\n    `.:::::... ~~^?888b..:::::::::::d888P^~...::::::::'\n     `.::::::::::....~~~ .:::::::::~~~:::::::::::::::'\n      `..:::::::::::   .   ....::::    ::::::::::::,'\n        `. .:::::::    .      .::::.    ::::::::'.'\n          `._ .:::    .        :::::.    :::::_.'\n             `-. :    .        :::::      :,-'\n                :.   :___     .:::___   .::\n      ..--~~~~--:+::. ~~^?b..:::dP^~~.::++:--~~~~--..\n        ___....--`+:::.    `~8~'    .:::+'--....___\n      ~~   __..---`_=:: ___gd8bg___ :==_'---..__   ~~\n       -~~~  _.--~~`-.~~~~~~~~~~~~~~~,-' ~~--._ ~~~-\n          -~~            ~~~~~~~~~   _ Seal _  ~~-");
app.MapGet("/current_user", [Authorize](HttpContext context) => {
    if (context.User.Identity == null)
        return Results.BadRequest("No username");
    return Results.Ok(context.User.Identity.Name);
});

app.MapPost("/login", async ([FromBody] Auth auth, HttpContext context) => {
    bool result = await ao.LogIn(auth.login, auth.password, context, db);
    if (!result){
        return Results.Unauthorized();
    }
    return Results.Ok();
    });
app.MapPost("/signup", ([FromBody] Auth auth) =>{
    if (db.AddUser(auth.login, auth.password)) return Results.Ok("User "+auth.login+" registered successfully!");
    return Results.Problem("Failed to register user " + auth.login); 
});
app.MapPost("/update_password", ([FromBody] Auth auth, HttpContext context) => {
    return ao.UpdatePassword(auth.login, auth.password, auth.newPassword, context, db).Result;
});
app.MapGet("/get_user_logs", (string login) =>{
    var jsonResponse = db.GetUserLogs(login);
    if (jsonResponse == null) return Results.Problem("Unable to get user logs");
    return Results.Json(jsonResponse);
});
app.MapDelete("/delete_all_logs", () => {
    db.EraseLogs();
    Results.Ok();
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

app.MapPost("/text_add", ([FromBody] Texts text) => {
    if (textdb.AddText(text.newtext)){
        db.AddLog(text.logLogin, "Added new text");
        return Results.Ok($"Added new text: {text.newtext}");
    }
    else return Results.Problem("Something went wrong");
});

app.MapPatch("/text_edit", ([FromBody] Texts text) => {
    if(textdb.EditTextNewRC(text.newtext, text.textID)){
        db.AddLog(text.logLogin, $"Edited Text {text.textID}");
        return Results.Ok($"Edited text: {text.textID}");
    } 
    else return Results.Problem("Something went wrong");
});

app.MapDelete("/text_delete", (int textID, string login) => {
    if(textdb.deleteText(textID)){
        db.AddLog(login, $"Deleted text {textID}");
        return Results.Ok($"Deleted text {textID}.");
    }
    else return Results.Problem("Something went wrong");
});

app.MapGet("/text_get_single", (string login, int textID) => {
    string text = textdb.getSingle(textID);
    if(text != null)
    {
        db.AddLog(login, "Got text");
        return Results.Json(text);
    }
    else return Results.Problem("Something went wrong");
});
app.MapGet("/text_get_all", (string login) => {
    List<string> textList = textdb.getAllTexts();
    if (textList != null)
    {
        var jsonResponse = new Dictionary<string, string>();
        int counter = 1;

        foreach (string text in textList)
        {
            jsonResponse.Add($"text{counter}", text);
            counter++;
        }
        db.AddLog(login, "Got all texts");
        return Results.Json(jsonResponse);
    }
    else
    {
        var errorResponse = new { error = "Something went wrong" };
        return Results.Json(errorResponse, statusCode: 500);
    }
});
app.MapPost("/text_encrypt", ([FromBody] Texts texts) => {
    if(textdb.getSingleWithRC != null){
        var result = textdb.getSingleWithRC(texts.textID);
        string text = result.Value.Text;
        int rows = result.Value.Rows;
        int columns = result.Value.Columns;
        string enc = Codec.Encrypt(text, rows, columns);
        textdb.EditTextOldRC(enc, texts.textID);
        db.AddLog(texts.logLogin, $"Encrypter text {texts.textID}");
        return Results.Ok($"Encrypted text {enc}");
    }
    else return Results.Problem("Something went wrong");
});
app.MapPost("/text_decrypt", ([FromBody] Texts texts) => {
    if(textdb.getSingleWithRC != null){
        var result = textdb.getSingleWithRC(texts.textID);
        string text = result.Value.Text;
        int rows = result.Value.Rows;
        int columns = result.Value.Columns;
        string dec = Codec.Decrypt(text, rows, columns);
        textdb.EditTextOldRC(dec, texts.textID);
        db.AddLog(texts.logLogin, $"Decrypted text {texts.textID}");
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
    public string newtext {get;set;}

    public string logLogin {get; set;}
}

public class AuthOptions{

    public async Task<bool> LogIn(string login, string password, HttpContext context, DBManager db){
        if (!db.CheckUser(login, password)){
            return false;
        }

        var claims = new List<Claim> { new Claim(ClaimTypes.Name, login) };
        var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity));

        return true;
    }
    public async Task<IResult> UpdatePassword(string login, string password, string newPassword, HttpContext context, DBManager db){
        if (db.UpdatePassword(login, password, newPassword)){
            var claims = new List<Claim> { new Claim(ClaimTypes.Name, login) };
            var claimsIdentity = new ClaimsIdentity(claims, "Cookies");

            await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity));

            return Results.Ok("Password updated successfully!");
        }
        return Results.BadRequest($"Failed to change password for {login}");
    }
}

public class Codec
{
    public static string Encrypt(string input, int rows, int columns)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input text cannot be null or empty.");

        if (rows * columns < input.Length)
            throw new ArgumentException("Table size is too small for the input text.");

        char[,] table = new char[rows, columns];
        int index = 0;

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                table[i, j] = index < input.Length ? input[index++] : ' ';
            }
        }

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

    public static string Decrypt(string input, int rows, int columns)
    {
        if (string.IsNullOrEmpty(input))
            throw new ArgumentException("Input text cannot be null or empty.");

        if (rows * columns < input.Length)
            throw new ArgumentException("Table size is too small for the input text.");

        char[,] table = new char[rows, columns];
        int index = 0;

        for (int j = columns - 1; j >= 0; j--)
        {
            for (int i = 0; i < rows; i++)
            {
                table[i, j] = index < input.Length ? input[index++] : ' ';
            }
        }

        StringBuilder decryptedText = new StringBuilder();

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                decryptedText.Append(table[i, j]);
            }
        }

        return decryptedText.ToString().TrimEnd();
    }
}
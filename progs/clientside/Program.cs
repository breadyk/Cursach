using System.Net;
using System.Text;
using System.Text.Json;


CookieContainer cookies = new CookieContainer();
HttpClientHandler handler= new HttpClientHandler();
HttpClient client = new HttpClient(handler);
handler.CookieContainer = cookies; 

bool LogInOnServer(string? username, string? passWord){
    if (username == null || username.Length == 0 || passWord == null || passWord.Length == 0){
        return false;
    }

    string request = $"/login";

    var json_data = new {
        login = username,
        password = passWord
    };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PostAsync(request, content).Result;
    if (response.IsSuccessStatusCode){
        Console.WriteLine("Authorized successfully!");
        IEnumerable<Cookie> response_Cookies = cookies.GetAllCookies();
        foreach (Cookie cookie in response_Cookies){
            Console.WriteLine($"{cookie.Name}: {cookie.Value}");
        }
        return true;
    }
    else{
        Console.WriteLine("Authorization failed.");
        return false;
    }

}

bool SignUpOnServer(string? username, string? passWord){
    if (username == null || username.Length == 0 || passWord == null || passWord.Length == 0){
        return false;
    }

    string request = "/signup";
    var json_data = new {
        login = username,
        password = passWord
    };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PostAsync(request, content).Result;
    if (response.IsSuccessStatusCode){
        Console.WriteLine("Signed up successfully!");
        return true;
    }
    else{
        Console.WriteLine("SignUp failed.");
        return false;
    }

}

bool UpdatePassword(string username, string passWord, string newpassword){
    if (username == null || username.Length == 0 || passWord == null || passWord.Length == 0 || newpassword == null || newpassword.Length == 0){
        return false;
    }
    string request = "/update_password";
    var json_data = new {
        login = username,
        password = passWord,
        newPassword = newpassword
    };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PostAsync(request, content).Result;
    if (response.IsSuccessStatusCode){
        Console.WriteLine("Password has been successfully changed!");
        return true;
    }
    else{
        Console.WriteLine("Failed to change the password.");
        return false;
    }
}

bool GetUserLogs(string username){
    string request = $"/get_user_logs?login={username}";
    var response = client.GetAsync(request).Result;

    if (response.IsSuccessStatusCode)
    {
        var jsonString = response.Content.ReadAsStringAsync().Result;

        var texts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

        if (texts != null)
        {
            foreach (var entry in texts)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value}");
            }
        }
        else
        {
            Console.WriteLine("Error: null or wrong format.");
        }
        return true;
    }
    else
    {
        Console.WriteLine("Error: unable to get data.");
        return false;
    }
}
bool DeleteAllLogs(){
    string request = "/delete_all_logs";
    var response = client.DeleteAsync(request).Result;
    if (response.IsSuccessStatusCode){
        Console.WriteLine("Erased all logs.");
        return true;
    }
    Console.WriteLine("Failed to erase the logs");
    return false;
}


bool GetSingleText(string username, int textID){
    string request = $"/text_get_single?login={username}&textID={textID}";
    var response = client.GetAsync(request).Result;

    if (response.IsSuccessStatusCode)
    {
        var jsonString = response.Content.ReadAsStringAsync().Result;
        jsonString = JsonSerializer.Deserialize<string>(jsonString);
        Console.WriteLine(jsonString);
        return true;
    }
    Console.WriteLine("Something went wrong.");
    return false;
}

bool GetAllTexts(string username)
{
    string request = $"/text_get_all?login={username}";

    var response = client.GetAsync(request).Result;

    if (response.IsSuccessStatusCode)
    {
        var jsonString = response.Content.ReadAsStringAsync().Result;

        var texts = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(jsonString);

        if (texts != null)
        {
            foreach (var entry in texts)
            {
                Console.WriteLine($"{entry.Key}: {entry.Value}");
            }
        }
        else
        {
            Console.WriteLine("Error: null or wrong format.");
        }

        return true;
    }
    else
    {
        Console.WriteLine("Error: unable to get data.");
        return false;
    }
}

bool AddText(string text, string username){
    string request = "/text_add";

    var json_data = new
    {
        logLogin = username,
        newText = text
    };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PostAsync(request, content).Result;
    if (response.IsSuccessStatusCode)
    {
        string result = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine($"\n{result}");
        return true;
    }
    Console.WriteLine("Something went wrong.");
    return false;
}

bool EditText(string text, int textid, string username){
    string request = "/text_edit";
    var json_data = new
    {
        logLogin = username,
        newText = text,
        textID = textid
    };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PatchAsync(request, content).Result;
    if (response.IsSuccessStatusCode)
    {
        string result = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine($"\n{result}");
        return true;
    }
    Console.WriteLine("Something went wrong.");
    return false;
}

bool DeleteText(string textid, string username){
    
    string request = $"/text_delete?textID={textid}&login={username}";
    var response = client.DeleteAsync(request).Result;
    if (response.IsSuccessStatusCode)
    {
        string result = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine($"\n{result}");
        return true;
    }
    Console.WriteLine("Something went wrong.");
    return false;
}

bool TextEncrypt(int textid, string username){
    string request = "/text_encrypt";
    var json_data = new
    {
        logLogin = username,
        textID = textid
    };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PostAsync(request, content).Result;
    if (response.IsSuccessStatusCode)
    {
        string result = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine($"\n{result}");
        return true;
    }
    Console.WriteLine("Something went wrong.");
    return false;
}

bool TextDecrypt(int textid, string username){
    string request = "/text_decrypt";
    var json_data = new
    {
        logLogin = username,
        textID = textid
    };
    string jsonBody = JsonSerializer.Serialize(json_data);
    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

    var response = client.PostAsync(request, content).Result;
    if (response.IsSuccessStatusCode)
    {
        string result = response.Content.ReadAsStringAsync().Result;
        Console.WriteLine($"\n{result}");
        return true;
    }
    Console.WriteLine("Something went wrong.");
    return false;
}

bool inputValidator(string input){
    if (int.TryParse(input, out int value)){
        return true;
    }
    return false;
}
bool switchChoise(int choise, int low, int up){
    if (choise < low || choise > up){
        return false;
    }
    else return true;
}

void Main()
{
    string login = null;
    const string DEFAULT_SERVER_URL = "http://localhost:5000";
    Console.WriteLine("Enter server URL (Default: http://localhost5000):");
    string? server_url = Console.ReadLine();
    if (server_url == null || server_url.Length == 0){
        server_url = DEFAULT_SERVER_URL;
    }
    try{
        client.BaseAddress = new Uri(server_url);
        goto1:
        Console.WriteLine("You need to authorize:\n1)LogIn\n2)SignUp");
        int choise = int.MinValue;
        while(true){
            string strChoise = Console.ReadLine();
            if(inputValidator(strChoise)) {
                choise = int.Parse(strChoise);
                if(switchChoise(choise, 1,2)){
                    break;
                }
            }
            Console.WriteLine("Enter valid value");
        }
        switch(choise)
        {
            case 1: 
            {
                while(true)
                {
                    Console.WriteLine("Enter Login and Password...");
                    string? username = Console.ReadLine();
                    string? password = Console.ReadLine();
                    if (!LogInOnServer(username, password))
                    {
                        Console.WriteLine("Something went wrong, try again");
                    }
                    else
                    {
                        login = username;
                        break;
                    }
                }
                break;
            }
            case 2:
            {
                while(true)
                {
                    Console.WriteLine("Enter Login and Password...");
                    string? username = Console.ReadLine();
                    string? password = Console.ReadLine();
                    bool signstatus = SignUpOnServer(username, password);
                    if (!signstatus)
                    {
                        Console.WriteLine("Something went wrong, try again");
                    }
                    else break;
                }
                goto goto1;
            }
        }
        while(true){
            Console.WriteLine("\nSelect action:");
            Console.WriteLine("1) Add new text to DB;\n2) Edit text in DB;\n3) Delete text from DB;");
            Console.Write("4) Get single text from DB;\n5) Get ALL texts from DB;\n");
            Console.Write("6) Encrypt text in DB;\n7) Decrypt text in DB;\n");
            Console.Write("8) Get your logs;\n9) Erase EVERYONE'S logs;\n");
            Console.Write("10) Update your password;\n\n11) Exit\n");

            while(true){
                string strChoise = Console.ReadLine();
                if(inputValidator(strChoise)) {
                    choise = int.Parse(strChoise);
                    if(switchChoise(choise, 1,11)){
                        break;
                    }
                }
                Console.WriteLine("Enter valid value");
            }
            switch(choise){
                case 1:{
                    Console.WriteLine("Enter text to add:");
                    AddText(Console.ReadLine(), login);
                    break;  
                }
                case 2: {
                    Console.WriteLine("Enter ID of a text to edit:");
                    int textID = int.Parse(Console.ReadLine());
                    Console.WriteLine("Enter new text:");
                    EditText(Console.ReadLine(),textID, login);
                    break;
                }
                case 3:{
                    Console.WriteLine("Enter ID of a text to delete:");
                    DeleteText(Console.ReadLine(), login);
                    break;
                }
                case 4:{
                    Console.WriteLine("Enter ID of a text to get:");
                    GetSingleText(login, int.Parse(Console.ReadLine()));
                    break;
                }
                case 5:{
                    GetAllTexts(login);
                    break;
                }
                case 6:{
                    Console.WriteLine("Enter ID of a text to encrypt:");
                    TextEncrypt(int.Parse(Console.ReadLine()), login);
                    break;
                }
                case 7:{
                    Console.WriteLine("Enter ID of a text to decrypt:");
                    TextDecrypt(int.Parse(Console.ReadLine()), login);
                    break;
                }
                case 8:{
                    GetUserLogs(login);
                    break;
                }
                case 9:{
                    DeleteAllLogs();
                    break;
                }
                case 10:{
                    Console.WriteLine("Enter your current password:");
                    string curpass = Console.ReadLine();
                    Console.WriteLine("Enter new password:");
                    string newpass = Console.ReadLine();
                    UpdatePassword(login, curpass, newpass);
                    break;
                }
                case 11:{
                    return;
                }

            }
        }
        
    }
    catch(Exception exp){
        Console.WriteLine("Error: " + exp.Message);
    }
}
Main();
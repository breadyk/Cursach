
using System.Net;
using System.Net.Http.Headers;
using System.Reflection.Metadata.Ecma335;
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

    string request = $"/signup";
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

string GetRandom(){
    string request = $"/random";
    var response=client.GetAsync(request).Result;
    if (response.IsSuccessStatusCode){
        return response.Content.ReadAsStringAsync().Result;
    }
    else{
        return "Data is not avialable";
    }
}
bool inputValidator(string input){
    if (int.TryParse(input, out int value)){
        return true;
    }
    return false;
}
bool switchChoise(int choise, int lb, int ub){
    if (choise < lb || choise>ub){
        return false;
    }
    else return true;
}

void Main()
{
    const string DEFAULT_SERVER_URL = "http://localhost:5000";
    Console.WriteLine("Enter server URL (Default: http://localhost5000):");
    string? server_url = Console.ReadLine();
    if (server_url == null || server_url.Length == 0){
        server_url = DEFAULT_SERVER_URL;
    }
    try{
        client.BaseAddress = new Uri(server_url);

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
                    else break;
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
                    if (!SignUpOnServer(username, password))
                    {
                        Console.WriteLine("Something went wrong, try again");
                    }
                    else break;
                }
                break;
            }

        }
    }
    catch(Exception exp){
        Console.WriteLine("Error: " + exp.Message);
    }
}
/*try{
    client.BaseAddress = new Uri(server_url);

    Console.WriteLine("Enter login and password:");
    string? username = Console.ReadLine();
    string? password = Console.ReadLine();

    if (!LogInOnServer(username, password)){
        Console.WriteLine("Can't function without auth, shutting down...");
        return;
    }

    Console.WriteLine(GetRandom());
    }
    catch(Exception exp){
        Console.WriteLine("Error: " + exp.Message);
    }*/

using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.Sqlite;

public class DBManager{
    private SqliteConnection? connection = null;

    private string HashPassword(string password){
        using (var algorithm = SHA256.Create()){
            var bytes_hash = algorithm.ComputeHash(Encoding.Unicode.GetBytes(password));
            return Encoding.Unicode.GetString(bytes_hash);
        }
    }
    public bool ConnectToDB(string path){
        Console.WriteLine("Connecting to DB...");
        try{
            connection = new SqliteConnection("Data Source = " + path);
            connection.Open();

            if(connection.State != System.Data.ConnectionState.Open){
                Console.WriteLine("Failed to connect");
                return false;
            }
        }
        catch(Exception exp){
            Console.WriteLine(exp.Message);
            return false;
        }
        Console.WriteLine("Done!");
        return true;
    }

    public void Disconnect(){
        if(null == connection) return;
        Console.WriteLine("Disconnecting from DB...");
        if(connection.State != System.Data.ConnectionState.Open) return;
        connection.Close();
        Console.WriteLine("Disconnected.");
        
    }
    public bool UpdatePassword(string login, string password, string newPassword){
        if(CheckUser(login, password)){
            string REQUEST = $"INSERT INTO users (Password) VALUES ('{HashPassword(newPassword)}') WHERE Login='{login}'";
            return validateResponse(REQUEST);
        }
        return false;
    }

    public bool AddUser(string login, string password){
        if (null == connection) return false;
        if(connection.State != System.Data.ConnectionState.Open) return false;
        
        string REQUEST = $"INSERT INTO users (Login, Password) VALUES ('{login}', '{HashPassword(password)}')";
        return validateResponse(REQUEST);
    }

    public bool CheckUser(string login, string password){
        if (null == connection) return false;

        if(connection.State != System.Data.ConnectionState.Open) return false;
        string REQUEST = $"SELECT Login, Password FROM users WHERE Login='{login}' AND Password = '{HashPassword(password)}'";
        var command = new SqliteCommand(REQUEST, connection);
        try{
            var reader = command.ExecuteReader();

            if (reader.HasRows) return true;
            else return false;
        }
        catch(Exception exp){
            Console.WriteLine(exp.Message);
            return false;
        }
    } 
    public bool AddText(string newText){
        (int rows, int columns) = TableDimensionCalculator.DetermineDimensions(newText.Length);
        string REQUEST = $"INSERT into Texts (Text, Rows, Columns) VALUES ('{newText}', {rows}, {columns})";
        return validateResponse(REQUEST);
    }
    public bool EditTextNewRC(string newText, int ID){
        (int rows, int columns) = TableDimensionCalculator.DetermineDimensions(newText.Length);
        string REQUEST = $"UPDATE Texts SET Text = '{newText}', Rows = {rows}, Columns = {columns} WHERE ID = {ID}";
        return validateResponse(REQUEST);
    }
    public bool EditTextOldRC(string newText, int ID){
        string REQUEST = $"UPDATE Texts SET Text = '{newText}' WHERE ID = {ID}";
        return validateResponse(REQUEST);
    }
    public bool deleteText(int ID){
        string REQUEST = $"DELETE FROM Texts WHERE ID = {ID}";
        return validateResponse(REQUEST);
    }
    public (string Text, int Rows, int Columns)? getSingleWithRC(int ID) {
    string REQUEST = $"SELECT Text, Rows, Columns FROM Texts WHERE ID = {ID}";
    var command = new SqliteCommand(REQUEST, connection);
    try {
        var reader = command.ExecuteReader();

        if (reader.HasRows) {
            reader.Read();
            string text = reader["Text"].ToString();
            int rows = Convert.ToInt32(reader["Rows"]);
            int columns = Convert.ToInt32(reader["Columns"]);
            return (text, rows, columns);
        } else {
            return null;
        }
    }
    catch (Exception exp) {
        Console.WriteLine(exp.Message);
        return null;
    }
}

    public string getSingle(int ID){
        string REQUEST = $"SELECT Text FROM Texts WHERE ID = {ID}";
        var command = new SqliteCommand(REQUEST, connection);
        try {
            var reader = command.ExecuteReader();

            if (reader.HasRows) {
                reader.Read();
                return reader["Text"].ToString();
            } else {
                return null;
            }
        }
        catch (Exception exp) {
            Console.WriteLine(exp.Message);
            return null;
        }
        
    }
    public List<string> getAllTexts(){
        string REQUEST = "SELECT Text FROM Texts";
        var command = new SqliteCommand(REQUEST, connection);
        var texts = new List<string>();
        try {
            var reader = command.ExecuteReader();

            while (reader.Read()) {
                texts.Add(reader["Text"].ToString()); // Добавляем текст в список
            }

            return texts;
        }
        catch (Exception exp) {
            Console.WriteLine(exp.Message);
            return new List<string>(); // В случае ошибки возвращаем пустой список
        }
    }



    public bool validateResponse(string REQUEST){
        var command = new SqliteCommand(REQUEST, connection);
        int result = 0;
        try{
            result = command.ExecuteNonQuery();
        }
        catch(Exception exp){
            Console.WriteLine(exp.Message);
            return false;
        }

        if(1 == result) return true;
        else return false;
    }
}
public class TableDimensionCalculator
{
    public static (int rows, int columns) DetermineDimensions(int inputLength)
    {
        if (inputLength <= 0)
            throw new ArgumentException("Input length must be greater than zero.");

        // Рассчитываем количество столбцов как ближайшее к квадратному корню
        int columns = (int)Math.Ceiling(Math.Sqrt(inputLength));

        // Рассчитываем минимальное количество строк, чтобы вместить весь текст
        int rows = (int)Math.Ceiling((double)inputLength / columns);

        return (rows, columns);
    }
}
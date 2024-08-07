using Konscious.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using Scrypt;
using PasswordGenerator;
using Spectre.Console;
using System.Runtime.CompilerServices;
using System.Collections;
using System.Security.Cryptography.X509Certificates;


class Dexter
{
    #pragma warning disable CS8602 // Dereference of a possibly null reference.
    #pragma warning disable CS8604 // Possible null reference argument for parameter 's' in 'int int.Parse(string s)'.
    #pragma warning disable CA1416

    private static void remix(){
        
    }
    
    public static void Main(){        
        Console.OutputEncoding = Encoding.UTF8;
        TestMenue();
        //run();
        //Entry();
    }   

    public static string GeneratePassword(int length , int strength, List<string> options )
    {
        string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        string symbols = strength >= 2 ?  "♠♣♥♦♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼" : "";
        string multiLang =  strength == 3 ? "абвгдеёжзийклмнопрстуфхцчшщъыьэюяαβγδεζηθικλμνξοπρστυφχψωאבגדהוזחטיכלמנסעפצקרשת ب ت ث ج ح خ د ذ ر ز س ش ص ض ط ظ ع غ ف ق ك ل م ن ه و ي" : "";   
        string uppercase =  options[0] == null ? "" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        string numbers = options[1] == null ? "" : "0123456789๑๒๓๔๕๖๗๘๙๑๐一二三四五六七八九十";   //english , thai and japanese numbers
        string specChars = options[2] == null ? "": "!@#$%^&*()_+{}|:<>?-=[];',./";
        string ambiguous =  options[3] == null ? "": "il1Lo0O";
        bool noDupes = options[4] != null;
        bool noSeq = options[5] != null;

        //future enhancement with custom codex for character representations not available on standard keyboard
            // Hashtable codex = new(); 
            // codex.Add("p", "/>");
            // codex.Add("e", "£");

        string allChars = Lowercase + uppercase + numbers + specChars + symbols + multiLang;

        foreach(char am in ambiguous){
         allChars = allChars.Replace(am.ToString(), "");
        }

        StringBuilder password = new();

        Random random = new(GetSecureSeed());

        while (password.Length <= length)
        {
            char nextChar = allChars[random.Next(allChars.Length)];

            if (noDupes && password.ToString().Contains(nextChar)) continue;
        
            if (noSeq && password.Length > 0)
            {
                char lastChar = password[password.Length - 1];
                if(IsSequential(lastChar, nextChar)) continue;          
            }

            password.Append(nextChar);
        }
        return password.ToString();
    }

    public static bool IsSequential(char a , char b ){
        return Math.Abs(a-b) == 1;
    }   

    public static int GetSecureSeed(){
        using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[64];
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
    }

    public static byte[] GenerateSalt()
    {
        byte[] salt = new byte[128];
        using (var rng = RandomNumberGenerator.Create())
        {
          rng.GetBytes(salt);
        }
        return salt;
    }

    public static string getHash(string password)
    {
        password += getPepper();

        var algo = AnsiConsole.Prompt(
             new SelectionPrompt<string>()
            .Title("How Should Dexter hide your Secrete")
            .AddChoices( new [] {
                  "1. Argon2id",
                  "2. SCrypt"
            }));

        return algo switch
        {
            "1. Argon2id" => getArgon2Hash(password, GenerateSalt()),
            "2. SCrypt" => getSCryptHash(password, GenerateSalt()),
            _ => throw new Exception("Invalid input, please enter a number between 1 and 2"),
        };
    }

    public static string getArgon2Hash(string password, byte[] salt)
    {
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = 14,
            MemorySize = 8192,
            Iterations = 4
        };
        return Convert.ToBase64String(argon2.GetBytes(64));
    }

    public static string getSCryptHash(string password, byte[] saltnPepper)
    {
        string saltString  = Convert.ToBase64String(saltnPepper);
        string saltedPass =  saltString + password;
      
        ScryptEncoder scryptEncoder = new ScryptEncoder(); // Create an instance of ScryptEncoder
        string hashedPassword =  scryptEncoder.Encode(saltedPass);// Call the Encode method on the instance
        
        return hashedPassword.Replace("-","");
    }

    public static string getPepper()
    {
        byte[] pepper = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(pepper);
        }
        return Convert.ToBase64String(pepper);
    }

    public static void Type(string text)
    {
        foreach (char c in text)
        {
            Console.Write(c);
            Thread.Sleep(1);
        }
        Console.WriteLine();
    }

    public static async void PasswordGen()
    {
         try{
            showHeader();
            Type("Set password length: [min recommended: 17]");
            int length = int.Parse(Console.ReadLine());

            List<string> strengthOptions = ["1. Absurdly Strong","2. Even Mightier","3. Dexters Special Brew"];
            strengthOptions.ForEach(x => Type(x));
            int strength = int.Parse(Console.ReadLine());
            Console.Clear();
            showHeader();
            var options = AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                .Title("Tell Dexter How?")
                .InstructionsText("[grey](Press [blue]<space>[/] to toggle an option, " + 
                                  "[green]<enter>[/] to continue)[/]")
                .AddChoices(new[] {
                        "Uppercase Letters", "Numbers", "Special Characters",
                        "Visually Ambiguous Characters", "Duplicate Characters", "Sequential Characters",
                }));

            string password = GeneratePassword(length,strength,options);
            Console.WriteLine(" ");
            Console.ForegroundColor = ConsoleColor.Green;
            Type($"Your Super Cooked Secure Password: {password}"); 
            await SubMenu(password);
       }catch(IOException){
           Console.WriteLine("Invalid input, please try again");
           PasswordGen();
       }catch(Exception e){
           Console.WriteLine(e.Message);
           PasswordGen();
       }
    }

    public static async Task SubMenu(string password){       //1. make 3rd option to see hash , 2. change to selection menue
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("Password Menu")
            .AddChoices(new[]{
                "1. Generate Another Password","2. Save Previously Generated Password",
                "3. Main Menu","4. Exit"
            }));

        switch(option){
            case "1. Generate Another Password": Console.Clear();
            PasswordGen();
            break;
            case "2. Save Previously Generated Password": await SaveAync(getHash(password)); 
            break;
            case "3. Main Menu": Console.Clear();
            TestMenue();
            break;
            case "4. Exit": Environment.Exit(0);
            break;
            default: await SubMenu(password); break;
        }  
    }

    public static async Task SaveAync(string hash){
       try{
            Type("Give the password a name");
            string? name = Console.ReadLine();
                
            var path = "yourPasswords.csv";

            using var reader = new StreamReader(path);
            if (reader.ReadToEnd().Contains(name))
            {
                Type("Password has already been saved");
            }
            else
            {
                using (var writer = new StreamWriter(path))
                {
                    await writer.WriteLineAsync("Password_Name , Hash");
                    await writer.WriteLineAsync(" ");
                    await writer.WriteLineAsync($"{name} , {hash}");
                }
                Type("Password saved to 'yourPasswords.csv'");
            }
        }
        catch(FileNotFoundException e){Type(e.Message);
       }catch(Exception e ){Type(e.Message);}
    }

    public static void TestMenue()  //new features for Dextera and improvement to navigation for Dexter v2 
    {      
        showHeader();
        Console.ForegroundColor = ConsoleColor.Cyan;
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("What can Dexter do for you?")
            .AddChoices([
                "1. Secure Password Generation"
                // "2. File Encyption/Decryption",
                // "3. General File Health Checks(Zips)",
                // "4. Data Compression/Decompression",
                // "5. System Information",
                // "6. Nothing"
            ])); 
            switch(string.Format(option)){
                case "1. Secure Password Generation" : Console.Clear();
                PasswordGen();
                break;
                // case "2. File Encyption/Decryption": break;
                // case "3. General File Health Checks(Zips)": break;
                // case "4. Data Compression/Decompression": break;
                // case "5. System Information" : break;
                // case "6. Nothing" : Environment.Exit(0); break;
            }   
            Console.Clear();
    }

    public static void showHeader(){  // update ascii style 
            Console.Title = "Dexter v1.2";
            //Console.SetWindowSize(0, 30);
            string name = @"
             /$$$$$$$                       /$$                        
            | $$__  $$                     | $$                        
            | $$  \ $$  /$$$$$$  /$$   /$$/$$$$$$    /$$$$$$   /$$$$$$\
            | $$  | $$ /$$__  $$ | $$ /$$/_  $$_/   /$$__  $$ /$$__  $$
            | $$  | $$| $$$$$$$$  \ $$$$/  | $$    | $$$$$$$$| $$  \__/
            | $$  | $$| $$_____/  >$$  $$  | $$ /$$| $$_____/| $$      
            | $$$$$$$/|  $$$$$$$ /$$/\  $$ |  $$$$/|  $$$$$$$| $$      
            |_______/  \_______/|__/  \__/  \___ /  \_______/|__/       v1.1";

            Console.WriteLine(name);
            Console.WriteLine("___________________________________");
            Console.WriteLine(" ");
    }
 }
 
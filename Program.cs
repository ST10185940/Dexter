using Konscious.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using Scrypt;
using PasswordGenerator;
using Spectre.Console;


class Dexter
{

    #pragma warning disable CS8602 // Dereference of a possibly null reference.
    #pragma warning disable CS8604 // Possible null reference argument for parameter 's' in 'int int.Parse(string s)'.
    #pragma warning disable CA1416


    public static void Main(){        
        Console.OutputEncoding = Encoding.UTF8;
        // TestMenue();
        // run();
        Entry();
    }   

    public static string GeneratePassword(int length , bool upper , bool nums , bool special , int strength ,bool avoidAmbiguous , bool noDupes , bool noSeq)
    {
        string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        string uppercase =  upper ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "";
        string multiLang =  strength == 3 ? "абвгдеёжзийклмнопрстуфхцчшщъыьэюяαβγδεζηθικλμνξοπρστυφχψωאבגדהוזחטיכלמנסעפצקרשת ب ت ث ج ح خ د ذ ر ز س ش ص ض ط ظ ع غ ف ق ك ل م ن ه و ي" : "";
        string numbers = nums ?  "0123456789๑ ๒ ๓ ๔ ๕ ๖ ๗ ๘ ๙ ๑๐一 二 三 四 五 六 七 八 九 十" : "";   //english , thai and japanese numbers
        string specChars = special ? "!@#$%^&*()_+{}|:<>?-=[];',./" : "";
        string symbols = strength <= 2 ?  "♠♣♥♦♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼" : "";
        string ambiguous =  avoidAmbiguous ? "il1Lo0O" : "";

        string allChars = Lowercase + uppercase + numbers + specChars + symbols + multiLang;

        foreach(char am in ambiguous){
            allChars = allChars.Replace(am.ToString(), "");
        }

        StringBuilder password = new StringBuilder();

        Random random = new Random(GetSecureSeed());


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
        using (var rng =  RandomNumberGenerator.Create())
        {
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }

    public static byte[] GenerateSalt()
    {
        byte[] salt = new byte[32];
        byte[] pepper = new byte[32];
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
            .Title("Select the desired hash algorithm")
            .AddChoices( new [] {
                  "Argon2id",
                  "SCrypt"
            }));
        
            switch (algo)
            {
                case "Argon2id":
                    return getArgon2Hash(password, GenerateSalt());
                case "SCrypt":
                    return getSCryptHash(password, GenerateSalt());
                default:
                    throw new Exception("Invalid input, please enter a number between 1 and 2");
            }
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

    public static async void run()
    {
         try{

            Console.Title = "Dexter v1.1";
            Console.SetWindowSize(85, 30);
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
            Console.WriteLine(" ");
            Console.WriteLine(" ____  _  _  ____  __  __  __  __");
            Console.WriteLine(" ");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Type("Set password length: [min recommended: 17]");
            int legnth = int.Parse(Console.ReadLine());
            
            Type("Specify Password strength: [1-3]");
            int strength = int.Parse(Console.ReadLine());
            
            Console.Clear();
            Console.WriteLine(name);
            Console.WriteLine(" ");

            Type("Include uppercase letters? [y/n]");
            bool upper = Console.ReadLine().ToLower() == " y" ? true : false;

            Type("Include numbers? [y/n]");
            bool nums = Console.ReadLine().ToLower() == "y" ? true : false;

            Type("Include special characters? [y/n]");
            bool special = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.Clear();
            Console.WriteLine(name);
            Console.WriteLine(" ");

            Type("Exclude visually ambiguous characters? [y/n]");
            bool avoidAmbiguous = Console.ReadLine().ToLower() == "y" ? true : false;

            Type("Void duplicate characters? [y/n]");
            bool noDupes = Console.ReadLine().ToLower() == "y" ? true : false;

            Type("Void sequential characters? [y/n]");
            bool noSeq = string.Equals(Console.ReadLine().ToLower(),"y") ? true : false;
                            
            Console.Clear();
            Console.WriteLine(name);
            Console.WriteLine();
            
            string password = GeneratePassword(legnth, upper, nums , special , strength , avoidAmbiguous , noDupes , noSeq);
           
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(name);
            Console.WriteLine(" ");

            Console.ForegroundColor = ConsoleColor.Cyan;
            Type($"Your Super Cooked Secure Password: {password}"); 

            await SubMenu(password);

                    
            /*Console.WriteLine(" ");
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Type("View password hash? [y/n]");
            bool get = Console.ReadLine().ToLower() == "y" ? true : false;
            
            if(get){           
                try {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                     Type($"Hashed password: {getHash(password)}");
                     Menu(getHash(password)); 
                }catch(Exception e){
                    getHash(password);
                    Console.WriteLine(e.Message);
                }
            }*/
       }catch(IOException){
           Console.WriteLine("Invalid input, please try again");
           Main();
       }catch(Exception e){
           Console.WriteLine(e.Message);
       }
    }
    

    public static void Entry(){

         while(true){
            var select = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
            .Title("What Can Dexter Do For You")
            .AddChoices(new[] {
                "Secure Password Generation",
                "Nothing"
          })); 

          switch(select){
              case "Secure Password Generation": run();
                      break;
              case "Nothing":
                    Type("Until next time!");
                    Environment.Exit(10);
                    break;
            default: 
                    Type("Be serious here, Okay!");
                    break;
          }
        }   
    }

    public static async Task SubMenu(string password){       //1. make 3rd option to see hash , 2. change to selection menue
        while(true){
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("1. Generate Another Password"); 
                Console.WriteLine("2. Save Password");
                Console.WriteLine("3. Main Menu");
                Console.WriteLine("4. Exit ");

                int choice = int.Parse(Console.ReadLine());

                switch(choice){
                    case 1: 
                        Console.Clear();
                        run();
                        break;
                    case 2: 
                        Type(await SaveAync(getHash(password))); 
                        break;
                    case 3: 
                        Console.Clear();
                        Entry();
                        break;
                    case 4: 
                        Environment.Exit(0);
                        break;
                    default: 
                        Type("Invalid input, please enter a number between 1 and 2");
                        break;
                }
        }
    }

    public static async Task<string> SaveAync(string hash){

        Type("Give the password a name");
        string? name = Console.ReadLine();
        Console.WriteLine(" ");
            
        var path = "yourPasswords.csv";

        using var writer = new StreamWriter(path);
        await writer.WriteLineAsync("Password_Name , Hash");
        await writer.WriteLineAsync(" ");
        await writer.WriteLineAsync($"{name} , {hash}");

        return "Password Saved locally to yourPasswords.csv ";
    }

    // public static void TestMenue()  //new features for Dextera and improvement to navigation for Dexter v2 
    // {      

    //         var option = AnsiConsole.Prompt(
    //             new SelectionPrompt<string>()
    //             .Title("What can Dexter do for you?")
    //             .AddChoices(new[] {
    //                 "Secure Password Generation",
    //                 "File Encyption/Decryption",
    //                 "General File Health Checks(Zips)",
    //                 "Data Compression/Decompression",
    //                 "System Information",
    //                 "Nothing"
    //          }));    
    // }
 }
 
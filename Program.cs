using System;
using Konscious.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using Scrypt;

class PasswordGenerator
{

    #pragma warning disable CS8602 // Dereference of a possibly null reference.
    #pragma warning disable CS8604 // Possible null reference argument for parameter 's' in 'int int.Parse(string s)'.
    

    public static void Main(){
        run();
    }


    public static string GeneratePassword(int length , bool upper , bool nums , bool special , int strength ,bool avoidAmbiguous , bool noDupes , bool noSeq)
    {
        string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        string uppercase =  upper ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "";
        string multiLang =  strength == 3 ? "абвгдеёжзийклмнопрстуфхцчшщъыьэюяαβγδεζηθικλμνξοπρστυφχψωאבגדהוזחטיכלמנסעפצקרשת ب ت ث ج ح خ د ذ ر ز س ش ص ض ط ظ ع غ ف ق ك ل م ن ه و ي" : "";
        string numbers = nums ?  "0123456789" : "";
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
        Console.WriteLine("Enter the desired hash algorithm: (1-2):");
        Console.WriteLine("1. Argon2");
        Console.WriteLine("2. SCrypt");
        int algo = int.Parse(Console.ReadLine());
        try{
            switch (algo)
            {
                case 1:
                    return getArgon2Hash(password, GenerateSalt());
                case 2:
                    return getSCryptHash(password, GenerateSalt());
                default:
                    throw new Exception("Invalid algorithm, please enter a number between 1 and 2");
            }
        }catch(Exception e){
            throw new Exception(e.Message);
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
            Thread.Sleep(25);
        }
        Console.WriteLine();
    }


    public static void run()
    {
         try{
           // Console.SetWindowSize(500, 400);
            Console.Title = "Password Generator v1.0";
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
            Console.WriteLine(" ____  _  _  ____  __  __  __  __");
            Console.WriteLine();

            Console.ForegroundColor = ConsoleColor.Cyan;
            Type("Enter the desired passord length (*recommended: min 17 ):");
            int legnth = int.Parse(Console.ReadLine());
            
            Type("Specify Password strength: (1-3):");
            int strength = int.Parse(Console.ReadLine());
            
            Type("Include uppercase letters? (y/n)");

            bool upper = Console.ReadLine().ToLower() == "y" ? true : false;

            Type("Inculde numbers? (y/n)");
            bool nums = Console.ReadLine().ToLower() == "y" ? true : false;

            Type("Include special characters? (y/n)");
            bool special = Console.ReadLine().ToLower() == "y" ? true : false;

            Type("Avoid visually ambiguous characters? (y/n)");
            bool avoidAmbiguous = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.WriteLine("No duplicate characters? (y/n)");
            bool noDupes = Console.ReadLine().ToLower() == "y" ? true : false;

            Type("No sequential characters? (y/n)");
            bool noSeq = string.Equals(Console.ReadLine().ToLower(),"y")? true : false;
                            
            string password = GeneratePassword(legnth, upper, nums , special , strength , avoidAmbiguous , noDupes , noSeq);
           
            Console.Clear();
            Console.WriteLine(name);

            Type($"Generated password: {password}"); 
            Type("Get password hash? (y/n)");
            bool get = Console.ReadLine().ToLower() == "y" ? true : false;
    
            if(get){           
                try {
                    Type($"Hashed password: {getHash(password)}"); 
                }catch(Exception e){
                    getHash(password);
                    Console.WriteLine(e.Message);
                }
            }

       }catch(IOException){
           Console.WriteLine("Invalid input, please try again");
           Main();
       }catch(Exception e){
           Console.WriteLine(e.Message);
       }

    }
}
 
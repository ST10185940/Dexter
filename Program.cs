using System;
using Konscious.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using Scrypt;
using System.Xml.Serialization;


class PasswordGenerator
{

    public static void Main(){
        run();
    }


    public static string GeneratePassword(int length , bool upper , bool nums , bool special , int strength ,bool avoidAmbiguous , bool noDupes , bool noSeq)
    {
        string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        string uppercase =  upper ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "";
        string multiLang =  (strength == 3) ? "абвгдеёжзийклмнопрстуфхцчшщъыьэюяαβγδεζηθικλμνξοπρστυφχψωאבגדהוזחטיכלמנסעפצקרשת ب ت ث ج ح خ د ذ ر ز س ش ص ض ط ظ ع غ ف ق ك ل م ن ه و ي" : "";
        string numbers = nums ?  "0123456789" : "";
        string specChars = special ? "!@#$%^&*()_+{}|:<>?-=[];',./" : "";
        string symbols = (strength <= 2) "♠♣♥♦♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼" : "";
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
                    //return getSCryptHash(password, GenerateSalt());
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

     public static string getSCryptHash(string password, byte[] salt)
    {
        byte[] saltBytes = salt;
        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] hashedPassword = ScryptEncoder.Encode(passwordBytes,saltBytes, 16384, 8, 1, 32);
        return Convert.ToBase64String(hashedPassword);
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

    public static void run()
    {
         try{
            Console.Title = "Password Generator v1.0";
            string name = @"
             /$$$$$$$                       /$$                        
            | $$__  $$                     | $$                        
            | $$  \ $$  /$$$$$$  /$$   /$$/$$$$$$    /$$$$$$   /$$$$$$\
            | $$  | $$ /$$__  $$|  $$ /$$/_  $$_/   /$$__  $$ /$$__  $$
            | $$  | $$| $$$$$$$$ \  $$$$/  | $$    | $$$$$$$$| $$  \__/
            | $$  | $$| $$_____/  >$$  $$  | $$ /$$| $$_____/| $$      
            | $$$$$$$/|  $$$$$$$ /$$/\  $$ |  $$$$/|  $$$$$$$| $$      
            |_______/  \_______/|__/  \__/  \___/   \_______/|__/       v1.0";
        
            Console.WriteLine(name);
            Console.WriteLine("");
        

            Console.WriteLine("Enter the desired passord length (*recommended: min 17 ):");
            int legnth = int.Parse(Console.ReadLine());

            Console.WriteLine("Specify Password strength: (1-3):");
            int strength = int.Parse(Console.ReadLine());
            
            Console.WriteLine("Include uppercase letters? (y/n)");
            bool upper = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.WriteLine("Inculde numbers? (y/n)");
            bool nums = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.WriteLine("Include special characters? (y/n)");
            bool special = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.WriteLine("Avoid visually ambiguous characters? (y/n)");
            bool avoidAmbiguous = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.WriteLine("No duplicate characters? (y/n)");
            bool noDupes = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.WriteLine("No sequential characters? (y/n)");
            bool noSeq = Console.ReadLine().ToLower() == "y" ? true : false;
                            
            string password = GeneratePassword(legnth, upper, nums , special , strength , avoidAmbiguous , noDupes , noSeq);

            Console.WriteLine($"Generated password: {password}"); 

            Console.WriteLine("Get password hash? (y/n)");
            bool get = Console.ReadLine().ToLower() == "y" ? true : false;
    
            if(get){           
                try {
                    Console.WriteLine($"Hashed password: {getHash(password)}"); 
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
 
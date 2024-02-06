using System;
using System.Security.Cryptography.X509Certificates;
using Argon2;
using BCrypt.Net;
using SCrypt;


class PasswordGenerator
{

    public static void Main(){

       try{
            Console.SetWindowSize(80, 30);
            Console.SetBufferSize(80, 30);
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.Green;
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
        
            Console.WriteLine("------------------");

            Console.WriteLine("Enter the desired passord length:");
            int legnth = int.Parse(Console.ReadLine());

            Console.WriteLine("Specify Password strength: (1-3):");
            int strength = int.Parse(Console.ReadLine());

            if(strength =< 1 || strength => 3){
                throw new Exception("Invalid strength, please enter a number between 1 and 3");
            }
            
            Console.WriteLine("Include uppercase letters? (y/n)");
            bool upper = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.WriteLine("Inculde numbers? (y/n)");
            bool nums = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.WriteLine("Include special characters? (y/n)");
            bool special = Console.ReadLine().ToLower() == "y" ? true : false;

            Console.WriteLine("Avoid visulally ambiguous characters? (y/n)");
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
                    Console.WriteLine($"Hashed password: { getHash(password);}"); 
                }catch(Exception e){
                    getHash(password);
                    Console.WriteLine(e.Message);
                }
            }

            Console.WriteLine("Save password to ledgder? (y/n)");
            Console.ReadLine().ToLower() == ? savePass(0) : exit;

       }catch(IOException){
           Console.WriteLine("Invalid input, please try again");
           Main();
       }catch(Exception e){
           Console.WriteLine(e.Message);
       }

    }

    public static string GeneratePassword(int length , bool upper , bool nums , bool special , int strength ,bool avoidAmbiguous , boole noDupes , bool noSeq)
    {
        string Lowercase = "abcdefghijklmnopqrstuvwxyz";
        string uppercase =  upper ? "ABCDEFGHIJKLMNOPQRSTUVWXYZ" : "";
        string numbers = nums ?  "0123456789" : "";
        string specChars = special ? "!@#$%^&*()_+{}|:<>?-=[];',./" : "";
        string emojies = (strength < 3) ? "" : "😀😁😂🤣😃😄😅😆😉😊😋😎😍😘😗😙😚😇😐😑😶😏😣😥😮😯😪😫😴😌😛😜😝😒😓😔😕😲😷😖😞😟😤😢😭😦😧😨😩😬😰😱😳😵😡😠";
        string symbols = (strength < 2) ? "" :"♠♣♥♦♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼" ;
        string ambiguous =  avoidAmbiguous ? "il1Lo0O" : "";

        string allChars = Lowercase + uppercase + numbers + specChars + emojies + symbols;

        foreach(char ambiguous in ambiguous){
            allChars = allChars.Replace(ambiguous.ToString(), "");
        }

      
        //char[] password = new char[length];

        /*for (int i = 0 ; i < length; i++){
         password[i] = allChars[random.Next(allChars.Length)];
        }*/


        StringBuilder password = new StringBuilder();

        Random random = new Random(GetSecureSeed());


        while (password.Length < length)
        {
            char nextChar = allChars[random.Next(allChars.Length)];

            if (noDupes && password.ToString().Contains(nextChar)) continue;
        
            if (noSeq && password.Length > 0)
            {
                char lastChar = passord[password.Length - 1];
                if (IsSequential(lastChar, nextChar) continue;          
            }

            password.Append(nextChar);
        }
        

        return password.ToString();
    }

    public static bool IsSequential(char a , char b ){
        return Math.Abs(a-b) == 1;
    }   

    public int GetSecureSeed(){
        using (var rng = new RNGCryptoServiceProvider())
        {
            byte[] bytes = new byte[4];
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }
    }

    public static string getHash(string password)
    {
        Console.WriteLine("Enter a salt: (leave empty to generate a random salt)");
        string salt = Console.ReadLine();
        if (string.IsNullOrEmpty(salt))
        {
            salt = BCryptHelper.GenerateSalt(12);
        }
        string hash;

        Console.WriteLine("Enter the desired hash algorithm: (1-5):");
        Console.WriteLine("1. Argon2");
        Console.WriteLine("2. BCrypt");
        Console.WriteLine("4. SCrypt");
        Console.WriteLine("5. SHA256");
        int algo = int.Parse(Console.ReadLine());
        switch (algo)
        {

        }

        return new string hash;
    
    }

    public byte GenerateSalt()
    {
        byte[] salt = new byte();
        using (var rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }
    {
        
    }
    
}
 
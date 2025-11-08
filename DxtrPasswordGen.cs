using Konscious.Security.Cryptography;
using System.Text;
using System.Security.Cryptography;
using Scrypt;
using System.ComponentModel.DataAnnotations;

namespace DxtrPasswordGen
{
    public class Password(string PasswordName, string payload)
    {
        [Required]
        private protected string Name { get; set; } = PasswordName;

        [Required]
        private protected string Hash { get; set; } = payload;
    }

    public class DxPassword
    {
        public static string GeneratePassword(int length, int strength, List<string> options)
        {
            string Lowercase = "abcdefghijklmnopqrstuvwxyz";
            string symbols = strength >= 2 ? "♠♣♥♦♪♫☼►◄↕‼¶§▬↨↑↓→←∟↔▲▼" : "";
            string multiLang = strength == 3 ? "абвгдеёжзийклмнопрстуфхцчшщъыьэюяαβγδεζηθικλμνξοπρστυφχψωאבגדהוזחטסעפצקרשת ب ت ث ج ح خ د ذ ر ز س ش ص ض ط ظ ع غ ف ق ك ل م ن ه و ي" : "";

            var uppercaseSelected = options.Where(x => x.Contains("Uppercase Letters"));
            string uppercase = uppercaseSelected == null ? "" : "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

            var numbersSelected = options.Where(x => x.Contains("Numbers"));
            string numbers = numbersSelected == null ? "" : "0123456789๑๒๓๔๕๖๗๘๙๑๐一二三四五六七八九十";

            var specSelected = options.Where(x => x.Contains("Special Characters"));
            string specChars = specSelected == null ? "" : "!@#$%^&*()_+{}|:<>?-=[];',./";

            var ambigSelected = options.Where(x => x.Contains("Exclude Visually Ambiguous Characters"));
            string ambiguous = ambigSelected == null ? "" : "il1Lo0O";

            var nodupesSelected = options.Where(x => x.Contains("Exclude duplicate Characters"));
            bool noDupes = nodupesSelected != null;

            var noSeqSelected = options.Where(x => x.Contains("Exclude Sequential Characters"));
            bool noSeq = noSeqSelected != null;

            string allChars = Lowercase + uppercase + numbers + specChars + symbols + multiLang;

            if (!string.IsNullOrEmpty(ambiguous))
            {
                foreach (char am in ambiguous)
                {
                    allChars = allChars.Replace(am.ToString(), "");
                }
            }

            StringBuilder password = new();

            Random random = new(GetSecureSeed());

            while (password.Length <= length)
            {
                char nextChar = allChars[random.Next(allChars.Length)];

                if (noDupes && password.ToString().Contains(nextChar)) continue;

                if (noSeq && password.Length > 0)
                {
                    char lastChar = password[^1];
                    if (IsSequential(lastChar, nextChar)) continue;
                }

                password.Append(nextChar);
            }

            return password.ToString();
        }

        private static bool IsSequential(char a, char b)
        {
            return Math.Abs(a - b) == 1;
        }

        private static int GetSecureSeed()
        {
            using var rng = RandomNumberGenerator.Create();
            byte[] bytes = new byte[64];
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        private static byte[] GenerateSalt()
        {
            byte[] salt = new byte[128];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }

            return salt;
        }

        public static string GetHash(string password, string algorithm)
        {
            password += GetPepper();

            return algorithm switch
            {
                "Argon2id" => GetArgon2Hash(password, GenerateSalt()),
                "SCrypt" => GetSCryptHash(password, GenerateSalt()),
                _ => throw new Exception("Invalid algorithm"),
            };
        }

        private static string GetArgon2Hash(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 28,
                MemorySize = 16200,
                Iterations = 100000
            };

            return Convert.ToBase64String(argon2.GetBytes(64));
        }

        private static string GetSCryptHash(string password, byte[] saltNPepper)
        {
            string saltString = Convert.ToBase64String(saltNPepper);
            string saltedPass = saltString + password;

            ScryptEncoder scryptEncoder = new ScryptEncoder();
            string hashedPassword = scryptEncoder.Encode(saltedPass);

            return hashedPassword.Replace("-", "");
        }

        private static string GetPepper()
        {
            byte[] pepper = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(pepper);
            }
            return Convert.ToBase64String(pepper);
        }

        public static async Task<SaveResult> SavePasswordAsync(string name, string hash)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return new SaveResult { Success = false, Message = "Please enter a password name" };
                }

                Password saved = new(name, hash);

                // Use absolute path to ensure consistent file location
                string directory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Dexter Vault");
                Directory.CreateDirectory(directory);
                string path = Path.Combine(directory, "yourPasswords.csv");

                // Check if password name already exists
                bool fileExists = File.Exists(path);
                string content = fileExists ? await File.ReadAllTextAsync(path) : "";

                if (content.Contains(name))
                {
                    return new SaveResult { Success = false, Message = "Password has already been saved", IsDuplicate = true };
                }

                // Append to file, creating header if file is new or empty
                bool needsHeader = !fileExists || new FileInfo(path).Length == 0;
                
                using (var writer = new StreamWriter(path, append: true))
                {
                    if (needsHeader)
                    {
                        await writer.WriteLineAsync("Password_Name , Hash");
                        await writer.WriteLineAsync(" ");
                    }
                    await writer.WriteLineAsync($"{saved}");
                    await writer.FlushAsync();
                }
                
                return new SaveResult { Success = true, Message = "Password saved successfully!" };
            }
            catch (Exception ex)
            {
                return new SaveResult { Success = false, Message = $"Error: {ex.Message}" };
            }
        }
    }

    public class SaveResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public bool IsDuplicate { get; set; }
    }
}


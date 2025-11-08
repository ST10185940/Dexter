using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace DxtrEncrypt
{
    public class DxCrypt
    {
        private const int KeySize = 32;
        private const int TagSize = 16;
        private const int SaltSize = 16;
        private const int NonceSize = 12;
        private const int BufferSize = 64 * 1024; // 64KB buffer for reading

        public static void Encrypt(string sourcePath, string destinationPath, string password)
        {
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("Source file not found", sourcePath);

            if (string.IsNullOrEmpty(destinationPath) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Destination path and password cannot be empty");

            // Generate salt and nonce
            byte[] salt = new byte[SaltSize];
            byte[] nonce = new byte[NonceSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
                rng.GetBytes(nonce);
            }

            // Derive key using Argon2id
            byte[] key = DeriveKey(password, salt);

            try
            {
                // Read file into memory (for AesGcm which requires full data)
                // For very large files, this could be optimized further
                byte[] plaintext;
                using (var inputStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan))
                {
                    long fileLength = inputStream.Length;
                    if (fileLength > int.MaxValue)
                        throw new NotSupportedException("File is too large to encrypt");

                    plaintext = new byte[fileLength];
                    int totalBytesRead = 0;
                    byte[] buffer = new byte[BufferSize];

                    while (totalBytesRead < fileLength)
                    {
                        int bytesRead = inputStream.Read(buffer, 0, (int)Math.Min(BufferSize, fileLength - totalBytesRead));
                        if (bytesRead == 0) break;

                        Buffer.BlockCopy(buffer, 0, plaintext, totalBytesRead, bytesRead);
                        totalBytesRead += bytesRead;
                    }
                }

                // Encrypt the data
                byte[] ciphertext = new byte[plaintext.Length];
                byte[] tag = new byte[TagSize];

                using (var aesGcm = new AesGcm(key, TagSize))
                {
                    aesGcm.Encrypt(nonce, plaintext, ciphertext, tag);
                }

                // Write encrypted file
                string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? Path.GetDirectoryName(tempFile) ?? "");

                using (var outputStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize))
                {
                    outputStream.Write(salt, 0, salt.Length);
                    outputStream.Write(nonce, 0, nonce.Length);
                    outputStream.Write(tag, 0, tag.Length);
                    outputStream.Write(ciphertext, 0, ciphertext.Length);
                    outputStream.Flush(true);
                }

                // Move temp file to destination
                if (File.Exists(destinationPath))
                    File.Delete(destinationPath);

                File.Move(tempFile, destinationPath);

                // Clear sensitive data
                Array.Clear(plaintext, 0, plaintext.Length);
                Array.Clear(ciphertext, 0, ciphertext.Length);
            }
            finally
            {
                Array.Clear(key, 0, key.Length);
                Array.Clear(salt, 0, salt.Length);
                Array.Clear(nonce, 0, nonce.Length);
            }
        }

        public static void Decrypt(string sourcePath, string destinationPath, string password)
        {
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath))
                throw new FileNotFoundException("Source file not found", sourcePath);

            if (string.IsNullOrEmpty(destinationPath) || string.IsNullOrEmpty(password))
                throw new ArgumentException("Destination path and password cannot be empty");

            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? "");

            try
            {
                // Read encrypted file
                byte[] fileContent;
                using (var inputStream = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, BufferSize, FileOptions.SequentialScan))
                {
                    long fileLength = inputStream.Length;
                    int headerSize = SaltSize + NonceSize + TagSize;

                    if (fileLength < headerSize)
                        throw new InvalidDataException("Encrypted file is corrupted or too small");

                    if (fileLength > int.MaxValue)
                        throw new NotSupportedException("File is too large to decrypt");

                    fileContent = new byte[fileLength];
                    int totalBytesRead = 0;
                    byte[] buffer = new byte[BufferSize];

                    while (totalBytesRead < fileLength)
                    {
                        int bytesRead = inputStream.Read(buffer, 0, (int)Math.Min(BufferSize, fileLength - totalBytesRead));
                        if (bytesRead == 0) break;

                        Buffer.BlockCopy(buffer, 0, fileContent, totalBytesRead, bytesRead);
                        totalBytesRead += bytesRead;
                    }
                }

                // Extract salt, nonce, tag, and ciphertext
                byte[] salt = new byte[SaltSize];
                byte[] nonce = new byte[NonceSize];
                byte[] tag = new byte[TagSize];

                Buffer.BlockCopy(fileContent, 0, salt, 0, SaltSize);
                Buffer.BlockCopy(fileContent, SaltSize, nonce, 0, NonceSize);
                Buffer.BlockCopy(fileContent, SaltSize + NonceSize, tag, 0, TagSize);

                int ciphertextLength = fileContent.Length - (SaltSize + NonceSize + TagSize);
                byte[] ciphertext = new byte[ciphertextLength];
                Buffer.BlockCopy(fileContent, SaltSize + NonceSize + TagSize, ciphertext, 0, ciphertextLength);

                // Derive key using Argon2id
                byte[] key = DeriveKey(password, salt);

                try
                {
                    // Decrypt the data
                    byte[] plaintext = new byte[ciphertextLength];

                    using (var aesGcm = new AesGcm(key, TagSize))
                    {
                        aesGcm.Decrypt(nonce, ciphertext, tag, plaintext);
                    }

                    // Write decrypted file
                    string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

                    using (var outputStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, BufferSize))
                    {
                        outputStream.Write(plaintext, 0, plaintext.Length);
                        outputStream.Flush(true);
                    }

                    // Move temp file to destination
                    if (File.Exists(destinationPath))
                        File.Delete(destinationPath);

                    File.Move(tempFile, destinationPath);

                    // Clear sensitive data
                    Array.Clear(plaintext, 0, plaintext.Length);
                    Array.Clear(ciphertext, 0, ciphertext.Length);
                    Array.Clear(fileContent, 0, fileContent.Length);
                }
                catch (CryptographicException)
                {
                    throw new InvalidOperationException("Decryption failed. The file may be corrupted or the password is incorrect.");
                }
                finally
                {
                    Array.Clear(key, 0, key.Length);
                    Array.Clear(salt, 0, salt.Length);
                    Array.Clear(nonce, 0, nonce.Length);
                    Array.Clear(tag, 0, tag.Length);
                }
            }
            catch (CryptographicException)
            {
                throw new InvalidOperationException("Decryption failed. The file may be corrupted or the password is incorrect.");
            }
        }

        private static byte[] DeriveKey(string password, byte[] salt)
        {
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                MemorySize = 65536, // 64 MB
                Iterations = 3
            };

            return argon2.GetBytes(KeySize);
        }
    }
}

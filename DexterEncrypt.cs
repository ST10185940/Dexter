using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace DxtrEncryption
{
    public class DxCrypt()
   {
        private const int KeySize = 32, TagSize = 32, SaltSize = 32, Iterations = 100000;
        private const int NonceSize = 24; // number once size 

        private static void Encrypt(string sourcePath, string destinationPath, string password){
            
            byte[] fileData, cipherText, key, tag;

            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath)) 
                throw new FileNotFoundException("Source file not found", sourcePath);

            if (string.IsNullOrEmpty(destinationPath) || string.IsNullOrEmpty(password))
                throw new ArgumentException("destination path / password cannot be empty");

            using (var fs = new FileStream(sourcePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.SequentialScan))
            {
                fileData = new byte[fs.Length];
                fs.Read(fileData, 0, fileData.Length);
            } //change later to read the file in chunks and not load it entirely into memory


            byte[] salt = new byte[SaltSize];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
                
            key = DeriveKey(password, salt);


            byte[] nonce = new byte[NonceSize];
            using (var rng = RandomNumberGenerator.Create())
            {   
                rng.GetBytes(nonce);
            }

            cipherText = new byte[fileData.Length];
            tag = new byte[TagSize];

            GCHandle keyHandle = GCHandle.Alloc(key, GCHandleType.Pinned);
            GCHandle dataHandle = GCHandle.Alloc(fileData, GCHandleType.Pinned);

           try{

                using (var aesGcm = new AesGcm(key))
                {
                    aesGcm.Encrypt(nonce, fileData, cipherText, tag);  
                }

           }finally{

             if (keyHandle.IsAllocated) keyHandle.Free();
             if (dataHandle.IsAllocated) dataHandle.Free();

           }

            try{
                string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                using (var OutputStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Encrypted | FileOptions.WriteThrough))
                using (var writer = new BinaryWriter(OutputStream))
                {
                    writer.Write(salt);
                    writer.Write(nonce);
                    writer.Write(tag);
                    writer.Write(cipherText);
                    writer.Flush();
                    OutputStream.Flush(true);
                }

                // if (!VerifyEncryptedFileIntegrity) //do this later. compare byte size and salt, nonce , ciphertext (current vs expected or something...) etc are intact and stored correctly 

                if (!File.Exists(destinationPath)) SecureDelete(destinationPath);
                
                File.Move(tempFile, destinationPath);

            }finally{

                if (fileData != null) Array.Clear(fileData, 0, fileData.Length);
                if (cipherText != null) Array.Clear(cipherText, 0, cipherText.Length);
                if (key != null) Array.Clear(key, 0, key.Length);

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }


        private static void SecureDelete(string filePath)
        {
            if (!File.Exists(filePath)) return;

            FileInfo fileInfo = new FileInfo(filePath);
            long length = fileInfo.Length;

            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Write))
            {
                byte[] buffer = new byte[64 * 1024];

                for (int pass = 0; pass < 3; pass++){
                    fs.Position = 0;
                    long bytesLeft = length;

                    using (var rng =  RandomNumberGenerator.Create()){
                        rng.GetBytes(buffer);
                    }

                    while (bytesLeft > 0){
                        int bytesToWrite = (int)Math.Min(buffer.Length, bytesLeft);
                        fs.Write(buffer, 0, bytesToWrite);
                        bytesLeft -= bytesToWrite;
                    }

                    fs.Flush(true);
                }
            }

            File.Delete(filePath);
        }

        private static byte[] DeriveKey(string password, byte[] salt)  // hello darkness my old friend 
        {
           using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA512);
           return pbkdf2.GetBytes(KeySize);
        }

        private static void Decrypt(string sourcePath, string destinationPath, string password){
            
            if (string.IsNullOrEmpty(sourcePath) || !File.Exists(sourcePath)) return;
            
            if (string.IsNullOrEmpty(password)) return;

            byte[] fileContent;

            //Directory.CreateDirectory(Path.GetDirectoryName(destinationPath) ?? string.Empty);

            try{

                fileContent = File.ReadAllBytes(sourcePath);

                if (fileContent.Length < SaltSize + TagSize + NonceSize)
                    throw new InvalidDataException("Encrypted file is corrupted or too small");

                byte[] salt = new byte[SaltSize];
                byte[] tag = new byte[TagSize];
                byte[] nonce = new byte[NonceSize];

                Buffer.BlockCopy(fileContent, 0 , salt, 0, SaltSize);
                Buffer.BlockCopy(fileContent, SaltSize, nonce, 0, NonceSize);
                Buffer.BlockCopy(fileContent, SaltSize + NonceSize , tag, 0, TagSize);

                int cipherTextLength = fileContent.Length - (SaltSize + TagSize + NonceSize);
                byte[] cipherText = new byte[cipherTextLength];
                
                Buffer.BlockCopy(fileContent, SaltSize + TagSize + NonceSize, cipherText, 0 , cipherTextLength);

                byte[] key = DeriveKey(password, salt);

                byte[] plainText = new byte[cipherTextLength];

                using (var aesGcm = new AesGcm(key))
                {
                    aesGcm.Decrypt(nonce, cipherText, tag, plainText);
                }        

                string tempFile = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                File.WriteAllBytes(tempFile, plainText);

                if (File.Exists(destinationPath)) File.Delete(destinationPath);
                
                File.Move(tempFile, destinationPath);

                if (fileContent != null)  Array.Clear(fileContent, 0 , fileContent.Length);
                if (plainText != null) Array.Clear(plainText, 0, plainText.Length);
                if (key != null) Array.Clear(key, 0, key.Length);
            
            }catch (CryptographicException) {
                throw new InvalidOperationException("Decryption failed. The file may be corrupted or the password is incorrect.");
            }
        } 
    }
}
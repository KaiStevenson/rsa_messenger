﻿using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.NetworkInformation;

namespace Rsa_Messenger
{
    class RsaMessageClient
    {
        public byte[] RSAKey;
        public byte[] RSATargetKey;
        public string RSATargetKeyName;
        public byte[] AESKey;
        public string username;
        public string matchServer;
        static void Main(string[] args)
        {
            Console.InputEncoding = Encoding.UTF8;
            Console.OutputEncoding = Encoding.UTF8;
            Directory.CreateDirectory("Keys");
            Directory.CreateDirectory("Messages");
            var p = new RsaMessageClient();
            p.UI();
        }
        //need receiver's public key
        public byte[] EncryptStringRSA(string text, byte[] key)
        {
            using (var rsa = RSA.Create(2048))
            {
                rsa.ImportRSAPublicKey(key, out int bytesRead);
                var data = Encoding.UTF8.GetBytes(text);
                return rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            }
        }
        //need private key
        public string DecryptBytesRSA(byte[] data, byte[] key)
        {
            using (var rsa = RSA.Create(2048))
            {
                rsa.ImportRSAPrivateKey(key, out int bytesRead);
                var ds = rsa.Decrypt(data, RSAEncryptionPadding.Pkcs1);
                return Encoding.UTF8.GetString(ds);
            }
        }
        public string EncryptStringAES(string text, byte[] key, byte[] iv)
        {
            using (var aes = Aes.Create())
            {
                var stream = new MemoryStream();
                var cryptStream = new CryptoStream(stream, aes.CreateEncryptor(key, iv), CryptoStreamMode.Write, true);
                var cStreamWriter = new StreamWriter(cryptStream, Encoding.UTF8, 1024, true);
                cStreamWriter.WriteLine(text);
                cStreamWriter.Close();
                cryptStream.Close();
                var streamWriter = new StreamWriter(stream, Encoding.UTF8, 1024, true);
                streamWriter.WriteLine("<END MESSAGE>");
                streamWriter.Close();
                stream.Position = 0;
                var streamReader = new StreamReader(stream, Encoding.UTF8, true, 1024, true);
                var r = streamReader.ReadToEnd();
                streamReader.Close();
                stream.Close();
                return r;
            }
        }
        static byte[] ReadBytesFromNetworkStream(NetworkStream ns)
        {
            var bytes = new byte[1024];
            var l = ns.Read(bytes, 0, 1024);
            var t = ns.FlushAsync();
            t.Wait();
            var bytesT = new byte[l];
            for (int i = 0; i < l; i++)
            {
                bytesT[i] = bytes[i];
            }
            return bytesT;
        }
        static bool IPIsOnNetwork(string ipTest)
        {
            foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (!ip.IsDnsEligible)
                    {
                        if (ip.Address.ToString() == ipTest)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        public void UI()
        {
            while (true)
            {
                if (username != null)
                {
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine(string.Format(">> SIGNED IN AS '{0}' <<", username));
                    if(matchServer == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(">> NOT CONNECTED TO A MATCH SERVER <<");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(string.Format(">> CONNECTED TO MATCH SERVER '{0}' <<", matchServer));
                    }
                    if (RSAKey == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(">> NO KEY LOADED <<");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(">> LOCAL KEY LOADED <<");
                    }
                    if (RSATargetKey == null)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine(">> NO KEY TARGETED <<");
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine(string.Format(">> KEY '{0}' TARGETED <<", RSATargetKeyName));
                    }
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("0. Configure keys");
                    Console.WriteLine("1. Send message");
                    Console.WriteLine("2. Receive message");
                    Console.WriteLine("3. Connect over network");
                    Console.WriteLine("4. Quit");
                    Console.WriteLine("Please make a selection:");
                    var k = Console.ReadKey();
                    Console.Clear();
                    if (k.Key == ConsoleKey.D0)
                    {
                        Console.WriteLine("0. AES");
                        Console.WriteLine("1. RSA");
                        Console.WriteLine("Please make a selection:");
                        var k2 = Console.ReadKey();
                        Console.Clear();
                        if (k2.Key == ConsoleKey.D0)
                        {
                            //check for key file
                            if (false)
                            {

                            }
                            else
                            {
                                Console.WriteLine("No AES key found! Generate new key? Y/N:");
                                var k3 = Console.ReadKey();
                                Console.Clear();
                                if (k3.Key == ConsoleKey.Y)
                                {
                                    var c = Aes.Create();
                                    c.GenerateKey();
                                    AESKey = c.Key;
                                    Console.WriteLine("Key saved");
                                }
                            }
                        }
                        bool newKey = false;
                        if (k2.Key == ConsoleKey.D1)
                        {
                            while (true)
                            {
                                if (RSAKey == null)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(">> NO KEY LOADED <<");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(">> LOCAL KEY LOADED <<");
                                }
                                if (RSATargetKey == null)
                                {
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    Console.WriteLine(">> NO KEY TARGETED <<");
                                }
                                else
                                {
                                    Console.ForegroundColor = ConsoleColor.Green;
                                    Console.WriteLine(string.Format(">> KEY '{0}' TARGETED <<", RSATargetKeyName));
                                }
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("0. Load/create key");
                                Console.WriteLine("1. Target key");
                                Console.WriteLine("2. Back");
                                Console.WriteLine("Please make a selection:");
                                var k3 = Console.ReadKey();
                                Console.Clear();
                                if (k3.Key == ConsoleKey.D0)
                                {
                                    if (File.Exists(string.Format("Keys\\{0}_PRIVATE.xkey", username)))
                                    {
                                        var keyFilePrivate = new FileStream(string.Format("Keys\\{0}_PRIVATE.xkey", username), FileMode.Open);
                                        Console.WriteLine("RSA key found. Load key? Y/N:");
                                        var k4 = Console.ReadKey();
                                        Console.Clear();
                                        if (k4.Key == ConsoleKey.Y)
                                        {
                                            byte[] bytes = new byte[keyFilePrivate.Length];
                                            keyFilePrivate.Read(bytes, 0, (int)keyFilePrivate.Length);
                                            keyFilePrivate.Close();
                                            RSAKey = bytes;
                                        }
                                        else
                                        {
                                            Console.WriteLine("Generate new key?");
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            Console.WriteLine(">> WARNING: THIS WILL DELETE YOUR EXISTING KEYPAIR <<");
                                            Console.WriteLine(">> PRESS THE X KEY FIVE TIMES TO CONFIRM, OR PRESS ESCAPE TO CANCEL <<");
                                            var x = 0;
                                            while (x < 5)
                                            {
                                                var k5 = Console.ReadKey();
                                                if (k5.Key == ConsoleKey.X)
                                                {
                                                    x++;
                                                    Console.CursorLeft -= 1;
                                                    Console.Write("\u2588");
                                                    Console.Write("\u2588");
                                                }
                                                else
                                                {
                                                    Console.Clear();
                                                    break;
                                                }
                                            }
                                            if (x == 5)
                                            {
                                                Console.ForegroundColor = ConsoleColor.White;

                                                Console.Clear();

                                                keyFilePrivate.Close();
                                                File.Delete(string.Format("Keys\\{0}_PRIVATE.xkey", username));
                                                File.Delete(string.Format("Keys\\{0}.xtkey", username));
                                                newKey = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("No RSA key found! Generate new key? Y/N:");
                                        var k4 = Console.ReadKey();
                                        Console.Clear();
                                        if (k4.Key == ConsoleKey.Y)
                                        {
                                            newKey = true;
                                        }
                                    }
                                    if (newKey)
                                    {
                                        var c = new RSACryptoServiceProvider(2048);
                                        RSAKey = c.ExportRSAPrivateKey();
                                        var privateWriter = new FileStream(string.Format("Keys\\{0}_PRIVATE.xkey", username), FileMode.Create);
                                        privateWriter.Write(c.ExportRSAPrivateKey());
                                        privateWriter.Close();
                                        var publicWriter = new FileStream(string.Format("Keys\\{0}.xtkey", username), FileMode.Create);
                                        publicWriter.Write(c.ExportRSAPublicKey());
                                        publicWriter.Close();
                                    }
                                }
                                if (k3.Key == ConsoleKey.D1)
                                {
                                    var publicKeys = Directory.GetFiles("Keys", @"*.xtkey");
                                    if (publicKeys.Length > 0)
                                    {
                                        Console.WriteLine("Found the following keys:");
                                        for (int i = 0; i < publicKeys.Length; i++)
                                        {
                                            Console.WriteLine(string.Format("{0}. '{1}'", i, publicKeys[i]));
                                        }
                                        Console.WriteLine("Please make a selection:");
                                        var k4 = Console.ReadKey();
                                        Console.Clear();
                                        if (char.IsDigit(k4.KeyChar))
                                        {
                                            var keyTarget = new FileStream(publicKeys[int.Parse(k4.KeyChar.ToString())], FileMode.Open);
                                            byte[] bytes2 = new byte[keyTarget.Length];
                                            keyTarget.Read(bytes2, 0, (int)keyTarget.Length);
                                            keyTarget.Close();
                                            RSATargetKey = bytes2;
                                            RSATargetKeyName = Path.GetFileNameWithoutExtension(publicKeys[int.Parse(k4.KeyChar.ToString())]);
                                        }
                                    }
                                    else
                                    {
                                        Console.ForegroundColor = ConsoleColor.Red;
                                        Console.WriteLine(">> NO TARGETABLE KEYS FOUND ON LOCAL DEVICE <<");
                                        Console.ForegroundColor = ConsoleColor.White;
                                        Console.WriteLine("Press any key to return");
                                        Console.ReadKey();
                                        Console.Clear();
                                    }
                                }
                                if (k3.Key == ConsoleKey.D2)
                                {
                                    break;
                                }
                            }
                        }
                    }
                    if (k.Key == ConsoleKey.D1)
                    {
                        Console.WriteLine("0. AES");
                        Console.WriteLine("1. RSA");
                        Console.WriteLine("Please make a selection:");
                        var k2 = Console.ReadKey();
                        Console.Clear();
                        if (k2.Key == ConsoleKey.D0)
                        {
                            Console.WriteLine("Enter message content:");
                            var mc = Console.ReadLine();
                            byte[] key = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
                            byte[] iv = { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x12, 0x13, 0x14, 0x15, 0x16 };
                            var es = EncryptStringAES(mc, key, iv);
                            Console.WriteLine(es);
                        }
                        if (k2.Key == ConsoleKey.D1)
                        {
                            if (RSATargetKey != null)
                            {
                                Console.WriteLine("Enter message content:");
                                var mc = Console.ReadLine();
                                var es = EncryptStringRSA(mc, RSATargetKey);
                                Console.WriteLine("<BEGIN RSA MESSAGE>");
                                var outputMessage = new FileStream(string.Format("Messages\\from{0}to{1}.rsam", username, RSATargetKeyName), FileMode.Create);
                                for (int i = 0; i < es.Length; i++)
                                {
                                    Console.Write(es[i]);
                                }
                                outputMessage.Write(es);
                                Console.WriteLine("\n<END RSA MESSAGE>");
                                outputMessage.Close();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine(string.Format("<< Message saved to {0} >>", outputMessage.Name));
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("Press any key to return");
                                Console.ReadKey();
                                Console.Clear();
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(">> TARGET A KEY BEFORE ACCESSING ENCRYPTION <<");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("Press any key to return");
                                Console.ReadKey();
                                Console.Clear();
                            }
                        }
                    }
                    if (k.Key == ConsoleKey.D2)
                    {
                        Console.WriteLine("0. AES");
                        Console.WriteLine("1. RSA");
                        Console.WriteLine("Please make a selection:");
                        var k2 = Console.ReadKey();
                        Console.Clear();
                        if (k2.Key == ConsoleKey.D1)
                        {
                            if (RSAKey != null)
                            {
                                var files = Directory.GetFiles("Messages", @"*.rsam");
                                if (files.Length > 0)
                                {
                                    Console.WriteLine("Found the following message files:");
                                    for (int i = 0; i < files.Length; i++)
                                    {
                                        Console.WriteLine(string.Format("{0}. '{1}'", i, files[i]));
                                    }
                                    Console.WriteLine("Please make a selection:");
                                    var k3 = Console.ReadKey();
                                    Console.Clear();
                                    if (char.IsDigit(k3.KeyChar))
                                    {
                                        var message = new FileStream(files[int.Parse(k3.KeyChar.ToString())], FileMode.Open);
                                        byte[] bytes2 = new byte[256];
                                        message.Read(bytes2, 0, (int)message.Length);
                                        message.Close();
                                        var ds = DecryptBytesRSA(bytes2, RSAKey);
                                        Console.WriteLine("<BEGIN PLAINTEXT MESSAGE>");
                                        Console.WriteLine(ds);
                                        Console.WriteLine("<END PLAINTEXT MESSAGE>");
                                        Console.WriteLine("Press any key to return");
                                        Console.ReadKey();
                                        Console.Clear();
                                    }
                                }
                            }
                            else
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.WriteLine(">> LOAD A KEY BEFORE ACCESSING DECRYPTION <<");
                                Console.ForegroundColor = ConsoleColor.White;
                                Console.WriteLine("Press any key to return");
                                Console.ReadKey();
                                Console.Clear();
                            }
                        }
                    }
                    if (k.Key == ConsoleKey.D3)
                    {
                        if (RSAKey != null)
                        {
                            Console.WriteLine("0. Connect via username");
                            Console.WriteLine("1. Connect via IP");
                            Console.WriteLine("Please make a selection:");
                            var k2 = Console.ReadKey();
                            Console.Clear();
                            string hostName = null;
                            if (k2.Key == ConsoleKey.D0)
                            {
                                TcpClient client = null;
                                for (int i = 0; i < 60; i++)
                                {
                                    if (Console.KeyAvailable)
                                    {
                                        break;
                                    }
                                    if (client == null || !client.Connected)
                                    {
                                        try
                                        {
                                            client = new TcpClient("msg.aetherdestroyer.net", 24848);
                                        }
                                        catch
                                        {
                                            Console.Write(".");
                                        }
                                    }
                                    if (client != null && client.Connected)
                                    {
                                        Console.Clear();
                                        Console.WriteLine("Local client created and connected");
                                        break;
                                    }
                                    Thread.Sleep(1000);
                                    Console.Write(".");
                                }
                                if (client != null)
                                {
                                    var ns = client.GetStream();
                                    Console.WriteLine("Enter username:");
                                    var targetName = Console.ReadLine();
                                    ns.Write(Encoding.UTF8.GetBytes("READING USER|"));
                                    ns.Write(Encoding.UTF8.GetBytes(targetName));
                                    while (!ns.DataAvailable)
                                    {
                                        Thread.Sleep(1000);
                                    }
                                    var r = Encoding.UTF8.GetString(ReadBytesFromNetworkStream(ns));
                                    Console.WriteLine("Received user information:");
                                    var rs = r.Split('|');
                                    Console.WriteLine(rs[0]);
                                    Console.WriteLine(rs[1]);
                                    Thread.Sleep(1000);
                                    if (IPIsOnNetwork(rs[1]))
                                    {
                                        hostName = rs[1];
                                        Console.WriteLine("Connecting to local IP");
                                    }
                                    else
                                    {
                                        hostName = rs[0];
                                        Console.WriteLine("Connecting to public IP");
                                    }
                                }
                            }
                            if (k2.Key == ConsoleKey.D1)
                            {
                                Console.WriteLine("Enter IP/hostname:");
                                hostName = Console.ReadLine();
                            }
                            var listener = new TcpListener(IPAddress.IPv6Any, 24846);
                            listener.Start();
                            Console.WriteLine("Press any key to cancel");
                            Console.Write("Connecting");
                            TcpClient output = null;
                            TcpClient input = null;
                            for (int i = 0; i < 60; i++)
                            {
                                if (Console.KeyAvailable)
                                {
                                    break;
                                }
                                if (output == null || !output.Connected)
                                {
                                    try
                                    {
                                        output = new TcpClient(hostName, 24846);
                                    }
                                    catch
                                    {
                                        Console.Write(".");
                                    }
                                    if (output != null && output.Connected)
                                    {
                                        Console.WriteLine();
                                        Console.WriteLine("Local client created and connected");
                                        Thread.Sleep(1000);
                                    }
                                }
                                if (input == null)
                                {
                                    if (listener.Pending())
                                    {
                                        input = listener.AcceptTcpClient();
                                        Console.WriteLine();
                                        Console.WriteLine("Local server created and bound");
                                    }
                                }
                                if ((output != null && output.Connected) && input != null)
                                {
                                    Console.WriteLine("Two way connection complete");
                                    break;
                                }
                                Thread.Sleep(1000);
                                Console.Write(".");
                            }
                            listener.Stop();
                            Console.WriteLine();
                            Thread.Sleep(1000);
                            Console.WriteLine("Syncing data");

                            var inS = input.GetStream();
                            var outS = output.GetStream();

                            //sync usernames and keys

                            //username
                            outS.Write(Encoding.UTF8.GetBytes(username.Trim()));
                            Thread.Sleep(1000);
                            while (!inS.DataAvailable)
                            {
                                Thread.Sleep(1000);
                            }
                            while (outS.DataAvailable)
                            {
                                Thread.Sleep(1000);
                            }
                            var otherName = Encoding.UTF8.GetString(ReadBytesFromNetworkStream(inS));
                            Console.WriteLine(string.Format("Other client name received: {0}", otherName));
                            //key
                            try
                            {
                                var myKey = new FileStream(string.Format("Keys\\{0}.xtkey", username), FileMode.Open);
                                var outBytes = new byte[myKey.Length];
                                myKey.Read(outBytes, 0, (int)myKey.Length);
                                myKey.Close();
                                outS.Write(outBytes);
                                Thread.Sleep(1000);
                                while (!inS.DataAvailable)
                                {
                                    Thread.Sleep(1000);
                                }
                                while (outS.DataAvailable)
                                {
                                    Thread.Sleep(1000);
                                }
                                var otherKey = ReadBytesFromNetworkStream(inS);
                                Console.WriteLine("Other client key received");
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine("<< CHAT INITIATED >>");
                                Console.ForegroundColor = ConsoleColor.White;
                                var sendMessageBuffer = "";
                                Console.Write(">> ");
                                //chat loop
                                while (true)
                                {
                                    if (inS.DataAvailable)
                                    {
                                        var incomingMessage = ReadBytesFromNetworkStream(inS);
                                        Console.CursorLeft = 0;
                                        Console.Write(new string(' ', Console.WindowWidth));
                                        Console.CursorLeft = 0;
                                        Console.WriteLine(string.Format(">> {0}: {1}", otherName, DecryptBytesRSA(incomingMessage, RSAKey)));
                                        Console.Write(">> ");
                                        Console.Write(sendMessageBuffer);
                                    }
                                    if (Console.KeyAvailable)
                                    {
                                        var key = Console.ReadKey();
                                        if (key.Key == ConsoleKey.Enter)
                                        {
                                            //send message
                                            Console.WriteLine(string.Format(">> {0}: {1}", username, sendMessageBuffer));
                                            Console.Write(">> ");
                                            outS.Write(EncryptStringRSA(sendMessageBuffer, otherKey));
                                            sendMessageBuffer = "";
                                        }
                                        else if (key.Key == ConsoleKey.Backspace && Console.CursorLeft > 1)
                                        {
                                            sendMessageBuffer = sendMessageBuffer.Remove(sendMessageBuffer.Length - 1);
                                            Console.Write(" ");
                                            Console.CursorLeft -= 1;
                                        }
                                        else
                                        {
                                            sendMessageBuffer += key.KeyChar;
                                        }
                                    }
                                }
                            }
                            catch (FileNotFoundException)
                            {
                                Console.WriteLine(">> ERROR: USER MUST GENERATE KEY PAIR UNDER CURRENT USERNAME <<");
                                Console.ReadKey();
                            }
                        }
                        else
                        {
                            Console.WriteLine(">> ERROR: USER MUST LOAD KEY <<");
                            Console.ReadKey();
                        }
                    }
                    if (k.Key == ConsoleKey.D4)
                    {
                        break;
                    }
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(">> NOT SIGNED IN <<");
                    Console.ForegroundColor = ConsoleColor.White;
                    Console.WriteLine("Enter your username:");
                    username = Console.ReadLine().Trim();
                    Console.Clear();
                    if (File.Exists(string.Format("Keys\\{0}_PRIVATE.xkey", username)))
                    {
                        var keyFilePrivate = new FileStream(string.Format("Keys\\{0}_PRIVATE.xkey", username), FileMode.Open);
                        Console.WriteLine("RSA key found. Load key? Y/N:");
                        var k2 = Console.ReadKey();
                        Console.Clear();
                        if (k2.Key == ConsoleKey.Y)
                        {
                            byte[] bytes = new byte[keyFilePrivate.Length];
                            keyFilePrivate.Read(bytes, 0, (int)keyFilePrivate.Length);
                            keyFilePrivate.Close();
                            RSAKey = bytes;
                        }
                        keyFilePrivate.Close();
                    }
                    Console.WriteLine("Use match server? Y/N:");
                    var k3 = Console.ReadKey();
                    Console.Clear();
                    if (k3.Key == ConsoleKey.Y)
                    {
                        Console.WriteLine("Enter match server hostname (enter 'default' for provided match server'):");
                        matchServer = Console.ReadLine();
                        if(matchServer == "default")
                        {
                            matchServer = "msg.aetherdestroyer.net";
                        }
                        Console.Clear();
                        Console.WriteLine("Connecting to server");
                        Console.WriteLine("Press any key to cancel (hostname fetching will be unavailable)");
                        TcpClient client = null;
                        for (int i = 0; i < 60; i++)
                        {
                            if (Console.KeyAvailable)
                            {
                                break;
                            }
                            if (client == null || !client.Connected)
                            {
                                try
                                {
                                    client = new TcpClient(matchServer, 24848);
                                }
                                catch
                                {
                                    Console.Write(".");
                                }
                            }
                            if (client != null && client.Connected)
                            {
                                Console.Clear();
                                Console.WriteLine("Local client created and connected");
                                break;
                            }
                            Thread.Sleep(1000);
                            Console.Write(".");
                        }
                        if (client != null)
                        {
                            var ns = client.GetStream();
                            ns.Write(Encoding.UTF8.GetBytes("ADDING USER|"));
                            ns.Write(Encoding.UTF8.GetBytes(username + "|"));
                            var hostName = Dns.GetHostName();
                            ns.Write(Encoding.UTF8.GetBytes(Dns.GetHostEntry(hostName).AddressList[0].ToString()));
                            ns.Close();
                            client.Close();
                        }
                    }
                }
            }
        }
    }
}
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

class AuthServer
{
    // Dictionary to store user data (username -> (key, hwid))
    private static Dictionary<string, (string Key, string HWID)> userDatabase = new Dictionary<string, (string Key, string HWID)>();

    static async Task Main(string[] args)
    {
        const int port = 5000; // Example port
        TcpListener listener = new TcpListener(IPAddress.Any, port);
        listener.Start();
        Console.WriteLine($"Server started on port {port}");

        while (true)
        {
            TcpClient client = await listener.AcceptTcpClientAsync();
            _ = HandleClient(client); // Handle each client in a separate task
        }
    }

    private static async Task HandleClient(TcpClient client)
    {
        using (NetworkStream stream = client.GetStream())
        {
            StreamReader reader = new StreamReader(stream);
            StreamWriter writer = new StreamWriter(stream) { AutoFlush = true };

            string request = await reader.ReadLineAsync();
            Console.WriteLine($"Received: {request}");

            if (request.StartsWith("REGISTER"))
            {
                string[] parts = request.Split('|');
                string username = parts[1];
                string key = parts[2];
                string hwid = parts[3];

                if (userDatabase.ContainsKey(username))
                {
                    var existingUser = userDatabase[username];
                    if (existingUser.HWID == hwid)
                    {
                        await writer.WriteLineAsync("REGISTERED:USERNAME_EXISTS_SAME_HWID");
                    }
                    else
                    {
                        await writer.WriteLineAsync("ERROR:USERNAME_EXISTS_DIFFERENT_HWID");
                    }
                }
                else
                {
                    userDatabase[username] = (key, hwid);
                    await writer.WriteLineAsync("SUCCESS:REGISTERED");
                    Console.WriteLine($"Registered: {username}");
                }
            }
            else if (request.StartsWith("LOGIN"))
            {
                string[] parts = request.Split('|');
                string username = parts[1];
                string key = parts[2];
                string hwid = parts[3];

                if (userDatabase.ContainsKey(username))
                {
                    var user = userDatabase[username];
                    if (user.Key == key && user.HWID == hwid)
                    {
                        await writer.WriteLineAsync("SUCCESS:LOGIN");
                        Console.WriteLine($"Login successful: {username}");
                    }
                    else
                    {
                        await writer.WriteLineAsync("ERROR:INVALID_CREDENTIALS_OR_HWID");
                    }
                }
                else
                {
                    await writer.WriteLineAsync("ERROR:USERNAME_NOT_FOUND");
                }
            }
            else
            {
                await writer.WriteLineAsync("ERROR:INVALID_REQUEST");
            }
        }
        client.Close();
    }
}
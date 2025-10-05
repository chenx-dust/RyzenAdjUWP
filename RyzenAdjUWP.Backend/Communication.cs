﻿using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using Windows.Storage;

namespace RyzenAdjUWP.Backend
{
    internal class Communication
    {
        private readonly NamedPipeServerStream _server;
        private readonly StreamReader _reader;
        private readonly StreamWriter _writer;

        public EventHandler ConnectedEvent {  get; set; }
        public EventHandler DisconnectedEvent {  get; set; }
        public EventHandler<string> ReceivedEvent { get; set; }

        public Communication(string packageSid)
        {
            Console.WriteLine($"[Connection] Package SID: {packageSid}");
            var pipeName = $"Sessions\\{Process.GetCurrentProcess().SessionId}\\AppContainerNamedObjects\\{packageSid}\\RyzenAdjPipe";
            Console.WriteLine($"[Connection] Pipe name: {pipeName}");
            
            _server = new NamedPipeServerStream(
                pipeName,
                PipeDirection.InOut, 1,
                PipeTransmissionMode.Message,
                PipeOptions.Asynchronous, 128, 128, GetPipeSecurity(packageSid));
            _reader = new StreamReader(_server);
            _writer = new StreamWriter(_server);
        }

        private static PipeSecurity GetPipeSecurity(string packageSid)
        {
            var ps = new PipeSecurity();
            var clientRule = new PipeAccessRule(
                new SecurityIdentifier(packageSid),
                PipeAccessRights.ReadWrite,
                AccessControlType.Allow);
            var ownerRule = new PipeAccessRule(
                WindowsIdentity.GetCurrent().User,
                PipeAccessRights.FullControl,
                AccessControlType.Allow);
            ps.AddAccessRule(clientRule);
            ps.AddAccessRule(ownerRule);
            return ps;
        }

        public void Run()
        {
            Console.WriteLine($"[Connection] Waiting for connection");
            _server.WaitForConnection();
            Console.WriteLine($"[Connection] Connection established");
            ConnectedEvent?.Invoke(this, null);

            while (true)
            {
                if (!_server.IsConnected)
                {
                    _server.Disconnect();
                    Console.WriteLine("[Connection] Disconnected, waiting for reconnecting");
                    DisconnectedEvent?.Invoke(this, null);
                    _server.WaitForConnection();
                    Console.WriteLine("[Connection] Reconnected");
                    ConnectedEvent?.Invoke(this, null);
                }
                string message = _reader.ReadLine();
                Console.WriteLine($"[Connection] Received: {message}");
                if (!string.IsNullOrEmpty(message))
                    ReceivedEvent?.Invoke(this, message);
            }
        }

        public void Send(string message)
        {
            if (!_server.IsConnected || message == null)
                return;

            Console.WriteLine($"[Connection] Sent: {message}");
            _writer.WriteLine(message);
            _writer.Flush();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RyzenAdjUWP.Backend
{
    internal class Handler
    {
        private readonly RyzenAdj _ryzenAdj;
        private readonly AutoStart _autoStart;

        public Handler(RyzenAdj ryzenAdj, AutoStart autoStart)
        {
            _ryzenAdj = ryzenAdj;
            _autoStart = autoStart;
        }

        public void Register(Communication comm)
        {
            comm.ConnectedEvent += OnConnected;
            comm.ReceivedEvent += OnReceived;
        }

        void OnConnected(object sender, EventArgs e)
        {
            (sender as Communication).Send("connected");
        }

        void OnReceived(object sender, string message)
        {
            var comm = sender as Communication;
            string[] args = message.Split(' ');
            if (args.Length == 0)
                return;
            switch (args[0])
            {
                case "set-tdp":
                    {
                        Console.WriteLine($"[Handler] Setting TDP to {args[1]} W");
                        var tdp = float.Parse(args[1]);
                        _ryzenAdj.StapmLimit_W = tdp;
                        _ryzenAdj.FastLimit_W = tdp;
                        _ryzenAdj.SlowLimit_W = tdp;
                    }
                    break;
                case "get-tdp":
                    {
                        _ryzenAdj.RefreshTable();
                        var currentTdp = Math.Max(_ryzenAdj.StapmLimit_W, Math.Max(_ryzenAdj.FastLimit_W, _ryzenAdj.SlowLimit_W));
                        var currentTdpInt = (int)Math.Round(currentTdp);
                        comm.Send($"tdp {currentTdpInt}");
                    }
                    break;
                case "init":
                    {
                        Console.WriteLine("[Handler] Get TDP Limit");
                        _ryzenAdj.RefreshTable();
                        var originStapmLimit = _ryzenAdj.StapmLimit_W;
                        var originFastLimit = _ryzenAdj.FastLimit_W;
                        var originSlowLimit = _ryzenAdj.SlowLimit_W;
                        Console.WriteLine($"[Handler] Origin StapmLimit: {originStapmLimit} W, FastLimit: {originFastLimit} W, SlowLimit: {originSlowLimit} W");
                        var currentTdp = Math.Max(originStapmLimit, Math.Max(originFastLimit, originSlowLimit));
                        var currentTdpInt = (int)Math.Round(currentTdp);
                        comm.Send($"tdp {currentTdpInt}");
                        const float testMaxTdp = 400, testMinTdp = 0;
                        _ryzenAdj.StapmLimit_W = testMaxTdp;
                        _ryzenAdj.FastLimit_W = testMaxTdp;
                        _ryzenAdj.SlowLimit_W = testMaxTdp;
                        _ryzenAdj.RefreshTable();
                        Console.WriteLine($"[Handler] Max StapmLimit: {_ryzenAdj.StapmLimit_W} W, FastLimit: {_ryzenAdj.FastLimit_W} W, SlowLimit: {_ryzenAdj.SlowLimit_W} W");
                        var maxTdp = Math.Max(_ryzenAdj.StapmLimit_W, Math.Max(_ryzenAdj.FastLimit_W, _ryzenAdj.SlowLimit_W));
                        _ryzenAdj.StapmLimit_W = testMinTdp;
                        _ryzenAdj.FastLimit_W = testMinTdp;
                        _ryzenAdj.SlowLimit_W = testMinTdp;
                        _ryzenAdj.RefreshTable();
                        Console.WriteLine($"[Handler] Min StapmLimit: {_ryzenAdj.StapmLimit_W} W, FastLimit: {_ryzenAdj.FastLimit_W} W, SlowLimit: {_ryzenAdj.SlowLimit_W} W");
                        var minTdp = Math.Min(_ryzenAdj.StapmLimit_W, Math.Min(_ryzenAdj.FastLimit_W, _ryzenAdj.SlowLimit_W));
                        _ryzenAdj.StapmLimit_W = originStapmLimit;
                        _ryzenAdj.FastLimit_W = originFastLimit;
                        _ryzenAdj.SlowLimit_W = originSlowLimit;
                        var maxTdpInt = (int)Math.Round(maxTdp);
                        var minTdpInt = (int)Math.Round(minTdp);
                        Console.WriteLine($"[Handler] Max TDP: {maxTdpInt} W, Min TDP: {minTdpInt} W");
                        comm.Send($"tdp-limit {maxTdpInt} {minTdpInt}");
                    }
                    {
                        bool enabled = AutoStart.IsEnabled(_autoStart.name);
                        Console.WriteLine($"[Handler] Get AutoStart Status: {enabled}");
                        comm.Send($"autostart {enabled}");
                    }
                    break;
                case "autostart":
                    {
                        Console.WriteLine($"[Handler] Set Auto Start: {args[1]}");
                        bool enabled = bool.Parse(args[1]);
                        _autoStart.SetEnabled(enabled);
                        comm.Send($"autostart {AutoStart.IsEnabled(_autoStart.name)}");
                    }
                    break;
            }
        }
    }
}

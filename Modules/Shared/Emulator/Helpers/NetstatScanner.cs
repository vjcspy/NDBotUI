﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace NDBotUI.Modules.Shared.Emulator.Helpers;

public class NetstatScanner
{
    public static List<int> GetAdbPorts()
    {
        var adbPorts = new List<int>();

        try
        {
            // Chạy lệnh netstat để lấy danh sách các cổng đang mở
            var psi = new ProcessStartInfo
            {
                FileName = "netstat.exe",
                Arguments = "-a -n -o -p TCP",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = new Process();
            process.StartInfo = psi;
            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var regex = new Regex(@"TCP\s+\S+:(\d+)\s+\S+:\d+\s+LISTENING\s+(\d+)", RegexOptions.IgnoreCase);
            var matches = regex.Matches(output);

            foreach (Match match in matches)
            {
                if (int.TryParse(match.Groups[1].Value, out var port))
                {
                    adbPorts.Add(port);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error scanning netstat: {ex.Message}");
        }

        return adbPorts;
    }
}
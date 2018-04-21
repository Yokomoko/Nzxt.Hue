using Nzxt.Hue.Core;
using System;
using System.Collections.Generic;
using System.CommandLine.Parser;
using System.CommandLine.Parser.Parameters;
using System.Linq;
using System.Runtime.InteropServices;

namespace Nzxt.Hue.Controller
{
    public static class Program
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr GetCommandLine();

        private static void GetCommandLine(out string commandLine)
        {
            commandLine = Marshal.PtrToStringUni(GetCommandLine());
        }

        public static int Main(string[] args)
        {
            var on = default(bool?);
            var off = default(bool?);
            var color = default(Color);
            GetParameters(out on, out off, out color);
            return Main(on, off, color);
        }

#pragma warning disable 0028
        public static int Main(bool? on, bool? off, Color color)
#pragma warning restore 0028
        {

            try
            {
                using (var device = new Device())
                {
                    var manager = new Manager(device);
                    manager.Start();
                    if (on.HasValue)
                    {
                        manager.SetHubState(true);
                    }
                    if (off.HasValue)
                    {
                        manager.SetHubState(false);
                    }
                    if (color != null)
                    {
                        manager.SetLightingColor(
                            color.Red,
                            color.Green,
                            color.Blue
                        );
                    }
                }
                return 0;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: {0}", e.Message);
            }
            return 0;
        }

        private static void GetParameters(out bool? on, out bool? off, out Color color)
        {
            on = default(bool?);
            off = default(bool?);
            color = default(Color);
            var commandLine = default(string);
            GetCommandLine(out commandLine);
            if (string.IsNullOrEmpty(commandLine))
            {
                return;
            }
            var arguments = commandLine.Split(new[] { "Nzxt.Kraken.Controller.exe", "\"" }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
            if (string.IsNullOrEmpty(arguments))
            {
                return;
            }
            var parser = new CommandLineParser();
            var parameters = parser.Parse(arguments);
            if (HasParameter(parameters.Parameters, "on"))
            {
                on = true;
            }
            if (HasParameter(parameters.Parameters, "off"))
            {
                off = true;
            }
            var red = GetParameter(parameters.Parameters, "r");
            var green = GetParameter(parameters.Parameters, "g");
            var blue = GetParameter(parameters.Parameters, "b");
            if (red.HasValue && green.HasValue && blue.HasValue)
            {
                color = new Color()
                {
                    Red = red.Value,
                    Green = green.Value,
                    Blue = blue.Value
                };
            }
        }

        private static bool HasParameter(IDictionary<string, Parameter> parameters, string name)
        {
            return parameters.ContainsKey(name);
        }

        private static byte? GetParameter(IDictionary<string, Parameter> parameters, string name)
        {
            var parameter = default(Parameter);
            if (!parameters.TryGetValue(name, out parameter))
            {
                return null;
            }
            if (parameter.Kind != ParameterKind.Number)
            {
                return null;
            }
            return Convert.ToByte((parameter as NumberParameter).Value);
        }

        public class Color
        {
            public byte Red { get; set; }

            public byte Green { get; set; }

            public byte Blue { get; set; }
        }
    }
}

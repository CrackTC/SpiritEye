using System.CommandLine;
using System.Xml;
using System.Xml.Linq;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace SpiritEye
{
    static class CommandLine
    {
        public static void HandleCommandLine(string[] args)
        {
            var targetArgument = new Argument<string>("target", "Scan target");
            var outputOption = new Option<string>(new[] { "-o", "--output" }, "Output file path");
            var rootCommand = new RootCommand("SpiritEye")
            {
                targetArgument,
                outputOption
            };

            rootCommand.SetHandler(Handle, outputOption, targetArgument);
            rootCommand.Invoke(args);
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Result))]
        static void Handle(string output, string target)
        {
            using var process = NMapHelper.LaunchNMap(target);
            if (process is null) return;

            // using var xmlReader = XmlReader.Create(File.OpenRead("/home/chen/result.xml"), new() { DtdProcessing = DtdProcessing.Ignore });
            using var xmlReader = XmlReader.Create(process.StandardOutput.BaseStream, new() { DtdProcessing = DtdProcessing.Ignore });

            Dictionary<string, Result> results = new();

            Utils.WithCursorHidden(() =>
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            switch (xmlReader.Name)
                            {
                                case "taskprogress":
                                    var task = xmlReader.GetAttribute("task") ?? "";
                                    var percent = float.Parse(xmlReader.GetAttribute("percent") ?? "0.00");
                                    Utils.ProgressBar(percent, task);
                                    break;
                                case "host":
                                    XElement host = (XElement)XElement.ReadFrom(xmlReader);
                                    var (ip, result) = ParseResult(host);
                                    results[ip] = result;
                                    break;
                            }
                            break;
                        default:
                            break;
                    }
                }
            });

            // output to file
            if (output != null)
            {
                using var file = File.OpenWrite(output);
                JsonSerializer.Serialize(file, results, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                Console.WriteLine();
                Console.WriteLine(JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true }));
            }

            Utils.Info("Scan finished");
        }

        static (string IP, Result Result) ParseResult(XElement hostElement)
        {
            var ip = hostElement.Element("address")!.Attribute("addr")!.Value;
            var services = hostElement.Element("ports")!.Elements("port")
                .Where(port => port.Element("state")!.Attribute("state")!.Value == "open")
                .Select(port =>
                {
                    var portId = int.Parse(port.Attribute("portid")!.Value);
                    var protocol = port.Attribute("protocol")!.Value;

                    var serviceElement = port.Element("service")!;
                    var product = serviceElement.Attribute("product")?.Value;

                    if (product is null)
                    {
                        return new Service(portId, protocol, null);
                    }
                    else
                    {
                        var version = serviceElement.Attribute("version")?.Value;

                        // [TODO) Additional service apps detection
                        return new Service(portId, protocol, new[] { new ServiceApp(product, version) });
                    }
                });

            // [TODO) Additional device info detection
            DeviceInfo? deviceInfo = null;

            // [TODO) honeypot detection
            var honeyPots = new HoneyPot[] { };

            return (ip, new Result(services, deviceInfo, honeyPots));
        }
    }
}

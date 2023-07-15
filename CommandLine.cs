using System.CommandLine;
using System.Dynamic;
using System.Net;
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
            var outputOption = new Option<string>(new[] { "-o", "--output" }, "Output file path");

            var targetArgument = new Argument<List<string>>("target", "Scan target") { Arity = ArgumentArity.OneOrMore };
            var scanCommand = new Command("scan", "Scan target")
            {
                outputOption,
                targetArgument
            };
            scanCommand.SetHandler(Scan, outputOption, targetArgument);

            var mergeArgument = new Argument<List<string>>("files", "Scan result files") { Arity = ArgumentArity.OneOrMore };
            var mergeCommand = new Command("merge", "Merge scan results")
            {
                outputOption,
                mergeArgument
            };
            mergeCommand.SetHandler(Merge, outputOption, mergeArgument);


            var rootCommand = new RootCommand("SpiritEye")
            {
                scanCommand,
                mergeCommand
            };
            rootCommand.Invoke(args);
        }

        static void Merge(string output, List<string> files)
        {
            var results = new Dictionary<string, dynamic>();

            files.ForEach(file =>
            {
                using var stream = File.OpenRead(file);
                var scanResult = JsonSerializer.Deserialize<Dictionary<string, dynamic>>(stream);
                foreach (var (ip, result) in scanResult!)
                    results[ip] = result;
            });

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
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Result))]
        static void Scan(string output, List<string> target)
        {
            var processes = NMapHelper.LaunchNMap(target);
            if (processes is null) return;

            var tasks = new List<Task<Dictionary<string, Result>>>();
            var results = new Dictionary<string, Result>();

            ServicePointManager.ServerCertificateValidationCallback += (_, _, _, _) => true;

            PostProcessing.OS.Register();
            PostProcessing.App.Register();
            PostProcessing.HoneyPot.Register();
            PostProcessing.Device.Register();
            PostProcessing.Protocol.Register();


            processes.ForEach(process =>
            {
                tasks.Add(Task.Run(() =>
                {
                    process.Start();

                    // using var xmlReader = XmlReader.Create(File.OpenRead("/home/chen/result.xml"), new() { DtdProcessing = DtdProcessing.Ignore });
                    using var xmlReader = XmlReader.Create(process.StandardOutput.BaseStream, new() { DtdProcessing = DtdProcessing.Ignore });
                    var taskResults = new Dictionary<string, Result>();

                    var tasks = new List<Task<(string, Result)>>();

                    using (process)
                    {
                        Utils.WithCursorHidden(() =>
                        {
                            while (xmlReader.Read())
                            {
                                if (xmlReader.NodeType == XmlNodeType.Element)
                                {
                                    try
                                    {
                                        switch (xmlReader.Name)
                                        {
                                            case "taskprogress":
                                                var task = xmlReader.GetAttribute("task") ?? "";
                                                var percent = float.Parse(xmlReader.GetAttribute("percent") ?? "0.00");
                                                Utils.ProgressBar(percent, task);
                                                break;
                                            case "host":
                                                XElement host = (XElement)XElement.ReadFrom(xmlReader);
                                                tasks.Add(ParseResult(host));
                                                break;
                                        }
                                    }
                                    catch (Exception? e)
                                    {
                                        Utils.Error(e.Message);
                                    }
                                }

                            }
                        });

                        Task.WaitAll(tasks.ToArray());
                        tasks.ForEach(task =>
                        {
                            var (ip, result) = task.Result;
                            if (ip != "")
                                taskResults[ip] = result;
                        });
                        return taskResults;
                    }
                }));
            });

            Task.WaitAll(tasks.ToArray());

            tasks.ForEach(task =>
            {
                var taskResults = task.Result;

                foreach (var (ip, result) in taskResults)
                    results[ip] = result;
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

        static async Task<(string, Result)> ParseResult(XElement hostElement)
        {
            try
            {
                var ip = hostElement.Element("address")!.Attribute("addr")!.Value;

                var services = hostElement.Element("ports")!.Elements("port")
                    .Where(port => port.Element("state")!.Attribute("state")!.Value == "open")
                    .Select(port =>
                    {
                        var portId = int.Parse(port.Attribute("portid")!.Value);
                        var serviceElement = port.Element("service");
                        return (portId, serviceElement);
                    });

                var taskInfos = (from service in services
                                 select (service.portId, task: PostProcess.Process(ip, service.portId, service.serviceElement))).ToList();

                await Task.WhenAll(taskInfos.Select(taskInfo => taskInfo.task));

                var results = from taskInfo in taskInfos
                              select (taskInfo.portId, processResults: taskInfo.task.Result);

                var processedServices = from result in results
                                        let port = result.portId
                                        let protocols = from processResult in result.processResults
                                                        where processResult.Type == PostProcessResultType.Protocol
                                                        select processResult.Protocol
                                        let processResults = result.processResults
                                        let serviceApps = from processResult in processResults
                                                          where processResult.Type == PostProcessResultType.ServiceApp
                                                          select processResult.ServiceApp
                                        select new Service(port, protocols.FirstOrDefault(), serviceApps.Any() ? serviceApps : null);

                var deviceInfo = from result in results
                                 from processResult in result.processResults
                                 where processResult.Type == PostProcessResultType.Device
                                 select processResult.DeviceInfo;

                var honeyPots = from result in results
                                from processResult in result.processResults
                                where processResult.Type == PostProcessResultType.HoneyPot
                                select processResult.HoneyPot;

                return (ip, new(processedServices, deviceInfo.Any() ? deviceInfo : null, honeyPots.Any() ? honeyPots : null));
            }
            catch (Exception? e)
            {
                Utils.Error(e.Message);
                return ("", new(Enumerable.Empty<Service>(), null, null));
            }
        }
    }
}

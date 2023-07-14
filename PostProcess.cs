using System.Xml.Linq;

namespace SpiritEye
{
    enum PostProcessResultType
    {
        None,
        ServiceApp,
        Protocol,
        HoneyPot,
        Device
    }

    class PostProcessResult
    {
        public PostProcessResultType Type { get; set; }

        public string? ServiceApp { get; set; }

        public string? Protocol { get; set; }

        public HoneyPot? HoneyPot { get; set; }

        public DeviceInfo? DeviceInfo { get; set; }
    }

    delegate PostProcessResult PostProcessor(string ip, int port, XElement serviceApp);

    static class PostProcess
    {
        private static List<PostProcessor> PostProcessors = new();

        public static void Register(PostProcessor postProcessor) => PostProcessors.Add(postProcessor);

        public static async Task<IEnumerable<PostProcessResult>> Process(string ip, int port, XElement? serviceElement)
        {
            if (serviceElement is null) return Enumerable.Empty<PostProcessResult>();

            Utils.Info($"Post processing {ip}:{port}...");
            var tasks = from processor in PostProcessors
                        select Task.Run(() =>
                        {
                            try
                            {
                                return processor(ip, port, serviceElement);
                            }
                            catch (Exception? e)
                            {
                                while (e is not null)
                                {
                                    Utils.Error(e.Message);
                                    e = e.InnerException;
                                }

                                return new PostProcessResult
                                {
                                    Type = PostProcessResultType.None
                                };
                            }
                        });

            var results = await Task.WhenAll(tasks);

            return from result in results
                   where result.Type != PostProcessResultType.None
                   select result;
        }
    }
}

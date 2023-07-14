using System.Text.RegularExpressions;

namespace SpiritEye.PostProcessing
{
    static class Device
    {
        public static void Register()
        {
            PostProcess.Register(PFSense);
            PostProcess.Register(Hikvision);
            PostProcess.Register(Dahua);
            PostProcess.Register(Cisco);
            PostProcess.Register(Synology);
        }

        public static PostProcessor PFSense = (ip, port, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "http" or "https")
            {
                using var client = new HttpClient(new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                });
                using var responseHttp = client.GetAsync($"http://{ip}:{port}/").Result;
                var content = responseHttp.Content.ReadAsStringAsync().Result;

                if (content.ToLower().Contains("pfsense"))
                    return new()
                    {
                        Type = PostProcessResultType.Device,
                        DeviceInfo = new("firewall", "pfsense")
                    };

                using var responseHttps = client.GetAsync($"https://{ip}:{port}/").Result;
                content = responseHttps.Content.ReadAsStringAsync().Result;
                if (content.ToLower().Contains("pfsense"))
                    return new()
                    {
                        Type = PostProcessResultType.Device,
                        DeviceInfo = new("firewall", "pfsense")
                    };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Hikvision = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "Hikvision", RegexOptions.IgnoreCase))
            {
                return new()
                {
                    Type = PostProcessResultType.Device,
                    DeviceInfo = new("Webcam", "Hikvision")
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Dahua = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "Dahua", RegexOptions.IgnoreCase))
            {
                return new()
                {
                    Type = PostProcessResultType.Device,
                    DeviceInfo = new("Webcam", "dahua")
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Cisco = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "Cisco", RegexOptions.IgnoreCase))
            {
                return new()
                {
                    Type = PostProcessResultType.Device,
                    DeviceInfo = new("switch", "cisco")
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Synology = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "Synology", RegexOptions.IgnoreCase))
            {
                return new()
                {
                    Type = PostProcessResultType.Device,
                    DeviceInfo = new("Nas", "synology")
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };
    }
}

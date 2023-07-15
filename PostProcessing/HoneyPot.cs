using System.Diagnostics;

namespace SpiritEye.PostProcessing
{
    static class HoneyPot
    {
        public static void Register()
        {
            PostProcess.Register(Glastopf);
            PostProcess.Register(Kippo);
            PostProcess.Register(HFish);
        }

        public static PostProcessor Glastopf = (ip, port, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "http" or "https")
            {
                using var client = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                })
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                using var responseHttp = client.GetAsync($"http://{ip}:{port}/").Result;

                var content = responseHttp.Content.ReadAsStringAsync().Result;
                if (content.Contains("<h2>Blog Comments</h2>") && content.Contains("Please post your comments for the blog"))
                {
                    return new()
                    {
                        Type = PostProcessResultType.HoneyPot,
                        HoneyPot = new(port, "glastopf")
                    };
                }

                using var responseHttps = client.GetAsync($"https://{ip}:{port}/").Result;

                content = responseHttps.Content.ReadAsStringAsync().Result;
                if (content.Contains("<h2>Blog Comments</h2>") && content.Contains("Please post your comments for the blog"))
                {
                    return new()
                    {
                        Type = PostProcessResultType.HoneyPot,
                        HoneyPot = new(port, "glastopf")
                    };
                }
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Kippo = (ip, port, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "ssh")
            {
                using var process = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = "ssh-keyscan",
                        Arguments = $"-p {port} {ip}",
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };

                process.Start();

                var output = process.StandardError.ReadToEnd();
                if (output.Contains("couldn't match all kex parts"))
                {
                    return new()
                    {
                        Type = PostProcessResultType.HoneyPot,
                        HoneyPot = new(port, "Kippo")
                    };
                }
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor HFish = (ip, port, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "http" or "https")
            {
                using var client = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                })
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                using var responseHttp = client.GetAsync($"http://{ip}:{port}/").Result;

                var content = responseHttp.Content.ReadAsStringAsync().Result;
                if (content.Contains("w-logo-blue.png?ver=20131202") &&
                    content.Contains("ver=5.2.2") &&
                    content.Contains("static/x.js") &&
                    !content.Contains("bcd"))
                {
                    return new()
                    {
                        Type = PostProcessResultType.HoneyPot,
                        HoneyPot = new(port, "HFish")
                    };
                }

                using var responseHttps = client.GetAsync($"https://{ip}:{port}/").Result;

                content = responseHttps.Content.ReadAsStringAsync().Result;
                if (content.Contains("w-logo-blue.png?ver=20131202") &&
                    content.Contains("ver=5.2.2") &&
                    content.Contains("static/x.js") &&
                    !content.Contains("bcd"))
                {
                    return new()
                    {
                        Type = PostProcessResultType.HoneyPot,
                        HoneyPot = new(port, "HFish")
                    };
                }
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };
    }
}

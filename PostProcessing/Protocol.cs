using System.Text.RegularExpressions;

namespace SpiritEye.PostProcessing
{
    static class Protocol
    {
        public static void Register()
        {
            PostProcess.Register(HTTP);
            PostProcess.Register(HTTPS);
            PostProcess.Register(FTP);
            PostProcess.Register(SSH);
            PostProcess.Register(RTSP);
            PostProcess.Register(Redis);
            PostProcess.Register(MySQL);
            PostProcess.Register(Telnet);
            PostProcess.Register(AMQP);
            PostProcess.Register(MongoDB);
        }

        public static PostProcessor HTTP = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "http" &&
                serviceElement.Attribute("tunnel")?.Value is not "ssl")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "http"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor HTTPS = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "https" ||
                serviceElement.Attribute("name")?.Value is "http" &&
                serviceElement.Attribute("tunnel")?.Value is "ssl")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "https"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor FTP = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "ftp")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "ftp"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor SSH = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "ssh")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "ssh"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor RTSP = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "rtsp")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "rtsp"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Redis = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "redis")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "redis"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor MySQL = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "mysql")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "mysql"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Telnet = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "telnet")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "telnet"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor AMQP = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "amqp")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "amqp"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor MongoDB = (_, _, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "mongodb")
            {
                return new()
                {
                    Type = PostProcessResultType.Protocol,
                    Protocol = "mongodb"
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };
    }
}

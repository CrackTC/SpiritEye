using System.Text.RegularExpressions;

namespace SpiritEye.PostProcessing
{
    static class OS
    {
        public static void Register()
        {
            PostProcess.Register(Windows);
            PostProcess.Register(CentOS);
            PostProcess.Register(Ubuntu);
            PostProcess.Register(Debian);
        }

        public static PostProcessor Windows = (_, _, serviceElement) =>
        {
            // contains windows or iis
            if (Regex.IsMatch(serviceElement.ToString(), "windows| iis |sql server", RegexOptions.IgnoreCase))
            {
                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "Windows/N",
                };
            }
            else
            {
                return new()
                {
                    Type = PostProcessResultType.None,
                };
            }
        };

        public static PostProcessor CentOS = (_, _, serviceElement) =>
        {
            // contains centos
            if (Regex.IsMatch(serviceElement.ToString(), "centos", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(serviceElement.ToString(), @"centos[\s_]*([0-9.]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = $"centos/{match.Groups[1].Value}",
                    };
                }
                else
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "centos/N",
                    };
                }
            }
            else
            {
                return new()
                {
                    Type = PostProcessResultType.None,
                };
            }
        };

        public static PostProcessor Ubuntu = (_, _, serviceElement) =>
        {
            Utils.Info(serviceElement.ToString());
            // contains ubuntu
            if (Regex.IsMatch(serviceElement.ToString(), "ubuntu", RegexOptions.IgnoreCase))
            {
                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "ubuntu/N",
                };
            }
            else
            {
                return new()
                {
                    Type = PostProcessResultType.None,
                };
            }
        };

        public static PostProcessor Debian = (_, _, serviceElement) =>
        {
            // contains debian
            if (Regex.IsMatch(serviceElement.ToString(), "debian", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(serviceElement.ToString(), @"debian[\s_]*([0-9.]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = $"debian/{match.Groups[1].Value}",
                    };
                }
                else
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "debian/N",
                    };
                }
            }
            else
            {
                return new()
                {
                    Type = PostProcessResultType.None,
                };
            }
        };
    }
}

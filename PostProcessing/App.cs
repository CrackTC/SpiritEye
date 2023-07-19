using System.Text.RegularExpressions;

namespace SpiritEye.PostProcessing
{
    static class App
    {
        public static void Register()
        {
            PostProcess.Register(OpenSSH);
            PostProcess.Register(OpenSSL);
            PostProcess.Register(WordPress);
            PostProcess.Register(LiteSpeed);
            PostProcess.Register(Jetty);
            PostProcess.Register(Java);
            PostProcess.Register(NodeJS);
            PostProcess.Register(Express);
            PostProcess.Register(AspNET);
            PostProcess.Register(PHP);
            PostProcess.Register(MicrosoftHttpAPI);
            PostProcess.Register(RabbitMQ);
            PostProcess.Register(Apache);
            PostProcess.Register(IIS);
            PostProcess.Register(Nginx);
            PostProcess.Register(MicroHttpd);
            PostProcess.Register(OpenResty);
            PostProcess.Register(Grafana);
            PostProcess.Register(WebLogic);
            PostProcess.Register(ElasticSearch);
        }

        public static PostProcessor OpenSSH = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "openssh", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(serviceElement.Attribute("version")?.Value ?? "", @"([0-9.]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var version = match.Groups[1].Value;
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "openssh/" + version,
                    };
                }
            }
            else if (Regex.IsMatch(serviceElement.Attribute("extrainfo")?.Value ?? "", "openssh", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(serviceElement.Attribute("extrainfo")?.Value ?? "", @"OpenSSH(?:_|/| )([0-9.]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var version = match.Groups[1].Value;
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "openssh/" + version,
                    };
                }
            }
            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor OpenSSL = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("extrainfo")?.Value ?? "", "openssl", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("extrainfo")?.Value ?? "";
                var match = Regex.Match(version, @"OpenSSL(?:_|/| )([0-9.]+)", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    version = match.Groups[1].Value;
                }
                else
                {
                    version = "N";
                }

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "openssl/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor WordPress = (ip, port, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "http" or "https")
            {
                using var client = new HttpClient(new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                    AllowAutoRedirect = false,
                })
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };
                using var responseHttp = client.GetAsync($"http://{ip}:{port}/wp-login.php").Result;

                responseHttp.Headers.TryGetValues("X-Redirect-By", out var xRedirectBy);
                var body = responseHttp.Content.ReadAsStringAsync().Result;

                if (body.Contains("wp-submit") || xRedirectBy?.Any(x => x.Contains("WordPress")) is true)
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "wordpress/N",
                    };
                }

                using var responseHttps = client.GetAsync($"https://{ip}:{port}/wp-login.php").Result;

                responseHttps.Headers.TryGetValues("X-Redirect-By", out xRedirectBy);
                body = responseHttps.Content.ReadAsStringAsync().Result;

                if (body.Contains("wp-submit") || xRedirectBy?.Any(x => x.Contains("WordPress")) is true)
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "wordpress/N",
                    };
                }
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor LiteSpeed = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "LiteSpeed", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "LiteSpeed/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Jetty = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "Jetty", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "Jetty/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Java = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "Java", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";
                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "java/" + version,
                };
            }
            else if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "Apache Tomcat", RegexOptions.IgnoreCase))
            {
                var match = Regex.Match(serviceElement.Attribute("extrainfo")?.Value ?? "", @"Java(?:_|/| )([0-9.]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var version = match.Groups[1].Value;
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "java/" + version,
                    };
                }

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "java/N",
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor NodeJS = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "node", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";
                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "nodejs/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Express = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "express", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";
                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "express/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor AspNET = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "microsoft asp|kestrel", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";
                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "asp.net/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor PHP = (ip, port, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("extrainfo")?.Value ?? "", "php", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.ToString();
                var match = Regex.Match(version, @"PHP(?:_|/| )([0-9.]+)", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    version = match.Groups[1].Value;
                }
                else
                {
                    version = "N";
                }

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "php/" + version,
                };
            }
            else if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "php", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "php/" + version,
                };
            }
            else if (serviceElement.Attribute("name")?.Value is "http" or "https")
            {
                using var client = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                })
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                using var responseHttp = client.GetAsync($"http://{ip}:{port}/").Result;
                responseHttp.Headers.TryGetValues("Set-Cookie", out var cookies);

                if (cookies?.Any(cookie => cookie.Contains("PHPSESSID")) is true)
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "php/N",
                    };
                }

                using var responseHttps = client.GetAsync($"https://{ip}:{port}/").Result;
                responseHttps.Headers.TryGetValues("Set-Cookie", out cookies);

                if (cookies?.Any(cookie => cookie.Contains("PHPSESSID")) is true)
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "php/N",
                    };
                }
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor MicrosoftHttpAPI = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "microsoft httpapi", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "Microsoft-HTTPAPI/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor RabbitMQ = (ip, port, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "rabbitmq", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "rabbitmq/" + version,
                };
            }
            else if (Regex.IsMatch(serviceElement.Attribute("extrainfo")?.Value ?? "", "rabbitmq", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.ToString();
                var match = Regex.Match(version, @"RabbitMQ(?:_|/| )([0-9.]+)", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    version = match.Groups[1].Value;
                }
                else
                {
                    version = "N";
                }

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "rabbitmq/" + version,
                };
            }
            else
            {
                using var client = new HttpClient(new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                })
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                using var responseHttp = client.GetAsync($"http://{ip}:{port}/").Result;
                var content = responseHttp.Content.ReadAsStringAsync().Result;

                if (content.Contains("RabbitMQ"))
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "rabbitmq/N",
                    };
                }

                using var responseHttps = client.GetAsync($"https://{ip}:{port}/").Result;
                content = responseHttps.Content.ReadAsStringAsync().Result;

                if (content.Contains("RabbitMQ"))
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "rabbitmq/N",
                    };
                }
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Apache = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "apache", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "apache/" + version,
                };
            }
            else if (Regex.IsMatch(serviceElement.Attribute("extrainfo")?.Value ?? "", "apache", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.ToString();
                var match = Regex.Match(version, @"Apache(?:_|/| )([0-9.]+)", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    version = match.Groups[1].Value;
                }
                else
                {
                    version = "N";
                }

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "apache/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor IIS = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", " iis ", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "iis/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Nginx = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "nginx", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "nginx/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor MicroHttpd = (ip, port, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "http" or "https")
            {
                using var client = new HttpClient(new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                })
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                using var responseHttp = client.GetAsync($"http://{ip}:{port}/").Result;
                responseHttp.Headers.TryGetValues("Server", out var server);
                var content = responseHttp.Content.ReadAsStringAsync().Result;
                if (server?.Any(x => x.Contains("micro_httpd") || x.Contains("microhttpd")) is true ||
                    content.Contains("micro_httpd") || content.Contains("microhttpd"))
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "micro_httpd/N",
                    };
                }

                using var responseHttps = client.GetAsync($"https://{ip}:{port}/").Result;
                responseHttps.Headers.TryGetValues("Server", out server);
                content = responseHttps.Content.ReadAsStringAsync().Result;
                if (server?.Any(x => x.Contains("micro_httpd") || x.Contains("microhttpd")) is true ||
                    content.Contains("micro_httpd") || content.Contains("microhttpd"))
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "micro_httpd/N",
                    };
                }
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor OpenResty = (_, _, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "openresty", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "openresty/" + version,
                };
            }
            else if (Regex.IsMatch(serviceElement.Attribute("extrainfo")?.Value ?? "", "openresty", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.ToString();
                var match = Regex.Match(version, @"OpenResty(?:_|/| )([0-9.]+)", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    version = match.Groups[1].Value;
                }
                else
                {
                    version = "N";
                }

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "openresty/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor Grafana = (ip, port, serviceElement) =>
        {
            if (serviceElement.Attribute("name")?.Value is "http" or "https")
            {
                using var client = new HttpClient(new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                })
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                using var responseHttp = client.GetAsync($"http://{ip}:{port}/").Result;

                responseHttp.Headers.TryGetValues("Set-Cookie", out var cookie);
                if (cookie?.Any(x => x.ToLower().Contains("grafana")) is true)
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "grafana/N",
                    };
                }

                using var responseHttps = client.GetAsync($"https://{ip}:{port}/").Result;
                responseHttps.Headers.TryGetValues("Set-Cookie", out cookie);
                if (cookie?.Any(x => x.ToLower().Contains("grafana")) is true)
                {
                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "grafana/N",
                    };
                }
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor WebLogic = (ip, port, serviceElement) =>
        {
            if (Regex.IsMatch(serviceElement.Attribute("product")?.Value ?? "", "weblogic", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.Attribute("version")?.Value ?? "N";

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "weblogic/" + version,
                };
            }
            else if (Regex.IsMatch(serviceElement.Attribute("extrainfo")?.Value ?? "", "weblogic", RegexOptions.IgnoreCase))
            {
                var version = serviceElement.ToString();
                var match = Regex.Match(version, @"WebLogic(?:_|/| )([0-9.]+)", RegexOptions.IgnoreCase);

                if (match.Success)
                {
                    version = match.Groups[1].Value;
                }
                else
                {
                    version = "N";
                }

                return new()
                {
                    Type = PostProcessResultType.ServiceApp,
                    ServiceApp = "weblogic/" + version,
                };
            }

            return new()
            {
                Type = PostProcessResultType.None
            };
        };

        public static PostProcessor ElasticSearch = (ip, port, serviceElement) =>
        {
            if (port == 9200 || port == 9300)
            {
                using var client = new HttpClient(new HttpClientHandler()
                {
                    AllowAutoRedirect = true,
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true,
                })
                {
                    Timeout = TimeSpan.FromSeconds(10)
                };

                using var responseHttp = client.GetAsync($"http://{ip}:{port}/").Result;

                var content = responseHttp.Content.ReadAsStringAsync().Result;
                if (content.Contains("elasticsearch"))
                {
                    var match = Regex.Match(content, "lucene_version\"[^\"]*\"([^\"]*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    var version = match.Success ? match.Groups[1].Value : "N";

                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "elasticsearch/" + version,
                    };
                }

                using var responseHttps = client.GetAsync($"https://{ip}:{port}/").Result;

                content = responseHttps.Content.ReadAsStringAsync().Result;
                if (content.Contains("elasticsearch"))
                {
                    var match = Regex.Match(content, "lucene_version\"[^\"]*\"([^\"]*)", RegexOptions.IgnoreCase | RegexOptions.Multiline);
                    var version = match.Success ? match.Groups[1].Value : "N";

                    return new()
                    {
                        Type = PostProcessResultType.ServiceApp,
                        ServiceApp = "elasticsearch/" + version,
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

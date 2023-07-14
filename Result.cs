using System.Text.Json.Serialization;
using System.Diagnostics.CodeAnalysis;

namespace SpiritEye
{
    class ServiceApp
    {
        public string Name { get; }
        public string Version { get; }

        public ServiceApp(string name, string? version)
        {
            Name = name;
            Version = version ?? "N";
        }

        public override string ToString() => Name + "/" + Version;
    }

    class Service
    {
        [JsonPropertyName("port")]
        public int Port { get; }

        [JsonPropertyName("protocol")]
        public string? Protocol { get; }

        [JsonPropertyName("service_app")]
        public IEnumerable<string>? ServiceApps { get; }

        public Service(int port, string? protocol, IEnumerable<string>? serviceApps)
        {
            Port = port;
            Protocol = protocol;
            ServiceApps = serviceApps;
        }

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(ServiceApp))]
        public Service(int port, string? protocol, IEnumerable<ServiceApp>? serviceApps)
        {
            Port = port;
            Protocol = protocol;
            ServiceApps = serviceApps?.Select(serviceApp => serviceApp.ToString());
        }
    }


    class DeviceInfo
    {
        public string Type { get; set; }
        public string Name { get; set; }

        public DeviceInfo(string type, string name)
        {
            Type = type;
            Name = name;
        }

        public override string ToString()
        {
            return Type + "/" + Name;
        }
    }

    class HoneyPot
    {
        public int Port { get; }

        public string Name { get; }

        public HoneyPot(int port, string name = "N")
        {
            Port = port;
            Name = name;
        }

        public override string ToString()
        {
            return Port + "/" + Name;
        }
    }

    class Result
    {
        [JsonPropertyName("services")]
        public IEnumerable<Service> Services { get; }

        [JsonPropertyName("deviceinfo")]
        public IEnumerable<string>? DeviceInfo { get; }

        [JsonPropertyName("honeypot")]
        public IEnumerable<string>? HoneyPots { get; }

        // yyyy-MM-dd HH:mm:ss
        // [JsonPropertyName("timestamp")]
        // public string TimeStamp { get; }

        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(Service))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(DeviceInfo))]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, typeof(HoneyPot))]
        public Result(IEnumerable<Service> services, IEnumerable<DeviceInfo>? deviceInfo, IEnumerable<HoneyPot>? honeyPots)
        {
            Services = services;
            DeviceInfo = deviceInfo?.Select(deviceInfo => deviceInfo.ToString()).Distinct();
            HoneyPots = honeyPots?.Select(honeyPot => honeyPot.ToString());
            // TimeStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }
    }
}

﻿namespace Ocelot.Provider.Nacos.NacosClient
{
    public class Host
    {
        public bool Valid { get; set; }

        public bool Marked { get; set; }

        public string InstanceId { get; set; }

        public bool Healthy { get; set; } = true;

        public string Enabled { get; set; }

        public string Ephemeral { get; set; }

        public string ServiceName { get; set; }

        public string ClusterName { get; set; }

        public int Port { get; set; }

        public string Ip { get; set; }

        public double Weight { get; set; } = 1;

        public string ToInetAddr()
        {
            return Ip + ":" + Port;
        }

        public override string ToString()
        {
            return "Instance{" + "instanceId='" + InstanceId + '\'' + ", ip='" + Ip + '\'' + ", port=" + Port + ", weight="
                    + Weight + ", healthy=" + Healthy + ", enabled=" + Enabled + ", ephemeral=" + Ephemeral
                    + ", clusterName='" + ClusterName + '\'' + ", serviceName='" + ServiceName + '\'' + ", metadata="
                    + Metadata + '}';
        }

        public object Metadata { get; set; }
    }
}

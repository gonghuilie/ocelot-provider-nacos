using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Nacos.V2;

using Newtonsoft.Json;

using Ocelot.Configuration.File;

namespace Ocelot.Provider.Nacos
{
    public class NacosFileConfigurationListener:IListener
    {
        public delegate void OnReceiveConfigInfo(FileConfiguration fileConfig);

        public OnReceiveConfigInfo onReceiveConfigInfo;
        public NacosFileConfigurationListener(OnReceiveConfigInfo action)
        {
            onReceiveConfigInfo = action;
        }
        public void ReceiveConfigInfo(string config)
        {
            var newConfig = JsonConvert.DeserializeObject<FileConfiguration>(config);
            onReceiveConfigInfo.Invoke(newConfig);
        }
    }
}

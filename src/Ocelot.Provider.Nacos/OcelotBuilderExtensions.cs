using System;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Nacos.V2;

using Newtonsoft.Json;

using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Nacos.NacosClient.V2;
using Ocelot.ServiceDiscovery;

namespace Ocelot.Provider.Nacos
{
    public static class OcelotBuilderExtensions
    {
        public static IOcelotBuilder AddNacosDiscovery(this IOcelotBuilder builder)
        {
            builder.Services.AddNacosAspNet(builder.Configuration);
            builder.Services.AddSingleton<ServiceDiscoveryFinderDelegate>(NacosProviderFactory.Get);
            builder.Services.AddSingleton<OcelotMiddlewareConfigurationDelegate>(NacosMiddlewareConfigurationProvider.Get);
            return builder;
        }

        /// <summary>
        /// 使用基于Nacos配置管理和服务注册发现的Ocelot网关管理
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IOcelotBuilder AddOcelotWithNacos(this IServiceCollection services,string ocelotDataId= "Ocelot", string ocelotGroup= "Gateway")
        {
            INacosConfigService configService = services.BuildServiceProvider().GetService<INacosConfigService>();
            IConfiguration configuration = services.BuildServiceProvider().GetRequiredService<IConfiguration>();
            FileConfiguration fileConfiguration = new FileConfiguration();
            try
            {
                var data = configService.GetConfig(ocelotDataId, ocelotGroup, 6000).Result;
                fileConfiguration = JsonConvert.DeserializeObject<FileConfiguration>(data);
            }
            catch(Exception ex)
            {
                throw new Exception("Nacos config fetch failure.", ex);
            }

            IOcelotBuilder builder = new NacosOcelotBuilder(services, configuration, fileConfiguration);

            return builder.AddNacosDiscovery();
        }

    }
}

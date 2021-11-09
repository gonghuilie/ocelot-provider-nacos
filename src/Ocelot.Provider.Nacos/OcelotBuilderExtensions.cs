using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Nacos.V2;

using Newtonsoft.Json;

using Ocelot.Configuration;
using Ocelot.Configuration.ChangeTracking;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Configuration.Setter;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Provider.Nacos.NacosClient.V2;
using Ocelot.Responses;
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
            services.AddOptions();
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


        /// <summary>
        /// 增加配置变更监听
        /// </summary>
        /// <param name="app"></param>
        /// <param name="ocelotDataId"></param>
        /// <param name="ocelotGroup"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseOcelotNacosConfigureListeners(this IApplicationBuilder app, string ocelotDataId = "Ocelot", string ocelotGroup = "Gateway")
        {

            var provider = app.ApplicationServices;
            INacosConfigService nacosConfigService = provider.GetService<INacosConfigService>();
            nacosConfigService.AddListener(ocelotDataId, ocelotGroup, new NacosFileConfigurationListener(newFileConfig =>
            {
                if (newFileConfig.Routes == null || newFileConfig.Routes.Count == 0)
                {
                    throw new Exception("Ocelot routes can not be empty");
                }
              
                var internalConfigCreator = provider.GetService<IInternalConfigurationCreator>();
                Response<IInternalConfiguration> taskResponse = internalConfigCreator.Create(newFileConfig).Result;
                if (taskResponse.IsError)
                {
                    throw new Exception($"Unable to start Ocelot, errors are: {string.Join(",", taskResponse.Errors.Select(x => x.ToString()))}");
                }
                IInternalConfigurationRepository internalConfigurationRepository = provider.GetService<IInternalConfigurationRepository>();
                internalConfigurationRepository.AddOrReplace(taskResponse.Data);


                var adminPath = provider.GetService<IAdministrationPath>();
                var configurations = provider.GetServices<OcelotMiddlewareConfigurationDelegate>();

                // Todo - this has just been added for consul so far...will there be an ordering problem in the future? Should refactor all config into this pattern?
                foreach (var configuration in configurations)
                {
                    configuration(app);
                }
                if (adminPath !=null)
                {
                    var configSetter = provider.GetRequiredService<IFileConfigurationSetter>();
                    configSetter.Set(newFileConfig).Wait();
                }

            }));
            return app;
        }

    }
}

using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Ocelot.Configuration;
using Ocelot.Configuration.ChangeTracking;
using Ocelot.Configuration.Creator;
using Ocelot.Configuration.File;
using Ocelot.Configuration.Repository;
using Ocelot.Configuration.Setter;
using Ocelot.Responses;

namespace Ocelot.Provider.Nacos
{
    public class NacosIoptionsFileConfigurationRepository : IFileConfigurationRepository
    {
        public IServiceCollection Services { get; }
        private readonly IOcelotConfigurationChangeTokenSource _changeTokenSource;
        private readonly IOptionsMonitor<FileConfiguration> _optionsMonitor;

        public NacosIoptionsFileConfigurationRepository(IServiceCollection services)
        {
            var provider = services.BuildServiceProvider();
            _optionsMonitor = provider.GetRequiredService<IOptionsMonitor<FileConfiguration>>();
            _changeTokenSource = provider.GetRequiredService<IOcelotConfigurationChangeTokenSource>();
            Services = services;
        }

        public Task<Response<FileConfiguration>> Get()
        {          
            return Task.FromResult<Response<FileConfiguration>>(new OkResponse<FileConfiguration>(_optionsMonitor.CurrentValue));
        }

        public Task<Response> Set(FileConfiguration fileConfiguration)
        {
            Services.PostConfigure<FileConfiguration>(x =>
            {
                x.Aggregates = fileConfiguration?.Aggregates;
                x.DynamicRoutes = fileConfiguration?.DynamicRoutes;
                x.GlobalConfiguration = fileConfiguration?.GlobalConfiguration;
                x.Routes = fileConfiguration?.Routes;
            });

            _changeTokenSource.Activate();

            return Task.FromResult<Response>(new OkResponse());
        }
    }
}

# ocelot-provider-nacos
Ocelot集成Nacos注册中心组件

### 站在巨人的肩膀上

[https://github.com/softlgl/Ocelot.Provider.Nacos]: softlgl




### 开发环境
+ .Net5.0
+ Ocelot版本 v17.0.0
+ Nacos 2.0.3
+ Nacos访问组件 [nacos-sdk-csharp](https://github.com/nacos-group/nacos-sdk-csharp) 1.2.1

### 实现原理 ###

```c#
public static IOcelotBuilder AddNacosDiscovery(this IOcelotBuilder builder)
{
    builder.Services.AddNacosAspNet(builder.Configuration);
    builder.Services.AddSingleton<ServiceDiscoveryFinderDelegate>(NacosProviderFactory.Get);
    builder.Services.AddSingleton<OcelotMiddlewareConfigurationDelegate>(NacosMiddlewareConfigurationProvider.Get);
    return builder;
}

/// <summary>
/// 使用基于Nacos配置管理和服务注册发现的Ocelot
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
```

DI 注入

```C#
public void ConfigureServices(IServiceCollection services)
{
    ...
    services.AddOptions();
    services.AddLogging();
    services.Configure<NacosSdkOptions>(configuration.GetSection("NacosConfig"));
    services.AddSingleton<INacosConfigService>(p =>
    {
        return new NacosConfigService(p.GetService<ILoggerFactory>(), p.GetService<IOptions<NacosSdkOptions>>());
    });
    services.AddOcelotWithNacos();
    ...
}
```


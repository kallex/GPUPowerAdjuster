using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using NvGPUController;

namespace GPUPowerAdjusterSvc
{
    public class ControlLimits
    {
        public string Name { get; set; }
        public string ProcName { get; set; }
        public int Limit { get; set; }
    }
    public class GPULimitConfig
    {
        public string Brand { get; set; }
        public ControlLimits[] ControlLimits { get; set; }
    }

    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            var typedConfig = configuration.GetSection("NvGPUControllerConfig").Get<GPULimitConfig>();

            //var typedConfig = new GPULimitConfig();
            //configuration.Bind("NvGPUControllerConfig", typedConfig);


            (string name, PowerRuleEvaluator)[] activePowerRules = typedConfig.ControlLimits.Select(
                item =>
                {
                    PowerRuleEvaluator evaluator;
                    if (item.ProcName == null)
                        evaluator = () => (true, 0, item.Limit);
                    else
                        evaluator = NvGPUPowerController.CreateProcessNameEvaluator(item.Limit, item.ProcName);
                    return (item.Name, evaluator);
                }).ToArray();
            var nvController = new NvGPUPowerController(activePowerRules);

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                var result = nvController.EvaluateAndSet();
                if (result.isChanged)
                {
                    _logger.LogInformation("Changed Power Value at: {time} to {}%", DateTime.Now, result.powerPercentage);
                }
                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}

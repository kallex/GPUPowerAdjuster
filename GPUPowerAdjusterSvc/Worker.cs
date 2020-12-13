using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NvGPUController;

namespace GPUPowerAdjusterSvc
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var nvController = new NvGPUPowerController();

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

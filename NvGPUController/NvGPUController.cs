using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GPU.Structures;

namespace NvGPUController
{
    public delegate (bool isMatch, int minPower, int maxPower) PowerRuleEvaluator();
    public class NvGPUPowerController
    {

        public NvGPUPowerController((string name, PowerRuleEvaluator)[] activePowerRules)
        {
            ActivePowerRules = activePowerRules;
        }

        public (string name, PowerRuleEvaluator evaluator)[] ActivePowerRules;
        public static (bool isChanged, int powerPercentage) SetGPUPowerLimit(int targetPercentage)
        {
            var gpus = PhysicalGPU.GetPhysicalGPUs();
            uint powerValue = (uint) targetPercentage * 1000;
            bool anyChanged = false;
            foreach (var gpu in gpus)
            {
                var currentInfo = GPUApi.ClientPowerPoliciesGetStatus(gpu.Handle);

                if (currentInfo.PowerPolicyStatusEntries.Any(item => item.PowerTargetInPCM == powerValue))
                    continue;

                GPUApi.ClientPowerPoliciesSetStatus(gpu.Handle, new PrivatePowerPoliciesStatusV1(new PrivatePowerPoliciesStatusV1.PowerPolicyStatusEntry[]
                {
                    new PrivatePowerPoliciesStatusV1.PowerPolicyStatusEntry(powerValue)
                }));
                anyChanged = true;
            }

            return (anyChanged, targetPercentage);
        }

        public (bool isChanged, int powerPercentage) EvaluateAndSet()
        {
            var matching = ActivePowerRules.Select(item =>
                    new
                    {
                        item.name,
                        result = item.evaluator()
                    })
                .First(item => item.result.isMatch);

            var targetPercentage = matching.result.maxPower;
            var result = SetGPUPowerLimit(targetPercentage);
            return result;
        }

        public static PowerRuleEvaluator CreateProcessNameEvaluator(int limit, string procName)
        {
            PowerRuleEvaluator result = () =>
            {
                int minPower = 0;
                int maxPower = limit;

                //var processes = Process.GetProcessesByName("FahCore_22.exe");
                var processes = Process.GetProcesses();
                bool isMatch = processes.Any(item => item.ProcessName.Contains(procName));

                return (isMatch, minPower, maxPower);
            };
            return result;
        }
    }
}

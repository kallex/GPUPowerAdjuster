using System;
using System.Diagnostics;
using System.Linq;
using NvAPIWrapper.GPU;
using NvAPIWrapper.Native;
using NvAPIWrapper.Native.GPU.Structures;

namespace NvGPUController
{
    public class NvGPUPowerController
    {
        public delegate (bool isMatch, int minPower, int maxPower) PowerRuleEvaluator();

        public (string name, PowerRuleEvaluator evaluator)[] ActivePowerRules = new (string name, PowerRuleEvaluator)[]
        {
            ("Folding@Home", () =>
            {
                int minPower = 0;
                int maxPower = 72;

                //var processes = Process.GetProcessesByName("FahCore_22.exe");
                var processes = Process.GetProcesses();
                bool isMatch = processes.Any(item => item.ProcessName.Contains("FahCore_22"));

                return (isMatch, minPower, maxPower);
            }),
            ("Default", () => (true, 100, 100))
        };

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
    }
}

using System;
using System.Globalization;
using System.Windows.Data;

using Optuna.Sampler;
using Optuna.Sampler.Dashboard;
using Optuna.Sampler.OptunaHub;

namespace Tunny.WPF.Common
{
    internal sealed class SamplerUIEnableConvertor : IMultiValueConverter
    {
        public object Convert(object[] values, System.Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length != 4)
            {
                return false;
            }

            bool isMultiObjective = (bool)values[0];
            bool hasConstraint = (bool)values[1];
            bool isHumanInTheLoop = (bool)values[2];
            var samplerType = (SelectSamplerType)values[3];

            SamplerBase sampler = GetSelectSampler(samplerType);

            return CheckCanEnable(isMultiObjective, hasConstraint, isHumanInTheLoop, sampler);
        }

        private static bool CheckCanEnable(bool isMultiObjective, bool hasConstraint, bool isHumanInTheLoop, SamplerBase sampler)
        {
            bool result = true;
            if (isHumanInTheLoop)
            {
                if (!sampler.RecommendedHumanInTheLoop)
                {
                    result = false;
                }
            }
            else
            {
                if (hasConstraint && !sampler.SupportsConstraint)
                {
                    result = false;
                }
                else if (isMultiObjective && !sampler.SupportsMultiObjective)
                {
                    result = false;
                }
                else if (!isMultiObjective && sampler.IsSinglePurposeRestricted)
                {
                    result = false;
                }
            }
            return result;
        }

        public object[] ConvertBack(object value, System.Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private static SamplerBase GetSelectSampler(SelectSamplerType selectSamplerType)
        {
            SamplerBase sampler;
            switch (selectSamplerType)
            {
                case SelectSamplerType.TPE:
                    sampler = new TpeSampler();
                    break;
                case SelectSamplerType.GpOptuna:
                    sampler = new GPSampler();
                    break;
                case SelectSamplerType.GpBoTorch:
                    sampler = new BoTorchSampler();
                    break;
                case SelectSamplerType.PreferentialGp:
                    sampler = new PreferentialGpSampler();
                    break;
                case SelectSamplerType.NSGAII:
                    sampler = new NSGAIISampler();
                    break;
                case SelectSamplerType.NSGAIII:
                    sampler = new NSGAIIISampler();
                    break;
                case SelectSamplerType.CmaEs:
                    sampler = new CmaEsSampler();
                    break;
                case SelectSamplerType.Random:
                    sampler = new RandomSampler();
                    break;
                case SelectSamplerType.QMC:
                    sampler = new QMCSampler();
                    break;
                case SelectSamplerType.BruteForce:
                    sampler = new BruteForceSampler();
                    break;
                case SelectSamplerType.AUTO:
                    sampler = new AutoSampler();
                    break;
                case SelectSamplerType.MOEAD:
                    sampler = new MOEADSampler();
                    break;
                case SelectSamplerType.MoCmaEs:
                    sampler = new MoCmaEsSampler();
                    break;
                case SelectSamplerType.DE:
                    sampler = new DESampler();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(selectSamplerType), selectSamplerType, null);
            }
            return sampler;
        }
    }
}

using System;

using Tunny.Core.Util;

namespace Tunny.Core.Input
{
    [Serializable]
    [LoggingAspect]
    public class NumberVariable : VariableBase
    {
        public double LowerBond { get; }
        public double UpperBond { get; }
        public bool IsInteger { get; }
        public double Epsilon { get; }
        public double Value { get; }
        public bool IsLogScale { get; set; }

        public NumberVariable(
            double lowerBond, double upperBond, bool isInteger, string nickName, double epsilon,
            double value, Guid id)
        : base(nickName, id)
        {
            LowerBond = lowerBond;
            UpperBond = upperBond;
            IsInteger = isInteger;
            Epsilon = epsilon;
            Value = value;
        }
    }
}

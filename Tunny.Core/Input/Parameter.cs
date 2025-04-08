using System.Globalization;

using Tunny.Core.Util;

namespace Tunny.Core.Input
{
    [LoggingAspect]
    public class Parameter
    {
        public string Category { get; }
        public double Number { get; }
        public bool HasNumber => !HasCategory;
        public bool HasCategory => !string.IsNullOrEmpty(Category);

        public Parameter(double number)
        {
            Number = number;
        }

        public Parameter(string category)
        {
            Category = category;
        }

        public override string ToString()
        {
            if (HasCategory)
            {
                return Category;
            }
            else
            {
                return Number.ToString(CultureInfo.InvariantCulture);
            }
        }
    }
}

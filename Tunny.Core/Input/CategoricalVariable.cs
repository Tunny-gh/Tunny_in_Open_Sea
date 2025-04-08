using System;

using Tunny.Core.Util;

namespace Tunny.Core.Input
{
    [Serializable]
    [LoggingAspect]
    public class CategoricalVariable : VariableBase
    {
        public string[] Categories { get; }
        public string SelectedItem { get; }

        public CategoricalVariable(string[] categories, string selectedItem, string nickName, Guid id)
         : base(nickName, id)
        {
            Categories = categories;
            SelectedItem = selectedItem;
        }
    }
}

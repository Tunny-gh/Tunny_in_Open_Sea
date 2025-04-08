using System;

using Tunny.Core.Util;

namespace Tunny.Core.Input
{
    [LoggingAspect]
    public class VariableBase
    {
        public string NickName { get; }
        public Guid InstanceId { get; }

        public VariableBase(string nickName, Guid id)
        {
            NickName = nickName;
            InstanceId = id;
        }
    }
}

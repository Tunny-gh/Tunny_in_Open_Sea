using MethodBoundaryAspect.Fody.Attributes;

namespace Tunny.Core.Util
{
    public class LoggingAspect : OnMethodBoundaryAspect
    {
        public override void OnEntry(MethodExecutionArgs arg)
        {
            TLog.Verbose("OnEntry: {ClassName}.{MethodName} - Arguments: {@Arguments}",
                arg.Method.DeclaringType?.Name,
                arg.Method.Name,
                arg.Arguments);
        }

        public override void OnExit(MethodExecutionArgs arg)
        {
            TLog.Verbose("OnExit: {ClassName}.{MethodName} - ReturnValue: {@ReturnValue}",
                arg.Method.DeclaringType?.Name,
                arg.Method.Name,
                arg.ReturnValue);
        }

        public override void OnException(MethodExecutionArgs arg)
        {
            TLog.Error(arg.Exception, "OnException: {ClassName}.{MethodName} - Message: {Message}",
                arg.Method.DeclaringType?.Name,
                arg.Method.Name,
                arg.Exception.Message);
        }
    }
}

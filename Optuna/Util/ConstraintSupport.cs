namespace Optuna.Util
{
    /// <summary>
    /// Indicates whether the sampler supports constraints.
    /// </summary>
    public enum ConstraintSupport
    {
        None,
        Supported,
        OnlyWithConstraint,
    }
}

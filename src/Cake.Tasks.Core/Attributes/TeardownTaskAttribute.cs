namespace Cake.Tasks.Core;

public sealed class TeardownTaskAttribute : BaseTaskAttribute
{
    public int Order { get; set; }
}

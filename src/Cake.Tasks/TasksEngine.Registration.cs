using System.IO;
using Cake.Core.Diagnostics;

namespace Cake.Tasks.Module
{
    public sealed partial class TasksEngine
    {
        private void RegisterPlugins()
        {
            Log.Information(Path.GetFullPath(Context.Configuration.GetValue("Paths_AddIns")));
        }
    }
}

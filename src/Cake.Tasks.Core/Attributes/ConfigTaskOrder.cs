namespace Cake.Tasks.Core
{
    public static class ConfigTaskOrder
    {
        public const int Default = 0;

        /// <summary>
        ///     Priority config tasks should run after all the normal config tasks, but before any CI
        ///     system tasks.
        /// </summary>
        public const int Priority = 100;

        /// <summary>
        ///     Config tasks for CI systems should run after all the normal and priority tasks, but
        ///     before any recipe tasks.
        /// </summary>
        public const int CiSystem = 1000;

        /// <summary>
        ///     Config tasks for recipes should always run last, as they are application-specific.
        /// </summary>
        public const int Recipe = int.MaxValue;
    }
}

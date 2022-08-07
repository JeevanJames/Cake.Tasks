namespace Cake.Tasks.Tests;

public sealed class TestConfig : PluginConfig
{
    public TestConfig(TaskConfig taskConfig)
        : base(taskConfig)
    {
    }

    public ConfigValue<bool> BoolConfig
    {
        get => GetValue<bool>(nameof(BoolConfig));
        set => Set(nameof(BoolConfig), value);
    }

    public ConfigValue<int> IntConfig
    {
        get => GetValue<int>(nameof(IntConfig));
        set => Set(nameof(IntConfig), value);
    }
}

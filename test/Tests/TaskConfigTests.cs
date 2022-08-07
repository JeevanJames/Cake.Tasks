namespace Cake.Tasks.Tests;

public sealed class TaskConfigTests
{
    private readonly TaskConfig _config;
    private readonly TestConfig _testConfig;

    public TaskConfigTests()
    {
        _config = TaskConfig.Current;
        _config.Data.Clear();

        _testConfig = _config.Load<TestConfig>();
    }

    [Fact]
    public void Can_write_property_through_plugin()
    {
        _testConfig.BoolConfig = true;

        _config.Data.Count.ShouldBe(1);

        bool pluginValue = _testConfig.BoolConfig.Resolve();

        pluginValue.ShouldBeTrue();
    }

    [Fact]
    public void Can_read_raw_value_through_plugin()
    {
        _config.Data.Add(nameof(TestConfig.BoolConfig), true);

        bool pluginValue = _testConfig.BoolConfig.Resolve();

        pluginValue.ShouldBeTrue();
    }

    [Fact]
    public void Can_read_raw_string_value_through_plugin()
    {
        _config.Data.Add(nameof(TestConfig.BoolConfig), "true");
        _testConfig.BoolConfig.Resolve().ShouldBe(true);

        _config.Data.Add(nameof(TestConfig.IntConfig), "610");
        _testConfig.IntConfig.Resolve().ShouldBe(610);
    }
}

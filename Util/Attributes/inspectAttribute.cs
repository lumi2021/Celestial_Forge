namespace GameEngine.Util.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class InspectAttribute : Attribute
{

    public enum Usage { usage_default, multiline_text, range}
    public readonly Usage usage;
    public readonly string parameters;

    public InspectAttribute()
    {
        usage = Usage.usage_default;
        parameters = "";
    }
    public InspectAttribute(Usage usage)
    {
        this.usage = usage;
        parameters = "";
    }
    public InspectAttribute(Usage usage, string args)
    {
        this.usage = usage;
        parameters = args;
    }

}
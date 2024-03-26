using GameEngine.Util.Values;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameEngine.Util.Core;

public class ProjectSettings
{

    public Vector2<int> canvasDefaultSize = new(800, 600);

    public bool projectLoaded = false;

    public string projectPath = "";

    public string entryScene = "";


    public static string Serialize(ProjectSettings obj)
    {
        var serializer = new SerializerBuilder()
            .WithTypeConverter(new ProjectSettingsSerialiser())
            .Build();

        return serializer.Serialize(obj);
    }
    public static object? Deserialize(string pSettings)
    {
        var deserializer = new DeserializerBuilder()
            .WithTypeConverter(new ProjectSettingsSerialiser())
            .Build();

        return deserializer.Deserialize<ProjectSettings>(pSettings);
    }

}

class ProjectSettingsSerialiser : IYamlTypeConverter
{
    bool IYamlTypeConverter.Accepts(Type type) => type == typeof(ProjectSettings);

    object? IYamlTypeConverter.ReadYaml(IParser parser, Type type)
    {
        
        /*
        while(!parser.TryConsume<MappingEnd>(out var _))
        {
            Console.WriteLine(parser.Current);
            parser.MoveNext();
        }
        */

        parser.Consume<MappingStart>();
        Console.WriteLine("Printing yaml content:");

        var key1 = parser.Consume<Scalar>().Value;
        parser.Consume<SequenceStart>();
        var value1_x = parser.Consume<Scalar>().Value;
        var value1_y = parser.Consume<Scalar>().Value;
        Console.WriteLine($"- {key1}: [{value1_x}, {value1_y}];");
        parser.Consume<SequenceEnd>();

        var key2 = parser.Consume<Scalar>().Value;
        var value2 = parser.Consume<Scalar>().Value;
        Console.WriteLine($"- {key2}: {value2};");

        while(!parser.TryConsume<MappingEnd>(out _)) parser.MoveNext();

        return null;

    }

    void IYamlTypeConverter.WriteYaml(IEmitter emitter, object? value, Type type)
    {

        var obj = (ProjectSettings)value!;

        emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

        emitter.Emit(new Comment("Window & Viewport", false));
        emitter.Emit(new Scalar("canvas-default-size"));
        emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Flow));

        emitter.Emit(new Scalar(null, null, obj.canvasDefaultSize.X.ToString(), ScalarStyle.Plain, true, false));
        emitter.Emit(new Scalar(null, null, obj.canvasDefaultSize.Y.ToString(), ScalarStyle.Plain, true, false));

        emitter.Emit(new SequenceEnd());

        emitter.Emit(new Comment("Run", false));
        emitter.Emit(new Scalar("entry-point-scene"));
        emitter.Emit(new Scalar(obj.entryScene));

        emitter.Emit(new MappingEnd());

    }
}

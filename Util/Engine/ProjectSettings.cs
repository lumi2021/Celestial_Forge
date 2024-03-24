using GameEngine.Util.Values;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace GameEngine.Util.Core;

public class ProjectSettings
{

    //public static ProjectSettings Load(FileReference path)
    //{
    //}

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

}

class ProjectSettingsSerialiser : IYamlTypeConverter
{
    bool IYamlTypeConverter.Accepts(Type type) => type == typeof(ProjectSettings);

    object? IYamlTypeConverter.ReadYaml(IParser parser, Type type)
    {
        throw new NotImplementedException();
    }

    void IYamlTypeConverter.WriteYaml(IEmitter emitter, object? value, Type type)
    {

        var obj = (ProjectSettings)value!;

        emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

        emitter.Emit(new Comment("# Window & Viewport"));
        emitter.Emit(new Scalar("canvas-default-size"));
        emitter.Emit(new SequenceStart(null, null, true, SequenceStyle.Flow));

        emitter.Emit(new Scalar(null, null, obj.canvasDefaultSize.X.ToString(), ScalarStyle.Plain, true, false));
        emitter.Emit(new Scalar(null, null, obj.canvasDefaultSize.Y.ToString(), ScalarStyle.Plain, true, false));

        emitter.Emit(new SequenceEnd());

        emitter.Emit(new Comment("# Run"));
        emitter.Emit(new Scalar("entry-point-scene"));
        emitter.Emit(new Scalar(entryScene));

        emitter.Emit(new MappingEnd());

    }
}
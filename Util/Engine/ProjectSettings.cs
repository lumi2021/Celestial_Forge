using GameEngine.Util.Values;

namespace GameEngine.Util.Core;

public class ProjectSettings
{

    public Vector2<int> canvasDefaultSize = new(800, 600);

    public bool projectLoaded = false;

    public string projectPath = "";

    public string entryScene = "";

}
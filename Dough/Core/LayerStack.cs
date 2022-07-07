namespace Dough.Core;

public class LayerStack : IDisposable
{
    private readonly List<Layer> _layers = new();
    private readonly Application _application;
    private int _overlayCount = 0;

    public LayerStack(Application app)
    {
        _application = app;
    }

    public void PushLayer(Layer layer)
    {
        layer.SetApplication(_application);
        _layers.Insert(_layers.Count - _overlayCount, layer);
        layer.OnAttach();
    }

    public void PopLayer(Layer layer)
    {
        layer.OnDetach();
        _layers.Remove(layer);
    }

    public void PushOverlay(Layer layer)
    {
        layer.SetApplication(_application);
        _layers.Add(layer);
        _overlayCount++;
        layer.OnAttach();
    }

    public void PopOverlay(Layer layer)
    {
        layer.OnDetach();
        _layers.Remove(layer);
        _overlayCount--;
    }

    public void UpdateLayers(double deltaTime)
    {
        foreach (var layer in _layers)
            layer.OnUpdate(deltaTime);
    }

    public void Dispose()
    {
        foreach (var layer in _layers)
            layer.Dispose();
    }
}

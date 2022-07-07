namespace Dough.Core;

public class Layer : IDisposable
{
    public string Name
    {
        get;
        private set;
    }

    protected Application application;

    public Layer(string newName = "Layer")
    {
        Name = newName;
    }

    public virtual void SetApplication(Application app) => application = app;

    public virtual void OnAttach()
    { }

    public virtual void OnDetach()
    { }

    public virtual void OnUpdate(double deltaTime) 
    { }

    public virtual void Dispose()
    { }
}

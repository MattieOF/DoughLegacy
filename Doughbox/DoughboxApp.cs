using Dough.Core;

namespace Doughbox
{
    public class ExampleLayer : Layer
    {
        public ExampleLayer()
            : base("Example Layer")
        { }

        public override void OnUpdate(double deltaTime)
        {
            Log.Info("Updating ExampleLayer!");
        }
    }

    public class ExampleOverlayLayer : Layer
    {
        public ExampleOverlayLayer()
            : base("Example Overlay Layer")
        { }

        public override void OnUpdate(double deltaTime)
        {
            Log.Info("Updating ExampleOverlayLayer!");
        }
    }
    
    public class DoughboxApp : Application
    {
        public DoughboxApp(ApplicationSpecification specification)
            : base(specification)
        {
            Log.Info("I am sentient.");
            layerStack.PushLayer(new ExampleLayer());
            layerStack.PushLayer(new ExampleLayer());
            layerStack.PushOverlay(new ExampleOverlayLayer());
            layerStack.PushLayer(new ExampleLayer());
            layerStack.PushOverlay(new ExampleOverlayLayer());
            layerStack.PushLayer(new ExampleLayer());
        }
    }
}

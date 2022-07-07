using Dough.Core;

namespace Doughbox
{
    public class DoughboxApp : Application
    {
        public DoughboxApp(ApplicationSpecification specification)
            : base(specification)
        {
            Log.Info("I am sentient.");
        }
    }
}

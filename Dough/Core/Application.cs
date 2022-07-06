using System;
using NLog;

namespace Dough.Core
{
    public class Application : IDisposable
    {
        public ApplicationSpecification Specification
        {
            get;
            protected set;
        }
        
        public Application(ApplicationSpecification specification)
        {
            Specification = specification;
            Environment.CurrentDirectory = Specification.workingDirectory;
            Log.Init(Specification.appName);
            ConfigManager.InitConfig();
        }

        public bool Run()
        {
            return true;
        }
        
        public void Dispose()
        {
            Log.Shutdown();
        }
    }

    public struct ApplicationSpecification
    {
        public string appName;
        public string workingDirectory;
        public string[] commandLineArguments;

        public ApplicationSpecification(string name, string[] arguments, string workingDir = ".")
        {
            appName = name;
            workingDirectory = workingDir;
            commandLineArguments = arguments;
        }
    }
}

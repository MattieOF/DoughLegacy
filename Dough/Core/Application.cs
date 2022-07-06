using System;

namespace Dough.Core
{
    public class Application : IDisposable
    {
        public ApplicationSpecification Specification
        {
            get;
            private set;
        }
        
        public Application(ApplicationSpecification specification)
        {
            Specification = specification;
            Environment.CurrentDirectory = Specification.workingDirectory;
            Log.Init(Specification.appName);
        }

        public void Run()
        {
            
        }
        
        public void Dispose()
        {
            
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

using System;
using Dough.Core;

namespace Doughbox
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            bool completed = false;
            
            while (!completed)
                using (var app = new DoughboxApp(new ApplicationSpecification("Doughbox", args)))
                    completed = app.Run();
        }
    }
}

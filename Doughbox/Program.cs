using System;
using Dough.Core;

namespace Doughbox
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            using (var app = new DoughboxApp(new ApplicationSpecification("Doughbox", args)))
                app.Run();
        }
    }
}

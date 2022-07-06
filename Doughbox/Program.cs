using System;
using Dough.Core;

namespace Doughbox
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Engine.Init();
            using (var app = new DoughboxApp())
                app.Run();
            Engine.Shutdown();
        }
    }
}

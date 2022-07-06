using System;

namespace Doughbox
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            DoughboxApp app = new DoughboxApp();
            app.Run();
            app.Dispose();
        }
    }
}

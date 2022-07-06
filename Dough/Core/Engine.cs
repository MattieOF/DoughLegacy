namespace Dough.Core
{
    public static class Engine
    {
        public static void Init()
        {
            Log.Init();
        }

        public static void Shutdown()
        {
            Log.Shutdown();
        }
    }
}
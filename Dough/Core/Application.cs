﻿using System;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Dough.Core
{
    public class Application : IDisposable
    {
        protected IWindow window;

        [ConfigValue("WindowSize", ConfigFiles.EngineVideo)]
        private static Vector2D<int> _windowSize = new Vector2D<int>(1280, 720);

        [ConfigValue("Fullscreen", ConfigFiles.EngineVideo)]
        private static bool _fullscreen = false;

        [ConfigValue("BorderlessWindow", ConfigFiles.EngineVideo)]
        private static bool _borderlessWindow = false;

        [ConfigValue("Fullscreen", ConfigFiles.EngineVideo)]
        private static bool _vsync = true;

        [ConfigValue("MaxFPS", ConfigFiles.EngineVideo)]
        private static int _maxFps = 120;

        // Window related utility properties 
        private WindowBorder Border => (_fullscreen && _borderlessWindow ? WindowBorder.Hidden : WindowBorder.Fixed);
        private WindowState State => _fullscreen
            ? (_borderlessWindow ? WindowState.Maximized : WindowState.Fullscreen)
            : WindowState.Normal;
        
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
            Log.Configure();

            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = Specification.appName;
            windowOptions.Size = _windowSize;

            try
            {
                window = Window.Create(windowOptions);
                window.Center();
            }
            catch (Exception e)
            {
                Log.EngineFatal(e, "Failed to create window!");
                throw;
            }
        }

        public bool Run()
        {
            window.Run();
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

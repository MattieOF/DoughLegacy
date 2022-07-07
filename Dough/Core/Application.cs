using System.Numerics;
using System.Reflection;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;

namespace Dough.Core
{
    public class Application : IDisposable
    {
        protected IWindow window;
        protected IInputContext input;

        [ConfigValue("WindowSize", ConfigFiles.EngineVideo)]
        private static Vector2D<int> _windowSize = new Vector2D<int>(1280, 720);

        [ConfigValue("Fullscreen", ConfigFiles.EngineVideo)]
        private static bool _fullscreen = false;

        [ConfigValue("BorderlessWindow", ConfigFiles.EngineVideo, "If using fullscreen, should a borderless window be used?")]
        private static bool _borderlessWindow = false;

        [ConfigValue("VSync", ConfigFiles.EngineVideo)]
        private static bool _vsync = true;

        [ConfigValue("MaxFPS", ConfigFiles.EngineVideo, "If vsync is disabled, maximum frame rate to run at")]
        private static int _maxFps = 120;

        // Window related utility properties 
        /// <summary>
        /// Border to use: Hidden if fullscreen/borderless window, fixed otherwise.
        /// </summary>
        private WindowBorder Border => (_fullscreen && _borderlessWindow ? WindowBorder.Hidden : WindowBorder.Fixed);
        /// <summary>
        /// Window state: Fullscreen or normal depending on <see cref="_fullscreen"/>
        /// </summary>
        private WindowState State => _fullscreen
            ? (_borderlessWindow ? WindowState.Maximized : WindowState.Fullscreen)
            : WindowState.Normal;
        
        /// <summary>
        /// Application spec defines name, arguments, and working directory.
        /// </summary>
        public ApplicationSpecification Specification
        {
            get;
            protected set;
        }
        
        /// <summary>
        /// The core Application constructor initialises the engine, and then the Application window based on the provided <see cref="ApplicationSpecification"/>.
        /// </summary>
        /// <param name="specification"><see cref="ApplicationSpecification"/> to use </param>
        public Application(ApplicationSpecification specification)
        {
            Specification = specification;
            Environment.CurrentDirectory = Specification.workingDirectory;
            Log.Init(Specification.appName);
            
            // Init config
            ConfigManager.InitConfig(); // Init engine config
            // Caller for this function should be the clients override of Application
            var caller = Assembly.GetCallingAssembly();
            if (Assembly.GetExecutingAssembly() != caller) // Init config of app
                ConfigManager.InitConfig(assembly: caller);
            
            Log.Configure();

            WindowOptions windowOptions = WindowOptions.Default;
            windowOptions.Title = Specification.appName;
            windowOptions.Size = _windowSize;
            windowOptions.WindowBorder = Border;
            windowOptions.WindowState = State;
            windowOptions.FramesPerSecond = _maxFps;
            windowOptions.VSync = _vsync;

            try
            {
                window = Window.Create(windowOptions);
                window.Initialize();
                window.Center();
            }
            catch (Exception e)
            {
                Log.EngineFatal(e, "Failed to create window!");
                throw;
            }

            input = window.CreateInput();
            RefreshInputDevices();
        }

        protected void RefreshInputDevices()
        {
            if (input is null)
            {
                Log.EngineError($"Attempted to RefreshInputDevices() in Application \"{Specification.appName}\" but the input context is null!");
                return;
            }

            foreach (IKeyboard keyboard in input.Keyboards)
            {
                keyboard.KeyChar += OnKeyChar;
                keyboard.KeyDown += OnKeyDown;
                keyboard.KeyUp   += OnKeyUp;
            }

            foreach (IMouse mouse in input.Mice)
            {
                mouse.Click += OnMouseClick;
                mouse.Scroll += OnScroll;
                mouse.DoubleClick += OnMouseDoubleClick;
                mouse.MouseDown += OnMouseDown;
                mouse.MouseUp += OnMouseUp;
                mouse.MouseMove += OnMouseMove;
            }
        }

        protected void OnMouseMove(IMouse mouse, Vector2 newPosition)
        {
        }

        protected void OnMouseUp(IMouse mouse, MouseButton button)
        {
        }

        protected void OnMouseDown(IMouse mouse, MouseButton button)
        {
        }

        protected void OnMouseDoubleClick(IMouse mouse, MouseButton button, Vector2 position)
        {
        }

        protected void OnScroll(IMouse mouse, ScrollWheel scrollWheel)
        {
        }

        protected void OnMouseClick(IMouse mouse, MouseButton button, Vector2 position)
        {
        }

        protected void OnKeyUp(IKeyboard keyboard, Key key, int phyiscalKeyId)
        {
        }

        protected void OnKeyChar(IKeyboard keyboard, char character)
        {
        }

        protected void OnKeyDown(IKeyboard keyboard, Key key, int physicalKeyId)
        {
        }

        /// <summary>
        /// Run the application.
        /// </summary>
        /// <returns>True if ready to exit, false if requesting a restart.</returns>
        public bool Run()
        {
            window.Run();
            return true;
        }
        
        /// <summary>
        /// Shutdown the application and engine.
        /// </summary>
        public void Dispose()
        {
            window.Dispose();
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

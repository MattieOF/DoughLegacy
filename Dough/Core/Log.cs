using System;
using System.Text;
using NLog;
using NLog.Conditions;
using NLog.Config;
using NLog.Targets;

namespace Dough.Core
{
    public static class Log
    {
        private static Logger _engineLogger;
        private static Logger _appLogger;
        
        private static FileTarget _logFile;
        private static ColoredConsoleTarget _logConsole;
        
        [ConfigValue("LogEnabled", ConfigFiles.EngineCore)]
        private static bool _logEnabled = true;
        
        public static void Init(string appName = "App")
        {
            var logConfig = new LoggingConfiguration();
            
            _logFile = new FileTarget("logFile")
            {
                FileName = "Logs/log.txt",
                Encoding = Encoding.UTF8,
                ArchiveOldFileOnStartup = true,
            };
            _logConsole = new ColoredConsoleTarget("logConsole")
            {
                Encoding = Encoding.UTF8
            };
            _logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            _logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            _logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Fatal"), ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange));
            
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, _logConsole);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, _logFile);

            LogManager.Configuration = logConfig;
            
            _engineLogger = LogManager.Setup().GetLogger("Dough");
            _appLogger = LogManager.Setup().GetLogger(appName);
            
            EngineInfo("Initialised log!");
        }

        public static void Configure()
        {
            // This is a bit of a hack. Since we have to init the logger before the config system to
            // allow the config system to log (which I believe is more important), we have to configure
            // based on configvalues after we have already called Log.Init(). 
            // For now, the solution is just this Reconfigure function which is called directly after
            // the config systems initialisation, however there are a couple other things we could do instead:
            // 1. Not have config for the log. As long as we're not opening a console for distribution builds, it's fine.
            // 2. Have the log manage its own config. Could still just use Tomlet, serialise a "LogConfig" class. ez.
            if (!_logEnabled)
            {
                // If log is disabled, just null loggers.
                _engineLogger = LogManager.CreateNullLogger();
                _appLogger = LogManager.CreateNullLogger();
            }
        }

        public static void Shutdown()
        {
            LogManager.Shutdown();
        }

        public static void Trace(string msg) => _appLogger.Trace(msg);
        public static void Info(string msg) => _appLogger.Info(msg);
        public static void Warn(string msg) => _appLogger.Warn(msg);
        public static void Error(string msg) => _appLogger.Error(msg);
        public static void Fatal(string msg) => _appLogger.Fatal(msg);
        public static void Fatal(Exception e, string msg) => _appLogger.Fatal(e, msg);

        internal static void EngineTrace(string msg) => _engineLogger.Trace(msg);
        internal static void EngineInfo(string msg) => _engineLogger.Info(msg);
        internal static void EngineWarn(string msg) => _engineLogger.Warn(msg);
        internal static void EngineError(string msg) => _engineLogger.Error(msg);
        internal static void EngineFatal(string msg) => _engineLogger.Fatal(msg);
        internal static void EngineFatal(Exception e, string msg) => _engineLogger.Fatal(e, msg);
    }
}

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
        private static Logger engineLogger;
        private static Logger appLogger;
        
        public static void Init(string appName = "App")
        {
            var logConfig = new LoggingConfiguration();
            
            var logFile = new FileTarget("logFile")
            {
                FileName = "Logs/log.txt",
                Encoding = Encoding.UTF8,
                ArchiveOldFileOnStartup = true,
            };
            var logConsole = new ColoredConsoleTarget("logConsole")
            {
                Encoding = Encoding.UTF8
            };
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Warn"), ConsoleOutputColor.Yellow, ConsoleOutputColor.NoChange));
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Error"), ConsoleOutputColor.Red, ConsoleOutputColor.NoChange));
            logConsole.RowHighlightingRules.Add(new ConsoleRowHighlightingRule(ConditionParser.ParseExpression("level == LogLevel.Fatal"), ConsoleOutputColor.DarkRed, ConsoleOutputColor.NoChange));
            
            logConfig.AddRule(LogLevel.Trace, LogLevel.Fatal, logConsole);
            logConfig.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);

            LogManager.Configuration = logConfig;

            engineLogger = LogManager.Setup().GetLogger("Dough");
            appLogger = LogManager.Setup().GetLogger(appName);
            
            EngineInfo("Initialised log!");
        }

        public static void Shutdown()
        {
            LogManager.Shutdown();
        }

        public static void Trace(string msg) => appLogger.Trace(msg);
        public static void Info(string msg) => appLogger.Info(msg);
        public static void Warn(string msg) => appLogger.Warn(msg);
        public static void Error(string msg) => appLogger.Error(msg);
        public static void Fatal(string msg) => appLogger.Fatal(msg);
        public static void Fatal(Exception e, string msg) => appLogger.Fatal(e, msg);

        internal static void EngineTrace(string msg) => engineLogger.Trace(msg);
        internal static void EngineInfo(string msg) => engineLogger.Info(msg);
        internal static void EngineWarn(string msg) => engineLogger.Warn(msg);
        internal static void EngineError(string msg) => engineLogger.Error(msg);
        internal static void EngineFatal(string msg) => engineLogger.Fatal(msg);
        internal static void EngineFatal(Exception e, string msg) => engineLogger.Fatal(e, msg);
    }
}

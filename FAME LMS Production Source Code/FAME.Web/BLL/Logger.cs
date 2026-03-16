using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Serilog;
using Serilog.Events;

namespace First_Aid_Made_Easy.BLL
{
    public static class JzLogger
    {
        private static readonly ILogger Errorlog;
        private static readonly ILogger Warninglog;
        private static readonly ILogger Debuglog;
        private static readonly ILogger Verboselog;
        private static readonly ILogger Fatallog;
        private static readonly ILogger Information;

        static JzLogger()
        {

            // 5 MB = 5242880 bytes

            Errorlog = new LoggerConfiguration()
                .MinimumLevel.Error()
               .WriteTo.File(System.Web.Hosting.HostingEnvironment.MapPath("~/ErrorLog/Error/log.txt"),
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 5242880,
                rollOnFileSizeLimit: true)
                .CreateLogger();

            Warninglog = new LoggerConfiguration()
                .MinimumLevel.Warning()
              .WriteTo.File(System.Web.Hosting.HostingEnvironment.MapPath("~/ErrorLog/Warning/log.txt"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 5242880,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            Debuglog = new LoggerConfiguration()
                .MinimumLevel.Debug()
              .WriteTo.File(System.Web.Hosting.HostingEnvironment.MapPath("~/ErrorLog/Debug/log.txt"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 5242880,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            Verboselog = new LoggerConfiguration()
                .MinimumLevel.Verbose()
              .WriteTo.File(System.Web.Hosting.HostingEnvironment.MapPath("~/ErrorLog/Verbose/log.txt"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 5242880,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            Fatallog = new LoggerConfiguration()
                .MinimumLevel.Fatal()
              .WriteTo.File(System.Web.Hosting.HostingEnvironment.MapPath("~/ErrorLog/Fatal/log.txt"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 5242880,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

            Information = new LoggerConfiguration()
                .MinimumLevel.Information()
              .WriteTo.File(System.Web.Hosting.HostingEnvironment.MapPath("~/ErrorLog/Information/log.txt"),
                    rollingInterval: RollingInterval.Day,
                    fileSizeLimitBytes: 5242880,
                    rollOnFileSizeLimit: true)
                .CreateLogger();

        }

        public static void WriteError(Exception ex, string message)
        {
            Errorlog.Write(LogEventLevel.Error, ex, message);
        }

        public static void WriteWarning(Exception ex, string message)
        {
            Warninglog.Write(LogEventLevel.Warning, ex, message);
        }

        public static void WriteDebug(Exception ex, string message)
        {
            Debuglog.Write(LogEventLevel.Debug, ex, message);
        }

        public static void WriteVerbose(Exception ex, string message)
        {
            Verboselog.Write(LogEventLevel.Verbose, ex, message);
        }

        public static void WriteFatal(Exception ex, string message)
        {
            Fatallog.Write(LogEventLevel.Fatal, ex, message);
        }

        public static void WriteInformation(string message)
        {
            Exception ex = null;
            Information.Write(LogEventLevel.Information, ex, message); ;
        }

    }
}
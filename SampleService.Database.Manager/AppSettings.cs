using System;
using System.Collections.Generic;
using System.Text;

namespace SampleService.Database.Manager
{
    public class AppSettings
    {
        public ConnectionStringSection ConnectionStrings { get; set; }

        public LoggingSection Logging { get; set; }
    }

    public class ConnectionStringSection
    {
        public string DefaultConnection { get; set; }
    }

    public class LoggingSection
    {
        /// <summary>
        /// All providers, LogLevel applies to all the enabled providers.
        /// </summary>
        public LogLevelSection LogLevel { get; set; }
    }

    /// <summary>
    /// All providers, LogLevel applies to all the enabled providers.
    /// </summary>
    public class LogLevelSection
    {
        /// <summary>
        /// Default logging, Error and higher.
        /// </summary>
        public string Default { get; set; }

        /// <summary>
        /// All Microsoft* categories, Warning and higher.
        /// </summary>
        public string Microsoft { get; set; }
    }
}

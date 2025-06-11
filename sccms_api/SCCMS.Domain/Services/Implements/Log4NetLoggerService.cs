using log4net;
using SCCMS.Domain.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Implements
{
    public class Log4NetLoggerService<T> : ILoggerService<T>
    {
        private readonly ILog _logger;

        public Log4NetLoggerService()
        {
            _logger = LogManager.GetLogger(typeof(T));
        }

        public void LogInfo(string message)
        {
            if (_logger.IsInfoEnabled)
            {
                _logger.Info(message);
            }
        }

        public void LogWarning(string message)
        {
            if (_logger.IsWarnEnabled)
            {
                _logger.Warn(message);
            }
        }

        public void LogError(string message)
        {
            if (_logger.IsErrorEnabled)
            {
                _logger.Error(message);
            }
        }
    }
}

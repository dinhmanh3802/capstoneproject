using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCCMS.Domain.Services.Interfaces
{
    public interface ILoggerService<T>
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace VideotonTestApp
{

    public class SimpleLogging
    {
        private readonly ILogger _logger;

        public SimpleLogging(ILogger logger)
        {
            _logger = logger;
        }

        public void itLogsError(string msg)
        {
            _logger.LogError("ERROR OCURED: {message}", msg);
        }

        public void itLogsInfo(string msg)
        {
            _logger.LogInformation("INFO: {message}", msg);
        }

    }
}

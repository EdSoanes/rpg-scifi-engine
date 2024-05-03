using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.AzFunc.Common
{
    public class AppSettings
    {
        public static readonly string StorageConnectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage")!;
    }
}

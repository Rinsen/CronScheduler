using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rinsen.CronScheduler
{
    public interface ICronJob
    {
        Task RunJob(CancellationToken cancellationToken);
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StartupImpact
{
    public class ProfilerTickCount : ProfilerSingleThread
    {
        int startTime;
        
        public override void start()
        {
            startTime = Environment.TickCount;
        }

        public override int stop()
        {
            return Environment.TickCount - startTime;
        }
    }

}

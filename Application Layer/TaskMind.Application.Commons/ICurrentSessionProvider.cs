using System;
using System.Collections.Generic;
using System.Text;

namespace TaskMind.Applications.Commons
{
    public interface ICurrentSessionProvider
    {
        Guid? GetUserId();
    }
}

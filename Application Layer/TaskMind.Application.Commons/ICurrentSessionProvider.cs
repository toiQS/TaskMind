using System;
using System.Collections.Generic;
using System.Text;

namespace TaskMind.Application.Commons
{
    public interface ICurrentSessionProvider
    {
        Guid? GetUserId();
    }
}

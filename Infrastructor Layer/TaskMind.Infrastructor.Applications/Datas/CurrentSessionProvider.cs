using System;
using System.Collections.Generic;
using System.Text;
using TaskMind.Applications.Commons;

namespace TaskMind.Infrastructor.Applications.Datas
{
    public class CurrentSessionProvider : ICurrentSessionProvider
    {
        public Guid? GetUserId() => null; // TODO: lấy từ session/JWT hiện tại
    }
}

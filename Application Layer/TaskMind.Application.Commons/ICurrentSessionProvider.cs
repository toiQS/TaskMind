namespace TaskMind.Applications.Commons
{
    public interface ICurrentSessionProvider
    {
        Guid? GetUserId();
    }
}

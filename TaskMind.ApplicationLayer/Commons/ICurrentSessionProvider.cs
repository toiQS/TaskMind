namespace TaskMind.ApplicationLayer.Commons
{
    public interface ICurrentSessionProvider
    {
        Guid? GetUserId();
    }
}

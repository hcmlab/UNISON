public interface IStateSubscriber<in T>
{
    void OnUpdated(T newT, T oldT);
}
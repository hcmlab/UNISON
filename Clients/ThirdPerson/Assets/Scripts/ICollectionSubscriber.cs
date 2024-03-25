public interface ICollectionSubscriber<in T>
{
    void OnAdded(T t);
    void OnRemoved(T t);
}
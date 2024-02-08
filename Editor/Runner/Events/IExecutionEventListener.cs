namespace UnityFlow.Events
{
    public interface IExecutionEventListener
    {
        void OnEvent(IExecutionEvent executionEvent);
    }
}

namespace UnitySpec.Events
{
    public interface IExecutionEventListener
    {
        void OnEvent(IExecutionEvent executionEvent);
    }
}

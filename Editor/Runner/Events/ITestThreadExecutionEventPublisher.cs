using System;

namespace UnitySpec.Events
{
    public interface ITestThreadExecutionEventPublisher
    {
        void PublishEvent(IExecutionEvent executionEvent);

        void AddListener(IExecutionEventListener listener);

        void AddHandler<TEvent>(Action<TEvent> handler) where TEvent : IExecutionEvent;
    }
}

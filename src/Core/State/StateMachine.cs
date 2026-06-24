using System;
using TapeAdhesionApp.Core.Models;

namespace TapeAdhesionApp.Core.State;

public enum HookState
{
    Idle,
    Running,
    Completed
}

public class TestCompletedEventArgs : EventArgs
{
    public string HookId { get; set; } = string.Empty;
    public uint DropTime { get; set; }
    public uint LastValue { get; set; }
    public DateTime CompletedAt { get; set; }
}

public class StateMachine
{
    public HookState CurrentState { get; private set; } = HookState.Idle;
    public string HookId { get; }
    
    public event EventHandler<TestCompletedEventArgs>? OnTestCompleted;

    private uint _lastValue = 0;
    private int _unchangedCycles = 0;
    private const int DebounceStopThreshold = 3;

    public StateMachine(string hookId)
    {
        HookId = hookId;
    }

    public void ProcessValue(uint currentValue)
    {
        switch (CurrentState)
        {
            case HookState.Idle:
                if (currentValue > 0)
                {
                    CurrentState = HookState.Running;
                    _unchangedCycles = 0;
                }
                break;

            case HookState.Running:
                if (currentValue == 0)
                {
                    CurrentState = HookState.Idle;
                    _unchangedCycles = 0;
                }
                else if (currentValue > _lastValue)
                {
                    _unchangedCycles = 0;
                }
                else if (currentValue == _lastValue)
                {
                    _unchangedCycles++;
                    if (_unchangedCycles >= DebounceStopThreshold)
                    {
                        CurrentState = HookState.Completed;
                        OnTestCompleted?.Invoke(this, new TestCompletedEventArgs
                        {
                            HookId = this.HookId,
                            DropTime = currentValue,
                            LastValue = currentValue,
                            CompletedAt = DateTime.Now
                        });
                    }
                }
                break;

            case HookState.Completed:
                if (currentValue == 0)
                {
                    CurrentState = HookState.Idle;
                }
                break;
        }

        _lastValue = currentValue;
    }

    public void Reset()
    {
        CurrentState = HookState.Idle;
        _unchangedCycles = 0;
        _lastValue = 0;
    }
}

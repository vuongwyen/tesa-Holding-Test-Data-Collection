using System.ComponentModel;

namespace TapeAdhesionApp.Core.Models;

public class MeasurementRow : INotifyPropertyChanged
{
    private string _nart = string.Empty;
    private string _batch = string.Empty;
    private string _tester = string.Empty;
    private string _location = string.Empty;
    private uint _plcValue;
    private string _state = "IDLE";
    private string _samplePosition = string.Empty;
    private string _testCondition = string.Empty;
    private string _sampleNote = string.Empty;
    private DateTime? _startedAt;
    private DateTime? _completedAt;

    public string RackId { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int HookIndex { get; set; }
    public string HookId => $"{RackId}-T{Floor}-M{HookIndex}";

    public string Nart
    {
        get => _nart;
        set { if (_nart != value) { _nart = value; OnPropertyChanged(nameof(Nart)); } }
    }

    public string Batch
    {
        get => _batch;
        set { if (_batch != value) { _batch = value; OnPropertyChanged(nameof(Batch)); } }
    }

    public string Tester
    {
        get => _tester;
        set { if (_tester != value) { _tester = value; OnPropertyChanged(nameof(Tester)); } }
    }

    public string Location
    {
        get => _location;
        set { if (_location != value) { _location = value; OnPropertyChanged(nameof(Location)); } }
    }

    public uint PlcValue
    {
        get => _plcValue;
        set { if (_plcValue != value) { _plcValue = value; OnPropertyChanged(nameof(PlcValue)); } }
    }

    public string State
    {
        get => _state;
        set { if (_state != value) { _state = value; OnPropertyChanged(nameof(State)); } }
    }

    public string SamplePosition
    {
        get => _samplePosition;
        set { if (_samplePosition != value) { _samplePosition = value; OnPropertyChanged(nameof(SamplePosition)); } }
    }

    public string TestCondition
    {
        get => _testCondition;
        set { if (_testCondition != value) { _testCondition = value; OnPropertyChanged(nameof(TestCondition)); } }
    }

    public string SampleNote
    {
        get => _sampleNote;
        set { if (_sampleNote != value) { _sampleNote = value; OnPropertyChanged(nameof(SampleNote)); } }
    }

    public DateTime? StartedAt
    {
        get => _startedAt;
        set { if (_startedAt != value) { _startedAt = value; OnPropertyChanged(nameof(StartedAt)); OnPropertyChanged(nameof(StartedAtText)); } }
    }

    public string StartedAtText => _startedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";

    public DateTime? CompletedAt
    {
        get => _completedAt;
        set { if (_completedAt != value) { _completedAt = value; OnPropertyChanged(nameof(CompletedAt)); OnPropertyChanged(nameof(CompletedAtText)); } }
    }

    public string CompletedAtText => _completedAt?.ToString("dd/MM/yyyy HH:mm:ss") ?? "";

    public void UpdateTimestampsFromState(string newState, uint valueTicks)
    {
        if (_state != "RUNNING" && newState == "RUNNING")
        {
            StartedAt = DateTime.Now.AddMilliseconds(-valueTicks * 100);
            CompletedAt = null;
        }
        else if (_state != "COMPLETED" && newState == "COMPLETED")
        {
            CompletedAt = DateTime.Now;
            if (_startedAt == null)
            {
                StartedAt = CompletedAt.Value.AddMilliseconds(-valueTicks * 100);
            }
        }
        else if (newState == "IDLE" && _state != "IDLE")
        {
            StartedAt = null;
            CompletedAt = null;
        }
    }

    public void SetQuietly(uint plcValue, string state)
    {
        _plcValue = plcValue;
        _state = state;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

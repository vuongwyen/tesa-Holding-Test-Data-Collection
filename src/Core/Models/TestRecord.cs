using System;

namespace TapeAdhesionApp.Core.Models;

public class TestRecord
{
    public int Id { get; set; }
    public string RackId { get; set; } = string.Empty;
    public int Floor { get; set; }
    public int HookIndex { get; set; }
    public string HookId { get; set; } = string.Empty;
    public string BatchCode { get; set; } = string.Empty;
    public string NartCode { get; set; } = string.Empty;
    public string Tester { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string SamplePosition { get; set; } = string.Empty;
    public uint DropTime { get; set; }
    public uint PlcValue { get; set; }
    public DateTime CompletedAt { get; set; }
}

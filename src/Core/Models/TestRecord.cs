using System;

namespace TapeAdhesionApp.Core.Models;

public class TestRecord
{
    public string BatchCode { get; set; } = string.Empty;
    public string NartCode { get; set; } = string.Empty;
    public uint DropTime { get; set; }
    public DateTime CompletedAt { get; set; }
}

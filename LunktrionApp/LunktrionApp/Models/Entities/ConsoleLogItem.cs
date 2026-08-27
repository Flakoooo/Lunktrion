using LunktrionApp.Models.Enums;
using System;

namespace LunktrionApp.Models.Entities
{
    public record class ConsoleLogItem(
        string Text, 
        ConsoleMessageType Type,
        DateTime Timestamp
    )
    {
        public ConsoleLogItem(string Text, ConsoleMessageType Type)
            : this(Text, Type, DateTime.Now) { }
    }
}

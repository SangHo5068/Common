using System;

namespace Common.Types
{
    public enum ColumnType
    {
        Default,
        Combo,
        Check,
        Int32,
        Decimal,
        Time
    }

    [Flags]
    public enum DataType
    {
        Normal = 0x0,
        Other = 0x100,
        StDevZero = 0x201
    }
}

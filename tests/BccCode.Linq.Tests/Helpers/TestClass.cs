﻿namespace BccCode.Linq.Tests.Helpers;

public class TestClass
{
    public string StrProp { get; set; }
    public string AnotherStrProp { get; set; }
    public int NumberIntergerProp { get; set; }
    public int? IntNullable { get; set; }
    public double NumberDoubleProp { get; set; }
    public long NumberLongProp { get; set; }
    public bool BooleanProp { get; set; }
    public string[] StringArrayProp { get; set; }
    public decimal Amount { get; set; }
    public decimal? AmountNullable { get; set; }
    public DateTime AnyDate { get; set; }
    public DateTime? DateNullable { get; set; }
    public NestedClass Nested { get; set; }
}

public class NestedClass
{

    public string NestedStrProp { get; set; }
}

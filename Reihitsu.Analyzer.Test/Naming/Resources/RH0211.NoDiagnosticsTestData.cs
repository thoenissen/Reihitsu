using System;

namespace Reihitsu.Analyzer.Test.Naming.Resources;

public record class TestRecord(int ParameterName);

public record struct TestRecordStruct(int ParameterName);

public class TestClass(int parameterName);

public struct TestStruct(int parameterName);
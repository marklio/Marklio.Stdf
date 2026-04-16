using System.Buffers;
using System.Diagnostics.CodeAnalysis;
using Marklio.Stdf;
using Marklio.Stdf.Records;

namespace Marklio.Stdf.Tests;

[SuppressMessage("", "STDF0001", Justification = "Testing experimental error infrastructure")]
public class ErrorRecordTests
{
    private static ErrorRecord MakeError(
        ErrorSeverity severity = ErrorSeverity.Error,
        string code = "TEST_CODE",
        string message = "Something went wrong") =>
        new() { Severity = severity, Code = code, Message = message };

    #region ErrorRecord basics

    [Fact]
    public void ErrorRecord_CanCreate_WithRequiredProperties()
    {
        var error = MakeError();

        Assert.Equal(ErrorSeverity.Error, error.Severity);
        Assert.Equal("TEST_CODE", error.Code);
        Assert.Equal("Something went wrong", error.Message);
    }

    [Fact]
    public void ErrorRecord_RecordType_IsZero()
    {
        var error = MakeError();
        Assert.Equal((byte)0, error.RecordType);
    }

    [Fact]
    public void ErrorRecord_RecordSubType_IsZero()
    {
        var error = MakeError();
        Assert.Equal((byte)0, error.RecordSubType);
    }

    [Fact]
    public void ErrorRecord_Serialize_ThrowsNotSupportedException()
    {
        var error = MakeError();
        var writer = new ArrayBufferWriter<byte>();

        Assert.Throws<NotSupportedException>(() =>
            error.Serialize(writer, Endianness.LittleEndian));
    }

    [Fact]
    public void ErrorRecord_SourceRecord_CanBeSetAndRetrieved()
    {
        var source = new Far { CpuType = 2, StdfVersion = 4 };
        var error = MakeError() with { SourceRecord = source };

        Assert.NotNull(error.SourceRecord);
        Assert.IsType<Far>(error.SourceRecord);
    }

    [Fact]
    public void ErrorRecord_SourceRecord_DefaultsToNull()
    {
        var error = MakeError();
        Assert.Null(error.SourceRecord);
    }

    [Fact]
    public void ErrorRecord_InheritsFromStdfRecord()
    {
        var error = MakeError();
        Assert.IsAssignableFrom<StdfRecord>(error);
    }

    #endregion

    #region ErrorSeverity

    [Fact]
    public void ErrorSeverity_Warning_IsLessThanError()
    {
        Assert.True(ErrorSeverity.Warning < ErrorSeverity.Error);
    }

    #endregion

    #region StdfValidationException

    [Fact]
    public void StdfValidationException_CanCreateFromErrorRecord()
    {
        var error = MakeError(message: "bad data");
        var ex = new StdfValidationException(error);

        Assert.NotNull(ex);
    }

    [Fact]
    public void StdfValidationException_Message_MatchesErrorRecordMessage()
    {
        var error = MakeError(message: "bad data");
        var ex = new StdfValidationException(error);

        Assert.Equal("bad data", ex.Message);
    }

    [Fact]
    public void StdfValidationException_ErrorRecord_IsAccessible()
    {
        var error = MakeError(message: "bad data");
        var ex = new StdfValidationException(error);

        Assert.Same(error, ex.ErrorRecord);
    }

    [Fact]
    public void StdfValidationException_CustomMessage_OverridesDefault()
    {
        var error = MakeError(message: "original");
        var ex = new StdfValidationException("custom message", error);

        Assert.Equal("custom message", ex.Message);
        Assert.Same(error, ex.ErrorRecord);
    }

    [Fact]
    public void StdfValidationException_IsException()
    {
        var error = MakeError();
        var ex = new StdfValidationException(error);

        Assert.IsAssignableFrom<Exception>(ex);
    }

    #endregion

    #region ThrowOnError — sync

    [Fact]
    public void ThrowOnError_Sync_NoErrors_PassesAllRecords()
    {
        StdfRecord[] records = [new Far { CpuType = 2, StdfVersion = 4 }, new Mrr()];

        var result = records.AsEnumerable().ThrowOnError().ToList();

        Assert.Equal(2, result.Count);
        Assert.IsType<Far>(result[0]);
        Assert.IsType<Mrr>(result[1]);
    }

    [Fact]
    public void ThrowOnError_Sync_ErrorSeverity_Throws()
    {
        StdfRecord[] records =
        [
            new Far { CpuType = 2, StdfVersion = 4 },
            MakeError(ErrorSeverity.Error),
        ];

        Assert.Throws<StdfValidationException>(() =>
            records.AsEnumerable().ThrowOnError().ToList());
    }

    [Fact]
    public void ThrowOnError_Sync_WarningSeverity_ThrowsByDefault()
    {
        StdfRecord[] records =
        [
            new Far { CpuType = 2, StdfVersion = 4 },
            MakeError(ErrorSeverity.Warning),
        ];

        Assert.Throws<StdfValidationException>(() =>
            records.AsEnumerable().ThrowOnError().ToList());
    }

    [Fact]
    public void ThrowOnError_Sync_WarningSeverity_PassesThroughWhenMinimumIsError()
    {
        var warning = MakeError(ErrorSeverity.Warning);
        StdfRecord[] records = [new Far { CpuType = 2, StdfVersion = 4 }, warning];

        var result = records.AsEnumerable()
            .ThrowOnError(minimumSeverity: ErrorSeverity.Error)
            .ToList();

        Assert.Equal(2, result.Count);
        Assert.Same(warning, result[1]);
    }

    [Fact]
    public void ThrowOnError_Sync_ExceptionContainsErrorRecord()
    {
        var error = MakeError(ErrorSeverity.Error);
        StdfRecord[] records = [error];

        var ex = Assert.Throws<StdfValidationException>(() =>
            records.AsEnumerable().ThrowOnError().ToList());

        Assert.Same(error, ex.ErrorRecord);
    }

    [Fact]
    public void ThrowOnError_Sync_YieldsRecordsBeforeError()
    {
        var far = new Far { CpuType = 2, StdfVersion = 4 };
        var mrr = new Mrr();
        StdfRecord[] records = [far, mrr, MakeError(ErrorSeverity.Error)];

        var yielded = new List<StdfRecord>();
        Assert.Throws<StdfValidationException>(() =>
        {
            foreach (var r in records.AsEnumerable().ThrowOnError())
                yielded.Add(r);
        });

        Assert.Equal(2, yielded.Count);
        Assert.Same(far, yielded[0]);
        Assert.Same(mrr, yielded[1]);
    }

    #endregion

    #region ThrowOnError — async

    [Fact]
    public async Task ThrowOnError_Async_NoErrors_PassesAllRecords()
    {
        StdfRecord[] records = [new Far { CpuType = 2, StdfVersion = 4 }, new Mrr()];

        var result = new List<StdfRecord>();
        await foreach (var r in ToAsync(records).ThrowOnError())
            result.Add(r);

        Assert.Equal(2, result.Count);
        Assert.IsType<Far>(result[0]);
        Assert.IsType<Mrr>(result[1]);
    }

    [Fact]
    public async Task ThrowOnError_Async_ErrorSeverity_Throws()
    {
        StdfRecord[] records =
        [
            new Far { CpuType = 2, StdfVersion = 4 },
            MakeError(ErrorSeverity.Error),
        ];

        await Assert.ThrowsAsync<StdfValidationException>(async () =>
        {
            await foreach (var _ in ToAsync(records).ThrowOnError()) { }
        });
    }

    [Fact]
    public async Task ThrowOnError_Async_WarningSeverity_ThrowsByDefault()
    {
        StdfRecord[] records =
        [
            new Far { CpuType = 2, StdfVersion = 4 },
            MakeError(ErrorSeverity.Warning),
        ];

        await Assert.ThrowsAsync<StdfValidationException>(async () =>
        {
            await foreach (var _ in ToAsync(records).ThrowOnError()) { }
        });
    }

    [Fact]
    public async Task ThrowOnError_Async_WarningSeverity_PassesThroughWhenMinimumIsError()
    {
        var warning = MakeError(ErrorSeverity.Warning);
        StdfRecord[] records = [new Far { CpuType = 2, StdfVersion = 4 }, warning];

        var result = new List<StdfRecord>();
        await foreach (var r in ToAsync(records).ThrowOnError(minimumSeverity: ErrorSeverity.Error))
            result.Add(r);

        Assert.Equal(2, result.Count);
        Assert.Same(warning, result[1]);
    }

    [Fact]
    public async Task ThrowOnError_Async_ExceptionContainsErrorRecord()
    {
        var error = MakeError(ErrorSeverity.Error);
        StdfRecord[] records = [error];

        var ex = await Assert.ThrowsAsync<StdfValidationException>(async () =>
        {
            await foreach (var _ in ToAsync(records).ThrowOnError()) { }
        });

        Assert.Same(error, ex.ErrorRecord);
    }

    [Fact]
    public async Task ThrowOnError_Async_YieldsRecordsBeforeError()
    {
        var far = new Far { CpuType = 2, StdfVersion = 4 };
        var mrr = new Mrr();
        StdfRecord[] records = [far, mrr, MakeError(ErrorSeverity.Error)];

        var yielded = new List<StdfRecord>();
        await Assert.ThrowsAsync<StdfValidationException>(async () =>
        {
            await foreach (var r in ToAsync(records).ThrowOnError())
                yielded.Add(r);
        });

        Assert.Equal(2, yielded.Count);
        Assert.Same(far, yielded[0]);
        Assert.Same(mrr, yielded[1]);
    }

    #endregion

    private static async IAsyncEnumerable<StdfRecord> ToAsync(params StdfRecord[] records)
    {
        foreach (var r in records) yield return r;
        await Task.CompletedTask;
    }
}

# Microsoft.Extensions.Compliance.Redaction with ZeroRedact
A sample project to see how to integrate Microsoft​.Extensions​.Compliance​.Redaction with Nikouu.ZeroRedact.

# Prerequisites

1. Knowledge of [Source generated logging](https://learn.microsoft.com/en-us/dotnet/core/extensions/logger-message-generator)
2. A general understanding of Microsoft.Extensions.Compliance.Redaction, see [Redacting sensitive data in logs with Microsoft​.Extensions​.Compliance​.Redaction by Andrew Lock](https://andrewlock.net/redacting-sensitive-data-with-microsoft-extensions-compliance/)
3. A general understanding of [ZeroRedact](https://github.com/nikouu/ZeroRedact)

# Example walkthrough

This code is a rough example of how ZeroRedact can work with Microsoft.Extensions.Compliance.Redaction. It assumes you have the knowledge above and will skip over those topics.

## Logging configuration

In order for redaction to work, the redaction must be enabled, and for ZeroRedact, LoggerRedactionOptions.ApplyDiscriminator must be off:
```csharp
builder.Logging.EnableRedaction();
builder.Services.Configure<LoggerRedactionOptions>(options =>
{
    // by default this is true, which passes the value and discriminator to the redactor
    // e.g. "Isla@example.com:customer.Email"
    // Need to set to false to avoid the extra discriminator when using ZeroRedact
    // https://github.com/dotnet/extensions/pull/4516
    // https://learn.microsoft.com/en-au/dotnet/api/microsoft.extensions.logging.loggerredactionoptions.applydiscriminator
    options.ApplyDiscriminator = false;
});
```

## Inject ZeroRedact.Redactor

For this example, the most of the settings were based in the constructor:

```csharp
builder.Services.AddSingleton<IRedactor>(redactor => new Redactor(new RedactorOptions
{
    CreditCardRedactorOptions = new CreditCardRedactorOptions { RedactorType = CreditCardRedaction.ShowLastFour },
    EmailAddressRedactorOptions = new EmailAddressRedactorOptions { RedactorType = EmailAddressRedaction.ShowFirstCharacters },
    DateRedactorOptions = new DateRedactorOptions { RedactorType = DateRedaction.Day },
    PhoneNumberRedactorOptions = new PhoneNumberRedactorOptions { RedactorType = PhoneNumberRedaction.ShowLastFour }
}));
```

This `Redactor` object is then injected into the redactors for Microsoft.Extensions.Compliance.Redaction. For example:

```csharp
public class CreditCardRedactor : Redactor
{
    private IRedactor _redactor { get; init; }

    public CreditCardRedactor(IRedactor redactor)
    {
        _redactor = redactor;
    }

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        var result = _redactor.RedactCreditCard(source);
        result.CopyTo(destination);
        return result.Length;
    }

    public override int GetRedactedLength(ReadOnlySpan<char> input)
    {
        return input.Length;
    }
}
```

# ⚠ Gotchas

## When the redaction length is not the same as the original string length

Due to how [Microsoft.Extensions.Compliance.Redaction.Redactor](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.compliance.redaction.redactor) works, it needs to get the length of the final redaction first, then it requests the actual redaction. The problem here is the following redactions can have a different length from the original source input:
- Date redaction: due to culture handling where "2022-05-01" may end up as "2022-05-1" with the padding removed
- Using a fixed length redaction. This includes when an exception occurs when redacting and a fixed length redaction is returned

These will result in extra `\u0000` characters in the string at the end.

## Calculating the final redaction length for when the redaction length is not the same as the original string length

Due to the above, a solution may need to be worked out in some cases where the redacted output could be a different length to the input. The safest, but slowest way is to redact twice, one for the length and one for the value:
```csharp
public class SensitiveDateRedactor : Redactor
{
    private IRedactor _redactor { get; init; }

    public SensitiveDateRedactor(IRedactor redactor)
    {
        _redactor = redactor;
    }

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
           
        // The problem here is a date like "2022-05-01" with invariant culture is "2022-05-1" meaning the redacted
        // date is one character less due to no zero padding for the day. This results in the output being "*/05/2022\u0000"
        // however that may not be an issue for some loggers, but for the sake of ensuring this works fine, this will do 
        // double duty and redact the date twice.
        var result = RedactDate(source);
        result.CopyTo(destination);
        return destination.Length;
    }

    public override int GetRedactedLength(ReadOnlySpan<char> input)
    {
        var result = RedactDate(input);
        return result.Length;
    }

    private string RedactDate(ReadOnlySpan<char> input)
    {
        // Would need better type checking
        // JsonConsoleFormatter in Microsoft.Extensions.Logging.Console makes strings Invariant Culture
        // So I think that's why the date string that ends up here isn't my current culture format
        var dateOnly = DateOnly.Parse(input, System.Globalization.CultureInfo.InvariantCulture);
        return _redactor.RedactDate(dateOnly);
    }
}
```

I'm sure there are safe ways to use things like ThreadLocals or some other storage, however those are at your own risk.

# Links
- [Redacting sensitive data in logs with Microsoft​.Extensions​.Compliance​.Redaction by Andrew Lock](https://andrewlock.net/redacting-sensitive-data-with-microsoft-extensions-compliance/)
- [The New Data Protection Features of .NET 8 (GDPR) by Nick Chapsas](https://www.youtube.com/watch?v=rK3-tO7K6i8)
- [Enhancing Data Privacy with Microsoft.Extensions.Compliance.Redaction by Sharmila Subbiah](https://medium.com/@malarsharmila/enhancing-data-privacy-with-microsoft-extensions-compliance-redaction-c5190776a223)
- [How to use Microsoft.Extensions.Compliance.Redaction package by Jose Perez Rodriguez](https://gist.github.com/joperezr/f5f022bcb4d0ce8f077e40e1f77239c8)
- [LogRedactionDemo by bitbonk](https://github.com/bitbonk/LogRedactionDemo)
- [How does Microsoft.Extensions.Compliance.Redaction work #4735](https://github.com/dotnet/extensions/discussions/4735)
- [Microsoft.Extensions.Compliance.Redaction repo](https://github.com/dotnet/extensions/tree/main/src/Libraries/Microsoft.Extensions.Compliance.Redaction)
- [LoggerRedactionOptions.ApplyDiscriminator Property](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loggerredactionoptions.applydiscriminator)
- [LoggerRedactionOptions.ApplyDiscriminator Property commit](https://github.com/dotnet/extensions/pull/4516)

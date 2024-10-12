using ZeroRedact;
using Redactor = Microsoft.Extensions.Compliance.Redaction.Redactor;

namespace Compliance.RedactionWithZeroRedact.Redactors
{
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
}

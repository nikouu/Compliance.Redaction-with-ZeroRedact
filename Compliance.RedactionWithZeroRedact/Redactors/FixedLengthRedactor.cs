using ZeroRedact;
using Redactor = Microsoft.Extensions.Compliance.Redaction.Redactor;

namespace Compliance.RedactionWithZeroRedact.Redactors
{
    public class FixedLengthRedactor : Redactor
    {
        private IRedactor _redactor { get; init; }
        private static StringRedactorOptions _options = new StringRedactorOptions
        {
            RedactorType = StringRedaction.FixedLength,
            FixedLengthSize = 3
        };


        public FixedLengthRedactor(IRedactor redactor)
        {
            _redactor = redactor;
        }

        // If the source data length does not fit the redacted data length, then some manual working is needed.
        // Either from knowing a fixed size, or perhaps doing double duty and working out the redaction twice.
        public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
        {
            var result = _redactor.RedactString(source, _options);
            result.CopyTo(destination);
            return result.Length;
        }
        
        // There's probably a better way
        public override int GetRedactedLength(ReadOnlySpan<char> input)
        {
            return _options.FixedLengthSize;
        }
    }
}

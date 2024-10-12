using ZeroRedact;
using Redactor = Microsoft.Extensions.Compliance.Redaction.Redactor;

namespace Compliance.RedactionWithZeroRedact.Redactors
{
    public class SensitiveEmailAddressRedactor : Redactor
    {
        private IRedactor _redactor { get; init; }

        public SensitiveEmailAddressRedactor(IRedactor redactor)
        {
            _redactor = redactor;
        }

        public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
        {
            var result = _redactor.RedactEmailAddress(source);
            result.CopyTo(destination);
            return result.Length;
        }

        public override int GetRedactedLength(ReadOnlySpan<char> input)
        {
            return input.Length;
        }
    }
}

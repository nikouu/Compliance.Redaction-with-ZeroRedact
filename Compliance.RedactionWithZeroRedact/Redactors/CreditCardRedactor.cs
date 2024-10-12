using ZeroRedact;
using Redactor = Microsoft.Extensions.Compliance.Redaction.Redactor;

namespace Compliance.RedactionWithZeroRedact.Redactors
{
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
}

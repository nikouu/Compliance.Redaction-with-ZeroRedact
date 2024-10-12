using Microsoft.Extensions.Compliance.Classification;

namespace Compliance.RedactionWithZeroRedact
{
    public static class DataTaxonomy
    {
        public static string TaxonomyName { get; } = typeof(DataTaxonomy).FullName!;
        public static DataClassification SensitiveEmail { get; } = new(TaxonomyName, nameof(SensitiveEmail));
        public static DataClassification CreditCard { get; } = new(TaxonomyName, nameof(CreditCard));
        public static DataClassification SensitiveDate { get; } = new(TaxonomyName, nameof(SensitiveDate));
        public static DataClassification SensitivePhoneNumber { get; } = new(TaxonomyName, nameof(SensitivePhoneNumber));
        public static DataClassification DefaultRedactor { get; } = new(TaxonomyName, nameof(DefaultRedactor));

    }

    public class SensitiveEmailAttribute : DataClassificationAttribute
    {
        public SensitiveEmailAttribute() : base(DataTaxonomy.SensitiveEmail)
        {
        }
    }

    public class CreditCardAttribute : DataClassificationAttribute
    {
        public CreditCardAttribute() : base(DataTaxonomy.CreditCard)
        {
        }
    }

    public class SensitiveDateAttribute : DataClassificationAttribute
    {
        public SensitiveDateAttribute() : base(DataTaxonomy.SensitiveDate)
        {
        }
    }

    public class SensitivePhoneNumberAttribute : DataClassificationAttribute
    {
        public SensitivePhoneNumberAttribute() : base(DataTaxonomy.SensitivePhoneNumber)
        {
        }
    }

    public class DefaultRedactorAttribute : DataClassificationAttribute
    {
        public DefaultRedactorAttribute() : base(DataTaxonomy.DefaultRedactor)
        {
        }
    }
}

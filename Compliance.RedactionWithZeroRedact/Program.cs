using Compliance.RedactionWithZeroRedact;
using Compliance.RedactionWithZeroRedact.Redactors;
using Microsoft.Extensions.Compliance.Classification;
using System.Text.Json;
using ZeroRedact;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddRedaction(x =>
{
    x.SetRedactor<CreditCardRedactor>(new DataClassificationSet(DataTaxonomy.CreditCard));
    x.SetRedactor<SensitiveEmailAddressRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveEmail));
    x.SetRedactor<SensitiveDateRedactor>(new DataClassificationSet(DataTaxonomy.SensitiveDate));
    x.SetRedactor<SensitivePhoneNumberRedactor>(new DataClassificationSet(DataTaxonomy.SensitivePhoneNumber));
    x.SetRedactor<FixedLengthRedactor>(new DataClassificationSet(DataTaxonomy.DefaultRedactor));
});

builder.Logging.ClearProviders();
builder.Logging.AddJsonConsole(option => option.JsonWriterOptions = new JsonWriterOptions
{
    Indented = true,
    // Unsafe to use normally, but for demonstration purposes here, it will help us see the values better
    // as for instance "+" is correctly converted to "\u002B" normally
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
});

builder.Services.AddZeroRedact(new RedactorOptions
{
    CreditCardRedactorOptions = new CreditCardRedactorOptions { RedactorType = CreditCardRedaction.ShowLastFour },
    EmailAddressRedactorOptions = new EmailAddressRedactorOptions { RedactorType = EmailAddressRedaction.ShowFirstCharacters },
    DateRedactorOptions = new DateRedactorOptions { RedactorType = DateRedaction.Day },
    PhoneNumberRedactorOptions = new PhoneNumberRedactorOptions { RedactorType = PhoneNumberRedaction.ShowLastFour }
});

// or manually
//builder.Services.AddSingleton<IRedactor>(redactor => new Redactor(new RedactorOptions
//{
//    CreditCardRedactorOptions = new CreditCardRedactorOptions { RedactorType = CreditCardRedaction.ShowLastFour },
//    EmailAddressRedactorOptions = new EmailAddressRedactorOptions { RedactorType = EmailAddressRedaction.ShowFirstCharacters },
//    DateRedactorOptions = new DateRedactorOptions { RedactorType = DateRedaction.Day },
//    PhoneNumberRedactorOptions = new PhoneNumberRedactorOptions { RedactorType = PhoneNumberRedaction.ShowLastFour }
//}));

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseHttpsRedirection();

var names = new[] { "Isla", "Amelia", "Charlotte" };

var baseDate = new DateOnly(2020, 1, 1);

app.MapGet("/customer", (ILogger<Program> logger) =>
{
    var name = Random.Shared.GetItems(names, 1)[0];

    var customer = new Customer(
        name,
        $"{name}@example.com",
        baseDate.AddDays(Random.Shared.Next(0, 1000)),
        "4111-1111-1111-1111",
        "+999 137 1234 5678");

    logger.LogRequestedCustomer(customer);

    return customer;
});

app.Run();

public static partial class Logging
{
    // Rebuild solution if CS8795
    [LoggerMessage(LogLevel.Information, "Returning customer")]
    public static partial void LogRequestedCustomer(this ILogger logger, [LogProperties] Customer customer);
}

// Demonstration purposes only
public record Customer
{
    public string Name { get; init; }

    [SensitiveEmail]
    public string Email { get; init; }

    [CreditCard]
    public string CreditCard { get; init; }

    [SensitiveDate]
    public DateOnly JoiningDate { get; init; }

    [SensitivePhoneNumber]
    public string PhoneNumber { get; init; }

    [DefaultRedactor]
    public Guid Id { get; } = Guid.NewGuid();

    public Customer(string name, string email, DateOnly joiningDate, string creditCard, string phoneNumber)
    {
        Name = name;
        Email = email;
        JoiningDate = joiningDate;
        CreditCard = creditCard;
        PhoneNumber = phoneNumber;
    }
}
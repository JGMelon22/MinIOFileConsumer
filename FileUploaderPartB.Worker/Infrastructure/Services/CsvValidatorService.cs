using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;
using FileUploaderPartB.Worker.Interfaces;
using FileUploaderPartB.Worker.Models;
using FileUploaderPartB.Worker.Shared;

namespace FileUploaderPartB.Worker.Infrastructure.Services;
public class CsvValidatorService : ICsvValidatorService
{
    private readonly ILogger<CsvValidatorService> _logger;

    public CsvValidatorService(ILogger<CsvValidatorService> logger)
    {
        _logger = logger;
    }

    public async Task<Result<List<string>>> ValidateCsvAsync(MemoryStream stream)
    {
        List<string> errors = new();

        try
        {
            using StreamReader streamReader = new(stream, Encoding.UTF8);
            CsvConfiguration csvConfig = new(CultureInfo.InvariantCulture)
            {
                Delimiter = ";",
                HasHeaderRecord = true,
                MissingFieldFound = null
            };

            using CsvReader csvReader = new(streamReader, csvConfig);
            if (!await csvReader.ReadAsync() || !csvReader.ReadHeader())
            {
                _logger.LogError("CSV file is empty or missing headers.");
                return Result<List<string>>.Failure("CSV file is empty or missing headers.");
            }

            while (await csvReader.ReadAsync())
            {
                List<string> rowErrors = new();
                ContactImport contact = MapContact(csvReader);

                rowErrors.AddRange(ValidateName(contact.FirstName, "Firstname"));
                rowErrors.AddRange(ValidateName(contact.LastName, "Lastname"));
                rowErrors.AddRange(ValidateEmail(contact.Email));
                rowErrors.AddRange(ValidateCpf(contact.Cpf));
                rowErrors.AddRange(ValidatePhone(contact.Phone));
                rowErrors.AddRange(ValidateGender(contact.Gender));
                rowErrors.AddRange(ValidateBirthday(contact.Birthday));

                if (rowErrors.Any())
                {
                    errors.Add($"Linha {csvReader.Context.Parser!.Row}: {string.Join("; ", rowErrors)}");
                    _logger.LogWarning("Validation failed at row {Row}: {Errors}", csvReader.Context.Parser!.Row, string.Join("; ", rowErrors));
                }
            }

            if (errors.Any())
            {
                _logger.LogError("Validation failed with errors: {Errors}", string.Join(" | ", errors));
                return Result<List<string>>.Failure(string.Join(" | ", errors));
            }

            return Result<List<string>>.Success(new List<string>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during CSV validation.");
            return Result<List<string>>.Failure($"Erro durante a validação: {ex.Message}");
        }
    }

    private ContactImport MapContact(CsvReader csv)
    {
        return new ContactImport
        {
            FirstName = csv.GetField<string>("FirstName")!,
            LastName = csv.GetField<string>("LastName")!,
            Email = csv.GetField<string>("Email")!,
            Cpf = csv.GetField<string>("Cpf")!,
            Phone = AdjustPhone(csv.GetField<string>("Phone")!),
            Gender = MapGender(csv.GetField<string>("Gender")!),
            Birthday = ParseDate(csv.GetField<string>("Birthday")!)
        };
    }

    private static string AdjustPhone(string phone)
    {
        string digitsOnly = new string(phone.Where(char.IsDigit).ToArray());
        return digitsOnly.StartsWith("55") ? digitsOnly : "55" + digitsOnly;
    }

    private static Gender? MapGender(string genderValue)
    {
        return genderValue.ToUpper() switch
        {
            "FEMININO" => Gender.Feminino,
            "MASCULINO" => Gender.Masculino,
            _ => (Gender?)-1
        };
    }

    private static DateTime? ParseDate(string dateValue)
    {
        if (string.IsNullOrWhiteSpace(dateValue)) return null;
        return DateTime.TryParseExact(dateValue, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
    }

    private static List<string> ValidateName(string value, string fieldName)
    {
        return !string.IsNullOrWhiteSpace(value) && !Regex.IsMatch(value, @"^[\p{L}]+$")
            ? [$"{fieldName} deve conter apenas letras."]
            : [];
    }

    private static List<string> ValidateEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) && !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$")
            ? ["Email deve estar em um formato válido."]
            : [];
    }

    private static List<string> ValidateCpf(string cpf)
    {
        return !string.IsNullOrWhiteSpace(cpf) && !Regex.IsMatch(cpf, @"^\d{11}$")
            ? ["CPF deve conter apenas números e ter 11 dígitos."]
            : [];
    }

    private static List<string> ValidatePhone(string phone)
    {
        string phoneWithoutPrefix = phone.StartsWith("55") && phone.Length >= 3 ? phone[2..] : phone;
        return !string.IsNullOrWhiteSpace(phoneWithoutPrefix) && !Regex.IsMatch(phoneWithoutPrefix, @"^\d{10,11}$")
            ? ["Telefone deve conter 10 ou 11 dígitos, incluindo o DDD (após o código do país 55)."]
            : [];
    }

    private static List<string> ValidateGender(Gender? gender)
    {
        return gender is Gender.Feminino or Gender.Masculino
            ? new List<string>()
            : new List<string> { "Gênero deve ser 'FEMININO' ou 'MASCULINO'." };
    }

    private static List<string> ValidateBirthday(DateTime? birthday)
    {
        return birthday.HasValue ? [] : ["Data de nascimento deve estar no formato válido de data."];
    }
}

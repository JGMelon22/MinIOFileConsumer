namespace FileUploaderPartB.Worker.Models;

public class ContactImport
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public Gender? Gender { get; set; }
    public DateTime? Birthday { get; set; }
}
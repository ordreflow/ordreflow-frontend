using System.ComponentModel.DataAnnotations;

namespace OrdreFlow.Frontend.Models;

public class LoginFormModel
{
    [Required(ErrorMessage = "Indtast din e-mail.")]
    [EmailAddress(ErrorMessage = "Indtast en gyldig e-mail.")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Indtast din adgangskode.")]
    public string? Password { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace OrdreFlow.Frontend.Models;

public sealed class UserFormModel
{
    [Required(ErrorMessage = "Indtast et navn.")]
    [StringLength(200, ErrorMessage = "Navnet må højst være 200 tegn.")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Indtast en e-mailadresse.")]
    [EmailAddress(ErrorMessage = "Indtast en gyldig e-mailadresse.")]
    [StringLength(320)]
    public string Email { get; set; } = "";

    public string Password { get; set; } = "";
    public string Role { get; set; } = "Employee";
}

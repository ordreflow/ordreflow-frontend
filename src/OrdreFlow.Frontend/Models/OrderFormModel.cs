using System.ComponentModel.DataAnnotations;

namespace OrdreFlow.Frontend.Models;

public sealed class OrderFormModel
{
    [Required(ErrorMessage = "Indtast et ordrenavn.")]
    [StringLength(200, ErrorMessage = "Ordrenavnet må højst være 200 tegn.")]
    public string Name { get; set; } = "";
}

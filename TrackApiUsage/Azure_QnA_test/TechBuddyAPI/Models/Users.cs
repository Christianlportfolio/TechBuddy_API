using System.ComponentModel.DataAnnotations;

namespace TechBuddyAPI.Models
{
    public class Users
    {
        [Key]
        public int UserID { get; set; }

        [Required(ErrorMessage = "Brugernavn påkrævet")]
        [StringLength(16, ErrorMessage = "Skal være mellem 3 og 16 tegn", MinimumLength = 3)]
        public string Username { get; set; }

        [Required(ErrorMessage = "Adgangskode pålrævet")]
        [StringLength(255, ErrorMessage = "Skal være mellem 5 og 255 tegn", MinimumLength = 5)]
        [RegularExpression(@"^((?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9])).*",
         ErrorMessage = "Adgangskode skal indeholde mindst én lille bogstav (a-z)], skal indeholde mindst én stor bogstav (A-Z), skal indeholde mindst ét tal, skal indeholde mindst ét tegn, som ikke er et tal eller et bogstav (specialtegn)")]
        public string Password { get; set; }

        public string Salt { get; set; }
    }
}

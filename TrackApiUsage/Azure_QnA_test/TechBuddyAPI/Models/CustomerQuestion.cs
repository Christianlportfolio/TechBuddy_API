using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace TechBuddyAPI.Models
{

    public class CustomerQuestion
    {
        [Key]
        public int CustomerQuestionID { get; set; }
        public string Subject { get; set; }
        public string Question { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string? CustomerNumber { get; set; }
        public string? InstallationNumber { get; set; }
        public DateTime? SubmitDate { get; set; }
        public bool IsChecked { get; set; }
    }
}

﻿
using System.ComponentModel.DataAnnotations;


namespace Shared.Models
{
    public class CreateStudentFundRequestForNewStudent
    {
        [Required]
        public string IDNumber { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [Phone]
        public string PhoneNumber { get; set; }

        [Required]
        public string GenderName { get; set; }

        [Required]
        public string RaceName { get; set; }

        [Required]
        public int UniversityID { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        public int Grade { get; set; }

        [Required]
        public decimal Amount { get; set; }
        public int UserID { get; set; }
    }
}

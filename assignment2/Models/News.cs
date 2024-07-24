using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class News
    {
        public int NewsId { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FileName { get; set; } = String.Empty;

        [Required]
        [DataType(DataType.Url)]
        public string Url { get; set; } = String.Empty;

        [Required]
        [ForeignKey("SportClub")]
        public string SportClubId { get; set; } = String.Empty;

        public SportClub SportClub { get; set; }
    }
}
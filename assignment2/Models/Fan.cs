using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Assignment2.Models
{
    public class Fan
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Display(Name = "Birth Date")]
        public DateTime BirthDate { get; set; }

        public string FullName
        {
            get { return LastName + ", " + FirstName; }
        }

        // Navigation property
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}

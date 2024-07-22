using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Assignment2.Models
{
    public class Subscription
    {
        [Key, Column(Order = 0)]
        public int FanId { get; set; }

        [Key, Column(Order = 1)]
        public string SportClubId { get; set; }

        public Fan Fan { get; set; }
        public SportClub SportClub { get; set; }
    }
}

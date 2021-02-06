using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RhythmHelper.Data.Entities
{
    public class Guild
    {
        [Required]
        [Key]
        public string GuildId { get; set; }
        [Required]
        public string Name { get; set; }
        public User Owner { get; set; }

        public ICollection<Role> Roles { get; set; }
        public ICollection<User> Users { get; set; }

        public int Limit { get; set; }
        public RestrictType Restrict { get; set; }

        public TimeSpan VideoLengthMin { get; set; }
        public TimeSpan VideoLengthMax { get; set; }
        public int DiceDefault { get; set; }
        [Required]
        [StringLength(3, MinimumLength = 1)]
        public string CommandPrefix { get; set; }



    }
}

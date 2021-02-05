using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RhythmHelper.Data.Entities
{
    public class User
    {
        [Required]
        [Key]
        public string UserId { get; set; }
        public string Username { get; set; }
        public string Discriminator { get; set; }
        public Guild Guild { get; set; }
        public ICollection<Role> Roles { get; set; }

        public bool OPointer { get; set; }
        public int OPoints { get; set; }
    }
}

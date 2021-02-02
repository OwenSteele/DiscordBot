using System.ComponentModel.DataAnnotations;

namespace RhythmHelper.Data.Entities
{
    public class Role
    {
        [Required]
        [Key]
        public string RoleId { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public int Color { get; set; }
        public bool Hoist { get; set; }
        public int Position { get; set; }
        public string Permissions { get; set; }
        public bool Managed { get; set; }
        public bool Mentionable { get; set; }
        public Guild Guild { get; set; }
    }
}
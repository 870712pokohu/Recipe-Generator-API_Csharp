using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Model
{
    public class RecipeLink
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(255)]
        public string Url { get; set; }
        public virtual Detail Detail { get; set; }
    }
}
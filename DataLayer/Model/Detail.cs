using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DataLayer.Model
{
    public class Detail
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(100)]
        public string Title { get; set; }
        [MaxLength(255)]
        public string Image { get; set; }
        [ForeignKey("FK_RecipeLink_Id")]
        public int RecipeId { get; set; }
        public virtual RecipeLink RecipeLink {get; set;}
        // representing a string array
        [Column(TypeName = "json")]
        public string[] TotalTime { get; set; }
        // representing a string array
        [Column(TypeName = "json")]
        public string[] Ingredient { get; set; }
        // representing a string array
        [Column(TypeName="json")]
        public string[] Instruction {get; set; }
        
        public IList<RecipeKeyword> RecipeKeyword { get; set; } 
    }
}
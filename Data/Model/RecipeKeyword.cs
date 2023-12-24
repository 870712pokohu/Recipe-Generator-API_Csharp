using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Model{
    public class RecipeKeyword
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("FK_Detail_Id")]
        public int DetailId { get; set; }
        [ForeignKey("FK_Keyword_Id")]
        public int KeywordId { get; set; }

        // navigation properties
        public virtual Detail Detail{ get; set; }
        // navigation properties
        public virtual Keyword Keyword { get; set; }  
        
    }
}
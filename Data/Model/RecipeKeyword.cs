using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Data.Model{
    public class RecipeKeyword
    {
        public int DetailId { get; set; }
        public int KeywordId { get; set; }

        // navigation properties
        public virtual Detail Detail{ get; set; }
        // navigation properties
        public virtual Keyword Keyword { get; set; }  
        
    }
}
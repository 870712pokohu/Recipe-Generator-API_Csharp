using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Model{
    public class Keyword
    {
        [Key]
        public int Id{ get; set; }
        [MaxLength(50)]
        public string KeywordName{ get; set; }
        public IList<RecipeKeyword> RecipeKeyword { get; set; }

    }
}
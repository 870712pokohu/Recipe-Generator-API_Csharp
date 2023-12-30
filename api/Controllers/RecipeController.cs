
using System.Text.Json;
using Data.Context;
using Data.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api.Controllers
{
    [Route("api")]
    [ApiController]
    public class RecipeController: ControllerBase
    {
        private readonly AppDbContext appDbContext;
        private int position; 
        private int itemPerPage;
        private int pageNumber;
        private int count;

        public RecipeController(){
            // initialize the appDbContext 
            appDbContext = new AppDbContext();
            itemPerPage = 12;
            position = 0;
            pageNumber = 1;
            count = 0;
        }
        
        private void PageCount(){
            if (pageNumber == 1)
            {
                position = 0;
            }
            else
            {
                if (pageNumber * itemPerPage > count)
                {
                    itemPerPage = pageNumber * itemPerPage - count;
                }
                else
                {
                    position = pageNumber * itemPerPage - 1;
                }
            }
        }

        [HttpPost]
        public IActionResult GetAll([FromBody] JsonElement jsonElement)
        {
            string searchInput = "";
            JsonElement temp;
                
            if(!jsonElement.TryGetProperty("searchInput", out temp) || !jsonElement.TryGetProperty("pageNumber", out temp)){
                return BadRequest("recipe not found");
            }
            else{
                searchInput = jsonElement.GetProperty("searchInput").GetString();
                pageNumber = int.Parse(jsonElement.GetProperty("pageNumber").GetString());
                if(string.IsNullOrEmpty(searchInput)){
                    // a list of recipe links and images
                    var RecipeLinkCollections = (from link in appDbContext.RecipeLink
                                                join detail in appDbContext.Detail
                                                on link.Id equals detail.RecipeId
                                                select new
                                                {
                                                    Id = detail.RecipeId,
                                                    Url = link.Url,
                                                    Image = detail.Image,
                                                    Title = detail.Title
                                                }).ToList();
                    count = RecipeLinkCollections.Count();
                    PageCount();
                    var collections = RecipeLinkCollections.Skip(position).Take(itemPerPage);
                    if (collections != null)
                    {
                        return Ok(new { count, collections});
                    }
                    else
                    {
                        return BadRequest("no recipe!");
                    }
                }
                else{
                    var recipeCategoryCollection = (from detail in appDbContext.Detail
                                                    join link in appDbContext.RecipeLink
                                                    on detail.RecipeId equals link.Id
                                                    join match in appDbContext.RecipeKeyword
                                                    on detail.Id equals match.DetailId
                                                    join keyword in appDbContext.Keyword
                                                    on match.KeywordId equals keyword.Id
                                                    where keyword.KeywordName.Contains(searchInput)
                                                    select new
                                                    {
                                                        Id = detail.RecipeId,
                                                        Url = link.Url,
                                                        Image = detail.Image,
                                                        Title = detail.Title,
                                                    }
                                           )
                                           .ToList().Distinct();
                    count = recipeCategoryCollection.Count();
                    PageCount();
                    var collections = recipeCategoryCollection.Skip(position).Take(itemPerPage);
                    if (collections != null)
                    {
                        return Ok(new { count, collections });
                    }
                    else
                    {
                        return BadRequest("no recipe!");
                    }
                }
            }
            
        }
        
        // get a single recipe (uncategorized)
        [HttpGet("{id}")]
        public IActionResult GetId([FromRoute] int id){
            var singleRecipe = (from detail in appDbContext.Detail
                                join link in appDbContext.RecipeLink
                                on detail.RecipeId equals link.Id
                                select new {
                                    Id = detail.RecipeId,
                                    Url = link.Url,
                                    Image = detail.Image,
                                    Title = detail.Title,
                                    Ingredient = detail.Ingredient,
                                    Instruction = detail.Instruction,
                                    TimeSet = detail.TotalTime
                                })
                                .Where(detail => detail.Id == id)
                                .Take(1);

            var keywordCollection = (from detail in appDbContext.Detail
                                     join match in appDbContext.RecipeKeyword
                                     on detail.Id equals match.DetailId
                                     join keyword in appDbContext.Keyword
                                     on match.KeywordId equals keyword.Id 
                                     where detail.Id.Equals(id)
                                     select new{
                                        Keyword = keyword.KeywordName
                                     }).ToList();

            if (singleRecipe == null || keywordCollection == null)
            {
                return NotFound();
            }else{
                return Ok(new {singleRecipe, keywordCollection});
            }
        }

        [HttpPost("random")]
        public IActionResult PostRandomRecipe([FromBody] string content){
            var randomRecipe = (from detail in appDbContext.Detail
                                join link in appDbContext.RecipeLink
                                on detail.RecipeId equals link.Id
                                join match in appDbContext.RecipeKeyword
                                on detail.Id equals match.DetailId
                                join keyword in appDbContext.Keyword
                                on match.KeywordId equals keyword.Id
                                where keyword.KeywordName.Contains(content)
                                select new
                                {
                                    Id = link.Id,
                                    Url = link.Url,
                                    Image = detail.Image,
                                    Title = detail.Title,
                                    TimeSet = detail.TotalTime,
                                }
                                )
                                .OrderBy(r => EF.Functions.Random()).Take(1);
            if(randomRecipe.Count() > 0){
                return Ok(randomRecipe);
            }else{
                return BadRequest("can not find");
            }
        }
    }
}
using System.Diagnostics;
using Data.Context;
using Data.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;

namespace WebScrappingConsole{
  public class Program{
    private static IConfiguration? configuration {get; set;}
    private static string? ScrapeURL {get; set;}
    private static string? recipeName {get; set;}
    private static string? recipeSrc { get; set; }

    private static List<string> categoryList { get; set; }
    private static List<string> timeList { get; set; }
    private static List<string> ingredientList { get; set; }
    private static List<string> instructionList { get; set; }
    private static List<RecipeLink> Recipes = new List<RecipeLink>();

    private static AppDbContext appDbContext = new AppDbContext();
    public static async Task Main(string[] args){
      // get url string
      configuration = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
      if(configuration!=null){
        ScrapeURL = configuration["ScrapeURL"].ToString();
        Console.WriteLine(ScrapeURL);
      }
      // download the browser executable
      await new BrowserFetcher().DownloadAsync();
      // browser execution configs
      var launchOptions  = new LaunchOptions{
        Headless = true,
      };
      using(var browser = await Puppeteer.LaunchAsync(launchOptions)){
        using(var page = await browser.NewPageAsync()){
          // visit the target page
          await page.GoToAsync(ScrapeURL);
          // css selector for next page link
          var nextPageLink = await page.QuerySelectorAsync(".pagination-next");
          // total time for viewing all pages takes around 9,742 secs
          Stopwatch stopwatch = new Stopwatch();
          stopwatch.Start();
          // check if it's the end page
          while (nextPageLink != null)
          {
            // refresh the css selector again 
            nextPageLink = await page.QuerySelectorAsync(".pagination-next");
            // fetch the recipe url
            await FetchRecipeUrl(page);
            if(nextPageLink !=null){
              // navigate to the next page
              await page.ClickAsync(".pagination-next");
              await page.WaitForNavigationAsync();
            }
          }
          foreach(var recipe in Recipes){
            // render the recipe detail
            await FetchRecipeDetail(page, recipe);
          }
          stopwatch.Stop();
          Console.WriteLine(stopwatch.Elapsed.TotalSeconds.ToString());
        }
      }
    }

    public static async Task FetchRecipeUrl(IPage page){
      // css selector for all recipe link
      var links = await page.QuerySelectorAllAsync(".entry-title-link");
      // iterate through the links
      foreach(var link in links){
        var url = await link.EvaluateFunctionAsync<string>("a=>a.href");
        // check if the url is already existed in the db
        var findURL = await appDbContext.RecipeLink.SingleOrDefaultAsync(a=>a.Url == url);
        // the url was not found in the db
        if(findURL == null){
          // instantiate a new url object
          var newURL = new RecipeLink{Url=url};
          // store the fetch url in the list
          Recipes.Add(newURL);
          // insert the fetch url to the db
          await appDbContext.RecipeLink.AddAsync(newURL);
          await appDbContext.SaveChangesAsync();
        }
      }
    }

    public static async Task FetchRecipeDetail(IPage page, RecipeLink recipe){
      try{
        // redirect the page to the designated url
        string url = recipe.Url;
        await page.GoToAsync(url);
        // a collection of recipe detail
        var recipeImgDiv = await page.QuerySelectorAsync(".wp-block-image");
        var recipeNameDiv = await page.QuerySelectorAsync(".wprm-recipe-name");
        var keywordDiv = await page.QuerySelectorAsync(".wprm-recipe-keyword");
        var timeDiv = await page.QuerySelectorAsync(".wprm-recipe-times-container");
        var categories = await page.QuerySelectorAllAsync(".entry-categories");
        var ingredientCollection = await page.QuerySelectorAllAsync(".wprm-recipe-ingredient");
        var instructionCollection = await page.QuerySelectorAllAsync(".wprm-recipe-instruction-text");
        
        // detail data that has to be inserted into the db
        categoryList = new List<string>();
        timeList = new List<string>();
        ingredientList = new List<string>();
        instructionList = new List<string>();

        if (recipeNameDiv!=null && 
            recipeImgDiv != null && 
            timeDiv != null && 
            categories != null && 
            ingredientCollection != null && 
            instructionCollection != null)
        {
          recipeName = await recipeNameDiv.EvaluateFunctionAsync<string>("name => name.innerText");
          recipeSrc = await recipeImgDiv.QuerySelectorAsync("figure img").EvaluateFunctionAsync<string>("a=>a.src"); 
          // keyword in the recipe
          if(keywordDiv != null){
            string keywords = await keywordDiv.EvaluateFunctionAsync<string>("keyword => keyword.innerText");
            string[] keywordSet = keywords.Split(", ");
            // convert array to list
            categoryList = keywordSet.ToList();
          }
          else
          {
            // Keyword is not provided, process categories
            foreach (var category in categories)
            {
              string text = await category.EvaluateFunctionAsync<string>("_ => _.innerText");

              if (!string.IsNullOrEmpty(text))
              {
                // Query for anchor elements within the category
                var categoryAnchors = await category.QuerySelectorAllAsync("a");
                foreach (var anchor in categoryAnchors)
                {
                  string anchorText = await anchor.EvaluateFunctionAsync<string>("a => a.innerText");
                  categoryList.Add(anchorText);
                }
              }
            }
          }

          string timeText = await timeDiv.EvaluateFunctionAsync<string>("time=> time.innerText");
          string[] timeCollection = timeText.Split("\n");
          for (int i = 0; i < timeCollection.Length; i += 2)
          {
            if(i+1 >= timeCollection.Length){
              break;
            }
            string text = timeCollection[i] + " : " + timeCollection[i + 1];
            timeList.Add(text);
          }

          foreach(var ingredient in ingredientCollection){
            var spans = await ingredient.QuerySelectorAllAsync("span");
            string text = "";
            foreach(var span in spans){
              text += await span.EvaluateFunctionAsync<string>("_=>_.innerText") + " ";
            }
            ingredientList.Add(text);
          }

          foreach (var instruction in instructionCollection)
          {
            string text = await instruction.EvaluateFunctionAsync<string>("_=>_.innerText");
            instructionList.Add(text);
          }
          // insert data to the db
          await InsertData(recipe);
         
        }
      
      }catch(Exception ex){
        Console.WriteLine(ex.ToString());
      }
    }    

    public static async Task InsertData(RecipeLink recipe)
    {
      try{
        // find the current recipeId
        var findRecipe = await appDbContext.RecipeLink.SingleOrDefaultAsync(x => x.Url == recipe.Url);
        if (findRecipe != null)
        {
          // create a detail object 
          var insertedDetail = new Detail
          {
            Title = recipeName,
            Image = recipeSrc,
            TotalTime = timeList.ToArray(),
            Ingredient = ingredientList.ToArray(),
            Instruction = instructionList.ToArray(),
            RecipeId = findRecipe.Id
          };
          await appDbContext.Detail.AddAsync(insertedDetail);
          await appDbContext.SaveChangesAsync();

          // create a key object
          foreach (var keyword in categoryList)
          {

            // find if the current keyword exists in the db
            var findKeyword = await appDbContext.Keyword.SingleOrDefaultAsync(x => x.KeywordName == keyword);

            // no exist in the db
            if (findKeyword == null)
            {
              // add keyword to the db
              appDbContext.Keyword.Add(new Keyword { KeywordName = keyword });
              await appDbContext.SaveChangesAsync();
              // find keyword again
              findKeyword = await appDbContext.Keyword.SingleOrDefaultAsync(x => x.KeywordName == keyword);
            }

            //create a recipe detail object
            var findDetail = await appDbContext.Detail.SingleOrDefaultAsync(x => x.RecipeId == findRecipe.Id);
            if (findDetail != null && findKeyword != null)
            {
              var detailKeyword = new RecipeKeyword
              {
                DetailId = findDetail.Id,
                KeywordId = findKeyword.Id
              };
              appDbContext.RecipeKeyword.Add(detailKeyword);
              await appDbContext.SaveChangesAsync();
            }
          }
        }

      }
      catch (Exception ex){
        Console.WriteLine(ex.ToString());
      } 
    }
  }
}

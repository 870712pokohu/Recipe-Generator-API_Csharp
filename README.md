# 💻 Recipe Scraper & RESTful API
Welcome to the Recipe Scraper & RESTful API project!
- This project is inspired by the culinary wonders showcased on [TiffyCooks](https://tiffycooks.com/). 
- This repository merges ASP.NET Core, EF Core, PuppeterSharp, and .NET 8, offering an immersive experience in crafting Web APIs and seamlessly extracting recipe data from the web.
- [Navigate to frontend UI](https://github.com/870712pokohu/Recipe-Generator-Client)

### 🕸️ Web API Route

![image](https://github.com/870712pokohu/recipegenerator-api_csharp/assets/46664953/a3f3f42e-cd23-4f12-8f8a-43e619657b3c)

## :zap: Usage
### 🖥️ RTE Configuration (Prerequisite)
1. [.NET 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
2. [MySQL](https://dev.mysql.com/downloads/installer/)
###  :electric_plug: Installation
1. Clone the project directory to the designated location
```
git clone https://github.com/870712pokohu/recipegenerator-api_csharp.git
cd recipegenerator-api_csharp
```
2. Setup the Database
 - Install [MySQL](https://dev.mysql.com/downloads/installer/)
 - Update the connection string in the `appsettings.json` under the **ConnectionStrings**
   ```
   "ConnectionStrings": {
    "MySQL_Connection_String": "server=localhostName;port=portNumber;database=database_name;user=user_name;password=password"}
   ```
 - Run Migrations
   ```
   dotnet ef database update 
   ```
 - Build and Run the API
   (explore the API at `htttp://localhost:5002`)
   ```
   dotnet build
   dotnet watch run
   ```
3. Run Recipe Scraper Console App
  - Navigate to **WebScrappingConsole** directory
    ```
    cd WebScrappingConsole
    ```
  - Build and Run the console app
    ```
    dotnet build
    dotnet run
    ```
  - Check the Scrape Recipe Data in **MySQL Workbench**
    ```
    SELECT * FROM RecipeLink;
    SELECT * FROM Detail;
    SELECT * FROM Keyword;
    SELECT * FROM RecipeKeyword;
    ```

## 👨 Contributor
[Jin Ci Hu](https://github.com/870712pokohu)

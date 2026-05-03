using BlogMinimalApi.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseStaticFiles();

var categories = new List<Category>
{
    new Category { Id = 1, Name = "Category01" },
    new Category { Id = 2, Name = "Category02" },
    new Category { Id = 3, Name = "Category03" }
};

var posts = new List<Post>
{
    new Post
    {
        Id = 1,
        Title = "PostTitle01",
        Slug = "post01-slug",
        Content = "Text and Content...",
        ImageUrl = "/images/url.png",
        Categories = new List<Category> { categories[0], categories[2] }
    },
    new Post
    {
        Id = 2,
        Title = "PostTitle02",
        Slug = "post02-slug",
        Content = "Content and Text...",
        ImageUrl = "/images/url.png",
        Categories = new List<Category> { categories[1], categories[2] }
    }
};


app.MapGet("/posts", () => Results.Ok(posts));
app.MapGet("/posts/{identifier}", (string identifier) =>
{
    if (int.TryParse(identifier, out int id))
    {
        var postById = posts.FirstOrDefault(p => p.Id == id);
        return postById != null ? Results.Ok(postById) : Results.NotFound("Пост не найден");
    }

    var postBySlug = posts.FirstOrDefault(p => p.Slug.ToLower() == identifier.ToLower());
    return postBySlug != null ? Results.Ok(postBySlug) : Results.NotFound("Пост не найден");
});

app.MapGet("/posts/category/{categoryName}", (string categoryName) =>
{
    var filteredPosts = posts.Where(p => p.Categories.Any(c => c.Name.ToLower() == categoryName.ToLower())).ToList();
    return Results.Ok(filteredPosts);
});

app.MapGet("/categories", () =>
{
    var categoriesWithLinks = categories.Select(c => new
    {
        Category = c,
        LinkToPosts = $"/posts/category/{c.Name.ToLower()}"
    });
    return Results.Ok(categoriesWithLinks);
});

app.MapPost("/posts/add", ([FromBody] Post newPost) =>
{
    if (string.IsNullOrWhiteSpace(newPost.Title) || string.IsNullOrWhiteSpace(newPost.Slug))
        return Results.BadRequest("Title и Slug обязательны!");

    newPost.Id = posts.Any() ? posts.Max(p => p.Id) + 1 : 1;
    posts.Add(newPost);
    return Results.Created($"/posts/{newPost.Id}", newPost);
});

app.MapPost("/categories/add", ([FromBody] Category newCategory) =>
{
    if (string.IsNullOrWhiteSpace(newCategory.Name))
        return Results.BadRequest("Имя категории обязательно!");

    newCategory.Id = categories.Any() ? categories.Max(c => c.Id) + 1 : 1;
    categories.Add(newCategory);
    return Results.Ok(newCategory);
});

app.MapPost("/categories/edit/{id}", (int id, [FromBody] Category updatedCategory) =>
{
    var category = categories.FirstOrDefault(c => c.Id == id);
    if (category == null) return Results.NotFound("Категория не найдена");

    if (string.IsNullOrWhiteSpace(updatedCategory.Name))
        return Results.BadRequest("Имя не может быть пустым!");

    category.Name = updatedCategory.Name;
    return Results.Ok(category);
});

app.MapPost("/posts/edit/{id}", (int id, [FromBody] Post updatedPost) =>
{
    var post = posts.FirstOrDefault(p => p.Id == id);
    if (post == null) return Results.NotFound("Пост не найден");

    post.Title = updatedPost.Title;
    post.Slug = updatedPost.Slug;
    post.Content = updatedPost.Content;
    post.ImageUrl = updatedPost.ImageUrl;
    return Results.Ok(post);
});

app.MapDelete("/posts/delete/{id}", (int id) =>
{
    var post = posts.FirstOrDefault(p => p.Id == id);
    if (post == null) return Results.NotFound();

    posts.Remove(post);
    return Results.Ok("Пост удален");
});

app.MapDelete("/categories/delete/{id}", (int id) =>
{
    var category = categories.FirstOrDefault(c => c.Id == id);
    if (category == null) return Results.NotFound();

    categories.Remove(category);
    foreach (var post in posts) { post.Categories.RemoveAll(c => c.Id == id); }

    return Results.Ok("Категория удалена");
});

app.MapGet("/posts/search", ([FromQuery(Name = "q")] string searchQuery) =>
{
    if (string.IsNullOrWhiteSpace(searchQuery)) return Results.Ok(posts);

    var result = posts.Where(p => p.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                                  p.Content.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
    return Results.Ok(result);
});

app.MapGet("/about", (IWebHostEnvironment env) =>
{
    var filepath = Path.Combine(env.WebRootPath, "about.html");
    return Results.File(filepath, "text/html");
});

app.Run();

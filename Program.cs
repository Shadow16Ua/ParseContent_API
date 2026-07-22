using Microsoft.AspNetCore.Mvc.Diagnostics;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseHttpsRedirection();

app.MapPost("/api/v1/parse-content", (ParseRequest request) =>
{
    var decodedContent = Helper.base64Decode(request.Content);  // Decode the base64 content

    // Initialize variables to hold the result count and data
    int resultCount = 0;    
    object resultData = null;   

    if (request.Type == ContentType.CSV)
    {
        var lines = decodedContent.Split('\n'); // Split the content into 
        List<string[]> parsedData = new List<string[]>();   // Create a list to hold the parsed data

        foreach (var line in lines)             
        {
            var values = line.Split(',');   // Split each item into values by comma
            parsedData.Add(values);
            resultCount++;
            resultData = parsedData;
        }
    }
    else if (request.Type == ContentType.INTERNAL_JSON)
    {
        var jsonData = JsonSerializer.Deserialize<object>(decodedContent);  // Deserialize the JSON content
        resultCount = 1;
        resultData = jsonData;
    }
    else
    {
        return Results.BadRequest("Invalid content type");
    }

    return Results.Ok(new { status = "Success", Count = resultCount, Data = resultData });
});

app.Run();


public enum ContentType
{
    CSV,
    INTERNAL_JSON,
}

public record ParseRequest(ContentType Type, string Content);
public static class Helper { 
    public static string base64Decode(string base64EncodedData)
    {
        byte[] convertedBase64 = Convert.FromBase64String(base64EncodedData);
        return System.Text.Encoding.UTF8.GetString(convertedBase64);
    }
}

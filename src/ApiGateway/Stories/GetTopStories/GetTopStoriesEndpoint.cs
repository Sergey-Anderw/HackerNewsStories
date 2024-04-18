using Carter;
using MediatR;
using Stories.API.Models;

namespace Stories.API.Stories.GetTopStories
{
    public record GetTopStoriesResponse(IEnumerable<Story> Stories);

    public class GetTopStoriesEndpoint : ICarterModule
    {
        public void AddRoutes(IEndpointRouteBuilder app)
        {
            app.MapGet("/stories/{count}", async (int count, ISender sender) =>
                {
                    var response = await sender.Send(new GetTopStoriesQuery(count));

                    return Results.Ok(response);
                })
                .WithName("GetTopStories")
                .Produces<GetTopStoriesResponse>()
                .ProducesProblem(StatusCodes.Status400BadRequest)
                .WithSummary("Get Top Stories By Count")
                .WithDescription("Get Top Stories By Count");
        }
    }
}

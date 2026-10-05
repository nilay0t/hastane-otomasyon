using HastaneOtomasyon.Data.Context;
using HastaneOtomasyon.Data.Entities;
using HastaneOtomasyon.Shared.DTOs.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace HastaneOtomasyon.API.Endpoints
{
    public static class PoliklinikEndpoints
    {
        public static IEndpointRouteBuilder MapPoliklinikEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/poliklinikler");

            group.MapGet("/", GetAllAsync);

            return routes;
        }

        private static async Task<IResult> GetAllAsync(AppDbContext context)
        {
            var poliklinikler = await context.Poliklinikler
                .OrderBy(p => p.PoliklinikAdi)
                .ToListAsync();

            return Results.Ok(new ApiResponse<List<Poliklinik>>(
                true,
                "Poliklinikler basariyla listelendi.",
                poliklinikler));
        }
    }
}

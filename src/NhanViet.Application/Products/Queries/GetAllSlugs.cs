using MediatR;
using NhanViet.Domain.Interfaces;

namespace NhanViet.Application.Products.Queries;

public record GetAllSlugsQuery : IRequest<List<string>>;

public class GetAllSlugsHandler(IProductRepository products)
    : IRequestHandler<GetAllSlugsQuery, List<string>>
{
    public Task<List<string>> Handle(GetAllSlugsQuery req, CancellationToken ct) =>
        products.GetAllSlugsAsync(ct);
}

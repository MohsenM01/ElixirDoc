namespace Identity.Contracts;

public interface IModule
{
    IEndpointRouteBuilder RegisterEndpoints(IEndpointRouteBuilder endpoints);
}


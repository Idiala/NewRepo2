using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

public class RoutePrefixConvention(string prefix) : IApplicationModelConvention
{
    private readonly AttributeRouteModel _routePrefix = new(new RouteAttribute(prefix));

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            foreach (var selector in controller.Selectors.Where(s => s.AttributeRouteModel != null))
            {
                selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                    _routePrefix,selector.AttributeRouteModel
                );
            }
        }
    }
}

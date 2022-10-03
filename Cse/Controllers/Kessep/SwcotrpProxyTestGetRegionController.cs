// Program: SWCOTRP_PROXY_TEST_GET_REGION, ID: 374536108, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<SwcotrpProxyTestGetRegion.Import>;
using Response = Client.ServerRequest<SwcotrpProxyTestGetRegion.Export>;

/// <summary>
/// Controller for the procedure SwcotrpProxyTestGetRegion.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SwcotrpProxyTestGetRegionController: ControllerBase
{
  /// <summary>
  /// Executes the server procedure.
  /// </summary>
  /// <param name="request">A request instance.</param>
  /// <returns>A response instance.</returns>
  [HttpPost]
  public Response Execute(
    [FromBody]
    Request request) => Client.Execute<Response>(
      HttpContext.RequestServices, User,
      ControllerContext.ActionDescriptor.ControllerName, request);
}

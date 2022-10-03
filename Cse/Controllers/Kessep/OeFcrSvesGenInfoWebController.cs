// Program: OE_FCR_SVES_GEN_INFO_WEB, ID: 945074630, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<OeFcrSvesGenInfoWeb.Import>;
using Response = Client.ServerRequest<OeFcrSvesGenInfoWeb.Export>;

/// <summary>
/// Controller for the procedure OeFcrSvesGenInfoWeb.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OeFcrSvesGenInfoWebController: ControllerBase
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

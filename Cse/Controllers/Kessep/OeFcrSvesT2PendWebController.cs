// Program: OE_FCR_SVES_T2_PEND_WEB, ID: 945076820, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<OeFcrSvesT2PendWeb.Import>;
using Response = Client.ServerRequest<OeFcrSvesT2PendWeb.Export>;

/// <summary>
/// Controller for the procedure OeFcrSvesT2PendWeb.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OeFcrSvesT2PendWebController: ControllerBase
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

// Program: OE_FCR_SVES_T16_WEB, ID: 945076017, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<OeFcrSvesT16Web.Import>;
using Response = Client.ServerRequest<OeFcrSvesT16Web.Export>;

/// <summary>
/// Controller for the procedure OeFcrSvesT16Web.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OeFcrSvesT16WebController: ControllerBase
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

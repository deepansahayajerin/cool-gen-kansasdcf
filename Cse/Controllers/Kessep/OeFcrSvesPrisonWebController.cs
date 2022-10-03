// Program: OE_FCR_SVES_PRISON_WEB, ID: 945075439, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<OeFcrSvesPrisonWeb.Import>;
using Response = Client.ServerRequest<OeFcrSvesPrisonWeb.Export>;

/// <summary>
/// Controller for the procedure OeFcrSvesPrisonWeb.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OeFcrSvesPrisonWebController: ControllerBase
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

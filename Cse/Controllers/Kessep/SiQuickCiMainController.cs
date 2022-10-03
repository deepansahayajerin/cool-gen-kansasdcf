// Program: SI_QUICK_CI_MAIN, ID: 374541231, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<SiQuickCiMain.Import>;
using Response = Client.ServerRequest<SiQuickCiMain.Export>;

/// <summary>
/// Controller for the procedure SiQuickCiMain.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SiQuickCiMainController: ControllerBase
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

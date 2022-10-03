// Program: SI_QUICK_AUDIT_MAIN, ID: 374537051, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<SiQuickAuditMain.Import>;
using Response = Client.ServerRequest<SiQuickAuditMain.Export>;

/// <summary>
/// Controller for the procedure SiQuickAuditMain.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SiQuickAuditMainController: ControllerBase
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

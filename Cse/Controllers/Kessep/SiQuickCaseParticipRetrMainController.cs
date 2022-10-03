// Program: SI_QUICK_CASE_PARTICIP_RETR_MAIN, ID: 374540663, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<SiQuickCaseParticipRetrMain.Import>;
using Response = Client.ServerRequest<SiQuickCaseParticipRetrMain.Export>;

/// <summary>
/// Controller for the procedure SiQuickCaseParticipRetrMain.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SiQuickCaseParticipRetrMainController: ControllerBase
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

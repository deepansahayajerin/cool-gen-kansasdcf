// Program: SI_QUICK_CASE_ACTIV_RETR_MAIN, ID: 374537221, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<SiQuickCaseActivRetrMain.Import>;
using Response = Client.ServerRequest<SiQuickCaseActivRetrMain.Export>;

/// <summary>
/// Controller for the procedure SiQuickCaseActivRetrMain.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SiQuickCaseActivRetrMainController: ControllerBase
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

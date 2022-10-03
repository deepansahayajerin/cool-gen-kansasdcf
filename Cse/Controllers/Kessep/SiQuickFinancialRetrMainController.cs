// Program: SI_QUICK_FINANCIAL_RETR_MAIN, ID: 374541607, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<SiQuickFinancialRetrMain.Import>;
using Response = Client.ServerRequest<SiQuickFinancialRetrMain.Export>;

/// <summary>
/// Controller for the procedure SiQuickFinancialRetrMain.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SiQuickFinancialRetrMainController: ControllerBase
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

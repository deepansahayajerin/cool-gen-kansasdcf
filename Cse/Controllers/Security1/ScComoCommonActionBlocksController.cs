// Program: SC_COMO_COMMON_ACTION_BLOCKS, ID: 371452949, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Security1.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.ServerRequest<ScComoCommonActionBlocks.Import>;
using Response = Client.ServerRequest<ScComoCommonActionBlocks.Export>;

/// <summary>
/// Controller for the procedure ScComoCommonActionBlocks.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ScComoCommonActionBlocksController: ControllerBase
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

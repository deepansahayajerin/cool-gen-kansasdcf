// Program: CO_CDNL_LIST_CODE_NAMES, ID: 371823068, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.Request<CoCdnlListCodeNames.Import>;
using Response =
  Client.Response<CoCdnlListCodeNames.Import, CoCdnlListCodeNames.Export>;

/// <summary>
/// Controller for the procedure CoCdnlListCodeNames.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CoCdnlListCodeNamesController: ControllerBase
{
  /// <summary>
  /// Gets the current state.
  /// </summary>
  /// <param name="index">A state index.</param>
  /// <param name="id">A procedure id.</param>
  /// <returns>A response instance.</returns>
  [HttpGet]
  public Response Get(
    [FromQuery]
    int index,
    [FromQuery]
    int id) => Client.
      Get<Response>(HttpContext.RequestServices, User, index, id);

  /// <summary>
  /// Executes an event.
  /// </summary>
  /// <param name="request">A request instance.</param>
  /// <returns>A response instance.</returns>
  [HttpPost("event")]
  public Response Event(
    [FromBody]
    Request request) => Client.Event<Response>(
      HttpContext.RequestServices, User, request);
}

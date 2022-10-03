// Program: FN_CRAM_MTN_CASH_SRCE_ASSIGNMENT, ID: 372315632, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.Request<FnCramMtnCashSrceAssignment.Import>;
using Response =
  Client.Response<FnCramMtnCashSrceAssignment.Import,
  FnCramMtnCashSrceAssignment.Export>;

/// <summary>
/// Controller for the procedure FnCramMtnCashSrceAssignment.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class FnCramMtnCashSrceAssignmentController: ControllerBase
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

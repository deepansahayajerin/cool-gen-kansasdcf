// Program: LE_WKCD_WORKERS_COMP_DETAIL, ID: 1625337202, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.Request<LeWkcdWorkersCompDetail.Import>;
using Response =
  Client.Response<LeWkcdWorkersCompDetail.Import,
  LeWkcdWorkersCompDetail.Export>;

/// <summary>
/// Controller for the procedure LeWkcdWorkersCompDetail.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LeWkcdWorkersCompDetailController: ControllerBase
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

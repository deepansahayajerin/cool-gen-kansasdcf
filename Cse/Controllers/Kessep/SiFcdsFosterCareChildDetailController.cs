// Program: SI_FCDS_FOSTER_CARE_CHILD_DETAIL, ID: 371758425, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.Request<SiFcdsFosterCareChildDetail.Import>;
using Response =
  Client.Response<SiFcdsFosterCareChildDetail.Import,
  SiFcdsFosterCareChildDetail.Export>;

/// <summary>
/// Controller for the procedure SiFcdsFosterCareChildDetail.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SiFcdsFosterCareChildDetailController: ControllerBase
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

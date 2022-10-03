// Program: OE_CALC_CALCULATE_WKSHEET_PAGE_1, ID: 371397314, model: 746.
using Microsoft.AspNetCore.Mvc;
using Bphx.Cool;

namespace Gov.Kansas.DCF.Cse.Kessep.Controllers;

using Microsoft.AspNetCore.Authorization;

using Request = Client.Request<OeCalcCalculateWksheetPage1.Import>;
using Response =
  Client.Response<OeCalcCalculateWksheetPage1.Import,
  OeCalcCalculateWksheetPage1.Export>;

/// <summary>
/// Controller for the procedure OeCalcCalculateWksheetPage1.
/// </summary>
[ApiController]
[Authorize]
[Route("api/[controller]")]
public class OeCalcCalculateWksheetPage1Controller: ControllerBase
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

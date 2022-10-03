using System;
using System.IO;

using Microsoft.AspNetCore.Http;

using Bphx.Cool;

namespace Gov.KansasDCF.Cse.App;

public class State: ISessions
{
  public State(
    IHttpContextAccessor httpContextAccessor,
    ISerializer serializer)
  {
    this.httpContext = httpContextAccessor.HttpContext;
    this.serializer = serializer;
  }

  public void Dispose()
  {
  }

  public string Id => httpContext.Session.Id;

  public ISessionManager this[int index]
  {
    get
    {
      var id = key + index;
      var data = httpContext.Session.Get(id);

      return data == null ? null :
        serializer.Deserilize<ISessionManager>(new MemoryStream(data));
    }
    set
    {
      var id = key + index;

      if(value == null)
      {
        httpContext.Session.Remove(id);
      }
      else
      {
        var stream = new MemoryStream();

        serializer.Serilize(value, stream);
        httpContext.Session.Set(key + index, stream.ToArray());
      }
    }
  }

  public int Create()
  {
    for(var i = 0; i < int.MaxValue; ++i)
    {
      if(httpContext.Session.Get(key + i) == null)
      {
        return i;
      }
    }

    throw new InvalidOperationException("Cannot allocate a session");
  }

  private readonly HttpContext httpContext;
  private readonly ISerializer serializer;

  private const string key = "bphx.sessions.";
}

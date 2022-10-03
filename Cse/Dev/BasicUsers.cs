using System.Collections.Generic;
using System.Linq;

using Microsoft.Extensions.Options;

namespace Gov.KansasDCF.Cse.App
{
  /// <summary>
  /// Users store.
  /// </summary>
  public class BasicUsers
  {
    public BasicUsers(IOptions<List<BasicUser>> users)
    {
      Users = users.Value.ToDictionary(item => item.UserName);
    }

    /// <summary>
    /// A dictionary of users.
    /// </summary>
    public Dictionary<string, BasicUser> Users { get; }
  }
}

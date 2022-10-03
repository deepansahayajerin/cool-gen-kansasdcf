using System.Collections.Generic;

namespace Gov.KansasDCF.Cse.App;

/// <summary>A simple EBCDIC based collator.</summary>
public class EbcdicCollator: Comparer<string>
{
  /// <summary>
  /// Compares two strings.
  /// </summary>
  /// <param name="x">First string to compare.</param>
  /// <param name="y">Second string to compare.</param>
  /// <returns>Comparision result.</returns>
  public override int Compare(string x, string y)
  {
    var xlength = x?.Length ?? 0;
    var ylength = y?.Length ?? 0;
    var length = xlength > ylength ? xlength : ylength;

    for(var i = 0; i < length; ++i)
    {
      var c = Compare(i < xlength ? x[i] : ' ', i < ylength ? y[i] : ' ');

      if (c != 0)
      {
        return c;
      }
    }

    return 0;
  }

  /// <summary>
  /// Compares two characters using EBCDIC encoding.
  /// </summary>
  /// <param name="first">First character to compare.</param>
  /// <param name="second">Second character to compare.</param>
  /// <returns>
  /// Zero if characters are considered equal, 
  /// -1 if the first character follows before the second, and 
  /// 1 if the first character follows before the second.
  /// </returns>
  public static int Compare(char first, char second) =>
    first == second ? 0 :
      order.TryGetValue(first, out int firstIndex) &&
        order.TryGetValue(second, out int secondIndex) ? 
        firstIndex - secondIndex : first - second;

  static EbcdicCollator()
  {
    const string chars = 
     " ¢.<(+|" +
     "&!$*);¬" +
     "-/¦,%_>?" +
     "`:#@'=\"" +
     "abcdefghi±" +
     "jklmnopqr" +
     "~stuvwxyz" +
     "^[]" +
     "{ABCDEFGHI" +
     "}JKLMNOPQR" +
     "\\STUVWXYZ" +
     "0123456789";

    for(var i = 0; i < chars.Length; ++i)
    {
      order[chars[i]] = i;
    }
  }

  /// <summary>
  /// EBCDIC ordered characters.
  /// </summary>
  private static readonly Dictionary<char, int> order = new();
}

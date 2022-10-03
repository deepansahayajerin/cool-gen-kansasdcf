// Program: CAB_FCR_FORMAT_CITY_OF_BIRTH, ID: 371066918, model: 746.
// Short name: SWE01286
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: CAB_FCR_FORMAT_CITY_OF_BIRTH.
/// </summary>
[Serializable]
public partial class CabFcrFormatCityOfBirth: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the CAB_FCR_FORMAT_CITY_OF_BIRTH program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new CabFcrFormatCityOfBirth(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of CabFcrFormatCityOfBirth.
  /// </summary>
  public CabFcrFormatCityOfBirth(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    MoveCsePerson(import.CsePerson, export.CsePerson);

    if (IsEmpty(export.CsePerson.BirthPlaceCity))
    {
      return;
    }

    for(local.ForLoop.Count = 1; local.ForLoop.Count <= 15; ++
      local.ForLoop.Count)
    {
      local.OmitPosition.Count =
        Verify(export.CsePerson.BirthPlaceCity, "ABCDEFGHIJKLMNOPQRSTUVWXYZ");

      if (local.OmitPosition.Count < 1)
      {
        break;
      }

      if (local.OmitPosition.Count == 15)
      {
        export.CsePerson.BirthPlaceCity =
          Substring(export.CsePerson.BirthPlaceCity, 1, 14);
      }
      else if (local.OmitPosition.Count == 1)
      {
        export.CsePerson.BirthPlaceCity =
          Substring(export.CsePerson.BirthPlaceCity, 2, 14);
      }
      else
      {
        local.Next.Count = local.OmitPosition.Count + 1;
        local.Length.Count = 15 - local.OmitPosition.Count;
        export.CsePerson.BirthPlaceCity =
          Substring(export.CsePerson.BirthPlaceCity, 15, 1,
          local.OmitPosition.Count - 1) + Substring
          (export.CsePerson.BirthPlaceCity, 15, local.Next.Count,
          local.Length.Count);
      }
    }

    local.Substrung.Text3 = export.CsePerson.BirthPlaceCity ?? Spaces(3);

    switch(TrimEnd(local.Substrung.Text3))
    {
      case "XXX":
        export.CsePerson.BirthPlaceCity = "";

        break;
      case "UNK":
        export.CsePerson.BirthPlaceCity = "";

        break;
      case "KC":
        export.CsePerson.BirthPlaceCity = "KANSASCITY";

        break;
      case "NY":
        export.CsePerson.BirthPlaceCity = "NEWYORK";

        break;
      default:
        local.Length.Count = Length(TrimEnd(export.CsePerson.BirthPlaceCity));

        if (local.Length.Count < 3)
        {
          export.CsePerson.BirthPlaceCity = "";
        }

        break;
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.BirthPlaceCity = source.BirthPlaceCity;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of OmitPosition.
    /// </summary>
    [JsonPropertyName("omitPosition")]
    public Common OmitPosition
    {
      get => omitPosition ??= new();
      set => omitPosition = value;
    }

    /// <summary>
    /// A value of Next.
    /// </summary>
    [JsonPropertyName("next")]
    public Common Next
    {
      get => next ??= new();
      set => next = value;
    }

    /// <summary>
    /// A value of Length.
    /// </summary>
    [JsonPropertyName("length")]
    public Common Length
    {
      get => length ??= new();
      set => length = value;
    }

    /// <summary>
    /// A value of ForLoop.
    /// </summary>
    [JsonPropertyName("forLoop")]
    public Common ForLoop
    {
      get => forLoop ??= new();
      set => forLoop = value;
    }

    /// <summary>
    /// A value of Substrung.
    /// </summary>
    [JsonPropertyName("substrung")]
    public WorkArea Substrung
    {
      get => substrung ??= new();
      set => substrung = value;
    }

    private Common omitPosition;
    private Common next;
    private Common length;
    private Common forLoop;
    private WorkArea substrung;
  }
#endregion
}

// Program: OE_GET_AR_FOR_URA_EXCESS_COLL, ID: 372762914, model: 746.
// Short name: SWE02019
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_GET_AR_FOR_URA_EXCESS_COLL.
/// </summary>
[Serializable]
public partial class OeGetArForUraExcessColl: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GET_AR_FOR_URA_EXCESS_COLL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGetArForUraExcessColl(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGetArForUraExcessColl.
  /// </summary>
  public OeGetArForUraExcessColl(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -----------------------------------------------------
    // Initial Version - SWSRKXD
    // Date - 6/24/99
    // This CAB has been coded after discussing with other URA
    // team members(Carl G, Sury G, Kent P) and is in sync with
    // the rest of URA batches and reports that use excess_coll
    // to determine AR.
    // -------------------------------------------------------
    // ------------------------------------------
    // Both READ EACHes optimized for 1 row.
    // ------------------------------------------
    if (ReadImHouseholdMemberImHousehold())
    {
      if (ReadCsePersonCaseRole())
      {
        export.CsePerson.Number = entities.CsePerson.Number;
        MoveImHousehold(entities.ImHousehold, export.ImHousehold);

        return;
      }

      ExitState = "CO0000_AR_NF";

      return;
    }

    ExitState = "OE0000_IM_HOUSEHOLD_MEMBER_NF";
  }

  private static void MoveImHousehold(ImHousehold source, ImHousehold target)
  {
    target.AeCaseNo = source.AeCaseNo;
    target.ZdelType = source.ZdelType;
  }

  private bool ReadCsePersonCaseRole()
  {
    entities.CaseRole.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(
          command, "casNumber", entities.ImHouseholdMember.CseCaseNumber ?? ""
          );
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 0);
        entities.CaseRole.CasNumber = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.Populated = true;
        entities.CsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadImHouseholdMemberImHousehold()
  {
    entities.ImHousehold.Populated = false;
    entities.ImHouseholdMember.Populated = false;

    return Read("ReadImHouseholdMemberImHousehold",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", import.UraExcessCollection.InitiatingCsePerson);
          
        db.SetInt32(
          command, "seqNumber", import.UraExcessCollection.SequenceNumber);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMember.ImhAeCaseNo = db.GetString(reader, 0);
        entities.ImHousehold.AeCaseNo = db.GetString(reader, 0);
        entities.ImHouseholdMember.CspNumber = db.GetString(reader, 1);
        entities.ImHouseholdMember.StartDate = db.GetDate(reader, 2);
        entities.ImHouseholdMember.CseCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ImHousehold.ZdelType = db.GetNullableString(reader, 4);
        entities.ImHousehold.Populated = true;
        entities.ImHouseholdMember.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Entities entities = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    private UraExcessCollection uraExcessCollection;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private ImHousehold imHousehold;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of ImHousehold.
    /// </summary>
    [JsonPropertyName("imHousehold")]
    public ImHousehold ImHousehold
    {
      get => imHousehold ??= new();
      set => imHousehold = value;
    }

    /// <summary>
    /// A value of ImHouseholdMember.
    /// </summary>
    [JsonPropertyName("imHouseholdMember")]
    public ImHouseholdMember ImHouseholdMember
    {
      get => imHouseholdMember ??= new();
      set => imHouseholdMember = value;
    }

    /// <summary>
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of UraExcessCollection.
    /// </summary>
    [JsonPropertyName("uraExcessCollection")]
    public UraExcessCollection UraExcessCollection
    {
      get => uraExcessCollection ??= new();
      set => uraExcessCollection = value;
    }

    private CaseRole caseRole;
    private Case1 case1;
    private ImHousehold imHousehold;
    private ImHouseholdMember imHouseholdMember;
    private CsePerson csePerson;
    private UraExcessCollection uraExcessCollection;
  }
#endregion
}

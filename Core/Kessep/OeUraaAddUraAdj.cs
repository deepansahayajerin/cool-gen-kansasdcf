// Program: OE_URAA_ADD_URA_ADJ, ID: 372551167, model: 746.
// Short name: SWE01862
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_URAA_ADD_URA_ADJ.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block creates a URA Adjustment record.
/// </para>
/// </summary>
[Serializable]
public partial class OeUraaAddUraAdj: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_URAA_ADD_URA_ADJ program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeUraaAddUraAdj(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeUraaAddUraAdj.
  /// </summary>
  public OeUraaAddUraAdj(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    if (!ReadImHouseholdMember())
    {
      ExitState = "OE0000_IM_HOUSEHOLD_MEMBER_NF";

      return;
    }

    if (ReadUraAdjustment())
    {
      local.Last.Identifier = entities.ExistingLast.Identifier;
    }

    ++local.Last.Identifier;

    try
    {
      CreateUraAdjustment();
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "URA_ADJUSTMENT_AE";

          break;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }
  }

  private void CreateUraAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMember.Populated);

    var identifier = local.Last.Identifier;
    var adjYear = import.UraAdjustment.AdjYear;
    var adjMonth = import.UraAdjustment.AdjMonth;
    var adjAdcGrant = import.UraAdjustment.AdjAdcGrant.GetValueOrDefault();
    var adjPassthru = import.UraAdjustment.AdjPassthru.GetValueOrDefault();
    var adjMedAssistance =
      import.UraAdjustment.AdjMedAssistance.GetValueOrDefault();
    var adjFcGrant = import.UraAdjustment.AdjFcGrant.GetValueOrDefault();
    var adjHouseholdUra =
      import.UraAdjustment.AdjHouseholdUra.GetValueOrDefault();
    var adjReason = import.UraAdjustment.AdjReason ?? "";
    var createdBy = global.UserId;
    var createdTstamp = Now();
    var cspNumber = entities.ImHouseholdMember.CspNumber;
    var imhAeCaseNo = entities.ImHouseholdMember.ImhAeCaseNo;
    var ihmStartDate = entities.ImHouseholdMember.StartDate;
    var adjHholdMedicalUra =
      import.UraAdjustment.AdjHholdMedicalUra.GetValueOrDefault();

    entities.UraAdjustment.Populated = false;
    Update("CreateUraAdjustment",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetInt32(command, "adjYear", adjYear);
        db.SetInt32(command, "adjMonth", adjMonth);
        db.SetNullableDecimal(command, "adjAdcGrant", adjAdcGrant);
        db.SetNullableDecimal(command, "adjPassthru", adjPassthru);
        db.SetNullableDecimal(command, "adjMedAssist", adjMedAssistance);
        db.SetNullableDecimal(command, "adjFcGrant", adjFcGrant);
        db.SetNullableDecimal(command, "adjHura", adjHouseholdUra);
        db.SetNullableString(command, "adjReason", adjReason);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTstamp", createdTstamp);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "imhAeCaseNo", imhAeCaseNo);
        db.SetDate(command, "ihmStartDate", ihmStartDate);
        db.SetNullableDecimal(command, "adjMediUra", adjHholdMedicalUra);
      });

    entities.UraAdjustment.Identifier = identifier;
    entities.UraAdjustment.AdjYear = adjYear;
    entities.UraAdjustment.AdjMonth = adjMonth;
    entities.UraAdjustment.AdjAdcGrant = adjAdcGrant;
    entities.UraAdjustment.AdjPassthru = adjPassthru;
    entities.UraAdjustment.AdjMedAssistance = adjMedAssistance;
    entities.UraAdjustment.AdjFcGrant = adjFcGrant;
    entities.UraAdjustment.AdjHouseholdUra = adjHouseholdUra;
    entities.UraAdjustment.AdjReason = adjReason;
    entities.UraAdjustment.CreatedBy = createdBy;
    entities.UraAdjustment.CreatedTstamp = createdTstamp;
    entities.UraAdjustment.CspNumber = cspNumber;
    entities.UraAdjustment.ImhAeCaseNo = imhAeCaseNo;
    entities.UraAdjustment.IhmStartDate = ihmStartDate;
    entities.UraAdjustment.AdjHholdMedicalUra = adjHholdMedicalUra;
    entities.UraAdjustment.Populated = true;
  }

  private bool ReadImHouseholdMember()
  {
    entities.ImHouseholdMember.Populated = false;

    return Read("ReadImHouseholdMember",
      (db, command) =>
      {
        db.SetString(command, "imhAeCaseNo", import.ImHousehold.AeCaseNo);
        db.SetDate(
          command, "startDate",
          import.ImHouseholdMember.StartDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ImHouseholdMember.ImhAeCaseNo = db.GetString(reader, 0);
        entities.ImHouseholdMember.CspNumber = db.GetString(reader, 1);
        entities.ImHouseholdMember.StartDate = db.GetDate(reader, 2);
        entities.ImHouseholdMember.Populated = true;
      });
  }

  private bool ReadUraAdjustment()
  {
    System.Diagnostics.Debug.Assert(entities.ImHouseholdMember.Populated);
    entities.ExistingLast.Populated = false;

    return Read("ReadUraAdjustment",
      (db, command) =>
      {
        db.SetDate(
          command, "ihmStartDate",
          entities.ImHouseholdMember.StartDate.GetValueOrDefault());
        db.SetString(
          command, "imhAeCaseNo", entities.ImHouseholdMember.ImhAeCaseNo);
        db.
          SetString(command, "cspNumber", entities.ImHouseholdMember.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.ExistingLast.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLast.CspNumber = db.GetString(reader, 1);
        entities.ExistingLast.ImhAeCaseNo = db.GetString(reader, 2);
        entities.ExistingLast.IhmStartDate = db.GetDate(reader, 3);
        entities.ExistingLast.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
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
    /// A value of UraAdjustment.
    /// </summary>
    [JsonPropertyName("uraAdjustment")]
    public UraAdjustment UraAdjustment
    {
      get => uraAdjustment ??= new();
      set => uraAdjustment = value;
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

    private UraAdjustment uraAdjustment;
    private ImHouseholdMember imHouseholdMember;
    private ImHousehold imHousehold;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public UraAdjustment Last
    {
      get => last ??= new();
      set => last = value;
    }

    private UraAdjustment last;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingLast.
    /// </summary>
    [JsonPropertyName("existingLast")]
    public UraAdjustment ExistingLast
    {
      get => existingLast ??= new();
      set => existingLast = value;
    }

    /// <summary>
    /// A value of UraAdjustment.
    /// </summary>
    [JsonPropertyName("uraAdjustment")]
    public UraAdjustment UraAdjustment
    {
      get => uraAdjustment ??= new();
      set => uraAdjustment = value;
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

    private UraAdjustment existingLast;
    private UraAdjustment uraAdjustment;
    private ImHouseholdMember imHouseholdMember;
    private ImHousehold imHousehold;
    private CsePerson csePerson;
  }
#endregion
}

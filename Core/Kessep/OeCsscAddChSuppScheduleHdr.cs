// Program: OE_CSSC_ADD_CH_SUPP_SCHEDULE_HDR, ID: 371909264, model: 746.
// Short name: SWE00895
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CSSC_ADD_CH_SUPP_SCHEDULE_HDR.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeCsscAddChSuppScheduleHdr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CSSC_ADD_CH_SUPP_SCHEDULE_HDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCsscAddChSuppScheduleHdr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCsscAddChSuppScheduleHdr.
  /// </summary>
  public OeCsscAddChSuppScheduleHdr(IContext context, Import import,
    Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    export.ChildSupportSchedule.Assign(import.ChildSupportSchedule);
    UseOeCabSetMnemonics();

    if (ReadChildSupportSchedule2())
    {
      ExitState = "OE0000_CHILD_SUPP_SCHED_AE_RB";

      return;
    }

    if (Equal(import.ChildSupportSchedule.EffectiveDate, null))
    {
      export.ChildSupportSchedule.EffectiveDate = Now().Date;
    }

    if (Equal(import.ChildSupportSchedule.ExpirationDate, null))
    {
      export.ChildSupportSchedule.ExpirationDate = local.MaxDate.ExpirationDate;
    }

    if (ReadChildSupportSchedule1())
    {
      export.ChildSupportSchedule.Assign(import.ChildSupportSchedule);
      ExitState = "OE0000_CHILD_SUPP_SCHED_OVERLAP";

      return;
    }

    ReadChildSupportSchedule3();
    local.Work.Count = entities.ChildSupportSchedule.Identifier + 1;
    export.ChildSupportSchedule.Identifier =
      entities.ChildSupportSchedule.Identifier + 1;

    try
    {
      CreateChildSupportSchedule();
      export.ChildSupportSchedule.Assign(entities.ChildSupportSchedule);
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";

      if (Equal(export.ChildSupportSchedule.ExpirationDate,
        local.MaxDate.ExpirationDate))
      {
        export.ChildSupportSchedule.ExpirationDate = null;
      }
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "CHILD_SUPPORT_SCHEDULE_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    try
    {
      CreateAgeGroupSupportSchedule1();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "AGE_GROUP_SCHEDULE_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    try
    {
      CreateAgeGroupSupportSchedule3();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "AGE_GROUP_SCHEDULE_AE_RB";

          return;
        case ErrorCode.PermittedValueViolation:
          break;
        case ErrorCode.DatabaseError:
          break;
        default:
          throw;
      }
    }

    try
    {
      CreateAgeGroupSupportSchedule2();
      ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
    }
    catch(Exception e)
    {
      switch(GetErrorCode(e))
      {
        case ErrorCode.AlreadyExists:
          ExitState = "AGE_GROUP_SCHEDULE_AE_RB";

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

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private void CreateAgeGroupSupportSchedule1()
  {
    var cssIdentifier = entities.ChildSupportSchedule.Identifier;
    var maximumAgeInARange = 6;
    var ageGroupFactor = import.Import06.AgeGroupFactor;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cssGuidelineYr = entities.ChildSupportSchedule.CsGuidelineYear;

    entities.N06.Populated = false;
    Update("CreateAgeGroupSupportSchedule1",
      (db, command) =>
      {
        db.SetInt32(command, "cssIdentifier", cssIdentifier);
        db.SetInt32(command, "maxAgeInRange", maximumAgeInARange);
        db.SetDecimal(command, "ageGroupFactor", ageGroupFactor);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
      });

    entities.N06.CssIdentifier = cssIdentifier;
    entities.N06.MaximumAgeInARange = maximumAgeInARange;
    entities.N06.AgeGroupFactor = ageGroupFactor;
    entities.N06.CreatedBy = createdBy;
    entities.N06.CreatedTimestamp = createdTimestamp;
    entities.N06.LastUpdatedBy = createdBy;
    entities.N06.LastUpdatedTimestamp = createdTimestamp;
    entities.N06.CssGuidelineYr = cssGuidelineYr;
    entities.N06.Populated = true;
  }

  private void CreateAgeGroupSupportSchedule2()
  {
    var cssIdentifier = entities.ChildSupportSchedule.Identifier;
    var maximumAgeInARange = 18;
    var ageGroupFactor = 1M;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cssGuidelineYr = entities.ChildSupportSchedule.CsGuidelineYear;

    entities.N1618.Populated = false;
    Update("CreateAgeGroupSupportSchedule2",
      (db, command) =>
      {
        db.SetInt32(command, "cssIdentifier", cssIdentifier);
        db.SetInt32(command, "maxAgeInRange", maximumAgeInARange);
        db.SetDecimal(command, "ageGroupFactor", ageGroupFactor);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
      });

    entities.N1618.CssIdentifier = cssIdentifier;
    entities.N1618.MaximumAgeInARange = maximumAgeInARange;
    entities.N1618.AgeGroupFactor = ageGroupFactor;
    entities.N1618.CreatedBy = createdBy;
    entities.N1618.CreatedTimestamp = createdTimestamp;
    entities.N1618.LastUpdatedBy = createdBy;
    entities.N1618.LastUpdatedTimestamp = createdTimestamp;
    entities.N1618.CssGuidelineYr = cssGuidelineYr;
    entities.N1618.Populated = true;
  }

  private void CreateAgeGroupSupportSchedule3()
  {
    var cssIdentifier = entities.ChildSupportSchedule.Identifier;
    var maximumAgeInARange = 15;
    var ageGroupFactor = import.Import715.AgeGroupFactor;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cssGuidelineYr = entities.ChildSupportSchedule.CsGuidelineYear;

    entities.N715.Populated = false;
    Update("CreateAgeGroupSupportSchedule3",
      (db, command) =>
      {
        db.SetInt32(command, "cssIdentifier", cssIdentifier);
        db.SetInt32(command, "maxAgeInRange", maximumAgeInARange);
        db.SetDecimal(command, "ageGroupFactor", ageGroupFactor);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
      });

    entities.N715.CssIdentifier = cssIdentifier;
    entities.N715.MaximumAgeInARange = maximumAgeInARange;
    entities.N715.AgeGroupFactor = ageGroupFactor;
    entities.N715.CreatedBy = createdBy;
    entities.N715.CreatedTimestamp = createdTimestamp;
    entities.N715.LastUpdatedBy = createdBy;
    entities.N715.LastUpdatedTimestamp = createdTimestamp;
    entities.N715.CssGuidelineYr = cssGuidelineYr;
    entities.N715.Populated = true;
  }

  private void CreateChildSupportSchedule()
  {
    var identifier = export.ChildSupportSchedule.Identifier;
    var expirationDate = export.ChildSupportSchedule.ExpirationDate;
    var effectiveDate = export.ChildSupportSchedule.EffectiveDate;
    var monthlyIncomePovertyLevelInd =
      export.ChildSupportSchedule.MonthlyIncomePovertyLevelInd;
    var incomeMultiplier = export.ChildSupportSchedule.IncomeMultiplier;
    var incomeExponent = export.ChildSupportSchedule.IncomeExponent;
    var numberOfChildrenInFamily =
      export.ChildSupportSchedule.NumberOfChildrenInFamily;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var csGuidelineYear = export.ChildSupportSchedule.CsGuidelineYear;

    entities.ChildSupportSchedule.Populated = false;
    Update("CreateChildSupportSchedule",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", identifier);
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetInt32(command, "mincPovLevInd", monthlyIncomePovertyLevelInd);
        db.SetDecimal(command, "incomeMultiplier", incomeMultiplier);
        db.SetDecimal(command, "incomeExponent", incomeExponent);
        db.SetInt32(command, "noOfChInFamily", numberOfChildrenInFamily);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "csGuidelineYear", csGuidelineYear);
      });

    entities.ChildSupportSchedule.Identifier = identifier;
    entities.ChildSupportSchedule.ExpirationDate = expirationDate;
    entities.ChildSupportSchedule.EffectiveDate = effectiveDate;
    entities.ChildSupportSchedule.MonthlyIncomePovertyLevelInd =
      monthlyIncomePovertyLevelInd;
    entities.ChildSupportSchedule.IncomeMultiplier = incomeMultiplier;
    entities.ChildSupportSchedule.IncomeExponent = incomeExponent;
    entities.ChildSupportSchedule.NumberOfChildrenInFamily =
      numberOfChildrenInFamily;
    entities.ChildSupportSchedule.CreatedBy = createdBy;
    entities.ChildSupportSchedule.CreatedTimestamp = createdTimestamp;
    entities.ChildSupportSchedule.LastUpdatedBy = createdBy;
    entities.ChildSupportSchedule.LastUpdatedTimestamp = createdTimestamp;
    entities.ChildSupportSchedule.CsGuidelineYear = csGuidelineYear;
    entities.ChildSupportSchedule.Populated = true;
  }

  private bool ReadChildSupportSchedule1()
  {
    entities.ChildSupportSchedule.Populated = false;

    return Read("ReadChildSupportSchedule1",
      (db, command) =>
      {
        db.SetInt32(
          command, "noOfChInFamily",
          export.ChildSupportSchedule.NumberOfChildrenInFamily);
        db.SetNullableDate(
          command, "effectiveDate",
          export.ChildSupportSchedule.ExpirationDate.GetValueOrDefault());
        db.SetDate(
          command, "expirationDate",
          export.ChildSupportSchedule.EffectiveDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ChildSupportSchedule.Identifier = db.GetInt32(reader, 0);
        entities.ChildSupportSchedule.ExpirationDate = db.GetDate(reader, 1);
        entities.ChildSupportSchedule.EffectiveDate =
          db.GetNullableDate(reader, 2);
        entities.ChildSupportSchedule.MonthlyIncomePovertyLevelInd =
          db.GetInt32(reader, 3);
        entities.ChildSupportSchedule.IncomeMultiplier =
          db.GetDecimal(reader, 4);
        entities.ChildSupportSchedule.IncomeExponent = db.GetDecimal(reader, 5);
        entities.ChildSupportSchedule.NumberOfChildrenInFamily =
          db.GetInt32(reader, 6);
        entities.ChildSupportSchedule.CreatedBy = db.GetString(reader, 7);
        entities.ChildSupportSchedule.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ChildSupportSchedule.LastUpdatedBy = db.GetString(reader, 9);
        entities.ChildSupportSchedule.LastUpdatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.ChildSupportSchedule.CsGuidelineYear = db.GetInt32(reader, 11);
        entities.ChildSupportSchedule.Populated = true;
      });
  }

  private bool ReadChildSupportSchedule2()
  {
    entities.ChildSupportSchedule.Populated = false;

    return Read("ReadChildSupportSchedule2",
      (db, command) =>
      {
        db.SetInt32(
          command, "csGuidelineYear",
          import.ChildSupportSchedule.CsGuidelineYear);
        db.SetInt32(
          command, "noOfChInFamily",
          import.ChildSupportSchedule.NumberOfChildrenInFamily);
      },
      (db, reader) =>
      {
        entities.ChildSupportSchedule.Identifier = db.GetInt32(reader, 0);
        entities.ChildSupportSchedule.ExpirationDate = db.GetDate(reader, 1);
        entities.ChildSupportSchedule.EffectiveDate =
          db.GetNullableDate(reader, 2);
        entities.ChildSupportSchedule.MonthlyIncomePovertyLevelInd =
          db.GetInt32(reader, 3);
        entities.ChildSupportSchedule.IncomeMultiplier =
          db.GetDecimal(reader, 4);
        entities.ChildSupportSchedule.IncomeExponent = db.GetDecimal(reader, 5);
        entities.ChildSupportSchedule.NumberOfChildrenInFamily =
          db.GetInt32(reader, 6);
        entities.ChildSupportSchedule.CreatedBy = db.GetString(reader, 7);
        entities.ChildSupportSchedule.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ChildSupportSchedule.LastUpdatedBy = db.GetString(reader, 9);
        entities.ChildSupportSchedule.LastUpdatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.ChildSupportSchedule.CsGuidelineYear = db.GetInt32(reader, 11);
        entities.ChildSupportSchedule.Populated = true;
      });
  }

  private bool ReadChildSupportSchedule3()
  {
    entities.ChildSupportSchedule.Populated = false;

    return Read("ReadChildSupportSchedule3",
      null,
      (db, reader) =>
      {
        entities.ChildSupportSchedule.Identifier = db.GetInt32(reader, 0);
        entities.ChildSupportSchedule.ExpirationDate = db.GetDate(reader, 1);
        entities.ChildSupportSchedule.EffectiveDate =
          db.GetNullableDate(reader, 2);
        entities.ChildSupportSchedule.MonthlyIncomePovertyLevelInd =
          db.GetInt32(reader, 3);
        entities.ChildSupportSchedule.IncomeMultiplier =
          db.GetDecimal(reader, 4);
        entities.ChildSupportSchedule.IncomeExponent = db.GetDecimal(reader, 5);
        entities.ChildSupportSchedule.NumberOfChildrenInFamily =
          db.GetInt32(reader, 6);
        entities.ChildSupportSchedule.CreatedBy = db.GetString(reader, 7);
        entities.ChildSupportSchedule.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ChildSupportSchedule.LastUpdatedBy = db.GetString(reader, 9);
        entities.ChildSupportSchedule.LastUpdatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.ChildSupportSchedule.CsGuidelineYear = db.GetInt32(reader, 11);
        entities.ChildSupportSchedule.Populated = true;
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
    /// A value of ChildSupportSchedule.
    /// </summary>
    [JsonPropertyName("childSupportSchedule")]
    public ChildSupportSchedule ChildSupportSchedule
    {
      get => childSupportSchedule ??= new();
      set => childSupportSchedule = value;
    }

    /// <summary>
    /// A value of Import06.
    /// </summary>
    [JsonPropertyName("import06")]
    public AgeGroupSupportSchedule Import06
    {
      get => import06 ??= new();
      set => import06 = value;
    }

    /// <summary>
    /// A value of Import1618.
    /// </summary>
    [JsonPropertyName("import1618")]
    public AgeGroupSupportSchedule Import1618
    {
      get => import1618 ??= new();
      set => import1618 = value;
    }

    /// <summary>
    /// A value of Import715.
    /// </summary>
    [JsonPropertyName("import715")]
    public AgeGroupSupportSchedule Import715
    {
      get => import715 ??= new();
      set => import715 = value;
    }

    private ChildSupportSchedule childSupportSchedule;
    private AgeGroupSupportSchedule import06;
    private AgeGroupSupportSchedule import1618;
    private AgeGroupSupportSchedule import715;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ChildSupportSchedule.
    /// </summary>
    [JsonPropertyName("childSupportSchedule")]
    public ChildSupportSchedule ChildSupportSchedule
    {
      get => childSupportSchedule ??= new();
      set => childSupportSchedule = value;
    }

    private ChildSupportSchedule childSupportSchedule;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public Common Work
    {
      get => work ??= new();
      set => work = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private Common work;
    private Code maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ChildSupportSchedule.
    /// </summary>
    [JsonPropertyName("childSupportSchedule")]
    public ChildSupportSchedule ChildSupportSchedule
    {
      get => childSupportSchedule ??= new();
      set => childSupportSchedule = value;
    }

    /// <summary>
    /// A value of N06.
    /// </summary>
    [JsonPropertyName("n06")]
    public AgeGroupSupportSchedule N06
    {
      get => n06 ??= new();
      set => n06 = value;
    }

    /// <summary>
    /// A value of N1618.
    /// </summary>
    [JsonPropertyName("n1618")]
    public AgeGroupSupportSchedule N1618
    {
      get => n1618 ??= new();
      set => n1618 = value;
    }

    /// <summary>
    /// A value of N715.
    /// </summary>
    [JsonPropertyName("n715")]
    public AgeGroupSupportSchedule N715
    {
      get => n715 ??= new();
      set => n715 = value;
    }

    private ChildSupportSchedule childSupportSchedule;
    private AgeGroupSupportSchedule n06;
    private AgeGroupSupportSchedule n1618;
    private AgeGroupSupportSchedule n715;
  }
#endregion
}

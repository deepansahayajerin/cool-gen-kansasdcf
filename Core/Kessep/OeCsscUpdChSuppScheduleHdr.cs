// Program: OE_CSSC_UPD_CH_SUPP_SCHEDULE_HDR, ID: 371909265, model: 746.
// Short name: SWE00899
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CSSC_UPD_CH_SUPP_SCHEDULE_HDR.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeCsscUpdChSuppScheduleHdr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CSSC_UPD_CH_SUPP_SCHEDULE_HDR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCsscUpdChSuppScheduleHdr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCsscUpdChSuppScheduleHdr.
  /// </summary>
  public OeCsscUpdChSuppScheduleHdr(IContext context, Import import,
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

    if (Equal(import.ChildSupportSchedule.EffectiveDate, null))
    {
      export.ChildSupportSchedule.EffectiveDate = Now().Date;
    }

    if (Equal(import.ChildSupportSchedule.ExpirationDate, null))
    {
      export.ChildSupportSchedule.ExpirationDate = local.MaxDate.ExpirationDate;
    }

    if (ReadChildSupportSchedule())
    {
      try
      {
        UpdateChildSupportSchedule();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

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
    else
    {
      ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

      return;
    }

    if (ReadAgeGroupSupportSchedule1())
    {
      try
      {
        UpdateAgeGroupSupportSchedule1();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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

    if (ReadAgeGroupSupportSchedule3())
    {
      try
      {
        UpdateAgeGroupSupportSchedule3();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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

    if (ReadAgeGroupSupportSchedule2())
    {
      try
      {
        UpdateAgeGroupSupportSchedule2();
        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
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
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private bool ReadAgeGroupSupportSchedule1()
  {
    entities.N06.Populated = false;

    return Read("ReadAgeGroupSupportSchedule1",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.N06.CssIdentifier = db.GetInt32(reader, 0);
        entities.N06.MaximumAgeInARange = db.GetInt32(reader, 1);
        entities.N06.AgeGroupFactor = db.GetDecimal(reader, 2);
        entities.N06.CreatedBy = db.GetString(reader, 3);
        entities.N06.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.N06.LastUpdatedBy = db.GetString(reader, 5);
        entities.N06.LastUpdatedTimestamp = db.GetDateTime(reader, 6);
        entities.N06.CssGuidelineYr = db.GetInt32(reader, 7);
        entities.N06.Populated = true;
      });
  }

  private bool ReadAgeGroupSupportSchedule2()
  {
    entities.N1618.Populated = false;

    return Read("ReadAgeGroupSupportSchedule2",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.N1618.CssIdentifier = db.GetInt32(reader, 0);
        entities.N1618.MaximumAgeInARange = db.GetInt32(reader, 1);
        entities.N1618.AgeGroupFactor = db.GetDecimal(reader, 2);
        entities.N1618.CreatedBy = db.GetString(reader, 3);
        entities.N1618.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.N1618.LastUpdatedBy = db.GetString(reader, 5);
        entities.N1618.LastUpdatedTimestamp = db.GetDateTime(reader, 6);
        entities.N1618.CssGuidelineYr = db.GetInt32(reader, 7);
        entities.N1618.Populated = true;
      });
  }

  private bool ReadAgeGroupSupportSchedule3()
  {
    entities.N715.Populated = false;

    return Read("ReadAgeGroupSupportSchedule3",
      (db, command) =>
      {
        db.SetInt32(
          command, "cssIdentifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.ChildSupportSchedule.CsGuidelineYear);
      },
      (db, reader) =>
      {
        entities.N715.CssIdentifier = db.GetInt32(reader, 0);
        entities.N715.MaximumAgeInARange = db.GetInt32(reader, 1);
        entities.N715.AgeGroupFactor = db.GetDecimal(reader, 2);
        entities.N715.CreatedBy = db.GetString(reader, 3);
        entities.N715.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.N715.LastUpdatedBy = db.GetString(reader, 5);
        entities.N715.LastUpdatedTimestamp = db.GetDateTime(reader, 6);
        entities.N715.CssGuidelineYr = db.GetInt32(reader, 7);
        entities.N715.Populated = true;
      });
  }

  private bool ReadChildSupportSchedule()
  {
    entities.ChildSupportSchedule.Populated = false;

    return Read("ReadChildSupportSchedule",
      (db, command) =>
      {
        db.SetInt32(
          command, "csGuidelineYear",
          import.ChildSupportSchedule.CsGuidelineYear);
        db.SetInt32(
          command, "identifier", import.ChildSupportSchedule.Identifier);
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

  private void UpdateAgeGroupSupportSchedule1()
  {
    System.Diagnostics.Debug.Assert(entities.N06.Populated);

    var ageGroupFactor = import.Import06.AgeGroupFactor;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.N06.Populated = false;
    Update("UpdateAgeGroupSupportSchedule1",
      (db, command) =>
      {
        db.SetDecimal(command, "ageGroupFactor", ageGroupFactor);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(command, "cssIdentifier", entities.N06.CssIdentifier);
        db.SetInt32(command, "maxAgeInRange", entities.N06.MaximumAgeInARange);
        db.SetInt32(command, "cssGuidelineYr", entities.N06.CssGuidelineYr);
      });

    entities.N06.AgeGroupFactor = ageGroupFactor;
    entities.N06.LastUpdatedBy = lastUpdatedBy;
    entities.N06.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.N06.Populated = true;
  }

  private void UpdateAgeGroupSupportSchedule2()
  {
    System.Diagnostics.Debug.Assert(entities.N1618.Populated);

    var ageGroupFactor = import.Import1618.AgeGroupFactor;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.N1618.Populated = false;
    Update("UpdateAgeGroupSupportSchedule2",
      (db, command) =>
      {
        db.SetDecimal(command, "ageGroupFactor", ageGroupFactor);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(command, "cssIdentifier", entities.N1618.CssIdentifier);
        db.
          SetInt32(command, "maxAgeInRange", entities.N1618.MaximumAgeInARange);
          
        db.SetInt32(command, "cssGuidelineYr", entities.N1618.CssGuidelineYr);
      });

    entities.N1618.AgeGroupFactor = ageGroupFactor;
    entities.N1618.LastUpdatedBy = lastUpdatedBy;
    entities.N1618.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.N1618.Populated = true;
  }

  private void UpdateAgeGroupSupportSchedule3()
  {
    System.Diagnostics.Debug.Assert(entities.N715.Populated);

    var ageGroupFactor = import.Import715.AgeGroupFactor;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.N715.Populated = false;
    Update("UpdateAgeGroupSupportSchedule3",
      (db, command) =>
      {
        db.SetDecimal(command, "ageGroupFactor", ageGroupFactor);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(command, "cssIdentifier", entities.N715.CssIdentifier);
        db.SetInt32(command, "maxAgeInRange", entities.N715.MaximumAgeInARange);
        db.SetInt32(command, "cssGuidelineYr", entities.N715.CssGuidelineYr);
      });

    entities.N715.AgeGroupFactor = ageGroupFactor;
    entities.N715.LastUpdatedBy = lastUpdatedBy;
    entities.N715.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.N715.Populated = true;
  }

  private void UpdateChildSupportSchedule()
  {
    var expirationDate = export.ChildSupportSchedule.ExpirationDate;
    var effectiveDate = export.ChildSupportSchedule.EffectiveDate;
    var monthlyIncomePovertyLevelInd =
      export.ChildSupportSchedule.MonthlyIncomePovertyLevelInd;
    var incomeMultiplier = export.ChildSupportSchedule.IncomeMultiplier;
    var incomeExponent = export.ChildSupportSchedule.IncomeExponent;
    var numberOfChildrenInFamily =
      export.ChildSupportSchedule.NumberOfChildrenInFamily;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ChildSupportSchedule.Populated = false;
    Update("UpdateChildSupportSchedule",
      (db, command) =>
      {
        db.SetDate(command, "expirationDate", expirationDate);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetInt32(command, "mincPovLevInd", monthlyIncomePovertyLevelInd);
        db.SetDecimal(command, "incomeMultiplier", incomeMultiplier);
        db.SetDecimal(command, "incomeExponent", incomeExponent);
        db.SetInt32(command, "noOfChInFamily", numberOfChildrenInFamily);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "identifier", entities.ChildSupportSchedule.Identifier);
        db.SetInt32(
          command, "csGuidelineYear",
          entities.ChildSupportSchedule.CsGuidelineYear);
      });

    entities.ChildSupportSchedule.ExpirationDate = expirationDate;
    entities.ChildSupportSchedule.EffectiveDate = effectiveDate;
    entities.ChildSupportSchedule.MonthlyIncomePovertyLevelInd =
      monthlyIncomePovertyLevelInd;
    entities.ChildSupportSchedule.IncomeMultiplier = incomeMultiplier;
    entities.ChildSupportSchedule.IncomeExponent = incomeExponent;
    entities.ChildSupportSchedule.NumberOfChildrenInFamily =
      numberOfChildrenInFamily;
    entities.ChildSupportSchedule.LastUpdatedBy = lastUpdatedBy;
    entities.ChildSupportSchedule.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ChildSupportSchedule.Populated = true;
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
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

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

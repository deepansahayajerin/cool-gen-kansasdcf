// Program: OE_CSSC_ADD_CH_SUPP_SCHEDULE_DET, ID: 371909263, model: 746.
// Short name: SWE00894
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CSSC_ADD_CH_SUPP_SCHEDULE_DET.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeCsscAddChSuppScheduleDet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CSSC_ADD_CH_SUPP_SCHEDULE_DET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCsscAddChSuppScheduleDet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCsscAddChSuppScheduleDet.
  /// </summary>
  public OeCsscAddChSuppScheduleDet(IContext context, Import import,
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
    ExitState = "ACO_NN0000_ALL_OK";
    UseOeCabSetMnemonics();
    export.ChildSupportSchedule.Assign(import.ChildSupportSchedule);

    if (Equal(export.ChildSupportSchedule.ExpirationDate,
      local.MaxDate.ExpirationDate))
    {
      export.ChildSupportSchedule.ExpirationDate = null;
    }

    if (!ReadChildSupportSchedule())
    {
      ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

      return;
    }

    if (ReadAgeGroupSupportSchedule1())
    {
      try
      {
        CreateCsGrossMonthlyIncSched1();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CS_GROSS_MONTHLY_INCOME_AE";

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
        CreateCsGrossMonthlyIncSched3();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CS_GROSS_MONTHLY_INCOME_AE";

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
        CreateCsGrossMonthlyIncSched2();
        ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
      }
      catch(Exception e)
      {
        switch(GetErrorCode(e))
        {
          case ErrorCode.AlreadyExists:
            ExitState = "CS_GROSS_MONTHLY_INCOME_AE";

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

  private void CreateCsGrossMonthlyIncSched1()
  {
    System.Diagnostics.Debug.
      Assert(entities.N06geGroupSupportSchedule.Populated);

    var cssIdentifier = entities.N06geGroupSupportSchedule.CssIdentifier;
    var agsMaxAgeRange = entities.N06geGroupSupportSchedule.MaximumAgeInARange;
    var combinedGrossMnthlyIncomeAmt =
      import.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt;
    var perChildSupportAmount =
      import.Import06CsGrossMonthlyIncSched.PerChildSupportAmount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cssGuidelineYr = entities.N06geGroupSupportSchedule.CssGuidelineYr;

    entities.N06sGrossMonthlyIncSched.Populated = false;
    Update("CreateCsGrossMonthlyIncSched1",
      (db, command) =>
      {
        db.SetInt32(command, "cssIdentifier", cssIdentifier);
        db.SetInt32(command, "agsMaxAgeRange", agsMaxAgeRange);
        db.SetInt32(command, "combGrMthInc", combinedGrossMnthlyIncomeAmt);
        db.SetInt32(command, "perChildSuppAmt", perChildSupportAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
      });

    entities.N06sGrossMonthlyIncSched.CssIdentifier = cssIdentifier;
    entities.N06sGrossMonthlyIncSched.AgsMaxAgeRange = agsMaxAgeRange;
    entities.N06sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
      combinedGrossMnthlyIncomeAmt;
    entities.N06sGrossMonthlyIncSched.PerChildSupportAmount =
      perChildSupportAmount;
    entities.N06sGrossMonthlyIncSched.CreatedBy = createdBy;
    entities.N06sGrossMonthlyIncSched.CreatedTimestamp = createdTimestamp;
    entities.N06sGrossMonthlyIncSched.LastUpdatedBy = createdBy;
    entities.N06sGrossMonthlyIncSched.LastUpdatedTimestamp = createdTimestamp;
    entities.N06sGrossMonthlyIncSched.CssGuidelineYr = cssGuidelineYr;
    entities.N06sGrossMonthlyIncSched.Populated = true;
  }

  private void CreateCsGrossMonthlyIncSched2()
  {
    System.Diagnostics.Debug.Assert(
      entities.N1618geGroupSupportSchedule.Populated);

    var cssIdentifier = entities.N1618geGroupSupportSchedule.CssIdentifier;
    var agsMaxAgeRange =
      entities.N1618geGroupSupportSchedule.MaximumAgeInARange;
    var combinedGrossMnthlyIncomeAmt =
      import.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt;
    var perChildSupportAmount =
      import.Import1618CsGrossMonthlyIncSched.PerChildSupportAmount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cssGuidelineYr = entities.N1618geGroupSupportSchedule.CssGuidelineYr;

    entities.N1618sGrossMonthlyIncSched.Populated = false;
    Update("CreateCsGrossMonthlyIncSched2",
      (db, command) =>
      {
        db.SetInt32(command, "cssIdentifier", cssIdentifier);
        db.SetInt32(command, "agsMaxAgeRange", agsMaxAgeRange);
        db.SetInt32(command, "combGrMthInc", combinedGrossMnthlyIncomeAmt);
        db.SetInt32(command, "perChildSuppAmt", perChildSupportAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
      });

    entities.N1618sGrossMonthlyIncSched.CssIdentifier = cssIdentifier;
    entities.N1618sGrossMonthlyIncSched.AgsMaxAgeRange = agsMaxAgeRange;
    entities.N1618sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
      combinedGrossMnthlyIncomeAmt;
    entities.N1618sGrossMonthlyIncSched.PerChildSupportAmount =
      perChildSupportAmount;
    entities.N1618sGrossMonthlyIncSched.CreatedBy = createdBy;
    entities.N1618sGrossMonthlyIncSched.CreatedTimestamp = createdTimestamp;
    entities.N1618sGrossMonthlyIncSched.LastUpdatedBy = createdBy;
    entities.N1618sGrossMonthlyIncSched.LastUpdatedTimestamp = createdTimestamp;
    entities.N1618sGrossMonthlyIncSched.CssGuidelineYr = cssGuidelineYr;
    entities.N1618sGrossMonthlyIncSched.Populated = true;
  }

  private void CreateCsGrossMonthlyIncSched3()
  {
    System.Diagnostics.Debug.Assert(
      entities.N715geGroupSupportSchedule.Populated);

    var cssIdentifier = entities.N715geGroupSupportSchedule.CssIdentifier;
    var agsMaxAgeRange = entities.N715geGroupSupportSchedule.MaximumAgeInARange;
    var combinedGrossMnthlyIncomeAmt =
      import.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt;
    var perChildSupportAmount =
      import.Import715CsGrossMonthlyIncSched.PerChildSupportAmount;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var cssGuidelineYr = entities.N715geGroupSupportSchedule.CssGuidelineYr;

    entities.N715sGrossMonthlyIncSched.Populated = false;
    Update("CreateCsGrossMonthlyIncSched3",
      (db, command) =>
      {
        db.SetInt32(command, "cssIdentifier", cssIdentifier);
        db.SetInt32(command, "agsMaxAgeRange", agsMaxAgeRange);
        db.SetInt32(command, "combGrMthInc", combinedGrossMnthlyIncomeAmt);
        db.SetInt32(command, "perChildSuppAmt", perChildSupportAmount);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "lastUpdatedBy", createdBy);
        db.SetDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetInt32(command, "cssGuidelineYr", cssGuidelineYr);
      });

    entities.N715sGrossMonthlyIncSched.CssIdentifier = cssIdentifier;
    entities.N715sGrossMonthlyIncSched.AgsMaxAgeRange = agsMaxAgeRange;
    entities.N715sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
      combinedGrossMnthlyIncomeAmt;
    entities.N715sGrossMonthlyIncSched.PerChildSupportAmount =
      perChildSupportAmount;
    entities.N715sGrossMonthlyIncSched.CreatedBy = createdBy;
    entities.N715sGrossMonthlyIncSched.CreatedTimestamp = createdTimestamp;
    entities.N715sGrossMonthlyIncSched.LastUpdatedBy = createdBy;
    entities.N715sGrossMonthlyIncSched.LastUpdatedTimestamp = createdTimestamp;
    entities.N715sGrossMonthlyIncSched.CssGuidelineYr = cssGuidelineYr;
    entities.N715sGrossMonthlyIncSched.Populated = true;
  }

  private bool ReadAgeGroupSupportSchedule1()
  {
    entities.N06geGroupSupportSchedule.Populated = false;

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
        entities.N06geGroupSupportSchedule.CssIdentifier =
          db.GetInt32(reader, 0);
        entities.N06geGroupSupportSchedule.MaximumAgeInARange =
          db.GetInt32(reader, 1);
        entities.N06geGroupSupportSchedule.AgeGroupFactor =
          db.GetDecimal(reader, 2);
        entities.N06geGroupSupportSchedule.CreatedBy = db.GetString(reader, 3);
        entities.N06geGroupSupportSchedule.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.N06geGroupSupportSchedule.LastUpdatedBy =
          db.GetString(reader, 5);
        entities.N06geGroupSupportSchedule.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.N06geGroupSupportSchedule.CssGuidelineYr =
          db.GetInt32(reader, 7);
        entities.N06geGroupSupportSchedule.Populated = true;
      });
  }

  private bool ReadAgeGroupSupportSchedule2()
  {
    entities.N1618geGroupSupportSchedule.Populated = false;

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
        entities.N1618geGroupSupportSchedule.CssIdentifier =
          db.GetInt32(reader, 0);
        entities.N1618geGroupSupportSchedule.MaximumAgeInARange =
          db.GetInt32(reader, 1);
        entities.N1618geGroupSupportSchedule.AgeGroupFactor =
          db.GetDecimal(reader, 2);
        entities.N1618geGroupSupportSchedule.CreatedBy =
          db.GetString(reader, 3);
        entities.N1618geGroupSupportSchedule.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.N1618geGroupSupportSchedule.LastUpdatedBy =
          db.GetString(reader, 5);
        entities.N1618geGroupSupportSchedule.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.N1618geGroupSupportSchedule.CssGuidelineYr =
          db.GetInt32(reader, 7);
        entities.N1618geGroupSupportSchedule.Populated = true;
      });
  }

  private bool ReadAgeGroupSupportSchedule3()
  {
    entities.N715geGroupSupportSchedule.Populated = false;

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
        entities.N715geGroupSupportSchedule.CssIdentifier =
          db.GetInt32(reader, 0);
        entities.N715geGroupSupportSchedule.MaximumAgeInARange =
          db.GetInt32(reader, 1);
        entities.N715geGroupSupportSchedule.AgeGroupFactor =
          db.GetDecimal(reader, 2);
        entities.N715geGroupSupportSchedule.CreatedBy = db.GetString(reader, 3);
        entities.N715geGroupSupportSchedule.CreatedTimestamp =
          db.GetDateTime(reader, 4);
        entities.N715geGroupSupportSchedule.LastUpdatedBy =
          db.GetString(reader, 5);
        entities.N715geGroupSupportSchedule.LastUpdatedTimestamp =
          db.GetDateTime(reader, 6);
        entities.N715geGroupSupportSchedule.CssGuidelineYr =
          db.GetInt32(reader, 7);
        entities.N715geGroupSupportSchedule.Populated = true;
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
    /// A value of CsGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("csGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched CsGrossMonthlyIncSched
    {
      get => csGrossMonthlyIncSched ??= new();
      set => csGrossMonthlyIncSched = value;
    }

    /// <summary>
    /// A value of Import1618CsGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("import1618CsGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched Import1618CsGrossMonthlyIncSched
    {
      get => import1618CsGrossMonthlyIncSched ??= new();
      set => import1618CsGrossMonthlyIncSched = value;
    }

    /// <summary>
    /// A value of Import715CsGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("import715CsGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched Import715CsGrossMonthlyIncSched
    {
      get => import715CsGrossMonthlyIncSched ??= new();
      set => import715CsGrossMonthlyIncSched = value;
    }

    /// <summary>
    /// A value of Import06CsGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("import06CsGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched Import06CsGrossMonthlyIncSched
    {
      get => import06CsGrossMonthlyIncSched ??= new();
      set => import06CsGrossMonthlyIncSched = value;
    }

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
    /// A value of Import06AgeGroupSupportSchedule.
    /// </summary>
    [JsonPropertyName("import06AgeGroupSupportSchedule")]
    public AgeGroupSupportSchedule Import06AgeGroupSupportSchedule
    {
      get => import06AgeGroupSupportSchedule ??= new();
      set => import06AgeGroupSupportSchedule = value;
    }

    /// <summary>
    /// A value of Import1618AgeGroupSupportSchedule.
    /// </summary>
    [JsonPropertyName("import1618AgeGroupSupportSchedule")]
    public AgeGroupSupportSchedule Import1618AgeGroupSupportSchedule
    {
      get => import1618AgeGroupSupportSchedule ??= new();
      set => import1618AgeGroupSupportSchedule = value;
    }

    /// <summary>
    /// A value of Import715AgeGroupSupportSchedule.
    /// </summary>
    [JsonPropertyName("import715AgeGroupSupportSchedule")]
    public AgeGroupSupportSchedule Import715AgeGroupSupportSchedule
    {
      get => import715AgeGroupSupportSchedule ??= new();
      set => import715AgeGroupSupportSchedule = value;
    }

    private CsGrossMonthlyIncSched csGrossMonthlyIncSched;
    private CsGrossMonthlyIncSched import1618CsGrossMonthlyIncSched;
    private CsGrossMonthlyIncSched import715CsGrossMonthlyIncSched;
    private CsGrossMonthlyIncSched import06CsGrossMonthlyIncSched;
    private ChildSupportSchedule childSupportSchedule;
    private AgeGroupSupportSchedule import06AgeGroupSupportSchedule;
    private AgeGroupSupportSchedule import1618AgeGroupSupportSchedule;
    private AgeGroupSupportSchedule import715AgeGroupSupportSchedule;
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
    /// A value of N1618sGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("n1618sGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched N1618sGrossMonthlyIncSched
    {
      get => n1618sGrossMonthlyIncSched ??= new();
      set => n1618sGrossMonthlyIncSched = value;
    }

    /// <summary>
    /// A value of N715sGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("n715sGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched N715sGrossMonthlyIncSched
    {
      get => n715sGrossMonthlyIncSched ??= new();
      set => n715sGrossMonthlyIncSched = value;
    }

    /// <summary>
    /// A value of N06sGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("n06sGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched N06sGrossMonthlyIncSched
    {
      get => n06sGrossMonthlyIncSched ??= new();
      set => n06sGrossMonthlyIncSched = value;
    }

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
    /// A value of N06geGroupSupportSchedule.
    /// </summary>
    [JsonPropertyName("n06geGroupSupportSchedule")]
    public AgeGroupSupportSchedule N06geGroupSupportSchedule
    {
      get => n06geGroupSupportSchedule ??= new();
      set => n06geGroupSupportSchedule = value;
    }

    /// <summary>
    /// A value of N1618geGroupSupportSchedule.
    /// </summary>
    [JsonPropertyName("n1618geGroupSupportSchedule")]
    public AgeGroupSupportSchedule N1618geGroupSupportSchedule
    {
      get => n1618geGroupSupportSchedule ??= new();
      set => n1618geGroupSupportSchedule = value;
    }

    /// <summary>
    /// A value of N715geGroupSupportSchedule.
    /// </summary>
    [JsonPropertyName("n715geGroupSupportSchedule")]
    public AgeGroupSupportSchedule N715geGroupSupportSchedule
    {
      get => n715geGroupSupportSchedule ??= new();
      set => n715geGroupSupportSchedule = value;
    }

    private CsGrossMonthlyIncSched n1618sGrossMonthlyIncSched;
    private CsGrossMonthlyIncSched n715sGrossMonthlyIncSched;
    private CsGrossMonthlyIncSched n06sGrossMonthlyIncSched;
    private ChildSupportSchedule childSupportSchedule;
    private AgeGroupSupportSchedule n06geGroupSupportSchedule;
    private AgeGroupSupportSchedule n1618geGroupSupportSchedule;
    private AgeGroupSupportSchedule n715geGroupSupportSchedule;
  }
#endregion
}

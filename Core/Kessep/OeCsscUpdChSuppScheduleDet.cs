// Program: OE_CSSC_UPD_CH_SUPP_SCHEDULE_DET, ID: 371909261, model: 746.
// Short name: SWE00898
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CSSC_UPD_CH_SUPP_SCHEDULE_DET.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeCsscUpdChSuppScheduleDet: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CSSC_UPD_CH_SUPP_SCHEDULE_DET program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCsscUpdChSuppScheduleDet(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCsscUpdChSuppScheduleDet.
  /// </summary>
  public OeCsscUpdChSuppScheduleDet(IContext context, Import import,
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
    export.ChildSupportSchedule.Assign(import.ChildSupportSchedule);

    if (!ReadChildSupportSchedule())
    {
      ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

      return;
    }

    if (ReadAgeGroupSupportSchedule1())
    {
      if (ReadCsGrossMonthlyIncSched1())
      {
        try
        {
          UpdateCsGrossMonthlyIncSched1();
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

    if (ReadAgeGroupSupportSchedule3())
    {
      if (ReadCsGrossMonthlyIncSched3())
      {
        try
        {
          UpdateCsGrossMonthlyIncSched3();
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

    if (ReadAgeGroupSupportSchedule2())
    {
      if (ReadCsGrossMonthlyIncSched2())
      {
        try
        {
          UpdateCsGrossMonthlyIncSched2();
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

  private bool ReadCsGrossMonthlyIncSched1()
  {
    System.Diagnostics.Debug.
      Assert(entities.N06geGroupSupportSchedule.Populated);
    entities.N06sGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched1",
      (db, command) =>
      {
        db.SetInt32(
          command, "agsMaxAgeRange",
          entities.N06geGroupSupportSchedule.MaximumAgeInARange);
        db.SetInt32(
          command, "cssIdentifier",
          entities.N06geGroupSupportSchedule.CssIdentifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.N06geGroupSupportSchedule.CssGuidelineYr);
        db.SetInt32(
          command, "combGrMthInc",
          import.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt);
      },
      (db, reader) =>
      {
        entities.N06sGrossMonthlyIncSched.CssIdentifier =
          db.GetInt32(reader, 0);
        entities.N06sGrossMonthlyIncSched.AgsMaxAgeRange =
          db.GetInt32(reader, 1);
        entities.N06sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
          db.GetInt32(reader, 2);
        entities.N06sGrossMonthlyIncSched.PerChildSupportAmount =
          db.GetInt32(reader, 3);
        entities.N06sGrossMonthlyIncSched.CreatedBy = db.GetString(reader, 4);
        entities.N06sGrossMonthlyIncSched.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.N06sGrossMonthlyIncSched.LastUpdatedBy =
          db.GetString(reader, 6);
        entities.N06sGrossMonthlyIncSched.LastUpdatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.N06sGrossMonthlyIncSched.CssGuidelineYr =
          db.GetInt32(reader, 8);
        entities.N06sGrossMonthlyIncSched.Populated = true;
      });
  }

  private bool ReadCsGrossMonthlyIncSched2()
  {
    System.Diagnostics.Debug.Assert(
      entities.N1618geGroupSupportSchedule.Populated);
    entities.N1618sGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched2",
      (db, command) =>
      {
        db.SetInt32(
          command, "agsMaxAgeRange",
          entities.N1618geGroupSupportSchedule.MaximumAgeInARange);
        db.SetInt32(
          command, "cssIdentifier",
          entities.N1618geGroupSupportSchedule.CssIdentifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.N1618geGroupSupportSchedule.CssGuidelineYr);
        db.SetInt32(
          command, "combGrMthInc",
          import.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt);
      },
      (db, reader) =>
      {
        entities.N1618sGrossMonthlyIncSched.CssIdentifier =
          db.GetInt32(reader, 0);
        entities.N1618sGrossMonthlyIncSched.AgsMaxAgeRange =
          db.GetInt32(reader, 1);
        entities.N1618sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
          db.GetInt32(reader, 2);
        entities.N1618sGrossMonthlyIncSched.PerChildSupportAmount =
          db.GetInt32(reader, 3);
        entities.N1618sGrossMonthlyIncSched.CreatedBy = db.GetString(reader, 4);
        entities.N1618sGrossMonthlyIncSched.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.N1618sGrossMonthlyIncSched.LastUpdatedBy =
          db.GetString(reader, 6);
        entities.N1618sGrossMonthlyIncSched.LastUpdatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.N1618sGrossMonthlyIncSched.CssGuidelineYr =
          db.GetInt32(reader, 8);
        entities.N1618sGrossMonthlyIncSched.Populated = true;
      });
  }

  private bool ReadCsGrossMonthlyIncSched3()
  {
    System.Diagnostics.Debug.Assert(
      entities.N715geGroupSupportSchedule.Populated);
    entities.N715sGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched3",
      (db, command) =>
      {
        db.SetInt32(
          command, "agsMaxAgeRange",
          entities.N715geGroupSupportSchedule.MaximumAgeInARange);
        db.SetInt32(
          command, "cssIdentifier",
          entities.N715geGroupSupportSchedule.CssIdentifier);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.N715geGroupSupportSchedule.CssGuidelineYr);
        db.SetInt32(
          command, "combGrMthInc",
          import.CsGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt);
      },
      (db, reader) =>
      {
        entities.N715sGrossMonthlyIncSched.CssIdentifier =
          db.GetInt32(reader, 0);
        entities.N715sGrossMonthlyIncSched.AgsMaxAgeRange =
          db.GetInt32(reader, 1);
        entities.N715sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt =
          db.GetInt32(reader, 2);
        entities.N715sGrossMonthlyIncSched.PerChildSupportAmount =
          db.GetInt32(reader, 3);
        entities.N715sGrossMonthlyIncSched.CreatedBy = db.GetString(reader, 4);
        entities.N715sGrossMonthlyIncSched.CreatedTimestamp =
          db.GetDateTime(reader, 5);
        entities.N715sGrossMonthlyIncSched.LastUpdatedBy =
          db.GetString(reader, 6);
        entities.N715sGrossMonthlyIncSched.LastUpdatedTimestamp =
          db.GetDateTime(reader, 7);
        entities.N715sGrossMonthlyIncSched.CssGuidelineYr =
          db.GetInt32(reader, 8);
        entities.N715sGrossMonthlyIncSched.Populated = true;
      });
  }

  private void UpdateCsGrossMonthlyIncSched1()
  {
    System.Diagnostics.Debug.
      Assert(entities.N06sGrossMonthlyIncSched.Populated);

    var perChildSupportAmount =
      import.Import06CsGrossMonthlyIncSched.PerChildSupportAmount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.N06sGrossMonthlyIncSched.Populated = false;
    Update("UpdateCsGrossMonthlyIncSched1",
      (db, command) =>
      {
        db.SetInt32(command, "perChildSuppAmt", perChildSupportAmount);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "cssIdentifier",
          entities.N06sGrossMonthlyIncSched.CssIdentifier);
        db.SetInt32(
          command, "agsMaxAgeRange",
          entities.N06sGrossMonthlyIncSched.AgsMaxAgeRange);
        db.SetInt32(
          command, "combGrMthInc",
          entities.N06sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.N06sGrossMonthlyIncSched.CssGuidelineYr);
      });

    entities.N06sGrossMonthlyIncSched.PerChildSupportAmount =
      perChildSupportAmount;
    entities.N06sGrossMonthlyIncSched.LastUpdatedBy = lastUpdatedBy;
    entities.N06sGrossMonthlyIncSched.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.N06sGrossMonthlyIncSched.Populated = true;
  }

  private void UpdateCsGrossMonthlyIncSched2()
  {
    System.Diagnostics.Debug.Assert(
      entities.N1618sGrossMonthlyIncSched.Populated);

    var perChildSupportAmount =
      import.Import1618CsGrossMonthlyIncSched.PerChildSupportAmount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.N1618sGrossMonthlyIncSched.Populated = false;
    Update("UpdateCsGrossMonthlyIncSched2",
      (db, command) =>
      {
        db.SetInt32(command, "perChildSuppAmt", perChildSupportAmount);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "cssIdentifier",
          entities.N1618sGrossMonthlyIncSched.CssIdentifier);
        db.SetInt32(
          command, "agsMaxAgeRange",
          entities.N1618sGrossMonthlyIncSched.AgsMaxAgeRange);
        db.SetInt32(
          command, "combGrMthInc",
          entities.N1618sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.N1618sGrossMonthlyIncSched.CssGuidelineYr);
      });

    entities.N1618sGrossMonthlyIncSched.PerChildSupportAmount =
      perChildSupportAmount;
    entities.N1618sGrossMonthlyIncSched.LastUpdatedBy = lastUpdatedBy;
    entities.N1618sGrossMonthlyIncSched.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.N1618sGrossMonthlyIncSched.Populated = true;
  }

  private void UpdateCsGrossMonthlyIncSched3()
  {
    System.Diagnostics.Debug.
      Assert(entities.N715sGrossMonthlyIncSched.Populated);

    var perChildSupportAmount =
      import.Import715CsGrossMonthlyIncSched.PerChildSupportAmount;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.N715sGrossMonthlyIncSched.Populated = false;
    Update("UpdateCsGrossMonthlyIncSched3",
      (db, command) =>
      {
        db.SetInt32(command, "perChildSuppAmt", perChildSupportAmount);
        db.SetString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
        db.SetInt32(
          command, "cssIdentifier",
          entities.N715sGrossMonthlyIncSched.CssIdentifier);
        db.SetInt32(
          command, "agsMaxAgeRange",
          entities.N715sGrossMonthlyIncSched.AgsMaxAgeRange);
        db.SetInt32(
          command, "combGrMthInc",
          entities.N715sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt);
        db.SetInt32(
          command, "cssGuidelineYr",
          entities.N715sGrossMonthlyIncSched.CssGuidelineYr);
      });

    entities.N715sGrossMonthlyIncSched.PerChildSupportAmount =
      perChildSupportAmount;
    entities.N715sGrossMonthlyIncSched.LastUpdatedBy = lastUpdatedBy;
    entities.N715sGrossMonthlyIncSched.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.N715sGrossMonthlyIncSched.Populated = true;
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

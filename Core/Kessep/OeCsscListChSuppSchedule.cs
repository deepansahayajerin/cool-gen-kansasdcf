// Program: OE_CSSC_LIST_CH_SUPP_SCHEDULE, ID: 371909259, model: 746.
// Short name: SWE00897
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CSSC_LIST_CH_SUPP_SCHEDULE.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeCsscListChSuppSchedule: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CSSC_LIST_CH_SUPP_SCHEDULE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCsscListChSuppSchedule(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCsscListChSuppSchedule.
  /// </summary>
  public OeCsscListChSuppSchedule(IContext context, Import import, Export export)
    :
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ************************************************************
    // MAINTENANCE LOG
    // AUTHOR		DATE		CHG REQ #	DESCRIPTION
    // Ty Hill-MTW    04/29/97                         Change Current_date
    // GVandy	       12/14/15		CQ50299		Add CS_Guideline_Year
    // ************************************************************
    local.Current.Date = Now().Date;

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    MoveChildSupportSchedule(import.ChildSupportSchedule,
      export.ChildSupportSchedule);
    UseOeCabSetMnemonics();

    if (import.ChildSupportSchedule.CsGuidelineYear == 0)
    {
      // -- Find the currently active guideline year.
      if (ReadChildSupportSchedule1())
      {
        export.ChildSupportSchedule.Assign(entities.ChildSupportSchedule);

        if (Equal(entities.ChildSupportSchedule.ExpirationDate,
          local.MaxDate.ExpirationDate))
        {
          export.ChildSupportSchedule.ExpirationDate = null;
        }
      }
      else
      {
        ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

        return;
      }
    }
    else
    {
      // -- Read the guideline year requested.
      if (ReadChildSupportSchedule2())
      {
        export.ChildSupportSchedule.Assign(entities.ChildSupportSchedule);

        if (Equal(entities.ChildSupportSchedule.ExpirationDate,
          local.MaxDate.ExpirationDate))
        {
          export.ChildSupportSchedule.ExpirationDate = null;
        }
      }
      else
      {
        ExitState = "CHILD_SUPPORT_SCHEDULE_NF";

        return;
      }
    }

    if (ReadAgeGroupSupportSchedule1())
    {
      export.Export06.Assign(entities.N06geGroupSupportSchedule);
    }

    if (ReadAgeGroupSupportSchedule3())
    {
      export.Export715.Assign(entities.N715geGroupSupportSchedule);
    }

    if (ReadAgeGroupSupportSchedule2())
    {
      export.Export1618.Assign(entities.N1618geGroupSupportSchedule);
    }

    export.Export1.Index = 0;
    export.Export1.Clear();

    foreach(var item in ReadCsGrossMonthlyIncSched3())
    {
      export.Export1.Update.Export06.PerChildSupportAmount =
        entities.N06sGrossMonthlyIncSched.PerChildSupportAmount;
      export.Export1.Update.Detail.CombinedGrossMnthlyIncomeAmt =
        entities.N06sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt;
      export.Export1.Update.PrevH.CombinedGrossMnthlyIncomeAmt =
        entities.N06sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt;

      if (ReadCsGrossMonthlyIncSched2())
      {
        export.Export1.Update.Export715.PerChildSupportAmount =
          entities.N715sGrossMonthlyIncSched.PerChildSupportAmount;
      }

      if (ReadCsGrossMonthlyIncSched1())
      {
        export.Export1.Update.Export1618.PerChildSupportAmount =
          entities.N1618sGrossMonthlyIncSched.PerChildSupportAmount;
      }

      export.Export1.Next();
    }

    if (export.Export1.IsEmpty)
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
    }
    else
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
  }

  private static void MoveChildSupportSchedule(ChildSupportSchedule source,
    ChildSupportSchedule target)
  {
    target.ExpirationDate = source.ExpirationDate;
    target.EffectiveDate = source.EffectiveDate;
    target.MonthlyIncomePovertyLevelInd = source.MonthlyIncomePovertyLevelInd;
    target.IncomeMultiplier = source.IncomeMultiplier;
    target.IncomeExponent = source.IncomeExponent;
    target.NumberOfChildrenInFamily = source.NumberOfChildrenInFamily;
    target.CsGuidelineYear = source.CsGuidelineYear;
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

  private bool ReadChildSupportSchedule1()
  {
    entities.ChildSupportSchedule.Populated = false;

    return Read("ReadChildSupportSchedule1",
      (db, command) =>
      {
        db.SetInt32(
          command, "noOfChInFamily",
          import.ChildSupportSchedule.NumberOfChildrenInFamily);
        db.SetDate(
          command, "expirationDate", local.Current.Date.GetValueOrDefault());
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
          command, "noOfChInFamily",
          import.ChildSupportSchedule.NumberOfChildrenInFamily);
        db.SetInt32(
          command, "csGuidelineYear",
          import.ChildSupportSchedule.CsGuidelineYear);
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
    System.Diagnostics.Debug.Assert(
      entities.N1618geGroupSupportSchedule.Populated);
    entities.N1618sGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched1",
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
          entities.N06sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt);
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

  private bool ReadCsGrossMonthlyIncSched2()
  {
    System.Diagnostics.Debug.Assert(
      entities.N715geGroupSupportSchedule.Populated);
    entities.N715sGrossMonthlyIncSched.Populated = false;

    return Read("ReadCsGrossMonthlyIncSched2",
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
          entities.N06sGrossMonthlyIncSched.CombinedGrossMnthlyIncomeAmt);
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

  private IEnumerable<bool> ReadCsGrossMonthlyIncSched3()
  {
    System.Diagnostics.Debug.
      Assert(entities.N06geGroupSupportSchedule.Populated);

    return ReadEach("ReadCsGrossMonthlyIncSched3",
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
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

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

        return true;
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

    private ChildSupportSchedule childSupportSchedule;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of PrevH.
      /// </summary>
      [JsonPropertyName("prevH")]
      public CsGrossMonthlyIncSched PrevH
      {
        get => prevH ??= new();
        set => prevH = value;
      }

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
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public CsGrossMonthlyIncSched Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of Export06.
      /// </summary>
      [JsonPropertyName("export06")]
      public CsGrossMonthlyIncSched Export06
      {
        get => export06 ??= new();
        set => export06 = value;
      }

      /// <summary>
      /// A value of Export715.
      /// </summary>
      [JsonPropertyName("export715")]
      public CsGrossMonthlyIncSched Export715
      {
        get => export715 ??= new();
        set => export715 = value;
      }

      /// <summary>
      /// A value of Export1618.
      /// </summary>
      [JsonPropertyName("export1618")]
      public CsGrossMonthlyIncSched Export1618
      {
        get => export1618 ??= new();
        set => export1618 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private CsGrossMonthlyIncSched prevH;
      private Common work;
      private CsGrossMonthlyIncSched detail;
      private CsGrossMonthlyIncSched export06;
      private CsGrossMonthlyIncSched export715;
      private CsGrossMonthlyIncSched export1618;
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
    /// A value of Export06.
    /// </summary>
    [JsonPropertyName("export06")]
    public AgeGroupSupportSchedule Export06
    {
      get => export06 ??= new();
      set => export06 = value;
    }

    /// <summary>
    /// A value of Export1618.
    /// </summary>
    [JsonPropertyName("export1618")]
    public AgeGroupSupportSchedule Export1618
    {
      get => export1618 ??= new();
      set => export1618 = value;
    }

    /// <summary>
    /// A value of Export715.
    /// </summary>
    [JsonPropertyName("export715")]
    public AgeGroupSupportSchedule Export715
    {
      get => export715 ??= new();
      set => export715 = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
    }

    private ChildSupportSchedule childSupportSchedule;
    private AgeGroupSupportSchedule export06;
    private AgeGroupSupportSchedule export1618;
    private AgeGroupSupportSchedule export715;
    private Array<ExportGroup> export1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
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

    private DateWorkArea current;
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
    /// A value of N715sGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("n715sGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched N715sGrossMonthlyIncSched
    {
      get => n715sGrossMonthlyIncSched ??= new();
      set => n715sGrossMonthlyIncSched = value;
    }

    /// <summary>
    /// A value of N1618sGrossMonthlyIncSched.
    /// </summary>
    [JsonPropertyName("n1618sGrossMonthlyIncSched")]
    public CsGrossMonthlyIncSched N1618sGrossMonthlyIncSched
    {
      get => n1618sGrossMonthlyIncSched ??= new();
      set => n1618sGrossMonthlyIncSched = value;
    }

    private ChildSupportSchedule childSupportSchedule;
    private AgeGroupSupportSchedule n06geGroupSupportSchedule;
    private AgeGroupSupportSchedule n1618geGroupSupportSchedule;
    private AgeGroupSupportSchedule n715geGroupSupportSchedule;
    private CsGrossMonthlyIncSched n06sGrossMonthlyIncSched;
    private CsGrossMonthlyIncSched n715sGrossMonthlyIncSched;
    private CsGrossMonthlyIncSched n1618sGrossMonthlyIncSched;
  }
#endregion
}

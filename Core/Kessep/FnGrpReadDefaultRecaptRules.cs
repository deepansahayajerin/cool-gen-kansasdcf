// Program: FN_GRP_READ_DEFAULT_RECAPT_RULES, ID: 371741099, model: 746.
// Short name: SWE00478
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_GRP_READ_DEFAULT_RECAPT_RULES.
/// </summary>
[Serializable]
public partial class FnGrpReadDefaultRecaptRules: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_GRP_READ_DEFAULT_RECAPT_RULES program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnGrpReadDefaultRecaptRules(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnGrpReadDefaultRecaptRules.
  /// </summary>
  public FnGrpReadDefaultRecaptRules(IContext context, Import import,
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
    // ---------------------------------------------
    // FN Grp Read Default Recapt Rules
    // Date Created    Created by
    // 08/01/1995      Terry W. Cooley - MTW
    // Date Modified	Modified by
    // 09/25/1995	D.M. Nilsen
    // 04/30/97	SHERAZ MALIK	CHANGE CURRENT_DATE
    // ---------------------------------------------
    local.Current.Date = Now().Date;
    local.Maximum.Date = UseCabSetMaximumDiscontinueDate();

    if (AsChar(import.ShowHistory.Flag) == 'Y')
    {
      // ******
      // For history read all of the default rules out there
      // ******
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadDefaultRule2())
      {
        // ******
        // the following read will abort if no type exists because an obligation
        // type is mandatory for default rule
        // ******
        if (ReadObligationType())
        {
          export.Export1.Update.DetailDefaultRule.Assign(entities.DefaultRule);
          MoveObligationType(entities.ObligationType,
            export.Export1.Update.DetailObligationType);

          if (Equal(entities.DefaultRule.DiscontinueDate, local.Maximum.Date))
          {
            export.Export1.Update.DetailDefaultRule.DiscontinueDate =
              local.Zero.Date;
          }
        }

        export.Export1.Next();
      }
    }
    else
    {
      // ******
      // read only default rules currently active
      // ******
      export.Export1.Index = 0;
      export.Export1.Clear();

      foreach(var item in ReadDefaultRule1())
      {
        // ******
        // the following read will abort if no type exists because an obligation
        // type is mandatory for default rule
        // ******
        if (ReadObligationType())
        {
          export.Export1.Update.DetailDefaultRule.Assign(entities.DefaultRule);
          MoveObligationType(entities.ObligationType,
            export.Export1.Update.DetailObligationType);

          if (Equal(entities.DefaultRule.DiscontinueDate, local.Maximum.Date))
          {
            export.Export1.Update.DetailDefaultRule.DiscontinueDate =
              local.Zero.Date;
          }
        }

        export.Export1.Next();
      }
    }
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private IEnumerable<bool> ReadDefaultRule1()
  {
    return ReadEach("ReadDefaultRule1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.DefaultRule.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DefaultRule.DtyGeneratedId = db.GetNullableInt32(reader, 1);
        entities.DefaultRule.EffectiveDate = db.GetDate(reader, 2);
        entities.DefaultRule.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.DefaultRule.NonAdcArrearsPercentage =
          db.GetNullableInt32(reader, 4);
        entities.DefaultRule.NonAdcCurrentPercentage =
          db.GetNullableInt32(reader, 5);
        entities.DefaultRule.PassthruPercentage =
          db.GetNullableInt32(reader, 6);
        entities.DefaultRule.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadDefaultRule2()
  {
    return ReadEach("ReadDefaultRule2",
      null,
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.DefaultRule.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.DefaultRule.DtyGeneratedId = db.GetNullableInt32(reader, 1);
        entities.DefaultRule.EffectiveDate = db.GetDate(reader, 2);
        entities.DefaultRule.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.DefaultRule.NonAdcArrearsPercentage =
          db.GetNullableInt32(reader, 4);
        entities.DefaultRule.NonAdcCurrentPercentage =
          db.GetNullableInt32(reader, 5);
        entities.DefaultRule.PassthruPercentage =
          db.GetNullableInt32(reader, 6);
        entities.DefaultRule.Populated = true;

        return true;
      });
  }

  private bool ReadObligationType()
  {
    System.Diagnostics.Debug.Assert(entities.DefaultRule.Populated);
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          entities.DefaultRule.DtyGeneratedId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public RecaptureRule Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of ShowHistory.
    /// </summary>
    [JsonPropertyName("showHistory")]
    public Common ShowHistory
    {
      get => showHistory ??= new();
      set => showHistory = value;
    }

    private RecaptureRule last;
    private Common showHistory;
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
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailObligationType.
      /// </summary>
      [JsonPropertyName("detailObligationType")]
      public ObligationType DetailObligationType
      {
        get => detailObligationType ??= new();
        set => detailObligationType = value;
      }

      /// <summary>
      /// A value of DetailDefaultRule.
      /// </summary>
      [JsonPropertyName("detailDefaultRule")]
      public RecaptureRule DetailDefaultRule
      {
        get => detailDefaultRule ??= new();
        set => detailDefaultRule = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 200;

      private Common detailCommon;
      private ObligationType detailObligationType;
      private RecaptureRule detailDefaultRule;
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
    /// A value of Maximum.
    /// </summary>
    [JsonPropertyName("maximum")]
    public DateWorkArea Maximum
    {
      get => maximum ??= new();
      set => maximum = value;
    }

    /// <summary>
    /// A value of DateWorkArea.
    /// </summary>
    [JsonPropertyName("dateWorkArea")]
    public DateWorkArea DateWorkArea
    {
      get => dateWorkArea ??= new();
      set => dateWorkArea = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
    }

    /// <summary>
    /// A value of Last.
    /// </summary>
    [JsonPropertyName("last")]
    public RecaptureRule Last
    {
      get => last ??= new();
      set => last = value;
    }

    /// <summary>
    /// A value of Work.
    /// </summary>
    [JsonPropertyName("work")]
    public RecaptureRule Work
    {
      get => work ??= new();
      set => work = value;
    }

    private DateWorkArea current;
    private DateWorkArea maximum;
    private DateWorkArea dateWorkArea;
    private DateWorkArea zero;
    private RecaptureRule last;
    private RecaptureRule work;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of RuleObligationType.
    /// </summary>
    [JsonPropertyName("ruleObligationType")]
    public ObligationType RuleObligationType
    {
      get => ruleObligationType ??= new();
      set => ruleObligationType = value;
    }

    /// <summary>
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// A value of DefaultRule.
    /// </summary>
    [JsonPropertyName("defaultRule")]
    public RecaptureRule DefaultRule
    {
      get => defaultRule ??= new();
      set => defaultRule = value;
    }

    private ObligationType ruleObligationType;
    private ObligationType obligationType;
    private RecaptureRule defaultRule;
  }
#endregion
}

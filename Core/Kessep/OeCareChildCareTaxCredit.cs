// Program: OE_CARE_CHILD_CARE_TAX_CREDIT, ID: 371894783, model: 746.
// Short name: SWECAREP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_CARE_CHILD_CARE_TAX_CREDIT.
/// </para>
/// <para>
/// Resp - OBLGESTB
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeCareChildCareTaxCredit: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_CARE_CHILD_CARE_TAX_CREDIT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeCareChildCareTaxCredit(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeCareChildCareTaxCredit.
  /// </summary>
  public OeCareChildCareTaxCredit(IContext context, Import import, Export export)
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
    // ---------------------------------------------
    // Date          Author           Reason
    // 04/06/95       	Sid           	Initial Creation
    // 02/06/96      	A. HACKLER     	RETRO FITS
    // 11/14/96	R. Marchman	Add new security and next tran.
    // 06/17/97        M. Wheaton      Removed datenum
    // ---------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      export.ChildCareTaxCreditFactors.Assign(import.ChildCareTaxCreditFactors);

      return;
    }

    // ---------------------------------------------
    // Move Imports to Exports
    // ---------------------------------------------
    export.ChildCareTaxCreditFactors.Assign(import.ChildCareTaxCreditFactors);
    export.Prev.Assign(import.Prev);

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Detail.Assign(import.Import1.Item.Detail);
      export.Export1.Update.Work.SelectChar =
        import.Import1.Item.Work.SelectChar;
      export.Export1.Update.Detail.KansasTaxCreditPercent =
        export.ChildCareTaxCreditFactors.KansasTaxCreditPercent;
      export.Export1.Update.Detail.EffectiveDate =
        export.ChildCareTaxCreditFactors.EffectiveDate;
      export.Export1.Update.Detail.ExpirationDate =
        export.ChildCareTaxCreditFactors.ExpirationDate;

      if (!IsEmpty(import.Import1.Item.Work.SelectChar) && AsChar
        (import.Import1.Item.Work.SelectChar) != 'S' && AsChar
        (import.Import1.Item.Work.SelectChar) != '*')
      {
        var field = GetField(export.Export1.Item.Work, "selectChar");

        field.Error = true;

        ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";
      }
      else if (AsChar(import.Import1.Item.Work.SelectChar) == 'S')
      {
        ++local.WorkSelect.Count;
      }

      export.Export1.Next();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ****
      // ****
      // this is where you would set the local next_tran_info attributes to the 
      // import view attributes for the data to be passed to the next
      // transaction
      // ****
      // ****
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      // ****
      // this is where you set your export value to the export hidden next tran 
      // values if the user is comming into this procedure on a next tran
      // action.
      // ****
      UseScCabNextTranGet();
      global.Command = "DISPLAY";

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    // to validate action level security
    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    switch(TrimEnd(global.Command))
    {
      case "SIGNOFF":
        UseScCabSignoff();

        return;
      case "DISPLAY":
        UseOeListAllChildcareTaxCredit();
        export.Prev.Assign(export.ChildCareTaxCreditFactors);

        break;
      case "ADD":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            if (export.Export1.Item.Detail.MaxMonthlyCreditMultChildren == 0)
            {
              var field =
                GetField(export.Export1.Item.Detail,
                "maxMonthlyCreditMultChildren");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.Detail.MaxMonthlyCredit1Child == 0)
            {
              var field =
                GetField(export.Export1.Item.Detail, "maxMonthlyCredit1Child");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.Detail.FederalTaxCreditPercent == 0)
            {
              var field =
                GetField(export.Export1.Item.Detail, "federalTaxCreditPercent");
                

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }
        }

        if (!Equal(export.ChildCareTaxCreditFactors.ExpirationDate,
          local.Zero.Date))
        {
          if (!Lt(import.ChildCareTaxCreditFactors.EffectiveDate,
            import.ChildCareTaxCreditFactors.ExpirationDate))
          {
            var field =
              GetField(export.ChildCareTaxCreditFactors, "expirationDate");

            field.Error = true;

            ExitState = "EXPIRE_DATE_PRIOR_TO_EFFECTIVE";
          }

          if (Lt(import.ChildCareTaxCreditFactors.ExpirationDate, Now().Date))
          {
            var field =
              GetField(export.ChildCareTaxCreditFactors, "expirationDate");

            field.Error = true;

            ExitState = "EXPIRATION_DATE_PRIOR_TO_CURRENT";
          }
        }

        if (export.ChildCareTaxCreditFactors.KansasTaxCreditPercent == 0)
        {
          var field =
            GetField(export.ChildCareTaxCreditFactors, "kansasTaxCreditPercent");
            

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            local.Common.Flag = "Y";
            UseOeCreateChildCareTaxCredit();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_ADD"))
            {
              export.Export1.Update.Work.SelectChar = "*";
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              if (IsExitState("OE0178_CHILD_CARE_TAX_ADD_ERRIR"))
              {
                var field =
                  GetField(export.ChildCareTaxCreditFactors,
                  "kansasTaxCreditPercent");

                field.Error = true;
              }

              return;
            }
          }
        }

        if (IsEmpty(local.Common.Flag))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_ADD";
        }

        break;
      case "UPDATE":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            if (export.Export1.Item.Detail.MaxMonthlyCreditMultChildren == 0)
            {
              var field =
                GetField(export.Export1.Item.Detail,
                "maxMonthlyCreditMultChildren");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.Detail.MaxMonthlyCredit1Child == 0)
            {
              var field =
                GetField(export.Export1.Item.Detail, "maxMonthlyCredit1Child");

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }

            if (export.Export1.Item.Detail.FederalTaxCreditPercent == 0)
            {
              var field =
                GetField(export.Export1.Item.Detail, "federalTaxCreditPercent");
                

              field.Error = true;

              ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
            }
          }
        }

        if (!Equal(export.ChildCareTaxCreditFactors.ExpirationDate,
          local.Zero.Date))
        {
          if (!Lt(import.ChildCareTaxCreditFactors.EffectiveDate,
            import.ChildCareTaxCreditFactors.ExpirationDate))
          {
            var field =
              GetField(export.ChildCareTaxCreditFactors, "expirationDate");

            field.Error = true;

            ExitState = "EXPIRE_DATE_PRIOR_TO_EFFECTIVE";
          }

          if (Lt(import.ChildCareTaxCreditFactors.ExpirationDate, Now().Date))
          {
            var field =
              GetField(export.ChildCareTaxCreditFactors, "expirationDate");

            field.Error = true;

            ExitState = "EXPIRATION_DATE_PRIOR_TO_CURRENT";
          }
        }

        if (export.ChildCareTaxCreditFactors.KansasTaxCreditPercent == 0)
        {
          var field =
            GetField(export.ChildCareTaxCreditFactors, "kansasTaxCreditPercent");
            

          field.Error = true;

          ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";
        }

        if (!IsExitState("ACO_NN0000_ALL_OK"))
        {
          return;
        }

        if (!Equal(export.ChildCareTaxCreditFactors.EffectiveDate,
          export.Prev.EffectiveDate) || !
          Equal(export.ChildCareTaxCreditFactors.ExpirationDate,
          export.Prev.ExpirationDate) || export
          .ChildCareTaxCreditFactors.KansasTaxCreditPercent != export
          .Prev.KansasTaxCreditPercent)
        {
          for(export.Export1.Index = 0; export.Export1.Index < export
            .Export1.Count; ++export.Export1.Index)
          {
            UseOeUpdateChildCareCreditDate();
          }
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            UseOeUpdateChildCareTaxCredits();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_UPDATE"))
            {
              export.Export1.Update.Work.SelectChar = "*";
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              return;
            }
          }
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

        break;
      case "PREV":
        ExitState = "ZD_ACO_NE0000_INVALID_BACKWARD_2";

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "DELETE":
        if (export.Prev.KansasTaxCreditPercent != import
          .ChildCareTaxCreditFactors.KansasTaxCreditPercent)
        {
          var field =
            GetField(export.ChildCareTaxCreditFactors, "kansasTaxCreditPercent");
            

          field.Error = true;

          ExitState = "OE0012_DISP_REC_BEFORE_DELETE";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Work.SelectChar) == 'S')
          {
            local.Common.Flag = "Y";
            UseOeDeleteChildCareTaxCredit();

            if (IsExitState("ACO_NI0000_SUCCESSFUL_DELETE"))
            {
              export.Export1.Update.Work.SelectChar = "*";
              ExitState = "ACO_NN0000_ALL_OK";
            }
            else
            {
              var field = GetField(export.Export1.Item.Work, "selectChar");

              field.Error = true;

              return;
            }
          }
        }

        if (IsEmpty(local.Common.Flag))
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }
        else
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DELETE";
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }

    global.Command = "";
  }

  private static void MoveChildCareTaxCreditFactors1(
    ChildCareTaxCreditFactors source, ChildCareTaxCreditFactors target)
  {
    target.ExpirationDate = source.ExpirationDate;
    target.EffectiveDate = source.EffectiveDate;
    target.Identifier = source.Identifier;
    target.AdjustedGrossIncomeMaximum = source.AdjustedGrossIncomeMaximum;
    target.AdjustedGrossIncomeMinimum = source.AdjustedGrossIncomeMinimum;
    target.KansasTaxCreditPercent = source.KansasTaxCreditPercent;
    target.FederalTaxCreditPercent = source.FederalTaxCreditPercent;
    target.MaxMonthlyCreditMultChildren = source.MaxMonthlyCreditMultChildren;
    target.MaxMonthlyCredit1Child = source.MaxMonthlyCredit1Child;
  }

  private static void MoveChildCareTaxCreditFactors2(
    ChildCareTaxCreditFactors source, ChildCareTaxCreditFactors target)
  {
    target.ExpirationDate = source.ExpirationDate;
    target.EffectiveDate = source.EffectiveDate;
    target.KansasTaxCreditPercent = source.KansasTaxCreditPercent;
  }

  private static void MoveExport1(OeListAllChildcareTaxCredit.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.Work.SelectChar = source.Work.SelectChar;
  }

  private static void MoveExport1ToImport1(Export.ExportGroup source,
    OeUpdateChildCareCreditDate.Import.ImportGroup target)
  {
    target.Detail.Assign(source.Detail);
    target.Work.SelectChar = source.Work.SelectChar;
  }

  private static void MoveNextTranInfo(NextTranInfo source, NextTranInfo target)
  {
    target.LegalActionIdentifier = source.LegalActionIdentifier;
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CsePersonNumberAp = source.CsePersonNumberAp;
    target.CsePersonNumberObligee = source.CsePersonNumberObligee;
    target.CsePersonNumberObligor = source.CsePersonNumberObligor;
    target.CourtOrderNumber = source.CourtOrderNumber;
    target.ObligationId = source.ObligationId;
    target.StandardCrtOrdNumber = source.StandardCrtOrdNumber;
    target.InfrastructureId = source.InfrastructureId;
    target.MiscText1 = source.MiscText1;
    target.MiscText2 = source.MiscText2;
    target.MiscNum1 = source.MiscNum1;
    target.MiscNum2 = source.MiscNum2;
    target.MiscNum1V2 = source.MiscNum1V2;
    target.MiscNum2V2 = source.MiscNum2V2;
  }

  private void UseOeCreateChildCareTaxCredit()
  {
    var useImport = new OeCreateChildCareTaxCredit.Import();
    var useExport = new OeCreateChildCareTaxCredit.Export();

    MoveChildCareTaxCreditFactors1(export.Export1.Item.Detail,
      useImport.ChildCareTaxCreditFactors);

    Call(OeCreateChildCareTaxCredit.Execute, useImport, useExport);
  }

  private void UseOeDeleteChildCareTaxCredit()
  {
    var useImport = new OeDeleteChildCareTaxCredit.Import();
    var useExport = new OeDeleteChildCareTaxCredit.Export();

    MoveChildCareTaxCreditFactors1(export.Export1.Item.Detail,
      useImport.ChildCareTaxCreditFactors);

    Call(OeDeleteChildCareTaxCredit.Execute, useImport, useExport);
  }

  private void UseOeListAllChildcareTaxCredit()
  {
    var useImport = new OeListAllChildcareTaxCredit.Import();
    var useExport = new OeListAllChildcareTaxCredit.Export();

    Call(OeListAllChildcareTaxCredit.Execute, useImport, useExport);

    export.ChildCareTaxCreditFactors.
      Assign(useExport.ChildCareTaxCreditFactors);
    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseOeUpdateChildCareCreditDate()
  {
    var useImport = new OeUpdateChildCareCreditDate.Import();
    var useExport = new OeUpdateChildCareCreditDate.Export();

    useImport.ChildCareTaxCreditFactors.
      Assign(export.ChildCareTaxCreditFactors);
    export.Export1.CopyTo(useImport.Import1, MoveExport1ToImport1);

    Call(OeUpdateChildCareCreditDate.Execute, useImport, useExport);

    MoveChildCareTaxCreditFactors2(useExport.ChildCareTaxCreditFactors,
      export.ChildCareTaxCreditFactors);
  }

  private void UseOeUpdateChildCareTaxCredits()
  {
    var useImport = new OeUpdateChildCareTaxCredits.Import();
    var useExport = new OeUpdateChildCareTaxCredits.Export();

    MoveChildCareTaxCreditFactors1(export.Export1.Item.Detail,
      useImport.ChildCareTaxCreditFactors);

    Call(OeUpdateChildCareTaxCredits.Execute, useImport, useExport);
  }

  private void UseScCabNextTranGet()
  {
    var useImport = new ScCabNextTranGet.Import();
    var useExport = new ScCabNextTranGet.Export();

    Call(ScCabNextTranGet.Execute, useImport, useExport);

    export.Hidden.Assign(useExport.NextTranInfo);
  }

  private void UseScCabNextTranPut()
  {
    var useImport = new ScCabNextTranPut.Import();
    var useExport = new ScCabNextTranPut.Export();

    useImport.Standard.NextTransaction = import.Standard.NextTransaction;
    MoveNextTranInfo(export.Hidden, useImport.NextTranInfo);

    Call(ScCabNextTranPut.Execute, useImport, useExport);
  }

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ChildCareTaxCreditFactors Detail
      {
        get => detail ??= new();
        set => detail = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ChildCareTaxCreditFactors detail;
      private Common work;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public ChildCareTaxCreditFactors Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
    }

    /// <summary>
    /// A value of ChildCareTaxCreditFactors.
    /// </summary>
    [JsonPropertyName("childCareTaxCreditFactors")]
    public ChildCareTaxCreditFactors ChildCareTaxCreditFactors
    {
      get => childCareTaxCreditFactors ??= new();
      set => childCareTaxCreditFactors = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private ChildCareTaxCreditFactors prev;
    private Array<ImportGroup> import1;
    private ChildCareTaxCreditFactors childCareTaxCreditFactors;
    private NextTranInfo hidden;
    private Standard standard;
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
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ChildCareTaxCreditFactors Detail
      {
        get => detail ??= new();
        set => detail = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ChildCareTaxCreditFactors detail;
      private Common work;
    }

    /// <summary>
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public ChildCareTaxCreditFactors Prev
    {
      get => prev ??= new();
      set => prev = value;
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

    /// <summary>
    /// A value of ChildCareTaxCreditFactors.
    /// </summary>
    [JsonPropertyName("childCareTaxCreditFactors")]
    public ChildCareTaxCreditFactors ChildCareTaxCreditFactors
    {
      get => childCareTaxCreditFactors ??= new();
      set => childCareTaxCreditFactors = value;
    }

    /// <summary>
    /// A value of Hidden.
    /// </summary>
    [JsonPropertyName("hidden")]
    public NextTranInfo Hidden
    {
      get => hidden ??= new();
      set => hidden = value;
    }

    /// <summary>
    /// A value of Standard.
    /// </summary>
    [JsonPropertyName("standard")]
    public Standard Standard
    {
      get => standard ??= new();
      set => standard = value;
    }

    private ChildCareTaxCreditFactors prev;
    private Array<ExportGroup> export1;
    private ChildCareTaxCreditFactors childCareTaxCreditFactors;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
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
    /// A value of WorkSelect.
    /// </summary>
    [JsonPropertyName("workSelect")]
    public Common WorkSelect
    {
      get => workSelect ??= new();
      set => workSelect = value;
    }

    private Common common;
    private DateWorkArea zero;
    private Common workSelect;
  }
#endregion
}

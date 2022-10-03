// Program: FN_ROHL_LST_OBLIG_RECAP_HIST, ID: 372131398, model: 746.
// Short name: SWEROHLP
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
/// A program: FN_ROHL_LST_OBLIG_RECAP_HIST.
/// </para>
/// <para>
/// This procedure sill list all of the recovery obligations that an obligor has
/// had along with the recapture effective and discontinue dates. This list
/// will be ordered by the date the recovery obligation was established. Any one
/// of these recovery obligations can be selected and carried forward to
/// another screen.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRohlLstObligRecapHist: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_ROHL_LST_OBLIG_RECAP_HIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRohlLstObligRecapHist(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRohlLstObligRecapHist.
  /// </summary>
  public FnRohlLstObligRecapHist(IContext context, Import import, Export export):
    
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
    // List Obligation Recapture History
    // Date Created    Created by
    // 07/24/1995      Terry W. Cooley - MTW
    // Date Modified   Modified by
    // 03/19/1996      R.B.Mohapatra - MTW
    // 12/11/96	R. Marchman	Add new security and next tran
    // ---------------------------------------------
    // SWSRKXD PR149011 08/16/2002
    // - Fix screen Help Id.
    // ***************************************************************
    ExitState = "ACO_NN0000_ALL_OK";

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    if (Equal(global.Command, "EXIT"))
    {
      ExitState = "ECO_LNK_RETURN_TO_MENU";

      return;
    }

    // *** Move imports to exports.  Check for return from CSE Person List 
    // screen.
    export.Hidden.Assign(import.Hidden);
    MoveCsePerson(import.CsePerson, export.CsePerson);
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    MoveCsePerson(import.Previous, export.Previous);
    export.DisbursementType.Code = import.DisbursementType.Code;
    export.RecaptureRule.Assign(import.RecaptureRule);
    export.Prompt.SelectChar = import.Prompt.SelectChar;

    if (Equal(global.Command, "RETCSENO"))
    {
      // *** If nothing was sent back, escape.
      export.Prompt.SelectChar = "";

      // *** If a CSE person was selected on NAME, set person number to the new 
      // one.
      if (!IsEmpty(import.CsePersonsWorkSet.Number))
      {
        export.CsePerson.Number = import.CsePersonsWorkSet.Number;
      }

      global.Command = "DISPLAY";
    }

    if (!Equal(global.Command, "DISPLAY"))
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.ObligationType.Code =
          import.Import1.Item.ObligationType.Code;
        export.Export1.Update.Obligation.SystemGeneratedIdentifier =
          import.Import1.Item.Obligation.SystemGeneratedIdentifier;
        export.Export1.Update.DebtDetail.DueDt =
          import.Import1.Item.DebtDetail.DueDt;
        MoveLegalAction(import.Import1.Item.LegalAction,
          export.Export1.Update.LegalAction);
        export.Export1.Update.ObligationTransaction.Amount =
          import.Import1.Item.ObligationTransaction.Amount;
        export.Export1.Update.RecaptureInclusion.Assign(
          import.Import1.Item.RecaptureInclusion);
        export.Export1.Next();
      }
    }

    local.LeftPadding.Text10 = export.CsePerson.Number;
    UseEabPadLeftWithZeros();
    export.CsePerson.Number = local.LeftPadding.Text10;

    if (!Equal(global.Command, "LIST"))
    {
      export.Prompt.SelectChar = "";
    }

    // **** NEXT TRAN in or out handling ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces, this implies the user 
    // requested a next tran action. now validate
    if (!IsEmpty(import.Standard.NextTransaction))
    {
      export.Hidden.CsePersonNumberObligor = import.CsePerson.Number;
      export.Hidden.CsePersonNumber = import.CsePerson.Number;
      UseScCabNextTranPut();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
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

      if (!IsEmpty(export.Hidden.CsePersonNumberObligor))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumberObligor ?? Spaces
          (10);
      }
      else
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }

      // *** If CSE Person number is spaces, escape.  Otherwise, format person 
      // number, and set command to display.
      if (IsEmpty(export.CsePerson.Number))
      {
        return;
      }
      else
      {
        local.LeftPadding.Text10 = export.CsePerson.Number;
        UseEabPadLeftWithZeros();
        export.CsePerson.Number = local.LeftPadding.Text10;
        global.Command = "DISPLAY";
      }
    }

    // **** end NEXT TRAN in or out handling ****
    // **** Security ****
    // to validate action level security
    if (Equal(global.Command, "RHST") || Equal(global.Command, "RCAP") || Equal
      (global.Command, "ENTER"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        return;
      }
    }

    // **** end security ****
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (!IsEmpty(export.CsePerson.Number))
        {
          export.CsePersonsWorkSet.Number = export.CsePerson.Number;
          UseSiReadCsePerson();

          if (!IsExitState("ACO_NN0000_ALL_OK"))
          {
            return;
          }

          UseFnGrpReadObligRecaptHist();

          if (IsExitState("ACO_NN0000_ALL_OK"))
          {
            MoveCsePerson(export.CsePerson, export.Previous);
            export.CsePersonsWorkSet.Number = export.CsePerson.Number;

            if (export.Export1.IsEmpty)
            {
              ExitState = "FN0000_NO_RECORDS_FOUND";
            }
            else
            {
              ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
            }
          }
          else
          {
            var field = GetField(export.CsePerson, "number");

            field.Error = true;
          }
        }
        else
        {
          ExitState = "FN0000_OBLIGOR_NUMBER_REQUIRED";

          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }

        break;
      case "EXIT":
        break;
      case "LIST":
        if (AsChar(import.Prompt.SelectChar) == 'S')
        {
          // ---------------------------------------------
          // The LIST Command is set when the user has
          // pressed the PF4 Key. The procedure will link
          // to another procedure which will list the
          // CSE_PERSON table, allow the user to select
          // a record and return the key back to this
          // procedure.
          // 7/27/1995      tw cooley.
          // ---------------------------------------------
          ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.Prompt, "selectChar");

          field.Error = true;
        }

        break;
      case "NEXT":
        ExitState = "ACO_NE0000_INVALID_FORWARD";

        break;
      case "PREV":
        ExitState = "ACO_NE0000_INVALID_BACKWARD";

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "RCAP":
        ExitState = "FN0000_LNK_RCAP_LST_RECOV_OBLIGS";

        break;
      case "RHST":
        ExitState = "FN0000_LNK_RHST_LST_RC_INSTR_HST";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
  }

  private static void MoveExport1(FnGrpReadObligRecaptHist.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.ObligationType.Code = source.ObligationType.Code;
    target.Obligation.SystemGeneratedIdentifier =
      source.Obligation.SystemGeneratedIdentifier;
    target.DebtDetail.DueDt = source.DebtDetail.DueDt;
    target.ObligationTransaction.Amount = source.ObligationTransaction.Amount;
    target.RecaptureInclusion.Assign(source.RecaptureInclusion);
    MoveLegalAction(source.LegalAction, target.LegalAction);
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.CourtCaseNumber = source.CourtCaseNumber;
    target.StandardNumber = source.StandardNumber;
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

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.LeftPadding.Text10;
    useExport.TextWorkArea.Text10 = local.LeftPadding.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.LeftPadding.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseFnGrpReadObligRecaptHist()
  {
    var useImport = new FnGrpReadObligRecaptHist.Import();
    var useExport = new FnGrpReadObligRecaptHist.Export();

    MoveCsePerson(export.CsePerson, useImport.CsePerson);

    Call(FnGrpReadObligRecaptHist.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
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

    useImport.CsePerson.Number = import.CsePerson.Number;
    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = export.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    export.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
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
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>
      /// A value of Obligation.
      /// </summary>
      [JsonPropertyName("obligation")]
      public Obligation Obligation
      {
        get => obligation ??= new();
        set => obligation = value;
      }

      /// <summary>
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
      }

      /// <summary>
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
      }

      /// <summary>
      /// A value of RecaptureInclusion.
      /// </summary>
      [JsonPropertyName("recaptureInclusion")]
      public RecaptureInclusion RecaptureInclusion
      {
        get => recaptureInclusion ??= new();
        set => recaptureInclusion = value;
      }

      /// <summary>
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private ObligationType obligationType;
      private Obligation obligation;
      private DebtDetail debtDetail;
      private ObligationTransaction obligationTransaction;
      private RecaptureInclusion recaptureInclusion;
      private LegalAction legalAction;
    }

    /// <summary>
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of RecaptureRule.
    /// </summary>
    [JsonPropertyName("recaptureRule")]
    public RecaptureRule RecaptureRule
    {
      get => recaptureRule ??= new();
      set => recaptureRule = value;
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

    /// <summary>
    /// A value of ZdelGroupImportSelect.
    /// </summary>
    [JsonPropertyName("zdelGroupImportSelect")]
    public Common ZdelGroupImportSelect
    {
      get => zdelGroupImportSelect ??= new();
      set => zdelGroupImportSelect = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private Common prompt;
    private CsePerson csePerson;
    private CsePerson previous;
    private Obligation obligation;
    private DisbursementType disbursementType;
    private RecaptureRule recaptureRule;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private Common zdelGroupImportSelect;
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
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>
      /// A value of Obligation.
      /// </summary>
      [JsonPropertyName("obligation")]
      public Obligation Obligation
      {
        get => obligation ??= new();
        set => obligation = value;
      }

      /// <summary>
      /// A value of DebtDetail.
      /// </summary>
      [JsonPropertyName("debtDetail")]
      public DebtDetail DebtDetail
      {
        get => debtDetail ??= new();
        set => debtDetail = value;
      }

      /// <summary>
      /// A value of ObligationTransaction.
      /// </summary>
      [JsonPropertyName("obligationTransaction")]
      public ObligationTransaction ObligationTransaction
      {
        get => obligationTransaction ??= new();
        set => obligationTransaction = value;
      }

      /// <summary>
      /// A value of RecaptureInclusion.
      /// </summary>
      [JsonPropertyName("recaptureInclusion")]
      public RecaptureInclusion RecaptureInclusion
      {
        get => recaptureInclusion ??= new();
        set => recaptureInclusion = value;
      }

      /// <summary>
      /// A value of LegalAction.
      /// </summary>
      [JsonPropertyName("legalAction")]
      public LegalAction LegalAction
      {
        get => legalAction ??= new();
        set => legalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 180;

      private ObligationType obligationType;
      private Obligation obligation;
      private DebtDetail debtDetail;
      private ObligationTransaction obligationTransaction;
      private RecaptureInclusion recaptureInclusion;
      private LegalAction legalAction;
    }

    /// <summary>
    /// A value of PromptedFrom.
    /// </summary>
    [JsonPropertyName("promptedFrom")]
    public Common PromptedFrom
    {
      get => promptedFrom ??= new();
      set => promptedFrom = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of Prompt.
    /// </summary>
    [JsonPropertyName("prompt")]
    public Common Prompt
    {
      get => prompt ??= new();
      set => prompt = value;
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
    }

    /// <summary>
    /// A value of DisbursementType.
    /// </summary>
    [JsonPropertyName("disbursementType")]
    public DisbursementType DisbursementType
    {
      get => disbursementType ??= new();
      set => disbursementType = value;
    }

    /// <summary>
    /// A value of RecaptureRule.
    /// </summary>
    [JsonPropertyName("recaptureRule")]
    public RecaptureRule RecaptureRule
    {
      get => recaptureRule ??= new();
      set => recaptureRule = value;
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

    /// <summary>
    /// A value of ZdelGrpExportSelect.
    /// </summary>
    [JsonPropertyName("zdelGrpExportSelect")]
    public Common ZdelGrpExportSelect
    {
      get => zdelGrpExportSelect ??= new();
      set => zdelGrpExportSelect = value;
    }

    private Common promptedFrom;
    private Array<ExportGroup> export1;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common prompt;
    private CsePerson csePerson;
    private CsePerson previous;
    private Obligation obligation;
    private DisbursementType disbursementType;
    private RecaptureRule recaptureRule;
    private NextTranInfo hidden;
    private Standard standard;
    private Common zdelGrpExportSelect;
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
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
    }

    private Common work;
    private TextWorkArea leftPadding;
  }
#endregion
}

// Program: FN_RCAP_MTN_RECAPTURE_INSTRUCTNS, ID: 372126722, model: 746.
// Short name: SWERCAPP
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
/// A program: FN_RCAP_MTN_RECAPTURE_INSTRUCTNS.
/// </para>
/// <para>
/// This procedure will support the maintenance of recapture instructions for an
/// individual obligor. The recapture instructions are used to determine if
/// money being disbursed out to an obligee should be withheld(recaptured) in
/// order to pay off a recovery debt that is owed by the obligee. In this
/// situation the obligee is also an obligor and some types of disbursements are
/// withheld to pay off the recovery obligations. This procedure will allow for
/// setting up and changing the conditions which dictate when the money will be
/// recaptured. Both the obligations for which recapturing is allowed and the
/// disbursement types that recapturing is allowed on are maintained by this
/// procedure.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnRcapMtnRecaptureInstructns: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_RCAP_MTN_RECAPTURE_INSTRUCTNS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnRcapMtnRecaptureInstructns(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnRcapMtnRecaptureInstructns.
  /// </summary>
  public FnRcapMtnRecaptureInstructns(IContext context, Import import,
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
    // --------------------------------------------------------------
    // Maintain Recapture Rules
    // Date Created    Created by
    // 08/17/1995      Terry W. Cooley - MTW
    // Date Modified   Modified by
    // 09/11/1995      D.M. Nilsen - MTW
    // 03/18/1996      R.B.Mohapatra - MTW
    // 05/22/1996      Siraj Konkader - MTW
    // Added print functions
    // 12/18/96	R. Marchman	Add new security/next tran
    // 01/19/98	R. Marchman     Removed the update capability for Recapture 
    // Rules.
    // 03/26/98	Siraj Konkader		ZDEL cleanup -= partial.
    // 04/04/97	A.Kinney   Remove print function. (done in batch)
    // 08/05/2002   K.Doshi   PR149011    Fix screen help Id.
    // -----------------------------------------------------------------
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

    // *** Move imports to exports
    export.Hidden.Assign(import.Hidden);
    export.Previous.Number = import.Previous.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.Assign(import.CsePersonsWorkSet);
    export.PromptCsePerson.SelectChar = import.PromptCsePerson.SelectChar;

    // *** Check if it is a RETURN for PROMPT-action from the cse_person List
    if (Equal(global.Command, "RETCSENO"))
    {
      export.PromptCsePerson.SelectChar = "";

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
      export.Export1.Index = -1;

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        ++export.Export1.Index;
        export.Export1.CheckSize();

        export.Export1.Update.LegalAction.
          Assign(import.Import1.Item.LegalAction);
        MoveObligationType(import.Import1.Item.ObligationType,
          export.Export1.Update.ObligationType);
        export.Export1.Update.Recap.Flag = import.Import1.Item.Recap.Flag;
        export.Export1.Update.RecapPrev.Flag =
          import.Import1.Item.RecapPrev.Flag;
        export.Export1.Update.Select.SelectChar =
          import.Import1.Item.Select.SelectChar;
        export.Export1.Update.ObligationTransaction.Amount =
          import.Import1.Item.ObligationTransaction.Amount;
        MoveDebtDetail(import.Import1.Item.DebtDetail,
          export.Export1.Update.DebtDetail);
        export.Export1.Update.Obligation.SystemGeneratedIdentifier =
          import.Import1.Item.Obligation.SystemGeneratedIdentifier;
        export.Export1.Update.RecaptureInclusion.SystemGeneratedId =
          import.Import1.Item.RecaptureInclusion.SystemGeneratedId;
      }
    }

    // : If returning from OPSC, display what was there before.
    //   OPSC is the only program that sets command to this value.
    if (Equal(global.Command, "RETLINK"))
    {
      return;
    }

    local.LeftPadding.Text10 = export.CsePerson.Number;
    UseEabPadLeftWithZeros();
    export.CsePerson.Number = local.LeftPadding.Text10;

    if (!Equal(global.Command, "LIST"))
    {
      export.PromptCsePerson.SelectChar = "";
    }

    // **** Perform processing related to NEXT TRAN  ****
    export.Standard.NextTransaction = import.Standard.NextTransaction;

    // if the next tran info is not equal to spaces,  the user requested a next 
    // tran action. now validate
    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      export.Hidden.CsePersonNumberObligor = import.CsePerson.Number;
      export.Hidden.CsePersonNumber = import.CsePerson.Number;
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

      if (IsEmpty(export.Hidden.CsePersonNumberObligor))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }
      else
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumberObligor ?? Spaces
          (10);
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

    // **** end of processing related to NEXT TRAN ****
    // **** Check Security ****
    // to validate action level security
    if (Equal(global.Command, "OREC") || Equal(global.Command, "RHST") || Equal
      (global.Command, "ROHL") || Equal(global.Command, "NORCP") || Equal
      (global.Command, "YESRCP") || Equal(global.Command, "OPSC") || Equal
      (global.Command, "ENTER"))
    {
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    // **** End of Security Check  ****
    if (Equal(global.Command, "NORCP") || Equal(global.Command, "YESRCP"))
    {
      local.Common.Count = 0;

      for(export.Export1.Index = 0; export.Export1.Index < export
        .Export1.Count; ++export.Export1.Index)
      {
        if (!export.Export1.CheckSize())
        {
          break;
        }

        if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
        {
          ++local.Common.Count;

          if (AsChar(export.Export1.Item.Recap.Flag) == 'Y' && Equal
            (global.Command, "YESRCP"))
          {
            var field = GetField(export.Export1.Item.Select, "selectChar");

            field.Error = true;

            ExitState = "FN0000_RECAP_INC_ALREADY_CREATED";
          }

          if (AsChar(export.Export1.Item.Recap.Flag) == 'N' && Equal
            (global.Command, "NORCP"))
          {
            var field = GetField(export.Export1.Item.Select, "selectChar");

            field.Error = true;

            ExitState = "FN0000_RECAP_INC_ALREADY_NO";
          }
        }
        else if (!IsEmpty(export.Export1.Item.Select.SelectChar))
        {
          var field = GetField(export.Export1.Item.Select, "selectChar");

          field.Error = true;

          ExitState = "ZD_ACO_NE0000_INVALID_SELECT_COD";
        }
      }

      export.Export1.CheckIndex();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          return;
        }
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        if (ReadCsePerson())
        {
          // *** Check to see if the person number entered is an obligor.
          if (!ReadObligor())
          {
            if (AsChar(entities.CsePerson.Type1) == 'O')
            {
              var field = GetField(export.CsePerson, "number");

              field.Error = true;

              ExitState = "FN0000_ORGANIZATION_NOT_OBLIGOR";
            }
            else
            {
              var field = GetField(export.CsePerson, "number");

              field.Error = true;

              ExitState = "CSE_PERSON_NOT_OBLIGOR";
            }

            return;
          }

          export.CsePersonsWorkSet.Number = entities.CsePerson.Number;

          if (AsChar(entities.CsePerson.Type1) == 'C')
          {
            UseSiReadCsePerson();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
          }
          else
          {
            export.CsePersonsWorkSet.FormattedName =
              entities.CsePerson.OrganizationName ?? Spaces(33);
          }
        }
        else
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";
          export.Prev.Number = "";
          export.CsePersonsWorkSet.FormattedName = "";

          return;
        }

        UseFnGrpReadRecaptureInclusions();

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";

          if (export.Export1.IsFull)
          {
            ExitState = "FN0000_MORE_DATA_EXISTS";
          }
          else if (export.Export1.IsEmpty)
          {
            ExitState = "FN0000_NO_RECORDS_FOUND";
          }
        }
        else
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;
        }

        break;
      case "LIST":
        if (AsChar(import.PromptCsePerson.SelectChar) == 'S')
        {
          // ---------------------------------------------
          // The LIST command is set when the user has
          // pressed the PF4 Key. The procedure will link
          // to another procedure which will list the
          // CSE_PERSON table, allow the user to select
          // a record and return the key back to this
          // procedure.
          // 08/17/1995      tw cooley.
          // ---------------------------------------------
          ExitState = "ECO_LNK_TO_SELECT_PERSON";
        }
        else
        {
          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";

          var field = GetField(export.PromptCsePerson, "selectChar");

          field.Error = true;
        }

        break;
      case "NORCP":
        // *** Discontinue Recapture on selected inclusions
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            UseFnSetRecaptureInclusionDates();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              var field = GetField(export.Export1.Item.Select, "selectChar");

              field.Error = true;

              return;
            }
          }
        }

        export.Export1.CheckIndex();

        // *** All updates completed ok, so update flags in group view 
        // accordingly.
        ExitState = "FN0000_RECAP_INCLUSION_UPDATED";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            export.Export1.Update.Select.SelectChar = "";
            export.Export1.Update.Recap.Flag = "N";
          }
        }

        export.Export1.CheckIndex();

        break;
      case "YESRCP":
        // *** Activate Recapture on selected inclusions
        export.Export1.Index = 0;

        for(var limit = export.Export1.Count; export.Export1.Index < limit; ++
          export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            UseFnSetRecaptureInclusionDates();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              UseEabRollbackCics();

              var field = GetField(export.Export1.Item.Select, "selectChar");

              field.Error = true;

              return;
            }
          }
        }

        export.Export1.CheckIndex();

        // *** All updates completed ok, so update flags in group view 
        // accordingly.
        ExitState = "FN0000_RECAP_INCLUSION_UPDATED";

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            export.Export1.Update.Select.SelectChar = "";
            export.Export1.Update.Recap.Flag = "Y";
          }
        }

        export.Export1.CheckIndex();

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
      case "OREC":
        export.PassThruFlowCsePersonsWorkSet.Number = export.CsePerson.Number;
        local.Common.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            export.Export1.Update.Select.SelectChar = "";
            export.PassThruFlowObligation.SystemGeneratedIdentifier =
              export.Export1.Item.Obligation.SystemGeneratedIdentifier;
            export.PassThruFlowObligationType.SystemGeneratedIdentifier =
              export.Export1.Item.ObligationType.SystemGeneratedIdentifier;
            export.PassThruFlowObligationType.Code =
              export.Export1.Item.ObligationType.Code;
            export.PassThruFlowLegalAction.Assign(
              export.Export1.Item.LegalAction);
            ++local.Common.Count;
          }

          if (local.Common.Count > 1)
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            var field = GetField(export.Export1.Item.Select, "selectChar");

            field.Protected = false;
            field.Focused = true;

            return;
          }
        }

        export.Export1.CheckIndex();

        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          var field = GetField(export.Export1.Item.Select, "selectChar");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        ExitState = "ECO_LNK_TO_MTN_RECOVERY_OBLIG";

        break;
      case "ROHL":
        export.PassThruFlowCsePersonsWorkSet.Number = export.CsePerson.Number;
        export.PassThruFlowCsePersonsWorkSet.FormattedName =
          export.CsePersonsWorkSet.FormattedName;
        ExitState = "FN0000_LNK_ROHL_LST_OB_RC_HIST";

        break;
      case "RHST":
        // *** Pass the Obligor Number and Name ***
        export.PassThruFlowCsePersonsWorkSet.Number = export.CsePerson.Number;
        export.PassThruFlowCsePersonsWorkSet.FormattedName =
          export.CsePersonsWorkSet.FormattedName;
        ExitState = "FN0000_LNK_RHST_LST_RC_INSTR_HST";

        break;
      case "OPSC":
        export.PassThruFlowCsePersonsWorkSet.Number = export.CsePerson.Number;
        local.Common.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!export.Export1.CheckSize())
          {
            break;
          }

          if (AsChar(export.Export1.Item.Select.SelectChar) == 'S')
          {
            export.Export1.Update.Select.SelectChar = "";
            export.PassThruFlowObligation.SystemGeneratedIdentifier =
              export.Export1.Item.Obligation.SystemGeneratedIdentifier;
            export.PassThruFlowObligationType.SystemGeneratedIdentifier =
              export.Export1.Item.ObligationType.SystemGeneratedIdentifier;
            export.PassThruFlowObligationType.Code =
              export.Export1.Item.ObligationType.Code;
            export.PassThruFlowLegalAction.Assign(
              export.Export1.Item.LegalAction);
            local.Common.Count = (int)((long)local.Common.Count + 1);
          }

          if (local.Common.Count > 1)
          {
            ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

            var field = GetField(export.Export1.Item.Select, "selectChar");

            field.Protected = false;
            field.Focused = true;

            return;
          }
        }

        export.Export1.CheckIndex();

        if (local.Common.Count == 0)
        {
          ExitState = "ACO_NE0000_NO_SELECTION_MADE";

          var field = GetField(export.Export1.Item.Select, "selectChar");

          field.Protected = false;
          field.Focused = true;

          return;
        }

        ExitState = "ECO_LNK_TO_LST_MTN_PYMNT_SCH";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveDebtDetail(DebtDetail source, DebtDetail target)
  {
    target.DueDt = source.DueDt;
    target.BalanceDueAmt = source.BalanceDueAmt;
  }

  private static void MoveExport1(FnGrpReadRecaptureInclusions.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Select.SelectChar = source.DetailSelect.SelectChar;
    MoveObligationType(source.DetailObligationType, target.ObligationType);
    MoveDebtDetail(source.DetailDebtDetail, target.DebtDetail);
    target.ObligationTransaction.Amount =
      source.DetailObligationTransaction.Amount;
    target.Recap.Flag = source.DetailRecap.Flag;
    target.LegalAction.Assign(source.DetailLegalAction);
    target.RecapPrev.Flag = source.DetailRecapPrev.Flag;
    target.Obligation.SystemGeneratedIdentifier =
      source.DetailObligation.SystemGeneratedIdentifier;
    target.RecaptureInclusion.SystemGeneratedId =
      source.DetailRecaptureInclusion.SystemGeneratedId;
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

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
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

  private void UseEabRollbackCics()
  {
    var useImport = new EabRollbackCics.Import();
    var useExport = new EabRollbackCics.Export();

    Call(EabRollbackCics.Execute, useImport, useExport);
  }

  private void UseFnGrpReadRecaptureInclusions()
  {
    var useImport = new FnGrpReadRecaptureInclusions.Import();
    var useExport = new FnGrpReadRecaptureInclusions.Export();

    useImport.CsePerson.Number = entities.CsePerson.Number;

    Call(FnGrpReadRecaptureInclusions.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseFnSetRecaptureInclusionDates()
  {
    var useImport = new FnSetRecaptureInclusionDates.Import();
    var useExport = new FnSetRecaptureInclusionDates.Export();

    useImport.RecaptureInclusion.SystemGeneratedId =
      export.Export1.Item.RecaptureInclusion.SystemGeneratedId;
    useImport.ObligationType.SystemGeneratedIdentifier =
      export.Export1.Item.ObligationType.SystemGeneratedIdentifier;
    useImport.Obligation.SystemGeneratedIdentifier =
      export.Export1.Item.Obligation.SystemGeneratedIdentifier;
    useImport.CsePerson.Number = export.CsePerson.Number;

    Call(FnSetRecaptureInclusionDates.Execute, useImport, useExport);

    export.Export1.Update.RecaptureInclusion.SystemGeneratedId =
      useExport.RecaptureInclusion.SystemGeneratedId;
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
    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

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

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadObligor()
  {
    entities.Obligor.Populated = false;

    return Read("ReadObligor",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Obligor.CspNumber = db.GetString(reader, 0);
        entities.Obligor.Type1 = db.GetString(reader, 1);
        entities.Obligor.Populated = true;
        CheckValid<CsePersonAccount>("Type1", entities.Obligor.Type1);
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
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
      /// A value of Recap.
      /// </summary>
      [JsonPropertyName("recap")]
      public Common Recap
      {
        get => recap ??= new();
        set => recap = value;
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

      /// <summary>
      /// A value of RecapPrev.
      /// </summary>
      [JsonPropertyName("recapPrev")]
      public Common RecapPrev
      {
        get => recapPrev ??= new();
        set => recapPrev = value;
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
      /// A value of RecaptureInclusion.
      /// </summary>
      [JsonPropertyName("recaptureInclusion")]
      public RecaptureInclusion RecaptureInclusion
      {
        get => recaptureInclusion ??= new();
        set => recaptureInclusion = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private Common select;
      private ObligationType obligationType;
      private DebtDetail debtDetail;
      private ObligationTransaction obligationTransaction;
      private Common recap;
      private LegalAction legalAction;
      private Common recapPrev;
      private Obligation obligation;
      private RecaptureInclusion recaptureInclusion;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PromptCsePerson.
    /// </summary>
    [JsonPropertyName("promptCsePerson")]
    public Common PromptCsePerson
    {
      get => promptCsePerson ??= new();
      set => promptCsePerson = value;
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
    /// A value of LastUpdate.
    /// </summary>
    [JsonPropertyName("lastUpdate")]
    public DateWorkArea LastUpdate
    {
      get => lastUpdate ??= new();
      set => lastUpdate = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public RecaptureRule Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of ZdelPreviousObligorRule.
    /// </summary>
    [JsonPropertyName("zdelPreviousObligorRule")]
    public RecaptureRule ZdelPreviousObligorRule
    {
      get => zdelPreviousObligorRule ??= new();
      set => zdelPreviousObligorRule = value;
    }

    /// <summary>
    /// A value of ZdelImportDetailPrev.
    /// </summary>
    [JsonPropertyName("zdelImportDetailPrev")]
    public DebtDetail ZdelImportDetailPrev
    {
      get => zdelImportDetailPrev ??= new();
      set => zdelImportDetailPrev = value;
    }

    /// <summary>
    /// A value of ZdelImportAlreadyDisplayed.
    /// </summary>
    [JsonPropertyName("zdelImportAlreadyDisplayed")]
    public Common ZdelImportAlreadyDisplayed
    {
      get => zdelImportAlreadyDisplayed ??= new();
      set => zdelImportAlreadyDisplayed = value;
    }

    /// <summary>
    /// A value of ZdelImportDefaulted.
    /// </summary>
    [JsonPropertyName("zdelImportDefaulted")]
    public Common ZdelImportDefaulted
    {
      get => zdelImportDefaulted ??= new();
      set => zdelImportDefaulted = value;
    }

    /// <summary>
    /// A value of ZdelImportLast.
    /// </summary>
    [JsonPropertyName("zdelImportLast")]
    public RecaptureInclusion ZdelImportLast
    {
      get => zdelImportLast ??= new();
      set => zdelImportLast = value;
    }

    private CsePerson csePerson;
    private CsePerson previous;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common promptCsePerson;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
    private DateWorkArea lastUpdate;
    private RecaptureRule zdel;
    private RecaptureRule zdelPreviousObligorRule;
    private DebtDetail zdelImportDetailPrev;
    private Common zdelImportAlreadyDisplayed;
    private Common zdelImportDefaulted;
    private RecaptureInclusion zdelImportLast;
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
      /// A value of Select.
      /// </summary>
      [JsonPropertyName("select")]
      public Common Select
      {
        get => select ??= new();
        set => select = value;
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
      /// A value of Recap.
      /// </summary>
      [JsonPropertyName("recap")]
      public Common Recap
      {
        get => recap ??= new();
        set => recap = value;
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

      /// <summary>
      /// A value of RecapPrev.
      /// </summary>
      [JsonPropertyName("recapPrev")]
      public Common RecapPrev
      {
        get => recapPrev ??= new();
        set => recapPrev = value;
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
      /// A value of RecaptureInclusion.
      /// </summary>
      [JsonPropertyName("recaptureInclusion")]
      public RecaptureInclusion RecaptureInclusion
      {
        get => recaptureInclusion ??= new();
        set => recaptureInclusion = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 125;

      private Common select;
      private ObligationType obligationType;
      private DebtDetail debtDetail;
      private ObligationTransaction obligationTransaction;
      private Common recap;
      private LegalAction legalAction;
      private Common recapPrev;
      private Obligation obligation;
      private RecaptureInclusion recaptureInclusion;
    }

    /// <summary>
    /// A value of PassThruFlowCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("passThruFlowCsePersonsWorkSet")]
    public CsePersonsWorkSet PassThruFlowCsePersonsWorkSet
    {
      get => passThruFlowCsePersonsWorkSet ??= new();
      set => passThruFlowCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PassThruFlowObligationType.
    /// </summary>
    [JsonPropertyName("passThruFlowObligationType")]
    public ObligationType PassThruFlowObligationType
    {
      get => passThruFlowObligationType ??= new();
      set => passThruFlowObligationType = value;
    }

    /// <summary>
    /// A value of PassThruFlowObligation.
    /// </summary>
    [JsonPropertyName("passThruFlowObligation")]
    public Obligation PassThruFlowObligation
    {
      get => passThruFlowObligation ??= new();
      set => passThruFlowObligation = value;
    }

    /// <summary>
    /// A value of Defaulted.
    /// </summary>
    [JsonPropertyName("defaulted")]
    public Common Defaulted
    {
      get => defaulted ??= new();
      set => defaulted = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PromptCsePerson.
    /// </summary>
    [JsonPropertyName("promptCsePerson")]
    public Common PromptCsePerson
    {
      get => promptCsePerson ??= new();
      set => promptCsePerson = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 =>
      export1 ??= new(ExportGroup.Capacity, 0);

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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public CsePerson Prev
    {
      get => prev ??= new();
      set => prev = value;
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
    /// A value of LastUpdate.
    /// </summary>
    [JsonPropertyName("lastUpdate")]
    public DateWorkArea LastUpdate
    {
      get => lastUpdate ??= new();
      set => lastUpdate = value;
    }

    /// <summary>
    /// A value of PassThruFlowLegalAction.
    /// </summary>
    [JsonPropertyName("passThruFlowLegalAction")]
    public LegalAction PassThruFlowLegalAction
    {
      get => passThruFlowLegalAction ??= new();
      set => passThruFlowLegalAction = value;
    }

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public RecaptureRule Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
    }

    /// <summary>
    /// A value of ZdelExportPrevious.
    /// </summary>
    [JsonPropertyName("zdelExportPrevious")]
    public RecaptureRule ZdelExportPrevious
    {
      get => zdelExportPrevious ??= new();
      set => zdelExportPrevious = value;
    }

    /// <summary>
    /// A value of ZdelGroupExportDetailPrev.
    /// </summary>
    [JsonPropertyName("zdelGroupExportDetailPrev")]
    public DebtDetail ZdelGroupExportDetailPrev
    {
      get => zdelGroupExportDetailPrev ??= new();
      set => zdelGroupExportDetailPrev = value;
    }

    /// <summary>
    /// A value of ZdelExportLast.
    /// </summary>
    [JsonPropertyName("zdelExportLast")]
    public RecaptureInclusion ZdelExportLast
    {
      get => zdelExportLast ??= new();
      set => zdelExportLast = value;
    }

    /// <summary>
    /// A value of ZdelExportAlreadyDisplayed.
    /// </summary>
    [JsonPropertyName("zdelExportAlreadyDisplayed")]
    public Common ZdelExportAlreadyDisplayed
    {
      get => zdelExportAlreadyDisplayed ??= new();
      set => zdelExportAlreadyDisplayed = value;
    }

    /// <summary>
    /// A value of ZdelPromptedFrom.
    /// </summary>
    [JsonPropertyName("zdelPromptedFrom")]
    public Common ZdelPromptedFrom
    {
      get => zdelPromptedFrom ??= new();
      set => zdelPromptedFrom = value;
    }

    /// <summary>
    /// A value of ZdelExportDlgflw.
    /// </summary>
    [JsonPropertyName("zdelExportDlgflw")]
    public RecaptureRule ZdelExportDlgflw
    {
      get => zdelExportDlgflw ??= new();
      set => zdelExportDlgflw = value;
    }

    private CsePersonsWorkSet passThruFlowCsePersonsWorkSet;
    private ObligationType passThruFlowObligationType;
    private Obligation passThruFlowObligation;
    private Common defaulted;
    private CsePerson csePerson;
    private CsePerson previous;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Common promptCsePerson;
    private Array<ExportGroup> export1;
    private CsePerson prev;
    private NextTranInfo hidden;
    private Standard standard;
    private DateWorkArea lastUpdate;
    private LegalAction passThruFlowLegalAction;
    private RecaptureRule zdel;
    private RecaptureRule zdelExportPrevious;
    private DebtDetail zdelGroupExportDetailPrev;
    private RecaptureInclusion zdelExportLast;
    private Common zdelExportAlreadyDisplayed;
    private Common zdelPromptedFrom;
    private RecaptureRule zdelExportDlgflw;
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
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    /// <summary>
    /// A value of InitializedToZero.
    /// </summary>
    [JsonPropertyName("initializedToZero")]
    public DateWorkArea InitializedToZero
    {
      get => initializedToZero ??= new();
      set => initializedToZero = value;
    }

    /// <summary>
    /// A value of ErrorEntryNo.
    /// </summary>
    [JsonPropertyName("errorEntryNo")]
    public Common ErrorEntryNo
    {
      get => errorEntryNo ??= new();
      set => errorEntryNo = value;
    }

    /// <summary>
    /// A value of RecapModified.
    /// </summary>
    [JsonPropertyName("recapModified")]
    public Common RecapModified
    {
      get => recapModified ??= new();
      set => recapModified = value;
    }

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
    /// A value of Print.
    /// </summary>
    [JsonPropertyName("print")]
    public Document Print
    {
      get => print ??= new();
      set => print = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
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
    /// A value of LeftPadding.
    /// </summary>
    [JsonPropertyName("leftPadding")]
    public TextWorkArea LeftPadding
    {
      get => leftPadding ??= new();
      set => leftPadding = value;
    }

    private DateWorkArea maxDate;
    private DateWorkArea initializedToZero;
    private Common errorEntryNo;
    private Common recapModified;
    private Common common;
    private Document print;
    private Common selected;
    private Common work;
    private TextWorkArea leftPadding;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Obligor.
    /// </summary>
    [JsonPropertyName("obligor")]
    public CsePersonAccount Obligor
    {
      get => obligor ??= new();
      set => obligor = value;
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

    private CsePersonAccount obligor;
    private CsePerson csePerson;
  }
#endregion
}

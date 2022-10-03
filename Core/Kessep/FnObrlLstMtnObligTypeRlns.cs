// Program: FN_OBRL_LST_MTN_OBLIG_TYPE_RLNS, ID: 371966153, model: 746.
// Short name: SWEOBRLP
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
/// A program: FN_OBRL_LST_MTN_OBLIG_TYPE_RLNS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This procedure lists Obligation Types related to a Distribution Policy Rule.
/// It allows allows associates and disassociates of the two entities.  This
/// procedure is also part of the create processing for Distribution Policy
/// Rules.  Therefore, if the import Distribution Policy Rule is not provided,
/// all active Obligation Types will be displayed.  As well, the user may
/// specify whether to show only Obligation Types related to the Distribution
/// Policy Rule, or ANY Obligation Type.  An indicator on the list will show
/// whether or not an Obligation Type is related to the current Distribution
/// Policy Rule.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnObrlLstMtnObligTypeRlns: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_OBRL_LST_MTN_OBLIG_TYPE_RLNS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnObrlLstMtnObligTypeRlns(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnObrlLstMtnObligTypeRlns.
  /// </summary>
  public FnObrlLstMtnObligTypeRlns(IContext context, Import import,
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
    // *******************************************************************
    // CHANGE LOG
    // Date           	Name          reference     Description
    // -------------------------------------------------------------------
    // 04/24/97    	H. Kennedy - MTW           Changed Associate
    //                                            
    // disassociate, and
    //                                            
    // display logic
    //                                            
    // to process with the new
    //                                            
    // DPR Entities, added to
    //                                            
    // resolve many to many
    //                                            
    // relationships
    // 04/28/97	A.Kinney	           Changed Current_Date
    // *******************************************************************
    local.Current.Date = Now().Date;

    // Set initial EXIT STATE.
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      return;
    }

    // Set maximum discontinue date.
    local.MaximumDiscontinueDate.Date = UseCabSetMaximumDiscontinueDate();

    // : Move all IMPORTs to EXPORTs.
    MoveDistributionPolicy(import.PassedDistributionPolicy,
      export.DistributionPolicy);
    export.DistributionPolicyRule.Assign(import.PassedDistributionPolicyRule);
    export.Function.Text13 = import.Function.Text13;
    export.Apply.TextLine10 = import.Apply.TextLine10;
    export.DebtState.TextLine10 = import.DebtState.TextLine10;
    export.Related.SelectChar = import.Related.SelectChar;

    export.Export1.Index = 0;
    export.Export1.Clear();

    for(import.Import1.Index = 0; import.Import1.Index < import.Import1.Count; ++
      import.Import1.Index)
    {
      if (export.Export1.IsFull)
      {
        break;
      }

      export.Export1.Update.Common.SelectChar =
        import.Import1.Item.Common.SelectChar;
      MoveObligationType1(import.Import1.Item.ObligationType,
        export.Export1.Update.ObligationType);
      export.Export1.Update.Related.SelectChar =
        import.Import1.Item.Related.SelectChar;
      export.Export1.Next();
    }

    export.Standard.NextTransaction = import.Standard.NextTransaction;

    if (IsEmpty(import.Standard.NextTransaction))
    {
    }
    else
    {
      // ************************************************
      // *Flowing from here to Next Tran.               *
      // ************************************************
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
      UseScCabNextTranGet();

      return;
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      return;
    }

    if (IsEmpty(export.Related.SelectChar))
    {
      export.Related.SelectChar = "N";
    }

    UseScCabTestSecurity();

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
    }
    else
    {
      return;
    }

    // : Edit Screen
    if (Equal(global.Command, "DISPLAY") || Equal(global.Command, "PROCESS"))
    {
      if (ReadDistributionPolicy())
      {
        MoveDistributionPolicy(entities.DistributionPolicy,
          export.DistributionPolicy);

        // ** Check to see if the Distribution Policy is in effect.
        if (!Equal(entities.DistributionPolicy.MaximumProcessedDt,
          local.NullDate.Date) || Lt
          (entities.DistributionPolicy.EffectiveDt, local.Current.Date))
        {
          local.Active.Flag = "Y";

          if (Equal(global.Command, "PROCESS"))
          {
            ExitState = "FN0000_DIST_PLCY_ACT_NO_UPDATES";

            return;
          }
        }

        if (import.PassedDistributionPolicyRule.SystemGeneratedIdentifier != 0)
        {
          if (ReadDistributionPolicyRule())
          {
            export.DistributionPolicyRule.
              Assign(entities.DistributionPolicyRule);
            UseFnSetDprFieldLiterals();
          }
          else
          {
            ExitState = "FN0000_DIST_PLCY_RULE_NF";

            return;
          }
        }
      }
      else
      {
        ExitState = "FN0000_DIST_PLCY_NF";

        return;
      }

      switch(AsChar(export.Related.SelectChar))
      {
        case 'Y':
          // ** OK **
          break;
        case 'N':
          // ** OK **
          break;
        case ' ':
          export.Related.SelectChar = "Y";

          break;
        default:
          var field = GetField(export.Related, "selectChar");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_INDICATOR_YN";

          return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "REFRESH":
        // : Reset Display
        global.Command = "DISPLAY";

        break;
      case "DISPLAY":
        break;
      case "RETURN":
        // Check to see if a selection has been made.
        local.Select.Flag = "N";

        export.Selected.Index = 0;
        export.Selected.Clear();

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (export.Selected.IsFull)
          {
            break;
          }

          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case 'S':
              MoveObligationType2(export.Export1.Item.ObligationType,
                export.Selected.Update.Selected1);

              // : Send back the Supported Person Indicator.
              export.ObligationType.SupportedPersonReqInd =
                export.Export1.Item.ObligationType.SupportedPersonReqInd;

              break;
            case ' ':
              break;
            case '*':
              break;
            default:
              ExitState = "ZD_ACO_NE0000_INVALID_SEL_CODE_2";

              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              export.Selected.Next();

              return;
          }

          export.Selected.Next();
        }

        break;
      case "PROCESS":
        if (import.PassedDistributionPolicyRule.SystemGeneratedIdentifier == 0)
        {
          ExitState = "FN0000_PROCESS_CMD_INV_NO_DPR";

          return;
        }

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          switch(AsChar(export.Export1.Item.Common.SelectChar))
          {
            case 'A':
              if (AsChar(export.Export1.Item.Related.SelectChar) == 'Y')
              {
                ExitState = "FN0000_DPR_ALRDY_RELATED_TO_OT";

                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;
              }

              break;
            case 'D':
              if (AsChar(export.Export1.Item.Related.SelectChar) != 'Y')
              {
                ExitState = "FN0000_DPR_IS_NOT_RELATED_TO_OT";

                var field1 = GetField(export.Export1.Item.Common, "selectChar");

                field1.Error = true;
              }

              break;
            case ' ':
              break;
            case '*':
              export.Export1.Update.Common.SelectChar = "";

              break;
            default:
              ExitState = "INVALID_ACTION_ENTER_A_OR_D";

              var field = GetField(export.Export1.Item.Common, "selectChar");

              field.Error = true;

              return;
          }
        }

        break;
      default:
        break;
    }

    // Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        // : READ EACH for selection list.
        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadObligationType2())
        {
          // : Bypass Obligation Types that are effective after the DPR,
          //   or Obligation Types that expire before the DPR.
          if (!Lt(entities.ObligationType.EffectiveDt,
            entities.DistributionPolicy.EffectiveDt))
          {
            export.Export1.Next();

            continue;
          }

          if (Lt(entities.ObligationType.DiscontinueDt,
            entities.DistributionPolicy.DiscontinueDt))
          {
            export.Export1.Next();

            continue;
          }

          // : Set a flag to indicate whether or not all the obligation types 
          // have Supported Persons.
          local.ObligationType.SupportedPersonReqInd =
            entities.ObligationType.SupportedPersonReqInd;
          export.Export1.Update.ObligationType.Assign(entities.ObligationType);
          export.Export1.Update.Related.SelectChar = "Y";
          export.Export1.Update.Common.SelectChar = "";
          export.Export1.Next();
        }

        if (AsChar(export.Related.SelectChar) == 'N')
        {
          // ** Search is not limited to only those Obligation Types related to 
          // the current DPR.
          local.NotRelated.Index = 0;
          local.NotRelated.Clear();

          foreach(var item in ReadObligationType3())
          {
            // : Bypass Obligation Types that are effective after the DPR,
            //   or Obligation Types that expire before the DPR.
            if (!Lt(entities.ObligationType.EffectiveDt,
              entities.DistributionPolicy.EffectiveDt))
            {
              local.NotRelated.Next();

              continue;
            }

            if (Lt(entities.ObligationType.DiscontinueDt,
              entities.DistributionPolicy.DiscontinueDt))
            {
              local.NotRelated.Next();

              continue;
            }

            if (export.Export1.IsEmpty)
            {
              local.NotRelated.Update.ObligationType.Assign(
                entities.ObligationType);
              local.NotRelated.Update.Related.SelectChar = "";
              local.ObligationType.SupportedPersonReqInd =
                entities.ObligationType.SupportedPersonReqInd;
            }
            else
            {
              // ** Check to see if the Obligation Type is related to the 
              // current DPR.
              // ** If it is, we will skip it, as it is already in the export 
              // group.
              for(export.Export1.Index = 0; export.Export1.Index < export
                .Export1.Count; ++export.Export1.Index)
              {
                if (entities.ObligationType.SystemGeneratedIdentifier == export
                  .Export1.Item.ObligationType.SystemGeneratedIdentifier)
                {
                  local.NotRelated.Next();

                  goto ReadEach;
                }
                else
                {
                  local.NotRelated.Update.ObligationType.Assign(
                    entities.ObligationType);
                  local.NotRelated.Update.Related.SelectChar = "";
                }
              }
            }

ReadEach:

            local.NotRelated.Next();
          }

          if (!local.NotRelated.IsEmpty)
          {
            // ** Call action block to combine the local and export groups.
            UseFnCombine2GroupViews();
          }
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }

        if (export.Export1.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
        }

        break;
      case "PROCESS":
        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (AsChar(export.Export1.Item.Common.SelectChar) == 'A')
          {
            // : The Obligation Type chosen must have the same supported person
            //   indicator as those already associated.
            if (AsChar(local.ObligationType.SupportedPersonReqInd) == 'Y')
            {
              if (AsChar(export.Export1.Item.ObligationType.
                SupportedPersonReqInd) == 'N')
              {
                ExitState = "FN0000_INVALID_OB_TYPE_SELECTION";

                return;
              }
            }
            else if (AsChar(local.ObligationType.SupportedPersonReqInd) == 'N')
            {
              if (AsChar(export.Export1.Item.ObligationType.
                SupportedPersonReqInd) == 'Y')
              {
                ExitState = "ZD_INVALID_OB_TYPE_SELECTION_2";

                return;
              }
            }
            else
            {
              // : If the indicator is blank, it means that the DPR has no 
              // associated Obligation Types.
              //   If selected Ob Type has supported indicator of "N", there 
              // should be no associated Pgms.
              //   If selected Ob Type has supported indicator of "Y", there 
              // MUST be an associated Pgm.
              if (ReadProgram())
              {
                if (AsChar(export.Export1.Item.ObligationType.
                  SupportedPersonReqInd) == 'N')
                {
                  ExitState = "FN0000_OB_TYPE_INV_DPR_HAS_PGM";

                  return;
                }
              }
              else if (AsChar(export.Export1.Item.ObligationType.
                SupportedPersonReqInd) == 'Y')
              {
                ExitState = "FN0000_OB_TYP_INV_DPR_HAS_NO_PGM";

                return;
              }
            }

            if (ReadObligationType1())
            {
              try
              {
                CreateDprObligType();
              }
              catch(Exception e)
              {
                switch(GetErrorCode(e))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "CO0000_DPR_OBLIG_TYPE_AE_RB";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "CO0000_DPR_OBLIG_TYPE_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              local.ObligationType.SupportedPersonReqInd =
                entities.ObligationType.SupportedPersonReqInd;
              export.Export1.Update.Related.SelectChar = "Y";
              export.Export1.Update.Common.SelectChar = "*";

              // : NOT FOUND exception will cause an abort.
            }
          }
          else if (AsChar(export.Export1.Item.Common.SelectChar) == 'D')
          {
            if (ReadObligationTypeDprObligType())
            {
              DeleteDprObligType();
              export.Export1.Update.Related.SelectChar = "";
              export.Export1.Update.Common.SelectChar = "*";

              // : NOT FOUND exception will cause an abort.
            }
          }
        }

        if (IsExitState("ACO_NN0000_ALL_OK"))
        {
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }

        break;
      case "RETURN":
        ExitState = "ACO_NE0000_RETURN";

        break;
      case "NEXT":
        ExitState = "ACO_NI0000_BOTTOM_OF_LIST";

        break;
      case "PREV":
        ExitState = "ACO_NI0000_TOP_OF_LIST";

        break;
      case "SIGNOFF":
        UseScEabSignoff();

        break;
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
    }
  }

  private static void MoveDistributionPolicy(DistributionPolicy source,
    DistributionPolicy target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Name = source.Name;
  }

  private static void MoveExport1(FnCombine2GroupViews.Export.
    ExportGroup source, Export.ExportGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.ObligationType.Assign(source.ObligationType);
    target.Related.SelectChar = source.Related1.SelectChar;
  }

  private static void MoveExport1ToRelated(Export.ExportGroup source,
    FnCombine2GroupViews.Import.RelatedGroup target)
  {
    target.Common.SelectChar = source.Common.SelectChar;
    target.ObligationType.Assign(source.ObligationType);
    target.Related1.SelectChar = source.Related.SelectChar;
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

  private static void MoveNotRelated(Local.NotRelatedGroup source,
    FnCombine2GroupViews.Import.NotRelatedGroup target)
  {
    target.Grp1Common.SelectChar = source.Common.SelectChar;
    target.Grp1ObligationType.Assign(source.ObligationType);
    target.Grp1RelFlag.SelectChar = source.Related.SelectChar;
  }

  private static void MoveObligationType1(ObligationType source,
    ObligationType target)
  {
    target.SupportedPersonReqInd = source.SupportedPersonReqInd;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
    target.Name = source.Name;
  }

  private static void MoveObligationType2(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnCombine2GroupViews()
  {
    var useImport = new FnCombine2GroupViews.Import();
    var useExport = new FnCombine2GroupViews.Export();

    export.Export1.CopyTo(useImport.Related, MoveExport1ToRelated);
    local.NotRelated.CopyTo(useImport.NotRelated, MoveNotRelated);

    Call(FnCombine2GroupViews.Execute, useImport, useExport);

    useExport.Export1.CopyTo(export.Export1, MoveExport1);
  }

  private void UseFnSetDprFieldLiterals()
  {
    var useImport = new FnSetDprFieldLiterals.Import();
    var useExport = new FnSetDprFieldLiterals.Export();

    useImport.DistributionPolicyRule.Assign(entities.DistributionPolicyRule);

    Call(FnSetDprFieldLiterals.Execute, useImport, useExport);

    export.Apply.TextLine10 = useExport.Apply.TextLine10;
    export.DebtState.TextLine10 = useExport.DebtState.TextLine10;
    export.Function.Text13 = useExport.Function.Text13;
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

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private void UseScEabSignoff()
  {
    var useImport = new ScEabSignoff.Import();
    var useExport = new ScEabSignoff.Export();

    Call(ScEabSignoff.Execute, useImport, useExport);
  }

  private void CreateDprObligType()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);

    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var otyGeneratedId = entities.ObligationType.SystemGeneratedIdentifier;
    var dbpGeneratedId = entities.DistributionPolicyRule.DbpGeneratedId;
    var dprGeneratedId =
      entities.DistributionPolicyRule.SystemGeneratedIdentifier;

    entities.DprObligType.Populated = false;
    Update("CreateDprObligType",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetInt32(command, "otyGeneratedId", otyGeneratedId);
        db.SetInt32(command, "dbpGeneratedId", dbpGeneratedId);
        db.SetInt32(command, "dprGeneratedId", dprGeneratedId);
      });

    entities.DprObligType.CreatedBy = createdBy;
    entities.DprObligType.CreatedTimestamp = createdTimestamp;
    entities.DprObligType.OtyGeneratedId = otyGeneratedId;
    entities.DprObligType.DbpGeneratedId = dbpGeneratedId;
    entities.DprObligType.DprGeneratedId = dprGeneratedId;
    entities.DprObligType.Populated = true;
  }

  private void DeleteDprObligType()
  {
    Update("DeleteDprObligType",
      (db, command) =>
      {
        db.SetInt32(
          command, "otyGeneratedId", entities.DprObligType.OtyGeneratedId);
        db.SetInt32(
          command, "dbpGeneratedId", entities.DprObligType.DbpGeneratedId);
        db.SetInt32(
          command, "dprGeneratedId", entities.DprObligType.DprGeneratedId);
      });
  }

  private bool ReadDistributionPolicy()
  {
    entities.DistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyId",
          import.PassedDistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DistributionPolicy.Name = db.GetString(reader, 1);
        entities.DistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.DistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.DistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.DistributionPolicy.Populated = true;
      });
  }

  private bool ReadDistributionPolicyRule()
  {
    entities.DistributionPolicyRule.Populated = false;

    return Read("ReadDistributionPolicyRule",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.DistributionPolicy.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "distPlcyRlId",
          import.PassedDistributionPolicyRule.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.DistributionPolicyRule.DbpGeneratedId = db.GetInt32(reader, 0);
        entities.DistributionPolicyRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.DistributionPolicyRule.DebtFunctionType =
          db.GetString(reader, 2);
        entities.DistributionPolicyRule.DebtState = db.GetString(reader, 3);
        entities.DistributionPolicyRule.ApplyTo = db.GetString(reader, 4);
        entities.DistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 5);
        entities.DistributionPolicyRule.Populated = true;
        CheckValid<DistributionPolicyRule>("DebtFunctionType",
          entities.DistributionPolicyRule.DebtFunctionType);
        CheckValid<DistributionPolicyRule>("DebtState",
          entities.DistributionPolicyRule.DebtState);
        CheckValid<DistributionPolicyRule>("ApplyTo",
          entities.DistributionPolicyRule.ApplyTo);
      });
  }

  private bool ReadObligationType1()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType1",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          export.Export1.Item.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private IEnumerable<bool> ReadObligationType2()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);

    return ReadEach("ReadObligationType2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.DistributionPolicyRule.DbpGeneratedId);
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private IEnumerable<bool> ReadObligationType3()
  {
    return ReadEach("ReadObligationType3",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "discontinueDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (local.NotRelated.IsFull)
        {
          return false;
        }

        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 6);
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);

        return true;
      });
  }

  private bool ReadObligationTypeDprObligType()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);
    entities.DprObligType.Populated = false;
    entities.ObligationType.Populated = false;

    return Read("ReadObligationTypeDprObligType",
      (db, command) =>
      {
        db.SetInt32(
          command, "debtTypId",
          export.Export1.Item.ObligationType.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dprGeneratedId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.DistributionPolicyRule.DbpGeneratedId);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.DprObligType.OtyGeneratedId = db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Name = db.GetString(reader, 2);
        entities.ObligationType.Classification = db.GetString(reader, 3);
        entities.ObligationType.EffectiveDt = db.GetDate(reader, 4);
        entities.ObligationType.DiscontinueDt = db.GetNullableDate(reader, 5);
        entities.ObligationType.SupportedPersonReqInd = db.GetString(reader, 6);
        entities.DprObligType.CreatedBy = db.GetString(reader, 7);
        entities.DprObligType.CreatedTimestamp = db.GetDateTime(reader, 8);
        entities.DprObligType.DbpGeneratedId = db.GetInt32(reader, 9);
        entities.DprObligType.DprGeneratedId = db.GetInt32(reader, 10);
        entities.DprObligType.Populated = true;
        entities.ObligationType.Populated = true;
        CheckValid<ObligationType>("Classification",
          entities.ObligationType.Classification);
        CheckValid<ObligationType>("SupportedPersonReqInd",
          entities.ObligationType.SupportedPersonReqInd);
      });
  }

  private bool ReadProgram()
  {
    System.Diagnostics.Debug.Assert(entities.DistributionPolicyRule.Populated);
    entities.Program.Populated = false;

    return Read("ReadProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.DistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.DistributionPolicyRule.DbpGeneratedId);
      },
      (db, reader) =>
      {
        entities.Program.SystemGeneratedIdentifier = db.GetInt32(reader, 0);
        entities.Program.Populated = true;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of Related.
      /// </summary>
      [JsonPropertyName("related")]
      public Common Related
      {
        get => related ??= new();
        set => related = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private ObligationType obligationType;
      private Common related;
    }

    /// <summary>
    /// A value of PassedDistributionPolicy.
    /// </summary>
    [JsonPropertyName("passedDistributionPolicy")]
    public DistributionPolicy PassedDistributionPolicy
    {
      get => passedDistributionPolicy ??= new();
      set => passedDistributionPolicy = value;
    }

    /// <summary>
    /// A value of PassedDistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("passedDistributionPolicyRule")]
    public DistributionPolicyRule PassedDistributionPolicyRule
    {
      get => passedDistributionPolicyRule ??= new();
      set => passedDistributionPolicyRule = value;
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
    /// A value of Apply.
    /// </summary>
    [JsonPropertyName("apply")]
    public ListScreenWorkArea Apply
    {
      get => apply ??= new();
      set => apply = value;
    }

    /// <summary>
    /// A value of DebtState.
    /// </summary>
    [JsonPropertyName("debtState")]
    public ListScreenWorkArea DebtState
    {
      get => debtState ??= new();
      set => debtState = value;
    }

    /// <summary>
    /// A value of Function.
    /// </summary>
    [JsonPropertyName("function")]
    public WorkArea Function
    {
      get => function ??= new();
      set => function = value;
    }

    /// <summary>
    /// A value of Related.
    /// </summary>
    [JsonPropertyName("related")]
    public Common Related
    {
      get => related ??= new();
      set => related = value;
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

    private DistributionPolicy passedDistributionPolicy;
    private DistributionPolicyRule passedDistributionPolicyRule;
    private Array<ImportGroup> import1;
    private ListScreenWorkArea apply;
    private ListScreenWorkArea debtState;
    private WorkArea function;
    private Common related;
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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
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
      /// A value of Related.
      /// </summary>
      [JsonPropertyName("related")]
      public Common Related
      {
        get => related ??= new();
        set => related = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private ObligationType obligationType;
      private Common related;
    }

    /// <summary>A SelectedGroup group.</summary>
    [Serializable]
    public class SelectedGroup
    {
      /// <summary>
      /// A value of Selected1.
      /// </summary>
      [JsonPropertyName("selected1")]
      public ObligationType Selected1
      {
        get => selected1 ??= new();
        set => selected1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private ObligationType selected1;
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
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
    }

    /// <summary>
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
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
    /// A value of Apply.
    /// </summary>
    [JsonPropertyName("apply")]
    public ListScreenWorkArea Apply
    {
      get => apply ??= new();
      set => apply = value;
    }

    /// <summary>
    /// A value of DebtState.
    /// </summary>
    [JsonPropertyName("debtState")]
    public ListScreenWorkArea DebtState
    {
      get => debtState ??= new();
      set => debtState = value;
    }

    /// <summary>
    /// A value of Function.
    /// </summary>
    [JsonPropertyName("function")]
    public WorkArea Function
    {
      get => function ??= new();
      set => function = value;
    }

    /// <summary>
    /// A value of Related.
    /// </summary>
    [JsonPropertyName("related")]
    public Common Related
    {
      get => related ??= new();
      set => related = value;
    }

    /// <summary>
    /// Gets a value of Selected.
    /// </summary>
    [JsonIgnore]
    public Array<SelectedGroup> Selected => selected ??= new(
      SelectedGroup.Capacity);

    /// <summary>
    /// Gets a value of Selected for json serialization.
    /// </summary>
    [JsonPropertyName("selected")]
    [Computed]
    public IList<SelectedGroup> Selected_Json
    {
      get => selected;
      set => Selected.Assign(value);
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

    private ObligationType obligationType;
    private DistributionPolicy distributionPolicy;
    private DistributionPolicyRule distributionPolicyRule;
    private Array<ExportGroup> export1;
    private ListScreenWorkArea apply;
    private ListScreenWorkArea debtState;
    private WorkArea function;
    private Common related;
    private Array<SelectedGroup> selected;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A NotRelatedGroup group.</summary>
    [Serializable]
    public class NotRelatedGroup
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
      /// A value of ObligationType.
      /// </summary>
      [JsonPropertyName("obligationType")]
      public ObligationType ObligationType
      {
        get => obligationType ??= new();
        set => obligationType = value;
      }

      /// <summary>
      /// A value of Related.
      /// </summary>
      [JsonPropertyName("related")]
      public Common Related
      {
        get => related ??= new();
        set => related = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common common;
      private ObligationType obligationType;
      private Common related;
    }

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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    /// <summary>
    /// Gets a value of NotRelated.
    /// </summary>
    [JsonIgnore]
    public Array<NotRelatedGroup> NotRelated => notRelated ??= new(
      NotRelatedGroup.Capacity);

    /// <summary>
    /// Gets a value of NotRelated for json serialization.
    /// </summary>
    [JsonPropertyName("notRelated")]
    [Computed]
    public IList<NotRelatedGroup> NotRelated_Json
    {
      get => notRelated;
      set => NotRelated.Assign(value);
    }

    /// <summary>
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public NullDate NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// A value of MaximumDiscontinueDate.
    /// </summary>
    [JsonPropertyName("maximumDiscontinueDate")]
    public DateWorkArea MaximumDiscontinueDate
    {
      get => maximumDiscontinueDate ??= new();
      set => maximumDiscontinueDate = value;
    }

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
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
    }

    private DateWorkArea current;
    private ObligationType obligationType;
    private Array<NotRelatedGroup> notRelated;
    private NullDate nullDate;
    private DateWorkArea maximumDiscontinueDate;
    private Common select;
    private Common active;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
    }

    /// <summary>
    /// A value of DprObligType.
    /// </summary>
    [JsonPropertyName("dprObligType")]
    public DprObligType DprObligType
    {
      get => dprObligType ??= new();
      set => dprObligType = value;
    }

    /// <summary>
    /// A value of Program.
    /// </summary>
    [JsonPropertyName("program")]
    public Program Program
    {
      get => program ??= new();
      set => program = value;
    }

    /// <summary>
    /// A value of DistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("distributionPolicyRule")]
    public DistributionPolicyRule DistributionPolicyRule
    {
      get => distributionPolicyRule ??= new();
      set => distributionPolicyRule = value;
    }

    /// <summary>
    /// A value of DistributionPolicy.
    /// </summary>
    [JsonPropertyName("distributionPolicy")]
    public DistributionPolicy DistributionPolicy
    {
      get => distributionPolicy ??= new();
      set => distributionPolicy = value;
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

    private DprProgram dprProgram;
    private DprObligType dprObligType;
    private Program program;
    private DistributionPolicyRule distributionPolicyRule;
    private DistributionPolicy distributionPolicy;
    private ObligationType obligationType;
  }
#endregion
}

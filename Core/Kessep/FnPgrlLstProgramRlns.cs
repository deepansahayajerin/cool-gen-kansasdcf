// Program: FN_PGRL_LST_PROGRAM_RLNS, ID: 371964012, model: 746.
// Short name: SWEPGRLP
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
/// A program: FN_PGRL_LST_PROGRAM_RLNS.
/// </para>
/// <para>
/// RESP: FINANCE
/// This procedure lists Programs related to a Distribution Policy Rule.  It 
/// allows allows associates and disassociates of the two entities.  This
/// procedure is also part of the CREATE processing for Distribution Policy
/// Rules.  Therefore, if the import Distribution Policy Rule is not provided,
/// all active Programs will be displayed.  As well, the user may specify
/// whether to show only Programs related to the Distribution Policy Rule, or
/// ANY Program.  An indicator on the list will show whether or not a Program is
/// related to the current Distribution Policy Rule.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class FnPgrlLstProgramRlns: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PGRL_LST_PROGRAM_RLNS program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnPgrlLstProgramRlns(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnPgrlLstProgramRlns.
  /// </summary>
  public FnPgrlLstProgramRlns(IContext context, Import import, Export export):
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
    // Date           Name          reference     Description
    // -------------------------------------------------------------------
    // 04/24/97    H. Kennedy - MTW               Changed Associate
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
    // 04/28/97	A.Kinney		   Changed Current_Date
    // 
    // 10/01/08       J. Harden    CQ4387         add assistance_type,
    // assistance_ind
    // *******************************************************************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      global.Command = "DISPLAY";
    }

    // Set maximum discontinue date.
    local.MaximumDiscontinueDate.Date = UseCabSetMaximumDiscontinueDate();

    // : Move all IMPORTs to EXPORTs.
    export.AssistanceType.SelectChar = import.AssistanceType.SelectChar;
    MoveDistributionPolicy(import.DistributionPolicy, export.DistributionPolicy);
      
    MoveDistributionPolicyRule(import.DistributionPolicyRule,
      export.DistributionPolicyRule);
    export.Function.Text13 = import.Function.Text13;
    export.Apply.TextLine10 = import.Apply.TextLine10;
    export.DebtState.TextLine10 = import.DebtState.TextLine10;
    export.Related.SelectChar = import.Related.SelectChar;

    export.Group.Index = 0;
    export.Group.Clear();

    for(import.Group.Index = 0; import.Group.Index < import.Group.Count; ++
      import.Group.Index)
    {
      if (export.Group.IsFull)
      {
        break;
      }

      export.Group.Update.Common.SelectChar =
        import.Group.Item.Common.SelectChar;
      export.Group.Update.Program.Assign(import.Group.Item.Program);
      MoveDprProgram(import.Group.Item.DprProgram,
        export.Group.Update.DprProgram);
      export.Group.Update.Related.SelectChar =
        import.Group.Item.Related.SelectChar;
      export.Group.Next();
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
        return;
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;

        return;
      }
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
      export.AssistanceType.SelectChar = "";
    }

    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // : Main CASE OF COMMAND.
    switch(TrimEnd(global.Command))
    {
      case "DISPLAY":
        switch(AsChar(export.Related.SelectChar))
        {
          case 'Y':
            break;
          case 'N':
            break;
          default:
            export.Related.SelectChar = "Y";

            break;
        }

        // ************CQ4387
        if (AsChar(export.Related.SelectChar) == 'Y')
        {
          switch(AsChar(export.AssistanceType.SelectChar))
          {
            case ' ':
              break;
            case 'T':
              break;
            case 'N':
              break;
            default:
              var field = GetField(export.AssistanceType, "selectChar");

              field.Error = true;

              ExitState = "FN0000_INVALID_VALUE_TN";

              return;
          }
        }
        else
        {
          export.AssistanceType.SelectChar = "";
        }

        if (ReadDistributionPolicy())
        {
          MoveDistributionPolicy(entities.ExistingDistributionPolicy,
            export.DistributionPolicy);

          if (ReadDistributionPolicyRule1())
          {
            export.DistributionPolicyRule.Assign(
              entities.ExistingDistributionPolicyRule);
            UseFnSetDprFieldLiterals();
          }
          else
          {
            ExitState = "FN0000_DIST_PLCY_RULE_NF";

            return;
          }
        }
        else
        {
          ExitState = "FN0000_DIST_PLCY_NF";

          return;
        }

        // : READ EACH for selection list.
        // @@@  Changed the isolation level on the read each below from "
        // Uncommitted / Browse" to "Do Not Specify" to eliminate a -713 sql
        // error
        //      code on the package bind.  This error only began occuring after 
        // the addition of the update dpr_program logic in the Case PROCESS
        // logic.
        //      Makes no sense but was the only way to eliminate the error...
        foreach(var item in ReadProgramDprProgram())
        {
          // ********CQ4387
          switch(AsChar(export.AssistanceType.SelectChar))
          {
            case 'T':
              if (AsChar(entities.ExistingDprProgram.AssistanceInd) == 'N')
              {
                continue;
              }

              break;
            case 'N':
              if (AsChar(entities.ExistingDprProgram.AssistanceInd) == 'T')
              {
                continue;
              }

              break;
            default:
              break;
          }

          if (local.Group.IsEmpty)
          {
            local.Group.Index = 0;
            local.Group.CheckSize();
          }
          else
          {
            ++local.Group.Index;
            local.Group.CheckSize();
          }

          local.Group.Update.Related.SelectChar = "Y";
          local.Group.Update.Program.Assign(entities.ExistingProgram);
          MoveDprProgram(entities.ExistingDprProgram,
            local.Group.Update.DprProgram);

          if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
          {
            break;
          }
        }

        if (AsChar(export.Related.SelectChar) == 'N')
        {
          foreach(var item in ReadProgram2())
          {
            if (Equal(entities.ExistingProgram.Code, "AF") || Equal
              (entities.ExistingProgram.Code, "FC"))
            {
              for(local.Common.Count = 1; local.Common.Count <= 4; ++
                local.Common.Count)
              {
                switch(local.Common.Count)
                {
                  case 1:
                    local.DprProgram.ProgramState = "PA";

                    break;
                  case 2:
                    if (Equal(entities.ExistingProgram.Code, "FC"))
                    {
                      continue;
                    }

                    local.DprProgram.ProgramState = "TA";

                    break;
                  case 3:
                    if (Equal(entities.ExistingProgram.Code, "FC"))
                    {
                      continue;
                    }

                    local.DprProgram.ProgramState = "CA";

                    break;
                  case 4:
                    local.DprProgram.ProgramState = "UK";

                    break;
                  default:
                    break;
                }

                if (local.Group.IsEmpty)
                {
                  local.Group.Index = 0;
                  local.Group.CheckSize();

                  local.Group.Update.Program.Assign(entities.ExistingProgram);
                  MoveDprProgram(local.DprProgram, local.Group.Update.DprProgram);
                    

                  continue;
                }

                for(local.Group.Index = 0; local.Group.Index < local
                  .Group.Count; ++local.Group.Index)
                {
                  if (!local.Group.CheckSize())
                  {
                    break;
                  }

                  if (Equal(entities.ExistingProgram.Code,
                    local.Group.Item.Program.Code) && Equal
                    (local.DprProgram.ProgramState,
                    local.Group.Item.DprProgram.ProgramState))
                  {
                    goto Next1;
                  }
                }

                local.Group.CheckIndex();

                local.Group.Index = local.Group.Count;
                local.Group.CheckSize();

                local.Group.Update.Program.Assign(entities.ExistingProgram);
                MoveDprProgram(local.DprProgram, local.Group.Update.DprProgram);

                if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
                {
                  goto ReadEach2;
                }

Next1:
                ;
              }

              continue;
            }

            if (Equal(entities.ExistingProgram.Code, "NA"))
            {
              for(local.Common.Count = 1; local.Common.Count <= 4; ++
                local.Common.Count)
              {
                switch(local.Common.Count)
                {
                  case 1:
                    local.DprProgram.ProgramState = "NA";

                    break;
                  case 2:
                    local.DprProgram.ProgramState = "UD";

                    break;
                  case 3:
                    local.DprProgram.ProgramState = "UP";

                    break;
                  case 4:
                    local.DprProgram.ProgramState = "CA";

                    break;
                  default:
                    break;
                }

                if (local.Group.IsEmpty)
                {
                  local.Group.Index = 0;
                  local.Group.CheckSize();

                  local.Group.Update.Program.Assign(entities.ExistingProgram);
                  MoveDprProgram(local.DprProgram, local.Group.Update.DprProgram);
                    

                  continue;
                }

                for(local.Group.Index = 0; local.Group.Index < local
                  .Group.Count; ++local.Group.Index)
                {
                  if (!local.Group.CheckSize())
                  {
                    break;
                  }

                  if (Equal(entities.ExistingProgram.Code,
                    local.Group.Item.Program.Code) && Equal
                    (local.DprProgram.ProgramState,
                    local.Group.Item.DprProgram.ProgramState))
                  {
                    goto Next2;
                  }
                }

                local.Group.CheckIndex();

                local.Group.Index = local.Group.Count;
                local.Group.CheckSize();

                local.Group.Update.Program.Assign(entities.ExistingProgram);
                MoveDprProgram(local.DprProgram, local.Group.Update.DprProgram);

                if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
                {
                  goto ReadEach2;
                }

Next2:
                ;
              }

              continue;
            }

            if (local.Group.IsEmpty)
            {
              local.Group.Index = 0;
              local.Group.CheckSize();

              local.Group.Update.Program.Assign(entities.ExistingProgram);
              MoveDprProgram(local.Null1, local.Group.Update.DprProgram);

              continue;
            }

            for(local.Group.Index = 0; local.Group.Index < local.Group.Count; ++
              local.Group.Index)
            {
              if (!local.Group.CheckSize())
              {
                break;
              }

              if (Equal(entities.ExistingProgram.Code,
                local.Group.Item.Program.Code))
              {
                goto ReadEach1;
              }
            }

            local.Group.CheckIndex();

            local.Group.Index = local.Group.Count;
            local.Group.CheckSize();

            local.Group.Update.Program.Assign(entities.ExistingProgram);
            MoveDprProgram(local.Null1, local.Group.Update.DprProgram);

            if (local.Group.Index + 1 == Local.GroupGroup.Capacity)
            {
              break;
            }

ReadEach1:
            ;
          }

ReadEach2:
          ;
        }

        local.Group.Index = -1;

        export.Group.Index = 0;
        export.Group.Clear();

        do
        {
          if (export.Group.IsFull)
          {
            break;
          }

          ++local.Group.Index;
          local.Group.CheckSize();

          MoveDprProgram(local.Group.Item.DprProgram,
            export.Group.Update.DprProgram);
          export.Group.Update.Common.SelectChar =
            local.Group.Item.Common.SelectChar;
          export.Group.Update.Program.Assign(local.Group.Item.Program);
          export.Group.Update.Related.SelectChar =
            local.Group.Item.Related.SelectChar;

          if (local.Group.Index == 0)
          {
            var field = GetField(export.Group.Item.Common, "selectChar");

            field.Protected = false;
            field.Focused = true;
          }

          export.Group.Next();
        }
        while(local.Group.Index + 1 != local.Group.Count);

        if (export.Group.IsEmpty)
        {
          var field = GetField(export.Related, "selectChar");

          field.Protected = false;
          field.Focused = true;

          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";

          return;
        }

        if (export.Group.IsFull)
        {
          ExitState = "ACO_NI0000_LIST_EXCEED_MAX_LNGTH";

          return;
        }

        ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";

        break;
      case "PROCESS":
        // : Check to see if the Distribution Policy is in effect.
        if (!Lt(local.Current.Date, export.DistributionPolicy.EffectiveDt))
        {
          ExitState = "FN0000_DIST_PLCY_ACT_NO_UPDATES";

          return;
        }

        if (!Equal(export.DistributionPolicy.MaximumProcessedDt,
          local.NullDate.Date))
        {
          ExitState = "FN0000_DIST_PLCY_ACT_NO_UPDATES";

          return;
        }

        if (!ReadDistributionPolicyRule2())
        {
          ExitState = "FN0000_DIST_PLCY_RULE_NF";

          return;
        }

        for(export.Group.Index = 0; export.Group.Index < export.Group.Count; ++
          export.Group.Index)
        {
          if (import.DistributionPolicyRule.SystemGeneratedIdentifier == 0)
          {
            ExitState = "FN0000_PROCESS_CMD_INV_NO_DPR";

            return;
          }

          switch(AsChar(export.Group.Item.Common.SelectChar))
          {
            case 'A':
              if (AsChar(export.Group.Item.Related.SelectChar) == 'Y')
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "FN0000_DPR_ALRDY_RELATED_TO_PGM";

                continue;
              }

              // *********CQ4387
              switch(AsChar(export.Group.Item.DprProgram.AssistanceInd))
              {
                case 'T':
                  break;
                case 'N':
                  break;
                case ' ':
                  break;
                default:
                  var field1 =
                    GetField(export.Group.Item.DprProgram, "assistanceInd");

                  field1.Error = true;

                  ExitState = "FN0000_INVALID_VALUE_TN";

                  continue;
              }

              if (ReadProgram1())
              {
                try
                {
                  CreateDprProgram();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "CO0000_DPR_PROGRAM_AE_RB";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "CO0000_DPR_PROGRAM_PV_RB";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }

                export.Group.Update.Common.SelectChar = "*";
                export.Group.Update.Related.SelectChar = "Y";
              }
              else
              {
                ExitState = "PROGRAM_NF_RB";

                return;
              }

              break;
            case 'D':
              if (AsChar(export.Group.Item.Related.SelectChar) != 'Y')
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "FN0000_DPR_IS_NOT_RELATED_TO_PGM";

                continue;
              }

              if (ReadDprProgram2())
              {
                DeleteDprProgram();
                export.Group.Update.Common.SelectChar = "*";
                export.Group.Update.Related.SelectChar = "";
                ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                // : NOT FOUND exception will cause an abort.
              }
              else
              {
                ExitState = "FN0000_DPR_PROGRAM_NF_RB";

                return;
              }

              break;
            case '*':
              break;
            case 'U':
              if (AsChar(export.Group.Item.Related.SelectChar) != 'Y')
              {
                var field1 = GetField(export.Group.Item.Common, "selectChar");

                field1.Error = true;

                ExitState = "FN0000_DPR_IS_NOT_RELATED_TO_PGM";

                continue;
              }

              if (ReadDprProgram1())
              {
                try
                {
                  UpdateDprProgram();
                }
                catch(Exception e)
                {
                  switch(GetErrorCode(e))
                  {
                    case ErrorCode.AlreadyExists:
                      ExitState = "CO0000_DPR_PROGRAM_NU_RB";

                      return;
                    case ErrorCode.PermittedValueViolation:
                      ExitState = "CO0000_DPR_PROGRAM_PV_RB";

                      return;
                    case ErrorCode.DatabaseError:
                      break;
                    default:
                      throw;
                  }
                }

                export.Group.Update.Common.SelectChar = "*";
                export.Group.Update.Related.SelectChar = "";
                ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";

                // : NOT FOUND exception will cause an abort.
              }
              else
              {
                ExitState = "FN0000_DPR_PROGRAM_NF_RB";

                return;
              }

              break;
            case ' ':
              break;
            default:
              var field = GetField(export.Group.Item.Common, "selectChar");

              field.Error = true;

              ExitState = "INVALID_ACTION_ENTER_A_U_OR_D";

              return;
          }
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
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
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
    target.Description = source.Description;
    target.EffectiveDt = source.EffectiveDt;
    target.DiscontinueDt = source.DiscontinueDt;
    target.MaximumProcessedDt = source.MaximumProcessedDt;
  }

  private static void MoveDistributionPolicyRule(DistributionPolicyRule source,
    DistributionPolicyRule target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.DebtFunctionType = source.DebtFunctionType;
    target.DebtState = source.DebtState;
    target.ApplyTo = source.ApplyTo;
  }

  private static void MoveDprProgram(DprProgram source, DprProgram target)
  {
    target.AssistanceInd = source.AssistanceInd;
    target.ProgramState = source.ProgramState;
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

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseFnSetDprFieldLiterals()
  {
    var useImport = new FnSetDprFieldLiterals.Import();
    var useExport = new FnSetDprFieldLiterals.Export();

    useImport.DistributionPolicyRule.Assign(
      entities.ExistingDistributionPolicyRule);

    Call(FnSetDprFieldLiterals.Execute, useImport, useExport);

    export.Function.Text13 = useExport.Function.Text13;
    export.DebtState.TextLine10 = useExport.DebtState.TextLine10;
    export.Apply.TextLine10 = useExport.Apply.TextLine10;
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

  private void CreateDprProgram()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingDistributionPolicyRule.Populated);

    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var dbpGeneratedId = entities.ExistingDistributionPolicyRule.DbpGeneratedId;
    var dprGeneratedId =
      entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier;
    var prgGeneratedId = entities.ExistingProgram.SystemGeneratedIdentifier;
    var programState = export.Group.Item.DprProgram.ProgramState;
    var assistanceInd = export.Group.Item.DprProgram.AssistanceInd ?? "";

    entities.ExistingDprProgram.Populated = false;
    Update("CreateDprProgram",
      (db, command) =>
      {
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetInt32(command, "dbpGeneratedId", dbpGeneratedId);
        db.SetInt32(command, "dprGeneratedId", dprGeneratedId);
        db.SetInt32(command, "prgGeneratedId", prgGeneratedId);
        db.SetString(command, "programState", programState);
        db.SetNullableString(command, "assistanceInd", assistanceInd);
      });

    entities.ExistingDprProgram.CreatedBy = createdBy;
    entities.ExistingDprProgram.CreatedTimestamp = createdTimestamp;
    entities.ExistingDprProgram.DbpGeneratedId = dbpGeneratedId;
    entities.ExistingDprProgram.DprGeneratedId = dprGeneratedId;
    entities.ExistingDprProgram.PrgGeneratedId = prgGeneratedId;
    entities.ExistingDprProgram.ProgramState = programState;
    entities.ExistingDprProgram.AssistanceInd = assistanceInd;
    entities.ExistingDprProgram.Populated = true;
  }

  private void DeleteDprProgram()
  {
    Update("DeleteDprProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDprProgram.DbpGeneratedId);
        db.SetInt32(
          command, "dprGeneratedId",
          entities.ExistingDprProgram.DprGeneratedId);
        db.SetInt32(
          command, "prgGeneratedId",
          entities.ExistingDprProgram.PrgGeneratedId);
        db.SetString(
          command, "programState", entities.ExistingDprProgram.ProgramState);
      });
  }

  private bool ReadDistributionPolicy()
  {
    entities.ExistingDistributionPolicy.Populated = false;

    return Read("ReadDistributionPolicy",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyId",
          export.DistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDistributionPolicy.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingDistributionPolicy.Name = db.GetString(reader, 1);
        entities.ExistingDistributionPolicy.EffectiveDt = db.GetDate(reader, 2);
        entities.ExistingDistributionPolicy.DiscontinueDt =
          db.GetNullableDate(reader, 3);
        entities.ExistingDistributionPolicy.MaximumProcessedDt =
          db.GetNullableDate(reader, 4);
        entities.ExistingDistributionPolicy.Description =
          db.GetString(reader, 5);
        entities.ExistingDistributionPolicy.Populated = true;
      });
  }

  private bool ReadDistributionPolicyRule1()
  {
    entities.ExistingDistributionPolicyRule.Populated = false;

    return Read("ReadDistributionPolicyRule1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDistributionPolicy.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "distPlcyRlId",
          export.DistributionPolicyRule.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDistributionPolicyRule.DbpGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingDistributionPolicyRule.DebtFunctionType =
          db.GetString(reader, 2);
        entities.ExistingDistributionPolicyRule.DebtState =
          db.GetString(reader, 3);
        entities.ExistingDistributionPolicyRule.ApplyTo =
          db.GetString(reader, 4);
        entities.ExistingDistributionPolicyRule.CreatedBy =
          db.GetString(reader, 5);
        entities.ExistingDistributionPolicyRule.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.ExistingDistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingDistributionPolicyRule.Populated = true;
        CheckValid<DistributionPolicyRule>("DebtFunctionType",
          entities.ExistingDistributionPolicyRule.DebtFunctionType);
        CheckValid<DistributionPolicyRule>("DebtState",
          entities.ExistingDistributionPolicyRule.DebtState);
        CheckValid<DistributionPolicyRule>("ApplyTo",
          entities.ExistingDistributionPolicyRule.ApplyTo);
      });
  }

  private bool ReadDistributionPolicyRule2()
  {
    entities.ExistingDistributionPolicyRule.Populated = false;

    return Read("ReadDistributionPolicyRule2",
      (db, command) =>
      {
        db.SetInt32(
          command, "distPlcyRlId",
          export.DistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          export.DistributionPolicy.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingDistributionPolicyRule.DbpGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier =
          db.GetInt32(reader, 1);
        entities.ExistingDistributionPolicyRule.DebtFunctionType =
          db.GetString(reader, 2);
        entities.ExistingDistributionPolicyRule.DebtState =
          db.GetString(reader, 3);
        entities.ExistingDistributionPolicyRule.ApplyTo =
          db.GetString(reader, 4);
        entities.ExistingDistributionPolicyRule.CreatedBy =
          db.GetString(reader, 5);
        entities.ExistingDistributionPolicyRule.CreatedTmst =
          db.GetDateTime(reader, 6);
        entities.ExistingDistributionPolicyRule.DprNextId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingDistributionPolicyRule.Populated = true;
        CheckValid<DistributionPolicyRule>("DebtFunctionType",
          entities.ExistingDistributionPolicyRule.DebtFunctionType);
        CheckValid<DistributionPolicyRule>("DebtState",
          entities.ExistingDistributionPolicyRule.DebtState);
        CheckValid<DistributionPolicyRule>("ApplyTo",
          entities.ExistingDistributionPolicyRule.ApplyTo);
      });
  }

  private bool ReadDprProgram1()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingDistributionPolicyRule.Populated);
    entities.ExistingDprProgram.Populated = false;

    return Read("ReadDprProgram1",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDistributionPolicyRule.DbpGeneratedId);
        db.SetInt32(
          command, "prgGeneratedId",
          export.Group.Item.Program.SystemGeneratedIdentifier);
        db.SetString(
          command, "programState", export.Group.Item.DprProgram.ProgramState);
      },
      (db, reader) =>
      {
        entities.ExistingDprProgram.CreatedBy = db.GetString(reader, 0);
        entities.ExistingDprProgram.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingDprProgram.DbpGeneratedId = db.GetInt32(reader, 2);
        entities.ExistingDprProgram.DprGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDprProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingDprProgram.ProgramState = db.GetString(reader, 5);
        entities.ExistingDprProgram.AssistanceInd =
          db.GetNullableString(reader, 6);
        entities.ExistingDprProgram.Populated = true;
      });
  }

  private bool ReadDprProgram2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingDistributionPolicyRule.Populated);
    entities.ExistingDprProgram.Populated = false;

    return Read("ReadDprProgram2",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDistributionPolicyRule.DbpGeneratedId);
        db.SetInt32(
          command, "prgGeneratedId",
          export.Group.Item.Program.SystemGeneratedIdentifier);
        db.SetString(
          command, "programState", export.Group.Item.DprProgram.ProgramState);
      },
      (db, reader) =>
      {
        entities.ExistingDprProgram.CreatedBy = db.GetString(reader, 0);
        entities.ExistingDprProgram.CreatedTimestamp =
          db.GetDateTime(reader, 1);
        entities.ExistingDprProgram.DbpGeneratedId = db.GetInt32(reader, 2);
        entities.ExistingDprProgram.DprGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingDprProgram.PrgGeneratedId = db.GetInt32(reader, 4);
        entities.ExistingDprProgram.ProgramState = db.GetString(reader, 5);
        entities.ExistingDprProgram.AssistanceInd =
          db.GetNullableString(reader, 6);
        entities.ExistingDprProgram.Populated = true;
      });
  }

  private bool ReadProgram1()
  {
    entities.ExistingProgram.Populated = false;

    return Read("ReadProgram1",
      (db, command) =>
      {
        db.SetInt32(
          command, "programId",
          export.Group.Item.Program.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingProgram.Code = db.GetString(reader, 1);
        entities.ExistingProgram.Title = db.GetString(reader, 2);
        entities.ExistingProgram.InterstateIndicator = db.GetString(reader, 3);
        entities.ExistingProgram.EffectiveDate = db.GetDate(reader, 4);
        entities.ExistingProgram.DiscontinueDate = db.GetDate(reader, 5);
        entities.ExistingProgram.CreatedBy = db.GetString(reader, 6);
        entities.ExistingProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.ExistingProgram.DistributionProgramType =
          db.GetString(reader, 8);
        entities.ExistingProgram.Populated = true;
      });
  }

  private IEnumerable<bool> ReadProgram2()
  {
    entities.ExistingProgram.Populated = false;

    return ReadEach("ReadProgram2",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingProgram.Code = db.GetString(reader, 1);
        entities.ExistingProgram.Title = db.GetString(reader, 2);
        entities.ExistingProgram.InterstateIndicator = db.GetString(reader, 3);
        entities.ExistingProgram.EffectiveDate = db.GetDate(reader, 4);
        entities.ExistingProgram.DiscontinueDate = db.GetDate(reader, 5);
        entities.ExistingProgram.CreatedBy = db.GetString(reader, 6);
        entities.ExistingProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.ExistingProgram.DistributionProgramType =
          db.GetString(reader, 8);
        entities.ExistingProgram.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadProgramDprProgram()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingDistributionPolicyRule.Populated);
    entities.ExistingDprProgram.Populated = false;
    entities.ExistingProgram.Populated = false;

    return ReadEach("ReadProgramDprProgram",
      (db, command) =>
      {
        db.SetInt32(
          command, "dprGeneratedId",
          entities.ExistingDistributionPolicyRule.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDistributionPolicyRule.DbpGeneratedId);
      },
      (db, reader) =>
      {
        entities.ExistingProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingDprProgram.PrgGeneratedId = db.GetInt32(reader, 0);
        entities.ExistingProgram.Code = db.GetString(reader, 1);
        entities.ExistingProgram.Title = db.GetString(reader, 2);
        entities.ExistingProgram.InterstateIndicator = db.GetString(reader, 3);
        entities.ExistingProgram.EffectiveDate = db.GetDate(reader, 4);
        entities.ExistingProgram.DiscontinueDate = db.GetDate(reader, 5);
        entities.ExistingProgram.CreatedBy = db.GetString(reader, 6);
        entities.ExistingProgram.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.ExistingProgram.DistributionProgramType =
          db.GetString(reader, 8);
        entities.ExistingDprProgram.CreatedBy = db.GetString(reader, 9);
        entities.ExistingDprProgram.CreatedTimestamp =
          db.GetDateTime(reader, 10);
        entities.ExistingDprProgram.DbpGeneratedId = db.GetInt32(reader, 11);
        entities.ExistingDprProgram.DprGeneratedId = db.GetInt32(reader, 12);
        entities.ExistingDprProgram.ProgramState = db.GetString(reader, 13);
        entities.ExistingDprProgram.AssistanceInd =
          db.GetNullableString(reader, 14);
        entities.ExistingDprProgram.Populated = true;
        entities.ExistingProgram.Populated = true;

        return true;
      });
  }

  private void UpdateDprProgram()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingDprProgram.Populated);

    var assistanceInd = export.Group.Item.DprProgram.AssistanceInd ?? "";

    entities.ExistingDprProgram.Populated = false;
    Update("UpdateDprProgram",
      (db, command) =>
      {
        db.SetNullableString(command, "assistanceInd", assistanceInd);
        db.SetInt32(
          command, "dbpGeneratedId",
          entities.ExistingDprProgram.DbpGeneratedId);
        db.SetInt32(
          command, "dprGeneratedId",
          entities.ExistingDprProgram.DprGeneratedId);
        db.SetInt32(
          command, "prgGeneratedId",
          entities.ExistingDprProgram.PrgGeneratedId);
        db.SetString(
          command, "programState", entities.ExistingDprProgram.ProgramState);
      });

    entities.ExistingDprProgram.AssistanceInd = assistanceInd;
    entities.ExistingDprProgram.Populated = true;
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
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common related;
      private Program program;
      private DprProgram dprProgram;
      private Common common;
    }

    /// <summary>
    /// A value of AssistanceType.
    /// </summary>
    [JsonPropertyName("assistanceType")]
    public Common AssistanceType
    {
      get => assistanceType ??= new();
      set => assistanceType = value;
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
    /// A value of Related.
    /// </summary>
    [JsonPropertyName("related")]
    public Common Related
    {
      get => related ??= new();
      set => related = value;
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
    /// A value of DebtState.
    /// </summary>
    [JsonPropertyName("debtState")]
    public ListScreenWorkArea DebtState
    {
      get => debtState ??= new();
      set => debtState = value;
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
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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

    private Common assistanceType;
    private DistributionPolicy distributionPolicy;
    private DistributionPolicyRule distributionPolicyRule;
    private Common related;
    private WorkArea function;
    private ListScreenWorkArea debtState;
    private ListScreenWorkArea apply;
    private Array<GroupGroup> group;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common related;
      private Program program;
      private DprProgram dprProgram;
      private Common common;
    }

    /// <summary>A DelMeGroup group.</summary>
    [Serializable]
    public class DelMeGroup
    {
      /// <summary>
      /// A value of DelMe1.
      /// </summary>
      [JsonPropertyName("delMe1")]
      public Program DelMe1
      {
        get => delMe1 ??= new();
        set => delMe1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 1;

      private Program delMe1;
    }

    /// <summary>
    /// A value of AssistanceType.
    /// </summary>
    [JsonPropertyName("assistanceType")]
    public Common AssistanceType
    {
      get => assistanceType ??= new();
      set => assistanceType = value;
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
    /// A value of Function.
    /// </summary>
    [JsonPropertyName("function")]
    public WorkArea Function
    {
      get => function ??= new();
      set => function = value;
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
    /// A value of Apply.
    /// </summary>
    [JsonPropertyName("apply")]
    public ListScreenWorkArea Apply
    {
      get => apply ??= new();
      set => apply = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
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
    /// Gets a value of DelMe.
    /// </summary>
    [JsonIgnore]
    public Array<DelMeGroup> DelMe => delMe ??= new(DelMeGroup.Capacity);

    /// <summary>
    /// Gets a value of DelMe for json serialization.
    /// </summary>
    [JsonPropertyName("delMe")]
    [Computed]
    public IList<DelMeGroup> DelMe_Json
    {
      get => delMe;
      set => DelMe.Assign(value);
    }

    private Common assistanceType;
    private Common related;
    private WorkArea function;
    private ListScreenWorkArea debtState;
    private ListScreenWorkArea apply;
    private Array<GroupGroup> group;
    private DistributionPolicyRule distributionPolicyRule;
    private DistributionPolicy distributionPolicy;
    private NextTranInfo hidden;
    private Standard standard;
    private Array<DelMeGroup> delMe;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A GroupGroup group.</summary>
    [Serializable]
    public class GroupGroup
    {
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
      /// A value of Program.
      /// </summary>
      [JsonPropertyName("program")]
      public Program Program
      {
        get => program ??= new();
        set => program = value;
      }

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
      /// A value of Common.
      /// </summary>
      [JsonPropertyName("common")]
      public Common Common
      {
        get => common ??= new();
        set => common = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common related;
      private Program program;
      private DprProgram dprProgram;
      private Common common;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DprProgram Null1
    {
      get => null1 ??= new();
      set => null1 = value;
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
    /// A value of DprProgram.
    /// </summary>
    [JsonPropertyName("dprProgram")]
    public DprProgram DprProgram
    {
      get => dprProgram ??= new();
      set => dprProgram = value;
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
    /// A value of Active.
    /// </summary>
    [JsonPropertyName("active")]
    public Common Active
    {
      get => active ??= new();
      set => active = value;
    }

    /// <summary>
    /// A value of RelatedFoundInd.
    /// </summary>
    [JsonPropertyName("relatedFoundInd")]
    public Common RelatedFoundInd
    {
      get => relatedFoundInd ??= new();
      set => relatedFoundInd = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public NullDate NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
    }

    /// <summary>
    /// Gets a value of Group.
    /// </summary>
    [JsonIgnore]
    public Array<GroupGroup> Group => group ??= new(GroupGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Group for json serialization.
    /// </summary>
    [JsonPropertyName("group")]
    [Computed]
    public IList<GroupGroup> Group_Json
    {
      get => group;
      set => Group.Assign(value);
    }

    private DprProgram null1;
    private Common common;
    private DprProgram dprProgram;
    private DateWorkArea current;
    private Common active;
    private Common relatedFoundInd;
    private DateWorkArea maximumDiscontinueDate;
    private NullDate nullDate;
    private Array<GroupGroup> group;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingDprProgram.
    /// </summary>
    [JsonPropertyName("existingDprProgram")]
    public DprProgram ExistingDprProgram
    {
      get => existingDprProgram ??= new();
      set => existingDprProgram = value;
    }

    /// <summary>
    /// A value of ExistingProgram.
    /// </summary>
    [JsonPropertyName("existingProgram")]
    public Program ExistingProgram
    {
      get => existingProgram ??= new();
      set => existingProgram = value;
    }

    /// <summary>
    /// A value of ExistingDistributionPolicy.
    /// </summary>
    [JsonPropertyName("existingDistributionPolicy")]
    public DistributionPolicy ExistingDistributionPolicy
    {
      get => existingDistributionPolicy ??= new();
      set => existingDistributionPolicy = value;
    }

    /// <summary>
    /// A value of ExistingDistributionPolicyRule.
    /// </summary>
    [JsonPropertyName("existingDistributionPolicyRule")]
    public DistributionPolicyRule ExistingDistributionPolicyRule
    {
      get => existingDistributionPolicyRule ??= new();
      set => existingDistributionPolicyRule = value;
    }

    private DprProgram existingDprProgram;
    private Program existingProgram;
    private DistributionPolicy existingDistributionPolicy;
    private DistributionPolicyRule existingDistributionPolicyRule;
  }
#endregion
}

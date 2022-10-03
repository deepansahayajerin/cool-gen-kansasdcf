// Program: SI_CADS_ADD_CASE_ROLE_DETAIL, ID: 371731795, model: 746.
// Short name: SWE01094
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CADS_ADD_CASE_ROLE_DETAIL.
/// </summary>
[Serializable]
public partial class SiCadsAddCaseRoleDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CADS_ADD_CASE_ROLE_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCadsAddCaseRoleDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCadsAddCaseRoleDetail.
  /// </summary>
  public SiCadsAddCaseRoleDetail(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------
    //           M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // ??-??-??  ?????????		Initial Development
    // 04/08/99 W.Campbell             Fixed consistency
    //                                 
    // check errors dealing with the
    //                                 
    // create of both non-cooperation
    //                                 
    // and good cause.  The mandatory
    //                                 
    // associations were not part of
    // the
    //                                 
    // create statements and caused
    //                                 
    // consistency check ERRORs.
    // -------------------------------------------------------------------
    // 06/21/99 W.Campbell        Modified the properties
    //                            of 4 READ statements to
    //                            Select Only.
    // ---------------------------------------------------------
    // 07/26/10 JHuss  CQ 376    Added infrastructure record in the event no 
    // case
    // 			  unit is found when adding non-coop or good cause.
    // ---------------------------------------------------------
    // 09/24/2010   Joyce Harden   CQ 21624  Remove spaces so date fits on the
    //                                       3rd line when reason code 
    // noncoopsettoy is used.
    // -------------------------------------------------------------------------------------------
    MoveCase2(import.Case1, export.Case1);
    export.Ap.Number = import.Ap.Number;
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    if (!IsEmpty(import.GoodCause.Code))
    {
      if (Equal(import.GoodCause.Code, "NO"))
      {
        local.GoodCause.Code = "CO";
      }
      else
      {
        local.GoodCause.Code = import.GoodCause.Code ?? "";
      }
    }

    if (!IsEmpty(import.NonCooperation.Code))
    {
      if (AsChar(import.NonCooperation.Code) == 'N')
      {
        local.NonCooperation.Code = "Y";
      }
      else
      {
        local.NonCooperation.Code = "C";
      }
    }

    // ---------------------------------------------------------
    // 06/21/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // ---------------------------------------------------------
    // 06/21/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    ReadCaseRoleCsePerson();

    if (!entities.ArCsePerson.Populated)
    {
      ExitState = "AR_NF_RB";

      return;
    }

    if (!IsEmpty(import.Ap.Number))
    {
      // ---------------------------------------------------------
      // 06/21/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCsePerson())
      {
        // ---------------------------------------------------------
        // 06/21/99 W.Campbell - Modified the properties
        // of the following READ statement to Select Only.
        // ---------------------------------------------------------
        if (ReadCaseRole2())
        {
          MoveCaseRole1(entities.ApCaseRole, export.CaseRole);
        }

        if (!entities.ApCaseRole.Populated)
        {
          ExitState = "AP_FOR_CASE_NF";

          return;
        }
      }
      else
      {
        ExitState = "AP_FOR_CASE_NF";

        return;
      }
    }

    if (AsChar(import.Row1AddFuncNc.Flag) == 'Y')
    {
      // -------------------------------------------------------------------
      // 04/08/99 W.Campbell - Fixed consistency
      // check errors dealing with the
      // create of both non-cooperation
      // and good cause.  The mandatory
      // associations were not part of the
      // create statements and caused
      // consistency check ERRORs.
      // -------------------------------------------------------------------
      if (AsChar(entities.Case1.Status) == 'C')
      {
        if (AsChar(import.NonCooperation.Code) == 'C' || AsChar
          (import.NonCooperation.Code) == 'N')
        {
        }
        else
        {
          ExitState = "CASE_CLOSED_NON_COOP";

          return;
        }
      }

      local.FirstFlag.Flag = "";
      local.EndDate.Date = local.Max.Date;
      local.Start.Date = import.NonCooperation.EffectiveDate;

      foreach(var item in ReadCaseRole3())
      {
        if (IsEmpty(local.FirstFlag.Flag))
        {
          local.Start.Timestamp = Now();

          try
          {
            CreateNonCooperation();
            local.FirstFlag.Flag = "Y";

            if (!IsEmpty(import.Ap.Number))
            {
              if (ReadCaseRole1())
              {
                AssociateNonCooperation();

                goto Test1;
              }

              ExitState = "AP_FOR_CASE_NF";

              return;
            }

Test1:

            if (ReadNonCooperation1())
            {
              local.Compare.Timestamp =
                entities.ReadUpdateNonCooperation.CreatedTimestamp;
            }

            foreach(var item1 in ReadNonCooperation3())
            {
              if (Equal(local.Compare.Timestamp,
                entities.ReadUpdateNonCooperation.CreatedTimestamp))
              {
                continue;
              }

              // end date the previous record
              try
              {
                UpdateNonCooperation();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "NCP_NON_COOP_NU";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "NON_COOP_PVV";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              break;
            }

            local.DateWorkArea.Date = import.NonCooperation.EffectiveDate;

            // ***	Begin Event insertion	***
            if (ReadInterstateRequest())
            {
              if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
              {
                local.Infrastructure.InitiatingStateCode = "KS";
              }
              else
              {
                local.Infrastructure.InitiatingStateCode = "OS";
              }
            }
            else
            {
              local.Infrastructure.InitiatingStateCode = "KS";
            }

            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.EventId = 46;
            local.Infrastructure.BusinessObjectCd = "CAU";
            local.Infrastructure.CaseNumber = import.Case1.Number;
            local.Infrastructure.UserId = "CADS";
            local.Infrastructure.ReferenceDate = local.DateWorkArea.Date;
            UseCabConvertDate2String();

            if (AsChar(local.NonCooperation.Code) == 'C')
            {
              local.Infrastructure.ReasonCode = "NONCOOPSETTOC";
            }
            else if (AsChar(local.NonCooperation.Code) == 'Y')
            {
              local.Infrastructure.ReasonCode = "NONCOOPSETTOY";
            }
            else if (AsChar(local.NonCooperation.Code) == 'N')
            {
              local.Infrastructure.ReasonCode = "NONCOOPSETTON";
            }

            if (!IsEmpty(import.Ap.Number))
            {
              local.TextWorkArea.Text30 = "Non Coop determined for AP:";
              local.Infrastructure.CsePersonNumber =
                entities.ApCsePerson.Number;
              local.Infrastructure.Detail =
                TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                (entities.ApCsePerson.Number) + "; " + TrimEnd("Type:") + TrimEnd
                (import.NonCooperation.Code) + "; Reason:" + (
                  import.NonCooperation.Reason ?? "") + "; Date:" + local
                .TextWorkArea.Text8;
              local.Infrastructure.CreatedBy = global.UserId;
              local.Infrastructure.CreatedTimestamp = Now();
              local.CaseUnitFound.Flag = "N";

              foreach(var item1 in ReadCaseUnit1())
              {
                local.Infrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
                local.CaseUnitFound.Flag = "Y";
                UseSpCabCreateInfrastructure();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
                else
                {
                  return;
                }
              }

              // 07/26/10 JHuss CQ# 376	Added check to create infrastructure if 
              // no case unit found
              if (AsChar(local.CaseUnitFound.Flag) == 'N')
              {
                UseSpCabCreateInfrastructure();
              }
            }
            else
            {
              local.TextWorkArea.Text30 = "Non Coop established for AR:";
              local.Infrastructure.CsePersonNumber =
                entities.ArCsePerson.Number;
              local.Infrastructure.Detail =
                TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                (entities.ArCsePerson.Number) + "; " + TrimEnd("Type:") + TrimEnd
                (import.NonCooperation.Code) + "; Reason:" + (
                  import.NonCooperation.Reason ?? "") + "; Date:" + local
                .TextWorkArea.Text8;
              local.Infrastructure.CreatedBy = global.UserId;
              local.Infrastructure.CreatedTimestamp = Now();
              local.CaseUnitFound.Flag = "N";

              foreach(var item1 in ReadCaseUnit2())
              {
                local.Infrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
                local.CaseUnitFound.Flag = "Y";
                UseSpCabCreateInfrastructure();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
                else
                {
                  return;
                }
              }

              // 07/26/10 JHuss CQ# 376	Added check to create infrastructure if 
              // no case unit found
              if (AsChar(local.CaseUnitFound.Flag) == 'N')
              {
                UseSpCabCreateInfrastructure();
              }
            }

            // ***	End Event insertion	***
            // ***	Begin External Alert insertion	***
            if (!IsEmpty(local.NonCooperation.Code))
            {
              if (AsChar(local.NonCooperation.Code) == 'Y')
              {
                local.InterfaceAlert.AlertCode = "40";
              }
              else
              {
                local.InterfaceAlert.AlertCode = "41";
              }

              UseSpCadsExternalAlert();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            // ***	End External Alert insertion	***
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "NON_COOP_ALREADY_EXISTS";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "NON_COOP_PVV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          local.Start.Timestamp = AddMicroseconds(local.Start.Timestamp, -1);
          ReadNonCooperation2();

          if (!entities.ReadUpdateNonCooperation.Populated)
          {
            continue;
          }

          local.EndDate.Date = import.NonCooperation.EffectiveDate;
          local.Start.Date = import.NonCooperation.EffectiveDate;

          try
          {
            CreateNonCooperation();

            if (!IsEmpty(import.Ap.Number))
            {
              if (ReadCaseRole1())
              {
                AssociateNonCooperation();

                goto Test2;
              }

              ExitState = "AP_FOR_CASE_NF";

              return;
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "NON_COOP_ALREADY_EXISTS";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "NON_COOP_PVV";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

Test2:
        ;
      }
    }

    if (AsChar(import.Row1AddFuncGc.Flag) == 'Y')
    {
      // -------------------------------------------------------------------
      // 04/08/99 W.Campbell - Fixed consistency
      // check errors dealing with the
      // create of both non-cooperation
      // and good cause.  The mandatory
      // associations were not part of the
      // create statements and caused
      // consistency check ERRORs.
      // -------------------------------------------------------------------
      local.FirstFlag.Flag = "";
      local.EndDate.Date = local.Max.Date;
      local.Start.Date = import.GoodCause.EffectiveDate;

      foreach(var item in ReadCaseRole3())
      {
        if (IsEmpty(local.FirstFlag.Flag))
        {
          try
          {
            CreateGoodCause();
            local.FirstFlag.Flag = "Y";

            if (!IsEmpty(import.Ap.Number))
            {
              if (ReadCaseRole1())
              {
                AssociateGoodCause();

                goto Test3;
              }

              ExitState = "AP_FOR_CASE_NF";

              return;
            }

Test3:

            if (ReadGoodCause1())
            {
              local.Compare.Timestamp =
                entities.ReadUpdateGoodCause.CreatedTimestamp;
            }

            foreach(var item1 in ReadGoodCause3())
            {
              if (Equal(local.Compare.Timestamp,
                entities.ReadUpdateGoodCause.CreatedTimestamp))
              {
                continue;
              }

              try
              {
                UpdateGoodCause();
              }
              catch(Exception e1)
              {
                switch(GetErrorCode(e1))
                {
                  case ErrorCode.AlreadyExists:
                    ExitState = "GOOD_CAUSE_NOT_UNIQUE";

                    return;
                  case ErrorCode.PermittedValueViolation:
                    ExitState = "GOOD_CAUSE_PV_RB";

                    return;
                  case ErrorCode.DatabaseError:
                    break;
                  default:
                    throw;
                }
              }

              break;
            }

            local.DateWorkArea.Date = import.GoodCause.EffectiveDate;

            // ***	Begin Event insertion	***
            if (ReadInterstateRequest())
            {
              if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
              {
                local.Infrastructure.InitiatingStateCode = "KS";
              }
              else
              {
                local.Infrastructure.InitiatingStateCode = "OS";
              }
            }
            else
            {
              local.Infrastructure.InitiatingStateCode = "KS";
            }

            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.EventId = 46;
            local.Infrastructure.BusinessObjectCd = "CAU";
            local.Infrastructure.CaseNumber = import.Case1.Number;
            local.Infrastructure.UserId = "CADS";
            local.Infrastructure.ReferenceDate = import.GoodCause.EffectiveDate;

            if (Equal(local.GoodCause.Code, "CO"))
            {
              local.Infrastructure.ReasonCode = "GOODCAUSETOCOOP";
            }
            else if (Equal(local.GoodCause.Code, "PD"))
            {
              local.Infrastructure.ReasonCode = "GOODCAUSEPD";
            }
            else if (Equal(local.GoodCause.Code, "GC"))
            {
              local.Infrastructure.ReasonCode = "GOODCAUSE";
            }

            UseCabConvertDate2String();

            if (!IsEmpty(import.Ap.Number))
            {
              local.TextWorkArea.Text30 = "Good Cause determined for AP:";
              local.Infrastructure.CsePersonNumber =
                entities.ApCsePerson.Number;
              local.TextWorkArea.Text10 = "; Type:";
              local.Infrastructure.Detail =
                TrimEnd(local.TextWorkArea.Text30) + entities
                .ApCsePerson.Number + TrimEnd(local.TextWorkArea.Text10) + TrimEnd
                (import.GoodCause.Code) + "; Date:" + local.TextWorkArea.Text8;
              local.Infrastructure.CreatedBy = global.UserId;
              local.Infrastructure.CreatedTimestamp = Now();
              local.CaseUnitFound.Flag = "N";

              foreach(var item1 in ReadCaseUnit1())
              {
                local.Infrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
                local.CaseUnitFound.Flag = "Y";
                UseSpCabCreateInfrastructure();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
                else
                {
                  return;
                }
              }

              // 07/26/10 JHuss CQ# 376	Added check to create infrastructure if 
              // no case unit found
              if (AsChar(local.CaseUnitFound.Flag) == 'N')
              {
                UseSpCabCreateInfrastructure();
              }
            }
            else
            {
              local.TextWorkArea.Text30 = "Good Cause established for AR:";
              local.Infrastructure.CsePersonNumber =
                entities.ArCsePerson.Number;
              local.TextWorkArea.Text10 = "; Type:";
              local.Infrastructure.Detail =
                TrimEnd(local.TextWorkArea.Text30) + entities
                .ArCsePerson.Number + TrimEnd(local.TextWorkArea.Text10) + TrimEnd
                (import.GoodCause.Code) + "; Date:" + local.TextWorkArea.Text8;
              local.Infrastructure.CreatedBy = global.UserId;
              local.Infrastructure.CreatedTimestamp = Now();
              local.CaseUnitFound.Flag = "N";

              foreach(var item1 in ReadCaseUnit2())
              {
                local.Infrastructure.CaseUnitNumber =
                  entities.CaseUnit.CuNumber;
                local.CaseUnitFound.Flag = "Y";
                UseSpCabCreateInfrastructure();

                if (IsExitState("ACO_NN0000_ALL_OK"))
                {
                }
                else
                {
                  return;
                }
              }

              // 07/26/10 JHuss CQ# 376	Added check to create infrastructure if 
              // no case unit found
              if (AsChar(local.CaseUnitFound.Flag) == 'N')
              {
                UseSpCabCreateInfrastructure();
              }
            }

            // ***	End Event insertion	***
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "GOOD_CAUSE_ALREADY_EXISTS";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "GOOD_CAUSE_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }
        else
        {
          ReadGoodCause2();

          if (!entities.ReadUpdateGoodCause.Populated)
          {
            continue;
          }

          local.EndDate.Date = import.GoodCause.EffectiveDate;
          local.Start.Date = import.GoodCause.EffectiveDate;

          try
          {
            CreateGoodCause();

            if (!IsEmpty(import.Ap.Number))
            {
              if (ReadCaseRole1())
              {
                AssociateGoodCause();

                goto Test4;
              }

              ExitState = "AP_FOR_CASE_NF";

              return;
            }
          }
          catch(Exception e)
          {
            switch(GetErrorCode(e))
            {
              case ErrorCode.AlreadyExists:
                ExitState = "GOOD_CAUSE_ALREADY_EXISTS";

                return;
              case ErrorCode.PermittedValueViolation:
                ExitState = "GOOD_CAUSE_PV_RB";

                return;
              case ErrorCode.DatabaseError:
                break;
              default:
                throw;
            }
          }
        }

Test4:
        ;
      }
    }
  }

  private static void MoveCase2(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.ClosureReason = source.ClosureReason;
    target.Status = source.Status;
    target.StatusDate = source.StatusDate;
    target.Note = source.Note;
  }

  private static void MoveCase3(Case1 source, Case1 target)
  {
    target.Number = source.Number;
    target.Status = source.Status;
  }

  private static void MoveCaseRole1(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
  }

  private static void MoveCaseRole2(CaseRole source, CaseRole target)
  {
    target.Identifier = source.Identifier;
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private static void MoveCaseRole3(CaseRole source, CaseRole target)
  {
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
  }

  private void UseCabConvertDate2String()
  {
    var useImport = new CabConvertDate2String.Import();
    var useExport = new CabConvertDate2String.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabConvertDate2String.Execute, useImport, useExport);

    local.TextWorkArea.Text8 = useExport.TextWorkArea.Text8;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private void UseSpCadsExternalAlert()
  {
    var useImport = new SpCadsExternalAlert.Import();
    var useExport = new SpCadsExternalAlert.Export();

    useImport.NonCooperation.Reason = entities.NonCooperation.Reason;
    MoveCase3(export.Case1, useImport.Case1);
    MoveCaseRole2(entities.ApCaseRole, useImport.Ap);
    useImport.InterfaceAlert.AlertCode = local.InterfaceAlert.AlertCode;
    MoveCaseRole3(entities.ArCaseRole, useImport.Ar);

    Call(SpCadsExternalAlert.Execute, useImport, useExport);
  }

  private void AssociateGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.GoodCause.Populated);
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);

    var casNumber1 = entities.ApCaseRole.CasNumber;
    var cspNumber1 = entities.ApCaseRole.CspNumber;
    var croType1 = entities.ApCaseRole.Type1;
    var croIdentifier1 = entities.ApCaseRole.Identifier;

    CheckValid<GoodCause>("CroType1", croType1);
    entities.GoodCause.Populated = false;
    Update("AssociateGoodCause",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber1);
        db.SetNullableString(command, "cspNumber1", cspNumber1);
        db.SetNullableString(command, "croType1", croType1);
        db.SetNullableInt32(command, "croIdentifier1", croIdentifier1);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.GoodCause.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.GoodCause.CasNumber);
        db.SetString(command, "cspNumber", entities.GoodCause.CspNumber);
        db.SetString(command, "croType", entities.GoodCause.CroType);
        db.SetInt32(command, "croIdentifier", entities.GoodCause.CroIdentifier);
      });

    entities.GoodCause.CasNumber1 = casNumber1;
    entities.GoodCause.CspNumber1 = cspNumber1;
    entities.GoodCause.CroType1 = croType1;
    entities.GoodCause.CroIdentifier1 = croIdentifier1;
    entities.GoodCause.Populated = true;
  }

  private void AssociateNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.NonCooperation.Populated);
    System.Diagnostics.Debug.Assert(entities.ApCaseRole.Populated);

    var casNumber1 = entities.ApCaseRole.CasNumber;
    var cspNumber1 = entities.ApCaseRole.CspNumber;
    var croType1 = entities.ApCaseRole.Type1;
    var croIdentifier1 = entities.ApCaseRole.Identifier;

    CheckValid<NonCooperation>("CroType1", croType1);
    entities.NonCooperation.Populated = false;
    Update("AssociateNonCooperation",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber1", casNumber1);
        db.SetNullableString(command, "cspNumber1", cspNumber1);
        db.SetNullableString(command, "croType1", croType1);
        db.SetNullableInt32(command, "croIdentifier1", croIdentifier1);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.NonCooperation.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.NonCooperation.CasNumber);
        db.SetString(command, "cspNumber", entities.NonCooperation.CspNumber);
        db.SetString(command, "croType", entities.NonCooperation.CroType);
        db.SetInt32(
          command, "croIdentifier", entities.NonCooperation.CroIdentifier);
      });

    entities.NonCooperation.CasNumber1 = casNumber1;
    entities.NonCooperation.CspNumber1 = cspNumber1;
    entities.NonCooperation.CroType1 = croType1;
    entities.NonCooperation.CroIdentifier1 = croIdentifier1;
    entities.NonCooperation.Populated = true;
  }

  private void CreateGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.ArCaseRole.Populated);

    var code = local.GoodCause.Code ?? "";
    var effectiveDate = local.Start.Date;
    var discontinueDate = local.EndDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = Now();
    var casNumber = entities.ArCaseRole.CasNumber;
    var cspNumber = entities.ArCaseRole.CspNumber;
    var croType = entities.ArCaseRole.Type1;
    var croIdentifier = entities.ArCaseRole.Identifier;

    CheckValid<GoodCause>("CroType", croType);
    entities.GoodCause.Populated = false;
    Update("CreateGoodCause",
      (db, command) =>
      {
        db.SetNullableString(command, "code", code);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.SetNullableDateTime(command, "lastUpdatedTmst", createdTimestamp);
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "croIdentifier", croIdentifier);
      });

    entities.GoodCause.Code = code;
    entities.GoodCause.EffectiveDate = effectiveDate;
    entities.GoodCause.DiscontinueDate = discontinueDate;
    entities.GoodCause.CreatedBy = createdBy;
    entities.GoodCause.CreatedTimestamp = createdTimestamp;
    entities.GoodCause.LastUpdatedBy = createdBy;
    entities.GoodCause.LastUpdatedTimestamp = createdTimestamp;
    entities.GoodCause.CasNumber = casNumber;
    entities.GoodCause.CspNumber = cspNumber;
    entities.GoodCause.CroType = croType;
    entities.GoodCause.CroIdentifier = croIdentifier;
    entities.GoodCause.CasNumber1 = null;
    entities.GoodCause.CspNumber1 = null;
    entities.GoodCause.CroType1 = null;
    entities.GoodCause.CroIdentifier1 = null;
    entities.GoodCause.Populated = true;
  }

  private void CreateNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.ArCaseRole.Populated);

    var code = local.NonCooperation.Code ?? "";
    var reason = import.NonCooperation.Reason ?? "";
    var effectiveDate = local.Start.Date;
    var discontinueDate = local.EndDate.Date;
    var createdBy = global.UserId;
    var createdTimestamp = local.Start.Timestamp;
    var lastUpdatedTimestamp = Now();
    var casNumber = entities.ArCaseRole.CasNumber;
    var cspNumber = entities.ArCaseRole.CspNumber;
    var croType = entities.ArCaseRole.Type1;
    var croIdentifier = entities.ArCaseRole.Identifier;

    CheckValid<NonCooperation>("CroType", croType);
    entities.NonCooperation.Populated = false;
    Update("CreateNonCooperation",
      (db, command) =>
      {
        db.SetNullableString(command, "code", code);
        db.SetNullableString(command, "reason", reason);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", createdBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetString(command, "casNumber", casNumber);
        db.SetString(command, "cspNumber", cspNumber);
        db.SetString(command, "croType", croType);
        db.SetInt32(command, "croIdentifier", croIdentifier);
      });

    entities.NonCooperation.Code = code;
    entities.NonCooperation.Reason = reason;
    entities.NonCooperation.EffectiveDate = effectiveDate;
    entities.NonCooperation.DiscontinueDate = discontinueDate;
    entities.NonCooperation.CreatedBy = createdBy;
    entities.NonCooperation.CreatedTimestamp = createdTimestamp;
    entities.NonCooperation.LastUpdatedBy = createdBy;
    entities.NonCooperation.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NonCooperation.CasNumber = casNumber;
    entities.NonCooperation.CspNumber = cspNumber;
    entities.NonCooperation.CroType = croType;
    entities.NonCooperation.CroIdentifier = croIdentifier;
    entities.NonCooperation.CasNumber1 = null;
    entities.NonCooperation.CspNumber1 = null;
    entities.NonCooperation.CroType1 = null;
    entities.NonCooperation.CroIdentifier1 = null;
    entities.NonCooperation.Populated = true;
  }

  private bool ReadCase()
  {
    entities.Case1.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Case1.FullServiceWithoutMedInd =
          db.GetNullableString(reader, 0);
        entities.Case1.FullServiceWithMedInd = db.GetNullableString(reader, 1);
        entities.Case1.LocateInd = db.GetNullableString(reader, 2);
        entities.Case1.ClosureReason = db.GetNullableString(reader, 3);
        entities.Case1.Number = db.GetString(reader, 4);
        entities.Case1.Status = db.GetNullableString(reader, 5);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 6);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 7);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 8);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 9);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 10);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 11);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 12);
        entities.Case1.Note = db.GetNullableString(reader, 13);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole1()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "endDate",
          entities.ArCaseRole.StartDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate",
          entities.ArCaseRole.EndDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.AssignmentDate = db.GetNullableDate(reader, 6);
        entities.ApCaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 7);
        entities.ApCaseRole.AssignmentOfRights =
          db.GetNullableString(reader, 8);
        entities.ApCaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 9);
        entities.ApCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.ApCaseRole.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.ApCaseRole.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ApCaseRole.CreatedBy = db.GetString(reader, 13);
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private bool ReadCaseRole2()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.AssignmentDate = db.GetNullableDate(reader, 6);
        entities.ApCaseRole.AssignmentTerminationCode =
          db.GetNullableString(reader, 7);
        entities.ApCaseRole.AssignmentOfRights =
          db.GetNullableString(reader, 8);
        entities.ApCaseRole.AssignmentTerminatedDt =
          db.GetNullableDate(reader, 9);
        entities.ApCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 10);
        entities.ApCaseRole.LastUpdatedBy = db.GetNullableString(reader, 11);
        entities.ApCaseRole.CreatedTimestamp = db.GetDateTime(reader, 12);
        entities.ApCaseRole.CreatedBy = db.GetString(reader, 13);
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseRole3()
  {
    entities.ArCaseRole.Populated = false;

    return ReadEach("ReadCaseRole3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ArCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);

        return true;
      });
  }

  private bool ReadCaseRoleCsePerson()
  {
    entities.ArCaseRole.Populated = false;
    entities.ArCsePerson.Populated = false;

    return Read("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "numb", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.ArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ArCsePerson.Number = db.GetString(reader, 1);
        entities.ArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ArCsePerson.Type1 = db.GetString(reader, 6);
        entities.ArCsePerson.OrganizationName = db.GetNullableString(reader, 7);
        entities.ArCaseRole.Populated = true;
        entities.ArCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ArCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ArCsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCaseUnit1()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAp", entities.ApCsePerson.Number);
        db.SetNullableString(command, "cspNoAr", entities.ArCsePerson.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit2()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAr", entities.ArCsePerson.Number);
        db.
          SetDate(command, "startDate", local.Current.Date.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.CaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    entities.ApCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.OrganizationName = db.GetNullableString(reader, 2);
        entities.ApCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
      });
  }

  private bool ReadGoodCause1()
  {
    entities.ReadUpdateGoodCause.Populated = false;

    return Read("ReadGoodCause1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ReadUpdateGoodCause.EffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.ReadUpdateGoodCause.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ReadUpdateGoodCause.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ReadUpdateGoodCause.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ReadUpdateGoodCause.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ReadUpdateGoodCause.CasNumber = db.GetString(reader, 5);
        entities.ReadUpdateGoodCause.CspNumber = db.GetString(reader, 6);
        entities.ReadUpdateGoodCause.CroType = db.GetString(reader, 7);
        entities.ReadUpdateGoodCause.CroIdentifier = db.GetInt32(reader, 8);
        entities.ReadUpdateGoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.ReadUpdateGoodCause.CroType);
      });
  }

  private bool ReadGoodCause2()
  {
    System.Diagnostics.Debug.Assert(entities.ArCaseRole.Populated);
    entities.ReadUpdateGoodCause.Populated = false;

    return Read("ReadGoodCause2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ArCaseRole.CasNumber);
        db.SetString(command, "cspNumber", entities.ArCaseRole.CspNumber);
        db.SetString(command, "croType", entities.ArCaseRole.Type1);
        db.SetInt32(command, "croIdentifier", entities.ArCaseRole.Identifier);
      },
      (db, reader) =>
      {
        entities.ReadUpdateGoodCause.EffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.ReadUpdateGoodCause.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ReadUpdateGoodCause.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ReadUpdateGoodCause.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ReadUpdateGoodCause.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ReadUpdateGoodCause.CasNumber = db.GetString(reader, 5);
        entities.ReadUpdateGoodCause.CspNumber = db.GetString(reader, 6);
        entities.ReadUpdateGoodCause.CroType = db.GetString(reader, 7);
        entities.ReadUpdateGoodCause.CroIdentifier = db.GetInt32(reader, 8);
        entities.ReadUpdateGoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.ReadUpdateGoodCause.CroType);
      });
  }

  private IEnumerable<bool> ReadGoodCause3()
  {
    entities.ReadUpdateGoodCause.Populated = false;

    return ReadEach("ReadGoodCause3",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ReadUpdateGoodCause.EffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.ReadUpdateGoodCause.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ReadUpdateGoodCause.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ReadUpdateGoodCause.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ReadUpdateGoodCause.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ReadUpdateGoodCause.CasNumber = db.GetString(reader, 5);
        entities.ReadUpdateGoodCause.CspNumber = db.GetString(reader, 6);
        entities.ReadUpdateGoodCause.CroType = db.GetString(reader, 7);
        entities.ReadUpdateGoodCause.CroIdentifier = db.GetInt32(reader, 8);
        entities.ReadUpdateGoodCause.Populated = true;
        CheckValid<GoodCause>("CroType", entities.ReadUpdateGoodCause.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.InterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 1);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 2);
        entities.InterstateRequest.Populated = true;
      });
  }

  private bool ReadNonCooperation1()
  {
    entities.ReadUpdateNonCooperation.Populated = false;

    return Read("ReadNonCooperation1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ReadUpdateNonCooperation.EffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.ReadUpdateNonCooperation.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ReadUpdateNonCooperation.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ReadUpdateNonCooperation.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ReadUpdateNonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ReadUpdateNonCooperation.CasNumber = db.GetString(reader, 5);
        entities.ReadUpdateNonCooperation.CspNumber = db.GetString(reader, 6);
        entities.ReadUpdateNonCooperation.CroType = db.GetString(reader, 7);
        entities.ReadUpdateNonCooperation.CroIdentifier =
          db.GetInt32(reader, 8);
        entities.ReadUpdateNonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType",
          entities.ReadUpdateNonCooperation.CroType);
      });
  }

  private bool ReadNonCooperation2()
  {
    System.Diagnostics.Debug.Assert(entities.ArCaseRole.Populated);
    entities.ReadUpdateNonCooperation.Populated = false;

    return Read("ReadNonCooperation2",
      (db, command) =>
      {
        db.SetInt32(command, "croIdentifier", entities.ArCaseRole.Identifier);
        db.SetString(command, "croType", entities.ArCaseRole.Type1);
        db.SetString(command, "cspNumber", entities.ArCaseRole.CspNumber);
        db.SetString(command, "casNumber", entities.ArCaseRole.CasNumber);
      },
      (db, reader) =>
      {
        entities.ReadUpdateNonCooperation.EffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.ReadUpdateNonCooperation.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ReadUpdateNonCooperation.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ReadUpdateNonCooperation.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ReadUpdateNonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ReadUpdateNonCooperation.CasNumber = db.GetString(reader, 5);
        entities.ReadUpdateNonCooperation.CspNumber = db.GetString(reader, 6);
        entities.ReadUpdateNonCooperation.CroType = db.GetString(reader, 7);
        entities.ReadUpdateNonCooperation.CroIdentifier =
          db.GetInt32(reader, 8);
        entities.ReadUpdateNonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType",
          entities.ReadUpdateNonCooperation.CroType);
      });
  }

  private IEnumerable<bool> ReadNonCooperation3()
  {
    entities.ReadUpdateNonCooperation.Populated = false;

    return ReadEach("ReadNonCooperation3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ArCsePerson.Number);
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ReadUpdateNonCooperation.EffectiveDate =
          db.GetNullableDate(reader, 0);
        entities.ReadUpdateNonCooperation.DiscontinueDate =
          db.GetNullableDate(reader, 1);
        entities.ReadUpdateNonCooperation.CreatedTimestamp =
          db.GetDateTime(reader, 2);
        entities.ReadUpdateNonCooperation.LastUpdatedBy =
          db.GetNullableString(reader, 3);
        entities.ReadUpdateNonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 4);
        entities.ReadUpdateNonCooperation.CasNumber = db.GetString(reader, 5);
        entities.ReadUpdateNonCooperation.CspNumber = db.GetString(reader, 6);
        entities.ReadUpdateNonCooperation.CroType = db.GetString(reader, 7);
        entities.ReadUpdateNonCooperation.CroIdentifier =
          db.GetInt32(reader, 8);
        entities.ReadUpdateNonCooperation.Populated = true;
        CheckValid<NonCooperation>("CroType",
          entities.ReadUpdateNonCooperation.CroType);

        return true;
      });
  }

  private void UpdateGoodCause()
  {
    System.Diagnostics.Debug.Assert(entities.ReadUpdateGoodCause.Populated);

    var discontinueDate = import.GoodCause.EffectiveDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ReadUpdateGoodCause.Populated = false;
    Update("UpdateGoodCause",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReadUpdateGoodCause.CreatedTimestamp.GetValueOrDefault());
        db.SetString(
          command, "casNumber", entities.ReadUpdateGoodCause.CasNumber);
        db.SetString(
          command, "cspNumber", entities.ReadUpdateGoodCause.CspNumber);
        db.SetString(command, "croType", entities.ReadUpdateGoodCause.CroType);
        db.SetInt32(
          command, "croIdentifier", entities.ReadUpdateGoodCause.CroIdentifier);
          
      });

    entities.ReadUpdateGoodCause.DiscontinueDate = discontinueDate;
    entities.ReadUpdateGoodCause.LastUpdatedBy = lastUpdatedBy;
    entities.ReadUpdateGoodCause.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.ReadUpdateGoodCause.Populated = true;
  }

  private void UpdateNonCooperation()
  {
    System.Diagnostics.Debug.
      Assert(entities.ReadUpdateNonCooperation.Populated);

    var discontinueDate = import.NonCooperation.EffectiveDate;
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();

    entities.ReadUpdateNonCooperation.Populated = false;
    Update("UpdateNonCooperation",
      (db, command) =>
      {
        db.SetNullableDate(command, "discontinueDate", discontinueDate);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTimes", lastUpdatedTimestamp);
          
        db.SetDateTime(
          command, "createdTimestamp",
          entities.ReadUpdateNonCooperation.CreatedTimestamp.
            GetValueOrDefault());
        db.SetString(
          command, "casNumber", entities.ReadUpdateNonCooperation.CasNumber);
        db.SetString(
          command, "cspNumber", entities.ReadUpdateNonCooperation.CspNumber);
        db.SetString(
          command, "croType", entities.ReadUpdateNonCooperation.CroType);
        db.SetInt32(
          command, "croIdentifier",
          entities.ReadUpdateNonCooperation.CroIdentifier);
      });

    entities.ReadUpdateNonCooperation.DiscontinueDate = discontinueDate;
    entities.ReadUpdateNonCooperation.LastUpdatedBy = lastUpdatedBy;
    entities.ReadUpdateNonCooperation.LastUpdatedTimestamp =
      lastUpdatedTimestamp;
    entities.ReadUpdateNonCooperation.Populated = true;
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
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of Row1AddFuncGc.
    /// </summary>
    [JsonPropertyName("row1AddFuncGc")]
    public Common Row1AddFuncGc
    {
      get => row1AddFuncGc ??= new();
      set => row1AddFuncGc = value;
    }

    /// <summary>
    /// A value of Row1AddFuncNc.
    /// </summary>
    [JsonPropertyName("row1AddFuncNc")]
    public Common Row1AddFuncNc
    {
      get => row1AddFuncNc ??= new();
      set => row1AddFuncNc = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePersonsWorkSet Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    private NonCooperation nonCooperation;
    private GoodCause goodCause;
    private Common row1AddFuncGc;
    private Common row1AddFuncNc;
    private CsePersonsWorkSet ap;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePersonsWorkSet ar;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private CsePersonsWorkSet ap;
    private CaseRole caseRole;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Start.
    /// </summary>
    [JsonPropertyName("start")]
    public DateWorkArea Start
    {
      get => start ??= new();
      set => start = value;
    }

    /// <summary>
    /// A value of EndDate.
    /// </summary>
    [JsonPropertyName("endDate")]
    public DateWorkArea EndDate
    {
      get => endDate ??= new();
      set => endDate = value;
    }

    /// <summary>
    /// A value of FirstFlag.
    /// </summary>
    [JsonPropertyName("firstFlag")]
    public Common FirstFlag
    {
      get => firstFlag ??= new();
      set => firstFlag = value;
    }

    /// <summary>
    /// A value of Compare.
    /// </summary>
    [JsonPropertyName("compare")]
    public DateWorkArea Compare
    {
      get => compare ??= new();
      set => compare = value;
    }

    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ErrOnAdabas.
    /// </summary>
    [JsonPropertyName("errOnAdabas")]
    public Common ErrOnAdabas
    {
      get => errOnAdabas ??= new();
      set => errOnAdabas = value;
    }

    /// <summary>
    /// A value of Row1AddFuncGc.
    /// </summary>
    [JsonPropertyName("row1AddFuncGc")]
    public Common Row1AddFuncGc
    {
      get => row1AddFuncGc ??= new();
      set => row1AddFuncGc = value;
    }

    /// <summary>
    /// A value of Row1AddFuncNc.
    /// </summary>
    [JsonPropertyName("row1AddFuncNc")]
    public Common Row1AddFuncNc
    {
      get => row1AddFuncNc ??= new();
      set => row1AddFuncNc = value;
    }

    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of TextWorkArea.
    /// </summary>
    [JsonPropertyName("textWorkArea")]
    public TextWorkArea TextWorkArea
    {
      get => textWorkArea ??= new();
      set => textWorkArea = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    /// <summary>
    /// A value of CaseUnitFound.
    /// </summary>
    [JsonPropertyName("caseUnitFound")]
    public Common CaseUnitFound
    {
      get => caseUnitFound ??= new();
      set => caseUnitFound = value;
    }

    private DateWorkArea start;
    private DateWorkArea endDate;
    private Common firstFlag;
    private DateWorkArea compare;
    private NonCooperation nonCooperation;
    private GoodCause goodCause;
    private DateWorkArea max;
    private DateWorkArea dateWorkArea;
    private DateWorkArea current;
    private Common errOnAdabas;
    private Common row1AddFuncGc;
    private Common row1AddFuncNc;
    private Infrastructure infrastructure;
    private TextWorkArea textWorkArea;
    private InterfaceAlert interfaceAlert;
    private Common caseUnitFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ReadUpdateNonCooperation.
    /// </summary>
    [JsonPropertyName("readUpdateNonCooperation")]
    public NonCooperation ReadUpdateNonCooperation
    {
      get => readUpdateNonCooperation ??= new();
      set => readUpdateNonCooperation = value;
    }

    /// <summary>
    /// A value of ReadUpdateGoodCause.
    /// </summary>
    [JsonPropertyName("readUpdateGoodCause")]
    public GoodCause ReadUpdateGoodCause
    {
      get => readUpdateGoodCause ??= new();
      set => readUpdateGoodCause = value;
    }

    /// <summary>
    /// A value of NonCooperation.
    /// </summary>
    [JsonPropertyName("nonCooperation")]
    public NonCooperation NonCooperation
    {
      get => nonCooperation ??= new();
      set => nonCooperation = value;
    }

    /// <summary>
    /// A value of ArCaseRole.
    /// </summary>
    [JsonPropertyName("arCaseRole")]
    public CaseRole ArCaseRole
    {
      get => arCaseRole ??= new();
      set => arCaseRole = value;
    }

    /// <summary>
    /// A value of GoodCause.
    /// </summary>
    [JsonPropertyName("goodCause")]
    public GoodCause GoodCause
    {
      get => goodCause ??= new();
      set => goodCause = value;
    }

    /// <summary>
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
    }

    /// <summary>
    /// A value of ApCaseRole.
    /// </summary>
    [JsonPropertyName("apCaseRole")]
    public CaseRole ApCaseRole
    {
      get => apCaseRole ??= new();
      set => apCaseRole = value;
    }

    /// <summary>
    /// A value of ApCsePerson.
    /// </summary>
    [JsonPropertyName("apCsePerson")]
    public CsePerson ApCsePerson
    {
      get => apCsePerson ??= new();
      set => apCsePerson = value;
    }

    /// <summary>
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    /// <summary>
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
    }

    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private NonCooperation readUpdateNonCooperation;
    private GoodCause readUpdateGoodCause;
    private NonCooperation nonCooperation;
    private CaseRole arCaseRole;
    private GoodCause goodCause;
    private CsePerson arCsePerson;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
  }
#endregion
}

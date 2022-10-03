// Program: SI_UPDATE_EMPLOYER, ID: 371762202, model: 746.
// Short name: SWE01249
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
/// A program: SI_UPDATE_EMPLOYER.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiUpdateEmployer: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_EMPLOYER program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateEmployer(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateEmployer.
  /// </summary>
  public SiUpdateEmployer(IContext context, Import import, Export export):
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
    //     M A I N T E N A N C E    L O G
    //   Date   Developer   Description
    // 09-24-95 K Evans     Initial Development
    // ---------------------------------------------
    // 07/12/99  Marek Lachowicz	Change property of READ (Select Only)
    export.Employer.Assign(import.Employer);
    local.Null1.Date = new DateTime(1, 1, 1);
    local.Current.Date = Now().Date;
    UseCabSetMaximumDiscontinueDate();

    // -- WR10355-- Added references to zip_code and zip4 to support zip code 
    // edits to support changes to EMPL screen to validate edit of employers by
    // zip code.
    export.Employer.Assign(import.Employer);

    if (ReadEmployerEmployerAddress())
    {
      MoveEmployer(entities.Employer, local.Employer);
      local.EmployerHistoryDetail.LineNumber = 0;
      local.EmployerHistoryDetail.CreatedBy = global.UserId;
      local.EmployerHistoryDetail.CreatedTimestamp = Now();
      local.EmployerHistoryDetail.LastUpdatedBy = global.UserId;
      local.EmployerHistoryDetail.LastUpdatedTimestamp = Now();
      local.EmployerHistory.CreatedTimestamp = Now();
      local.EmployerHistory.CreatedBy = global.UserId;
      local.EmployerHistory.ActionTaken = "Update";
      local.EmployerHistory.ActionDate = Now().Date;
      local.EmployerHistory.LastUpdatedBy = global.UserId;

      if (!Equal(import.EmployerAddress.Street1,
        entities.EmployerAddress.Street1) || !
        Equal(import.EmployerAddress.Street2, entities.EmployerAddress.Street2) ||
        !Equal(import.EmployerAddress.City, entities.EmployerAddress.City) || !
        Equal(import.EmployerAddress.State, entities.EmployerAddress.State) || !
        Equal(import.EmployerAddress.ZipCode, entities.EmployerAddress.ZipCode) ||
        !Equal(import.EmployerAddress.Zip4, entities.EmployerAddress.Zip4) || !
        Equal(import.Employer.Name, entities.Employer.Name) || !
        Equal(import.Employer.Ein, entities.Employer.Ein) || !
        Equal(import.Employer.AreaCode.GetValueOrDefault(),
        entities.Employer.AreaCode) || !
        Equal(import.Employer.PhoneNo, entities.Employer.PhoneNo) || !
        Equal(import.Employer.EffectiveDate, entities.Employer.EffectiveDate) ||
        !Equal(import.Employer.EiwoEndDate, entities.Employer.EiwoEndDate) || !
        Equal(import.Employer.EiwoStartDate, entities.Employer.EiwoStartDate) ||
        !
        Equal(import.Employer.EmailAddress, entities.Employer.EmailAddress) || !
        Equal(import.Employer.EndDate, entities.Employer.EndDate) || !
        Equal(import.Employer.FaxAreaCode.GetValueOrDefault(),
        entities.Employer.FaxAreaCode) || !
        Equal(import.Employer.FaxPhoneNo, entities.Employer.FaxPhoneNo) || !
        Equal(import.Employer.KansasId, entities.Employer.KansasId) || !
        Equal(import.EmployerAddress.Note, entities.EmployerAddress.Note))
      {
        try
        {
          CreateEmployerHistory();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_PV";

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
        ExitState = "NO_CHANGES_WERE_MADE";

        return;
      }

      local.ChgFound.Flag = "";

      if (!Equal(import.EmployerAddress.Street1,
        entities.EmployerAddress.Street1))
      {
        local.EmployerHistoryDetail.Change =
          entities.EmployerAddress.Street1 + "   ;   " + (
            import.EmployerAddress.Street1 ?? "");
        local.EmployerHistoryDetail.LineNumber =
          entities.EmployerHistoryDetail.LineNumber + 1;
        local.ChgFound.Flag = "Y";

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.EmployerAddress.Street2,
        entities.EmployerAddress.Street2))
      {
        local.EmployerHistoryDetail.Change =
          entities.EmployerAddress.Street2 + "   ;   " + (
            import.EmployerAddress.Street2 ?? "");
        ++local.EmployerHistoryDetail.LineNumber;
        local.ChgFound.Flag = "Y";

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.EmployerAddress.Street3,
        entities.EmployerAddress.Street3))
      {
        local.EmployerHistoryDetail.Change =
          entities.EmployerAddress.Street3 + "   ;   " + (
            import.EmployerAddress.Street3 ?? "");
        ++local.EmployerHistoryDetail.LineNumber;
        local.ChgFound.Flag = "Y";

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.EmployerAddress.Street4,
        entities.EmployerAddress.Street4))
      {
        local.EmployerHistoryDetail.Change =
          entities.EmployerAddress.Street4 + "   ;   " + (
            import.EmployerAddress.Street4 ?? "");
        ++local.EmployerHistoryDetail.LineNumber;
        local.ChgFound.Flag = "Y";

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.EmployerAddress.City, entities.EmployerAddress.City) ||
        !
        Equal(import.EmployerAddress.State, entities.EmployerAddress.State) || !
        Equal(import.EmployerAddress.ZipCode, entities.EmployerAddress.ZipCode) ||
        !Equal(import.EmployerAddress.Zip4, entities.EmployerAddress.Zip4))
      {
        local.ChgFound.Flag = "Y";
        local.City1.Text33 = entities.EmployerAddress.City + ",  " + entities
          .EmployerAddress.State + "  " + entities.EmployerAddress.ZipCode + "-"
          + entities.EmployerAddress.Zip4;
        local.City2.Text33 = (import.EmployerAddress.City ?? "") + ",  " + (
          import.EmployerAddress.State ?? "") + "  " + (
            import.EmployerAddress.ZipCode ?? "") + "-" + (
            import.EmployerAddress.Zip4 ?? "");
        local.EmployerHistoryDetail.Change = local.City1.Text33 + "   ;   " + local
          .City2.Text33;
        ++local.EmployerHistoryDetail.LineNumber;

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(entities.EmployerAddress.Note, import.EmployerAddress.Note))
      {
        ++local.EmployerHistoryDetail.LineNumber;
        local.EmployerHistoryDetail.Change = "From: " + entities
          .EmployerAddress.Note;
        local.ChgFound.Flag = "Y";

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        ++local.EmployerHistoryDetail.LineNumber;
        local.Employer.EmailAddress = "To: " + (import.EmployerAddress.Note ?? ""
          );

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (AsChar(local.ChgFound.Flag) == 'Y')
      {
        try
        {
          UpdateEmployerAddress();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (IsEmpty(import.Employer.Ein))
      {
        local.Employer.EiwoStartDate = local.DateWorkArea.Date;
        local.Employer.EiwoEndDate = local.DateWorkArea.Date;
      }
      else if (Equal(entities.Employer.Ein, import.Employer.Ein))
      {
        local.Employer.EiwoStartDate = import.Employer.EiwoStartDate;
        local.Employer.EiwoEndDate = import.Employer.EiwoEndDate;
      }
      else if (ReadEmployer1())
      {
        local.Employer.EiwoStartDate = entities.N2dRead.EiwoStartDate;
        local.Employer.EiwoEndDate = entities.N2dRead.EiwoEndDate;
      }
      else
      {
        local.Employer.EiwoStartDate = entities.Employer.EiwoStartDate;
        local.Employer.EiwoEndDate = entities.Employer.EiwoEndDate;
      }

      local.ChgFound.Flag = "";

      if (!Equal(import.Employer.Ein, entities.Employer.Ein))
      {
        local.EmployerHistoryDetail.Change = entities.Employer.Ein + "   ;   " +
          (import.Employer.Ein ?? "");
        ++local.EmployerHistoryDetail.LineNumber;
        local.ChgFound.Flag = "Y";

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.Employer.Name, entities.Employer.Name))
      {
        local.EmployerHistoryDetail.Change = entities.Employer.Name + "   ;   " +
          (import.Employer.Name ?? "");
        ++local.EmployerHistoryDetail.LineNumber;
        local.ChgFound.Flag = "Y";

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.Employer.KansasId, entities.Employer.KansasId))
      {
        local.EmployerHistoryDetail.Change = entities.Employer.KansasId + "   ;   " +
          (import.Employer.KansasId ?? "");
        ++local.EmployerHistoryDetail.LineNumber;
        local.ChgFound.Flag = "Y";

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.Employer.PhoneNo, entities.Employer.PhoneNo) || !
        Equal(import.Employer.AreaCode.GetValueOrDefault(),
        entities.Employer.AreaCode) || !
        Equal(import.Employer.FaxPhoneNo, entities.Employer.FaxPhoneNo) || !
        Equal(export.Employer.FaxAreaCode.GetValueOrDefault(),
        entities.Employer.FaxAreaCode))
      {
        local.ChgFound.Flag = "Y";
        local.PhoneAreaCode.Text3 =
          NumberToString(import.Employer.AreaCode.GetValueOrDefault(), 13, 3);
        local.FaxAreaCode.Text3 =
          NumberToString(import.Employer.FaxAreaCode.GetValueOrDefault(), 13, 3);
          
        local.City1.Text33 = "Phone " + local.PhoneAreaCode.Text3 + " " + (
          import.Employer.PhoneNo ?? "") + " Fax " + local.FaxAreaCode.Text3 + " " +
          (import.Employer.FaxPhoneNo ?? "");
        local.PhoneAreaCode.Text3 =
          NumberToString(entities.Employer.AreaCode.GetValueOrDefault(), 13, 3);
          
        local.FaxAreaCode.Text3 =
          NumberToString(entities.Employer.FaxAreaCode.GetValueOrDefault(), 13,
          3);
        local.City2.Text33 = "Phone " + local.PhoneAreaCode.Text3 + " " + entities
          .Employer.PhoneNo + " Fax " + local.FaxAreaCode.Text3 + " " + entities
          .Employer.FaxPhoneNo;
        local.EmployerHistoryDetail.Change = local.City2.Text33 + " ; " + local
          .City1.Text33;
        ++local.EmployerHistoryDetail.LineNumber;

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.Employer.EmailAddress, entities.Employer.EmailAddress))
      {
        ++local.EmployerHistoryDetail.LineNumber;
        local.EmployerHistoryDetail.Change = "From: " + entities
          .Employer.EmailAddress;
        local.ChgFound.Flag = "Y";

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        ++local.EmployerHistoryDetail.LineNumber;
        local.EmployerHistoryDetail.Change = "To: " + (
          import.Employer.EmailAddress ?? "");

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.Employer.EffectiveDate, entities.Employer.EffectiveDate) ||
        !Equal(import.Employer.EndDate, entities.Employer.EndDate))
      {
        if (Lt(import.Employer.EndDate, entities.Employer.EndDate) && !
          Lt(local.Null1.Date, import.Employer.EndDate) || Lt
          (entities.Employer.EndDate, import.Employer.EndDate) && Lt
          (local.Current.Date, import.Employer.EndDate))
        {
          export.Employer.EffectiveDate = local.Current.Date;
          export.Employer.EndDate = local.Max.Date;

          // REOPEN AN EMPLOYER
          local.PastRelationshipFound.Flag = "Y";
        }

        if (Lt(import.Employer.EndDate, entities.Employer.EndDate) && Equal
          (entities.Employer.EndDate, local.Max.Date))
        {
          export.Employer.EndDate = import.Employer.EndDate;

          // CLOSE AN EMPLOYER
          foreach(var item in ReadIncomeSourceCsePerson())
          {
            foreach(var item1 in ReadCaseCaseUnit())
            {
              local.Infrastructure.EventId = 10;
              local.Infrastructure.ProcessStatus = "Q";
              local.Infrastructure.CaseNumber = entities.Case1.Number;
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
              local.Infrastructure.ReasonCode = "INCSOURCECLOSE";
              local.Infrastructure.BusinessObjectCd = "ICS";
              local.Infrastructure.InitiatingStateCode = "";
              local.Infrastructure.CsenetInOutCode = "";
              local.Infrastructure.UserId = global.UserId;
              local.Infrastructure.CreatedBy = global.UserId;
              local.Infrastructure.CsePersonNumber = entities.CsePerson.Number;
              local.Infrastructure.SituationNumber = 0;
              local.Infrastructure.CreatedTimestamp = Now();
              local.Infrastructure.Detail = TrimEnd(entities.Employer.Name) + " is no longer active.";
                

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
                local.Infrastructure.InitiatingStateCode = "OS";
              }

              UseSpCabCreateInfrastructure();

              if (!IsExitState("ACO_NN0000_ALL_OK"))
              {
                return;
              }
            }

            try
            {
              UpdateIncomeSource();
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "INCOME_SOURCE_AE";

                  break;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "INCOME_SOURCE_PV";

                  break;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }

          foreach(var item in ReadEmployerRelation())
          {
            try
            {
              UpdateEmployerRelation();
              local.RelationshipFound.Flag = "Y";
            }
            catch(Exception e)
            {
              switch(GetErrorCode(e))
              {
                case ErrorCode.AlreadyExists:
                  ExitState = "EMPLOYER_HISTORY_NU";

                  return;
                case ErrorCode.PermittedValueViolation:
                  ExitState = "EMPLOYER_HISTORY_PV";

                  return;
                case ErrorCode.DatabaseError:
                  break;
                default:
                  throw;
              }
            }
          }
        }

        local.ChgFound.Flag = "Y";
        local.DateWorkArea.Date = export.Employer.EffectiveDate;
        UseCabDate2TextWithHyphens2();
        local.DateWorkArea.Date = export.Employer.EndDate;
        UseCabDate2TextWithHyphens1();
        local.City1.Text33 = "Eff: " + local.Date1.Text10 + " End: " + local
          .Date2.Text10;
        local.DateWorkArea.Date = entities.Employer.EffectiveDate;
        UseCabDate2TextWithHyphens2();
        local.DateWorkArea.Date = entities.Employer.EndDate;
        UseCabDate2TextWithHyphens1();
        local.City2.Text33 = "Eff: " + local.Date1.Text10 + " End: " + local
          .Date2.Text10;
        local.EmployerHistoryDetail.Change = local.City2.Text33 + " ; " + local
          .City1.Text33;
        ++local.EmployerHistoryDetail.LineNumber;

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (!Equal(import.Employer.EiwoStartDate, entities.Employer.EiwoStartDate) ||
        !Equal(import.Employer.EiwoEndDate, entities.Employer.EiwoEndDate))
      {
        ++local.EmployerHistoryDetail.LineNumber;
        local.ChgFound.Flag = "Y";
        local.DateWorkArea.Date = entities.Employer.EiwoStartDate;
        UseCabDate2TextWithHyphens2();
        local.DateWorkArea.Date = entities.Employer.EiwoEndDate;
        UseCabDate2TextWithHyphens1();
        local.EmployerHistoryDetail.Change = "From: e-IWO Eff Date " + local
          .Date1.Text10 + "  End Date " + local.Date2.Text10;

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        local.DateWorkArea.Date = import.Employer.EiwoStartDate;
        UseCabDate2TextWithHyphens2();
        local.DateWorkArea.Date = import.Employer.EiwoEndDate;
        UseCabDate2TextWithHyphens1();
        local.EmployerHistoryDetail.Change = "To: e-IWO Eff Date " + local
          .Date1.Text10 + "  End Date " + local.Date2.Text10;
        ++local.EmployerHistoryDetail.LineNumber;

        try
        {
          CreateEmployerHistoryDetail();
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_HISTORY_DETAIL_AE";

              return;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_HISTORY_DETIAL_PV";

              return;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }

      if (AsChar(local.ChgFound.Flag) == 'Y')
      {
        try
        {
          UpdateEmployer();
          ExitState = "ACO_NI0000_SUCCESSFUL_UPDATE";
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "EMPLOYER_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "EMPLOYER_PV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }

        export.Employer.EiwoStartDate = local.Employer.EiwoStartDate;
        export.Employer.EiwoEndDate = local.Employer.EiwoEndDate;

        if (ReadEmployer2())
        {
          export.Employer.EndDate = entities.Employer.EndDate;
        }
      }
    }
    else
    {
      ExitState = "EMPLOYER_NF";
    }

    if (AsChar(local.PastRelationshipFound.Flag) == 'Y')
    {
      ExitState = "EMPLOYER_HAD_PRIOR_RELATIONSHIP";
    }

    if (AsChar(local.RelationshipFound.Flag) == 'Y')
    {
      ExitState = "EMPLOYER_RELATIONSHIP_CLOSED";
    }
  }

  private static void MoveEmployer(Employer source, Employer target)
  {
    target.EiwoEndDate = source.EiwoEndDate;
    target.EiwoStartDate = source.EiwoStartDate;
    target.EmailAddress = source.EmailAddress;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.Function = source.Function;
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
    target.EventType = source.EventType;
    target.EventDetailName = source.EventDetailName;
    target.ReasonCode = source.ReasonCode;
    target.BusinessObjectCd = source.BusinessObjectCd;
    target.DenormNumeric12 = source.DenormNumeric12;
    target.DenormText12 = source.DenormText12;
    target.InitiatingStateCode = source.InitiatingStateCode;
    target.CsenetInOutCode = source.CsenetInOutCode;
    target.CaseNumber = source.CaseNumber;
    target.CsePersonNumber = source.CsePersonNumber;
    target.CaseUnitNumber = source.CaseUnitNumber;
    target.UserId = source.UserId;
    target.CreatedBy = source.CreatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private void UseCabDate2TextWithHyphens1()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.Date2.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabDate2TextWithHyphens2()
  {
    var useImport = new CabDate2TextWithHyphens.Import();
    var useExport = new CabDate2TextWithHyphens.Export();

    useImport.DateWorkArea.Date = local.DateWorkArea.Date;

    Call(CabDate2TextWithHyphens.Execute, useImport, useExport);

    local.Date1.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, local.Infrastructure);
  }

  private void CreateEmployerHistory()
  {
    var actionTaken = local.EmployerHistory.ActionTaken ?? "";
    var actionDate = local.EmployerHistory.ActionDate;
    var createdBy = local.EmployerHistory.CreatedBy;
    var createdTimestamp = local.EmployerHistory.CreatedTimestamp;
    var lastUpdatedBy = local.EmployerHistory.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = Now();
    var empId = entities.Employer.Identifier;

    entities.EmployerHistory.Populated = false;
    Update("CreateEmployerHistory",
      (db, command) =>
      {
        db.SetNullableString(command, "actionTaken", actionTaken);
        db.SetDate(command, "actionDate", actionDate);
        db.SetNullableString(command, "note", "");
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTmst", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetInt32(command, "empId", empId);
      });

    entities.EmployerHistory.ActionTaken = actionTaken;
    entities.EmployerHistory.ActionDate = actionDate;
    entities.EmployerHistory.Note = "";
    entities.EmployerHistory.CreatedBy = createdBy;
    entities.EmployerHistory.CreatedTimestamp = createdTimestamp;
    entities.EmployerHistory.LastUpdatedBy = lastUpdatedBy;
    entities.EmployerHistory.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.EmployerHistory.EmpId = empId;
    entities.EmployerHistory.Populated = true;
  }

  private void CreateEmployerHistoryDetail()
  {
    System.Diagnostics.Debug.Assert(entities.EmployerHistory.Populated);

    var empId = entities.EmployerHistory.EmpId;
    var ehxCreatedTmst = entities.EmployerHistory.CreatedTimestamp;
    var lineNumber = local.EmployerHistoryDetail.LineNumber;
    var createdBy = local.EmployerHistoryDetail.CreatedBy;
    var createdTimestamp = local.EmployerHistoryDetail.CreatedTimestamp;
    var lastUpdatedBy = local.EmployerHistoryDetail.LastUpdatedBy ?? "";
    var lastUpdatedTimestamp = local.EmployerHistoryDetail.LastUpdatedTimestamp;
    var change = local.EmployerHistoryDetail.Change ?? "";

    entities.EmployerHistoryDetail.Populated = false;
    Update("CreateEmployerHistoryDetail",
      (db, command) =>
      {
        db.SetInt32(command, "empId", empId);
        db.SetDateTime(command, "ehxCreatedTmst", ehxCreatedTmst);
        db.SetInt32(command, "lineNumber", lineNumber);
        db.SetString(command, "createdBy", createdBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "change", change);
      });

    entities.EmployerHistoryDetail.EmpId = empId;
    entities.EmployerHistoryDetail.EhxCreatedTmst = ehxCreatedTmst;
    entities.EmployerHistoryDetail.LineNumber = lineNumber;
    entities.EmployerHistoryDetail.CreatedBy = createdBy;
    entities.EmployerHistoryDetail.CreatedTimestamp = createdTimestamp;
    entities.EmployerHistoryDetail.LastUpdatedBy = lastUpdatedBy;
    entities.EmployerHistoryDetail.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.EmployerHistoryDetail.Change = change;
    entities.EmployerHistoryDetail.Populated = true;
  }

  private IEnumerable<bool> ReadCaseCaseUnit()
  {
    entities.CaseUnit.Populated = false;
    entities.Case1.Populated = false;

    return ReadEach("ReadCaseCaseUnit",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNoAp", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Case1.Number = db.GetString(reader, 0);
        entities.CaseUnit.CasNo = db.GetString(reader, 0);
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 1);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 2);
        entities.CaseUnit.Populated = true;
        entities.Case1.Populated = true;

        return true;
      });
  }

  private bool ReadEmployer1()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadEmployer1",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", import.Employer.Ein ?? "");
      },
      (db, reader) =>
      {
        entities.N2dRead.Identifier = db.GetInt32(reader, 0);
        entities.N2dRead.Ein = db.GetNullableString(reader, 1);
        entities.N2dRead.EiwoEndDate = db.GetNullableDate(reader, 2);
        entities.N2dRead.EiwoStartDate = db.GetNullableDate(reader, 3);
        entities.N2dRead.Populated = true;
      });
  }

  private bool ReadEmployer2()
  {
    entities.Employer.Populated = false;

    return Read("ReadEmployer2",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.Employer.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Employer.LastUpdatedTstamp = db.GetNullableDateTime(reader, 5);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 6);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 7);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 8);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 9);
        entities.Employer.FaxAreaCode = db.GetNullableInt32(reader, 10);
        entities.Employer.FaxPhoneNo = db.GetNullableString(reader, 11);
        entities.Employer.EmailAddress = db.GetNullableString(reader, 12);
        entities.Employer.EffectiveDate = db.GetNullableDate(reader, 13);
        entities.Employer.EndDate = db.GetNullableDate(reader, 14);
        entities.Employer.Populated = true;
      });
  }

  private bool ReadEmployerEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;
    entities.Employer.Populated = false;

    return Read("ReadEmployerEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(command, "identifier", import.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.Employer.Identifier = db.GetInt32(reader, 0);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 0);
        entities.Employer.Ein = db.GetNullableString(reader, 1);
        entities.Employer.KansasId = db.GetNullableString(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.Employer.LastUpdatedBy = db.GetNullableString(reader, 4);
        entities.Employer.LastUpdatedTstamp = db.GetNullableDateTime(reader, 5);
        entities.Employer.PhoneNo = db.GetNullableString(reader, 6);
        entities.Employer.AreaCode = db.GetNullableInt32(reader, 7);
        entities.Employer.EiwoEndDate = db.GetNullableDate(reader, 8);
        entities.Employer.EiwoStartDate = db.GetNullableDate(reader, 9);
        entities.Employer.FaxAreaCode = db.GetNullableInt32(reader, 10);
        entities.Employer.FaxPhoneNo = db.GetNullableString(reader, 11);
        entities.Employer.EmailAddress = db.GetNullableString(reader, 12);
        entities.Employer.EffectiveDate = db.GetNullableDate(reader, 13);
        entities.Employer.EndDate = db.GetNullableDate(reader, 14);
        entities.EmployerAddress.LocationType = db.GetString(reader, 15);
        entities.EmployerAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.EmployerAddress.LastUpdatedBy =
          db.GetNullableString(reader, 17);
        entities.EmployerAddress.CreatedTimestamp = db.GetDateTime(reader, 18);
        entities.EmployerAddress.CreatedBy = db.GetString(reader, 19);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 20);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 21);
        entities.EmployerAddress.City = db.GetNullableString(reader, 22);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 23);
        entities.EmployerAddress.Street3 = db.GetNullableString(reader, 24);
        entities.EmployerAddress.Street4 = db.GetNullableString(reader, 25);
        entities.EmployerAddress.Province = db.GetNullableString(reader, 26);
        entities.EmployerAddress.Country = db.GetNullableString(reader, 27);
        entities.EmployerAddress.PostalCode = db.GetNullableString(reader, 28);
        entities.EmployerAddress.State = db.GetNullableString(reader, 29);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 30);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 31);
        entities.EmployerAddress.Zip3 = db.GetNullableString(reader, 32);
        entities.EmployerAddress.County = db.GetNullableString(reader, 33);
        entities.EmployerAddress.Note = db.GetNullableString(reader, 34);
        entities.EmployerAddress.Populated = true;
        entities.Employer.Populated = true;
        CheckValid<EmployerAddress>("LocationType",
          entities.EmployerAddress.LocationType);
      });
  }

  private IEnumerable<bool> ReadEmployerRelation()
  {
    entities.EmployerRelation.Populated = false;

    return ReadEach("ReadEmployerRelation",
      (db, command) =>
      {
        db.SetInt32(command, "empLocId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "endDate", local.DateWorkArea.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.EmployerRelation.Identifier = db.GetInt32(reader, 0);
        entities.EmployerRelation.EffectiveDate = db.GetNullableDate(reader, 1);
        entities.EmployerRelation.EndDate = db.GetNullableDate(reader, 2);
        entities.EmployerRelation.UpdatedTimestamp = db.GetDateTime(reader, 3);
        entities.EmployerRelation.UpdatedBy = db.GetString(reader, 4);
        entities.EmployerRelation.EmpHqId = db.GetInt32(reader, 5);
        entities.EmployerRelation.EmpLocId = db.GetInt32(reader, 6);
        entities.EmployerRelation.Type1 = db.GetString(reader, 7);
        entities.EmployerRelation.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIncomeSourceCsePerson()
  {
    entities.CsePerson.Populated = false;
    entities.IncomeSource.Populated = false;

    return ReadEach("ReadIncomeSourceCsePerson",
      (db, command) =>
      {
        db.SetNullableInt32(command, "empId", entities.Employer.Identifier);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.Type1 = db.GetString(reader, 1);
        entities.IncomeSource.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 2);
        entities.IncomeSource.LastUpdatedBy = db.GetNullableString(reader, 3);
        entities.IncomeSource.CspINumber = db.GetString(reader, 4);
        entities.CsePerson.Number = db.GetString(reader, 4);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 5);
        entities.IncomeSource.StartDt = db.GetNullableDate(reader, 6);
        entities.IncomeSource.EndDt = db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.IncomeSource.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.IncomeSource.Type1);

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

  private void UpdateEmployer()
  {
    var ein = import.Employer.Ein ?? "";
    var kansasId = import.Employer.KansasId ?? "";
    var name = import.Employer.Name ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTstamp = Now();
    var phoneNo = import.Employer.PhoneNo ?? "";
    var areaCode = import.Employer.AreaCode.GetValueOrDefault();
    var eiwoEndDate = local.Employer.EiwoEndDate;
    var eiwoStartDate = local.Employer.EiwoStartDate;
    var faxAreaCode = import.Employer.FaxAreaCode.GetValueOrDefault();
    var faxPhoneNo = import.Employer.FaxPhoneNo ?? "";
    var emailAddress = import.Employer.EmailAddress ?? "";
    var effectiveDate = export.Employer.EffectiveDate;
    var endDate = export.Employer.EndDate;

    entities.Employer.Populated = false;
    Update("UpdateEmployer",
      (db, command) =>
      {
        db.SetNullableString(command, "ein", ein);
        db.SetNullableString(command, "kansasId", kansasId);
        db.SetNullableString(command, "name", name);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDateTime(command, "lastUpdTstamp", lastUpdatedTstamp);
        db.SetNullableString(command, "phoneNo", phoneNo);
        db.SetNullableInt32(command, "areaCode", areaCode);
        db.SetNullableDate(command, "eiwoEndDate", eiwoEndDate);
        db.SetNullableDate(command, "eiwoStartDate", eiwoStartDate);
        db.SetNullableInt32(command, "faxAreaCode", faxAreaCode);
        db.SetNullableString(command, "faxPhoneNo", faxPhoneNo);
        db.SetNullableString(command, "emailAddress", emailAddress);
        db.SetNullableDate(command, "effectiveDate", effectiveDate);
        db.SetNullableDate(command, "endDate", endDate);
        db.SetInt32(command, "identifier", entities.Employer.Identifier);
      });

    entities.Employer.Ein = ein;
    entities.Employer.KansasId = kansasId;
    entities.Employer.Name = name;
    entities.Employer.LastUpdatedBy = lastUpdatedBy;
    entities.Employer.LastUpdatedTstamp = lastUpdatedTstamp;
    entities.Employer.PhoneNo = phoneNo;
    entities.Employer.AreaCode = areaCode;
    entities.Employer.EiwoEndDate = eiwoEndDate;
    entities.Employer.EiwoStartDate = eiwoStartDate;
    entities.Employer.FaxAreaCode = faxAreaCode;
    entities.Employer.FaxPhoneNo = faxPhoneNo;
    entities.Employer.EmailAddress = emailAddress;
    entities.Employer.EffectiveDate = effectiveDate;
    entities.Employer.EndDate = endDate;
    entities.Employer.Populated = true;
  }

  private void UpdateEmployerAddress()
  {
    System.Diagnostics.Debug.Assert(entities.EmployerAddress.Populated);

    var locationType = import.EmployerAddress.LocationType;
    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var createdTimestamp = import.EmployerAddress.CreatedTimestamp;
    var createdBy = import.EmployerAddress.CreatedBy;
    var street1 = import.EmployerAddress.Street1 ?? "";
    var street2 = import.EmployerAddress.Street2 ?? "";
    var city = import.EmployerAddress.City ?? "";
    var street3 = import.EmployerAddress.Street3 ?? "";
    var street4 = import.EmployerAddress.Street4 ?? "";
    var province = import.EmployerAddress.Province ?? "";
    var country = import.EmployerAddress.Country ?? "";
    var postalCode = import.EmployerAddress.PostalCode ?? "";
    var state = import.EmployerAddress.State ?? "";
    var zipCode = import.EmployerAddress.ZipCode ?? "";
    var zip4 = import.EmployerAddress.Zip4 ?? "";
    var zip3 = import.EmployerAddress.Zip3 ?? "";
    var county = import.EmployerAddress.County ?? "";
    var note = import.EmployerAddress.Note ?? "";

    CheckValid<EmployerAddress>("LocationType", locationType);
    entities.EmployerAddress.Populated = false;
    Update("UpdateEmployerAddress",
      (db, command) =>
      {
        db.SetString(command, "locationType", locationType);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetDateTime(command, "createdTimestamp", createdTimestamp);
        db.SetString(command, "createdBy", createdBy);
        db.SetNullableString(command, "street1", street1);
        db.SetNullableString(command, "street2", street2);
        db.SetNullableString(command, "city", city);
        db.SetNullableString(command, "street3", street3);
        db.SetNullableString(command, "street4", street4);
        db.SetNullableString(command, "province", province);
        db.SetNullableString(command, "country", country);
        db.SetNullableString(command, "postalCode", postalCode);
        db.SetNullableString(command, "state", state);
        db.SetNullableString(command, "zipCode", zipCode);
        db.SetNullableString(command, "zip4", zip4);
        db.SetNullableString(command, "zip3", zip3);
        db.SetNullableString(command, "county", county);
        db.SetNullableString(command, "note", note);
        db.SetDateTime(
          command, "identifier",
          entities.EmployerAddress.Identifier.GetValueOrDefault());
        db.SetInt32(command, "empId", entities.EmployerAddress.EmpId);
      });

    entities.EmployerAddress.LocationType = locationType;
    entities.EmployerAddress.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.EmployerAddress.LastUpdatedBy = lastUpdatedBy;
    entities.EmployerAddress.CreatedTimestamp = createdTimestamp;
    entities.EmployerAddress.CreatedBy = createdBy;
    entities.EmployerAddress.Street1 = street1;
    entities.EmployerAddress.Street2 = street2;
    entities.EmployerAddress.City = city;
    entities.EmployerAddress.Street3 = street3;
    entities.EmployerAddress.Street4 = street4;
    entities.EmployerAddress.Province = province;
    entities.EmployerAddress.Country = country;
    entities.EmployerAddress.PostalCode = postalCode;
    entities.EmployerAddress.State = state;
    entities.EmployerAddress.ZipCode = zipCode;
    entities.EmployerAddress.Zip4 = zip4;
    entities.EmployerAddress.Zip3 = zip3;
    entities.EmployerAddress.County = county;
    entities.EmployerAddress.Note = note;
    entities.EmployerAddress.Populated = true;
  }

  private void UpdateEmployerRelation()
  {
    System.Diagnostics.Debug.Assert(entities.EmployerRelation.Populated);

    var endDate = export.Employer.EndDate;
    var updatedTimestamp = Now();
    var updatedBy = global.UserId;

    entities.EmployerRelation.Populated = false;
    Update("UpdateEmployerRelation",
      (db, command) =>
      {
        db.SetNullableDate(command, "endDate", endDate);
        db.SetDateTime(command, "lastUpdatedTmst", updatedTimestamp);
        db.SetString(command, "lastUpdatedBy", updatedBy);
        db.
          SetInt32(command, "identifier", entities.EmployerRelation.Identifier);
          
        db.SetInt32(command, "empHqId", entities.EmployerRelation.EmpHqId);
        db.SetInt32(command, "empLocId", entities.EmployerRelation.EmpLocId);
        db.SetString(command, "type", entities.EmployerRelation.Type1);
      });

    entities.EmployerRelation.EndDate = endDate;
    entities.EmployerRelation.UpdatedTimestamp = updatedTimestamp;
    entities.EmployerRelation.UpdatedBy = updatedBy;
    entities.EmployerRelation.Populated = true;
  }

  private void UpdateIncomeSource()
  {
    System.Diagnostics.Debug.Assert(entities.IncomeSource.Populated);

    var lastUpdatedTimestamp = Now();
    var lastUpdatedBy = global.UserId;
    var endDt = export.Employer.EndDate;

    entities.IncomeSource.Populated = false;
    Update("UpdateIncomeSource",
      (db, command) =>
      {
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.SetNullableDate(command, "endDt", endDt);
        db.SetDateTime(
          command, "identifier",
          entities.IncomeSource.Identifier.GetValueOrDefault());
        db.SetString(command, "cspINumber", entities.IncomeSource.CspINumber);
      });

    entities.IncomeSource.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.IncomeSource.LastUpdatedBy = lastUpdatedBy;
    entities.IncomeSource.EndDt = endDt;
    entities.IncomeSource.Populated = true;
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
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private EmployerAddress employerAddress;
    private Employer employer;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private Employer employer;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of PastRelationshipFound.
    /// </summary>
    [JsonPropertyName("pastRelationshipFound")]
    public Common PastRelationshipFound
    {
      get => pastRelationshipFound ??= new();
      set => pastRelationshipFound = value;
    }

    /// <summary>
    /// A value of ChgFound.
    /// </summary>
    [JsonPropertyName("chgFound")]
    public Common ChgFound
    {
      get => chgFound ??= new();
      set => chgFound = value;
    }

    /// <summary>
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of Found.
    /// </summary>
    [JsonPropertyName("found")]
    public EmployerRelation Found
    {
      get => found ??= new();
      set => found = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of EmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("employerHistoryDetail")]
    public EmployerHistoryDetail EmployerHistoryDetail
    {
      get => employerHistoryDetail ??= new();
      set => employerHistoryDetail = value;
    }

    /// <summary>
    /// A value of RelationshipFound.
    /// </summary>
    [JsonPropertyName("relationshipFound")]
    public Common RelationshipFound
    {
      get => relationshipFound ??= new();
      set => relationshipFound = value;
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
    /// A value of Date2.
    /// </summary>
    [JsonPropertyName("date2")]
    public TextWorkArea Date2
    {
      get => date2 ??= new();
      set => date2 = value;
    }

    /// <summary>
    /// A value of Date1.
    /// </summary>
    [JsonPropertyName("date1")]
    public TextWorkArea Date1
    {
      get => date1 ??= new();
      set => date1 = value;
    }

    /// <summary>
    /// A value of FaxAreaCode.
    /// </summary>
    [JsonPropertyName("faxAreaCode")]
    public WorkArea FaxAreaCode
    {
      get => faxAreaCode ??= new();
      set => faxAreaCode = value;
    }

    /// <summary>
    /// A value of PhoneAreaCode.
    /// </summary>
    [JsonPropertyName("phoneAreaCode")]
    public WorkArea PhoneAreaCode
    {
      get => phoneAreaCode ??= new();
      set => phoneAreaCode = value;
    }

    /// <summary>
    /// A value of City2.
    /// </summary>
    [JsonPropertyName("city2")]
    public WorkArea City2
    {
      get => city2 ??= new();
      set => city2 = value;
    }

    /// <summary>
    /// A value of City1.
    /// </summary>
    [JsonPropertyName("city1")]
    public WorkArea City1
    {
      get => city1 ??= new();
      set => city1 = value;
    }

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
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
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private Common pastRelationshipFound;
    private Common chgFound;
    private DateWorkArea null1;
    private EmployerRelation found;
    private DateWorkArea max;
    private DateWorkArea current;
    private EmployerHistoryDetail employerHistoryDetail;
    private Common relationshipFound;
    private Infrastructure infrastructure;
    private TextWorkArea date2;
    private TextWorkArea date1;
    private WorkArea faxAreaCode;
    private WorkArea phoneAreaCode;
    private WorkArea city2;
    private WorkArea city1;
    private EmployerHistory employerHistory;
    private DateWorkArea dateWorkArea;
    private Employer employer;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of EmployerHistoryDetail.
    /// </summary>
    [JsonPropertyName("employerHistoryDetail")]
    public EmployerHistoryDetail EmployerHistoryDetail
    {
      get => employerHistoryDetail ??= new();
      set => employerHistoryDetail = value;
    }

    /// <summary>
    /// A value of EmployerRelation.
    /// </summary>
    [JsonPropertyName("employerRelation")]
    public EmployerRelation EmployerRelation
    {
      get => employerRelation ??= new();
      set => employerRelation = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of IncomeSource.
    /// </summary>
    [JsonPropertyName("incomeSource")]
    public IncomeSource IncomeSource
    {
      get => incomeSource ??= new();
      set => incomeSource = value;
    }

    /// <summary>
    /// A value of EmployerHistory.
    /// </summary>
    [JsonPropertyName("employerHistory")]
    public EmployerHistory EmployerHistory
    {
      get => employerHistory ??= new();
      set => employerHistory = value;
    }

    /// <summary>
    /// A value of N2dRead.
    /// </summary>
    [JsonPropertyName("n2dRead")]
    public Employer N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
    }

    /// <summary>
    /// A value of EmployerAddress.
    /// </summary>
    [JsonPropertyName("employerAddress")]
    public EmployerAddress EmployerAddress
    {
      get => employerAddress ??= new();
      set => employerAddress = value;
    }

    /// <summary>
    /// A value of Employer.
    /// </summary>
    [JsonPropertyName("employer")]
    public Employer Employer
    {
      get => employer ??= new();
      set => employer = value;
    }

    private EmployerHistoryDetail employerHistoryDetail;
    private EmployerRelation employerRelation;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
    private Case1 case1;
    private CsePerson csePerson;
    private IncomeSource incomeSource;
    private EmployerHistory employerHistory;
    private Employer n2dRead;
    private EmployerAddress employerAddress;
    private Employer employer;
  }
#endregion
}

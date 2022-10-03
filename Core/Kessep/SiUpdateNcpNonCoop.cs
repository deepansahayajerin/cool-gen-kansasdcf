// Program: SI_UPDATE_NCP_NON_COOP, ID: 1902537690, model: 746.
// Short name: SWE03753
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_UPDATE_NCP_NON_COOP.
/// </summary>
[Serializable]
public partial class SiUpdateNcpNonCoop: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_UPDATE_NCP_NON_COOP program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiUpdateNcpNonCoop(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiUpdateNcpNonCoop.
  /// </summary>
  public SiUpdateNcpNonCoop(IContext context, Import import, Export export):
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
    // Date	    Developer		Description
    // 04/06/2016  DDupree		Initial Development
    local.Current.Date = Now().Date;
    local.Max.Date = UseCabSetMaximumDiscontinueDate();

    // ---------------------------------------------------------
    // Properties of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    // ---------------------------------------------------------
    // Properties of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (ReadCsePerson())
    {
      if (ReadNcpNonCooperation())
      {
        local.NcpNonCooperation.Assign(entities.NcpNonCooperation);

        try
        {
          UpdateNcpNonCooperation();

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
          local.Infrastructure.UserId = "NCOP";
          local.Infrastructure.ReferenceDate = local.Current.Date;

          if (!Equal(local.NcpNonCooperation.Letter1Date,
            import.NcpNonCooperation.Letter1Date))
          {
            local.DateWorkArea.Date = import.NcpNonCooperation.Letter1Date;
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Letter 1 date changed for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with a date of:" + local
              .TextWorkArea.Text8;

            if (!IsEmpty(import.NcpNonCooperation.Letter1Code))
            {
              if (!Equal(import.NcpNonCooperation.Letter1Code,
                local.NcpNonCooperation.Letter1Code))
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + ";on: " + local
                  .TextWorkArea.Text8 + ";from cd:" + (
                    local.NcpNonCooperation.Letter1Code ?? "") + ";to cd:" + (
                    import.NcpNonCooperation.Letter1Code ?? "");
              }
              else
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + "; on: " + local
                  .TextWorkArea.Text8 + " with code: " + (
                    import.NcpNonCooperation.Letter1Code ?? "");
              }
            }

            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }

            if (Lt(local.Blank.Date, local.NcpNonCooperation.Phone1Date))
            {
              // this record has been restarted so we don't want to add anymore 
              // hist records than these.
              return;
            }
          }
          else if (!Equal(import.NcpNonCooperation.Letter1Code,
            local.NcpNonCooperation.Letter1Code))
          {
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            local.TextWorkArea.Text30 = "Letter 1 code updated for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; from code:" + (
                local.NcpNonCooperation.Letter1Code ?? "") + "; to code:" + (
                import.NcpNonCooperation.Letter1Code ?? "");
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }

          if (Equal(local.NcpNonCooperation.Phone1Date, local.Blank.Date) && Lt
            (local.Blank.Date, import.NcpNonCooperation.Phone1Date))
          {
            local.Infrastructure.ReasonCode = "NCPPHONE1ADD";
            local.DateWorkArea.Date = import.NcpNonCooperation.Phone1Date;
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Phone 1 date added for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + " ; with a date of:" + local
              .TextWorkArea.Text8;

            if (!IsEmpty(import.NcpNonCooperation.Phone1Code))
            {
              local.Infrastructure.Detail =
                TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                (entities.ApCsePerson.Number) + "; on: " + local
                .TextWorkArea.Text8 + " with code: " + (
                  import.NcpNonCooperation.Phone1Code ?? "");
            }

            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (Lt(local.Blank.Date, import.NcpNonCooperation.Phone1Date) &&
            !
            Equal(import.NcpNonCooperation.Phone1Date,
            local.NcpNonCooperation.Phone1Date) && Lt
            (local.Blank.Date, local.NcpNonCooperation.Phone1Date))
          {
            local.DateWorkArea.Date = import.NcpNonCooperation.Phone1Date;
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Phone 1 date changed for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with a date of:" + local
              .TextWorkArea.Text8;

            if (!IsEmpty(import.NcpNonCooperation.Phone1Code))
            {
              if (!Equal(import.NcpNonCooperation.Phone1Code,
                local.NcpNonCooperation.Phone1Code))
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + "; on: " + local
                  .TextWorkArea.Text8 + ";from cd:" + (
                    local.NcpNonCooperation.Phone1Code ?? "") + ";to cd:" + (
                    import.NcpNonCooperation.Phone1Code ?? "");
              }
              else
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + "; on: " + local
                  .TextWorkArea.Text8 + " with code: " + (
                    import.NcpNonCooperation.Phone1Code ?? "");
              }
            }

            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (!Equal(import.NcpNonCooperation.Phone1Code,
            local.NcpNonCooperation.Phone1Code) && !
            IsEmpty(import.NcpNonCooperation.Phone1Code))
          {
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            local.TextWorkArea.Text30 = "Phone 1 code updated for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; from code:  " + (
                local.NcpNonCooperation.Phone1Code ?? "") + "; to code: " + (
                import.NcpNonCooperation.Phone1Code ?? "");
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (!Lt(local.Blank.Date, import.NcpNonCooperation.Phone1Date))
          {
            // do nothing, record could have reset
          }

          if (Equal(local.NcpNonCooperation.Letter2Date, local.Blank.Date) && Lt
            (local.Blank.Date, import.NcpNonCooperation.Letter2Date))
          {
            local.Infrastructure.ReasonCode = "NCPLTR2ADD";
            local.DateWorkArea.Date = import.NcpNonCooperation.Letter2Date;
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Letter 2 date added for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with a date of: " + local
              .TextWorkArea.Text8;

            if (!IsEmpty(import.NcpNonCooperation.Letter2Code))
            {
              local.Infrastructure.Detail =
                TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                (entities.ApCsePerson.Number) + "; on: " + local
                .TextWorkArea.Text8 + "; with code: " + (
                  import.NcpNonCooperation.Letter2Code ?? "");
            }

            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (Lt(local.Blank.Date, import.NcpNonCooperation.Letter2Date) &&
            !
            Equal(import.NcpNonCooperation.Letter2Date,
            local.NcpNonCooperation.Letter2Date) && Lt
            (local.Blank.Date, local.NcpNonCooperation.Letter2Date))
          {
            local.DateWorkArea.Date = import.NcpNonCooperation.Letter2Date;
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Letter 2 date changed for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with a date of: " + local
              .TextWorkArea.Text8;

            if (!IsEmpty(import.NcpNonCooperation.Letter2Code))
            {
              if (!Equal(import.NcpNonCooperation.Letter2Code,
                local.NcpNonCooperation.Letter2Code))
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + "; on: " + local
                  .TextWorkArea.Text8 + ";from cd:" + (
                    local.NcpNonCooperation.Letter2Code ?? "") + ";to cd:" + (
                    import.NcpNonCooperation.Letter2Code ?? "");
              }
              else
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + "; on: " + local
                  .TextWorkArea.Text8 + "; with code: " + (
                    import.NcpNonCooperation.Letter2Code ?? "");
              }
            }

            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (!Equal(import.NcpNonCooperation.Letter2Code,
            local.NcpNonCooperation.Letter2Code) && !
            IsEmpty(import.NcpNonCooperation.Letter2Code))
          {
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            local.TextWorkArea.Text30 = "Letter 2 code updated for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; from code:  " + (
                local.NcpNonCooperation.Letter2Code ?? "") + "; to code: " + (
                import.NcpNonCooperation.Letter2Code ?? "");
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (!Lt(local.Blank.Date, import.NcpNonCooperation.Letter2Date))
          {
            // do nothing, record could have reset
          }

          if (Equal(local.NcpNonCooperation.Phone2Date, local.Blank.Date) && Lt
            (local.Blank.Date, import.NcpNonCooperation.Phone2Date))
          {
            local.Infrastructure.ReasonCode = "NCPPHONE2ADD";
            local.DateWorkArea.Date = import.NcpNonCooperation.Phone2Date;
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Phone 2 date added for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with a date of: " + local
              .TextWorkArea.Text8;

            if (!IsEmpty(import.NcpNonCooperation.Phone2Code))
            {
              local.Infrastructure.Detail =
                TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                (entities.ApCsePerson.Number) + "; on: " + local
                .TextWorkArea.Text8 + "; with code: " + (
                  import.NcpNonCooperation.Phone2Code ?? "");
            }

            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (Lt(local.Blank.Date, import.NcpNonCooperation.Phone2Date) &&
            !
            Equal(import.NcpNonCooperation.Phone2Date,
            local.NcpNonCooperation.Phone2Date) && Lt
            (local.Blank.Date, local.NcpNonCooperation.Phone2Date))
          {
            local.DateWorkArea.Date = import.NcpNonCooperation.Phone2Date;
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Phone 2 date changed for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with a date of: " + local
              .TextWorkArea.Text8;

            if (!IsEmpty(import.NcpNonCooperation.Phone2Code))
            {
              if (!Equal(import.NcpNonCooperation.Phone2Code,
                local.NcpNonCooperation.Phone2Code))
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + "; on: " + local
                  .TextWorkArea.Text8 + "; from code: " + (
                    local.NcpNonCooperation.Phone2Code ?? "") + "; to code: " +
                  (import.NcpNonCooperation.Phone2Code ?? "");
              }
              else
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + "; on: " + local
                  .TextWorkArea.Text8 + "; with code: " + (
                    import.NcpNonCooperation.Phone2Code ?? "");
              }
            }

            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (!Equal(import.NcpNonCooperation.Phone2Code,
            local.NcpNonCooperation.Phone2Code) && !
            IsEmpty(import.NcpNonCooperation.Phone2Code))
          {
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            local.TextWorkArea.Text30 = "Phone 2 code updated for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; from code:  " + (
                local.NcpNonCooperation.Phone2Code ?? "") + "; to code: " + (
                import.NcpNonCooperation.Phone2Code ?? "");
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (!Lt(local.Blank.Date, import.NcpNonCooperation.Phone2Date))
          {
            // do nothing, record could have reset
          }

          if (Lt(import.NcpNonCooperation.EffectiveDate, local.Max.Date) && Equal
            (local.NcpNonCooperation.EffectiveDate, local.Max.Date))
          {
            if (Equal(import.NcpNonCooperation.EffectiveDate,
              import.NcpNonCooperation.EndDate))
            {
              // this recored was end dated but there was no effective date so 
              // the effective date was
              // set to the end date. We only want to capture the end date being
              // added since the
              // record was being closed.
              goto Test;
            }

            local.Infrastructure.ReasonCode = "NCPEFFDATEADD";
            local.DateWorkArea.Date = import.NcpNonCooperation.EffectiveDate;
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Effective date added for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; of: " + local
              .TextWorkArea.Text8 + "; with reason code: " + (
                import.NcpNonCooperation.ReasonCode ?? "");
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (Lt(local.Blank.Date, import.NcpNonCooperation.EffectiveDate) &&
            !
            Equal(import.NcpNonCooperation.EffectiveDate,
            local.NcpNonCooperation.EffectiveDate) && Lt
            (local.Blank.Date, local.NcpNonCooperation.EffectiveDate))
          {
            local.DateWorkArea.Date = import.NcpNonCooperation.EffectiveDate;
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Effective date changed for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with a date of: " + local
              .TextWorkArea.Text8;

            if (!IsEmpty(import.NcpNonCooperation.ReasonCode))
            {
              if (!Equal(import.NcpNonCooperation.ReasonCode,
                local.NcpNonCooperation.ReasonCode))
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + "; eff date: " + local
                  .TextWorkArea.Text8 + "; from code: " + (
                    local.NcpNonCooperation.ReasonCode ?? "") + "; to code: " +
                  (import.NcpNonCooperation.ReasonCode ?? "");
              }
              else
              {
                local.Infrastructure.Detail =
                  TrimEnd(local.TextWorkArea.Text30) + TrimEnd
                  (entities.ApCsePerson.Number) + "; of: " + local
                  .TextWorkArea.Text8 + "; with reason code: " + (
                    import.NcpNonCooperation.ReasonCode ?? "");
              }
            }

            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (!Equal(import.NcpNonCooperation.ReasonCode,
            local.NcpNonCooperation.ReasonCode) && !
            IsEmpty(import.NcpNonCooperation.ReasonCode))
          {
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            local.TextWorkArea.Text30 = "Reason code changed for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; from code:  " + (
                local.NcpNonCooperation.ReasonCode ?? "") + "; to code: " + (
                import.NcpNonCooperation.ReasonCode ?? "");
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
          else if (!Lt(local.Blank.Date, import.NcpNonCooperation.EffectiveDate))
            
          {
            // do nothing, record could have reset
          }

Test:

          // call that can be done to a end date or code is add it, it can never
          // be updated
          if (Lt(import.NcpNonCooperation.EndDate, local.Max.Date) && Equal
            (local.NcpNonCooperation.EndDate, local.Max.Date))
          {
            local.Infrastructure.ReasonCode = "NCPENDDATEADD";
            local.DateWorkArea.Date = import.NcpNonCooperation.EndDate;
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "End date added for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; of: " + local
              .TextWorkArea.Text8 + "; end reason code: " + (
                import.NcpNonCooperation.EndStatusCode ?? "");
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }

          if (!Equal(import.NcpNonCooperation.Note, local.NcpNonCooperation.Note))
            
          {
            local.Infrastructure.ReasonCode = "NCPNCOOPUPDATE";
            local.DateWorkArea.Date = local.Current.Date;
            UseCabConvertDate2String();
            local.TextWorkArea.Text30 = "Note updated for AP:";
            local.Infrastructure.CsePersonNumber = entities.ApCsePerson.Number;
            local.Infrastructure.Detail = TrimEnd(local.TextWorkArea.Text30) + TrimEnd
              (entities.ApCsePerson.Number) + "; with a date of: " + local
              .TextWorkArea.Text8;
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.CaseUnitFound.Flag = "N";

            foreach(var item in ReadCaseUnit())
            {
              local.Infrastructure.CaseUnitNumber = entities.CaseUnit.CuNumber;
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

            if (AsChar(local.CaseUnitFound.Flag) == 'N')
            {
              UseSpCabCreateInfrastructure();
            }
          }
        }
        catch(Exception e)
        {
          switch(GetErrorCode(e))
          {
            case ErrorCode.AlreadyExists:
              ExitState = "NCP_NON_COOP_NU";

              break;
            case ErrorCode.PermittedValueViolation:
              ExitState = "NCP_NON_COOP_PVV";

              break;
            case ErrorCode.DatabaseError:
              break;
            default:
              throw;
          }
        }
      }
      else
      {
        ExitState = "NON_COOP_NF";
      }
    }
    else
    {
      ExitState = "AP_FOR_CASE_NF";
    }
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

  private IEnumerable<bool> ReadCaseUnit()
  {
    entities.CaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.Case1.Number);
        db.SetNullableString(command, "cspNoAp", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.CaseUnit.StartDate = db.GetDate(reader, 1);
        entities.CaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.CaseUnit.CasNo = db.GetString(reader, 3);
        entities.CaseUnit.CspNoAp = db.GetNullableString(reader, 4);
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

  private bool ReadNcpNonCooperation()
  {
    entities.NcpNonCooperation.Populated = false;

    return Read("ReadNcpNonCooperation",
      (db, command) =>
      {
        db.SetDateTime(
          command, "createdTimestamp",
          import.NcpNonCooperation.CreatedTimestamp.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.NcpNonCooperation.NcpStatusCode =
          db.GetNullableString(reader, 0);
        entities.NcpNonCooperation.EffectiveDate =
          db.GetNullableDate(reader, 1);
        entities.NcpNonCooperation.ReasonCode = db.GetNullableString(reader, 2);
        entities.NcpNonCooperation.Letter1Date = db.GetNullableDate(reader, 3);
        entities.NcpNonCooperation.Letter1Code =
          db.GetNullableString(reader, 4);
        entities.NcpNonCooperation.Letter2Date = db.GetNullableDate(reader, 5);
        entities.NcpNonCooperation.Letter2Code =
          db.GetNullableString(reader, 6);
        entities.NcpNonCooperation.Phone1Date = db.GetNullableDate(reader, 7);
        entities.NcpNonCooperation.Phone1Code = db.GetNullableString(reader, 8);
        entities.NcpNonCooperation.Phone2Date = db.GetNullableDate(reader, 9);
        entities.NcpNonCooperation.Phone2Code =
          db.GetNullableString(reader, 10);
        entities.NcpNonCooperation.EndDate = db.GetNullableDate(reader, 11);
        entities.NcpNonCooperation.EndStatusCode =
          db.GetNullableString(reader, 12);
        entities.NcpNonCooperation.CreatedBy = db.GetString(reader, 13);
        entities.NcpNonCooperation.CreatedTimestamp =
          db.GetDateTime(reader, 14);
        entities.NcpNonCooperation.LastUpdatedBy =
          db.GetNullableString(reader, 15);
        entities.NcpNonCooperation.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 16);
        entities.NcpNonCooperation.CasNumber = db.GetString(reader, 17);
        entities.NcpNonCooperation.CspNumber = db.GetString(reader, 18);
        entities.NcpNonCooperation.Note = db.GetNullableString(reader, 19);
        entities.NcpNonCooperation.Populated = true;
      });
  }

  private void UpdateNcpNonCooperation()
  {
    System.Diagnostics.Debug.Assert(entities.NcpNonCooperation.Populated);

    var ncpStatusCode = import.NcpNonCooperation.NcpStatusCode ?? "";
    var effectiveDate = import.NcpNonCooperation.EffectiveDate;
    var reasonCode = import.NcpNonCooperation.ReasonCode ?? "";
    var letter1Date = import.NcpNonCooperation.Letter1Date;
    var letter1Code = import.NcpNonCooperation.Letter1Code ?? "";
    var letter2Date = import.NcpNonCooperation.Letter2Date;
    var letter2Code = import.NcpNonCooperation.Letter2Code ?? "";
    var phone1Date = import.NcpNonCooperation.Phone1Date;
    var phone1Code = import.NcpNonCooperation.Phone1Code ?? "";
    var phone2Date = import.NcpNonCooperation.Phone2Date;
    var phone2Code = import.NcpNonCooperation.Phone2Code ?? "";
    var endDate = import.NcpNonCooperation.EndDate;
    var endStatusCode = import.NcpNonCooperation.EndStatusCode ?? "";
    var lastUpdatedBy = global.UserId;
    var lastUpdatedTimestamp = Now();
    var note = import.NcpNonCooperation.Note ?? "";

    entities.NcpNonCooperation.Populated = false;
    Update("UpdateNcpNonCooperation",
      (db, command) =>
      {
        db.SetNullableString(command, "ncpStatusCd", ncpStatusCode);
        db.SetNullableDate(command, "effectiveDt", effectiveDate);
        db.SetNullableString(command, "reasonCd", reasonCode);
        db.SetNullableDate(command, "letter1Dt", letter1Date);
        db.SetNullableString(command, "letter1Cd", letter1Code);
        db.SetNullableDate(command, "letter2Dt", letter2Date);
        db.SetNullableString(command, "letter2Cd", letter2Code);
        db.SetNullableDate(command, "phone1Dt", phone1Date);
        db.SetNullableString(command, "phone1Cd", phone1Code);
        db.SetNullableDate(command, "phone2Dt", phone2Date);
        db.SetNullableString(command, "phone2Cd", phone2Code);
        db.SetNullableDate(command, "endDt", endDate);
        db.SetNullableString(command, "endStatusCd", endStatusCode);
        db.SetNullableString(command, "lastUpdatedBy", lastUpdatedBy);
        db.
          SetNullableDateTime(command, "lastUpdatedTmst", lastUpdatedTimestamp);
          
        db.SetNullableString(command, "note", note);
        db.SetDateTime(
          command, "createdTimestamp",
          entities.NcpNonCooperation.CreatedTimestamp.GetValueOrDefault());
        db.
          SetString(command, "casNumber", entities.NcpNonCooperation.CasNumber);
          
        db.
          SetString(command, "cspNumber", entities.NcpNonCooperation.CspNumber);
          
      });

    entities.NcpNonCooperation.NcpStatusCode = ncpStatusCode;
    entities.NcpNonCooperation.EffectiveDate = effectiveDate;
    entities.NcpNonCooperation.ReasonCode = reasonCode;
    entities.NcpNonCooperation.Letter1Date = letter1Date;
    entities.NcpNonCooperation.Letter1Code = letter1Code;
    entities.NcpNonCooperation.Letter2Date = letter2Date;
    entities.NcpNonCooperation.Letter2Code = letter2Code;
    entities.NcpNonCooperation.Phone1Date = phone1Date;
    entities.NcpNonCooperation.Phone1Code = phone1Code;
    entities.NcpNonCooperation.Phone2Date = phone2Date;
    entities.NcpNonCooperation.Phone2Code = phone2Code;
    entities.NcpNonCooperation.EndDate = endDate;
    entities.NcpNonCooperation.EndStatusCode = endStatusCode;
    entities.NcpNonCooperation.LastUpdatedBy = lastUpdatedBy;
    entities.NcpNonCooperation.LastUpdatedTimestamp = lastUpdatedTimestamp;
    entities.NcpNonCooperation.Note = note;
    entities.NcpNonCooperation.Populated = true;
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
    /// A value of NcpNonCooperation.
    /// </summary>
    [JsonPropertyName("ncpNonCooperation")]
    public NcpNonCooperation NcpNonCooperation
    {
      get => ncpNonCooperation ??= new();
      set => ncpNonCooperation = value;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
    }

    private NcpNonCooperation ncpNonCooperation;
    private CsePersonsWorkSet ap;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of NcpNonCooperation.
    /// </summary>
    [JsonPropertyName("ncpNonCooperation")]
    public NcpNonCooperation NcpNonCooperation
    {
      get => ncpNonCooperation ??= new();
      set => ncpNonCooperation = value;
    }

    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public DateWorkArea Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of CaseUnitFound.
    /// </summary>
    [JsonPropertyName("caseUnitFound")]
    public Common CaseUnitFound
    {
      get => caseUnitFound ??= new();
      set => caseUnitFound = value;
    }

    private NcpNonCooperation ncpNonCooperation;
    private DateWorkArea blank;
    private DateWorkArea max;
    private DateWorkArea dateWorkArea;
    private DateWorkArea current;
    private Infrastructure infrastructure;
    private TextWorkArea textWorkArea;
    private Common caseUnitFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of NcpNonCooperation.
    /// </summary>
    [JsonPropertyName("ncpNonCooperation")]
    public NcpNonCooperation NcpNonCooperation
    {
      get => ncpNonCooperation ??= new();
      set => ncpNonCooperation = value;
    }

    /// <summary>
    /// A value of ArDelete.
    /// </summary>
    [JsonPropertyName("arDelete")]
    public CsePerson ArDelete
    {
      get => arDelete ??= new();
      set => arDelete = value;
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

    private NcpNonCooperation ncpNonCooperation;
    private CsePerson arDelete;
    private CaseRole apCaseRole;
    private CsePerson apCsePerson;
    private Case1 case1;
    private InterstateRequest interstateRequest;
    private CaseUnit caseUnit;
  }
#endregion
}

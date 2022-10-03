// Program: LE_B607_ENTRY_APPEAR_EXTRACT, ID: 945198227, model: 746.
// Short name: SWEB607P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: LE_B607_ENTRY_APPEAR_EXTRACT.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Batch)]
public partial class LeB607EntryAppearExtract: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_B607_ENTRY_APPEAR_EXTRACT program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeB607EntryAppearExtract(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeB607EntryAppearExtract.
  /// </summary>
  public LeB607EntryAppearExtract(IContext context, Import import, Export export)
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
    // -------------------------------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	----------	
    // ---------------------------------------------------------------------------------
    // 08/14/2013 DDupree	CQ41132 	Initial Development.
    // -------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.EabFileHandling.Action = "OPEN";
    UseLeB607WriteExtractData2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      return;
    }

    local.Processing.Date = Now().Date;
    local.Max.Date = new DateTime(2099, 12, 31);

    foreach(var item in ReadLegalAction3())
    {
      if (Equal(entities.LegalAction.StandardNumber, local.Prev.StandardNumber))
      {
        continue;
      }

      if (ReadFipsTribunal())
      {
        if (entities.Fips.State != 20)
        {
          continue;
        }
      }
      else
      {
        continue;
      }

      local.Case1.Number = "";

      foreach(var item1 in ReadCsePersonCsePersonCase2())
      {
        if (ReadCaseRole())
        {
          // ok there is an active ap role for this ap on this case
          local.Case1.Number = entities.Case1.Number;

          break;
        }
      }

      if (IsEmpty(local.Case1.Number))
      {
        continue;
      }

      local.Attrny.Text2 = "";
      local.EstablishmentOnly.Flag = "N";

      switch(TrimEnd(entities.Fips.CountyAbbreviation))
      {
        case "AL":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "AN":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "AT":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "BA":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "BT":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "BB":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "BR":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "BU":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "CS":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "CQ":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "CK":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "CN":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "CA":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "CY":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "CD":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "CF":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "CM":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "CL":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "CR":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "DC":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "DK":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "DP":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "DG":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "ED":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "EK":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "EL":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "EW":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "FI":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "FO":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "FR":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "GE":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "GO":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "GH":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "GT":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "GY":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "GL":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "GW":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "HM":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "HP":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "HV":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "HS":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "HG":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "JA":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "JF":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "JW":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "JO":
          local.Attrny.Text2 = "3";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "KE":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "KM":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "KW":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "LB":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "LE":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "LV":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "LC":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "LN":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "LG":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "LY":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "MP":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "MN":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "MS":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "ME":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "MI":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "MC":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "MG":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "MR":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "MT":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "NM":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "NO":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "NS":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "NT":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "OS":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "OB":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "OT":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "PN":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "PL":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "PT":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "PR":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "RA":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "RN":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "RP":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "RC":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "RL":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "RO":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "RH":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "RS":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "SA":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "SC":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "SG":
          local.Attrny.Text2 = "7";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "SW":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "SN":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "SD":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "SH":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "SM":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "SF":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "ST":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "SV":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "SU":
          local.Attrny.Text2 = "2";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "TH":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "TR":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "WB":
          local.Attrny.Text2 = "1";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "WA":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "WS":
          local.Attrny.Text2 = "6";
          local.EstablishmentOnly.Flag = "N";

          break;
        case "WH":
          local.Attrny.Text2 = "8";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "WL":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "WO":
          local.Attrny.Text2 = "4";
          local.EstablishmentOnly.Flag = "Y";

          break;
        case "WY":
          local.Attrny.Text2 = "5";
          local.EstablishmentOnly.Flag = "N";

          break;
        default:
          continue;
      }

      if (AsChar(local.EstablishmentOnly.Flag) == 'Y')
      {
        local.JclassFound.Flag = "";

        if (ReadLegalAction1())
        {
          local.JclassFound.Flag = "Y";
        }

        if (AsChar(local.JclassFound.Flag) == 'Y')
        {
          continue;
        }
        else
        {
          // ok we can process this establishment only county since no record 
          // was found
        }
      }

      for(local.ApUsed.Index = 0; local.ApUsed.Index < local.ApUsed.Count; ++
        local.ApUsed.Index)
      {
        if (!local.ApUsed.CheckSize())
        {
          break;
        }

        local.ApUsed.Update.Ap.Number = "";
      }

      local.ApUsed.CheckIndex();
      local.ApUsed.Index = -1;
      local.ApUsed.Count = 0;

      for(local.ArUsed.Index = 0; local.ArUsed.Index < local.ArUsed.Count; ++
        local.ArUsed.Index)
      {
        if (!local.ArUsed.CheckSize())
        {
          break;
        }

        local.ArUsed.Update.Ar.Number = "";
      }

      local.ArUsed.CheckIndex();
      local.ArUsed.Index = -1;
      local.ArUsed.Count = 0;

      for(local.ChUsed.Index = 0; local.ChUsed.Index < local.ChUsed.Count; ++
        local.ChUsed.Index)
      {
        if (!local.ChUsed.CheckSize())
        {
          break;
        }

        local.ChUsed.Update.Ch.Number = "";
      }

      local.ChUsed.CheckIndex();
      local.ChUsed.Index = -1;
      local.ChUsed.Count = 0;

      for(local.CaseUsed.Index = 0; local.CaseUsed.Index < local
        .CaseUsed.Count; ++local.CaseUsed.Index)
      {
        if (!local.CaseUsed.CheckSize())
        {
          break;
        }

        local.CaseUsed.Update.Used.Number = "";
      }

      local.CaseUsed.CheckIndex();
      local.CaseUsed.Index = -1;
      local.CaseUsed.Count = 0;
      local.AttInfo.Text200 = "";
      local.ApNumber.Text200 = "";
      local.ArNumber.Text200 = "";

      foreach(var item1 in ReadLegalAction2())
      {
        foreach(var item2 in ReadCsePersonCsePersonCase1())
        {
          local.Case1.Number = "";

          if (ReadCaseRole())
          {
            // ok there is an active ap role for this ap on this case
            local.Case1.Number = entities.Case1.Number;
          }
          else
          {
            continue;
          }

          local.CaseFound.Flag = "N";

          for(local.CaseUsed.Index = 0; local.CaseUsed.Index < local
            .CaseUsed.Count; ++local.CaseUsed.Index)
          {
            if (!local.CaseUsed.CheckSize())
            {
              break;
            }

            if (Equal(entities.Case1.Number, local.CaseUsed.Item.Used.Number))
            {
              // ALREADY WRITTEN A HIST RECORD SO WE DO NOT WANT WRITE IT AGAIN
              local.CaseFound.Flag = "Y";
            }
          }

          local.CaseUsed.CheckIndex();

          if (AsChar(local.CaseFound.Flag) == 'N')
          {
            local.Infrastructure.ReasonCode = "EOAMASS";
            local.Infrastructure.EventId = 20;
            local.Infrastructure.CsenetInOutCode = "";
            local.Infrastructure.ProcessStatus = "Q";
            local.Infrastructure.UserId = global.UserId;
            local.Infrastructure.BusinessObjectCd = "LEA";
            local.Infrastructure.DenormNumeric12 = entities.N2dRead.Identifier;
            local.Infrastructure.DenormText12 =
              entities.N2dRead.CourtCaseNumber;
            local.Infrastructure.ReferenceDate = Now().Date;
            local.Infrastructure.Detail =
              "A Entry of Appearance has been created for court order number : ";
              
            local.Infrastructure.Detail =
              Substring(local.Infrastructure.Detail, 1, 63) + entities
              .LegalAction.StandardNumber;
            local.Infrastructure.CreatedBy = global.UserId;
            local.Infrastructure.CreatedTimestamp = Now();
            local.Infrastructure.CaseNumber = entities.Case1.Number;
            local.Infrastructure.InitiatingStateCode = "KS";
            UseSpCabCreateInfrastructure();

            if (!IsExitState("ACO_NN0000_ALL_OK"))
            {
              return;
            }
            else
            {
              // -------------------
              // Continue processing
              // -------------------
            }

            local.CaseUsed.Index = local.CaseUsed.Index;
            local.CaseUsed.CheckSize();

            local.CaseUsed.Update.Used.Number = entities.Case1.Number;
          }

          local.ApFound.Flag = "N";

          for(local.ApUsed.Index = 0; local.ApUsed.Index < local.ApUsed.Count; ++
            local.ApUsed.Index)
          {
            if (!local.ApUsed.CheckSize())
            {
              break;
            }

            if (Equal(entities.ApCsePerson.Number, local.ApUsed.Item.Ap.Number))
            {
              local.ApFound.Flag = "Y";
            }
          }

          local.ApUsed.CheckIndex();

          if (AsChar(local.ApFound.Flag) == 'N')
          {
            for(local.ArUsed.Index = 0; local.ArUsed.Index < local
              .ArUsed.Count; ++local.ArUsed.Index)
            {
              if (!local.ArUsed.CheckSize())
              {
                break;
              }

              if (Equal(entities.ApCsePerson.Number, local.ArUsed.Item.Ar.Number))
                
              {
                goto Test1;
              }
            }

            local.ArUsed.CheckIndex();

            for(local.ChUsed.Index = 0; local.ChUsed.Index < local
              .ChUsed.Count; ++local.ChUsed.Index)
            {
              if (!local.ChUsed.CheckSize())
              {
                break;
              }

              if (Equal(entities.ApCsePerson.Number, local.ChUsed.Item.Ch.Number))
                
              {
                goto Test1;
              }
            }

            local.ChUsed.CheckIndex();

            if (ReadPersonPrivateAttorney1())
            {
              local.AttIdNumber.Text2 =
                NumberToString(entities.PersonPrivateAttorney.Identifier, 14, 2);
                
              local.AttInfo.Text200 = TrimEnd(local.AttInfo.Text200) + entities
                .ApCsePerson.Number + local.AttIdNumber.Text2;
            }
            else
            {
              if (Lt(local.Null1.Date, entities.ApCsePerson.DateOfDeath))
              {
                goto Read1;
              }

              local.ApNumber.Text200 = TrimEnd(local.ApNumber.Text200) + entities
                .ApCsePerson.Number;
            }

Read1:

            local.ApUsed.Index = local.ApUsed.Index;
            local.ApUsed.CheckSize();

            local.ApUsed.Update.Ap.Number = entities.ApCsePerson.Number;
          }

Test1:

          if (ReadCsePerson())
          {
            local.ArFound.Flag = "N";

            for(local.ArUsed.Index = 0; local.ArUsed.Index < local
              .ArUsed.Count; ++local.ArUsed.Index)
            {
              if (!local.ArUsed.CheckSize())
              {
                break;
              }

              if (Equal(entities.Ar.Number, local.ArUsed.Item.Ar.Number))
              {
                local.ArFound.Flag = "Y";
              }
            }

            local.ArUsed.CheckIndex();

            if (AsChar(local.ArFound.Flag) == 'N')
            {
              for(local.ApUsed.Index = 0; local.ApUsed.Index < local
                .ApUsed.Count; ++local.ApUsed.Index)
              {
                if (!local.ApUsed.CheckSize())
                {
                  break;
                }

                if (Equal(entities.Ar.Number, local.ApUsed.Item.Ap.Number))
                {
                  goto Read3;
                }
              }

              local.ApUsed.CheckIndex();

              for(local.ChUsed.Index = 0; local.ChUsed.Index < local
                .ChUsed.Count; ++local.ChUsed.Index)
              {
                if (!local.ChUsed.CheckSize())
                {
                  break;
                }

                if (Equal(entities.Ar.Number, local.ChUsed.Item.Ch.Number))
                {
                  goto Read3;
                }
              }

              local.ChUsed.CheckIndex();

              if (ReadPersonPrivateAttorney2())
              {
                local.AttIdNumber.Text2 =
                  NumberToString(entities.PersonPrivateAttorney.Identifier, 14,
                  2);
                local.AttInfo.Text200 = TrimEnd(local.AttInfo.Text200) + entities
                  .Ar.Number + local.AttIdNumber.Text2;
              }
              else
              {
                if (Lt(local.Null1.Date, entities.Ar.DateOfDeath) || AsChar
                  (entities.Ar.Type1) == 'O')
                {
                  goto Read2;
                }

                local.ArNumber.Text200 = TrimEnd(local.ArNumber.Text200) + entities
                  .Ar.Number;
              }

Read2:

              local.ArUsed.Index = local.ArUsed.Index;
              local.ArUsed.CheckSize();

              local.ArUsed.Update.Ar.Number = entities.Ar.Number;
            }
          }

Read3:

          local.ChFound.Flag = "N";

          for(local.ChUsed.Index = 0; local.ChUsed.Index < local.ChUsed.Count; ++
            local.ChUsed.Index)
          {
            if (!local.ChUsed.CheckSize())
            {
              break;
            }

            if (Equal(entities.ChCsePerson.Number, local.ChUsed.Item.Ch.Number))
            {
              local.ChFound.Flag = "Y";
            }
          }

          local.ChUsed.CheckIndex();

          if (AsChar(local.ChFound.Flag) == 'N')
          {
            for(local.ApUsed.Index = 0; local.ApUsed.Index < local
              .ApUsed.Count; ++local.ApUsed.Index)
            {
              if (!local.ApUsed.CheckSize())
              {
                break;
              }

              if (Equal(entities.ChCsePerson.Number, local.ApUsed.Item.Ap.Number))
                
              {
                goto Test2;
              }
            }

            local.ApUsed.CheckIndex();

            for(local.ArUsed.Index = 0; local.ArUsed.Index < local
              .ArUsed.Count; ++local.ArUsed.Index)
            {
              if (!local.ArUsed.CheckSize())
              {
                break;
              }

              if (Equal(entities.ChCsePerson.Number, local.ArUsed.Item.Ar.Number))
                
              {
                goto Test2;
              }
            }

            local.ArUsed.CheckIndex();

            if (ReadPersonPrivateAttorney3())
            {
              local.AttIdNumber.Text2 =
                NumberToString(entities.PersonPrivateAttorney.Identifier, 14, 2);
                
              local.AttInfo.Text200 = TrimEnd(local.AttInfo.Text200) + entities
                .ChCsePerson.Number + local.AttIdNumber.Text2;
            }

            local.ChUsed.Index = local.ChUsed.Index;
            local.ChUsed.CheckSize();

            local.ChUsed.Update.Ch.Number = entities.ChCsePerson.Number;
          }

Test2:
          ;
        }
      }

      if (Equal(entities.Fips.CountyAbbreviation, "JO"))
      {
        // not printing johnson county
      }
      else
      {
        local.EabFileHandling.Action = "WRITE";
        UseLeB607WriteExtractData1();

        if (!Equal(local.EabFileHandling.Status, "OK"))
        {
          return;
        }
      }

      local.Prev.StandardNumber = entities.LegalAction.StandardNumber;
    }

    // -------------------------------------------------------------------------------------------------------------------------
    // --  Close the Extract File.
    // -------------------------------------------------------------------------------------------------------------------------
    local.EabFileHandling.Action = "CLOSE";
    UseLeB607WriteExtractData2();

    if (!Equal(local.EabFileHandling.Status, "OK"))
    {
      ExitState = "ACO_NN0000_ABEND_FOR_BATCH";

      return;
    }

    ExitState = "ACO_NI0000_PROCESSING_COMPLETE";
  }

  private static void MoveFips(Fips source, Fips target)
  {
    target.CountyAbbreviation = source.CountyAbbreviation;
    target.County = source.County;
  }

  private static void MoveInfrastructure(Infrastructure source,
    Infrastructure target)
  {
    target.SituationNumber = source.SituationNumber;
    target.ProcessStatus = source.ProcessStatus;
    target.EventId = source.EventId;
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
    target.ReferenceDate = source.ReferenceDate;
    target.Detail = source.Detail;
  }

  private static void MoveLegalAction(LegalAction source, LegalAction target)
  {
    target.Identifier = source.Identifier;
    target.StandardNumber = source.StandardNumber;
  }

  private void UseLeB607WriteExtractData1()
  {
    var useImport = new LeB607WriteExtractData.Import();
    var useExport = new LeB607WriteExtractData.Export();

    MoveFips(entities.Fips, useImport.Fips);
    useImport.LeadAttrny.Text2 = local.Attrny.Text2;
    MoveLegalAction(entities.LegalAction, useImport.LegalAction);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useImport.AttorInfo.Text200 = local.AttInfo.Text200;
    useImport.ApNumber.Text200 = local.ApNumber.Text200;
    useImport.ArNumber.Text200 = local.ArNumber.Text200;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB607WriteExtractData.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseLeB607WriteExtractData2()
  {
    var useImport = new LeB607WriteExtractData.Import();
    var useExport = new LeB607WriteExtractData.Export();

    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(LeB607WriteExtractData.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseSpCabCreateInfrastructure()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    MoveInfrastructure(local.Infrastructure, useImport.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    local.Infrastructure.Assign(useExport.Infrastructure);
  }

  private bool ReadCaseRole()
  {
    entities.ApCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetNullableDate(
          command, "startDate", local.Processing.Date.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ApCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ApCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ApCaseRole.Type1 = db.GetString(reader, 2);
        entities.ApCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ApCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ApCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ApCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ApCaseRole.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.Ar.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "endDate", local.Processing.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Ar.Number = db.GetString(reader, 0);
        entities.Ar.Type1 = db.GetString(reader, 1);
        entities.Ar.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.Ar.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Ar.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonCsePersonCase1()
  {
    entities.Case1.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCsePersonCase1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.N2dRead.Identifier);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.ChCsePerson.Number = db.GetString(reader, 3);
        entities.ChCsePerson.Type1 = db.GetString(reader, 4);
        entities.ChCsePerson.DateOfDeath = db.GetNullableDate(reader, 5);
        entities.Case1.Number = db.GetString(reader, 6);
        entities.Case1.Status = db.GetNullableString(reader, 7);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 8);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 9);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 10);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 11);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 12);
        entities.Case1.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCsePersonCase2()
  {
    entities.Case1.Populated = false;
    entities.ApCsePerson.Populated = false;
    entities.ChCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCsePersonCase2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", entities.LegalAction.Identifier);
      },
      (db, reader) =>
      {
        entities.ApCsePerson.Number = db.GetString(reader, 0);
        entities.ApCsePerson.Type1 = db.GetString(reader, 1);
        entities.ApCsePerson.DateOfDeath = db.GetNullableDate(reader, 2);
        entities.ChCsePerson.Number = db.GetString(reader, 3);
        entities.ChCsePerson.Type1 = db.GetString(reader, 4);
        entities.ChCsePerson.DateOfDeath = db.GetNullableDate(reader, 5);
        entities.Case1.Number = db.GetString(reader, 6);
        entities.Case1.Status = db.GetNullableString(reader, 7);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 8);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 9);
        entities.Case1.CreatedTimestamp = db.GetDateTime(reader, 10);
        entities.Case1.InterstateCaseId = db.GetNullableString(reader, 11);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 12);
        entities.Case1.Populated = true;
        entities.ApCsePerson.Populated = true;
        entities.ChCsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ApCsePerson.Type1);
        CheckValid<CsePerson>("Type1", entities.ChCsePerson.Type1);

        return true;
      });
  }

  private bool ReadFipsTribunal()
  {
    System.Diagnostics.Debug.Assert(entities.LegalAction.Populated);
    entities.Fips.Populated = false;
    entities.Tribunal.Populated = false;

    return Read("ReadFipsTribunal",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.LegalAction.TrbId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Tribunal.FipState = db.GetNullableInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Tribunal.FipCounty = db.GetNullableInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Tribunal.FipLocation = db.GetNullableInt32(reader, 2);
        entities.Fips.CountyAbbreviation = db.GetNullableString(reader, 3);
        entities.Tribunal.Name = db.GetString(reader, 4);
        entities.Tribunal.JudicialDistrict = db.GetString(reader, 5);
        entities.Tribunal.Identifier = db.GetInt32(reader, 6);
        entities.Fips.Populated = true;
        entities.Tribunal.Populated = true;
      });
  }

  private bool ReadLegalAction1()
  {
    entities.N2dRead.Populated = false;

    return Read("ReadLegalAction1",
      (db, command) =>
      {
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.N2dRead.Identifier = db.GetInt32(reader, 0);
        entities.N2dRead.Classification = db.GetString(reader, 1);
        entities.N2dRead.FiledDate = db.GetNullableDate(reader, 2);
        entities.N2dRead.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.N2dRead.StandardNumber = db.GetNullableString(reader, 4);
        entities.N2dRead.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalAction2()
  {
    entities.N2dRead.Populated = false;

    return ReadEach("ReadLegalAction2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "standardNo", entities.LegalAction.StandardNumber ?? "");
      },
      (db, reader) =>
      {
        entities.N2dRead.Identifier = db.GetInt32(reader, 0);
        entities.N2dRead.Classification = db.GetString(reader, 1);
        entities.N2dRead.FiledDate = db.GetNullableDate(reader, 2);
        entities.N2dRead.CourtCaseNumber = db.GetNullableString(reader, 3);
        entities.N2dRead.StandardNumber = db.GetNullableString(reader, 4);
        entities.N2dRead.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalAction3()
  {
    entities.LegalAction.Populated = false;

    return ReadEach("ReadLegalAction3",
      null,
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.Type1 = db.GetString(reader, 3);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 4);
        entities.LegalAction.InitiatingState = db.GetNullableString(reader, 5);
        entities.LegalAction.RespondingState = db.GetNullableString(reader, 6);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 7);
        entities.LegalAction.EndDate = db.GetNullableDate(reader, 8);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 9);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 10);
        entities.LegalAction.EstablishmentCode =
          db.GetNullableString(reader, 11);
        entities.LegalAction.TrbId = db.GetNullableInt32(reader, 12);
        entities.LegalAction.KpcTribunalId = db.GetNullableInt32(reader, 13);
        entities.LegalAction.Populated = true;

        return true;
      });
  }

  private bool ReadPersonPrivateAttorney1()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNumber", entities.N2dRead.CourtCaseNumber ?? "");
        db.
          SetDate(command, "dateDismissed", local.Max.Date.GetValueOrDefault());
          
        db.SetString(command, "cspNumber", entities.ApCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 2);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPersonPrivateAttorney2()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNumber", entities.N2dRead.CourtCaseNumber ?? "");
        db.
          SetDate(command, "dateDismissed", local.Max.Date.GetValueOrDefault());
          
        db.SetString(command, "cspNumber", entities.Ar.Number);
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 2);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPrivateAttorney.Populated = true;
      });
  }

  private bool ReadPersonPrivateAttorney3()
  {
    entities.PersonPrivateAttorney.Populated = false;

    return Read("ReadPersonPrivateAttorney3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNumber", entities.N2dRead.CourtCaseNumber ?? "");
        db.
          SetDate(command, "dateDismissed", local.Max.Date.GetValueOrDefault());
          
        db.SetString(command, "cspNumber", entities.ChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.PersonPrivateAttorney.CspNumber = db.GetString(reader, 0);
        entities.PersonPrivateAttorney.Identifier = db.GetInt32(reader, 1);
        entities.PersonPrivateAttorney.DateDismissed = db.GetDate(reader, 2);
        entities.PersonPrivateAttorney.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.PersonPrivateAttorney.Populated = true;
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
    /// <summary>A ChUsedGroup group.</summary>
    [Serializable]
    public class ChUsedGroup
    {
      /// <summary>
      /// A value of Ch.
      /// </summary>
      [JsonPropertyName("ch")]
      public CsePerson Ch
      {
        get => ch ??= new();
        set => ch = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson ch;
    }

    /// <summary>A ArUsedGroup group.</summary>
    [Serializable]
    public class ArUsedGroup
    {
      /// <summary>
      /// A value of Ar.
      /// </summary>
      [JsonPropertyName("ar")]
      public CsePerson Ar
      {
        get => ar ??= new();
        set => ar = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson ar;
    }

    /// <summary>A ApUsedGroup group.</summary>
    [Serializable]
    public class ApUsedGroup
    {
      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CsePerson Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CsePerson ap;
    }

    /// <summary>A CaseUsedGroup group.</summary>
    [Serializable]
    public class CaseUsedGroup
    {
      /// <summary>
      /// A value of Used.
      /// </summary>
      [JsonPropertyName("used")]
      public Case1 Used
      {
        get => used ??= new();
        set => used = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Case1 used;
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
    /// A value of CaseFound.
    /// </summary>
    [JsonPropertyName("caseFound")]
    public Common CaseFound
    {
      get => caseFound ??= new();
      set => caseFound = value;
    }

    /// <summary>
    /// A value of ChFound.
    /// </summary>
    [JsonPropertyName("chFound")]
    public Common ChFound
    {
      get => chFound ??= new();
      set => chFound = value;
    }

    /// <summary>
    /// A value of AttIdNumber.
    /// </summary>
    [JsonPropertyName("attIdNumber")]
    public TextWorkArea AttIdNumber
    {
      get => attIdNumber ??= new();
      set => attIdNumber = value;
    }

    /// <summary>
    /// A value of ArFound.
    /// </summary>
    [JsonPropertyName("arFound")]
    public Common ArFound
    {
      get => arFound ??= new();
      set => arFound = value;
    }

    /// <summary>
    /// A value of ApFound.
    /// </summary>
    [JsonPropertyName("apFound")]
    public Common ApFound
    {
      get => apFound ??= new();
      set => apFound = value;
    }

    /// <summary>
    /// Gets a value of ChUsed.
    /// </summary>
    [JsonIgnore]
    public Array<ChUsedGroup> ChUsed => chUsed ??= new(ChUsedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ChUsed for json serialization.
    /// </summary>
    [JsonPropertyName("chUsed")]
    [Computed]
    public IList<ChUsedGroup> ChUsed_Json
    {
      get => chUsed;
      set => ChUsed.Assign(value);
    }

    /// <summary>
    /// Gets a value of ArUsed.
    /// </summary>
    [JsonIgnore]
    public Array<ArUsedGroup> ArUsed => arUsed ??= new(ArUsedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ArUsed for json serialization.
    /// </summary>
    [JsonPropertyName("arUsed")]
    [Computed]
    public IList<ArUsedGroup> ArUsed_Json
    {
      get => arUsed;
      set => ArUsed.Assign(value);
    }

    /// <summary>
    /// Gets a value of ApUsed.
    /// </summary>
    [JsonIgnore]
    public Array<ApUsedGroup> ApUsed => apUsed ??= new(ApUsedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ApUsed for json serialization.
    /// </summary>
    [JsonPropertyName("apUsed")]
    [Computed]
    public IList<ApUsedGroup> ApUsed_Json
    {
      get => apUsed;
      set => ApUsed.Assign(value);
    }

    /// <summary>
    /// A value of AttInfo.
    /// </summary>
    [JsonPropertyName("attInfo")]
    public WorkArea AttInfo
    {
      get => attInfo ??= new();
      set => attInfo = value;
    }

    /// <summary>
    /// A value of ArNumber.
    /// </summary>
    [JsonPropertyName("arNumber")]
    public WorkArea ArNumber
    {
      get => arNumber ??= new();
      set => arNumber = value;
    }

    /// <summary>
    /// A value of ApNumber.
    /// </summary>
    [JsonPropertyName("apNumber")]
    public WorkArea ApNumber
    {
      get => apNumber ??= new();
      set => apNumber = value;
    }

    /// <summary>
    /// A value of JclassFound.
    /// </summary>
    [JsonPropertyName("jclassFound")]
    public Common JclassFound
    {
      get => jclassFound ??= new();
      set => jclassFound = value;
    }

    /// <summary>
    /// A value of EstablishmentOnly.
    /// </summary>
    [JsonPropertyName("establishmentOnly")]
    public Common EstablishmentOnly
    {
      get => establishmentOnly ??= new();
      set => establishmentOnly = value;
    }

    /// <summary>
    /// A value of Attrny.
    /// </summary>
    [JsonPropertyName("attrny")]
    public TextWorkArea Attrny
    {
      get => attrny ??= new();
      set => attrny = value;
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
    /// A value of Prev.
    /// </summary>
    [JsonPropertyName("prev")]
    public LegalAction Prev
    {
      get => prev ??= new();
      set => prev = value;
    }

    /// <summary>
    /// Gets a value of CaseUsed.
    /// </summary>
    [JsonIgnore]
    public Array<CaseUsedGroup> CaseUsed => caseUsed ??= new(
      CaseUsedGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of CaseUsed for json serialization.
    /// </summary>
    [JsonPropertyName("caseUsed")]
    [Computed]
    public IList<CaseUsedGroup> CaseUsed_Json
    {
      get => caseUsed;
      set => CaseUsed.Assign(value);
    }

    /// <summary>
    /// A value of EabFileHandling.
    /// </summary>
    [JsonPropertyName("eabFileHandling")]
    public EabFileHandling EabFileHandling
    {
      get => eabFileHandling ??= new();
      set => eabFileHandling = value;
    }

    /// <summary>
    /// A value of EabReportSend.
    /// </summary>
    [JsonPropertyName("eabReportSend")]
    public EabReportSend EabReportSend
    {
      get => eabReportSend ??= new();
      set => eabReportSend = value;
    }

    /// <summary>
    /// A value of External.
    /// </summary>
    [JsonPropertyName("external")]
    public External External
    {
      get => external ??= new();
      set => external = value;
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
    /// A value of Processing.
    /// </summary>
    [JsonPropertyName("processing")]
    public DateWorkArea Processing
    {
      get => processing ??= new();
      set => processing = value;
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

    private DateWorkArea max;
    private Common caseFound;
    private Common chFound;
    private TextWorkArea attIdNumber;
    private Common arFound;
    private Common apFound;
    private Array<ChUsedGroup> chUsed;
    private Array<ArUsedGroup> arUsed;
    private Array<ApUsedGroup> apUsed;
    private WorkArea attInfo;
    private WorkArea arNumber;
    private WorkArea apNumber;
    private Common jclassFound;
    private Common establishmentOnly;
    private TextWorkArea attrny;
    private Case1 case1;
    private LegalAction prev;
    private Array<CaseUsedGroup> caseUsed;
    private EabFileHandling eabFileHandling;
    private EabReportSend eabReportSend;
    private External external;
    private DateWorkArea null1;
    private DateWorkArea processing;
    private Infrastructure infrastructure;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Ap2NdRead.
    /// </summary>
    [JsonPropertyName("ap2NdRead")]
    public CaseRole Ap2NdRead
    {
      get => ap2NdRead ??= new();
      set => ap2NdRead = value;
    }

    /// <summary>
    /// A value of PersonPrivateAttorney.
    /// </summary>
    [JsonPropertyName("personPrivateAttorney")]
    public PersonPrivateAttorney PersonPrivateAttorney
    {
      get => personPrivateAttorney ??= new();
      set => personPrivateAttorney = value;
    }

    /// <summary>
    /// A value of N2dRead.
    /// </summary>
    [JsonPropertyName("n2dRead")]
    public LegalAction N2dRead
    {
      get => n2dRead ??= new();
      set => n2dRead = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of Tribunal.
    /// </summary>
    [JsonPropertyName("tribunal")]
    public Tribunal Tribunal
    {
      get => tribunal ??= new();
      set => tribunal = value;
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
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
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
    /// A value of ApLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("apLegalActionCaseRole")]
    public LegalActionCaseRole ApLegalActionCaseRole
    {
      get => apLegalActionCaseRole ??= new();
      set => apLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of ChLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("chLegalActionCaseRole")]
    public LegalActionCaseRole ChLegalActionCaseRole
    {
      get => chLegalActionCaseRole ??= new();
      set => chLegalActionCaseRole = value;
    }

    private CaseRole ap2NdRead;
    private PersonPrivateAttorney personPrivateAttorney;
    private LegalAction n2dRead;
    private LegalAction legalAction;
    private CsePerson ar;
    private CaseRole caseRole;
    private Case1 case1;
    private Fips fips;
    private Tribunal tribunal;
    private CsePerson apCsePerson;
    private CsePerson chCsePerson;
    private CaseRole apCaseRole;
    private LegalActionCaseRole apLegalActionCaseRole;
    private CaseRole chCaseRole;
    private LegalActionCaseRole chLegalActionCaseRole;
  }
#endregion
}

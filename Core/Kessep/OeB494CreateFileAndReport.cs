// Program: OE_B494_CREATE_FILE_AND_REPORT, ID: 372971491, model: 746.
// Short name: SWE02482
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_B494_CREATE_FILE_AND_REPORT.
/// </summary>
[Serializable]
public partial class OeB494CreateFileAndReport: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_B494_CREATE_FILE_AND_REPORT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeB494CreateFileAndReport(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeB494CreateFileAndReport.
  /// </summary>
  public OeB494CreateFileAndReport(IContext context, Import import,
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
    local.ReportNeeded.Flag = "Y";
    local.EabFileHandling.Action = "WRITE";
    export.RecordCount.Count = import.RecordCount.Count;

    // *************************************************************************
    // * Retrieve information related to the CHILDREN
    // *************************************************************************
    for(local.ChildrenInformation.Index = 0; local.ChildrenInformation.Index < local
      .ChildrenInformation.Count; ++local.ChildrenInformation.Index)
    {
      local.ChildrenInformation.Update.Child.Assign(local.ChildClear);
    }

    local.AtLeastOneMedicaidPgm.Flag = "N";

    local.ChildrenInformation.Index = 0;
    local.ChildrenInformation.Clear();

    foreach(var item in ReadCsePerson2())
    {
      // *************************************************************************
      // * Retrieve information related to the CHILD
      // *************************************************************************
      local.ProgramFound.Flag = "N";

      if (ReadPersonProgram())
      {
        local.ProgramFound.Flag = "Y";
        local.AtLeastOneMedicaidPgm.Flag = "Y";
      }

      if (AsChar(local.ProgramFound.Flag) != 'Y')
      {
        local.ChildrenInformation.Next();

        continue;
      }

      local.ChildrenInformation.Update.Child.Number = entities.CsePerson.Number;
      UseEabReadCsePersonBatch3();

      switch(AsChar(local.AbendData.Type1))
      {
        case 'A':
          switch(TrimEnd(local.AbendData.AdabasResponseCd))
          {
            case "0113":
              ExitState = "FN0000_CSE_PERSON_UNKNOWN";
              local.NeededToWrite.RptDetail =
                "Adabas response code 113, person not found for " + entities
                .CsePerson.Number;

              break;
            case "0148":
              ExitState = "ADABAS_UNAVAILABLE_RB";
              local.NeededToWrite.RptDetail =
                "Adabas response code 148, unavailable fetching person " + entities
                .CsePerson.Number;

              break;
            default:
              ExitState = "ADABAS_READ_UNSUCCESSFUL";
              local.NeededToWrite.RptDetail = "Adabas error. Type = " + local
                .AbendData.Type1 + " File number = " + local
                .AbendData.AdabasFileNumber + " File action = " + local
                .AbendData.AdabasFileAction + " Response code = " + local
                .AbendData.AdabasResponseCd + " Person number = " + entities
                .CsePerson.Number;

              break;
          }

          break;
        case 'C':
          if (IsEmpty(local.AbendData.CicsResponseCd))
          {
          }
          else
          {
            ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
            local.NeededToWrite.RptDetail =
              "CICS error fetching person number  " + entities
              .CsePerson.Number;
          }

          break;
        case ' ':
          break;
        default:
          ExitState = "ADABAS_INVALID_RETURN_CODE";
          local.NeededToWrite.RptDetail =
            "Unknown error fetching person number  " + entities
            .CsePerson.Number;

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseCabErrorReport();
        local.ChildrenInformation.Next();

        return;
      }

      local.ChildrenInformation.Update.Child.FormattedName =
        TrimEnd(local.ChildrenInformation.Item.Child.LastName) + ", " + TrimEnd
        (local.ChildrenInformation.Item.Child.FirstName) + " " + local
        .ChildrenInformation.Item.Child.MiddleInitial + "";
      local.ChildrenInformation.Next();
    }

    if (AsChar(local.AtLeastOneMedicaidPgm.Flag) == 'N')
    {
      return;
    }

    // *************************************************************************
    // * Retrieve information related to the AR
    // *************************************************************************
    if (ReadCsePersonCase())
    {
      local.ArCsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseEabReadCsePersonBatch1();

      switch(AsChar(local.AbendData.Type1))
      {
        case 'A':
          switch(TrimEnd(local.AbendData.AdabasResponseCd))
          {
            case "0113":
              ExitState = "FN0000_CSE_PERSON_UNKNOWN";
              local.NeededToWrite.RptDetail =
                "Adabas response code 113, person not found for " + entities
                .CsePerson.Number;

              break;
            case "0148":
              ExitState = "ADABAS_UNAVAILABLE_RB";
              local.NeededToWrite.RptDetail =
                "Adabas response code 148, unavailable fetching person " + entities
                .CsePerson.Number;

              break;
            default:
              ExitState = "ADABAS_READ_UNSUCCESSFUL";
              local.NeededToWrite.RptDetail = "Adabas error. Type = " + local
                .AbendData.Type1 + " File number = " + local
                .AbendData.AdabasFileNumber + " File action = " + local
                .AbendData.AdabasFileAction + " Response code = " + local
                .AbendData.AdabasResponseCd + " Person number = " + entities
                .CsePerson.Number;

              break;
          }

          break;
        case 'C':
          if (IsEmpty(local.AbendData.CicsResponseCd))
          {
          }
          else
          {
            ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
            local.NeededToWrite.RptDetail =
              "CICS error fetching person number  " + entities
              .CsePerson.Number;
          }

          break;
        case ' ':
          break;
        default:
          ExitState = "ADABAS_INVALID_RETURN_CODE";
          local.NeededToWrite.RptDetail =
            "Unknown error fetching person number  " + entities
            .CsePerson.Number;

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseCabErrorReport();

        return;
      }

      local.ArCsePersonsWorkSet.FormattedName =
        TrimEnd(local.ArCsePersonsWorkSet.LastName) + ", " + TrimEnd
        (local.ArCsePersonsWorkSet.FirstName) + " " + local
        .ArCsePersonsWorkSet.MiddleInitial + "";

      if (ReadCsePersonAddress())
      {
        local.ArCsePersonAddress.Assign(entities.CsePersonAddress);
      }

      if (!ReadOldNewXref())
      {
        local.NeededToWrite.RptDetail = "No XREF Found for case number: " + entities
          .Case1.Number;
        UseCabErrorReport();
      }
    }
    else
    {
      local.NeededToWrite.RptDetail = "No AR Found for court order: " + import
        .Pers.StandardNumber;
      UseCabErrorReport();

      return;
    }

    // *************************************************************************
    // * Retrieve information related to the AR's EMPLOYER
    // *************************************************************************
    for(local.ArEmployerInfo.Index = 0; local.ArEmployerInfo.Index < local
      .ArEmployerInfo.Count; ++local.ArEmployerInfo.Index)
    {
      local.ArEmployerInfo.Update.ArEmployerEmployer.Assign(
        local.EmployerGrpClearEmployer);
      local.ArEmployerInfo.Update.ArEmployerEmployerAddress.Assign(
        local.EmployerGrpClearEmployerAddress);
    }

    local.ArEmployerInfo.Index = 0;
    local.ArEmployerInfo.Clear();

    foreach(var item in ReadIncomeSourceEmployer2())
    {
      local.ArEmployerInfo.Update.ArEmployerEmployer.Name =
        entities.Employer.Name;

      if (ReadEmployerAddress())
      {
        local.ArEmployerInfo.Update.ArEmployerEmployerAddress.Assign(
          entities.EmployerAddress);
      }

      local.ArEmployerInfo.Next();
    }

    // *************************************************************************
    // * Retrieve information related to the AR's POLICIES
    // *************************************************************************
    for(local.ArInsurancePolicy.Index = 0; local.ArInsurancePolicy.Index < local
      .ArInsurancePolicy.Count; ++local.ArInsurancePolicy.Index)
    {
      MoveHealthInsuranceCompany(local.InsGrp1Clear,
        local.ArInsurancePolicy.Update.ArInsGrp1);
      local.ArInsurancePolicy.Update.ArInsGrp2.Assign(local.InsGrp2Clear);
    }

    local.ArInsurancePolicy.Index = 0;
    local.ArInsurancePolicy.Clear();

    foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCoverage2())
    {
      MoveHealthInsuranceCompany(entities.HealthInsuranceCompany,
        local.ArInsurancePolicy.Update.ArInsGrp1);
      local.ArInsurancePolicy.Update.ArInsGrp2.Assign(
        entities.HealthInsuranceCoverage);
      local.ArInsurancePolicy.Next();
    }

    // *************************************************************************
    // * Retrieve information related to the AP
    // *************************************************************************
    if (ReadCsePerson1())
    {
      local.ApCsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseEabReadCsePersonBatch2();

      switch(AsChar(local.AbendData.Type1))
      {
        case 'A':
          switch(TrimEnd(local.AbendData.AdabasResponseCd))
          {
            case "0113":
              ExitState = "FN0000_CSE_PERSON_UNKNOWN";
              local.NeededToWrite.RptDetail =
                "Adabas response code 113, person not found for " + entities
                .CsePerson.Number;

              break;
            case "0148":
              ExitState = "ADABAS_UNAVAILABLE_RB";
              local.NeededToWrite.RptDetail =
                "Adabas response code 148, unavailable fetching person " + entities
                .CsePerson.Number;

              break;
            default:
              ExitState = "ADABAS_READ_UNSUCCESSFUL";
              local.NeededToWrite.RptDetail = "Adabas error. Type = " + local
                .AbendData.Type1 + " File number = " + local
                .AbendData.AdabasFileNumber + " File action = " + local
                .AbendData.AdabasFileAction + " Response code = " + local
                .AbendData.AdabasResponseCd + " Person number = " + entities
                .CsePerson.Number;

              break;
          }

          break;
        case 'C':
          if (IsEmpty(local.AbendData.CicsResponseCd))
          {
          }
          else
          {
            ExitState = "ACO_NE0000_CICS_UNAVAILABLE";
            local.NeededToWrite.RptDetail =
              "CICS error fetching person number  " + entities
              .CsePerson.Number;
          }

          break;
        case ' ':
          break;
        default:
          ExitState = "ADABAS_INVALID_RETURN_CODE";
          local.NeededToWrite.RptDetail =
            "Unknown error fetching person number  " + entities
            .CsePerson.Number;

          break;
      }

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseCabErrorReport();

        return;
      }

      if (ReadCsePersonAddress())
      {
        local.ApCsePersonAddress.Assign(entities.CsePersonAddress);
      }

      local.ApCsePersonsWorkSet.FormattedName =
        TrimEnd(local.ApCsePersonsWorkSet.LastName) + ", " + TrimEnd
        (local.ApCsePersonsWorkSet.FirstName) + " " + local
        .ApCsePersonsWorkSet.MiddleInitial + "";
    }
    else
    {
      local.NeededToWrite.RptDetail = "No AP Found for court order: " + import
        .Pers.StandardNumber;
      UseCabErrorReport();

      return;
    }

    // *************************************************************************
    // * Retrieve information related to the AP's EMPLOYER
    // *************************************************************************
    for(local.ApEmployerInfo.Index = 0; local.ApEmployerInfo.Index < local
      .ApEmployerInfo.Count; ++local.ApEmployerInfo.Index)
    {
      local.ApEmployerInfo.Update.ApEmployerEmployer.Assign(
        local.EmployerGrpClearEmployer);
      local.ApEmployerInfo.Update.ApEmployerEmployerAddress.Assign(
        local.EmployerGrpClearEmployerAddress);
    }

    local.ApEmployerInfo.Index = 0;
    local.ApEmployerInfo.Clear();

    foreach(var item in ReadIncomeSourceEmployer1())
    {
      local.ApEmployerInfo.Update.ApEmployerEmployer.Name =
        entities.Employer.Name;

      if (ReadEmployerAddress())
      {
        local.ApEmployerInfo.Update.ApEmployerEmployerAddress.Assign(
          entities.EmployerAddress);
      }

      local.ApEmployerInfo.Next();
    }

    // *************************************************************************
    // * Retrieve information related to the AP's POLICIES
    // *************************************************************************
    for(local.ApInsurancePolicy.Index = 0; local.ApInsurancePolicy.Index < local
      .ApInsurancePolicy.Count; ++local.ApInsurancePolicy.Index)
    {
      MoveHealthInsuranceCompany(local.InsGrp1Clear,
        local.ApInsurancePolicy.Update.ApInsGrp1);
      local.ApInsurancePolicy.Update.ApInsGrp2.Assign(local.InsGrp2Clear);
    }

    local.ApInsurancePolicy.Index = 0;
    local.ApInsurancePolicy.Clear();

    foreach(var item in ReadHealthInsuranceCompanyHealthInsuranceCoverage1())
    {
      MoveHealthInsuranceCompany(entities.HealthInsuranceCompany,
        local.ApInsurancePolicy.Update.ApInsGrp1);
      local.ApInsurancePolicy.Update.ApInsGrp2.Assign(
        entities.HealthInsuranceCoverage);
      local.ApInsurancePolicy.Next();
    }

    // *************************************************************************
    // * Write information to extract file
    // *************************************************************************
    ++export.RecordCount.Count;
    UseEabWriteMedicalSupportChange();

    if (AsChar(local.ReportNeeded.Flag) == 'Y')
    {
      if (export.RecordCount.Count > 1)
      {
        // ************************** NEWPAGE  ****************************
        local.EabFileHandling.Action = "NEWPAGE";
        UseCabBusinessReport01();
      }

      local.EabFileHandling.Action = "WRITE";

      // ************************** FILED DATE  ****************************
      local.NeededToWrite.RptDetail = "COURT ORDER NUMBER      " + import
        .Pers.StandardNumber;
      UseCabBusinessReport01();

      // ************************** FILED DATE  ****************************
      local.NeededToWrite.RptDetail = "ORDER FILED DATE" + "        " + NumberToString
        (Month(import.Pers.FiledDate), 14, 2) + "-" + NumberToString
        (Day(import.Pers.FiledDate), 14, 2) + "-" + NumberToString
        (Year(import.Pers.FiledDate), 12, 4) + "                       " + Substring
        (" ", 1, 1) + " " + Substring(" ", 1, 1) + " " + Substring
        (" ", 1, 1) + " " + Substring(" ", 1, 1);
      UseCabBusinessReport01();

      // ************************** END DATE  ****************************
      if (Equal(import.Pers.EndDate, import.Max.Date))
      {
        local.NeededToWrite.RptDetail = "ORDER END DATE";
      }
      else
      {
        local.NeededToWrite.RptDetail = "ORDER END DATE  " + "        " + NumberToString
          (Month(import.Pers.EndDate), 14, 2) + "-" + NumberToString
          (Day(import.Pers.EndDate), 14, 2) + "-" + NumberToString
          (Year(import.Pers.EndDate), 12, 4) + "                       " + Substring
          (" ", 1, 1) + " " + Substring(" ", 1, 1) + " " + Substring
          (" ", 1, 1) + " " + Substring(" ", 1, 1);
      }

      UseCabBusinessReport01();

      // *********************** BLANK LINE *************************
      local.NeededToWrite.RptDetail = "";
      UseCabBusinessReport01();

      // ************************** NUMBER  ****************************
      local.NeededToWrite.RptDetail = "PERSON NUMBER" + "           " + local
        .ApCsePersonsWorkSet.Number + "                        " + local
        .ArCsePersonsWorkSet.Number;
      UseCabBusinessReport01();

      // ************************** SSN
      // 
      // *******************************
      local.NeededToWrite.RptDetail = "SSN" + "                     " + Substring
        (local.ArCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 1, 3) +
        "-" + Substring
        (local.ArCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 4, 2) +
        "-" + Substring
        (local.ArCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4) +
        "                       " + Substring
        (local.ArCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 1, 3) +
        "-" + Substring
        (local.ArCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 4, 2) +
        "-" + Substring
        (local.ArCsePersonsWorkSet.Ssn, CsePersonsWorkSet.Ssn_MaxLength, 6, 4) +
        " " + Substring(" ", 1, 1);
      UseCabBusinessReport01();

      // ************************** NAME
      // 
      // *******************************
      local.NeededToWrite.RptDetail = "NAME" + "                    " + local
        .ApCsePersonsWorkSet.FormattedName + " " + local
        .ArCsePersonsWorkSet.FormattedName;
      UseCabBusinessReport01();

      // ************************** 
      // ADDR1
      // ******************************
      local.NeededToWrite.RptDetail = "ADDRESS" + "                 " + (
        local.ApCsePersonAddress.Street1 ?? "") + "         " + (
          local.ArCsePersonAddress.Street1 ?? "");
      UseCabBusinessReport01();

      // ************************** 
      // ADDR2
      // ******************************
      if (!IsEmpty(local.ApCsePersonAddress.Street2) || !
        IsEmpty(local.ArCsePersonAddress.Street2))
      {
        local.NeededToWrite.RptDetail = "       " + "                 " + (
          local.ApCsePersonAddress.Street2 ?? "") + "         " + (
            local.ArCsePersonAddress.Street2 ?? "");
        UseCabBusinessReport01();
      }

      // ************************ FOREIGN ****************************
      if (AsChar(local.ApCsePersonAddress.LocationType) == 'F' || AsChar
        (local.ArCsePersonAddress.LocationType) == 'F')
      {
        if (!IsEmpty(local.ApCsePersonAddress.Street3) || !
          IsEmpty(local.ArCsePersonAddress.Street3))
        {
          // ************************** 
          // ADDR3
          // ******************************
          local.NeededToWrite.RptDetail = "       " + "                 " + (
            local.ApCsePersonAddress.Street3 ?? "") + "         " + (
              local.ArCsePersonAddress.Street3 ?? "");
          UseCabBusinessReport01();
        }

        if (!IsEmpty(local.ApCsePersonAddress.Street4) || !
          IsEmpty(local.ArCsePersonAddress.Street4))
        {
          // ************************** 
          // ADDR4
          // ******************************
          local.NeededToWrite.RptDetail = "       " + "                 " + (
            local.ApCsePersonAddress.Street4 ?? "") + "         " + (
              local.ArCsePersonAddress.Street4 ?? "");
          UseCabBusinessReport01();
        }

        // ************************** CITY
        // 
        // *******************************
        if (AsChar(local.ApCsePersonAddress.LocationType) == 'F')
        {
          local.ApFormattedCityState.Text30 =
            TrimEnd(local.ApCsePersonAddress.City) + ", " + TrimEnd
            (local.ApCsePersonAddress.Province) + " " + (
              local.ApCsePersonAddress.PostalCode ?? "");
        }
        else
        {
          local.ApFormattedCityState.Text30 =
            TrimEnd(local.ApCsePersonAddress.City) + ", " + (
              local.ApCsePersonAddress.State ?? "") + " " + (
              local.ApCsePersonAddress.ZipCode ?? "");

          if (!IsEmpty(local.ApCsePersonAddress.Zip4))
          {
            local.ApFormattedCityState.Text30 =
              TrimEnd(local.ApFormattedCityState.Text30) + "-" + (
                local.ApCsePersonAddress.Zip4 ?? "");
          }
        }

        if (AsChar(local.ArCsePersonAddress.LocationType) == 'F')
        {
          local.ApFormattedCityState.Text30 =
            TrimEnd(local.ArCsePersonAddress.City) + ", " + TrimEnd
            (local.ArCsePersonAddress.Province) + " " + (
              local.ArCsePersonAddress.PostalCode ?? "");
        }
        else
        {
          local.ArFormattedCityState.Text30 =
            TrimEnd(local.ArCsePersonAddress.City) + ", " + (
              local.ArCsePersonAddress.State ?? "") + " " + (
              local.ArCsePersonAddress.ZipCode ?? "");

          if (!IsEmpty(local.ArCsePersonAddress.Zip4))
          {
            local.ArFormattedCityState.Text30 =
              TrimEnd(local.ArFormattedCityState.Text30) + "-" + (
                local.ArCsePersonAddress.Zip4 ?? "");
          }
        }
      }
      else
      {
        // ************************** CITY
        // 
        // *******************************
        local.ApFormattedCityState.Text30 =
          TrimEnd(local.ApCsePersonAddress.City) + ", " + (
            local.ApCsePersonAddress.State ?? "") + " " + (
            local.ApCsePersonAddress.ZipCode ?? "");

        if (!IsEmpty(local.ApCsePersonAddress.Zip4))
        {
          local.ApFormattedCityState.Text30 =
            TrimEnd(local.ApFormattedCityState.Text30) + "-" + (
              local.ApCsePersonAddress.Zip4 ?? "");
        }

        local.ArFormattedCityState.Text30 =
          TrimEnd(local.ArCsePersonAddress.City) + ", " + (
            local.ArCsePersonAddress.State ?? "") + " " + (
            local.ArCsePersonAddress.ZipCode ?? "");

        if (!IsEmpty(local.ArCsePersonAddress.Zip4))
        {
          local.ArFormattedCityState.Text30 =
            TrimEnd(local.ArFormattedCityState.Text30) + "-" + (
              local.ArCsePersonAddress.Zip4 ?? "");
        }
      }

      local.NeededToWrite.RptDetail = "CITY, S" + "T, ZIP           " + local
        .ApFormattedCityState.Text30 + "    " + local
        .ArFormattedCityState.Text30;
      UseCabBusinessReport01();

      // *********************** BLANK LINE *************************
      local.NeededToWrite.RptDetail = "";
      UseCabBusinessReport01();

      // ************************** CASE
      // 
      // *******************************
      local.NeededToWrite.RptDetail = "CSE CASE NUMBER" + "         " + entities
        .Case1.Number + "    " + "";
      UseCabBusinessReport01();

      // ************************** AE CASE ****************************
      local.NeededToWrite.RptDetail = "AE CASE NUMBER " + "         " + entities
        .OldNewXref.CaecsesCaseNbr + "    " + "";
      UseCabBusinessReport01();

      // *********************** BLANK LINE *************************
      local.NeededToWrite.RptDetail = "";
      UseCabBusinessReport01();

      // ************************** EMPLOYER ***************************
      for(local.ArEmployerInfo.Index = 0; local.ArEmployerInfo.Index < local
        .ArEmployerInfo.Count; ++local.ArEmployerInfo.Index)
      {
        // ********************* EMPLOYER
        // NAME
        // ******************************
        local.NeededToWrite.RptDetail = "EMPLOYER NAME  " + "         " + "                                  " +
          (local.ArEmployerInfo.Item.ArEmployerEmployer.Name ?? "") + "";
        UseCabBusinessReport01();

        // ************************** 
        // ADDR1
        // ******************************
        if (!IsEmpty(local.ArEmployerInfo.Item.ArEmployerEmployerAddress.Street1))
          
        {
          local.NeededToWrite.RptDetail = "ADDRESS" + "                 " + "                                  " +
            (
              local.ArEmployerInfo.Item.ArEmployerEmployerAddress.Street1 ?? ""
            ) + "";
          UseCabBusinessReport01();
        }

        // ************************** 
        // ADDR2
        // ******************************
        if (!IsEmpty(local.ArEmployerInfo.Item.ArEmployerEmployerAddress.Street2))
          
        {
          local.NeededToWrite.RptDetail = "       " + "                 " + "                                  " +
            (
              local.ArEmployerInfo.Item.ArEmployerEmployerAddress.Street2 ?? ""
            ) + "";
          UseCabBusinessReport01();
        }

        // ************************ FOREIGN ****************************
        if (AsChar(local.ArEmployerInfo.Item.ArEmployerEmployerAddress.
          LocationType) == 'F')
        {
          if (!IsEmpty(local.ArEmployerInfo.Item.ArEmployerEmployerAddress.
            Street3))
          {
            // ************************** 
            // ADDR3
            // ******************************
            local.NeededToWrite.RptDetail = "       " + "                 " + "                                  " +
              (
                local.ArEmployerInfo.Item.ArEmployerEmployerAddress.Street3 ?? ""
              ) + "";
            UseCabBusinessReport01();
          }

          if (!IsEmpty(local.ArEmployerInfo.Item.ArEmployerEmployerAddress.
            Street4))
          {
            // ************************** 
            // ADDR4
            // ******************************
            local.NeededToWrite.RptDetail = "       " + "                 " + "                                  " +
              (
                local.ArEmployerInfo.Item.ArEmployerEmployerAddress.Street4 ?? ""
              ) + "";
            UseCabBusinessReport01();
          }

          // *********************** FOREIGN CITY ****************************
          local.ArFormattedCityState.Text30 =
            TrimEnd(local.ArEmployerInfo.Item.ArEmployerEmployerAddress.City) +
            ", " + TrimEnd
            (local.ArEmployerInfo.Item.ArEmployerEmployerAddress.Province) + " " +
            (local.ArEmployerInfo.Item.ArEmployerEmployerAddress.PostalCode ?? ""
            );
          local.NeededToWrite.RptDetail = "CITY, P" + "ROVINCE, POST    " + "                                  " +
            local.ArFormattedCityState.Text30 + "";
          UseCabBusinessReport01();
        }
        else
        {
          // ************************** CITY
          // 
          // *******************************
          local.ArFormattedCityState.Text30 =
            TrimEnd(local.ArEmployerInfo.Item.ArEmployerEmployerAddress.City) +
            ", " + (
              local.ArEmployerInfo.Item.ArEmployerEmployerAddress.State ?? ""
            ) + " " + (
              local.ArEmployerInfo.Item.ArEmployerEmployerAddress.ZipCode ?? ""
            );
          local.NeededToWrite.RptDetail = "CITY, S" + "TATE, ZIP        " + "                                  " +
            local.ArFormattedCityState.Text30 + "";
          UseCabBusinessReport01();
        }

        // ************************** 
        // PHONE
        // ******************************
        if (!IsEmpty(local.ArEmployerInfo.Item.ArEmployerEmployer.PhoneNo))
        {
          local.NeededToWrite.RptDetail = "PHONE NUMBER            " + "                                  (" +
            NumberToString
            (local.ArEmployerInfo.Item.ArEmployerEmployer.AreaCode.
              GetValueOrDefault(), 13, 3) + ") " + Substring
            (local.ArEmployerInfo.Item.ArEmployerEmployer.PhoneNo, 7, 1, 3) + "-"
            + Substring
            (local.ArEmployerInfo.Item.ArEmployerEmployer.PhoneNo, 7, 4, 4);
          UseCabBusinessReport01();
        }

        // *********************** BLANK LINE *************************
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport01();
      }

      for(local.ApEmployerInfo.Index = 0; local.ApEmployerInfo.Index < local
        .ApEmployerInfo.Count; ++local.ApEmployerInfo.Index)
      {
        // ********************* EMPLOYER
        // NAME
        // ******************************
        local.NeededToWrite.RptDetail = "EMPLOYER NAME  " + "         " + (
          local.ApEmployerInfo.Item.ApEmployerEmployer.Name ?? "") + "    " + ""
          ;
        UseCabBusinessReport01();

        // ************************** 
        // ADDR1
        // ******************************
        if (!IsEmpty(local.ApEmployerInfo.Item.ApEmployerEmployerAddress.Street1))
          
        {
          local.NeededToWrite.RptDetail = "ADDRESS" + "                 " + (
            local.ApEmployerInfo.Item.ApEmployerEmployerAddress.Street1 ?? ""
            ) + "" + "";
          UseCabBusinessReport01();
        }

        // ************************** 
        // ADDR2
        // ******************************
        if (!IsEmpty(local.ApEmployerInfo.Item.ApEmployerEmployerAddress.Street2))
          
        {
          local.NeededToWrite.RptDetail = "       " + "                 " + (
            local.ApEmployerInfo.Item.ApEmployerEmployerAddress.Street2 ?? ""
            ) + "" + "";
          UseCabBusinessReport01();
        }

        // ************************ FOREIGN ****************************
        if (AsChar(local.ApEmployerInfo.Item.ApEmployerEmployerAddress.
          LocationType) == 'F')
        {
          if (!IsEmpty(local.ApEmployerInfo.Item.ApEmployerEmployerAddress.
            Street3))
          {
            // ************************** 
            // ADDR3
            // ******************************
            local.NeededToWrite.RptDetail = "       " + "                 " + (
              local.ApEmployerInfo.Item.ApEmployerEmployerAddress.Street3 ?? ""
              ) + "" + "";
            UseCabBusinessReport01();
          }

          if (!IsEmpty(local.ApEmployerInfo.Item.ApEmployerEmployerAddress.
            Street4))
          {
            // ************************** 
            // ADDR4
            // ******************************
            local.NeededToWrite.RptDetail = "       " + "                 " + (
              local.ApEmployerInfo.Item.ApEmployerEmployerAddress.Street4 ?? ""
              ) + "" + "";
            UseCabBusinessReport01();
          }

          // *********************** FOREIGN CITY ****************************
          local.ApFormattedCityState.Text30 =
            TrimEnd(local.ApEmployerInfo.Item.ApEmployerEmployerAddress.City) +
            ", " + TrimEnd
            (local.ApEmployerInfo.Item.ApEmployerEmployerAddress.Province) + " " +
            (local.ApEmployerInfo.Item.ApEmployerEmployerAddress.PostalCode ?? ""
            );
          local.NeededToWrite.RptDetail = "CITY, P" + "ROVINCE, POST    " + local
            .ApFormattedCityState.Text30 + "    " + "";
          UseCabBusinessReport01();
        }
        else
        {
          // ************************** CITY
          // 
          // *******************************
          local.ApFormattedCityState.Text30 =
            TrimEnd(local.ApEmployerInfo.Item.ApEmployerEmployerAddress.City) +
            ", " + (
              local.ApEmployerInfo.Item.ApEmployerEmployerAddress.State ?? ""
            ) + " " + (
              local.ApEmployerInfo.Item.ApEmployerEmployerAddress.ZipCode ?? ""
            );
          local.NeededToWrite.RptDetail = "CITY, S" + "TATE, ZIP        " + local
            .ApFormattedCityState.Text30 + "    " + "";
          UseCabBusinessReport01();
        }

        // ************************** 
        // PHONE
        // ******************************
        if (!IsEmpty(local.ApEmployerInfo.Item.ApEmployerEmployer.PhoneNo))
        {
          local.NeededToWrite.RptDetail = "PHONE NUMBER            " + "(" + NumberToString
            (local.ApEmployerInfo.Item.ApEmployerEmployer.AreaCode.
              GetValueOrDefault(), 13, 3) + ") " + Substring
            (local.ApEmployerInfo.Item.ApEmployerEmployer.PhoneNo, 7, 1, 3) + "-"
            + Substring
            (local.ApEmployerInfo.Item.ApEmployerEmployer.PhoneNo, 7, 4, 4);
          UseCabBusinessReport01();
        }

        // *********************** BLANK LINE *************************
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport01();
      }

      // ************************** CHILDREN ***************************
      for(local.ChildrenInformation.Index = 0; local
        .ChildrenInformation.Index < local.ChildrenInformation.Count; ++
        local.ChildrenInformation.Index)
      {
        // ********************* CHILD'S 
        // NAME
        // ******************************
        local.NeededToWrite.RptDetail = "CHILD'S NAME   " + "         " + local
          .ChildrenInformation.Item.Child.FormattedName + "    " + "";
        UseCabBusinessReport01();

        // ************************** SSN
        // 
        // *******************************
        local.NeededToWrite.RptDetail = "SSN" + "                     " + Substring
          (local.ChildrenInformation.Item.Child.Ssn,
          CsePersonsWorkSet.Ssn_MaxLength, 1, 3) + "-" + Substring
          (local.ChildrenInformation.Item.Child.Ssn,
          CsePersonsWorkSet.Ssn_MaxLength, 4, 2) + "-" + Substring
          (local.ChildrenInformation.Item.Child.Ssn,
          CsePersonsWorkSet.Ssn_MaxLength, 6, 4) + "                       " + Substring
          (" ", 1, 1) + " " + Substring(" ", 1, 1) + " " + Substring
          (" ", 1, 1) + " " + Substring(" ", 1, 1);
        UseCabBusinessReport01();

        // ************************** DOB
        // 
        // *******************************
        local.NeededToWrite.RptDetail = "DOB" + "                     " + NumberToString
          (Month(local.ChildrenInformation.Item.Child.Dob), 14, 2) + "-" + NumberToString
          (Day(local.ChildrenInformation.Item.Child.Dob), 14, 2) + "-" + NumberToString
          (Year(local.ChildrenInformation.Item.Child.Dob), 12, 4) + "                       " +
          Substring(" ", 1, 1) + " " + Substring(" ", 1, 1) + " " + Substring
          (" ", 1, 1) + " " + Substring(" ", 1, 1);
        UseCabBusinessReport01();

        // *********************** BLANK LINE *************************
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport01();
      }

      // ************************** POLICIES ***************************
      for(local.ArInsurancePolicy.Index = 0; local.ArInsurancePolicy.Index < local
        .ArInsurancePolicy.Count; ++local.ArInsurancePolicy.Index)
      {
        // *********************** CARRIER CODE *************************
        local.NeededToWrite.RptDetail = "CARRIER CODE   " + "                                           " +
          (local.ArInsurancePolicy.Item.ArInsGrp1.CarrierCode ?? "") + "    " +
          "";
        UseCabBusinessReport01();

        // *********************** CARRIER NAME *************************
        local.NeededToWrite.RptDetail = "CARRIER NAME   " + "                                           " +
          (
            local.ArInsurancePolicy.Item.ArInsGrp1.InsurancePolicyCarrier ?? ""
          ) + "    " + "";
        UseCabBusinessReport01();

        // *********************** POLICY NUMBER *************************
        local.NeededToWrite.RptDetail = "POLICY NUMBER  " + "                                           " +
          (
            local.ArInsurancePolicy.Item.ArInsGrp2.InsurancePolicyNumber ?? ""
          ) + "    " + "";
        UseCabBusinessReport01();

        // *********************** GROUP NUMBER *************************
        local.NeededToWrite.RptDetail = "GROUP NUMBER   " + "                                           " +
          (
            local.ArInsurancePolicy.Item.ArInsGrp2.InsuranceGroupNumber ?? ""
          ) + "    " + "";
        UseCabBusinessReport01();

        // ************************** EFFECTIVE DATE *************************
        local.NeededToWrite.RptDetail = "EFFECTIVE DATE" + "                                            " +
          NumberToString
          (Month(local.ArInsurancePolicy.Item.ArInsGrp2.PolicyEffectiveDate),
          14, 2) + "-" + NumberToString
          (Day(local.ArInsurancePolicy.Item.ArInsGrp2.PolicyEffectiveDate), 14,
          2) + "-" + NumberToString
          (Year(local.ArInsurancePolicy.Item.ArInsGrp2.PolicyEffectiveDate), 12,
          4) + "" + Substring(" ", 1, 1) + " " + Substring(" ", 1, 1) + " " + Substring
          (" ", 1, 1) + " " + Substring(" ", 1, 1);
        UseCabBusinessReport01();

        // ************************** END
        // DATE
        // ******************************
        local.NeededToWrite.RptDetail = "END DATE      " + "                                            " +
          NumberToString
          (Month(local.ArInsurancePolicy.Item.ArInsGrp2.PolicyExpirationDate),
          14, 2) + "-" + NumberToString
          (Day(local.ArInsurancePolicy.Item.ArInsGrp2.PolicyExpirationDate), 14,
          2) + "-" + NumberToString
          (Year(local.ArInsurancePolicy.Item.ArInsGrp2.PolicyExpirationDate),
          12, 4) + "" + Substring(" ", 1, 1) + " " + Substring(" ", 1, 1) + " " +
          Substring(" ", 1, 1) + " " + Substring(" ", 1, 1);
        UseCabBusinessReport01();

        // ************************** COVERAGE ***************************
        local.NeededToWrite.RptDetail = Substring("COVERAGE CODES", 1, 14) + "                                            " +
          (local.ArInsurancePolicy.Item.ArInsGrp2.CoverageCode1 ?? "") + " " + (
            local.ArInsurancePolicy.Item.ArInsGrp2.CoverageCode2 ?? "") + " " +
          (local.ArInsurancePolicy.Item.ArInsGrp2.CoverageCode3 ?? "") + " " + (
            local.ArInsurancePolicy.Item.ArInsGrp2.CoverageCode4 ?? "") + " " +
          (local.ArInsurancePolicy.Item.ArInsGrp2.CoverageCode5 ?? "") + " " + (
            local.ArInsurancePolicy.Item.ArInsGrp2.CoverageCode6 ?? "") + " " +
          (local.ArInsurancePolicy.Item.ArInsGrp2.CoverageCode7 ?? "");
        UseCabBusinessReport01();

        // *********************** BLANK LINE *************************
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport01();
      }

      for(local.ApInsurancePolicy.Index = 0; local.ApInsurancePolicy.Index < local
        .ApInsurancePolicy.Count; ++local.ApInsurancePolicy.Index)
      {
        // *********************** CARRIER CODE *************************
        local.NeededToWrite.RptDetail = "CARRIER CODE   " + "         " + (
          local.ApInsurancePolicy.Item.ApInsGrp1.CarrierCode ?? "") + "    " + ""
          ;
        UseCabBusinessReport01();

        // *********************** CARRIER NAME *************************
        local.NeededToWrite.RptDetail = "CARRIER NAME   " + "         " + (
          local.ApInsurancePolicy.Item.ApInsGrp1.InsurancePolicyCarrier ?? ""
          ) + "    " + "";
        UseCabBusinessReport01();

        // *********************** POLICY NUMBER *************************
        local.NeededToWrite.RptDetail = "POLICY NUMBER  " + "         " + (
          local.ApInsurancePolicy.Item.ApInsGrp2.InsurancePolicyNumber ?? ""
          ) + "    " + "";
        UseCabBusinessReport01();

        // *********************** GROUP NUMBER *************************
        local.NeededToWrite.RptDetail = "GROUP NUMBER   " + "         " + (
          local.ApInsurancePolicy.Item.ApInsGrp2.InsuranceGroupNumber ?? "") + "    " +
          "";
        UseCabBusinessReport01();

        // ************************** EFFECTIVE DATE *************************
        local.NeededToWrite.RptDetail = "EFFECTIVE DATE" + "          " + NumberToString
          (Month(local.ApInsurancePolicy.Item.ApInsGrp2.PolicyEffectiveDate),
          14, 2) + "-" + NumberToString
          (Day(local.ApInsurancePolicy.Item.ApInsGrp2.PolicyEffectiveDate), 14,
          2) + "-" + NumberToString
          (Year(local.ApInsurancePolicy.Item.ApInsGrp2.PolicyEffectiveDate), 12,
          4) + "                       " + Substring(" ", 1, 1) + " " + Substring
          (" ", 1, 1) + " " + Substring(" ", 1, 1) + " " + Substring
          (" ", 1, 1);
        UseCabBusinessReport01();

        // ************************** END
        // DATE
        // ******************************
        local.NeededToWrite.RptDetail = "END DATE      " + "          " + NumberToString
          (Month(local.ApInsurancePolicy.Item.ApInsGrp2.PolicyExpirationDate),
          14, 2) + "-" + NumberToString
          (Day(local.ApInsurancePolicy.Item.ApInsGrp2.PolicyExpirationDate), 14,
          2) + "-" + NumberToString
          (Year(local.ApInsurancePolicy.Item.ApInsGrp2.PolicyExpirationDate),
          12, 4) + "                       " + Substring(" ", 1, 1) + " " + Substring
          (" ", 1, 1) + " " + Substring(" ", 1, 1) + " " + Substring
          (" ", 1, 1);
        UseCabBusinessReport01();

        // ************************** COVERAGE ***************************
        local.NeededToWrite.RptDetail = Substring("COVERAGE CODES", 1, 14) + "          " +
          (local.ApInsurancePolicy.Item.ApInsGrp2.CoverageCode1 ?? "") + " " + (
            local.ApInsurancePolicy.Item.ApInsGrp2.CoverageCode2 ?? "") + " " +
          (local.ApInsurancePolicy.Item.ApInsGrp2.CoverageCode3 ?? "") + " " + (
            local.ApInsurancePolicy.Item.ApInsGrp2.CoverageCode4 ?? "") + " " +
          (local.ApInsurancePolicy.Item.ApInsGrp2.CoverageCode5 ?? "") + " " + (
            local.ApInsurancePolicy.Item.ApInsGrp2.CoverageCode6 ?? "") + " " +
          (local.ApInsurancePolicy.Item.ApInsGrp2.CoverageCode7 ?? "");
        UseCabBusinessReport01();

        // *********************** BLANK LINE *************************
        local.NeededToWrite.RptDetail = "";
        UseCabBusinessReport01();
      }
    }
  }

  private static void MoveApEmployerInfo(Local.ApEmployerInfoGroup source,
    EabWriteMedicalSupportChange.Import.EmployerInfoGroup target)
  {
    target.EmployerEmployer.Assign(source.ApEmployerEmployer);
    target.EmployerEmployerAddress.Assign(source.ApEmployerEmployerAddress);
  }

  private static void MoveApInsurancePolicy(Local.ApInsurancePolicyGroup source,
    EabWriteMedicalSupportChange.Import.InsurancePolicyGroup target)
  {
    MoveHealthInsuranceCompany(source.ApInsGrp1,
      target.InsHealthInsuranceCompany);
    target.InsHealthInsuranceCoverage.Assign(source.ApInsGrp2);
  }

  private static void MoveChildrenInformation(Local.
    ChildrenInformationGroup source,
    EabWriteMedicalSupportChange.Import.ChildrenInformationGroup target)
  {
    target.Child.Assign(source.Child);
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.LastName = source.LastName;
  }

  private static void MoveHealthInsuranceCompany(HealthInsuranceCompany source,
    HealthInsuranceCompany target)
  {
    target.CarrierCode = source.CarrierCode;
    target.InsurancePolicyCarrier = source.InsurancePolicyCarrier;
  }

  private void UseCabBusinessReport01()
  {
    var useImport = new CabBusinessReport01.Import();
    var useExport = new CabBusinessReport01.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabBusinessReport01.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseCabErrorReport()
  {
    var useImport = new CabErrorReport.Import();
    var useExport = new CabErrorReport.Export();

    useImport.NeededToWrite.RptDetail = local.NeededToWrite.RptDetail;
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;

    Call(CabErrorReport.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private void UseEabReadCsePersonBatch1()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ArCsePersonsWorkSet.Number;
    useExport.CsePersonsWorkSet.Assign(local.ArCsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.ArCsePersonsWorkSet);
      
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabReadCsePersonBatch2()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number = local.ApCsePersonsWorkSet.Number;
    useExport.CsePersonsWorkSet.Assign(local.ApCsePersonsWorkSet);
    useExport.AbendData.Assign(local.AbendData);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet, local.ApCsePersonsWorkSet);
      
    local.AbendData.Assign(useExport.AbendData);
  }

  private void UseEabReadCsePersonBatch3()
  {
    var useImport = new EabReadCsePersonBatch.Import();
    var useExport = new EabReadCsePersonBatch.Export();

    useImport.CsePersonsWorkSet.Number =
      local.ChildrenInformation.Item.Child.Number;
    useExport.AbendData.Assign(local.AbendData);
    useExport.CsePersonsWorkSet.Assign(local.ChildrenInformation.Item.Child);

    Call(EabReadCsePersonBatch.Execute, useImport, useExport);

    local.AbendData.Assign(useExport.AbendData);
    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      local.ChildrenInformation.Update.Child);
  }

  private void UseEabWriteMedicalSupportChange()
  {
    var useImport = new EabWriteMedicalSupportChange.Import();
    var useExport = new EabWriteMedicalSupportChange.Export();

    useImport.OldNewXref.CaecsesCaseNbr = entities.OldNewXref.CaecsesCaseNbr;
    useImport.Case1.Number = entities.Case1.Number;
    local.ApInsurancePolicy.CopyTo(
      useImport.InsurancePolicy, MoveApInsurancePolicy);
    local.ChildrenInformation.CopyTo(
      useImport.ChildrenInformation, MoveChildrenInformation);
    local.ApEmployerInfo.CopyTo(useImport.EmployerInfo, MoveApEmployerInfo);
    useImport.ArCsePersonAddress.Assign(local.ArCsePersonAddress);
    useImport.ApCsePersonAddress.Assign(local.ApCsePersonAddress);
    useImport.ArCsePersonsWorkSet.Assign(local.ArCsePersonsWorkSet);
    useImport.ApCsePersonsWorkSet.Assign(local.ApCsePersonsWorkSet);
    useImport.EabFileHandling.Action = local.EabFileHandling.Action;
    useExport.EabFileHandling.Status = local.EabFileHandling.Status;

    Call(EabWriteMedicalSupportChange.Execute, useImport, useExport);

    local.EabFileHandling.Status = useExport.EabFileHandling.Status;
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", import.Pers.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson2()
  {
    return ReadEach("ReadCsePerson2",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", import.Pers.Identifier);
      },
      (db, reader) =>
      {
        if (local.ChildrenInformation.IsFull)
        {
          return false;
        }

        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonAddress()
  {
    entities.CsePersonAddress.Populated = false;

    return Read("ReadCsePersonAddress",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePersonAddress.Identifier = db.GetDateTime(reader, 0);
        entities.CsePersonAddress.CspNumber = db.GetString(reader, 1);
        entities.CsePersonAddress.SendDate = db.GetNullableDate(reader, 2);
        entities.CsePersonAddress.Source = db.GetNullableString(reader, 3);
        entities.CsePersonAddress.Street1 = db.GetNullableString(reader, 4);
        entities.CsePersonAddress.Street2 = db.GetNullableString(reader, 5);
        entities.CsePersonAddress.City = db.GetNullableString(reader, 6);
        entities.CsePersonAddress.Type1 = db.GetNullableString(reader, 7);
        entities.CsePersonAddress.VerifiedDate = db.GetNullableDate(reader, 8);
        entities.CsePersonAddress.EndDate = db.GetNullableDate(reader, 9);
        entities.CsePersonAddress.EndCode = db.GetNullableString(reader, 10);
        entities.CsePersonAddress.State = db.GetNullableString(reader, 11);
        entities.CsePersonAddress.ZipCode = db.GetNullableString(reader, 12);
        entities.CsePersonAddress.Zip4 = db.GetNullableString(reader, 13);
        entities.CsePersonAddress.Zip3 = db.GetNullableString(reader, 14);
        entities.CsePersonAddress.Street3 = db.GetNullableString(reader, 15);
        entities.CsePersonAddress.Street4 = db.GetNullableString(reader, 16);
        entities.CsePersonAddress.Province = db.GetNullableString(reader, 17);
        entities.CsePersonAddress.PostalCode = db.GetNullableString(reader, 18);
        entities.CsePersonAddress.Country = db.GetNullableString(reader, 19);
        entities.CsePersonAddress.LocationType = db.GetString(reader, 20);
        entities.CsePersonAddress.County = db.GetNullableString(reader, 21);
        entities.CsePersonAddress.Populated = true;
      });
  }

  private bool ReadCsePersonCase()
  {
    entities.Case1.Populated = false;
    entities.CsePerson.Populated = false;

    return Read("ReadCsePersonCase",
      (db, command) =>
      {
        db.SetInt32(command, "lgaId", import.Pers.Identifier);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Case1.Number = db.GetString(reader, 1);
        entities.Case1.Populated = true;
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadEmployerAddress()
  {
    entities.EmployerAddress.Populated = false;

    return Read("ReadEmployerAddress",
      (db, command) =>
      {
        db.SetInt32(command, "empId", entities.Employer.Identifier);
      },
      (db, reader) =>
      {
        entities.EmployerAddress.LocationType = db.GetString(reader, 0);
        entities.EmployerAddress.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 1);
        entities.EmployerAddress.LastUpdatedBy =
          db.GetNullableString(reader, 2);
        entities.EmployerAddress.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.EmployerAddress.CreatedBy = db.GetString(reader, 4);
        entities.EmployerAddress.Street1 = db.GetNullableString(reader, 5);
        entities.EmployerAddress.Street2 = db.GetNullableString(reader, 6);
        entities.EmployerAddress.City = db.GetNullableString(reader, 7);
        entities.EmployerAddress.Identifier = db.GetDateTime(reader, 8);
        entities.EmployerAddress.Street3 = db.GetNullableString(reader, 9);
        entities.EmployerAddress.Street4 = db.GetNullableString(reader, 10);
        entities.EmployerAddress.Province = db.GetNullableString(reader, 11);
        entities.EmployerAddress.Country = db.GetNullableString(reader, 12);
        entities.EmployerAddress.PostalCode = db.GetNullableString(reader, 13);
        entities.EmployerAddress.State = db.GetNullableString(reader, 14);
        entities.EmployerAddress.ZipCode = db.GetNullableString(reader, 15);
        entities.EmployerAddress.Zip4 = db.GetNullableString(reader, 16);
        entities.EmployerAddress.Zip3 = db.GetNullableString(reader, 17);
        entities.EmployerAddress.EmpId = db.GetInt32(reader, 18);
        entities.EmployerAddress.County = db.GetNullableString(reader, 19);
        entities.EmployerAddress.Populated = true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCompanyHealthInsuranceCoverage1()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCoverage1",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", local.ApCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        if (local.ApInsurancePolicy.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 3);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 14);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCoverage.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCompanyHealthInsuranceCoverage2()
  {
    return ReadEach("ReadHealthInsuranceCompanyHealthInsuranceCoverage2",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", local.ArCsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        if (local.ArInsurancePolicy.IsFull)
        {
          return false;
        }

        entities.HealthInsuranceCompany.Identifier = db.GetInt32(reader, 0);
        entities.HealthInsuranceCoverage.HicIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.HealthInsuranceCompany.CarrierCode =
          db.GetNullableString(reader, 1);
        entities.HealthInsuranceCompany.InsurancePolicyCarrier =
          db.GetNullableString(reader, 2);
        entities.HealthInsuranceCoverage.Identifier = db.GetInt64(reader, 3);
        entities.HealthInsuranceCoverage.InsuranceGroupNumber =
          db.GetNullableString(reader, 4);
        entities.HealthInsuranceCoverage.InsurancePolicyNumber =
          db.GetNullableString(reader, 5);
        entities.HealthInsuranceCoverage.PolicyExpirationDate =
          db.GetNullableDate(reader, 6);
        entities.HealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 7);
        entities.HealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 8);
        entities.HealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 9);
        entities.HealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 10);
        entities.HealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 11);
        entities.HealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 12);
        entities.HealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 13);
        entities.HealthInsuranceCoverage.PolicyEffectiveDate =
          db.GetNullableDate(reader, 14);
        entities.HealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 15);
        entities.HealthInsuranceCoverage.Populated = true;
        entities.HealthInsuranceCompany.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIncomeSourceEmployer1()
  {
    return ReadEach("ReadIncomeSourceEmployer1",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (local.ApEmployerInfo.IsFull)
        {
          return false;
        }

        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 2);
        entities.Employer.Identifier = db.GetInt32(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.IncomeSource.Populated = true;
        entities.Employer.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadIncomeSourceEmployer2()
  {
    return ReadEach("ReadIncomeSourceEmployer2",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        if (local.ArEmployerInfo.IsFull)
        {
          return false;
        }

        entities.IncomeSource.Identifier = db.GetDateTime(reader, 0);
        entities.IncomeSource.CspINumber = db.GetString(reader, 1);
        entities.IncomeSource.EmpId = db.GetNullableInt32(reader, 2);
        entities.Employer.Identifier = db.GetInt32(reader, 2);
        entities.Employer.Name = db.GetNullableString(reader, 3);
        entities.IncomeSource.Populated = true;
        entities.Employer.Populated = true;

        return true;
      });
  }

  private bool ReadOldNewXref()
  {
    entities.OldNewXref.Populated = false;

    return Read("ReadOldNewXref",
      (db, command) =>
      {
        db.SetString(command, "kessepCaseNbr", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.OldNewXref.KessepCaseNbr = db.GetString(reader, 0);
        entities.OldNewXref.CaecsesCaseNbr = db.GetString(reader, 1);
        entities.OldNewXref.ClientType = db.GetString(reader, 2);
        entities.OldNewXref.ClientNbr = db.GetInt64(reader, 3);
        entities.OldNewXref.Populated = true;
      });
  }

  private bool ReadPersonProgram()
  {
    entities.PersonProgram.Populated = false;

    return Read("ReadPersonProgram",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
        db.SetDate(
          command, "effectiveDate", import.Process.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.PersonProgram.CspNumber = db.GetString(reader, 0);
        entities.PersonProgram.EffectiveDate = db.GetDate(reader, 1);
        entities.PersonProgram.Status = db.GetNullableString(reader, 2);
        entities.PersonProgram.DiscontinueDate = db.GetNullableDate(reader, 3);
        entities.PersonProgram.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PersonProgram.ChangedInd = db.GetNullableString(reader, 5);
        entities.PersonProgram.ChangeDate = db.GetNullableDate(reader, 6);
        entities.PersonProgram.PrgGeneratedId = db.GetInt32(reader, 7);
        entities.PersonProgram.MedTypeDiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.PersonProgram.MedType = db.GetNullableString(reader, 9);
        entities.PersonProgram.Populated = true;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
    }

    /// <summary>
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    public Common RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
    }

    /// <summary>
    /// A value of Process.
    /// </summary>
    [JsonPropertyName("process")]
    public DateWorkArea Process
    {
      get => process ??= new();
      set => process = value;
    }

    /// <summary>
    /// A value of Pers.
    /// </summary>
    [JsonPropertyName("pers")]
    public LegalAction Pers
    {
      get => pers ??= new();
      set => pers = value;
    }

    private DateWorkArea max;
    private Common recordCount;
    private DateWorkArea process;
    private LegalAction pers;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of RecordCount.
    /// </summary>
    [JsonPropertyName("recordCount")]
    public Common RecordCount
    {
      get => recordCount ??= new();
      set => recordCount = value;
    }

    private Common recordCount;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ApInsurancePolicyGroup group.</summary>
    [Serializable]
    public class ApInsurancePolicyGroup
    {
      /// <summary>
      /// A value of ApInsGrp1.
      /// </summary>
      [JsonPropertyName("apInsGrp1")]
      public HealthInsuranceCompany ApInsGrp1
      {
        get => apInsGrp1 ??= new();
        set => apInsGrp1 = value;
      }

      /// <summary>
      /// A value of ApInsGrp2.
      /// </summary>
      [JsonPropertyName("apInsGrp2")]
      public HealthInsuranceCoverage ApInsGrp2
      {
        get => apInsGrp2 ??= new();
        set => apInsGrp2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private HealthInsuranceCompany apInsGrp1;
      private HealthInsuranceCoverage apInsGrp2;
    }

    /// <summary>A ArInsurancePolicyGroup group.</summary>
    [Serializable]
    public class ArInsurancePolicyGroup
    {
      /// <summary>
      /// A value of ArInsGrp1.
      /// </summary>
      [JsonPropertyName("arInsGrp1")]
      public HealthInsuranceCompany ArInsGrp1
      {
        get => arInsGrp1 ??= new();
        set => arInsGrp1 = value;
      }

      /// <summary>
      /// A value of ArInsGrp2.
      /// </summary>
      [JsonPropertyName("arInsGrp2")]
      public HealthInsuranceCoverage ArInsGrp2
      {
        get => arInsGrp2 ??= new();
        set => arInsGrp2 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private HealthInsuranceCompany arInsGrp1;
      private HealthInsuranceCoverage arInsGrp2;
    }

    /// <summary>A ChildrenInformationGroup group.</summary>
    [Serializable]
    public class ChildrenInformationGroup
    {
      /// <summary>
      /// A value of Child.
      /// </summary>
      [JsonPropertyName("child")]
      public CsePersonsWorkSet Child
      {
        get => child ??= new();
        set => child = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private CsePersonsWorkSet child;
    }

    /// <summary>A ApEmployerInfoGroup group.</summary>
    [Serializable]
    public class ApEmployerInfoGroup
    {
      /// <summary>
      /// A value of ApEmployerEmployer.
      /// </summary>
      [JsonPropertyName("apEmployerEmployer")]
      public Employer ApEmployerEmployer
      {
        get => apEmployerEmployer ??= new();
        set => apEmployerEmployer = value;
      }

      /// <summary>
      /// A value of ApEmployerEmployerAddress.
      /// </summary>
      [JsonPropertyName("apEmployerEmployerAddress")]
      public EmployerAddress ApEmployerEmployerAddress
      {
        get => apEmployerEmployerAddress ??= new();
        set => apEmployerEmployerAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Employer apEmployerEmployer;
      private EmployerAddress apEmployerEmployerAddress;
    }

    /// <summary>A ArEmployerInfoGroup group.</summary>
    [Serializable]
    public class ArEmployerInfoGroup
    {
      /// <summary>
      /// A value of ArEmployerEmployer.
      /// </summary>
      [JsonPropertyName("arEmployerEmployer")]
      public Employer ArEmployerEmployer
      {
        get => arEmployerEmployer ??= new();
        set => arEmployerEmployer = value;
      }

      /// <summary>
      /// A value of ArEmployerEmployerAddress.
      /// </summary>
      [JsonPropertyName("arEmployerEmployerAddress")]
      public EmployerAddress ArEmployerEmployerAddress
      {
        get => arEmployerEmployerAddress ??= new();
        set => arEmployerEmployerAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private Employer arEmployerEmployer;
      private EmployerAddress arEmployerEmployerAddress;
    }

    /// <summary>
    /// A value of AtLeastOneMedicaidPgm.
    /// </summary>
    [JsonPropertyName("atLeastOneMedicaidPgm")]
    public Common AtLeastOneMedicaidPgm
    {
      get => atLeastOneMedicaidPgm ??= new();
      set => atLeastOneMedicaidPgm = value;
    }

    /// <summary>
    /// A value of ProgramFound.
    /// </summary>
    [JsonPropertyName("programFound")]
    public Common ProgramFound
    {
      get => programFound ??= new();
      set => programFound = value;
    }

    /// <summary>
    /// Gets a value of ApInsurancePolicy.
    /// </summary>
    [JsonIgnore]
    public Array<ApInsurancePolicyGroup> ApInsurancePolicy =>
      apInsurancePolicy ??= new(ApInsurancePolicyGroup.Capacity);

    /// <summary>
    /// Gets a value of ApInsurancePolicy for json serialization.
    /// </summary>
    [JsonPropertyName("apInsurancePolicy")]
    [Computed]
    public IList<ApInsurancePolicyGroup> ApInsurancePolicy_Json
    {
      get => apInsurancePolicy;
      set => ApInsurancePolicy.Assign(value);
    }

    /// <summary>
    /// Gets a value of ArInsurancePolicy.
    /// </summary>
    [JsonIgnore]
    public Array<ArInsurancePolicyGroup> ArInsurancePolicy =>
      arInsurancePolicy ??= new(ArInsurancePolicyGroup.Capacity);

    /// <summary>
    /// Gets a value of ArInsurancePolicy for json serialization.
    /// </summary>
    [JsonPropertyName("arInsurancePolicy")]
    [Computed]
    public IList<ArInsurancePolicyGroup> ArInsurancePolicy_Json
    {
      get => arInsurancePolicy;
      set => ArInsurancePolicy.Assign(value);
    }

    /// <summary>
    /// A value of ChildClear.
    /// </summary>
    [JsonPropertyName("childClear")]
    public CsePersonsWorkSet ChildClear
    {
      get => childClear ??= new();
      set => childClear = value;
    }

    /// <summary>
    /// A value of ArFormattedCityState.
    /// </summary>
    [JsonPropertyName("arFormattedCityState")]
    public TextWorkArea ArFormattedCityState
    {
      get => arFormattedCityState ??= new();
      set => arFormattedCityState = value;
    }

    /// <summary>
    /// A value of ApFormattedCityState.
    /// </summary>
    [JsonPropertyName("apFormattedCityState")]
    public TextWorkArea ApFormattedCityState
    {
      get => apFormattedCityState ??= new();
      set => apFormattedCityState = value;
    }

    /// <summary>
    /// Gets a value of ChildrenInformation.
    /// </summary>
    [JsonIgnore]
    public Array<ChildrenInformationGroup> ChildrenInformation =>
      childrenInformation ??= new(ChildrenInformationGroup.Capacity);

    /// <summary>
    /// Gets a value of ChildrenInformation for json serialization.
    /// </summary>
    [JsonPropertyName("childrenInformation")]
    [Computed]
    public IList<ChildrenInformationGroup> ChildrenInformation_Json
    {
      get => childrenInformation;
      set => ChildrenInformation.Assign(value);
    }

    /// <summary>
    /// Gets a value of ApEmployerInfo.
    /// </summary>
    [JsonIgnore]
    public Array<ApEmployerInfoGroup> ApEmployerInfo => apEmployerInfo ??= new(
      ApEmployerInfoGroup.Capacity);

    /// <summary>
    /// Gets a value of ApEmployerInfo for json serialization.
    /// </summary>
    [JsonPropertyName("apEmployerInfo")]
    [Computed]
    public IList<ApEmployerInfoGroup> ApEmployerInfo_Json
    {
      get => apEmployerInfo;
      set => ApEmployerInfo.Assign(value);
    }

    /// <summary>
    /// Gets a value of ArEmployerInfo.
    /// </summary>
    [JsonIgnore]
    public Array<ArEmployerInfoGroup> ArEmployerInfo => arEmployerInfo ??= new(
      ArEmployerInfoGroup.Capacity);

    /// <summary>
    /// Gets a value of ArEmployerInfo for json serialization.
    /// </summary>
    [JsonPropertyName("arEmployerInfo")]
    [Computed]
    public IList<ArEmployerInfoGroup> ArEmployerInfo_Json
    {
      get => arEmployerInfo;
      set => ArEmployerInfo.Assign(value);
    }

    /// <summary>
    /// A value of ArCsePersonAddress.
    /// </summary>
    [JsonPropertyName("arCsePersonAddress")]
    public CsePersonAddress ArCsePersonAddress
    {
      get => arCsePersonAddress ??= new();
      set => arCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ApCsePersonAddress.
    /// </summary>
    [JsonPropertyName("apCsePersonAddress")]
    public CsePersonAddress ApCsePersonAddress
    {
      get => apCsePersonAddress ??= new();
      set => apCsePersonAddress = value;
    }

    /// <summary>
    /// A value of ArCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("arCsePersonsWorkSet")]
    public CsePersonsWorkSet ArCsePersonsWorkSet
    {
      get => arCsePersonsWorkSet ??= new();
      set => arCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ApCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("apCsePersonsWorkSet")]
    public CsePersonsWorkSet ApCsePersonsWorkSet
    {
      get => apCsePersonsWorkSet ??= new();
      set => apCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
    }

    /// <summary>
    /// A value of NeededToWrite.
    /// </summary>
    [JsonPropertyName("neededToWrite")]
    public EabReportSend NeededToWrite
    {
      get => neededToWrite ??= new();
      set => neededToWrite = value;
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
    /// A value of CoverageStart.
    /// </summary>
    [JsonPropertyName("coverageStart")]
    public TextWorkArea CoverageStart
    {
      get => coverageStart ??= new();
      set => coverageStart = value;
    }

    /// <summary>
    /// A value of CoverageEnd.
    /// </summary>
    [JsonPropertyName("coverageEnd")]
    public TextWorkArea CoverageEnd
    {
      get => coverageEnd ??= new();
      set => coverageEnd = value;
    }

    /// <summary>
    /// A value of InsGrp1Clear.
    /// </summary>
    [JsonPropertyName("insGrp1Clear")]
    public HealthInsuranceCompany InsGrp1Clear
    {
      get => insGrp1Clear ??= new();
      set => insGrp1Clear = value;
    }

    /// <summary>
    /// A value of InsGrp2Clear.
    /// </summary>
    [JsonPropertyName("insGrp2Clear")]
    public HealthInsuranceCoverage InsGrp2Clear
    {
      get => insGrp2Clear ??= new();
      set => insGrp2Clear = value;
    }

    /// <summary>
    /// A value of ReportNeeded.
    /// </summary>
    [JsonPropertyName("reportNeeded")]
    public Common ReportNeeded
    {
      get => reportNeeded ??= new();
      set => reportNeeded = value;
    }

    /// <summary>
    /// A value of EmployerGrpClearEmployer.
    /// </summary>
    [JsonPropertyName("employerGrpClearEmployer")]
    public Employer EmployerGrpClearEmployer
    {
      get => employerGrpClearEmployer ??= new();
      set => employerGrpClearEmployer = value;
    }

    /// <summary>
    /// A value of EmployerGrpClearEmployerAddress.
    /// </summary>
    [JsonPropertyName("employerGrpClearEmployerAddress")]
    public EmployerAddress EmployerGrpClearEmployerAddress
    {
      get => employerGrpClearEmployerAddress ??= new();
      set => employerGrpClearEmployerAddress = value;
    }

    private Common atLeastOneMedicaidPgm;
    private Common programFound;
    private Array<ApInsurancePolicyGroup> apInsurancePolicy;
    private Array<ArInsurancePolicyGroup> arInsurancePolicy;
    private CsePersonsWorkSet childClear;
    private TextWorkArea arFormattedCityState;
    private TextWorkArea apFormattedCityState;
    private Array<ChildrenInformationGroup> childrenInformation;
    private Array<ApEmployerInfoGroup> apEmployerInfo;
    private Array<ArEmployerInfoGroup> arEmployerInfo;
    private CsePersonAddress arCsePersonAddress;
    private CsePersonAddress apCsePersonAddress;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private AbendData abendData;
    private EabReportSend neededToWrite;
    private EabFileHandling eabFileHandling;
    private TextWorkArea coverageStart;
    private TextWorkArea coverageEnd;
    private HealthInsuranceCompany insGrp1Clear;
    private HealthInsuranceCoverage insGrp2Clear;
    private Common reportNeeded;
    private Employer employerGrpClearEmployer;
    private EmployerAddress employerGrpClearEmployerAddress;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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

    /// <summary>
    /// A value of OldNewXref.
    /// </summary>
    [JsonPropertyName("oldNewXref")]
    public OldNewXref OldNewXref
    {
      get => oldNewXref ??= new();
      set => oldNewXref = value;
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
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
    }

    /// <summary>
    /// A value of LegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("legalActionCaseRole")]
    public LegalActionCaseRole LegalActionCaseRole
    {
      get => legalActionCaseRole ??= new();
      set => legalActionCaseRole = value;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("healthInsuranceCoverage")]
    public HealthInsuranceCoverage HealthInsuranceCoverage
    {
      get => healthInsuranceCoverage ??= new();
      set => healthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of HealthInsuranceCompany.
    /// </summary>
    [JsonPropertyName("healthInsuranceCompany")]
    public HealthInsuranceCompany HealthInsuranceCompany
    {
      get => healthInsuranceCompany ??= new();
      set => healthInsuranceCompany = value;
    }

    /// <summary>
    /// A value of PersonProgram.
    /// </summary>
    [JsonPropertyName("personProgram")]
    public PersonProgram PersonProgram
    {
      get => personProgram ??= new();
      set => personProgram = value;
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

    private IncomeSource incomeSource;
    private EmployerAddress employerAddress;
    private Employer employer;
    private OldNewXref oldNewXref;
    private Case1 case1;
    private CsePersonAddress csePersonAddress;
    private LegalActionCaseRole legalActionCaseRole;
    private CaseRole caseRole;
    private CsePerson csePerson;
    private HealthInsuranceCoverage healthInsuranceCoverage;
    private HealthInsuranceCompany healthInsuranceCompany;
    private PersonProgram personProgram;
    private Program program;
  }
#endregion
}

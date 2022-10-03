// Program: OE_GTSC_RAISE_EVENTS, ID: 371797164, model: 746.
// Short name: SWE01794
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
/// A program: OE_GTSC_RAISE_EVENTS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// </para>
/// </summary>
[Serializable]
public partial class OeGtscRaiseEvents: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSC_RAISE_EVENTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtscRaiseEvents(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtscRaiseEvents.
  /// </summary>
  public OeGtscRaiseEvents(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------
    // Change log
    // date		author	remarks
    // 12/16/96	Sid 	initial creation
    // 01/02/1997	Raju	active case unit
    // 			check removed when
    // 			reading case units
    // 01/08/97	Raju	initiating state code
    // 			made 2 chars OS/KS
    // 01/09/97	Raju	major change
    //                         case : father = AP, mother = AR
    //                           rec not found in case unit
    // 05/9/00		PMcElderry	PR # 93888
    // Code changes resulting from the 1 - M (not 1 - 1) relationship
    // b/t CASE and INTERSTATE REQUEST
    // 07/13/2001             Vithal Madhira          PR# 120975
    // Qualify all the 'case_unit' READs to read only active case_unit (ie. 
    // Closure_date='2099-12-31').
    // ----------------------------------------------------------------
    // 10/18/00  I00105387  PPhinney  MONA's going to WRONG Service Provider
    // Make READ QUALIFIED on Correct CASE
    // Change READ to use VIEWS - NOT Literals
    // ******* END NON-SPECIAL 
    // MAINTENANCE LOG
    // ******************************
    // -----------------------------------------------------------------------
    // ********** SPECIAL MAINTENANCE ***************************
    // AUTHOR  DATE  		DESCRIPTION
    // R. Jean	07/09/99	Singleton reads changed to select only
    // ******* END MAINTENANCE LOG 
    // ******************************
    // 10/18/00  I00105387  PPhinney  MONA's going to WRONG Service Provider
    // mlb - PR00254384 - 09/08/2005 - -811 abend in DB2 - Multiple rows 
    // returned with read of
    // Interstate_Request, when one row expected. Am restricting read to open 
    // active true
    // interstate case requests but further qualifing read to look at 'O' and '
    // N' indicators.
    local.SaveDate.DiscontinueDate = Now().Date.AddDays(-1);
    local.SaveDate.EffectiveDate = Now().Date.AddDays(1);
    local.MaxDate.Date = new DateTime(2099, 12, 31);
    local.Infrastructure.Assign(import.Infrastructure);
    local.ReadEachStatusFlag.Text1 = "";

    // ------------------------------------------------
    // Assigning global infrastructure attribute values
    // ------------------------------------------------
    local.Infrastructure.EventId = 26;
    local.Infrastructure.CreatedBy = global.UserId;
    local.Infrastructure.LastUpdatedBy = "";
    local.Infrastructure.ProcessStatus = "Q";
    local.Infrastructure.BusinessObjectCd = "GNT";
    local.Infrastructure.UserId = "GTSC";
    local.Infrastructure.DenormText12 = "";

    if (!IsEmpty(import.GeneticTestInformation.CourtOrderNo))
    {
      // 10/18/00  I00105387  PPhinney  MONA's going to WRONG Service Provider
      // Make READ QUALIFIED on Correct CASE
      // Change READ to use VIEWS - NOT Literals
      local.Genetico.ActionTaken = "GENETICO";
      local.Gentest.ActionTaken = "GENTEST";
      local.At.RoleCode = "AT";
      local.Sa.RoleCode = "SA";

      foreach(var item in ReadLegalActionOfficeServiceProvider())
      {
        if (Equal(entities.ExistingLegalAction.FiledDate, local.Init1.Date))
        {
          continue;
        }

        if (Lt(entities.ExistingOfficeServiceProvider.DiscontinueDate,
          Now().Date) && Lt
          (local.Init1.Date,
          entities.ExistingOfficeServiceProvider.DiscontinueDate))
        {
          continue;
        }

        local.LegalActionFound.Flag = "Y";
        local.LegalAction.Assign(entities.ExistingLegalAction);

        goto Test;
      }
    }

Test:

    if (ReadCase1())
    {
      local.Infrastructure.CaseNumber = entities.ExistingCase.Number;

      // mlb - 254384 - 09/09/2005 - Further qualify read to find true 
      // interstate cases that are open.
      if (ReadInterstateRequest1())
      {
        if (AsChar(entities.ExistingInterstateRequest.KsCaseInd) == 'Y')
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
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (!IsEmpty(import.GeneticTestInformation.MotherPersonNo))
    {
      if (ReadCsePersonCsePersonCsePerson())
      {
        switch(TrimEnd(local.Infrastructure.ReasonCode))
        {
          case "RESULTNEG":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.FatherPersonNo;

            break;
          case "RESULTPOS":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.FatherPersonNo;

            break;
          case "FASCHED":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.FatherPersonNo;

            break;
          case "MOSCHED":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.MotherPersonNo;

            break;
          case "CHSCHED":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.ChildPersonNo;

            break;
          case "GTNOSHOFA":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.FatherPersonNo;

            break;
          case "GTNOSHOMO":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.MotherPersonNo;

            break;
          case "GTNOSHOCH":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.ChildPersonNo;

            break;
          case "GENTESCONSTART":
            local.Infrastructure.CsePersonNumber = "";

            break;
          case "GENTESCONEND":
            local.Infrastructure.CsePersonNumber = "";

            break;
          default:
            break;
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }
    else
    {
      // -------------------------------------------------------------------------------
      // This is the condtion for Motherless Comparisons. User may not select 
      // the MOTHER for genertic test.
      // -----------------------------------------------------------------------------------
      if (ReadCsePersonCsePerson())
      {
        switch(TrimEnd(local.Infrastructure.ReasonCode))
        {
          case "RESULTNEG":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.FatherPersonNo;

            break;
          case "RESULTPOS":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.FatherPersonNo;

            break;
          case "FASCHED":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.FatherPersonNo;

            break;
          case "MOSCHED":
            break;
          case "CHSCHED":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.ChildPersonNo;

            break;
          case "GTNOSHOFA":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.FatherPersonNo;

            break;
          case "GTNOSHOMO":
            break;
          case "GTNOSHOCH":
            local.Infrastructure.CsePersonNumber =
              import.GeneticTestInformation.ChildPersonNo;

            break;
          case "GENTESCONSTART":
            local.Infrastructure.CsePersonNumber = "";

            break;
          case "GENTESCONEND":
            local.Infrastructure.CsePersonNumber = "";

            break;
          default:
            break;
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }

    // ---------------------------------------------
    // Raju 01/02/1997 : 1110 hrs CST
    //   - Refer Jack's note dated 01/02/1997
    //     subject : Case Closure and Reopen,
    //               Case Unit Deactivation and
    //               Reactivation
    // Conclusion drawn from note :
    // The check for raising events only for active
    //   case units is to be removed from all
    //   raise event cabs.
    // The event processor will handle this.
    // ---------------------------------------------
    if (entities.ExistingMother.Populated)
    {
      if (ReadCaseUnit1())
      {
        local.Infrastructure.CaseUnitNumber =
          entities.ExistingCaseUnit.CuNumber;
        UseSpCabCreateInfrastructure1();

        // ------------------------------------------
        // Repeat the infrastructure record for legal
        // ------------------------------------------
        if (AsChar(local.LegalActionFound.Flag) == 'Y')
        {
          local.Legal.Assign(export.Infrastructure);
          local.Legal.EventId = 99;
          local.Legal.CreatedBy = global.UserId;
          local.Legal.LastUpdatedBy = "";
          local.Legal.ProcessStatus = "Q";
          local.Legal.BusinessObjectCd = "LEA";
          local.Legal.UserId = "GTSC";
          local.Legal.DenormNumeric12 = local.LegalAction.Identifier;
          local.Legal.DenormText12 = local.LegalAction.CourtCaseNumber ?? "";
          local.Legal.ReferenceDate = local.LegalAction.FiledDate;

          switch(TrimEnd(export.Infrastructure.ReasonCode))
          {
            case "RESULTNEG":
              local.Legal.ReasonCode = "RESULTNEGLEGAL";

              break;
            case "RESULTPOS":
              local.Legal.ReasonCode = "RESULTPOSLEGAL";

              break;
            case "FASCHED":
              local.Legal.ReasonCode = "SCHGENTESTFA";

              break;
            case "MOSCHED":
              local.Legal.ReasonCode = "SCHGENTESTMOM";

              break;
            case "CHSCHED":
              local.Legal.ReasonCode = "SCHGENTESTCH";

              break;
            case "GTNOSHOFA":
              local.Legal.ReasonCode = "GTNOSHOWFALEGAL";

              break;
            case "GTNOSHOMO":
              local.Legal.ReasonCode = "GTNOSHOWMOLEGAL";

              break;
            case "GTNOSHOCH":
              local.Legal.ReasonCode = "GTNOSHOWCHLEGAL";

              break;
            case "GENTESCONSTART":
              local.Legal.ReasonCode = "GTCONTSTLEGAL";

              break;
            case "GENTESCONEND":
              local.Legal.ReasonCode = "GTCONTENLEGAL";

              break;
            default:
              break;
          }

          UseSpCabCreateInfrastructure2();
        }
      }
      else
      {
        // ---------------------------------------------
        // Raju : 01/09/97:1300hrs CST
        // 	- refer SID for detailed explanation
        // 	- cases where father and mother
        // 		are APs will need this code
        // ---------------------------------------------
        // ---------------------------------------------
        // Raju : 01/09/97:1300hrs CST
        // Start of Code
        // ---------------------------------------------
        foreach(var item in ReadCaseUnit3())
        {
          local.ReadEachStatusFlag.Text1 = "1";

          if (ReadCase2())
          {
            local.Infrastructure.CaseNumber = entities.ExistingCase.Number;

            // mlb - 254384 - 09/09/2005 - Further qualify read to find true 
            // interstate cases that are open.
            if (ReadInterstateRequest1())
            {
              if (AsChar(entities.ExistingInterstateRequest.KsCaseInd) == 'Y')
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
          }
          else
          {
            ExitState = "CASE_NF";

            return;
          }

          local.Infrastructure.CsePersonNumber = entities.ExistingFather.Number;
          local.Infrastructure.CaseUnitNumber =
            entities.ExistingCaseUnit.CuNumber;
          UseSpCabCreateInfrastructure1();

          // ****************************************************************
          // * Repeat the infrastructure record for legal
          // ****************************************************************
          if (AsChar(local.LegalActionFound.Flag) == 'Y')
          {
            local.Legal.Assign(export.Infrastructure);
            local.Legal.EventId = 99;
            local.Legal.CreatedBy = global.UserId;
            local.Legal.LastUpdatedBy = "";
            local.Legal.ProcessStatus = "Q";
            local.Legal.BusinessObjectCd = "LEA";
            local.Legal.UserId = "GTSC";
            local.Legal.DenormNumeric12 = local.LegalAction.Identifier;
            local.Legal.DenormText12 = local.LegalAction.CourtCaseNumber ?? "";
            local.Legal.ReferenceDate = local.LegalAction.FiledDate;

            switch(TrimEnd(export.Infrastructure.ReasonCode))
            {
              case "RESULTNEG":
                local.Legal.ReasonCode = "RESULTNEGLEGAL";

                break;
              case "RESULTPOS":
                local.Legal.ReasonCode = "RESULTPOSLEGAL";

                break;
              case "FASCHED":
                local.Legal.ReasonCode = "SCHGENTESTFA";

                break;
              case "MOSCHED":
                local.Legal.ReasonCode = "SCHGENTESTMOM";

                break;
              case "CHSCHED":
                local.Legal.ReasonCode = "SCHGENTESTCH";

                break;
              case "GTNOSHOFA":
                local.Legal.ReasonCode = "GTNOSHOWFALEGAL";

                break;
              case "GTNOSHOMO":
                local.Legal.ReasonCode = "GTNOSHOWMOLEGAL";

                break;
              case "GTNOSHOCH":
                local.Legal.ReasonCode = "GTNOSHOWCHLEGAL";

                break;
              case "GENTESCONSTART":
                local.Legal.ReasonCode = "GTCONTSTLEGAL";

                break;
              case "GENTESCONEND":
                local.Legal.ReasonCode = "GTCONTENLEGAL";

                break;
              default:
                break;
            }

            UseSpCabCreateInfrastructure2();
          }
        }

        foreach(var item in ReadCaseUnit4())
        {
          local.ReadEachStatusFlag.Text1 = "2";

          if (ReadCase2())
          {
            local.Infrastructure.CaseNumber = entities.ExistingCase.Number;

            // mlb - 254384 - 09/09/2005 - Further qualify read to find true 
            // interstate cases that are open.
            if (ReadInterstateRequest2())
            {
              if (AsChar(entities.ExistingInterstateRequest.KsCaseInd) == 'Y')
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
          }
          else
          {
            ExitState = "CASE_NF";

            return;
          }

          local.Infrastructure.CsePersonNumber = entities.ExistingMother.Number;
          local.Infrastructure.CaseUnitNumber =
            entities.ExistingCaseUnit.CuNumber;
          UseSpCabCreateInfrastructure1();

          // ****************************************************************
          // * Repeat the infrastructure record for legal
          // ****************************************************************
          if (AsChar(local.LegalActionFound.Flag) == 'Y')
          {
            local.Legal.Assign(export.Infrastructure);
            local.Legal.EventId = 99;
            local.Legal.CreatedBy = global.UserId;
            local.Legal.LastUpdatedBy = "";
            local.Legal.ProcessStatus = "Q";
            local.Legal.BusinessObjectCd = "LEA";
            local.Legal.UserId = "GTSC";
            local.Legal.DenormNumeric12 = local.LegalAction.Identifier;
            local.Legal.DenormText12 = local.LegalAction.CourtCaseNumber ?? "";
            local.Legal.ReferenceDate = local.LegalAction.FiledDate;

            switch(TrimEnd(export.Infrastructure.ReasonCode))
            {
              case "RESULTNEG":
                local.Legal.ReasonCode = "RESULTNEGLEGAL";

                break;
              case "RESULTPOS":
                local.Legal.ReasonCode = "RESULTPOSLEGAL";

                break;
              case "FASCHED":
                local.Legal.ReasonCode = "SCHGENTESTFA";

                break;
              case "MOSCHED":
                local.Legal.ReasonCode = "SCHGENTESTMOM";

                break;
              case "CHSCHED":
                local.Legal.ReasonCode = "SCHGENTESTCH";

                break;
              case "GTNOSHOFA":
                local.Legal.ReasonCode = "GTNOSHOWFALEGAL";

                break;
              case "GTNOSHOMO":
                local.Legal.ReasonCode = "GTNOSHOWMOLEGAL";

                break;
              case "GTNOSHOCH":
                local.Legal.ReasonCode = "GTNOSHOWCHLEGAL";

                break;
              case "GENTESCONSTART":
                local.Legal.ReasonCode = "GTCONTSTLEGAL";

                break;
              case "GENTESCONEND":
                local.Legal.ReasonCode = "GTCONTENLEGAL";

                break;
              default:
                break;
            }

            UseSpCabCreateInfrastructure2();
          }
        }

        // -----------
        // End of Code
        // -----------
        if (IsEmpty(local.ReadEachStatusFlag.Text1))
        {
          ExitState = "CASE_UNIT_NF";
        }
      }
    }
    else
    {
      // -------------------------------------------------------------------------------
      // This is the condtion for Motherless Comparisons. User may not select 
      // the MOTHER for genertic test.
      // -----------------------------------------------------------------------------------
      if (ReadCaseUnit2())
      {
        local.Infrastructure.CaseUnitNumber =
          entities.ExistingCaseUnit.CuNumber;
        UseSpCabCreateInfrastructure1();

        // ------------------------------------------
        // Repeat the infrastructure record for legal
        // ------------------------------------------
        if (AsChar(local.LegalActionFound.Flag) == 'Y')
        {
          local.Legal.Assign(export.Infrastructure);
          local.Legal.EventId = 99;
          local.Legal.CreatedBy = global.UserId;
          local.Legal.LastUpdatedBy = "";
          local.Legal.ProcessStatus = "Q";
          local.Legal.BusinessObjectCd = "LEA";
          local.Legal.UserId = "GTSC";
          local.Legal.DenormNumeric12 = local.LegalAction.Identifier;
          local.Legal.DenormText12 = local.LegalAction.CourtCaseNumber ?? "";
          local.Legal.ReferenceDate = local.LegalAction.FiledDate;

          switch(TrimEnd(export.Infrastructure.ReasonCode))
          {
            case "RESULTNEG":
              local.Legal.ReasonCode = "RESULTNEGLEGAL";

              break;
            case "RESULTPOS":
              local.Legal.ReasonCode = "RESULTPOSLEGAL";

              break;
            case "FASCHED":
              local.Legal.ReasonCode = "SCHGENTESTFA";

              break;
            case "MOSCHED":
              local.Legal.ReasonCode = "SCHGENTESTMOM";

              break;
            case "CHSCHED":
              local.Legal.ReasonCode = "SCHGENTESTCH";

              break;
            case "GTNOSHOFA":
              local.Legal.ReasonCode = "GTNOSHOWFALEGAL";

              break;
            case "GTNOSHOMO":
              local.Legal.ReasonCode = "GTNOSHOWMOLEGAL";

              break;
            case "GTNOSHOCH":
              local.Legal.ReasonCode = "GTNOSHOWCHLEGAL";

              break;
            case "GENTESCONSTART":
              local.Legal.ReasonCode = "GTCONTSTLEGAL";

              break;
            case "GENTESCONEND":
              local.Legal.ReasonCode = "GTCONTENLEGAL";

              break;
            default:
              break;
          }

          UseSpCabCreateInfrastructure2();
        }
      }
      else
      {
        ExitState = "CASE_UNIT_NF";
      }
    }
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
    target.DenormDate = source.DenormDate;
    target.DenormTimestamp = source.DenormTimestamp;
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

  private void UseSpCabCreateInfrastructure1()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Infrastructure);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, export.Infrastructure);
  }

  private void UseSpCabCreateInfrastructure2()
  {
    var useImport = new SpCabCreateInfrastructure.Import();
    var useExport = new SpCabCreateInfrastructure.Export();

    useImport.Infrastructure.Assign(local.Legal);

    Call(SpCabCreateInfrastructure.Execute, useImport, useExport);

    MoveInfrastructure(useExport.Infrastructure, export.Infrastructure);
  }

  private bool ReadCase1()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.GeneticTestInformation.CaseNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCase2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCaseUnit.Populated);
    entities.ExistingCase.Populated = false;

    return Read("ReadCase2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingCaseUnit.CasNo);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCaseUnit1()
  {
    entities.ExistingCaseUnit.Populated = false;

    return Read("ReadCaseUnit1",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.ExistingCase.Number);
        db.SetNullableString(
          command, "cspNoChild", entities.ExistingChild.Number);
        db.
          SetNullableString(command, "cspNoAp", entities.ExistingFather.Number);
          
        db.
          SetNullableString(command, "cspNoAr", entities.ExistingMother.Number);
          
        db.SetNullableDate(
          command, "closureDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.ExistingCaseUnit.StartDate = db.GetDate(reader, 1);
        entities.ExistingCaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 3);
        entities.ExistingCaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.ExistingCaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.ExistingCaseUnit.Populated = true;
      });
  }

  private bool ReadCaseUnit2()
  {
    entities.ExistingCaseUnit.Populated = false;

    return Read("ReadCaseUnit2",
      (db, command) =>
      {
        db.SetString(command, "casNo", entities.ExistingCase.Number);
        db.SetNullableString(
          command, "cspNoChild", entities.ExistingChild.Number);
        db.
          SetNullableString(command, "cspNoAp", entities.ExistingFather.Number);
          
        db.SetNullableDate(
          command, "closureDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.ExistingCaseUnit.StartDate = db.GetDate(reader, 1);
        entities.ExistingCaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 3);
        entities.ExistingCaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.ExistingCaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.ExistingCaseUnit.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit3()
  {
    entities.ExistingCaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit3",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoChild", entities.ExistingChild.Number);
        db.
          SetNullableString(command, "cspNoAp", entities.ExistingFather.Number);
          
        db.SetNullableDate(
          command, "closureDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.ExistingCaseUnit.StartDate = db.GetDate(reader, 1);
        entities.ExistingCaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 3);
        entities.ExistingCaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.ExistingCaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.ExistingCaseUnit.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseUnit4()
  {
    entities.ExistingCaseUnit.Populated = false;

    return ReadEach("ReadCaseUnit4",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNoChild", entities.ExistingChild.Number);
        db.
          SetNullableString(command, "cspNoAp", entities.ExistingMother.Number);
          
        db.SetNullableDate(
          command, "closureDate", local.MaxDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseUnit.CuNumber = db.GetInt32(reader, 0);
        entities.ExistingCaseUnit.StartDate = db.GetDate(reader, 1);
        entities.ExistingCaseUnit.ClosureDate = db.GetNullableDate(reader, 2);
        entities.ExistingCaseUnit.CasNo = db.GetString(reader, 3);
        entities.ExistingCaseUnit.CspNoAr = db.GetNullableString(reader, 4);
        entities.ExistingCaseUnit.CspNoAp = db.GetNullableString(reader, 5);
        entities.ExistingCaseUnit.CspNoChild = db.GetNullableString(reader, 6);
        entities.ExistingCaseUnit.Populated = true;

        return true;
      });
  }

  private bool ReadCsePersonCsePerson()
  {
    entities.ExistingChild.Populated = false;
    entities.ExistingFather.Populated = false;

    return Read("ReadCsePersonCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb1", import.GeneticTestInformation.ChildPersonNo);
        db.SetString(
          command, "numb2", import.GeneticTestInformation.FatherPersonNo);
      },
      (db, reader) =>
      {
        entities.ExistingFather.Number = db.GetString(reader, 0);
        entities.ExistingChild.Number = db.GetString(reader, 1);
        entities.ExistingChild.Populated = true;
        entities.ExistingFather.Populated = true;
      });
  }

  private bool ReadCsePersonCsePersonCsePerson()
  {
    entities.ExistingChild.Populated = false;
    entities.ExistingMother.Populated = false;
    entities.ExistingFather.Populated = false;

    return Read("ReadCsePersonCsePersonCsePerson",
      (db, command) =>
      {
        db.SetString(
          command, "numb1", import.GeneticTestInformation.ChildPersonNo);
        db.SetString(
          command, "numb2", import.GeneticTestInformation.FatherPersonNo);
        db.SetString(
          command, "numb3", import.GeneticTestInformation.MotherPersonNo);
      },
      (db, reader) =>
      {
        entities.ExistingFather.Number = db.GetString(reader, 0);
        entities.ExistingMother.Number = db.GetString(reader, 1);
        entities.ExistingChild.Number = db.GetString(reader, 2);
        entities.ExistingChild.Populated = true;
        entities.ExistingMother.Populated = true;
        entities.ExistingFather.Populated = true;
      });
  }

  private bool ReadInterstateRequest1()
  {
    entities.ExistingInterstateRequest.Populated = false;

    return Read("ReadInterstateRequest1",
      (db, command) =>
      {
        db.
          SetNullableString(command, "casINumber", entities.ExistingCase.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingInterstateRequest.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingInterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.ExistingInterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.ExistingInterstateRequest.CasINumber =
          db.GetNullableString(reader, 3);
        entities.ExistingInterstateRequest.Populated = true;
      });
  }

  private bool ReadInterstateRequest2()
  {
    entities.ExistingInterstateRequest.Populated = false;

    return Read("ReadInterstateRequest2",
      (db, command) =>
      {
        db.
          SetNullableString(command, "casINumber", entities.ExistingCase.Number);
          
      },
      (db, reader) =>
      {
        entities.ExistingInterstateRequest.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingInterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 1);
        entities.ExistingInterstateRequest.KsCaseInd = db.GetString(reader, 2);
        entities.ExistingInterstateRequest.CasINumber =
          db.GetNullableString(reader, 3);
        entities.ExistingInterstateRequest.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionOfficeServiceProvider()
  {
    entities.ExistingOfficeServiceProvider.Populated = false;
    entities.ExistingLegalAction.Populated = false;

    return ReadEach("ReadLegalActionOfficeServiceProvider",
      (db, command) =>
      {
        db.SetNullableString(
          command, "courtCaseNo", import.GeneticTestInformation.CourtOrderNo);
        db.SetString(
          command, "casNumber", import.GeneticTestInformation.CaseNumber);
        db.SetString(command, "actionTaken1", local.Gentest.ActionTaken);
        db.SetString(command, "actionTaken2", local.Genetico.ActionTaken);
        db.SetString(command, "roleCode1", local.At.RoleCode);
        db.SetString(command, "roleCode2", local.Sa.RoleCode);
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.ActionTaken = db.GetString(reader, 1);
        entities.ExistingLegalAction.FiledDate = db.GetNullableDate(reader, 2);
        entities.ExistingLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 3);
        entities.ExistingOfficeServiceProvider.SpdGeneratedId =
          db.GetInt32(reader, 4);
        entities.ExistingOfficeServiceProvider.OffGeneratedId =
          db.GetInt32(reader, 5);
        entities.ExistingOfficeServiceProvider.RoleCode =
          db.GetString(reader, 6);
        entities.ExistingOfficeServiceProvider.EffectiveDate =
          db.GetDate(reader, 7);
        entities.ExistingOfficeServiceProvider.DiscontinueDate =
          db.GetNullableDate(reader, 8);
        entities.ExistingOfficeServiceProvider.Populated = true;
        entities.ExistingLegalAction.Populated = true;

        return true;
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
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    /// <summary>
    /// A value of GeneticTestInformation.
    /// </summary>
    [JsonPropertyName("geneticTestInformation")]
    public GeneticTestInformation GeneticTestInformation
    {
      get => geneticTestInformation ??= new();
      set => geneticTestInformation = value;
    }

    private Infrastructure infrastructure;
    private GeneticTestInformation geneticTestInformation;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Infrastructure.
    /// </summary>
    [JsonPropertyName("infrastructure")]
    public Infrastructure Infrastructure
    {
      get => infrastructure ??= new();
      set => infrastructure = value;
    }

    private Infrastructure infrastructure;
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
      /// A value of Infrastructure.
      /// </summary>
      [JsonPropertyName("infrastructure")]
      public Infrastructure Infrastructure
      {
        get => infrastructure ??= new();
        set => infrastructure = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Infrastructure infrastructure;
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

    /// <summary>
    /// A value of Init1.
    /// </summary>
    [JsonPropertyName("init1")]
    public DateWorkArea Init1
    {
      get => init1 ??= new();
      set => init1 = value;
    }

    /// <summary>
    /// A value of LegalActionFound.
    /// </summary>
    [JsonPropertyName("legalActionFound")]
    public Common LegalActionFound
    {
      get => legalActionFound ??= new();
      set => legalActionFound = value;
    }

    /// <summary>
    /// A value of CodeValue.
    /// </summary>
    [JsonPropertyName("codeValue")]
    public CodeValue CodeValue
    {
      get => codeValue ??= new();
      set => codeValue = value;
    }

    /// <summary>
    /// A value of Code.
    /// </summary>
    [JsonPropertyName("code")]
    public Code Code
    {
      get => code ??= new();
      set => code = value;
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
    /// A value of Legal.
    /// </summary>
    [JsonPropertyName("legal")]
    public Infrastructure Legal
    {
      get => legal ??= new();
      set => legal = value;
    }

    /// <summary>
    /// A value of ReadEachStatusFlag.
    /// </summary>
    [JsonPropertyName("readEachStatusFlag")]
    public WorkArea ReadEachStatusFlag
    {
      get => readEachStatusFlag ??= new();
      set => readEachStatusFlag = value;
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
    /// A value of At.
    /// </summary>
    [JsonPropertyName("at")]
    public OfficeServiceProvider At
    {
      get => at ??= new();
      set => at = value;
    }

    /// <summary>
    /// A value of Sa.
    /// </summary>
    [JsonPropertyName("sa")]
    public OfficeServiceProvider Sa
    {
      get => sa ??= new();
      set => sa = value;
    }

    /// <summary>
    /// A value of Genetico.
    /// </summary>
    [JsonPropertyName("genetico")]
    public LegalAction Genetico
    {
      get => genetico ??= new();
      set => genetico = value;
    }

    /// <summary>
    /// A value of Gentest.
    /// </summary>
    [JsonPropertyName("gentest")]
    public LegalAction Gentest
    {
      get => gentest ??= new();
      set => gentest = value;
    }

    /// <summary>
    /// A value of SaveDate.
    /// </summary>
    [JsonPropertyName("saveDate")]
    public CaseAssignment SaveDate
    {
      get => saveDate ??= new();
      set => saveDate = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public DateWorkArea MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private Array<GroupGroup> group;
    private DateWorkArea init1;
    private Common legalActionFound;
    private CodeValue codeValue;
    private Code code;
    private LegalAction legalAction;
    private Infrastructure legal;
    private WorkArea readEachStatusFlag;
    private Infrastructure infrastructure;
    private OfficeServiceProvider at;
    private OfficeServiceProvider sa;
    private LegalAction genetico;
    private LegalAction gentest;
    private CaseAssignment saveDate;
    private DateWorkArea maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingOfficeServiceProvider.
    /// </summary>
    [JsonPropertyName("existingOfficeServiceProvider")]
    public OfficeServiceProvider ExistingOfficeServiceProvider
    {
      get => existingOfficeServiceProvider ??= new();
      set => existingOfficeServiceProvider = value;
    }

    /// <summary>
    /// A value of ExistingLegalActionAssigment.
    /// </summary>
    [JsonPropertyName("existingLegalActionAssigment")]
    public LegalActionAssigment ExistingLegalActionAssigment
    {
      get => existingLegalActionAssigment ??= new();
      set => existingLegalActionAssigment = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CsePerson ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
    }

    /// <summary>
    /// A value of ExistingMother.
    /// </summary>
    [JsonPropertyName("existingMother")]
    public CsePerson ExistingMother
    {
      get => existingMother ??= new();
      set => existingMother = value;
    }

    /// <summary>
    /// A value of ExistingFather.
    /// </summary>
    [JsonPropertyName("existingFather")]
    public CsePerson ExistingFather
    {
      get => existingFather ??= new();
      set => existingFather = value;
    }

    /// <summary>
    /// A value of ExistingCaseUnit.
    /// </summary>
    [JsonPropertyName("existingCaseUnit")]
    public CaseUnit ExistingCaseUnit
    {
      get => existingCaseUnit ??= new();
      set => existingCaseUnit = value;
    }

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    /// <summary>
    /// A value of ExistingInterstateRequest.
    /// </summary>
    [JsonPropertyName("existingInterstateRequest")]
    public InterstateRequest ExistingInterstateRequest
    {
      get => existingInterstateRequest ??= new();
      set => existingInterstateRequest = value;
    }

    /// <summary>
    /// A value of LinkingLegalActionCaseRole.
    /// </summary>
    [JsonPropertyName("linkingLegalActionCaseRole")]
    public LegalActionCaseRole LinkingLegalActionCaseRole
    {
      get => linkingLegalActionCaseRole ??= new();
      set => linkingLegalActionCaseRole = value;
    }

    /// <summary>
    /// A value of LinkingCaseRole.
    /// </summary>
    [JsonPropertyName("linkingCaseRole")]
    public CaseRole LinkingCaseRole
    {
      get => linkingCaseRole ??= new();
      set => linkingCaseRole = value;
    }

    private OfficeServiceProvider existingOfficeServiceProvider;
    private LegalActionAssigment existingLegalActionAssigment;
    private LegalAction existingLegalAction;
    private CsePerson existingChild;
    private CsePerson existingMother;
    private CsePerson existingFather;
    private CaseUnit existingCaseUnit;
    private Case1 existingCase;
    private InterstateRequest existingInterstateRequest;
    private LegalActionCaseRole linkingLegalActionCaseRole;
    private CaseRole linkingCaseRole;
  }
#endregion
}

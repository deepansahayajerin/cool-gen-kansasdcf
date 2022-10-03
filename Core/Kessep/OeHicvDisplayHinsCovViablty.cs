// Program: OE_HICV_DISPLAY_HINS_COV_VIABLTY, ID: 371850660, model: 746.
// Short name: SWE00930
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
/// A program: OE_HICV_DISPLAY_HINS_COV_VIABLTY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block reads and populates health insurance coverage viability 
/// information for display.
/// </para>
/// </summary>
[Serializable]
public partial class OeHicvDisplayHinsCovViablty: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_HICV_DISPLAY_HINS_COV_VIABLTY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeHicvDisplayHinsCovViablty(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeHicvDisplayHinsCovViablty.
  /// </summary>
  public OeHicvDisplayHinsCovViablty(IContext context, Import import,
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
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE		CHG REQ#	DESCRIPTION
    // govind	05/17/95			Initial coding
    // G P Kim	04/30/97			Change Current Date
    // *********************************************
    // **********************************************************************
    // Who          When         Ref         Description
    // **********************************************************************
    // Gloria       04/27/2007   PR#00230558 Modified to add sort order for	
    //                                       
    // child person start date desc to
    //                                       
    // get active record,
    // **********************************************************************
    // ****************************************************************************************
    // G. Pan     11/27/2007   CQ469
    // 			Added a new Group View (100) for group_import_active_child
    //                         and group_export_active_child with three 
    // attributes -
    //                         1. id from case_role
    //                         2. number from child cse_person
    //                         3. flag for active/inactive child.
    //                         Added work view for import_child_count and 
    // export_child_count.
    //                         Added Identifier attribute to child entity view.
    //                         Modify logic in CASE display.
    // *****************************************************************************************
    // ****************************************************************************************
    // LSS      4/2/2008   PR330692  CQ3934
    //                               
    // Changed READ statement to READ
    // EACH in order to SORT
    //                               
    // and get the first record then
    // ESCAPE to the IF condition.
    // ****************************************************************************************
    // *********************************************
    // SYSTEM:		KESSEP
    // DESCRIPTION:
    // This action block reads all the information relevant to the determination
    // of health insurance viability alongwith the latest viability record.
    // PROCESSING:
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    //  CSE_PERSON			- R - -
    //  CASE				- R - -
    //  CASE_ROLE CHILD                - R - -
    //  PERSON_PROGRAM			- R - -
    //  PROGRAM			- R - -
    //  LEGAL_ACTION			- R - -
    //  LEGAL_ACTION_DETAIL		- R - -
    //  INCOME_SOURCE			- R - -
    //  PERSON_INCOME_HISTORY		- R - -
    //  HEALTH_INSURANCE_VIABILITY     - R - -
    // *********************************************
    local.Current.Date = Now().Date;
    export.ChildCsePerson.Number = import.ChildCsePerson.Number;
    export.ApCsePerson.Number = import.Ap.Number;
    export.Case1.Number = import.Case1.Number;
    export.ActiveChild1.Flag = "";

    // ****************************************************************************************
    // G. Pan     11/27/2007   CQ469
    // ****************************************************************************************
    export.ChildCount.Count = import.ChildCount.Count;

    if (Equal(global.Command, "DISPLAY"))
    {
    }
    else
    {
      for(import.ActiveChild.Index = 0; import.ActiveChild.Index < import
        .ActiveChild.Count; ++import.ActiveChild.Index)
      {
        if (!import.ActiveChild.CheckSize())
        {
          break;
        }

        export.ActiveChild.Index = import.ActiveChild.Index;
        export.ActiveChild.CheckSize();

        export.ActiveChild.Update.ActiveChildNum.Number =
          import.ActiveChild.Item.ActiveChildNum.Number;
        export.ActiveChild.Update.ActiveChildId.Identifier =
          import.ActiveChild.Item.ActiveChildId.Identifier;
        export.ActiveChild.Update.ActiveChild1.Flag =
          import.ActiveChild.Item.ActiveChild1.Flag;
      }

      import.ActiveChild.CheckIndex();
    }

    // ****************************************************************************************
    // G. Pan     11/27/2007   CQ469  Ended
    // ****************************************************************************************
    if (ReadCase())
    {
      if (AsChar(entities.ExistingCase.Status) == 'O')
      {
        export.CaseOpen.Flag = "Y";
      }
      else if (AsChar(entities.ExistingCase.Status) == 'C')
      {
        export.CaseOpen.Flag = "N";
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (IsEmpty(export.ApCsePerson.Number))
    {
      if (AsChar(export.CaseOpen.Flag) == 'Y')
      {
        foreach(var item in ReadCsePerson6())
        {
          if (AsChar(local.MultipleApExists.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }

          export.ApCsePerson.Number = entities.ExistingAp.Number;
          local.MultipleApExists.Flag = "Y";
        }
      }

      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        foreach(var item in ReadCsePersonAbsentParent())
        {
          if (AsChar(local.MultipleApExists.Flag) == 'Y')
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";

            return;
          }

          export.ApCsePerson.Number = entities.ExistingAp.Number;
          local.MultipleApExists.Flag = "Y";
        }
      }
    }

    if (ReadCsePerson1())
    {
      export.ApCsePerson.Number = entities.ExistingAp.Number;
      UseCabGetClientDetails3();
    }
    else
    {
      ExitState = "CO0000_ABSENT_PARENT_NF";

      return;
    }

    if (ReadCsePerson2())
    {
      export.ArCsePerson.Number = entities.ExistingAr.Number;
      UseCabGetClientDetails2();
    }
    else
    {
      if (AsChar(export.CaseOpen.Flag) == 'N')
      {
        if (ReadCsePersonApplicantRecipient())
        {
          export.ArCsePerson.Number = entities.ExistingAr.Number;
          UseCabGetClientDetails2();

          goto Read;
        }
      }

      ExitState = "CASE_ROLE_AR_NF";

      return;
    }

Read:

    switch(TrimEnd(import.UserAction.Command))
    {
      case "DISPLAY":
        local.ChildFound.Flag = "N";

        // ****************************************************************************************
        // G. Pan     11/27/2007   CQ469
        // 			For CASE display -
        //                         Modified first READ to create a table for 
        // active and inactive child.
        //                         Modified second READ to read from that table 
        // to get the first
        //                         child to display on HICV.
        // ****************************************************************************************
        export.ActiveChild.Update.ActiveChild1.Flag = "N";
        export.ActiveChild.Index = -1;
        export.ChildCount.Count = 1;

        foreach(var item in ReadCsePersonChild4())
        {
          local.ChildFound.Flag = "Y";

          ++export.ActiveChild.Index;
          export.ActiveChild.CheckSize();

          export.ActiveChild.Update.ActiveChildNum.Number =
            entities.ExistingChildCsePerson.Number;
          export.ActiveChild.Update.ActiveChildId.Identifier =
            entities.ExistingChild.Identifier;

          if (!Lt(local.Current.Date, entities.ExistingChild.StartDate) && !
            Lt(entities.ExistingChild.EndDate, local.Current.Date))
          {
            export.ActiveChild.Update.ActiveChild1.Flag = "Y";
          }
          else
          {
            export.ActiveChild.Update.ActiveChild1.Flag = "N";
          }
        }

        if (AsChar(local.ChildFound.Flag) == 'N')
        {
          ExitState = "CASE_ROLE_CHILD_NF";

          return;
        }

        export.ActiveChild.Index = 0;
        export.ActiveChild.CheckSize();

        if (ReadCsePersonChild2())
        {
          local.Child.Number = entities.ExistingChildCsePerson.Number;

          if (!Lt(local.Current.Date, entities.ExistingChild.StartDate) && !
            Lt(entities.ExistingChild.EndDate, local.Current.Date))
          {
            export.ActiveChild1.Flag = "Y";
          }
          else
          {
            export.ActiveChild1.Flag = "N";
          }
        }

        // ****************************************************************************************
        // G. Pan     11/27/2007   CQ469
        // 			For CASE chpv and chnx -
        //                         Modified READ statement to qualify child 
        // case_role id and child
        //                         cse_person number equal to the one passing 
        // from Pstep.
        // ****************************************************************************************
        break;
      case "CHPV":
        local.ChildFound.Flag = "N";

        if (ReadCsePersonChild1())
        {
          local.Child.Number = entities.ExistingChildCsePerson.Number;
          local.ChildFound.Flag = "Y";

          if (!Lt(local.Current.Date, entities.ExistingChild.StartDate) && !
            Lt(entities.ExistingChild.EndDate, local.Current.Date))
          {
            export.ActiveChild1.Flag = "Y";
          }
          else
          {
            export.ActiveChild1.Flag = "N";
          }
        }

        if (AsChar(local.ChildFound.Flag) == 'N')
        {
          UseCabGetClientDetails4();
          ExitState = "OE0119_NO_MORE_CHILD";

          return;
        }

        break;
      case "CHNX":
        local.ChildFound.Flag = "N";

        if (ReadCsePersonChild1())
        {
          local.Child.Number = entities.ExistingChildCsePerson.Number;
          local.ChildFound.Flag = "Y";

          if (!Lt(local.Current.Date, entities.ExistingChild.StartDate) && !
            Lt(entities.ExistingChild.EndDate, local.Current.Date))
          {
            export.ActiveChild1.Flag = "Y";
          }
          else
          {
            export.ActiveChild1.Flag = "N";
          }
        }

        if (AsChar(local.ChildFound.Flag) == 'N')
        {
          UseCabGetClientDetails4();
          ExitState = "OE0119_NO_MORE_CHILD";

          return;
        }

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        return;
    }

    // ****************************************************************************************
    // LSS      4/2/2008   PR330692  CQ3934
    //                               
    // Changed READ statement to READ
    // EACH in order to SORT
    //                               
    // to get the first record
    // according to the sort when
    // there are
    //                               
    // multiple records (identifiers)
    // for the Child then ESCAPE
    //                               
    // to the IF condition.
    // ****************************************************************************************
    if (ReadCsePersonChild3())
    {
      export.ChildCsePerson.Number = entities.ExistingChildCsePerson.Number;
      MoveCaseRole(entities.ExistingChild, export.Child);
    }

    if (entities.ExistingChild.Populated)
    {
    }
    else
    {
      UseCabGetClientDetails4();
      ExitState = "OE0118_ERROR_READING_CHILD";

      return;
    }

    UseCabGetClientDetails1();

    export.ChildsProgram.Index = 0;
    export.ChildsProgram.Clear();

    foreach(var item in ReadProgramPersonProgram())
    {
      export.ChildsProgram.Update.DetailChilds.Code =
        entities.ExistingChildProgram.Code;
      export.ChildsProgram.Next();
    }

    export.AvailHins.Index = 0;
    export.AvailHins.Clear();

    foreach(var item in ReadHealthInsuranceCoveragePersonalHealthInsurance())
    {
      if (ReadCsePerson3())
      {
        export.AvailHins.Update.DetailPolicyHoldr.Number =
          entities.ExistingHinsPolicyHolder.Number;
      }

      MoveHealthInsuranceCoverage(entities.
        ExistingAvailCovForChildHealthInsuranceCoverage,
        export.AvailHins.Update.DetailAvailHinsHealthInsuranceCoverage);
      MovePersonalHealthInsurance(entities.
        ExistingAvailCovForChildPersonalHealthInsurance,
        export.AvailHins.Update.DetailAvailHinsPersonalHealthInsurance);
      export.AvailHins.Next();
    }

    export.MonthlyTotalIncome.TotalCurrency = 0;

    foreach(var item in ReadIncomeSource())
    {
      foreach(var item1 in ReadPersonIncomeHistory())
      {
        UseCabComputeAvgMonthlyIncome();
        export.MonthlyTotalIncome.TotalCurrency += local.ApsIncome.
          TotalCurrency;

        goto ReadEach;
      }

ReadEach:
      ;
    }

    export.CurrentOblg.Index = 0;
    export.CurrentOblg.Clear();

    foreach(var item in ReadLegalActionDetailObligationType())
    {
      export.CurrentOblg.Update.DetailCurrentOblgLegalActionDetail.Assign(
        entities.ExistingMonetaryOblgLegalActionDetail);
      export.CurrentOblg.Update.DetailCurrentOblgObligationType.Code =
        entities.ExistingMonetaryOblgObligationType.Code;
      export.CurrentOblg.Next();
    }

    export.CurrentOblg.Index = 0;
    export.CurrentOblg.Clear();

    foreach(var item in ReadLegalActionDetailObligationTypeLegalActionPerson())
    {
      export.CurrentOblg.Update.DetailCurrentOblgLegalActionDetail.Assign(
        entities.ExistingMonetaryOblgLegalActionDetail);
      export.CurrentOblg.Update.DetailCurrentOblgLegalActionDetail.
        CurrentAmount = entities.ExisitngMonetaryOblgObligor.CurrentAmount;
      export.CurrentOblg.Update.DetailCurrentOblgObligationType.Code =
        entities.ExistingMonetaryOblgObligationType.Code;
      export.CurrentOblg.Next();
    }

    if (ReadLegalActionLegalActionDetailLegalActionPerson())
    {
      MoveLegalActionDetail(entities.ExistingHinsCovOblgLegalActionDetail,
        export.HinsCovOblgLegalActionDetail);
      export.HinsCovOblgLegalAction.CourtCaseNumber =
        entities.ExistingHinsCovOblgLegalAction.CourtCaseNumber;
    }

    if (ReadHealthInsuranceViability())
    {
      MoveHealthInsuranceViability(entities.ExistingHealthInsuranceViability,
        export.HealthInsuranceViability);

      export.HicvNote.Index = 0;
      export.HicvNote.Clear();

      foreach(var item in ReadHinsViabNote())
      {
        export.HicvNote.Update.Detail.Note = entities.ExistingHinsViabNote.Note;
        export.HicvNote.Next();
      }
    }

    export.ChildrenNeedingCoverage.Count = 0;

    foreach(var item in ReadCsePerson7())
    {
      local.ChildHasCoverage.Flag = "N";

      if (ReadPersonalHealthInsurance())
      {
        local.ChildHasCoverage.Flag = "Y";
      }

      if (AsChar(local.ChildHasCoverage.Flag) == 'N')
      {
        ++export.ChildrenNeedingCoverage.Count;
      }
    }

    export.ScrollingAttributes.PlusFlag = "";
    export.ScrollingAttributes.MinusFlag = "";

    if (ReadCsePerson4())
    {
      export.ScrollingAttributes.PlusFlag = "+";
    }

    if (ReadCsePerson5())
    {
      export.ScrollingAttributes.MinusFlag = "-";
    }
  }

  private static void MoveCaseRole(CaseRole source, CaseRole target)
  {
    target.Type1 = source.Type1;
    target.StartDate = source.StartDate;
    target.EndDate = source.EndDate;
    target.ArWaivedInsurance = source.ArWaivedInsurance;
    target.DateOfEmancipation = source.DateOfEmancipation;
    target.Over18AndInSchool = source.Over18AndInSchool;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveHealthInsuranceCoverage(
    HealthInsuranceCoverage source, HealthInsuranceCoverage target)
  {
    target.Identifier = source.Identifier;
    target.PolicyPaidByCsePersonInd = source.PolicyPaidByCsePersonInd;
    target.InsuranceGroupNumber = source.InsuranceGroupNumber;
    target.InsurancePolicyNumber = source.InsurancePolicyNumber;
    target.PolicyExpirationDate = source.PolicyExpirationDate;
  }

  private static void MoveHealthInsuranceViability(
    HealthInsuranceViability source, HealthInsuranceViability target)
  {
    target.Identifier = source.Identifier;
    target.HinsViableInd = source.HinsViableInd;
    target.HinsViableIndWorkerId = source.HinsViableIndWorkerId;
    target.HinsViableIndUpdatedDate = source.HinsViableIndUpdatedDate;
  }

  private static void MoveLegalActionDetail(LegalActionDetail source,
    LegalActionDetail target)
  {
    target.EndDate = source.EndDate;
    target.EffectiveDate = source.EffectiveDate;
  }

  private static void MovePersonalHealthInsurance(
    PersonalHealthInsurance source, PersonalHealthInsurance target)
  {
    target.CoverageVerifiedDate = source.CoverageVerifiedDate;
    target.PremiumVerifiedDate = source.PremiumVerifiedDate;
    target.CoverageCostAmount = source.CoverageCostAmount;
    target.CoverageEndDate = source.CoverageEndDate;
  }

  private void UseCabComputeAvgMonthlyIncome()
  {
    var useImport = new CabComputeAvgMonthlyIncome.Import();
    var useExport = new CabComputeAvgMonthlyIncome.Export();

    useImport.New1.Assign(entities.ExistingAps);

    Call(CabComputeAvgMonthlyIncome.Execute, useImport, useExport);

    local.ApsIncome.TotalCurrency = useExport.Common.TotalCurrency;
  }

  private void UseCabGetClientDetails1()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingChildCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.ChildCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseCabGetClientDetails2()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingAr.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.ArCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseCabGetClientDetails3()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingAp.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    MoveCsePersonsWorkSet(useExport.CsePersonsWorkSet,
      export.ApCsePersonsWorkSet);
  }

  private void UseCabGetClientDetails4()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = import.ChildCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.ChildCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingAp.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", export.ApCsePerson.Number);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingAp.Number = db.GetString(reader, 0);
        entities.ExistingAp.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    entities.ExistingAr.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAr.Number = db.GetString(reader, 0);
        entities.ExistingAr.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingAvailCovForChildHealthInsuranceCoverage.Populated);
    entities.ExistingHinsPolicyHolder.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(
          command, "numb",
          entities.ExistingAvailCovForChildHealthInsuranceCoverage.
            CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingHinsPolicyHolder.Number = db.GetString(reader, 0);
        entities.ExistingHinsPolicyHolder.Populated = true;
      });
  }

  private bool ReadCsePerson4()
  {
    entities.ExistingNextOrPrevChild.Populated = false;

    return Read("ReadCsePerson4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "numb", entities.ExistingChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingNextOrPrevChild.Number = db.GetString(reader, 0);
        entities.ExistingNextOrPrevChild.Populated = true;
      });
  }

  private bool ReadCsePerson5()
  {
    entities.ExistingNextOrPrevChild.Populated = false;

    return Read("ReadCsePerson5",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "numb", entities.ExistingChildCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingNextOrPrevChild.Number = db.GetString(reader, 0);
        entities.ExistingNextOrPrevChild.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCsePerson6()
  {
    entities.ExistingAp.Populated = false;

    return ReadEach("ReadCsePerson6",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAp.Number = db.GetString(reader, 0);
        entities.ExistingAp.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson7()
  {
    entities.ExistingNeedsCoverageChildCsePerson.Populated = false;

    return ReadEach("ReadCsePerson7",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingNeedsCoverageChildCsePerson.Number =
          db.GetString(reader, 0);
        entities.ExistingNeedsCoverageChildCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent()
  {
    entities.ExistingAbsentParent.Populated = false;
    entities.ExistingAp.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAp.Number = db.GetString(reader, 0);
        entities.ExistingAbsentParent.CspNumber = db.GetString(reader, 0);
        entities.ExistingAbsentParent.CasNumber = db.GetString(reader, 1);
        entities.ExistingAbsentParent.Type1 = db.GetString(reader, 2);
        entities.ExistingAbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingAbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingAbsentParent.Populated = true;
        entities.ExistingAp.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingAbsentParent.Type1);

        return true;
      });
  }

  private bool ReadCsePersonApplicantRecipient()
  {
    entities.ExistingAr.Populated = false;
    entities.ExistingApplicantRecipient.Populated = false;

    return Read("ReadCsePersonApplicantRecipient",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAr.Number = db.GetString(reader, 0);
        entities.ExistingApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.ExistingApplicantRecipient.CasNumber = db.GetString(reader, 1);
        entities.ExistingApplicantRecipient.Type1 = db.GetString(reader, 2);
        entities.ExistingApplicantRecipient.Identifier = db.GetInt32(reader, 3);
        entities.ExistingApplicantRecipient.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingApplicantRecipient.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingAr.Populated = true;
        entities.ExistingApplicantRecipient.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingApplicantRecipient.Type1);
          
      });
  }

  private bool ReadCsePersonChild1()
  {
    entities.ExistingChild.Populated = false;
    entities.ExistingChildCsePerson.Populated = false;

    return Read("ReadCsePersonChild1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", export.ChildCsePerson.Number);
        db.SetInt32(command, "caseRoleId", import.ChildCaseRole.Identifier);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingChildCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingChild.CspNumber = db.GetString(reader, 0);
        entities.ExistingChild.CasNumber = db.GetString(reader, 1);
        entities.ExistingChild.Type1 = db.GetString(reader, 2);
        entities.ExistingChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingChild.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingChild.ArWaivedInsurance =
          db.GetNullableString(reader, 6);
        entities.ExistingChild.DateOfEmancipation =
          db.GetNullableDate(reader, 7);
        entities.ExistingChild.Over18AndInSchool =
          db.GetNullableString(reader, 8);
        entities.ExistingChild.Populated = true;
        entities.ExistingChildCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingChild.Type1);
      });
  }

  private bool ReadCsePersonChild2()
  {
    entities.ExistingChild.Populated = false;
    entities.ExistingChildCsePerson.Populated = false;

    return Read("ReadCsePersonChild2",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", export.ActiveChild.Item.ActiveChildNum.Number);
        db.SetInt32(
          command, "caseRoleId",
          export.ActiveChild.Item.ActiveChildId.Identifier);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingChildCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingChild.CspNumber = db.GetString(reader, 0);
        entities.ExistingChild.CasNumber = db.GetString(reader, 1);
        entities.ExistingChild.Type1 = db.GetString(reader, 2);
        entities.ExistingChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingChild.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingChild.ArWaivedInsurance =
          db.GetNullableString(reader, 6);
        entities.ExistingChild.DateOfEmancipation =
          db.GetNullableDate(reader, 7);
        entities.ExistingChild.Over18AndInSchool =
          db.GetNullableString(reader, 8);
        entities.ExistingChild.Populated = true;
        entities.ExistingChildCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingChild.Type1);
      });
  }

  private bool ReadCsePersonChild3()
  {
    entities.ExistingChild.Populated = false;
    entities.ExistingChildCsePerson.Populated = false;

    return Read("ReadCsePersonChild3",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", local.Child.Number);
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingChildCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingChild.CspNumber = db.GetString(reader, 0);
        entities.ExistingChild.CasNumber = db.GetString(reader, 1);
        entities.ExistingChild.Type1 = db.GetString(reader, 2);
        entities.ExistingChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingChild.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingChild.ArWaivedInsurance =
          db.GetNullableString(reader, 6);
        entities.ExistingChild.DateOfEmancipation =
          db.GetNullableDate(reader, 7);
        entities.ExistingChild.Over18AndInSchool =
          db.GetNullableString(reader, 8);
        entities.ExistingChild.Populated = true;
        entities.ExistingChildCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingChild.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonChild4()
  {
    entities.ExistingChild.Populated = false;
    entities.ExistingChildCsePerson.Populated = false;

    return ReadEach("ReadCsePersonChild4",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
      },
      (db, reader) =>
      {
        entities.ExistingChildCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingChild.CspNumber = db.GetString(reader, 0);
        entities.ExistingChild.CasNumber = db.GetString(reader, 1);
        entities.ExistingChild.Type1 = db.GetString(reader, 2);
        entities.ExistingChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingChild.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingChild.ArWaivedInsurance =
          db.GetNullableString(reader, 6);
        entities.ExistingChild.DateOfEmancipation =
          db.GetNullableDate(reader, 7);
        entities.ExistingChild.Over18AndInSchool =
          db.GetNullableString(reader, 8);
        entities.ExistingChild.Populated = true;
        entities.ExistingChildCsePerson.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingChild.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadHealthInsuranceCoveragePersonalHealthInsurance()
  {
    return ReadEach("ReadHealthInsuranceCoveragePersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
        db.SetNullableDate(
          command, "coverEndDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.AvailHins.IsFull)
        {
          return false;
        }

        entities.ExistingAvailCovForChildHealthInsuranceCoverage.Identifier =
          db.GetInt64(reader, 0);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.HcvId =
          db.GetInt64(reader, 0);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.
          PolicyPaidByCsePersonInd = db.GetString(reader, 1);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.
          InsuranceGroupNumber = db.GetNullableString(reader, 2);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.VerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.
          VerifiedUserId = db.GetNullableString(reader, 4);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.
          InsurancePolicyNumber = db.GetNullableString(reader, 5);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.
          PolicyExpirationDate = db.GetNullableDate(reader, 6);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.CoverageCode1 =
          db.GetNullableString(reader, 7);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.CoverageCode2 =
          db.GetNullableString(reader, 8);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.CoverageCode3 =
          db.GetNullableString(reader, 9);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.CoverageCode4 =
          db.GetNullableString(reader, 10);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.CoverageCode5 =
          db.GetNullableString(reader, 11);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.CoverageCode6 =
          db.GetNullableString(reader, 12);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.CoverageCode7 =
          db.GetNullableString(reader, 13);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.
          PolicyEffectiveDate = db.GetNullableDate(reader, 14);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.CreatedBy =
          db.GetString(reader, 15);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.
          CreatedTimestamp = db.GetDateTime(reader, 16);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.LastUpdatedBy =
          db.GetString(reader, 17);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.
          LastUpdatedTimestamp = db.GetDateTime(reader, 18);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.CspNumber =
          db.GetNullableString(reader, 19);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.
          OtherCoveredPersons = db.GetNullableString(reader, 20);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.CspNumber =
          db.GetString(reader, 21);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.
          VerifiedUserId = db.GetNullableString(reader, 22);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.
          CoverageVerifiedDate = db.GetNullableDate(reader, 23);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.
          AlertFlagInsuranceExistsInd = db.GetNullableString(reader, 24);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.
          CoverageCostAmount = db.GetNullableDecimal(reader, 25);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.
          CoverageBeginDate = db.GetNullableDate(reader, 26);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.
          CoverageEndDate = db.GetNullableDate(reader, 27);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.CreatedBy =
          db.GetString(reader, 28);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.
          CreatedTimestamp = db.GetDateTime(reader, 29);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.LastUpdatedBy =
          db.GetString(reader, 30);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.
          LastUpdatedTimestamp = db.GetDateTime(reader, 31);
        entities.ExistingAvailCovForChildPersonalHealthInsurance.
          PremiumVerifiedDate = db.GetNullableDate(reader, 32);
        entities.ExistingAvailCovForChildHealthInsuranceCoverage.Populated =
          true;
        entities.ExistingAvailCovForChildPersonalHealthInsurance.Populated =
          true;

        return true;
      });
  }

  private bool ReadHealthInsuranceViability()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingChild.Populated);
    entities.ExistingHealthInsuranceViability.Populated = false;

    return Read("ReadHealthInsuranceViability",
      (db, command) =>
      {
        db.
          SetInt32(command, "croIdentifier", entities.ExistingChild.Identifier);
          
        db.SetString(command, "croType", entities.ExistingChild.Type1);
        db.SetString(command, "casNumber", entities.ExistingChild.CasNumber);
        db.SetString(command, "cspNumber", entities.ExistingChild.CspNumber);
        db.SetNullableString(command, "cspNum", entities.ExistingAp.Number);
      },
      (db, reader) =>
      {
        entities.ExistingHealthInsuranceViability.CroType =
          db.GetString(reader, 0);
        entities.ExistingHealthInsuranceViability.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingHealthInsuranceViability.CasNumber =
          db.GetString(reader, 2);
        entities.ExistingHealthInsuranceViability.CroIdentifier =
          db.GetInt32(reader, 3);
        entities.ExistingHealthInsuranceViability.Identifier =
          db.GetInt32(reader, 4);
        entities.ExistingHealthInsuranceViability.HinsViableInd =
          db.GetNullableString(reader, 5);
        entities.ExistingHealthInsuranceViability.HinsViableIndWorkerId =
          db.GetNullableString(reader, 6);
        entities.ExistingHealthInsuranceViability.HinsViableIndUpdatedDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingHealthInsuranceViability.CspNum =
          db.GetNullableString(reader, 8);
        entities.ExistingHealthInsuranceViability.Populated = true;
        CheckValid<HealthInsuranceViability>("CroType",
          entities.ExistingHealthInsuranceViability.CroType);
      });
  }

  private IEnumerable<bool> ReadHinsViabNote()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingHealthInsuranceViability.Populated);

    return ReadEach("ReadHinsViabNote",
      (db, command) =>
      {
        db.SetInt32(
          command, "hivId",
          entities.ExistingHealthInsuranceViability.Identifier);
        db.SetString(
          command, "casNumber",
          entities.ExistingHealthInsuranceViability.CasNumber);
        db.SetInt32(
          command, "croId",
          entities.ExistingHealthInsuranceViability.CroIdentifier);
        db.SetString(
          command, "cspNumber",
          entities.ExistingHealthInsuranceViability.CspNumber);
        db.SetString(
          command, "croType",
          entities.ExistingHealthInsuranceViability.CroType);
      },
      (db, reader) =>
      {
        if (export.HicvNote.IsFull)
        {
          return false;
        }

        entities.ExistingHinsViabNote.Identifier = db.GetInt32(reader, 0);
        entities.ExistingHinsViabNote.Note = db.GetNullableString(reader, 1);
        entities.ExistingHinsViabNote.CroId = db.GetInt32(reader, 2);
        entities.ExistingHinsViabNote.CroType = db.GetString(reader, 3);
        entities.ExistingHinsViabNote.CspNumber = db.GetString(reader, 4);
        entities.ExistingHinsViabNote.CasNumber = db.GetString(reader, 5);
        entities.ExistingHinsViabNote.HivId = db.GetInt32(reader, 6);
        entities.ExistingHinsViabNote.Populated = true;
        CheckValid<HinsViabNote>("CroType",
          entities.ExistingHinsViabNote.CroType);

        return true;
      });
  }

  private IEnumerable<bool> ReadIncomeSource()
  {
    entities.ExistingApsIncome.Populated = false;

    return ReadEach("ReadIncomeSource",
      (db, command) =>
      {
        db.SetString(command, "cspINumber", entities.ExistingAp.Number);
        db.SetNullableDate(
          command, "startDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingApsIncome.Identifier = db.GetDateTime(reader, 0);
        entities.ExistingApsIncome.Type1 = db.GetString(reader, 1);
        entities.ExistingApsIncome.CspINumber = db.GetString(reader, 2);
        entities.ExistingApsIncome.StartDt = db.GetNullableDate(reader, 3);
        entities.ExistingApsIncome.EndDt = db.GetNullableDate(reader, 4);
        entities.ExistingApsIncome.Populated = true;
        CheckValid<IncomeSource>("Type1", entities.ExistingApsIncome.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetailObligationType()
  {
    return ReadEach("ReadLegalActionDetailObligationType",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", entities.ExistingAp.Number);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.CurrentOblg.IsFull)
        {
          return false;
        }

        entities.ExistingMonetaryOblgLegalActionDetail.LgaIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingMonetaryOblgLegalActionDetail.Number =
          db.GetInt32(reader, 1);
        entities.ExistingMonetaryOblgLegalActionDetail.EndDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingMonetaryOblgLegalActionDetail.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingMonetaryOblgLegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingMonetaryOblgLegalActionDetail.DetailType =
          db.GetString(reader, 5);
        entities.ExistingMonetaryOblgLegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 6);
        entities.ExistingMonetaryOblgLegalActionDetail.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingMonetaryOblgObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingMonetaryOblgObligationType.Code =
          db.GetString(reader, 8);
        entities.ExistingMonetaryOblgObligationType.Populated = true;
        entities.ExistingMonetaryOblgLegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.ExistingMonetaryOblgLegalActionDetail.DetailType);

        return true;
      });
  }

  private IEnumerable<bool>
    ReadLegalActionDetailObligationTypeLegalActionPerson()
  {
    return ReadEach("ReadLegalActionDetailObligationTypeLegalActionPerson",
      (db, command) =>
      {
        db.
          SetNullableString(command, "cspNumber", export.ChildCsePerson.Number);
          
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.CurrentOblg.IsFull)
        {
          return false;
        }

        entities.ExistingMonetaryOblgLegalActionDetail.LgaIdentifier =
          db.GetInt32(reader, 0);
        entities.ExisitngMonetaryOblgObligor.LgaRIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingMonetaryOblgLegalActionDetail.Number =
          db.GetInt32(reader, 1);
        entities.ExisitngMonetaryOblgObligor.LadRNumber =
          db.GetNullableInt32(reader, 1);
        entities.ExistingMonetaryOblgLegalActionDetail.EndDate =
          db.GetNullableDate(reader, 2);
        entities.ExistingMonetaryOblgLegalActionDetail.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingMonetaryOblgLegalActionDetail.CurrentAmount =
          db.GetNullableDecimal(reader, 4);
        entities.ExistingMonetaryOblgLegalActionDetail.DetailType =
          db.GetString(reader, 5);
        entities.ExistingMonetaryOblgLegalActionDetail.FreqPeriodCode =
          db.GetNullableString(reader, 6);
        entities.ExistingMonetaryOblgLegalActionDetail.OtyId =
          db.GetNullableInt32(reader, 7);
        entities.ExistingMonetaryOblgObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 7);
        entities.ExistingMonetaryOblgObligationType.Code =
          db.GetString(reader, 8);
        entities.ExisitngMonetaryOblgObligor.Identifier =
          db.GetInt32(reader, 9);
        entities.ExisitngMonetaryOblgObligor.CspNumber =
          db.GetNullableString(reader, 10);
        entities.ExisitngMonetaryOblgObligor.EffectiveDate =
          db.GetDate(reader, 11);
        entities.ExisitngMonetaryOblgObligor.Role = db.GetString(reader, 12);
        entities.ExisitngMonetaryOblgObligor.EndDate =
          db.GetNullableDate(reader, 13);
        entities.ExisitngMonetaryOblgObligor.AccountType =
          db.GetNullableString(reader, 14);
        entities.ExisitngMonetaryOblgObligor.CurrentAmount =
          db.GetNullableDecimal(reader, 15);
        entities.ExisitngMonetaryOblgObligor.Populated = true;
        entities.ExistingMonetaryOblgObligationType.Populated = true;
        entities.ExistingMonetaryOblgLegalActionDetail.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.ExistingMonetaryOblgLegalActionDetail.DetailType);

        return true;
      });
  }

  private bool ReadLegalActionLegalActionDetailLegalActionPerson()
  {
    entities.ExistingHinsCovOblgChild.Populated = false;
    entities.ExistingHinsCovOblgLegalActionDetail.Populated = false;
    entities.ExistingHinsCovOblgLegalAction.Populated = false;

    return Read("ReadLegalActionLegalActionDetailLegalActionPerson",
      (db, command) =>
      {
        db.SetNullableString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
        db.SetNullableDate(
          command, "endDt", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "filedDt", local.Null1.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingHinsCovOblgLegalAction.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingHinsCovOblgLegalActionDetail.LgaIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingHinsCovOblgChild.LgaRIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ExistingHinsCovOblgLegalAction.Classification =
          db.GetString(reader, 1);
        entities.ExistingHinsCovOblgLegalAction.Type1 = db.GetString(reader, 2);
        entities.ExistingHinsCovOblgLegalAction.FiledDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingHinsCovOblgLegalAction.CourtCaseNumber =
          db.GetNullableString(reader, 4);
        entities.ExistingHinsCovOblgLegalAction.EndDate =
          db.GetNullableDate(reader, 5);
        entities.ExistingHinsCovOblgLegalActionDetail.Number =
          db.GetInt32(reader, 6);
        entities.ExistingHinsCovOblgChild.LadRNumber =
          db.GetNullableInt32(reader, 6);
        entities.ExistingHinsCovOblgLegalActionDetail.EndDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingHinsCovOblgLegalActionDetail.EffectiveDate =
          db.GetDate(reader, 8);
        entities.ExistingHinsCovOblgLegalActionDetail.NonFinOblgType =
          db.GetNullableString(reader, 9);
        entities.ExistingHinsCovOblgLegalActionDetail.DetailType =
          db.GetString(reader, 10);
        entities.ExistingHinsCovOblgChild.Identifier = db.GetInt32(reader, 11);
        entities.ExistingHinsCovOblgChild.CspNumber =
          db.GetNullableString(reader, 12);
        entities.ExistingHinsCovOblgChild.EffectiveDate =
          db.GetDate(reader, 13);
        entities.ExistingHinsCovOblgChild.Role = db.GetString(reader, 14);
        entities.ExistingHinsCovOblgChild.EndDate =
          db.GetNullableDate(reader, 15);
        entities.ExistingHinsCovOblgChild.AccountType =
          db.GetNullableString(reader, 16);
        entities.ExistingHinsCovOblgChild.Populated = true;
        entities.ExistingHinsCovOblgLegalActionDetail.Populated = true;
        entities.ExistingHinsCovOblgLegalAction.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.ExistingHinsCovOblgLegalActionDetail.DetailType);
      });
  }

  private IEnumerable<bool> ReadPersonIncomeHistory()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingApsIncome.Populated);
    entities.ExistingAps.Populated = false;

    return ReadEach("ReadPersonIncomeHistory",
      (db, command) =>
      {
        db.SetString(
          command, "cspINumber", entities.ExistingApsIncome.CspINumber);
        db.SetDateTime(
          command, "isrIdentifier",
          entities.ExistingApsIncome.Identifier.GetValueOrDefault());
        db.SetNullableDate(
          command, "incomeEffDt", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingAps.CspNumber = db.GetString(reader, 0);
        entities.ExistingAps.IsrIdentifier = db.GetDateTime(reader, 1);
        entities.ExistingAps.Identifier = db.GetDateTime(reader, 2);
        entities.ExistingAps.IncomeEffDt = db.GetNullableDate(reader, 3);
        entities.ExistingAps.IncomeAmt = db.GetNullableDecimal(reader, 4);
        entities.ExistingAps.Freq = db.GetNullableString(reader, 5);
        entities.ExistingAps.CspINumber = db.GetString(reader, 6);
        entities.ExistingAps.MilitaryBaqAllotment =
          db.GetNullableDecimal(reader, 7);
        entities.ExistingAps.Populated = true;

        return true;
      });
  }

  private bool ReadPersonalHealthInsurance()
  {
    entities.ExistingNeedsCoverageChildPersonalHealthInsurance.Populated =
      false;

    return Read("ReadPersonalHealthInsurance",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber",
          entities.ExistingNeedsCoverageChildCsePerson.Number);
        db.SetNullableDate(
          command, "coverEndDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingNeedsCoverageChildPersonalHealthInsurance.HcvId =
          db.GetInt64(reader, 0);
        entities.ExistingNeedsCoverageChildPersonalHealthInsurance.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingNeedsCoverageChildPersonalHealthInsurance.
          CoverageEndDate = db.GetNullableDate(reader, 2);
        entities.ExistingNeedsCoverageChildPersonalHealthInsurance.Populated =
          true;
      });
  }

  private IEnumerable<bool> ReadProgramPersonProgram()
  {
    return ReadEach("ReadProgramPersonProgram",
      (db, command) =>
      {
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
      },
      (db, reader) =>
      {
        if (export.ChildsProgram.IsFull)
        {
          return false;
        }

        entities.ExistingChildProgram.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ExistingChildPersonProgram.PrgGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingChildProgram.Code = db.GetString(reader, 1);
        entities.ExistingChildPersonProgram.CspNumber = db.GetString(reader, 2);
        entities.ExistingChildPersonProgram.EffectiveDate =
          db.GetDate(reader, 3);
        entities.ExistingChildPersonProgram.Status =
          db.GetNullableString(reader, 4);
        entities.ExistingChildPersonProgram.ClosureReason =
          db.GetNullableString(reader, 5);
        entities.ExistingChildPersonProgram.AssignedDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingChildPersonProgram.DiscontinueDate =
          db.GetNullableDate(reader, 7);
        entities.ExistingChildPersonProgram.CreatedTimestamp =
          db.GetDateTime(reader, 8);
        entities.ExistingChildProgram.Populated = true;
        entities.ExistingChildPersonProgram.Populated = true;

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
    /// <summary>A ActiveChildGroup group.</summary>
    [Serializable]
    public class ActiveChildGroup
    {
      /// <summary>
      /// A value of ActiveChildId.
      /// </summary>
      [JsonPropertyName("activeChildId")]
      public CaseRole ActiveChildId
      {
        get => activeChildId ??= new();
        set => activeChildId = value;
      }

      /// <summary>
      /// A value of ActiveChildNum.
      /// </summary>
      [JsonPropertyName("activeChildNum")]
      public CsePersonsWorkSet ActiveChildNum
      {
        get => activeChildNum ??= new();
        set => activeChildNum = value;
      }

      /// <summary>
      /// A value of ActiveChild1.
      /// </summary>
      [JsonPropertyName("activeChild1")]
      public Common ActiveChild1
      {
        get => activeChild1 ??= new();
        set => activeChild1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CaseRole activeChildId;
      private CsePersonsWorkSet activeChildNum;
      private Common activeChild1;
    }

    /// <summary>
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
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

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// Gets a value of ActiveChild.
    /// </summary>
    [JsonIgnore]
    public Array<ActiveChildGroup> ActiveChild => activeChild ??= new(
      ActiveChildGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ActiveChild for json serialization.
    /// </summary>
    [JsonPropertyName("activeChild")]
    [Computed]
    public IList<ActiveChildGroup> ActiveChild_Json
    {
      get => activeChild;
      set => ActiveChild.Assign(value);
    }

    /// <summary>
    /// A value of ChildCount.
    /// </summary>
    [JsonPropertyName("childCount")]
    public Common ChildCount
    {
      get => childCount ??= new();
      set => childCount = value;
    }

    private CaseRole childCaseRole;
    private CsePerson ap;
    private Case1 case1;
    private CsePerson childCsePerson;
    private Common userAction;
    private Array<ActiveChildGroup> activeChild;
    private Common childCount;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ActiveChildGroup group.</summary>
    [Serializable]
    public class ActiveChildGroup
    {
      /// <summary>
      /// A value of ActiveChildId.
      /// </summary>
      [JsonPropertyName("activeChildId")]
      public CaseRole ActiveChildId
      {
        get => activeChildId ??= new();
        set => activeChildId = value;
      }

      /// <summary>
      /// A value of ActiveChildNum.
      /// </summary>
      [JsonPropertyName("activeChildNum")]
      public CsePersonsWorkSet ActiveChildNum
      {
        get => activeChildNum ??= new();
        set => activeChildNum = value;
      }

      /// <summary>
      /// A value of ActiveChild1.
      /// </summary>
      [JsonPropertyName("activeChild1")]
      public Common ActiveChild1
      {
        get => activeChild1 ??= new();
        set => activeChild1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private CaseRole activeChildId;
      private CsePersonsWorkSet activeChildNum;
      private Common activeChild1;
    }

    /// <summary>A HicvNoteGroup group.</summary>
    [Serializable]
    public class HicvNoteGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public HinsViabNote Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private HinsViabNote detail;
    }

    /// <summary>A ChildsProgramGroup group.</summary>
    [Serializable]
    public class ChildsProgramGroup
    {
      /// <summary>
      /// A value of DetailChilds.
      /// </summary>
      [JsonPropertyName("detailChilds")]
      public Program DetailChilds
      {
        get => detailChilds ??= new();
        set => detailChilds = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 10;

      private Program detailChilds;
    }

    /// <summary>A AvailHinsGroup group.</summary>
    [Serializable]
    public class AvailHinsGroup
    {
      /// <summary>
      /// A value of DetailPolicyHoldr.
      /// </summary>
      [JsonPropertyName("detailPolicyHoldr")]
      public CsePerson DetailPolicyHoldr
      {
        get => detailPolicyHoldr ??= new();
        set => detailPolicyHoldr = value;
      }

      /// <summary>
      /// A value of DetailAvailHinsPersonalHealthInsurance.
      /// </summary>
      [JsonPropertyName("detailAvailHinsPersonalHealthInsurance")]
      public PersonalHealthInsurance DetailAvailHinsPersonalHealthInsurance
      {
        get => detailAvailHinsPersonalHealthInsurance ??= new();
        set => detailAvailHinsPersonalHealthInsurance = value;
      }

      /// <summary>
      /// A value of DetailAvailHinsHealthInsuranceCoverage.
      /// </summary>
      [JsonPropertyName("detailAvailHinsHealthInsuranceCoverage")]
      public HealthInsuranceCoverage DetailAvailHinsHealthInsuranceCoverage
      {
        get => detailAvailHinsHealthInsuranceCoverage ??= new();
        set => detailAvailHinsHealthInsuranceCoverage = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private CsePerson detailPolicyHoldr;
      private PersonalHealthInsurance detailAvailHinsPersonalHealthInsurance;
      private HealthInsuranceCoverage detailAvailHinsHealthInsuranceCoverage;
    }

    /// <summary>A CurrentOblgGroup group.</summary>
    [Serializable]
    public class CurrentOblgGroup
    {
      /// <summary>
      /// A value of DetailCurrentOblgObligationType.
      /// </summary>
      [JsonPropertyName("detailCurrentOblgObligationType")]
      public ObligationType DetailCurrentOblgObligationType
      {
        get => detailCurrentOblgObligationType ??= new();
        set => detailCurrentOblgObligationType = value;
      }

      /// <summary>
      /// A value of DetailCurrentOblgLegalActionDetail.
      /// </summary>
      [JsonPropertyName("detailCurrentOblgLegalActionDetail")]
      public LegalActionDetail DetailCurrentOblgLegalActionDetail
      {
        get => detailCurrentOblgLegalActionDetail ??= new();
        set => detailCurrentOblgLegalActionDetail = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 7;

      private ObligationType detailCurrentOblgObligationType;
      private LegalActionDetail detailCurrentOblgLegalActionDetail;
    }

    /// <summary>
    /// A value of ChildCount.
    /// </summary>
    [JsonPropertyName("childCount")]
    public Common ChildCount
    {
      get => childCount ??= new();
      set => childCount = value;
    }

    /// <summary>
    /// Gets a value of ActiveChild.
    /// </summary>
    [JsonIgnore]
    public Array<ActiveChildGroup> ActiveChild => activeChild ??= new(
      ActiveChildGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ActiveChild for json serialization.
    /// </summary>
    [JsonPropertyName("activeChild")]
    [Computed]
    public IList<ActiveChildGroup> ActiveChild_Json
    {
      get => activeChild;
      set => ActiveChild.Assign(value);
    }

    /// <summary>
    /// A value of ActiveChild1.
    /// </summary>
    [JsonPropertyName("activeChild1")]
    public Common ActiveChild1
    {
      get => activeChild1 ??= new();
      set => activeChild1 = value;
    }

    /// <summary>
    /// A value of MonthlyTotalIncome.
    /// </summary>
    [JsonPropertyName("monthlyTotalIncome")]
    public Common MonthlyTotalIncome
    {
      get => monthlyTotalIncome ??= new();
      set => monthlyTotalIncome = value;
    }

    /// <summary>
    /// Gets a value of HicvNote.
    /// </summary>
    [JsonIgnore]
    public Array<HicvNoteGroup> HicvNote => hicvNote ??= new(
      HicvNoteGroup.Capacity);

    /// <summary>
    /// Gets a value of HicvNote for json serialization.
    /// </summary>
    [JsonPropertyName("hicvNote")]
    [Computed]
    public IList<HicvNoteGroup> HicvNote_Json
    {
      get => hicvNote;
      set => HicvNote.Assign(value);
    }

    /// <summary>
    /// A value of HinsCovOblgLegalActionDetail.
    /// </summary>
    [JsonPropertyName("hinsCovOblgLegalActionDetail")]
    public LegalActionDetail HinsCovOblgLegalActionDetail
    {
      get => hinsCovOblgLegalActionDetail ??= new();
      set => hinsCovOblgLegalActionDetail = value;
    }

    /// <summary>
    /// A value of ChildrenNeedingCoverage.
    /// </summary>
    [JsonPropertyName("childrenNeedingCoverage")]
    public Common ChildrenNeedingCoverage
    {
      get => childrenNeedingCoverage ??= new();
      set => childrenNeedingCoverage = value;
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
    /// A value of ArCsePerson.
    /// </summary>
    [JsonPropertyName("arCsePerson")]
    public CsePerson ArCsePerson
    {
      get => arCsePerson ??= new();
      set => arCsePerson = value;
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
    /// A value of ChildCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("childCsePersonsWorkSet")]
    public CsePersonsWorkSet ChildCsePersonsWorkSet
    {
      get => childCsePersonsWorkSet ??= new();
      set => childCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of ChildCsePerson.
    /// </summary>
    [JsonPropertyName("childCsePerson")]
    public CsePerson ChildCsePerson
    {
      get => childCsePerson ??= new();
      set => childCsePerson = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CaseRole Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of HealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("healthInsuranceViability")]
    public HealthInsuranceViability HealthInsuranceViability
    {
      get => healthInsuranceViability ??= new();
      set => healthInsuranceViability = value;
    }

    /// <summary>
    /// A value of ScrollingAttributes.
    /// </summary>
    [JsonPropertyName("scrollingAttributes")]
    public ScrollingAttributes ScrollingAttributes
    {
      get => scrollingAttributes ??= new();
      set => scrollingAttributes = value;
    }

    /// <summary>
    /// A value of HinsCovOblgLegalAction.
    /// </summary>
    [JsonPropertyName("hinsCovOblgLegalAction")]
    public LegalAction HinsCovOblgLegalAction
    {
      get => hinsCovOblgLegalAction ??= new();
      set => hinsCovOblgLegalAction = value;
    }

    /// <summary>
    /// Gets a value of ChildsProgram.
    /// </summary>
    [JsonIgnore]
    public Array<ChildsProgramGroup> ChildsProgram => childsProgram ??= new(
      ChildsProgramGroup.Capacity);

    /// <summary>
    /// Gets a value of ChildsProgram for json serialization.
    /// </summary>
    [JsonPropertyName("childsProgram")]
    [Computed]
    public IList<ChildsProgramGroup> ChildsProgram_Json
    {
      get => childsProgram;
      set => ChildsProgram.Assign(value);
    }

    /// <summary>
    /// Gets a value of AvailHins.
    /// </summary>
    [JsonIgnore]
    public Array<AvailHinsGroup> AvailHins => availHins ??= new(
      AvailHinsGroup.Capacity);

    /// <summary>
    /// Gets a value of AvailHins for json serialization.
    /// </summary>
    [JsonPropertyName("availHins")]
    [Computed]
    public IList<AvailHinsGroup> AvailHins_Json
    {
      get => availHins;
      set => AvailHins.Assign(value);
    }

    /// <summary>
    /// Gets a value of CurrentOblg.
    /// </summary>
    [JsonIgnore]
    public Array<CurrentOblgGroup> CurrentOblg => currentOblg ??= new(
      CurrentOblgGroup.Capacity);

    /// <summary>
    /// Gets a value of CurrentOblg for json serialization.
    /// </summary>
    [JsonPropertyName("currentOblg")]
    [Computed]
    public IList<CurrentOblgGroup> CurrentOblg_Json
    {
      get => currentOblg;
      set => CurrentOblg.Assign(value);
    }

    /// <summary>
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
    }

    private Common childCount;
    private Array<ActiveChildGroup> activeChild;
    private Common activeChild1;
    private Common monthlyTotalIncome;
    private Array<HicvNoteGroup> hicvNote;
    private LegalActionDetail hinsCovOblgLegalActionDetail;
    private Common childrenNeedingCoverage;
    private CsePersonsWorkSet arCsePersonsWorkSet;
    private CsePerson arCsePerson;
    private CsePersonsWorkSet apCsePersonsWorkSet;
    private CsePerson apCsePerson;
    private Case1 case1;
    private CsePersonsWorkSet childCsePersonsWorkSet;
    private CsePerson childCsePerson;
    private CaseRole child;
    private HealthInsuranceViability healthInsuranceViability;
    private ScrollingAttributes scrollingAttributes;
    private LegalAction hinsCovOblgLegalAction;
    private Array<ChildsProgramGroup> childsProgram;
    private Array<AvailHinsGroup> availHins;
    private Array<CurrentOblgGroup> currentOblg;
    private Common caseOpen;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Null1.
    /// </summary>
    [JsonPropertyName("null1")]
    public DateWorkArea Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    /// <summary>
    /// A value of ApsIncome.
    /// </summary>
    [JsonPropertyName("apsIncome")]
    public Common ApsIncome
    {
      get => apsIncome ??= new();
      set => apsIncome = value;
    }

    /// <summary>
    /// A value of MultipleApExists.
    /// </summary>
    [JsonPropertyName("multipleApExists")]
    public Common MultipleApExists
    {
      get => multipleApExists ??= new();
      set => multipleApExists = value;
    }

    /// <summary>
    /// A value of ChildHasCoverage.
    /// </summary>
    [JsonPropertyName("childHasCoverage")]
    public Common ChildHasCoverage
    {
      get => childHasCoverage ??= new();
      set => childHasCoverage = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of ChildFound.
    /// </summary>
    [JsonPropertyName("childFound")]
    public Common ChildFound
    {
      get => childFound ??= new();
      set => childFound = value;
    }

    private DateWorkArea current;
    private DateWorkArea null1;
    private Common apsIncome;
    private Common multipleApExists;
    private Common childHasCoverage;
    private CsePerson child;
    private Common childFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExisitngMonetaryOblgObligor.
    /// </summary>
    [JsonPropertyName("exisitngMonetaryOblgObligor")]
    public LegalActionPerson ExisitngMonetaryOblgObligor
    {
      get => exisitngMonetaryOblgObligor ??= new();
      set => exisitngMonetaryOblgObligor = value;
    }

    /// <summary>
    /// A value of ExistingMonetaryOblgObligationType.
    /// </summary>
    [JsonPropertyName("existingMonetaryOblgObligationType")]
    public ObligationType ExistingMonetaryOblgObligationType
    {
      get => existingMonetaryOblgObligationType ??= new();
      set => existingMonetaryOblgObligationType = value;
    }

    /// <summary>
    /// A value of ExistingMonetaryOblgLegalAction.
    /// </summary>
    [JsonPropertyName("existingMonetaryOblgLegalAction")]
    public LegalAction ExistingMonetaryOblgLegalAction
    {
      get => existingMonetaryOblgLegalAction ??= new();
      set => existingMonetaryOblgLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingMonetaryOblgLegalActionDetail.
    /// </summary>
    [JsonPropertyName("existingMonetaryOblgLegalActionDetail")]
    public LegalActionDetail ExistingMonetaryOblgLegalActionDetail
    {
      get => existingMonetaryOblgLegalActionDetail ??= new();
      set => existingMonetaryOblgLegalActionDetail = value;
    }

    /// <summary>
    /// A value of ExistingHinsViabNote.
    /// </summary>
    [JsonPropertyName("existingHinsViabNote")]
    public HinsViabNote ExistingHinsViabNote
    {
      get => existingHinsViabNote ??= new();
      set => existingHinsViabNote = value;
    }

    /// <summary>
    /// A value of ExistingHinsCovOblgChild.
    /// </summary>
    [JsonPropertyName("existingHinsCovOblgChild")]
    public LegalActionPerson ExistingHinsCovOblgChild
    {
      get => existingHinsCovOblgChild ??= new();
      set => existingHinsCovOblgChild = value;
    }

    /// <summary>
    /// A value of ExistingHinsCovOblgLegalActionDetail.
    /// </summary>
    [JsonPropertyName("existingHinsCovOblgLegalActionDetail")]
    public LegalActionDetail ExistingHinsCovOblgLegalActionDetail
    {
      get => existingHinsCovOblgLegalActionDetail ??= new();
      set => existingHinsCovOblgLegalActionDetail = value;
    }

    /// <summary>
    /// A value of ExistingHinsCovOblgLegalAction.
    /// </summary>
    [JsonPropertyName("existingHinsCovOblgLegalAction")]
    public LegalAction ExistingHinsCovOblgLegalAction
    {
      get => existingHinsCovOblgLegalAction ??= new();
      set => existingHinsCovOblgLegalAction = value;
    }

    /// <summary>
    /// A value of ExistingNeedsCoverageChildCaseRole.
    /// </summary>
    [JsonPropertyName("existingNeedsCoverageChildCaseRole")]
    public CaseRole ExistingNeedsCoverageChildCaseRole
    {
      get => existingNeedsCoverageChildCaseRole ??= new();
      set => existingNeedsCoverageChildCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingNeedsCoverageChildPersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("existingNeedsCoverageChildPersonalHealthInsurance")]
    public PersonalHealthInsurance ExistingNeedsCoverageChildPersonalHealthInsurance
      
    {
      get => existingNeedsCoverageChildPersonalHealthInsurance ??= new();
      set => existingNeedsCoverageChildPersonalHealthInsurance = value;
    }

    /// <summary>
    /// A value of ExistingNeedsCoverageChildCsePerson.
    /// </summary>
    [JsonPropertyName("existingNeedsCoverageChildCsePerson")]
    public CsePerson ExistingNeedsCoverageChildCsePerson
    {
      get => existingNeedsCoverageChildCsePerson ??= new();
      set => existingNeedsCoverageChildCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingHinsPolicyHolder.
    /// </summary>
    [JsonPropertyName("existingHinsPolicyHolder")]
    public CsePerson ExistingHinsPolicyHolder
    {
      get => existingHinsPolicyHolder ??= new();
      set => existingHinsPolicyHolder = value;
    }

    /// <summary>
    /// A value of ExistingNextOrPrevChild.
    /// </summary>
    [JsonPropertyName("existingNextOrPrevChild")]
    public CsePerson ExistingNextOrPrevChild
    {
      get => existingNextOrPrevChild ??= new();
      set => existingNextOrPrevChild = value;
    }

    /// <summary>
    /// A value of ExistingNextOrPrev.
    /// </summary>
    [JsonPropertyName("existingNextOrPrev")]
    public CaseRole ExistingNextOrPrev
    {
      get => existingNextOrPrev ??= new();
      set => existingNextOrPrev = value;
    }

    /// <summary>
    /// A value of ExistingChildProgram.
    /// </summary>
    [JsonPropertyName("existingChildProgram")]
    public Program ExistingChildProgram
    {
      get => existingChildProgram ??= new();
      set => existingChildProgram = value;
    }

    /// <summary>
    /// A value of ExistingChildPersonProgram.
    /// </summary>
    [JsonPropertyName("existingChildPersonProgram")]
    public PersonProgram ExistingChildPersonProgram
    {
      get => existingChildPersonProgram ??= new();
      set => existingChildPersonProgram = value;
    }

    /// <summary>
    /// A value of ExistingHealthInsuranceViability.
    /// </summary>
    [JsonPropertyName("existingHealthInsuranceViability")]
    public HealthInsuranceViability ExistingHealthInsuranceViability
    {
      get => existingHealthInsuranceViability ??= new();
      set => existingHealthInsuranceViability = value;
    }

    /// <summary>
    /// A value of ExistingAps.
    /// </summary>
    [JsonPropertyName("existingAps")]
    public PersonIncomeHistory ExistingAps
    {
      get => existingAps ??= new();
      set => existingAps = value;
    }

    /// <summary>
    /// A value of ExistingApsIncome.
    /// </summary>
    [JsonPropertyName("existingApsIncome")]
    public IncomeSource ExistingApsIncome
    {
      get => existingApsIncome ??= new();
      set => existingApsIncome = value;
    }

    /// <summary>
    /// A value of ExistingAvailCovForChildHealthInsuranceCoverage.
    /// </summary>
    [JsonPropertyName("existingAvailCovForChildHealthInsuranceCoverage")]
    public HealthInsuranceCoverage ExistingAvailCovForChildHealthInsuranceCoverage
      
    {
      get => existingAvailCovForChildHealthInsuranceCoverage ??= new();
      set => existingAvailCovForChildHealthInsuranceCoverage = value;
    }

    /// <summary>
    /// A value of ExistingAvailCovForChildPersonalHealthInsurance.
    /// </summary>
    [JsonPropertyName("existingAvailCovForChildPersonalHealthInsurance")]
    public PersonalHealthInsurance ExistingAvailCovForChildPersonalHealthInsurance
      
    {
      get => existingAvailCovForChildPersonalHealthInsurance ??= new();
      set => existingAvailCovForChildPersonalHealthInsurance = value;
    }

    /// <summary>
    /// A value of ExistingChild.
    /// </summary>
    [JsonPropertyName("existingChild")]
    public CaseRole ExistingChild
    {
      get => existingChild ??= new();
      set => existingChild = value;
    }

    /// <summary>
    /// A value of ExistingChildCsePerson.
    /// </summary>
    [JsonPropertyName("existingChildCsePerson")]
    public CsePerson ExistingChildCsePerson
    {
      get => existingChildCsePerson ??= new();
      set => existingChildCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingAr.
    /// </summary>
    [JsonPropertyName("existingAr")]
    public CsePerson ExistingAr
    {
      get => existingAr ??= new();
      set => existingAr = value;
    }

    /// <summary>
    /// A value of ExistingApplicantRecipient.
    /// </summary>
    [JsonPropertyName("existingApplicantRecipient")]
    public CaseRole ExistingApplicantRecipient
    {
      get => existingApplicantRecipient ??= new();
      set => existingApplicantRecipient = value;
    }

    /// <summary>
    /// A value of ExistingAbsentParent.
    /// </summary>
    [JsonPropertyName("existingAbsentParent")]
    public CaseRole ExistingAbsentParent
    {
      get => existingAbsentParent ??= new();
      set => existingAbsentParent = value;
    }

    /// <summary>
    /// A value of ExistingAp.
    /// </summary>
    [JsonPropertyName("existingAp")]
    public CsePerson ExistingAp
    {
      get => existingAp ??= new();
      set => existingAp = value;
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

    private LegalActionPerson exisitngMonetaryOblgObligor;
    private ObligationType existingMonetaryOblgObligationType;
    private LegalAction existingMonetaryOblgLegalAction;
    private LegalActionDetail existingMonetaryOblgLegalActionDetail;
    private HinsViabNote existingHinsViabNote;
    private LegalActionPerson existingHinsCovOblgChild;
    private LegalActionDetail existingHinsCovOblgLegalActionDetail;
    private LegalAction existingHinsCovOblgLegalAction;
    private CaseRole existingNeedsCoverageChildCaseRole;
    private PersonalHealthInsurance existingNeedsCoverageChildPersonalHealthInsurance;
      
    private CsePerson existingNeedsCoverageChildCsePerson;
    private CsePerson existingHinsPolicyHolder;
    private CsePerson existingNextOrPrevChild;
    private CaseRole existingNextOrPrev;
    private Program existingChildProgram;
    private PersonProgram existingChildPersonProgram;
    private HealthInsuranceViability existingHealthInsuranceViability;
    private PersonIncomeHistory existingAps;
    private IncomeSource existingApsIncome;
    private HealthInsuranceCoverage existingAvailCovForChildHealthInsuranceCoverage;
      
    private PersonalHealthInsurance existingAvailCovForChildPersonalHealthInsurance;
      
    private CaseRole existingChild;
    private CsePerson existingChildCsePerson;
    private CsePerson existingAr;
    private CaseRole existingApplicantRecipient;
    private CaseRole existingAbsentParent;
    private CsePerson existingAp;
    private Case1 existingCase;
  }
#endregion
}

// Program: SP_CRPA_DISPLAY_CLOSED_CASE, ID: 372641397, model: 746.
// Short name: SWE02094
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
/// A program: SP_CRPA_DISPLAY_CLOSED_CASE.
/// </para>
/// <para>
/// RESP: SRVPLAN
/// </para>
/// </summary>
[Serializable]
public partial class SpCrpaDisplayClosedCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_CRPA_DISPLAY_CLOSED_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpCrpaDisplayClosedCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpCrpaDisplayClosedCase.
  /// </summary>
  public SpCrpaDisplayClosedCase(IContext context, Import import, Export export):
    
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
    //        M A I N T E N A N C E   L O G
    //  Date    Developer      Req #    	Description
    // 07/22/97 R. Grey	IDCR 357 	Initial Code
    // 03/23/99 Sury                           Get AP Details and
    //                                         
    // and Child age in months
    // 08/08/2000 SWSRCHF WR# 000170  Replace the read for Narrative by a read 
    // for
    //                                
    // Narrative Detail
    // 04/01/2011	T Pierce	CQ# 23212	Removed references to obsolete Narrative 
    // entity type.
    // ---------------------------------------------
    // 06/16/11  RMathews   CQ27977  Changed infrastructure read to use import 
    // id
    local.MaxDate.Date = UseCabSetMaximumDiscontinueDate();

    if (ReadCase())
    {
      local.CaseClosedDate.Date = entities.Closed.StatusDate;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    if (ReadCsePersonMother())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsEmpty(local.AbendData.Type1))
      {
        return;
      }

      MoveCsePersonsWorkSet2(local.CsePersonsWorkSet, export.Mo);
    }

    // ***************************************************************
    // * Display AR Details
    // ***************************************************************
    if (ReadCsePersonApplicantRecipient())
    {
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsEmpty(local.AbendData.Type1))
      {
        return;
      }

      MoveCsePersonsWorkSet3(local.CsePersonsWorkSet, export.Ar);
    }

    // ***************************************************************
    // * Determine more than more than one AP exist and Display
    // * AP Details
    // ***************************************************************
    local.Count.Count = 0;
    export.Ap.FormattedName = "";

    foreach(var item in ReadCsePersonAbsentParent())
    {
      ++local.Count.Count;
      local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
      UseSiReadCsePerson();

      if (!IsEmpty(local.AbendData.Type1))
      {
        return;
      }

      if (local.Count.Count > 0)
      {
        if (IsEmpty(export.Ap.FormattedName))
        {
          MoveCsePersonsWorkSet3(local.CsePersonsWorkSet, export.Ap);
        }
        else
        {
          export.MoreApsMsg.Text30 = "More AP's exist for this case";

          break;
        }
      }
    }

    export.ServiceProcess.ServiceDate = local.Initialized.Date;

    if (IsEmpty(export.MoreApsMsg.Text30))
    {
      if (ReadServiceProcessLegalActionLegalActionPerson2())
      {
        export.ServiceProcess.ServiceDate = entities.ServiceProcess.ServiceDate;
      }
    }
    else if (ReadServiceProcessLegalActionLegalActionPerson1())
    {
      export.ServiceProcess.ServiceDate = entities.ServiceProcess.ServiceDate;
    }

    if (ReadLegalReferral())
    {
      export.LegalReferral.ReferralDate = entities.LegalReferral.ReferralDate;
    }

    export.Child.Index = 0;
    export.Child.Clear();

    foreach(var item in ReadChild())
    {
      if (ReadCsePerson())
      {
        local.CsePersonsWorkSet.Number = entities.Child2.Number;
        UseSiReadCsePerson();

        if (!IsEmpty(local.AbendData.Type1))
        {
          export.Child.Next();

          return;
        }

        export.Child.Update.ChildCsePerson.Assign(entities.Child2);

        if (Equal(local.CsePersonsWorkSet.Dob, local.Initialized.Date) || Equal
          (local.CsePersonsWorkSet.Dob, local.MaxDate.Date))
        {
          export.Child.Update.ChildAge.Count = 0;
        }
        else
        {
          local.ChildAge.TotalInteger =
            (long)DaysFromAD(local.CaseClosedDate.Date) - DaysFromAD
            (local.CsePersonsWorkSet.Dob);

          if (local.ChildAge.TotalInteger < 365)
          {
            export.Child.Update.ChildAge.Count =
              (int)Math.Round(
                local.ChildAge.TotalInteger /
              30.6M, MidpointRounding.AwayFromZero);
            export.Child.Update.ChAgeMsgTxt.Text4 = "Mths";
          }
          else
          {
            UseCabCalcCurrentAgeFromDob();
            export.Child.Update.ChildAge.Count =
              (int)export.LocalReturnedAge.TotalInteger;
            export.Child.Update.ChAgeMsgTxt.Text4 = "Yrs";
          }
        }

        export.Child.Update.ChildCsePersonsWorkSet.Assign(
          local.CsePersonsWorkSet);
        export.ActivityLiteral.Text16 = "Required Cmpt Dt";

        if (ReadMonitoredActivity())
        {
          export.Child.Update.Compliance.Date =
            entities.MonitoredActivity.FedNonComplianceDate;
          export.Child.Update.StartDate.Date =
            entities.MonitoredActivity.StartDate;

          if (Lt(local.CaseClosedDate.Date,
            entities.MonitoredActivity.ClosureDate) || Equal
            (entities.MonitoredActivity.ClosureDate, local.MaxDate.Date))
          {
            // Indicates that the monitored activity has not been closed,
            // therefore AP has not been located.
            if (!Lt(local.CaseClosedDate.Date,
              entities.MonitoredActivity.FedNonComplianceDate))
            {
              export.Child.Update.FedCompliance.Flag = "N";
              export.ActivityLiteral.Text16 = "Required Cmpt Dt";
              export.Child.Update.DaysRemainInCompli.Count =
                DaysFromAD(entities.MonitoredActivity.FedNonComplianceDate) - DaysFromAD
                (local.CaseClosedDate.Date);
            }
            else if (Lt(local.CaseClosedDate.Date,
              entities.MonitoredActivity.FedNonComplianceDate))
            {
              export.Child.Update.FedCompliance.Flag = "";
              export.ActivityLiteral.Text16 = "Required Cmpt Dt";
              export.Child.Update.DaysRemainInCompli.Count = 0;
            }
          }
          else
          {
            // Indicates monitored activity has been closed, and other fields 
            // populated (ap address, etc.) indicate whether or not he's been
            // found.   Therefore, compliance indicator is set based on whether
            // the monitored activity was closed on or before compliance date.
            if (!Lt(entities.MonitoredActivity.FedNonComplianceDate,
              entities.MonitoredActivity.ClosureDate))
            {
              export.Child.Update.FedCompliance.Flag = "Y";
              export.ActivityLiteral.Text16 = " Activity Closed";
              export.Child.Update.Compliance.Date =
                entities.MonitoredActivity.ClosureDate;
            }
            else
            {
              export.Child.Update.FedCompliance.Flag = "N";
              export.ActivityLiteral.Text16 = " Activity Closed";
              export.Child.Update.Compliance.Date =
                entities.MonitoredActivity.ClosureDate;
            }

            export.Child.Update.DaysRemainInCompli.Count = 0;
          }
        }
      }
      else
      {
        ExitState = "CSE_PERSON_NF";
      }

      export.Child.Update.GenTestFlag.Flag = "";

      foreach(var item1 in ReadGeneticTest())
      {
        if (ReadMother())
        {
          export.Child.Update.GenTestFlag.Flag = "Y";

          if (ReadAbsentParent())
          {
            MoveGeneticTest(entities.GeneticTest,
              export.Child.Update.GeneticTest);

            break;
          }
          else if (ReadFather())
          {
            MoveGeneticTest(entities.GeneticTest,
              export.Child.Update.GeneticTest);

            break;
          }
        }
      }

      if (!Equal(export.Child.Item.GeneticTest.ActualTestDate, null) || !
        Equal(export.Child.Item.GeneticTest.TestResultReceivedDate, null))
      {
        export.Child.Update.GenTestFlag.Flag = "";
      }

      export.Child.Next();
    }

    if (Equal(import.HiddenPassedReviewType.ActionEntry, "R"))
    {
      // *** Work request 000170
      // *** 08/08/00 SWSRCHF
      // *** start
      local.Work.Index = -1;

      // CQ27977 Modified read to use import id instead of export id
      foreach(var item in ReadNarrativeDetail())
      {
        ++local.Work.Index;
        local.Work.CheckSize();

        local.Work.Update.Work1.NarrativeText =
          entities.PaternityReview.NarrativeText;

        if (local.Work.Index == 3)
        {
          break;
        }
      }

      if (!local.Work.IsEmpty)
      {
        local.Work.Index = 0;
        local.Work.CheckSize();

        export.PatReview.Text =
          Substring(local.Work.Item.Work1.NarrativeText, 14, 55);

        local.Work.Index = 1;
        local.Work.CheckSize();

        if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
        {
          export.PatReview.Text = TrimEnd(export.PatReview.Text) + Substring
            (local.Work.Item.Work1.NarrativeText, 68, 14, 55);
        }

        local.Work.Index = 2;
        local.Work.CheckSize();

        if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
        {
          export.PatReview.Text = TrimEnd(export.PatReview.Text) + Substring
            (local.Work.Item.Work1.NarrativeText, 68, 14, 55);
        }

        local.Work.Index = 3;
        local.Work.CheckSize();

        if (!IsEmpty(local.Work.Item.Work1.NarrativeText))
        {
          export.PatReview.Text = TrimEnd(export.PatReview.Text) + Substring
            (local.Work.Item.Work1.NarrativeText, 68, 14, 55);
        }
      }

      // *** end
      // *** 08/08/00 SWSRCHF
      // *** Work request 000170
    }

    if (IsEmpty(export.PatReview.Text))
    {
      ExitState = "SP0000_FIRST_REVIEW_4_CLOSD_CASE";

      return;
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      ExitState = "DISPLAY_OK_FOR_CLOSED_CASE";
    }
  }

  private static void MoveCsePersonsWorkSet1(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Sex = source.Sex;
    target.Dob = source.Dob;
    target.Ssn = source.Ssn;
    target.FirstName = source.FirstName;
    target.MiddleInitial = source.MiddleInitial;
    target.FormattedName = source.FormattedName;
    target.LastName = source.LastName;
  }

  private static void MoveCsePersonsWorkSet2(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.Dob = source.Dob;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveCsePersonsWorkSet3(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.Number = source.Number;
    target.FormattedName = source.FormattedName;
  }

  private static void MoveGeneticTest(GeneticTest source, GeneticTest target)
  {
    target.TestNumber = source.TestNumber;
    target.ActualTestDate = source.ActualTestDate;
    target.TestResultReceivedDate = source.TestResultReceivedDate;
    target.LastUpdatedBy = source.LastUpdatedBy;
  }

  private void UseCabCalcCurrentAgeFromDob()
  {
    var useImport = new CabCalcCurrentAgeFromDob.Import();
    var useExport = new CabCalcCurrentAgeFromDob.Export();

    useImport.CsePersonsWorkSet.Dob = local.CsePersonsWorkSet.Dob;

    Call(CabCalcCurrentAgeFromDob.Execute, useImport, useExport);

    export.LocalReturnedAge.TotalInteger = useExport.Common.TotalInteger;
  }

  private DateTime? UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    useImport.DateWorkArea.Date = local.MaxDate.Date;

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    return useExport.DateWorkArea.Date;
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    MoveCsePersonsWorkSet1(useExport.CsePersonsWorkSet, local.CsePersonsWorkSet);
      
    local.AbendData.Assign(useExport.AbendData);
  }

  private bool ReadAbsentParent()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.AllegedFather.Populated = false;

    return Read("ReadAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber1", entities.Closed.Number);
        db.SetInt32(
          command, "caseRoleId",
          entities.GeneticTest.CroAIdentifier.GetValueOrDefault());
        db.SetString(command, "type", entities.GeneticTest.CroAType ?? "");
        db.SetString(
          command, "casNumber2", entities.GeneticTest.CasANumber ?? "");
        db.
          SetString(command, "cspNumber", entities.GeneticTest.CspANumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.AllegedFather.CasNumber = db.GetString(reader, 0);
        entities.AllegedFather.CspNumber = db.GetString(reader, 1);
        entities.AllegedFather.Type1 = db.GetString(reader, 2);
        entities.AllegedFather.Identifier = db.GetInt32(reader, 3);
        entities.AllegedFather.EndDate = db.GetNullableDate(reader, 4);
        entities.AllegedFather.Populated = true;
        CheckValid<CaseRole>("Type1", entities.AllegedFather.Type1);
      });
  }

  private bool ReadCase()
  {
    entities.Closed.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.Closed.Number = db.GetString(reader, 0);
        entities.Closed.StatusDate = db.GetNullableDate(reader, 1);
        entities.Closed.Populated = true;
      });
  }

  private IEnumerable<bool> ReadChild()
  {
    return ReadEach("ReadChild",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Closed.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Child.IsFull)
        {
          return false;
        }

        entities.Child1.CasNumber = db.GetString(reader, 0);
        entities.Child1.CspNumber = db.GetString(reader, 1);
        entities.Child1.Type1 = db.GetString(reader, 2);
        entities.Child1.Identifier = db.GetInt32(reader, 3);
        entities.Child1.StartDate = db.GetNullableDate(reader, 4);
        entities.Child1.EndDate = db.GetNullableDate(reader, 5);
        entities.Child1.HealthInsuranceIndicator =
          db.GetNullableString(reader, 6);
        entities.Child1.MedicalSupportIndicator =
          db.GetNullableString(reader, 7);
        entities.Child1.ArWaivedInsurance = db.GetNullableString(reader, 8);
        entities.Child1.RelToAr = db.GetNullableString(reader, 9);
        entities.Child1.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Child1.Type1);

        return true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(entities.Child1.Populated);
    entities.Child2.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Child1.CspNumber);
      },
      (db, reader) =>
      {
        entities.Child2.Number = db.GetString(reader, 0);
        entities.Child2.Type1 = db.GetString(reader, 1);
        entities.Child2.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 2);
        entities.Child2.BirthCertFathersLastName =
          db.GetNullableString(reader, 3);
        entities.Child2.BirthCertFathersFirstName =
          db.GetNullableString(reader, 4);
        entities.Child2.BirthCertificateSignature =
          db.GetNullableString(reader, 5);
        entities.Child2.Populated = true;
        CheckValid<CsePerson>("Type1", entities.Child2.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePersonAbsentParent()
  {
    entities.CsePerson.Populated = false;
    entities.AbsentParent.Populated = false;

    return ReadEach("ReadCsePersonAbsentParent",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Closed.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.AbsentParent.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.AbsentParent.CasNumber = db.GetString(reader, 2);
        entities.AbsentParent.Type1 = db.GetString(reader, 3);
        entities.AbsentParent.Identifier = db.GetInt32(reader, 4);
        entities.AbsentParent.StartDate = db.GetNullableDate(reader, 5);
        entities.AbsentParent.EndDate = db.GetNullableDate(reader, 6);
        entities.AbsentParent.CreatedTimestamp = db.GetDateTime(reader, 7);
        entities.CsePerson.Populated = true;
        entities.AbsentParent.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.AbsentParent.Type1);

        return true;
      });
  }

  private bool ReadCsePersonApplicantRecipient()
  {
    entities.CsePerson.Populated = false;
    entities.ApplicantRecipient.Populated = false;

    return Read("ReadCsePersonApplicantRecipient",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Closed.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.ApplicantRecipient.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.ApplicantRecipient.CasNumber = db.GetString(reader, 2);
        entities.ApplicantRecipient.Type1 = db.GetString(reader, 3);
        entities.ApplicantRecipient.Identifier = db.GetInt32(reader, 4);
        entities.ApplicantRecipient.StartDate = db.GetNullableDate(reader, 5);
        entities.ApplicantRecipient.EndDate = db.GetNullableDate(reader, 6);
        entities.ApplicantRecipient.AssignmentDate =
          db.GetNullableDate(reader, 7);
        entities.CsePerson.Populated = true;
        entities.ApplicantRecipient.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.ApplicantRecipient.Type1);
      });
  }

  private bool ReadCsePersonMother()
  {
    entities.CsePerson.Populated = false;
    entities.Mother.Populated = false;

    return Read("ReadCsePersonMother",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Closed.Number);
        db.SetNullableDate(
          command, "startDate", local.CaseClosedDate.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.Mother.CspNumber = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.Mother.CasNumber = db.GetString(reader, 2);
        entities.Mother.Type1 = db.GetString(reader, 3);
        entities.Mother.Identifier = db.GetInt32(reader, 4);
        entities.Mother.StartDate = db.GetNullableDate(reader, 5);
        entities.Mother.EndDate = db.GetNullableDate(reader, 6);
        entities.CsePerson.Populated = true;
        entities.Mother.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.Mother.Type1);
      });
  }

  private bool ReadFather()
  {
    entities.Father.Populated = false;

    return Read("ReadFather",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Closed.Number);
      },
      (db, reader) =>
      {
        entities.Father.CasNumber = db.GetString(reader, 0);
        entities.Father.CspNumber = db.GetString(reader, 1);
        entities.Father.Type1 = db.GetString(reader, 2);
        entities.Father.Identifier = db.GetInt32(reader, 3);
        entities.Father.EndDate = db.GetNullableDate(reader, 4);
        entities.Father.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Father.Type1);
      });
  }

  private IEnumerable<bool> ReadGeneticTest()
  {
    System.Diagnostics.Debug.Assert(entities.Child1.Populated);
    entities.GeneticTest.Populated = false;

    return ReadEach("ReadGeneticTest",
      (db, command) =>
      {
        db.
          SetNullableInt32(command, "croIdentifier", entities.Child1.Identifier);
          
        db.SetNullableString(command, "croType", entities.Child1.Type1);
        db.SetNullableString(command, "casNumber", entities.Child1.CasNumber);
        db.SetNullableString(command, "cspNumber", entities.Child1.CspNumber);
      },
      (db, reader) =>
      {
        entities.GeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.GeneticTest.ActualTestDate = db.GetNullableDate(reader, 1);
        entities.GeneticTest.TestResultReceivedDate =
          db.GetNullableDate(reader, 2);
        entities.GeneticTest.CreatedTimestamp = db.GetDateTime(reader, 3);
        entities.GeneticTest.LastUpdatedBy = db.GetString(reader, 4);
        entities.GeneticTest.LastUpdatedTimestamp = db.GetDateTime(reader, 5);
        entities.GeneticTest.CasNumber = db.GetNullableString(reader, 6);
        entities.GeneticTest.CspNumber = db.GetNullableString(reader, 7);
        entities.GeneticTest.CroType = db.GetNullableString(reader, 8);
        entities.GeneticTest.CroIdentifier = db.GetNullableInt32(reader, 9);
        entities.GeneticTest.CasMNumber = db.GetNullableString(reader, 10);
        entities.GeneticTest.CspMNumber = db.GetNullableString(reader, 11);
        entities.GeneticTest.CroMType = db.GetNullableString(reader, 12);
        entities.GeneticTest.CroMIdentifier = db.GetNullableInt32(reader, 13);
        entities.GeneticTest.CasANumber = db.GetNullableString(reader, 14);
        entities.GeneticTest.CspANumber = db.GetNullableString(reader, 15);
        entities.GeneticTest.CroAType = db.GetNullableString(reader, 16);
        entities.GeneticTest.CroAIdentifier = db.GetNullableInt32(reader, 17);
        entities.GeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.GeneticTest.CroType);
        CheckValid<GeneticTest>("CroMType", entities.GeneticTest.CroMType);
        CheckValid<GeneticTest>("CroAType", entities.GeneticTest.CroAType);

        return true;
      });
  }

  private bool ReadLegalReferral()
  {
    entities.LegalReferral.Populated = false;

    return Read("ReadLegalReferral",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Closed.Number);
      },
      (db, reader) =>
      {
        entities.LegalReferral.CasNumber = db.GetString(reader, 0);
        entities.LegalReferral.Identifier = db.GetInt32(reader, 1);
        entities.LegalReferral.ReferralDate = db.GetDate(reader, 2);
        entities.LegalReferral.ReferralReason1 = db.GetString(reader, 3);
        entities.LegalReferral.ReferralReason2 = db.GetString(reader, 4);
        entities.LegalReferral.ReferralReason3 = db.GetString(reader, 5);
        entities.LegalReferral.ReferralReason4 = db.GetString(reader, 6);
        entities.LegalReferral.ReferralReason5 = db.GetString(reader, 7);
        entities.LegalReferral.Populated = true;
      });
  }

  private bool ReadMonitoredActivity()
  {
    entities.MonitoredActivity.Populated = false;

    return Read("ReadMonitoredActivity",
      (db, command) =>
      {
        db.SetNullableString(command, "caseNumber", import.Case1.Number);
        db.SetNullableString(command, "csePersonNum", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.MonitoredActivity.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.MonitoredActivity.ActivityControlNumber =
          db.GetInt32(reader, 1);
        entities.MonitoredActivity.FedNonComplianceDate =
          db.GetNullableDate(reader, 2);
        entities.MonitoredActivity.StartDate = db.GetDate(reader, 3);
        entities.MonitoredActivity.ClosureDate = db.GetNullableDate(reader, 4);
        entities.MonitoredActivity.CreatedTimestamp = db.GetDateTime(reader, 5);
        entities.MonitoredActivity.InfSysGenId = db.GetNullableInt32(reader, 6);
        entities.MonitoredActivity.Populated = true;
      });
  }

  private bool ReadMother()
  {
    System.Diagnostics.Debug.Assert(entities.GeneticTest.Populated);
    entities.Mother.Populated = false;

    return Read("ReadMother",
      (db, command) =>
      {
        db.SetString(command, "casNumber1", entities.Closed.Number);
        db.SetInt32(
          command, "caseRoleId",
          entities.GeneticTest.CroMIdentifier.GetValueOrDefault());
        db.SetString(command, "type", entities.GeneticTest.CroMType ?? "");
        db.SetString(
          command, "casNumber2", entities.GeneticTest.CasMNumber ?? "");
        db.
          SetString(command, "cspNumber", entities.GeneticTest.CspMNumber ?? "");
          
      },
      (db, reader) =>
      {
        entities.Mother.CasNumber = db.GetString(reader, 0);
        entities.Mother.CspNumber = db.GetString(reader, 1);
        entities.Mother.Type1 = db.GetString(reader, 2);
        entities.Mother.Identifier = db.GetInt32(reader, 3);
        entities.Mother.StartDate = db.GetNullableDate(reader, 4);
        entities.Mother.EndDate = db.GetNullableDate(reader, 5);
        entities.Mother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Mother.Type1);
      });
  }

  private IEnumerable<bool> ReadNarrativeDetail()
  {
    entities.PaternityReview.Populated = false;

    return ReadEach("ReadNarrativeDetail",
      (db, command) =>
      {
        db.SetInt32(
          command, "infrastructureId",
          import.HiddenPass1.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.PaternityReview.InfrastructureId = db.GetInt32(reader, 0);
        entities.PaternityReview.CreatedTimestamp = db.GetDateTime(reader, 1);
        entities.PaternityReview.NarrativeText =
          db.GetNullableString(reader, 2);
        entities.PaternityReview.LineNumber = db.GetInt32(reader, 3);
        entities.PaternityReview.Populated = true;

        return true;
      });
  }

  private bool ReadServiceProcessLegalActionLegalActionPerson1()
  {
    entities.LegalActionPerson.Populated = false;
    entities.ServiceProcess.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadServiceProcessLegalActionLegalActionPerson1",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 1);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 2);
        entities.LegalAction.Classification = db.GetString(reader, 3);
        entities.LegalAction.ActionTaken = db.GetString(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 8);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionPerson.Role = db.GetString(reader, 10);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 11);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 12);
        entities.LegalActionPerson.Populated = true;
        entities.ServiceProcess.Populated = true;
        entities.LegalAction.Populated = true;
      });
  }

  private bool ReadServiceProcessLegalActionLegalActionPerson2()
  {
    entities.LegalActionPerson.Populated = false;
    entities.ServiceProcess.Populated = false;
    entities.LegalAction.Populated = false;

    return Read("ReadServiceProcessLegalActionLegalActionPerson2",
      (db, command) =>
      {
        db.SetNullableString(command, "cspNumber", export.Ap.Number);
      },
      (db, reader) =>
      {
        entities.ServiceProcess.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalActionPerson.LgaIdentifier =
          db.GetNullableInt32(reader, 0);
        entities.ServiceProcess.ServiceDate = db.GetDate(reader, 1);
        entities.ServiceProcess.Identifier = db.GetInt32(reader, 2);
        entities.LegalAction.Classification = db.GetString(reader, 3);
        entities.LegalAction.ActionTaken = db.GetString(reader, 4);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 5);
        entities.LegalAction.CreatedTstamp = db.GetDateTime(reader, 6);
        entities.LegalActionPerson.Identifier = db.GetInt32(reader, 7);
        entities.LegalActionPerson.CspNumber = db.GetNullableString(reader, 8);
        entities.LegalActionPerson.EffectiveDate = db.GetDate(reader, 9);
        entities.LegalActionPerson.Role = db.GetString(reader, 10);
        entities.LegalActionPerson.EndDate = db.GetNullableDate(reader, 11);
        entities.LegalActionPerson.AccountType =
          db.GetNullableString(reader, 12);
        entities.LegalActionPerson.Populated = true;
        entities.ServiceProcess.Populated = true;
        entities.LegalAction.Populated = true;
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
    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of StartDate.
      /// </summary>
      [JsonPropertyName("startDate")]
      public DateWorkArea StartDate
      {
        get => startDate ??= new();
        set => startDate = value;
      }

      /// <summary>
      /// A value of Compliance.
      /// </summary>
      [JsonPropertyName("compliance")]
      public DateWorkArea Compliance
      {
        get => compliance ??= new();
        set => compliance = value;
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
      /// A value of Child1.
      /// </summary>
      [JsonPropertyName("child1")]
      public CaseRole Child1
      {
        get => child1 ??= new();
        set => child1 = value;
      }

      /// <summary>
      /// A value of GeneticTest.
      /// </summary>
      [JsonPropertyName("geneticTest")]
      public GeneticTest GeneticTest
      {
        get => geneticTest ??= new();
        set => geneticTest = value;
      }

      /// <summary>
      /// A value of LegalReferral.
      /// </summary>
      [JsonPropertyName("legalReferral")]
      public LegalReferral LegalReferral
      {
        get => legalReferral ??= new();
        set => legalReferral = value;
      }

      /// <summary>
      /// A value of ReferredToLegal.
      /// </summary>
      [JsonPropertyName("referredToLegal")]
      public Common ReferredToLegal
      {
        get => referredToLegal ??= new();
        set => referredToLegal = value;
      }

      /// <summary>
      /// A value of FedCompliance.
      /// </summary>
      [JsonPropertyName("fedCompliance")]
      public Common FedCompliance
      {
        get => fedCompliance ??= new();
        set => fedCompliance = value;
      }

      /// <summary>
      /// A value of ChildAge.
      /// </summary>
      [JsonPropertyName("childAge")]
      public Common ChildAge
      {
        get => childAge ??= new();
        set => childAge = value;
      }

      /// <summary>
      /// A value of DaysRemainInCompli.
      /// </summary>
      [JsonPropertyName("daysRemainInCompli")]
      public Common DaysRemainInCompli
      {
        get => daysRemainInCompli ??= new();
        set => daysRemainInCompli = value;
      }

      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CaseRole Ap
      {
        get => ap ??= new();
        set => ap = value;
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
      /// A value of ChildCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("childCsePersonsWorkSet")]
      public CsePersonsWorkSet ChildCsePersonsWorkSet
      {
        get => childCsePersonsWorkSet ??= new();
        set => childCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ChAgeMsgTxt.
      /// </summary>
      [JsonPropertyName("chAgeMsgTxt")]
      public SpTextWorkArea ChAgeMsgTxt
      {
        get => chAgeMsgTxt ??= new();
        set => chAgeMsgTxt = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private DateWorkArea startDate;
      private DateWorkArea compliance;
      private Common common;
      private CaseRole child1;
      private GeneticTest geneticTest;
      private LegalReferral legalReferral;
      private Common referredToLegal;
      private Common fedCompliance;
      private Common childAge;
      private Common daysRemainInCompli;
      private CaseRole ap;
      private CaseRole childCaseRole;
      private CsePersonsWorkSet childCsePersonsWorkSet;
      private SpTextWorkArea chAgeMsgTxt;
      private CsePerson childCsePerson;
    }

    /// <summary>
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity);

    /// <summary>
    /// Gets a value of Child for json serialization.
    /// </summary>
    [JsonPropertyName("child")]
    [Computed]
    public IList<ChildGroup> Child_Json
    {
      get => child;
      set => Child.Assign(value);
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
    /// A value of HiddenPassedReviewType.
    /// </summary>
    [JsonPropertyName("hiddenPassedReviewType")]
    public Common HiddenPassedReviewType
    {
      get => hiddenPassedReviewType ??= new();
      set => hiddenPassedReviewType = value;
    }

    /// <summary>
    /// A value of HiddenPass1.
    /// </summary>
    [JsonPropertyName("hiddenPass1")]
    public Infrastructure HiddenPass1
    {
      get => hiddenPass1 ??= new();
      set => hiddenPass1 = value;
    }

    /// <summary>
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
    }

    private Array<ChildGroup> child;
    private Case1 case1;
    private Common hiddenPassedReviewType;
    private Infrastructure hiddenPass1;
    private Common caseClosedIndicator;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ChildGroup group.</summary>
    [Serializable]
    public class ChildGroup
    {
      /// <summary>
      /// A value of Compliance.
      /// </summary>
      [JsonPropertyName("compliance")]
      public DateWorkArea Compliance
      {
        get => compliance ??= new();
        set => compliance = value;
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
      /// A value of Child1.
      /// </summary>
      [JsonPropertyName("child1")]
      public CaseRole Child1
      {
        get => child1 ??= new();
        set => child1 = value;
      }

      /// <summary>
      /// A value of GeneticTest.
      /// </summary>
      [JsonPropertyName("geneticTest")]
      public GeneticTest GeneticTest
      {
        get => geneticTest ??= new();
        set => geneticTest = value;
      }

      /// <summary>
      /// A value of LegalReferral.
      /// </summary>
      [JsonPropertyName("legalReferral")]
      public LegalReferral LegalReferral
      {
        get => legalReferral ??= new();
        set => legalReferral = value;
      }

      /// <summary>
      /// A value of StartDate.
      /// </summary>
      [JsonPropertyName("startDate")]
      public DateWorkArea StartDate
      {
        get => startDate ??= new();
        set => startDate = value;
      }

      /// <summary>
      /// A value of ReferredToLegal.
      /// </summary>
      [JsonPropertyName("referredToLegal")]
      public Common ReferredToLegal
      {
        get => referredToLegal ??= new();
        set => referredToLegal = value;
      }

      /// <summary>
      /// A value of FedCompliance.
      /// </summary>
      [JsonPropertyName("fedCompliance")]
      public Common FedCompliance
      {
        get => fedCompliance ??= new();
        set => fedCompliance = value;
      }

      /// <summary>
      /// A value of ChildAge.
      /// </summary>
      [JsonPropertyName("childAge")]
      public Common ChildAge
      {
        get => childAge ??= new();
        set => childAge = value;
      }

      /// <summary>
      /// A value of DaysRemainInCompli.
      /// </summary>
      [JsonPropertyName("daysRemainInCompli")]
      public Common DaysRemainInCompli
      {
        get => daysRemainInCompli ??= new();
        set => daysRemainInCompli = value;
      }

      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public CaseRole Ap
      {
        get => ap ??= new();
        set => ap = value;
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
      /// A value of ChildCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("childCsePersonsWorkSet")]
      public CsePersonsWorkSet ChildCsePersonsWorkSet
      {
        get => childCsePersonsWorkSet ??= new();
        set => childCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of ChAgeMsgTxt.
      /// </summary>
      [JsonPropertyName("chAgeMsgTxt")]
      public SpTextWorkArea ChAgeMsgTxt
      {
        get => chAgeMsgTxt ??= new();
        set => chAgeMsgTxt = value;
      }

      /// <summary>
      /// A value of GenTestFlag.
      /// </summary>
      [JsonPropertyName("genTestFlag")]
      public Common GenTestFlag
      {
        get => genTestFlag ??= new();
        set => genTestFlag = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 15;

      private DateWorkArea compliance;
      private Common common;
      private CaseRole child1;
      private GeneticTest geneticTest;
      private LegalReferral legalReferral;
      private DateWorkArea startDate;
      private Common referredToLegal;
      private Common fedCompliance;
      private Common childAge;
      private Common daysRemainInCompli;
      private CaseRole ap;
      private CaseRole childCaseRole;
      private CsePersonsWorkSet childCsePersonsWorkSet;
      private SpTextWorkArea chAgeMsgTxt;
      private Common genTestFlag;
      private CsePerson childCsePerson;
    }

    /// <summary>A HiddenPassGroup group.</summary>
    [Serializable]
    public class HiddenPassGroup
    {
      /// <summary>
      /// A value of GexportH.
      /// </summary>
      [JsonPropertyName("gexportH")]
      public NarrativeWork GexportH
      {
        get => gexportH ??= new();
        set => gexportH = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 5;

      private NarrativeWork gexportH;
    }

    /// <summary>
    /// Gets a value of Child.
    /// </summary>
    [JsonIgnore]
    public Array<ChildGroup> Child => child ??= new(ChildGroup.Capacity);

    /// <summary>
    /// Gets a value of Child for json serialization.
    /// </summary>
    [JsonPropertyName("child")]
    [Computed]
    public IList<ChildGroup> Child_Json
    {
      get => child;
      set => Child.Assign(value);
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
    /// A value of Mo.
    /// </summary>
    [JsonPropertyName("mo")]
    public CsePersonsWorkSet Mo
    {
      get => mo ??= new();
      set => mo = value;
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
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of LocalReturnedAge.
    /// </summary>
    [JsonPropertyName("localReturnedAge")]
    public Common LocalReturnedAge
    {
      get => localReturnedAge ??= new();
      set => localReturnedAge = value;
    }

    /// <summary>
    /// A value of PatReview.
    /// </summary>
    [JsonPropertyName("patReview")]
    public NarrativeWork PatReview
    {
      get => patReview ??= new();
      set => patReview = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
    }

    /// <summary>
    /// Gets a value of HiddenPass.
    /// </summary>
    [JsonIgnore]
    public Array<HiddenPassGroup> HiddenPass => hiddenPass ??= new(
      HiddenPassGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of HiddenPass for json serialization.
    /// </summary>
    [JsonPropertyName("hiddenPass")]
    [Computed]
    public IList<HiddenPassGroup> HiddenPass_Json
    {
      get => hiddenPass;
      set => HiddenPass.Assign(value);
    }

    /// <summary>
    /// A value of CaseClosedIndicator.
    /// </summary>
    [JsonPropertyName("caseClosedIndicator")]
    public Common CaseClosedIndicator
    {
      get => caseClosedIndicator ??= new();
      set => caseClosedIndicator = value;
    }

    /// <summary>
    /// A value of HiddenPassedReviewType.
    /// </summary>
    [JsonPropertyName("hiddenPassedReviewType")]
    public Common HiddenPassedReviewType
    {
      get => hiddenPassedReviewType ??= new();
      set => hiddenPassedReviewType = value;
    }

    /// <summary>
    /// A value of HiddenPass1.
    /// </summary>
    [JsonPropertyName("hiddenPass1")]
    public Infrastructure HiddenPass1
    {
      get => hiddenPass1 ??= new();
      set => hiddenPass1 = value;
    }

    /// <summary>
    /// A value of ActivityLiteral.
    /// </summary>
    [JsonPropertyName("activityLiteral")]
    public WorkArea ActivityLiteral
    {
      get => activityLiteral ??= new();
      set => activityLiteral = value;
    }

    /// <summary>
    /// A value of MoreApsMsg.
    /// </summary>
    [JsonPropertyName("moreApsMsg")]
    public TextWorkArea MoreApsMsg
    {
      get => moreApsMsg ??= new();
      set => moreApsMsg = value;
    }

    /// <summary>
    /// A value of HiddenPassed.
    /// </summary>
    [JsonPropertyName("hiddenPassed")]
    public Infrastructure HiddenPassed
    {
      get => hiddenPassed ??= new();
      set => hiddenPassed = value;
    }

    private Array<ChildGroup> child;
    private Case1 case1;
    private CsePersonsWorkSet mo;
    private CsePersonsWorkSet ar;
    private CsePersonsWorkSet ap;
    private LegalReferral legalReferral;
    private Common localReturnedAge;
    private NarrativeWork patReview;
    private ServiceProcess serviceProcess;
    private Array<HiddenPassGroup> hiddenPass;
    private Common caseClosedIndicator;
    private Common hiddenPassedReviewType;
    private Infrastructure hiddenPass1;
    private WorkArea activityLiteral;
    private TextWorkArea moreApsMsg;
    private Infrastructure hiddenPassed;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A WorkGroup group.</summary>
    [Serializable]
    public class WorkGroup
    {
      /// <summary>
      /// A value of Work1.
      /// </summary>
      [JsonPropertyName("work1")]
      public NarrativeDetail Work1
      {
        get => work1 ??= new();
        set => work1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private NarrativeDetail work1;
    }

    /// <summary>
    /// Gets a value of Work.
    /// </summary>
    [JsonIgnore]
    public Array<WorkGroup> Work => work ??= new(WorkGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Work for json serialization.
    /// </summary>
    [JsonPropertyName("work")]
    [Computed]
    public IList<WorkGroup> Work_Json
    {
      get => work;
      set => Work.Assign(value);
    }

    /// <summary>
    /// A value of ChildAge.
    /// </summary>
    [JsonPropertyName("childAge")]
    public Common ChildAge
    {
      get => childAge ??= new();
      set => childAge = value;
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

    /// <summary>
    /// A value of CaseClosedDate.
    /// </summary>
    [JsonPropertyName("caseClosedDate")]
    public DateWorkArea CaseClosedDate
    {
      get => caseClosedDate ??= new();
      set => caseClosedDate = value;
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
    /// A value of AbendData.
    /// </summary>
    [JsonPropertyName("abendData")]
    public AbendData AbendData
    {
      get => abendData ??= new();
      set => abendData = value;
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
    /// A value of Initialized.
    /// </summary>
    [JsonPropertyName("initialized")]
    public DateWorkArea Initialized
    {
      get => initialized ??= new();
      set => initialized = value;
    }

    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    private Array<WorkGroup> work;
    private Common childAge;
    private DateWorkArea maxDate;
    private DateWorkArea caseClosedDate;
    private CsePersonsWorkSet csePersonsWorkSet;
    private AbendData abendData;
    private DateWorkArea current;
    private DateWorkArea initialized;
    private Common count;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaternityReview.
    /// </summary>
    [JsonPropertyName("paternityReview")]
    public NarrativeDetail PaternityReview
    {
      get => paternityReview ??= new();
      set => paternityReview = value;
    }

    /// <summary>
    /// A value of LegalActionPerson.
    /// </summary>
    [JsonPropertyName("legalActionPerson")]
    public LegalActionPerson LegalActionPerson
    {
      get => legalActionPerson ??= new();
      set => legalActionPerson = value;
    }

    /// <summary>
    /// A value of ServiceProcess.
    /// </summary>
    [JsonPropertyName("serviceProcess")]
    public ServiceProcess ServiceProcess
    {
      get => serviceProcess ??= new();
      set => serviceProcess = value;
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
    /// A value of Closed.
    /// </summary>
    [JsonPropertyName("closed")]
    public Case1 Closed
    {
      get => closed ??= new();
      set => closed = value;
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
    /// A value of ApplicantRecipient.
    /// </summary>
    [JsonPropertyName("applicantRecipient")]
    public CaseRole ApplicantRecipient
    {
      get => applicantRecipient ??= new();
      set => applicantRecipient = value;
    }

    /// <summary>
    /// A value of LegalReferral.
    /// </summary>
    [JsonPropertyName("legalReferral")]
    public LegalReferral LegalReferral
    {
      get => legalReferral ??= new();
      set => legalReferral = value;
    }

    /// <summary>
    /// A value of Child1.
    /// </summary>
    [JsonPropertyName("child1")]
    public CaseRole Child1
    {
      get => child1 ??= new();
      set => child1 = value;
    }

    /// <summary>
    /// A value of MonitoredActivity.
    /// </summary>
    [JsonPropertyName("monitoredActivity")]
    public MonitoredActivity MonitoredActivity
    {
      get => monitoredActivity ??= new();
      set => monitoredActivity = value;
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
    /// A value of Mother.
    /// </summary>
    [JsonPropertyName("mother")]
    public CaseRole Mother
    {
      get => mother ??= new();
      set => mother = value;
    }

    /// <summary>
    /// A value of AllegedFather.
    /// </summary>
    [JsonPropertyName("allegedFather")]
    public CaseRole AllegedFather
    {
      get => allegedFather ??= new();
      set => allegedFather = value;
    }

    /// <summary>
    /// A value of Father.
    /// </summary>
    [JsonPropertyName("father")]
    public CaseRole Father
    {
      get => father ??= new();
      set => father = value;
    }

    /// <summary>
    /// A value of GeneticTest.
    /// </summary>
    [JsonPropertyName("geneticTest")]
    public GeneticTest GeneticTest
    {
      get => geneticTest ??= new();
      set => geneticTest = value;
    }

    /// <summary>
    /// A value of AbsentParent.
    /// </summary>
    [JsonPropertyName("absentParent")]
    public CaseRole AbsentParent
    {
      get => absentParent ??= new();
      set => absentParent = value;
    }

    /// <summary>
    /// A value of Child2.
    /// </summary>
    [JsonPropertyName("child2")]
    public CsePerson Child2
    {
      get => child2 ??= new();
      set => child2 = value;
    }

    private NarrativeDetail paternityReview;
    private LegalActionPerson legalActionPerson;
    private ServiceProcess serviceProcess;
    private LegalAction legalAction;
    private Case1 closed;
    private Case1 case1;
    private CsePerson csePerson;
    private CaseRole applicantRecipient;
    private LegalReferral legalReferral;
    private CaseRole child1;
    private MonitoredActivity monitoredActivity;
    private Infrastructure infrastructure;
    private CaseRole mother;
    private CaseRole allegedFather;
    private CaseRole father;
    private GeneticTest geneticTest;
    private CaseRole absentParent;
    private CsePerson child2;
  }
#endregion
}

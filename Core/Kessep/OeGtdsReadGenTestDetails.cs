// Program: OE_GTDS_READ_GEN_TEST_DETAILS, ID: 371793021, model: 746.
// Short name: SWE00915
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
/// A program: OE_GTDS_READ_GEN_TEST_DETAILS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This common action block reads and populates the views of genetic test 
/// entities for display of genetic test details for a given child.
/// </para>
/// </summary>
[Serializable]
public partial class OeGtdsReadGenTestDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTDS_READ_GEN_TEST_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtdsReadGenTestDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtdsReadGenTestDetails.
  /// </summary>
  public OeGtdsReadGenTestDetails(IContext context, Import import, Export export)
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
    // *********************************************
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block reads and populates export views for display of 
    // paternity establishment details for a child.
    // PROCESSING:
    // It is passed with case number and a child cse person number. It reads and
    // populates genetic test details for the child and the child's alleged
    // parents.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	GENETIC_TEST		- R - -
    // 	PERSON_GENETIC_TEST	- R - -
    // DATABASE FILES USED:
    // CREATED BY:	govindaraj
    // DATE CREATED:	02/20/95
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR		DATE		CHGREQ		DESCRIPTION
    // govind		02/20/95			Initial coding
    // Ty Hill-MTW     04/29/97                        Change Current_date
    // *********************************************
    local.Current.Date = Now().Date;
    export.ChildCsePerson.Number = import.Child.Number;
    export.Case1.Number = import.Case1.Number;

    if (ReadCsePerson1())
    {
      export.ChildCsePerson.Number = entities.ExistingChildCsePerson.Number;
      UseCabGetClientDetails1();
    }
    else
    {
      ExitState = "OE0056_NF_CHILD_CSE_PERSON";

      return;
    }

    if (ReadChild())
    {
      if (ReadCase())
      {
        export.Case1.Number = entities.ExistingCase.Number;
      }
    }
    else
    {
      ExitState = "OE0065_NF_CASE_ROLE_CHILD";

      return;
    }

    export.GenTestDetails.Index = 0;
    export.GenTestDetails.Clear();

    foreach(var item in ReadGeneticTest())
    {
      MoveGeneticTest(entities.ExistingGeneticTest,
        export.GenTestDetails.Update.DetailFatherGeneticTest);

      if (ReadLegalAction())
      {
        export.GenTestDetails.Update.DetailFatherLegalAction.CourtCaseNumber =
          entities.ExistingPaternityEstablishmt.CourtCaseNumber;
      }
      else
      {
        // ---------------------------------------------
        // Not an error.
        // ---------------------------------------------
      }

      if (!ReadAbsentParent())
      {
        ExitState = "OE0138_NF_CASE_ROLE_AP";
        export.GenTestDetails.Next();

        return;
      }

      if (!ReadMother())
      {
        ExitState = "OE0055_NF_CASE_ROLE_MOTHER";
        export.GenTestDetails.Next();

        return;
      }

      if (ReadCsePerson2())
      {
        export.GenTestDetails.Update.DetailFatherCsePerson.Number =
          entities.ExistingFatherCsePerson.Number;
        UseCabGetClientDetails3();
      }
      else
      {
        ExitState = "OE0059_NF_FATHER_CSE_PERSON";
        export.GenTestDetails.Next();

        return;
      }

      if (ReadCsePerson3())
      {
        export.GenTestDetails.Update.DetailMotherCsePerson.Number =
          entities.ExistingMotherCsePerson.Number;
        UseCabGetClientDetails2();
      }
      else
      {
        ExitState = "OE0062_NF_MOTHER_CSE_PERSON";
        export.GenTestDetails.Next();

        return;
      }

      // ---------------------------------------------
      // Read the latest PERSON_GENETIC_TEST for father, mother and child.
      // ---------------------------------------------
      if (ReadPersonGeneticTest1())
      {
        if (ReadPersonGeneticTest5())
        {
          MovePersonGeneticTest2(entities.ExistingReusedSampleChild,
            export.GenTestDetails.Update.DetailChild);
        }
        else
        {
          MovePersonGeneticTest1(entities.ExistingChildPersonGeneticTest,
            export.GenTestDetails.Update.DetailChild);
        }

        if (export.GenTestDetails.Item.DetailChild.ScheduledTestTime.
          GetValueOrDefault() == TimeSpan.Zero)
        {
          export.GenTestDetails.Update.Detail.ChildSchedTestTime = "";
        }
        else
        {
          local.WorkTime.TimeWithAmPm = "";
          local.WorkTime.Wtime =
            export.GenTestDetails.Item.DetailChild.ScheduledTestTime.
              GetValueOrDefault();
          UseCabConvertTimeFormat();
          export.GenTestDetails.Update.Detail.ChildSchedTestTime =
            local.WorkTime.TimeWithAmPm;
        }
      }

      if (ReadPersonGeneticTest3())
      {
        if (ReadPersonGeneticTest2())
        {
          MovePersonGeneticTest2(entities.ExistingReusedSampleFather,
            export.GenTestDetails.Update.DetailFatherPersonGeneticTest);
        }
        else
        {
          MovePersonGeneticTest1(entities.ExistingFatherPersonGeneticTest,
            export.GenTestDetails.Update.DetailFatherPersonGeneticTest);
        }

        if (export.GenTestDetails.Item.DetailFatherPersonGeneticTest.
          ScheduledTestTime.GetValueOrDefault() == TimeSpan.Zero)
        {
          export.GenTestDetails.Update.Detail.FatherSchedTestTime = "";
        }
        else
        {
          local.WorkTime.TimeWithAmPm = "";
          local.WorkTime.Wtime =
            export.GenTestDetails.Item.DetailFatherPersonGeneticTest.
              ScheduledTestTime.GetValueOrDefault();
          UseCabConvertTimeFormat();
          export.GenTestDetails.Update.Detail.FatherSchedTestTime =
            local.WorkTime.TimeWithAmPm;
        }
      }

      if (ReadPersonGeneticTest4())
      {
        if (ReadPersonGeneticTest6())
        {
          MovePersonGeneticTest2(entities.ExistingReusedSampleMother,
            export.GenTestDetails.Update.DetailMotherPersonGeneticTest);
        }
        else
        {
          MovePersonGeneticTest1(entities.ExistingMotherPersonGeneticTest,
            export.GenTestDetails.Update.DetailMotherPersonGeneticTest);
        }

        if (export.GenTestDetails.Item.DetailMotherPersonGeneticTest.
          ScheduledTestTime.GetValueOrDefault() == TimeSpan.Zero)
        {
          export.GenTestDetails.Update.Detail.MotherSchedTestTime = "";
        }
        else
        {
          local.WorkTime.TimeWithAmPm = "";
          local.WorkTime.Wtime =
            export.GenTestDetails.Item.DetailMotherPersonGeneticTest.
              ScheduledTestTime.GetValueOrDefault();
          UseCabConvertTimeFormat();
          export.GenTestDetails.Update.Detail.MotherSchedTestTime =
            local.WorkTime.TimeWithAmPm;
        }
      }

      export.GenTestDetails.Next();
    }
  }

  private static void MoveGeneticTest(GeneticTest source, GeneticTest target)
  {
    target.TestType = source.TestType;
    target.TestResultReceivedDate = source.TestResultReceivedDate;
    target.PaternityExclusionInd = source.PaternityExclusionInd;
    target.PaternityProbability = source.PaternityProbability;
  }

  private static void MovePersonGeneticTest1(PersonGeneticTest source,
    PersonGeneticTest target)
  {
    target.ShowInd = source.ShowInd;
    target.ScheduledTestTime = source.ScheduledTestTime;
    target.ScheduledTestDate = source.ScheduledTestDate;
  }

  private static void MovePersonGeneticTest2(PersonGeneticTest source,
    PersonGeneticTest target)
  {
    target.ScheduledTestTime = source.ScheduledTestTime;
    target.ScheduledTestDate = source.ScheduledTestDate;
  }

  private static void MoveWorkTime(WorkTime source, WorkTime target)
  {
    target.Wtime = source.Wtime;
    target.TimeWithAmPm = source.TimeWithAmPm;
  }

  private void UseCabConvertTimeFormat()
  {
    var useImport = new CabConvertTimeFormat.Import();
    var useExport = new CabConvertTimeFormat.Export();

    MoveWorkTime(local.WorkTime, useImport.WorkTime);

    Call(CabConvertTimeFormat.Execute, useImport, useExport);

    local.ErrorInTimeConversion.Flag = useExport.ErrorInConversion.Flag;
    MoveWorkTime(useExport.WorkTime, local.WorkTime);
  }

  private void UseCabGetClientDetails1()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = export.ChildCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.ChildCsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseCabGetClientDetails2()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number =
      export.GenTestDetails.Item.DetailMotherCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.GenTestDetails.Update.DetailMotherCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseCabGetClientDetails3()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number =
      export.GenTestDetails.Item.DetailFatherCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.GenTestDetails.Update.DetailFatherCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private bool ReadAbsentParent()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingGeneticTest.Populated);
    entities.ExistingAbsentParent.Populated = false;

    return Read("ReadAbsentParent",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId",
          entities.ExistingGeneticTest.CroAIdentifier.GetValueOrDefault());
        db.SetString(
          command, "type", entities.ExistingGeneticTest.CroAType ?? "");
        db.SetString(
          command, "casNumber", entities.ExistingGeneticTest.CasANumber ?? "");
        db.SetString(
          command, "cspNumber", entities.ExistingGeneticTest.CspANumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingAbsentParent.CasNumber = db.GetString(reader, 0);
        entities.ExistingAbsentParent.CspNumber = db.GetString(reader, 1);
        entities.ExistingAbsentParent.Type1 = db.GetString(reader, 2);
        entities.ExistingAbsentParent.Identifier = db.GetInt32(reader, 3);
        entities.ExistingAbsentParent.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingAbsentParent.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingAbsentParent.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingAbsentParent.Type1);
      });
  }

  private bool ReadCase()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleChild.Populated);
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingCaseRoleChild.CasNumber);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadChild()
  {
    entities.ExistingCaseRoleChild.Populated = false;

    return Read("ReadChild",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingCaseRoleChild.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRoleChild.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRoleChild.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRoleChild.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRoleChild.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRoleChild.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRoleChild.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRoleChild.Type1);
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingChildCsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Child.Number);
      },
      (db, reader) =>
      {
        entities.ExistingChildCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingChildCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingAbsentParent.Populated);
    entities.ExistingFatherCsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.ExistingAbsentParent.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingFatherCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingFatherCsePerson.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleMother.Populated);
    entities.ExistingMotherCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.
          SetString(command, "numb", entities.ExistingCaseRoleMother.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.ExistingMotherCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingMotherCsePerson.Populated = true;
      });
  }

  private IEnumerable<bool> ReadGeneticTest()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingCaseRoleChild.Populated);

    return ReadEach("ReadGeneticTest",
      (db, command) =>
      {
        db.SetNullableInt32(
          command, "croIdentifier", entities.ExistingCaseRoleChild.Identifier);
        db.SetNullableString(
          command, "croType", entities.ExistingCaseRoleChild.Type1);
        db.SetNullableString(
          command, "casNumber", entities.ExistingCaseRoleChild.CasNumber);
        db.SetNullableString(
          command, "cspNumber", entities.ExistingCaseRoleChild.CspNumber);
      },
      (db, reader) =>
      {
        if (export.GenTestDetails.IsFull)
        {
          return false;
        }

        entities.ExistingGeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.ExistingGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 1);
        entities.ExistingGeneticTest.TestType = db.GetNullableString(reader, 2);
        entities.ExistingGeneticTest.ActualTestDate =
          db.GetNullableDate(reader, 3);
        entities.ExistingGeneticTest.TestResultReceivedDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingGeneticTest.PaternityExclusionInd =
          db.GetNullableString(reader, 5);
        entities.ExistingGeneticTest.PaternityProbability =
          db.GetNullableDecimal(reader, 6);
        entities.ExistingGeneticTest.NoticeOfContestReceivedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingGeneticTest.CasNumber =
          db.GetNullableString(reader, 8);
        entities.ExistingGeneticTest.CspNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingGeneticTest.CroType = db.GetNullableString(reader, 10);
        entities.ExistingGeneticTest.CroIdentifier =
          db.GetNullableInt32(reader, 11);
        entities.ExistingGeneticTest.LgaIdentifier =
          db.GetNullableInt32(reader, 12);
        entities.ExistingGeneticTest.CasMNumber =
          db.GetNullableString(reader, 13);
        entities.ExistingGeneticTest.CspMNumber =
          db.GetNullableString(reader, 14);
        entities.ExistingGeneticTest.CroMType =
          db.GetNullableString(reader, 15);
        entities.ExistingGeneticTest.CroMIdentifier =
          db.GetNullableInt32(reader, 16);
        entities.ExistingGeneticTest.CasANumber =
          db.GetNullableString(reader, 17);
        entities.ExistingGeneticTest.CspANumber =
          db.GetNullableString(reader, 18);
        entities.ExistingGeneticTest.CroAType =
          db.GetNullableString(reader, 19);
        entities.ExistingGeneticTest.CroAIdentifier =
          db.GetNullableInt32(reader, 20);
        entities.ExistingGeneticTest.Populated = true;
        CheckValid<GeneticTest>("CroType", entities.ExistingGeneticTest.CroType);
          
        CheckValid<GeneticTest>("CroMType",
          entities.ExistingGeneticTest.CroMType);
        CheckValid<GeneticTest>("CroAType",
          entities.ExistingGeneticTest.CroAType);

        return true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingGeneticTest.Populated);
    entities.ExistingPaternityEstablishmt.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          entities.ExistingGeneticTest.LgaIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingPaternityEstablishmt.Identifier =
          db.GetInt32(reader, 0);
        entities.ExistingPaternityEstablishmt.CourtCaseNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingPaternityEstablishmt.Populated = true;
      });
  }

  private bool ReadMother()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingGeneticTest.Populated);
    entities.ExistingCaseRoleMother.Populated = false;

    return Read("ReadMother",
      (db, command) =>
      {
        db.SetInt32(
          command, "caseRoleId",
          entities.ExistingGeneticTest.CroMIdentifier.GetValueOrDefault());
        db.SetString(
          command, "type", entities.ExistingGeneticTest.CroMType ?? "");
        db.SetString(
          command, "casNumber", entities.ExistingGeneticTest.CasMNumber ?? "");
        db.SetString(
          command, "cspNumber", entities.ExistingGeneticTest.CspMNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingCaseRoleMother.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRoleMother.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRoleMother.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRoleMother.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRoleMother.StartDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingCaseRoleMother.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCaseRoleMother.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRoleMother.Type1);
      });
  }

  private bool ReadPersonGeneticTest1()
  {
    entities.ExistingChildPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest1",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingChildCsePerson.Number);
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingGeneticTest.TestNumber);
      },
      (db, reader) =>
      {
        entities.ExistingChildPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingChildPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingChildPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingChildPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingChildPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingChildPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingChildPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingChildPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingChildPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingChildPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingChildPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingChildPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingChildPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 12);
        entities.ExistingChildPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest2()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingFatherPersonGeneticTest.Populated);
    entities.ExistingFatherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingFatherPersonGeneticTest.PgtIdentifier.
            GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.ExistingFatherPersonGeneticTest.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingFatherPersonGeneticTest.GteRTestNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingFatherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingFatherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingFatherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingFatherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingFatherPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingFatherPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingFatherPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingFatherPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFatherPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingFatherPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingFatherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingFatherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingFatherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 12);
        entities.ExistingFatherPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest3()
  {
    entities.ExistingFatherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest3",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingFatherCsePerson.Number);
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingGeneticTest.TestNumber);
      },
      (db, reader) =>
      {
        entities.ExistingFatherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingFatherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingFatherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingFatherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingFatherPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingFatherPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingFatherPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingFatherPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingFatherPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingFatherPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingFatherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingFatherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingFatherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 12);
        entities.ExistingFatherPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest4()
  {
    entities.ExistingMotherPersonGeneticTest.Populated = false;

    return Read("ReadPersonGeneticTest4",
      (db, command) =>
      {
        db.SetString(
          command, "cspNumber", entities.ExistingMotherCsePerson.Number);
        db.SetInt32(
          command, "gteTestNumber", entities.ExistingGeneticTest.TestNumber);
      },
      (db, reader) =>
      {
        entities.ExistingMotherPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingMotherPersonGeneticTest.CspNumber =
          db.GetString(reader, 1);
        entities.ExistingMotherPersonGeneticTest.Identifier =
          db.GetInt32(reader, 2);
        entities.ExistingMotherPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingMotherPersonGeneticTest.SampleUsableInd =
          db.GetNullableString(reader, 4);
        entities.ExistingMotherPersonGeneticTest.CollectSampleInd =
          db.GetNullableString(reader, 5);
        entities.ExistingMotherPersonGeneticTest.ShowInd =
          db.GetNullableString(reader, 6);
        entities.ExistingMotherPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 7);
        entities.ExistingMotherPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 8);
        entities.ExistingMotherPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 9);
        entities.ExistingMotherPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 10);
        entities.ExistingMotherPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 11);
        entities.ExistingMotherPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 12);
        entities.ExistingMotherPersonGeneticTest.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest5()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingChildPersonGeneticTest.Populated);
    entities.ExistingReusedSampleChild.Populated = false;

    return Read("ReadPersonGeneticTest5",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingChildPersonGeneticTest.PgtIdentifier.
            GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.ExistingChildPersonGeneticTest.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingChildPersonGeneticTest.GteRTestNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingReusedSampleChild.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingReusedSampleChild.CspNumber = db.GetString(reader, 1);
        entities.ExistingReusedSampleChild.Identifier = db.GetInt32(reader, 2);
        entities.ExistingReusedSampleChild.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 3);
        entities.ExistingReusedSampleChild.ScheduledTestDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingReusedSampleChild.PgtIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingReusedSampleChild.CspRNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingReusedSampleChild.GteRTestNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingReusedSampleChild.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest6()
  {
    System.Diagnostics.Debug.Assert(
      entities.ExistingMotherPersonGeneticTest.Populated);
    entities.ExistingReusedSampleMother.Populated = false;

    return Read("ReadPersonGeneticTest6",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingMotherPersonGeneticTest.PgtIdentifier.
            GetValueOrDefault());
        db.SetString(
          command, "cspNumber",
          entities.ExistingMotherPersonGeneticTest.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingMotherPersonGeneticTest.GteRTestNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingReusedSampleMother.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingReusedSampleMother.CspNumber = db.GetString(reader, 1);
        entities.ExistingReusedSampleMother.Identifier = db.GetInt32(reader, 2);
        entities.ExistingReusedSampleMother.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 3);
        entities.ExistingReusedSampleMother.ScheduledTestDate =
          db.GetNullableDate(reader, 4);
        entities.ExistingReusedSampleMother.PgtIdentifier =
          db.GetNullableInt32(reader, 5);
        entities.ExistingReusedSampleMother.CspRNumber =
          db.GetNullableString(reader, 6);
        entities.ExistingReusedSampleMother.GteRTestNumber =
          db.GetNullableInt32(reader, 7);
        entities.ExistingReusedSampleMother.Populated = true;
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
    /// A value of Case1.
    /// </summary>
    [JsonPropertyName("case1")]
    public Case1 Case1
    {
      get => case1 ??= new();
      set => case1 = value;
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

    private Case1 case1;
    private CsePerson child;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A GenTestDetailsGroup group.</summary>
    [Serializable]
    public class GenTestDetailsGroup
    {
      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public GeneticTestInformation Detail
      {
        get => detail ??= new();
        set => detail = value;
      }

      /// <summary>
      /// A value of DetailChild.
      /// </summary>
      [JsonPropertyName("detailChild")]
      public PersonGeneticTest DetailChild
      {
        get => detailChild ??= new();
        set => detailChild = value;
      }

      /// <summary>
      /// A value of DetailMotherCsePerson.
      /// </summary>
      [JsonPropertyName("detailMotherCsePerson")]
      public CsePerson DetailMotherCsePerson
      {
        get => detailMotherCsePerson ??= new();
        set => detailMotherCsePerson = value;
      }

      /// <summary>
      /// A value of DetailMotherCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailMotherCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailMotherCsePersonsWorkSet
      {
        get => detailMotherCsePersonsWorkSet ??= new();
        set => detailMotherCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailMotherPersonGeneticTest.
      /// </summary>
      [JsonPropertyName("detailMotherPersonGeneticTest")]
      public PersonGeneticTest DetailMotherPersonGeneticTest
      {
        get => detailMotherPersonGeneticTest ??= new();
        set => detailMotherPersonGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailFatherCsePerson.
      /// </summary>
      [JsonPropertyName("detailFatherCsePerson")]
      public CsePerson DetailFatherCsePerson
      {
        get => detailFatherCsePerson ??= new();
        set => detailFatherCsePerson = value;
      }

      /// <summary>
      /// A value of DetailFatherCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("detailFatherCsePersonsWorkSet")]
      public CsePersonsWorkSet DetailFatherCsePersonsWorkSet
      {
        get => detailFatherCsePersonsWorkSet ??= new();
        set => detailFatherCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of DetailFatherPersonGeneticTest.
      /// </summary>
      [JsonPropertyName("detailFatherPersonGeneticTest")]
      public PersonGeneticTest DetailFatherPersonGeneticTest
      {
        get => detailFatherPersonGeneticTest ??= new();
        set => detailFatherPersonGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailFatherGeneticTest.
      /// </summary>
      [JsonPropertyName("detailFatherGeneticTest")]
      public GeneticTest DetailFatherGeneticTest
      {
        get => detailFatherGeneticTest ??= new();
        set => detailFatherGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailFatherLegalAction.
      /// </summary>
      [JsonPropertyName("detailFatherLegalAction")]
      public LegalAction DetailFatherLegalAction
      {
        get => detailFatherLegalAction ??= new();
        set => detailFatherLegalAction = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private GeneticTestInformation detail;
      private PersonGeneticTest detailChild;
      private CsePerson detailMotherCsePerson;
      private CsePersonsWorkSet detailMotherCsePersonsWorkSet;
      private PersonGeneticTest detailMotherPersonGeneticTest;
      private CsePerson detailFatherCsePerson;
      private CsePersonsWorkSet detailFatherCsePersonsWorkSet;
      private PersonGeneticTest detailFatherPersonGeneticTest;
      private GeneticTest detailFatherGeneticTest;
      private LegalAction detailFatherLegalAction;
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
    /// Gets a value of GenTestDetails.
    /// </summary>
    [JsonIgnore]
    public Array<GenTestDetailsGroup> GenTestDetails => genTestDetails ??= new(
      GenTestDetailsGroup.Capacity);

    /// <summary>
    /// Gets a value of GenTestDetails for json serialization.
    /// </summary>
    [JsonPropertyName("genTestDetails")]
    [Computed]
    public IList<GenTestDetailsGroup> GenTestDetails_Json
    {
      get => genTestDetails;
      set => GenTestDetails.Assign(value);
    }

    private CsePersonsWorkSet childCsePersonsWorkSet;
    private Case1 case1;
    private CsePerson childCsePerson;
    private Array<GenTestDetailsGroup> genTestDetails;
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
    /// A value of ErrorInTimeConversion.
    /// </summary>
    [JsonPropertyName("errorInTimeConversion")]
    public Common ErrorInTimeConversion
    {
      get => errorInTimeConversion ??= new();
      set => errorInTimeConversion = value;
    }

    /// <summary>
    /// A value of WorkTime.
    /// </summary>
    [JsonPropertyName("workTime")]
    public WorkTime WorkTime
    {
      get => workTime ??= new();
      set => workTime = value;
    }

    private DateWorkArea current;
    private Common errorInTimeConversion;
    private WorkTime workTime;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingReusedSampleMother.
    /// </summary>
    [JsonPropertyName("existingReusedSampleMother")]
    public PersonGeneticTest ExistingReusedSampleMother
    {
      get => existingReusedSampleMother ??= new();
      set => existingReusedSampleMother = value;
    }

    /// <summary>
    /// A value of ExistingReusedSampleFather.
    /// </summary>
    [JsonPropertyName("existingReusedSampleFather")]
    public PersonGeneticTest ExistingReusedSampleFather
    {
      get => existingReusedSampleFather ??= new();
      set => existingReusedSampleFather = value;
    }

    /// <summary>
    /// A value of ExistingReusedSampleChild.
    /// </summary>
    [JsonPropertyName("existingReusedSampleChild")]
    public PersonGeneticTest ExistingReusedSampleChild
    {
      get => existingReusedSampleChild ??= new();
      set => existingReusedSampleChild = value;
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
    /// A value of ExistingGenTestTypeCode.
    /// </summary>
    [JsonPropertyName("existingGenTestTypeCode")]
    public Code ExistingGenTestTypeCode
    {
      get => existingGenTestTypeCode ??= new();
      set => existingGenTestTypeCode = value;
    }

    /// <summary>
    /// A value of ExistingGenTestTypeCodeValue.
    /// </summary>
    [JsonPropertyName("existingGenTestTypeCodeValue")]
    public CodeValue ExistingGenTestTypeCodeValue
    {
      get => existingGenTestTypeCodeValue ??= new();
      set => existingGenTestTypeCodeValue = value;
    }

    /// <summary>
    /// A value of ExistingCaseRoleMother.
    /// </summary>
    [JsonPropertyName("existingCaseRoleMother")]
    public CaseRole ExistingCaseRoleMother
    {
      get => existingCaseRoleMother ??= new();
      set => existingCaseRoleMother = value;
    }

    /// <summary>
    /// A value of ExistingCaseRoleFather.
    /// </summary>
    [JsonPropertyName("existingCaseRoleFather")]
    public CaseRole ExistingCaseRoleFather
    {
      get => existingCaseRoleFather ??= new();
      set => existingCaseRoleFather = value;
    }

    /// <summary>
    /// A value of ExistingCaseRoleChild.
    /// </summary>
    [JsonPropertyName("existingCaseRoleChild")]
    public CaseRole ExistingCaseRoleChild
    {
      get => existingCaseRoleChild ??= new();
      set => existingCaseRoleChild = value;
    }

    /// <summary>
    /// A value of ExistingPaternityEstablishmt.
    /// </summary>
    [JsonPropertyName("existingPaternityEstablishmt")]
    public LegalAction ExistingPaternityEstablishmt
    {
      get => existingPaternityEstablishmt ??= new();
      set => existingPaternityEstablishmt = value;
    }

    /// <summary>
    /// A value of ExistingChildPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingChildPersonGeneticTest")]
    public PersonGeneticTest ExistingChildPersonGeneticTest
    {
      get => existingChildPersonGeneticTest ??= new();
      set => existingChildPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingMotherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingMotherPersonGeneticTest")]
    public PersonGeneticTest ExistingMotherPersonGeneticTest
    {
      get => existingMotherPersonGeneticTest ??= new();
      set => existingMotherPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingFatherPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingFatherPersonGeneticTest")]
    public PersonGeneticTest ExistingFatherPersonGeneticTest
    {
      get => existingFatherPersonGeneticTest ??= new();
      set => existingFatherPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingGeneticTest.
    /// </summary>
    [JsonPropertyName("existingGeneticTest")]
    public GeneticTest ExistingGeneticTest
    {
      get => existingGeneticTest ??= new();
      set => existingGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingFatherCsePerson.
    /// </summary>
    [JsonPropertyName("existingFatherCsePerson")]
    public CsePerson ExistingFatherCsePerson
    {
      get => existingFatherCsePerson ??= new();
      set => existingFatherCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingMotherCsePerson.
    /// </summary>
    [JsonPropertyName("existingMotherCsePerson")]
    public CsePerson ExistingMotherCsePerson
    {
      get => existingMotherCsePerson ??= new();
      set => existingMotherCsePerson = value;
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
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    private PersonGeneticTest existingReusedSampleMother;
    private PersonGeneticTest existingReusedSampleFather;
    private PersonGeneticTest existingReusedSampleChild;
    private CaseRole existingAbsentParent;
    private Code existingGenTestTypeCode;
    private CodeValue existingGenTestTypeCodeValue;
    private CaseRole existingCaseRoleMother;
    private CaseRole existingCaseRoleFather;
    private CaseRole existingCaseRoleChild;
    private LegalAction existingPaternityEstablishmt;
    private PersonGeneticTest existingChildPersonGeneticTest;
    private PersonGeneticTest existingMotherPersonGeneticTest;
    private PersonGeneticTest existingFatherPersonGeneticTest;
    private GeneticTest existingGeneticTest;
    private CsePerson existingFatherCsePerson;
    private CsePerson existingMotherCsePerson;
    private CsePerson existingChildCsePerson;
    private Case1 existingCase;
  }
#endregion
}

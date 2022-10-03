// Program: OE_GTSL_GENETIC_TEST_SAMPLE_LIST, ID: 371794195, model: 746.
// Short name: SWEGTSLP
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Security1;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_GTSL_GENETIC_TEST_SAMPLE_LIST.
/// </para>
/// <para>
/// Resp: OBLGEST	
/// This procedure(PRAD) Lists previous GENETIC_TEST samples that are known to 
/// the system.
/// Import is a CSE_PERSON.
/// All GENETIC_TEST records will be shoown if the sample-collected-indicator is
/// a &quot;Y&quot;.
/// </para>
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Online)]
public partial class OeGtslGeneticTestSampleList: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_GTSL_GENETIC_TEST_SAMPLE_LIST program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeGtslGeneticTestSampleList(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeGtslGeneticTestSampleList.
  /// </summary>
  public OeGtslGeneticTestSampleList(IContext context, Import import,
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
    // ******** MAINTENANCE LOG ********************
    // AUTHOR    	 DATE  	  CHG REQ# DESCRIPTION
    // Govinderaj 	05/11/95	Initial Code
    // T.O.Redmond	02/08/96	Retrofit
    // Regan Welborn   06/26/96        Left Pad EAB
    // R. Marchman	11/14/96	Add new security and next tran.
    // Ty Hill-MTW     04/28/97        Change Current_date
    // ******** END MAINTENANCE LOG ****************
    // *********************************************
    // SYSTEM:		KESSEP
    // DESCRIPTION:
    // This procedure step lists previous genetic test samples known to the 
    // system.
    // PROCESSING:
    // It is passed with CSE_PERSON NUMBER. It reads and lists all the 
    // person_genetic_test records where sample_collected_ind is "Y".
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	GENETIC_TEST		- R - -
    // 	PERSON_GENETIC_TEST	- R - -
    // 	VENDOR			- R - -
    // 	VENDOR_ADDRESS		- R - -
    // DATABASE FILES USED:
    // *********************************************
    local.Current.Date = Now().Date;
    ExitState = "ACO_NN0000_ALL_OK";
    export.Hidden.Assign(import.Hidden);

    if (Equal(global.Command, "CLEAR"))
    {
      export.Case1.Number = import.Case1.Number;
      export.CsePerson.Number = import.CsePerson.Number;
      export.CsePersonsWorkSet.FormattedName =
        import.CsePersonsWorkSet.FormattedName;

      return;
    }

    // Move Imports to Exports.
    export.Case1.Number = import.Case1.Number;
    export.CsePerson.Number = import.CsePerson.Number;
    export.CsePersonsWorkSet.FormattedName =
      import.CsePersonsWorkSet.FormattedName;
    export.ListCsePersons.PromptField = import.ListCsePersons.PromptField;
    export.AcceptPersonNo.SelectChar = import.AcceptPersonNo.SelectChar;
    export.Starting.ScheduledTestDate = import.Starting.ScheduledTestDate;

    if (!IsEmpty(export.Case1.Number))
    {
      local.TextWorkArea.Text10 = export.Case1.Number;
      UseEabPadLeftWithZeros();
      export.Case1.Number = local.TextWorkArea.Text10;
    }

    if (!IsEmpty(export.CsePerson.Number))
    {
      local.TextWorkArea.Text10 = export.CsePerson.Number;
      UseEabPadLeftWithZeros();
      export.CsePerson.Number = local.TextWorkArea.Text10;
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
      export.Hidden.CaseNumber = export.Case1.Number;
      export.Hidden.CsePersonNumber = export.CsePerson.Number;
      UseScCabNextTranPut();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        var field = GetField(export.Standard, "nextTransaction");

        field.Error = true;
      }

      return;
    }

    if (Equal(global.Command, "XXNEXTXX"))
    {
      UseScCabNextTranGet();

      // ************************************************
      // *Example: If not equal to spaces or zeroes     *
      // *Set export cse person number to export next   *
      // *tran hidden info cse_person_number.           *
      // ************************************************
      if (!IsEmpty(export.Hidden.CaseNumber))
      {
        export.Case1.Number = export.Hidden.CaseNumber ?? Spaces(10);
      }

      if (!IsEmpty(export.Hidden.CsePersonNumber))
      {
        export.CsePerson.Number = export.Hidden.CsePersonNumber ?? Spaces(10);
      }

      global.Command = "DISPLAY";
    }

    if (Equal(global.Command, "XXFMMENU"))
    {
      global.Command = "DISPLAY";
    }

    if (!Equal(global.Command, "LIST") && !Equal(global.Command, "RETCOMP") && !
      Equal(global.Command, "RETNAME"))
    {
      export.ListCsePersons.PromptField = "";
    }

    if (!import.Import1.IsEmpty)
    {
      export.Export1.Index = 0;
      export.Export1.Clear();

      for(import.Import1.Index = 0; import.Import1.Index < import
        .Import1.Count; ++import.Import1.Index)
      {
        if (export.Export1.IsFull)
        {
          break;
        }

        export.Export1.Update.DetailCommon.SelectChar =
          import.Import1.Item.DetailCommon.SelectChar;
        export.Export1.Update.DetailGeneticTest.Assign(
          import.Import1.Item.DetailGeneticTest);
        export.Export1.Update.DetailPersonGeneticTest.Assign(
          import.Import1.Item.DetailPersonGeneticTest);
        export.Export1.Update.DetailCollTime.Text6 =
          import.Import1.Item.DetailCollTime.Text6;
        MoveVendor(import.Import1.Item.DetailVendor,
          export.Export1.Update.DetailVendor);
        MoveVendorAddress(import.Import1.Item.DetailVendorAddress,
          export.Export1.Update.DetailVendorAddress);
        export.Export1.Next();
      }
    }

    if (Equal(global.Command, "RETCOMP") || Equal(global.Command, "RETNAME"))
    {
      export.CsePerson.Number = import.CsePersonsWorkSet.Number;
      export.ListCsePersons.PromptField = "";

      var field = GetField(export.Starting, "scheduledTestDate");

      field.Protected = false;
      field.Focused = true;

      global.Command = "DISPLAY";
    }

    if (AsChar(import.AcceptPersonNo.SelectChar) == 'N')
    {
      // ---------------------------------------------
      // Dont allow entry of person #/ prompt
      // ---------------------------------------------
      var field1 = GetField(export.CsePerson, "number");

      field1.Protected = true;

      var field2 = GetField(export.ListCsePersons, "promptField");

      field2.Protected = true;
    }

    if (Equal(global.Command, "RETPART") || Equal
      (global.Command, "RETCOMP") || Equal(global.Command, "RETNAME") || Equal
      (global.Command, "RETNAME") || Equal(global.Command, "ENTER"))
    {
      // ************************************************
      // *Security is not required on a return from Link*
      // ************************************************
    }
    else
    {
      UseScCabTestSecurity();

      if (IsExitState("ACO_NN0000_ALL_OK"))
      {
      }
      else
      {
        return;
      }
    }

    switch(TrimEnd(global.Command))
    {
      case "EXIT":
        ExitState = "ECO_LNK_RETURN_TO_MENU";

        break;
      case "SIGNOFF":
        UseScCabSignoff();

        break;
      case "LIST":
        if (AsChar(export.ListCsePersons.PromptField) == 'S')
        {
          if (!IsEmpty(export.Case1.Number))
          {
            ExitState = "ECO_LNK_TO_CASE_COMPOSITION";
          }
          else
          {
            ExitState = "ECO_LNK_TO_CSE_NAME_LIST";
          }
        }
        else
        {
          var field = GetField(export.ListCsePersons, "promptField");

          field.Error = true;

          ExitState = "ACO_NE0000_INVALID_SELECT_CODE";
        }

        break;
      case "DISPLAY":
        // ---------------------------------------------
        // Clear the previous screen contents
        // ---------------------------------------------
        export.CsePersonsWorkSet.FormattedName = "";

        export.Export1.Index = 0;
        export.Export1.Clear();

        for(import.Import1.Index = 0; import.Import1.Index < import
          .Import1.Count; ++import.Import1.Index)
        {
          if (export.Export1.IsFull)
          {
            break;
          }

          export.Export1.Next();

          break;

          export.Export1.Next();
        }

        if (IsEmpty(export.CsePerson.Number))
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "OE0026_INVALID_CSE_PERSON_NO";

          return;
        }

        if (ReadCsePerson())
        {
          export.CsePerson.Number = entities.ExistingCsePerson.Number;
          UseCabGetClientDetails();
        }
        else
        {
          var field = GetField(export.CsePerson, "number");

          field.Error = true;

          ExitState = "CSE_PERSON_NF";

          return;
        }

        if (!IsEmpty(export.Case1.Number))
        {
          if (!ReadCase())
          {
            var field = GetField(export.Case1, "number");

            field.Error = true;

            ExitState = "CASE_NF";

            return;
          }

          if (!ReadCaseRole())
          {
            ExitState = "OE0000_CASE_MEMBER_NE";

            var field1 = GetField(export.CsePerson, "number");

            field1.Error = true;

            var field2 = GetField(export.Case1, "number");

            field2.Error = true;

            return;
          }
        }

        local.Local01010001.ScheduledTestDate = local.NullDate.Date;

        export.Export1.Index = 0;
        export.Export1.Clear();

        foreach(var item in ReadPersonGeneticTestGeneticTest())
        {
          // ---------------------------------------------
          // Move either the current sample or previous reused sample depending 
          // on whether a previous sample was reused for this test.
          // ---------------------------------------------
          if (ReadPersonGeneticTest())
          {
            export.Export1.Update.DetailPersonGeneticTest.Assign(
              entities.ExistingReusedSample);

            if (ReadVendor2())
            {
              MoveVendor(entities.ExistingVendor,
                export.Export1.Update.DetailVendor);
              UseOeCabGetVendorAddress();
            }
          }
          else
          {
            export.Export1.Update.DetailPersonGeneticTest.Assign(
              entities.ExistingPersonGeneticTest);

            if (ReadVendor1())
            {
              MoveVendor(entities.ExistingVendor,
                export.Export1.Update.DetailVendor);
              UseOeCabGetVendorAddress();
            }
          }

          MoveGeneticTest(entities.ExistingGeneticTest,
            export.Export1.Update.DetailGeneticTest);

          if (export.Export1.Item.DetailPersonGeneticTest.ScheduledTestTime.
            GetValueOrDefault() == TimeSpan.Zero)
          {
            export.Export1.Update.DetailCollTime.Text6 = "";
          }
          else
          {
            local.WorkTime.Wtime =
              entities.ExistingPersonGeneticTest.ScheduledTestTime.
                GetValueOrDefault();
            local.WorkTime.TimeWithAmPm = "";
            UseCabConvertTimeFormat();
            export.Export1.Update.DetailCollTime.Text6 =
              local.WorkTime.TimeWithAmPm;
          }

          export.Export1.Next();
        }

        if (export.Export1.IsEmpty)
        {
          ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
        }
        else if (export.Export1.IsFull)
        {
          ExitState = "OE0000_LIST_FULL_PARTIAL_DATA_RT";
        }
        else
        {
          ExitState = "ACO_NI0000_DISPLAY_SUCCESSFUL";
        }

        break;
      case "RETURN":
        local.Selected.Count = 0;

        for(export.Export1.Index = 0; export.Export1.Index < export
          .Export1.Count; ++export.Export1.Index)
        {
          if (!IsEmpty(export.Export1.Item.DetailCommon.SelectChar) && AsChar
            (export.Export1.Item.DetailCommon.SelectChar) != 'S')
          {
            var field =
              GetField(export.Export1.Item.DetailCommon, "selectChar");

            field.Error = true;

            ExitState = "ACO_NE0000_INVALID_SELECT_CODE1";

            return;
          }

          if (AsChar(export.Export1.Item.DetailCommon.SelectChar) == 'S')
          {
            ++local.Selected.Count;

            if (local.Selected.Count > 1)
            {
              var field =
                GetField(export.Export1.Item.DetailCommon, "selectChar");

              field.Error = true;

              ExitState = "ACO_NE0000_INVALID_MULTIPLE_SEL";

              return;
            }

            MoveVendor(export.Export1.Item.DetailVendor,
              export.SelectedPrevSampleVendor);
            MoveVendorAddress(export.Export1.Item.DetailVendorAddress,
              export.SelectedPrevSampleVendorAddress);
            export.SelectedPrevSampleGeneticTest.Assign(
              export.Export1.Item.DetailGeneticTest);
            export.SelectedPrevSamplePersonGeneticTest.Assign(
              export.Export1.Item.DetailPersonGeneticTest);
            export.SelectedPrevSampCtime.Text6 =
              export.Export1.Item.DetailCollTime.Text6;
          }
        }

        ExitState = "ACO_NE0000_RETURN";

        break;
      case "PREV":
        ExitState = "OE0067_SCROLLED_BEY_FIRST_PAGE";

        break;
      case "NEXT":
        ExitState = "OE0068_SCROLLED_BEY_LAST_PAGE";

        break;
      case "ENTER":
        ExitState = "ACO_NE0000_INVALID_COMMAND";

        break;
      default:
        ExitState = "ACO_NE0000_INVALID_PF_KEY";

        break;
    }
  }

  private static void MoveGeneticTest(GeneticTest source, GeneticTest target)
  {
    target.LabCaseNo = source.LabCaseNo;
    target.TestNumber = source.TestNumber;
    target.TestType = source.TestType;
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

  private static void MoveVendor(Vendor source, Vendor target)
  {
    target.Identifier = source.Identifier;
    target.Name = source.Name;
  }

  private static void MoveVendorAddress(VendorAddress source,
    VendorAddress target)
  {
    target.City = source.City;
    target.State = source.State;
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

    MoveWorkTime(useExport.WorkTime, local.WorkTime);
  }

  private void UseCabGetClientDetails()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.CsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseEabPadLeftWithZeros()
  {
    var useImport = new EabPadLeftWithZeros.Import();
    var useExport = new EabPadLeftWithZeros.Export();

    useImport.TextWorkArea.Text10 = local.TextWorkArea.Text10;
    useExport.TextWorkArea.Text10 = local.TextWorkArea.Text10;

    Call(EabPadLeftWithZeros.Execute, useImport, useExport);

    local.TextWorkArea.Text10 = useExport.TextWorkArea.Text10;
  }

  private void UseOeCabGetVendorAddress()
  {
    var useImport = new OeCabGetVendorAddress.Import();
    var useExport = new OeCabGetVendorAddress.Export();

    useImport.Vendor.Identifier = entities.ExistingVendor.Identifier;

    Call(OeCabGetVendorAddress.Execute, useImport, useExport);

    MoveVendorAddress(useExport.VendorAddress,
      export.Export1.Update.DetailVendorAddress);
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

  private void UseScCabSignoff()
  {
    var useImport = new ScCabSignoff.Import();
    var useExport = new ScCabSignoff.Export();

    Call(ScCabSignoff.Execute, useImport, useExport);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.Case1.Number = import.Case1.Number;
    useImport.CsePerson.Number = import.CsePerson.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCase()
  {
    entities.ExistingCase.Populated = false;

    return Read("ReadCase",
      (db, command) =>
      {
        db.SetString(command, "numb", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCase.Number = db.GetString(reader, 0);
        entities.ExistingCase.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.ExistingCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.ExistingCase.Number);
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);
      });
  }

  private bool ReadCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", export.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingCsePerson.Populated = true;
      });
  }

  private bool ReadPersonGeneticTest()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingPersonGeneticTest.Populated);
    entities.ExistingReusedSample.Populated = false;

    return Read("ReadPersonGeneticTest",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingPersonGeneticTest.PgtIdentifier.GetValueOrDefault());
          
        db.SetString(
          command, "cspNumber",
          entities.ExistingPersonGeneticTest.CspRNumber ?? "");
        db.SetInt32(
          command, "gteTestNumber",
          entities.ExistingPersonGeneticTest.GteRTestNumber.
            GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingReusedSample.GteTestNumber = db.GetInt32(reader, 0);
        entities.ExistingReusedSample.CspNumber = db.GetString(reader, 1);
        entities.ExistingReusedSample.Identifier = db.GetInt32(reader, 2);
        entities.ExistingReusedSample.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingReusedSample.SampleCollectedInd =
          db.GetNullableString(reader, 4);
        entities.ExistingReusedSample.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 5);
        entities.ExistingReusedSample.ScheduledTestDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingReusedSample.VenIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.ExistingReusedSample.PgtIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.ExistingReusedSample.CspRNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingReusedSample.GteRTestNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingReusedSample.Populated = true;
      });
  }

  private IEnumerable<bool> ReadPersonGeneticTestGeneticTest()
  {
    return ReadEach("ReadPersonGeneticTestGeneticTest",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.ExistingCsePerson.Number);
        db.SetNullableDate(
          command, "schedTestDate", local.Current.Date.GetValueOrDefault());
        db.SetNullableDate(
          command, "scheduledTestDate1",
          import.Starting.ScheduledTestDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "scheduledTestDate2",
          local.Local01010001.ScheduledTestDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        if (export.Export1.IsFull)
        {
          return false;
        }

        entities.ExistingPersonGeneticTest.GteTestNumber =
          db.GetInt32(reader, 0);
        entities.ExistingGeneticTest.TestNumber = db.GetInt32(reader, 0);
        entities.ExistingPersonGeneticTest.CspNumber = db.GetString(reader, 1);
        entities.ExistingPersonGeneticTest.Identifier = db.GetInt32(reader, 2);
        entities.ExistingPersonGeneticTest.SpecimenId =
          db.GetNullableString(reader, 3);
        entities.ExistingPersonGeneticTest.SampleCollectedInd =
          db.GetNullableString(reader, 4);
        entities.ExistingPersonGeneticTest.ScheduledTestTime =
          db.GetNullableTimeSpan(reader, 5);
        entities.ExistingPersonGeneticTest.ScheduledTestDate =
          db.GetNullableDate(reader, 6);
        entities.ExistingPersonGeneticTest.VenIdentifier =
          db.GetNullableInt32(reader, 7);
        entities.ExistingPersonGeneticTest.PgtIdentifier =
          db.GetNullableInt32(reader, 8);
        entities.ExistingPersonGeneticTest.CspRNumber =
          db.GetNullableString(reader, 9);
        entities.ExistingPersonGeneticTest.GteRTestNumber =
          db.GetNullableInt32(reader, 10);
        entities.ExistingGeneticTest.LabCaseNo =
          db.GetNullableString(reader, 11);
        entities.ExistingGeneticTest.TestType =
          db.GetNullableString(reader, 12);
        entities.ExistingGeneticTest.Populated = true;
        entities.ExistingPersonGeneticTest.Populated = true;

        return true;
      });
  }

  private bool ReadVendor1()
  {
    System.Diagnostics.Debug.
      Assert(entities.ExistingPersonGeneticTest.Populated);
    entities.ExistingVendor.Populated = false;

    return Read("ReadVendor1",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingPersonGeneticTest.VenIdentifier.GetValueOrDefault());
          
      },
      (db, reader) =>
      {
        entities.ExistingVendor.Identifier = db.GetInt32(reader, 0);
        entities.ExistingVendor.Name = db.GetString(reader, 1);
        entities.ExistingVendor.Populated = true;
      });
  }

  private bool ReadVendor2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingReusedSample.Populated);
    entities.ExistingVendor.Populated = false;

    return Read("ReadVendor2",
      (db, command) =>
      {
        db.SetInt32(
          command, "identifier",
          entities.ExistingReusedSample.VenIdentifier.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingVendor.Identifier = db.GetInt32(reader, 0);
        entities.ExistingVendor.Name = db.GetString(reader, 1);
        entities.ExistingVendor.Populated = true;
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
    /// <summary>A ImportGroup group.</summary>
    [Serializable]
    public class ImportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailGeneticTest.
      /// </summary>
      [JsonPropertyName("detailGeneticTest")]
      public GeneticTest DetailGeneticTest
      {
        get => detailGeneticTest ??= new();
        set => detailGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailPersonGeneticTest.
      /// </summary>
      [JsonPropertyName("detailPersonGeneticTest")]
      public PersonGeneticTest DetailPersonGeneticTest
      {
        get => detailPersonGeneticTest ??= new();
        set => detailPersonGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailCollTime.
      /// </summary>
      [JsonPropertyName("detailCollTime")]
      public WorkArea DetailCollTime
      {
        get => detailCollTime ??= new();
        set => detailCollTime = value;
      }

      /// <summary>
      /// A value of DetailVendor.
      /// </summary>
      [JsonPropertyName("detailVendor")]
      public Vendor DetailVendor
      {
        get => detailVendor ??= new();
        set => detailVendor = value;
      }

      /// <summary>
      /// A value of DetailVendorAddress.
      /// </summary>
      [JsonPropertyName("detailVendorAddress")]
      public VendorAddress DetailVendorAddress
      {
        get => detailVendorAddress ??= new();
        set => detailVendorAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common detailCommon;
      private GeneticTest detailGeneticTest;
      private PersonGeneticTest detailPersonGeneticTest;
      private WorkArea detailCollTime;
      private Vendor detailVendor;
      private VendorAddress detailVendorAddress;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
    }

    /// <summary>
    /// A value of AcceptPersonNo.
    /// </summary>
    [JsonPropertyName("acceptPersonNo")]
    public Common AcceptPersonNo
    {
      get => acceptPersonNo ??= new();
      set => acceptPersonNo = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public PersonGeneticTest Starting
    {
      get => starting ??= new();
      set => starting = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Import1.
    /// </summary>
    [JsonIgnore]
    public Array<ImportGroup> Import1 => import1 ??= new(ImportGroup.Capacity);

    /// <summary>
    /// Gets a value of Import1 for json serialization.
    /// </summary>
    [JsonPropertyName("import1")]
    [Computed]
    public IList<ImportGroup> Import1_Json
    {
      get => import1;
      set => Import1.Assign(value);
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

    private Case1 case1;
    private Standard listCsePersons;
    private Common acceptPersonNo;
    private PersonGeneticTest starting;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ImportGroup> import1;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ExportGroup group.</summary>
    [Serializable]
    public class ExportGroup
    {
      /// <summary>
      /// A value of DetailCommon.
      /// </summary>
      [JsonPropertyName("detailCommon")]
      public Common DetailCommon
      {
        get => detailCommon ??= new();
        set => detailCommon = value;
      }

      /// <summary>
      /// A value of DetailGeneticTest.
      /// </summary>
      [JsonPropertyName("detailGeneticTest")]
      public GeneticTest DetailGeneticTest
      {
        get => detailGeneticTest ??= new();
        set => detailGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailPersonGeneticTest.
      /// </summary>
      [JsonPropertyName("detailPersonGeneticTest")]
      public PersonGeneticTest DetailPersonGeneticTest
      {
        get => detailPersonGeneticTest ??= new();
        set => detailPersonGeneticTest = value;
      }

      /// <summary>
      /// A value of DetailCollTime.
      /// </summary>
      [JsonPropertyName("detailCollTime")]
      public WorkArea DetailCollTime
      {
        get => detailCollTime ??= new();
        set => detailCollTime = value;
      }

      /// <summary>
      /// A value of DetailVendor.
      /// </summary>
      [JsonPropertyName("detailVendor")]
      public Vendor DetailVendor
      {
        get => detailVendor ??= new();
        set => detailVendor = value;
      }

      /// <summary>
      /// A value of DetailVendorAddress.
      /// </summary>
      [JsonPropertyName("detailVendorAddress")]
      public VendorAddress DetailVendorAddress
      {
        get => detailVendorAddress ??= new();
        set => detailVendorAddress = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private Common detailCommon;
      private GeneticTest detailGeneticTest;
      private PersonGeneticTest detailPersonGeneticTest;
      private WorkArea detailCollTime;
      private Vendor detailVendor;
      private VendorAddress detailVendorAddress;
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
    /// A value of ListCsePersons.
    /// </summary>
    [JsonPropertyName("listCsePersons")]
    public Standard ListCsePersons
    {
      get => listCsePersons ??= new();
      set => listCsePersons = value;
    }

    /// <summary>
    /// A value of AcceptPersonNo.
    /// </summary>
    [JsonPropertyName("acceptPersonNo")]
    public Common AcceptPersonNo
    {
      get => acceptPersonNo ??= new();
      set => acceptPersonNo = value;
    }

    /// <summary>
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public PersonGeneticTest Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of SelectedPrevSampleGeneticTest.
    /// </summary>
    [JsonPropertyName("selectedPrevSampleGeneticTest")]
    public GeneticTest SelectedPrevSampleGeneticTest
    {
      get => selectedPrevSampleGeneticTest ??= new();
      set => selectedPrevSampleGeneticTest = value;
    }

    /// <summary>
    /// A value of SelectedPrevSamplePersonGeneticTest.
    /// </summary>
    [JsonPropertyName("selectedPrevSamplePersonGeneticTest")]
    public PersonGeneticTest SelectedPrevSamplePersonGeneticTest
    {
      get => selectedPrevSamplePersonGeneticTest ??= new();
      set => selectedPrevSamplePersonGeneticTest = value;
    }

    /// <summary>
    /// A value of SelectedPrevSampCtime.
    /// </summary>
    [JsonPropertyName("selectedPrevSampCtime")]
    public WorkArea SelectedPrevSampCtime
    {
      get => selectedPrevSampCtime ??= new();
      set => selectedPrevSampCtime = value;
    }

    /// <summary>
    /// A value of SelectedPrevSampleVendor.
    /// </summary>
    [JsonPropertyName("selectedPrevSampleVendor")]
    public Vendor SelectedPrevSampleVendor
    {
      get => selectedPrevSampleVendor ??= new();
      set => selectedPrevSampleVendor = value;
    }

    /// <summary>
    /// A value of SelectedPrevSampleVendorAddress.
    /// </summary>
    [JsonPropertyName("selectedPrevSampleVendorAddress")]
    public VendorAddress SelectedPrevSampleVendorAddress
    {
      get => selectedPrevSampleVendorAddress ??= new();
      set => selectedPrevSampleVendorAddress = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// Gets a value of Export1.
    /// </summary>
    [JsonIgnore]
    public Array<ExportGroup> Export1 => export1 ??= new(ExportGroup.Capacity);

    /// <summary>
    /// Gets a value of Export1 for json serialization.
    /// </summary>
    [JsonPropertyName("export1")]
    [Computed]
    public IList<ExportGroup> Export1_Json
    {
      get => export1;
      set => Export1.Assign(value);
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

    private Case1 case1;
    private Standard listCsePersons;
    private Common acceptPersonNo;
    private PersonGeneticTest starting;
    private GeneticTest selectedPrevSampleGeneticTest;
    private PersonGeneticTest selectedPrevSamplePersonGeneticTest;
    private WorkArea selectedPrevSampCtime;
    private Vendor selectedPrevSampleVendor;
    private VendorAddress selectedPrevSampleVendorAddress;
    private CsePerson csePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private Array<ExportGroup> export1;
    private NextTranInfo hidden;
    private Standard standard;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Xxx.
    /// </summary>
    [JsonPropertyName("xxx")]
    public Common Xxx
    {
      get => xxx ??= new();
      set => xxx = value;
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
    /// A value of NullDate.
    /// </summary>
    [JsonPropertyName("nullDate")]
    public DateWorkArea NullDate
    {
      get => nullDate ??= new();
      set => nullDate = value;
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

    /// <summary>
    /// A value of Local01010001.
    /// </summary>
    [JsonPropertyName("local01010001")]
    public PersonGeneticTest Local01010001
    {
      get => local01010001 ??= new();
      set => local01010001 = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public Common Selected
    {
      get => selected ??= new();
      set => selected = value;
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

    private Common xxx;
    private DateWorkArea current;
    private DateWorkArea nullDate;
    private WorkTime workTime;
    private PersonGeneticTest local01010001;
    private Common selected;
    private TextWorkArea textWorkArea;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingReusedSample.
    /// </summary>
    [JsonPropertyName("existingReusedSample")]
    public PersonGeneticTest ExistingReusedSample
    {
      get => existingReusedSample ??= new();
      set => existingReusedSample = value;
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
    /// A value of ExistingPersonGeneticTest.
    /// </summary>
    [JsonPropertyName("existingPersonGeneticTest")]
    public PersonGeneticTest ExistingPersonGeneticTest
    {
      get => existingPersonGeneticTest ??= new();
      set => existingPersonGeneticTest = value;
    }

    /// <summary>
    /// A value of ExistingVendor.
    /// </summary>
    [JsonPropertyName("existingVendor")]
    public Vendor ExistingVendor
    {
      get => existingVendor ??= new();
      set => existingVendor = value;
    }

    /// <summary>
    /// A value of ExistingVendorAddress.
    /// </summary>
    [JsonPropertyName("existingVendorAddress")]
    public VendorAddress ExistingVendorAddress
    {
      get => existingVendorAddress ??= new();
      set => existingVendorAddress = value;
    }

    /// <summary>
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
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
    /// A value of ExistingCaseRole.
    /// </summary>
    [JsonPropertyName("existingCaseRole")]
    public CaseRole ExistingCaseRole
    {
      get => existingCaseRole ??= new();
      set => existingCaseRole = value;
    }

    private PersonGeneticTest existingReusedSample;
    private GeneticTest existingGeneticTest;
    private PersonGeneticTest existingPersonGeneticTest;
    private Vendor existingVendor;
    private VendorAddress existingVendorAddress;
    private CsePerson existingCsePerson;
    private Case1 existingCase;
    private CaseRole existingCaseRole;
  }
#endregion
}

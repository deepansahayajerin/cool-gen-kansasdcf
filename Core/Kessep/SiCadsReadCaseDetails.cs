// Program: SI_CADS_READ_CASE_DETAILS, ID: 371731799, model: 746.
// Short name: SWE01113
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CADS_READ_CASE_DETAILS.
/// </summary>
[Serializable]
public partial class SiCadsReadCaseDetails: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CADS_READ_CASE_DETAILS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiCadsReadCaseDetails(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiCadsReadCaseDetails.
  /// </summary>
  public SiCadsReadCaseDetails(IContext context, Import import, Export export):
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
    // 		M A I N T E N A N C E   L O G
    // Date	  Developer		Description
    // 12-20-95  Ken Evans		Initial Development
    // 01-26-96  P.Elie		Developement
    // 05/02/96  G. Lofton - MTW	Rework
    // 12/12/96  G. Lofton - MTW	Add case function logic.
    // ------------------------------------------------------------
    // 04/26/99 W.Campbell             Code was disabled
    //                                 
    // in order to display the case
    //                                 
    // details correctly dealing with
    //                                 
    // an Interstate case and
    //                                 
    // full_service with or without
    //                                 
    // medical and the locate_ind.
    // -----------------------------------------------------------
    // 06/03/99 W.Campbell             Added logic to qualify
    //                                 
    // Reads for Interstate Request
    //                                 
    // for the other_state_case_status
    //                                 
    // = "O" (Open).
    // -----------------------------------------------------------
    // 06/22/99 W.Campbell             Modified the properties
    //                                 
    // of 2 READ statements to
    //                                 
    // Select Only.
    // ---------------------------------------------------------
    // 06/22/99 W.Campbell             A READ statement
    //                                 
    // was disabled and replace with
    //                                 
    // a READ EACH which follows it.
    //                                 
    // This was done to insure that
    //                                 
    // the correct data for the AR
    //                                 
    // is obtained from the database.
    // ---------------------------------------------------------
    // 07/08/99 W.Campbell             Removed logic to qualify
    //                                 
    // Read for Interstate Request for
    // the
    //                                 
    // other_state_case_status = "O"
    //                                 
    // (Open), and put it inside the
    //                                 
    // when successful clause.
    // -----------------------------------------------------------
    // 07/08/99 W.Campbell             Added IF logic to
    //                                 
    // only set the displayed
    //                                 
    // "O"utgoing or "I"ncoming
    //                                 
    // flag on the screen if the
    //                                 
    // other_state_case_status = "O"
    //                                 
    // (Open) and put it inside the
    //                                 
    // when successful clause.
    // -----------------------------------------------------------
    // 07/08/99 W.Campbell             Logic testing Program Code
    //                                 
    // was disabled as per
    //                                 
    // Jolene Bickel (SME).  She
    //                                 
    // indicated that this test
    //                                 
    // should not be here.
    // ---------------------------------------------------------
    // 07/09/99 W.Campbell             Disabled logic
    //                                 
    // and replaced it with a
    //                                 
    // READ EACH immediately
    //                                 
    // following the disabled code.
    //                                 
    // All of this logic deals with
    //                                 
    // the display of
    //                                 
    // Interstate Case information.
    // -----------------------------------------------------------
    // 12/29/00 M.Lachowicz            Added AP number
    //                                 
    // to READ EACH interstate case.
    // ---------------------------------------------------------------------------
    // 02/14/01 swsrchf  I00112347     Added check for "CSI" functional type 
    // code.
    //                                 
    // Changed READ/READ EACH
    // properties
    //                                 
    // to Uncommitted/Browse.
    //                                 
    // Removed OLD commented out code.
    // ---------------------------------------------------------------------------
    // 07/18/03 GVandy  PR 182045	Removed check for ap case_role end_date = max 
    // date
    // 				when reading for interstate requests for the AP.
    // ---------------------------------------------------------------------------
    // 05/04/06 GVandy  WR230751	Add No Jurisdiction code.
    // 				Change interstate IV-D agency to 5 characters in length.
    // ---------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";
    local.ApCsePerson.Number = import.Ap.Number;
    local.Ar.Number = import.Ar.Number;
    export.DesignatedPayeeInd.Flag = "";
    UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;

    // *****  Determine if the AR has a designated Payee   *****
    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (ReadCsePersonDesigPayee())
    {
      export.DesignatedPayeeInd.Flag = "Y";
    }
    else
    {
      export.DesignatedPayeeInd.Flag = "N";
    }

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (ReadCase())
    {
      export.Case1.Assign(entities.Case1);

      // *** Problem report I00112347
      // *** 02/14/01 swsrchf
      local.Saved.Number = entities.Case1.Number;
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    UseSiCabReturnCaseFunction();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - The following READ
    // was disabled and replace with
    // the READ EACH which follows it.
    // This was done to insure that the correct
    // data for the AR is obtained from the database.
    // ---------------------------------------------------------
    if (ReadCaseRole())
    {
      export.Assign1.Assign(entities.Ar);

      if (Equal(export.Assign1.AssignmentDate, local.Max.Date))
      {
        export.Assign1.AssignmentDate = null;
      }

      if (Equal(export.Assign1.AssignmentTerminatedDt, local.Max.Date))
      {
        export.Assign1.AssignmentTerminatedDt = null;
      }
    }

    if (!entities.Ar.Populated)
    {
      ExitState = "CASE_ROLE_AR_NF";

      return;
    }

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - End of code where
    // READ was disabled and replace with
    // READ EACH which follows it.
    // This was done to insure that the correct
    // data for the AR is obtained from the database.
    // ---------------------------------------------------------
    UseSiReadCaseProgramType();

    if (IsExitState("SI0000_PERSON_PROGRAM_CASE_NF"))
    {
      export.NoActiveProgramInd.Flag = "Y";
      ExitState = "ACO_NN0000_ALL_OK";
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    // -----------------------------------------------------------
    // 07/09/99 W.Campbell - Disabled following logic
    // and replaced it with a READ EACH immediately
    // following the disabled code.
    // -----------------------------------------------------------
    // -----------------------------------------------------------
    // 07/09/99 W.Campbell - End of disabled logic
    // which was replaced it with a READ EACH
    // immediately following the disabled code.
    // -----------------------------------------------------------
    // ----------------------------------------
    // 07/09/99 W.Campbell - The purpose of the following
    // READ EACH and logic statements contained within it
    // is to set the export group of text4 fields to a
    // 3 character string consisting of the 2 char state code
    // followed by a 1 char indicator which is set to either
    // a "I" for (I)ncoming or an "O" for "O"utgoing.  This
    // then indicates to the user the other state involved
    // in the interstate case and whether it is an incoming
    // or outgoing type of interstate case.
    // Examples:
    //   TXO  implies an outgoing interstate case
    //            to Texas.
    //   OKI  implies an incoming interstate case
    //           from Oklahoma.
    // ----------------------------------------
    // 12/29/00 M.L
    // Added CSE_PERSON and AP Case Role to this read each.
    // *** Problem report I00112347
    // *** 02/14/01 swsrchf
    // *** start
    local.ApCaseRole.Type1 = "AP";
    local.Lo1.FunctionalTypeCode = "LO1";
    local.Csi.FunctionalTypeCode = "CSI";

    // *** end
    // *** 02/14/01 swsrchf
    // *** Problem report I00112347
    // 07/18/03  GVandy  PR 182045  Removed check for ap case_role end_date = 
    // max date in the read each below.
    // @@@
    export.Interstate.Index = 0;
    export.Interstate.Clear();

    foreach(var item in ReadInterstateRequest())
    {
      // *** Problem report I00112347
      // *** 02/14/01 swsrchf
      // *** start
      local.CsiLo1Total.Count = 0;
      local.NonCsiLo1Total.Count = 0;

      // *** Count the 'LO1' and 'CSI' types
      ReadInterstateRequestHistory1();

      // *** Count the NON 'LO1' and 'CSI' types
      ReadInterstateRequestHistory2();

      if (local.NonCsiLo1Total.Count == 0 && local.CsiLo1Total.Count > 0)
      {
        export.Interstate.Next();

        continue;
      }

      // >>
      // 02/25/02 T.Bobb PR134930
      // Added check for KS_CASE_IND =  spaces
      if (IsEmpty(entities.InterstateRequest.KsCaseInd))
      {
        export.Interstate.Next();

        continue;
      }

      // *** end
      // *** 02/14/01 swsrchf
      // *** Problem report I00112347
      // ----------------------------------------
      // 07/09/99 W.Campbell - The purpose of the following
      // statements is to set the export text4 field to a
      // 3 character string consisting of the 2 char state code
      // followed by a 1 char indicator which is set to either
      // a "I" for (I)ncoming or an "O" for "O"utgoing.  This
      // then indicates to the user the other state involved
      // in the interstate case and whether it is an incoming
      // or outgoing type of interstate case.
      // Examples:
      //   TXO  implies an outgoing interstate case
      //            to Texas.
      //   OKI  implies an incoming interstate case
      //           from Oklahoma.
      // ----------------------------------------
      if (AsChar(entities.InterstateRequest.KsCaseInd) == 'Y')
      {
        local.Temp.Text1 = "O";
      }
      else
      {
        local.Temp.Text1 = "I";
      }

      if (entities.InterstateRequest.OtherStateFips > 0)
      {
        if (ReadFips())
        {
          export.Interstate.Update.Interstate1.Text8 =
            entities.Fips.StateAbbreviation;
        }
        else
        {
          export.Interstate.Next();

          continue;
        }
      }
      else if (!IsEmpty(entities.InterstateRequest.TribalAgency))
      {
        export.Interstate.Update.Interstate1.Text8 =
          entities.InterstateRequest.TribalAgency ?? Spaces(8);
        export.Interstate.Update.Interstate1.Text10 = "TRIBAL";
      }
      else if (!IsEmpty(entities.InterstateRequest.Country))
      {
        export.Interstate.Update.Interstate1.Text8 =
          entities.InterstateRequest.Country ?? Spaces(8);
        export.Interstate.Update.Interstate1.Text10 = "FOREIGN";
      }
      else
      {
        export.Interstate.Next();

        continue;
      }

      export.Interstate.Update.Interstate1.Text8 =
        TrimEnd(export.Interstate.Item.Interstate1.Text8) + local.Temp.Text1;
      export.Interstate.Next();
    }
  }

  private static void MoveCaseFuncWorkSet(CaseFuncWorkSet source,
    CaseFuncWorkSet target)
  {
    target.FuncText3 = source.FuncText3;
    target.FuncDate = source.FuncDate;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiCabReturnCaseFunction()
  {
    var useImport = new SiCabReturnCaseFunction.Import();
    var useExport = new SiCabReturnCaseFunction.Export();

    useImport.Case1.Number = export.Case1.Number;

    Call(SiCabReturnCaseFunction.Execute, useImport, useExport);

    MoveCaseFuncWorkSet(useExport.CaseFuncWorkSet, export.CaseFuncWorkSet);
  }

  private void UseSiReadCaseProgramType()
  {
    var useImport = new SiReadCaseProgramType.Import();
    var useExport = new SiReadCaseProgramType.Export();

    useImport.Case1.Number = import.Case1.Number;

    Call(SiReadCaseProgramType.Execute, useImport, useExport);

    export.Program.Code = useExport.Program.Code;
    export.MedProgExists.Flag = useExport.MedProgExists.Flag;
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
        entities.Case1.InformationRequestNumber =
          db.GetNullableInt64(reader, 5);
        entities.Case1.Status = db.GetNullableString(reader, 6);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 7);
        entities.Case1.CseOpenDate = db.GetNullableDate(reader, 8);
        entities.Case1.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 9);
        entities.Case1.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 11);
        entities.Case1.PaMedicalService = db.GetNullableString(reader, 12);
        entities.Case1.ClosureLetterDate = db.GetNullableDate(reader, 13);
        entities.Case1.DuplicateCaseIndicator =
          db.GetNullableString(reader, 14);
        entities.Case1.Note = db.GetNullableString(reader, 15);
        entities.Case1.NoJurisdictionCd = db.GetNullableString(reader, 16);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.Ar.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Case1.Number);
        db.SetString(command, "cspNumber", import.Ar.Number);
      },
      (db, reader) =>
      {
        entities.Ar.CasNumber = db.GetString(reader, 0);
        entities.Ar.CspNumber = db.GetString(reader, 1);
        entities.Ar.Type1 = db.GetString(reader, 2);
        entities.Ar.Identifier = db.GetInt32(reader, 3);
        entities.Ar.EndDate = db.GetNullableDate(reader, 4);
        entities.Ar.AssignmentDate = db.GetNullableDate(reader, 5);
        entities.Ar.AssignmentTerminationCode = db.GetNullableString(reader, 6);
        entities.Ar.AssignmentOfRights = db.GetNullableString(reader, 7);
        entities.Ar.AssignmentTerminatedDt = db.GetNullableDate(reader, 8);
        entities.Ar.LastUpdatedTimestamp = db.GetNullableDateTime(reader, 9);
        entities.Ar.LastUpdatedBy = db.GetNullableString(reader, 10);
        entities.Ar.Populated = true;
        CheckValid<CaseRole>("Type1", entities.Ar.Type1);
      });
  }

  private bool ReadCsePersonDesigPayee()
  {
    entities.CsePersonDesigPayee.Populated = false;

    return Read("ReadCsePersonDesigPayee",
      (db, command) =>
      {
        db.SetString(command, "csePersoNum", import.Ar.Number);
        db.SetDate(
          command, "effectiveDate", local.Current.Date.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.CsePersonDesigPayee.SequentialIdentifier =
          db.GetInt32(reader, 0);
        entities.CsePersonDesigPayee.EffectiveDate = db.GetDate(reader, 1);
        entities.CsePersonDesigPayee.DiscontinueDate =
          db.GetNullableDate(reader, 2);
        entities.CsePersonDesigPayee.CsePersoNum = db.GetString(reader, 3);
        entities.CsePersonDesigPayee.Populated = true;
      });
  }

  private bool ReadFips()
  {
    entities.Fips.Populated = false;

    return Read("ReadFips",
      (db, command) =>
      {
        db.
          SetInt32(command, "state", entities.InterstateRequest.OtherStateFips);
          
      },
      (db, reader) =>
      {
        entities.Fips.State = db.GetInt32(reader, 0);
        entities.Fips.County = db.GetInt32(reader, 1);
        entities.Fips.Location = db.GetInt32(reader, 2);
        entities.Fips.StateAbbreviation = db.GetString(reader, 3);
        entities.Fips.Populated = true;
      });
  }

  private IEnumerable<bool> ReadInterstateRequest()
  {
    return ReadEach("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", entities.Case1.Number);
        db.SetNullableString(command, "croType", local.ApCaseRole.Type1);
        db.SetNullableString(command, "cspNumber", import.Ap.Number);
      },
      (db, reader) =>
      {
        if (export.Interstate.IsFull)
        {
          return false;
        }

        entities.InterstateRequest.IntHGeneratedId = db.GetInt32(reader, 0);
        entities.InterstateRequest.OtherStateFips = db.GetInt32(reader, 1);
        entities.InterstateRequest.OtherStateCaseStatus =
          db.GetString(reader, 2);
        entities.InterstateRequest.KsCaseInd = db.GetString(reader, 3);
        entities.InterstateRequest.CasINumber = db.GetNullableString(reader, 4);
        entities.InterstateRequest.CasNumber = db.GetNullableString(reader, 5);
        entities.InterstateRequest.CspNumber = db.GetNullableString(reader, 6);
        entities.InterstateRequest.CroType = db.GetNullableString(reader, 7);
        entities.InterstateRequest.CroId = db.GetNullableInt32(reader, 8);
        entities.InterstateRequest.Country = db.GetNullableString(reader, 9);
        entities.InterstateRequest.TribalAgency =
          db.GetNullableString(reader, 10);
        entities.InterstateRequest.Populated = true;
        CheckValid<InterstateRequest>("CroType",
          entities.InterstateRequest.CroType);

        return true;
      });
  }

  private bool ReadInterstateRequestHistory1()
  {
    return Read("ReadInterstateRequestHistory1",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.SetString(
          command, "functionalTypeCode1", local.Csi.FunctionalTypeCode);
        db.SetString(
          command, "functionalTypeCode2", local.Lo1.FunctionalTypeCode);
      },
      (db, reader) =>
      {
        local.CsiLo1Total.Count = db.GetInt32(reader, 0);
      });
  }

  private bool ReadInterstateRequestHistory2()
  {
    return Read("ReadInterstateRequestHistory2",
      (db, command) =>
      {
        db.SetInt32(
          command, "intGeneratedId",
          entities.InterstateRequest.IntHGeneratedId);
        db.
          SetString(command, "functionalTypeCo1", local.Csi.FunctionalTypeCode);
          
        db.
          SetString(command, "functionalTypeCo2", local.Lo1.FunctionalTypeCode);
          
      },
      (db, reader) =>
      {
        local.NonCsiLo1Total.Count = db.GetInt32(reader, 0);
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
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePersonsWorkSet Ap
    {
      get => ap ??= new();
      set => ap = value;
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

    private Case1 case1;
    private CsePersonsWorkSet ap;
    private CsePersonsWorkSet ar;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A InterstateGroup group.</summary>
    [Serializable]
    public class InterstateGroup
    {
      /// <summary>
      /// A value of Interstate1.
      /// </summary>
      [JsonPropertyName("interstate1")]
      public TextWorkArea Interstate1
      {
        get => interstate1 ??= new();
        set => interstate1 = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 3;

      private TextWorkArea interstate1;
    }

    /// <summary>A ZdelExportGroupIsGroup group.</summary>
    [Serializable]
    public class ZdelExportGroupIsGroup
    {
      /// <summary>
      /// A value of ZdelExportGrpIsState.
      /// </summary>
      [JsonPropertyName("zdelExportGrpIsState")]
      public Fips ZdelExportGrpIsState
      {
        get => zdelExportGrpIsState ??= new();
        set => zdelExportGrpIsState = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private Fips zdelExportGrpIsState;
    }

    /// <summary>
    /// Gets a value of Interstate.
    /// </summary>
    [JsonIgnore]
    public Array<InterstateGroup> Interstate => interstate ??= new(
      InterstateGroup.Capacity);

    /// <summary>
    /// Gets a value of Interstate for json serialization.
    /// </summary>
    [JsonPropertyName("interstate")]
    [Computed]
    public IList<InterstateGroup> Interstate_Json
    {
      get => interstate;
      set => Interstate.Assign(value);
    }

    /// <summary>
    /// Gets a value of ZdelExportGroupIs.
    /// </summary>
    [JsonIgnore]
    public Array<ZdelExportGroupIsGroup> ZdelExportGroupIs =>
      zdelExportGroupIs ??= new(ZdelExportGroupIsGroup.Capacity);

    /// <summary>
    /// Gets a value of ZdelExportGroupIs for json serialization.
    /// </summary>
    [JsonPropertyName("zdelExportGroupIs")]
    [Computed]
    public IList<ZdelExportGroupIsGroup> ZdelExportGroupIs_Json
    {
      get => zdelExportGroupIs;
      set => ZdelExportGroupIs.Assign(value);
    }

    /// <summary>
    /// A value of NoActiveProgramInd.
    /// </summary>
    [JsonPropertyName("noActiveProgramInd")]
    public Common NoActiveProgramInd
    {
      get => noActiveProgramInd ??= new();
      set => noActiveProgramInd = value;
    }

    /// <summary>
    /// A value of DesignatedPayeeInd.
    /// </summary>
    [JsonPropertyName("designatedPayeeInd")]
    public Common DesignatedPayeeInd
    {
      get => designatedPayeeInd ??= new();
      set => designatedPayeeInd = value;
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

    /// <summary>
    /// A value of Zdel.
    /// </summary>
    [JsonPropertyName("zdel")]
    public InterstateRequest Zdel
    {
      get => zdel ??= new();
      set => zdel = value;
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
    /// A value of Assign1.
    /// </summary>
    [JsonPropertyName("assign1")]
    public CaseRole Assign1
    {
      get => assign1 ??= new();
      set => assign1 = value;
    }

    /// <summary>
    /// A value of MedProgExists.
    /// </summary>
    [JsonPropertyName("medProgExists")]
    public Common MedProgExists
    {
      get => medProgExists ??= new();
      set => medProgExists = value;
    }

    /// <summary>
    /// A value of CaseFuncWorkSet.
    /// </summary>
    [JsonPropertyName("caseFuncWorkSet")]
    public CaseFuncWorkSet CaseFuncWorkSet
    {
      get => caseFuncWorkSet ??= new();
      set => caseFuncWorkSet = value;
    }

    private Array<InterstateGroup> interstate;
    private Array<ZdelExportGroupIsGroup> zdelExportGroupIs;
    private Common noActiveProgramInd;
    private Common designatedPayeeInd;
    private Program program;
    private InterstateRequest zdel;
    private Case1 case1;
    private CaseRole assign1;
    private Common medProgExists;
    private CaseFuncWorkSet caseFuncWorkSet;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Lo1.
    /// </summary>
    [JsonPropertyName("lo1")]
    public InterstateRequestHistory Lo1
    {
      get => lo1 ??= new();
      set => lo1 = value;
    }

    /// <summary>
    /// A value of Csi.
    /// </summary>
    [JsonPropertyName("csi")]
    public InterstateRequestHistory Csi
    {
      get => csi ??= new();
      set => csi = value;
    }

    /// <summary>
    /// A value of NonCsiLo1Total.
    /// </summary>
    [JsonPropertyName("nonCsiLo1Total")]
    public Common NonCsiLo1Total
    {
      get => nonCsiLo1Total ??= new();
      set => nonCsiLo1Total = value;
    }

    /// <summary>
    /// A value of CsiLo1Total.
    /// </summary>
    [JsonPropertyName("csiLo1Total")]
    public Common CsiLo1Total
    {
      get => csiLo1Total ??= new();
      set => csiLo1Total = value;
    }

    /// <summary>
    /// A value of Saved.
    /// </summary>
    [JsonPropertyName("saved")]
    public Case1 Saved
    {
      get => saved ??= new();
      set => saved = value;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public TextWorkArea Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of Zero.
    /// </summary>
    [JsonPropertyName("zero")]
    public DateWorkArea Zero
    {
      get => zero ??= new();
      set => zero = value;
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
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    /// <summary>
    /// A value of UpdateCase.
    /// </summary>
    [JsonPropertyName("updateCase")]
    public Common UpdateCase
    {
      get => updateCase ??= new();
      set => updateCase = value;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
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
    /// A value of Max.
    /// </summary>
    [JsonPropertyName("max")]
    public DateWorkArea Max
    {
      get => max ??= new();
      set => max = value;
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

    private InterstateRequestHistory lo1;
    private InterstateRequestHistory csi;
    private Common nonCsiLo1Total;
    private Common csiLo1Total;
    private Case1 saved;
    private CaseRole apCaseRole;
    private TextWorkArea temp;
    private DateWorkArea zero;
    private DateWorkArea current;
    private CaseUnit caseUnit;
    private Common updateCase;
    private AbendData abendData;
    private CsePerson ar;
    private CsePerson apCsePerson;
    private DateWorkArea max;
    private DateWorkArea blank;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of InterstateRequestHistory.
    /// </summary>
    [JsonPropertyName("interstateRequestHistory")]
    public InterstateRequestHistory InterstateRequestHistory
    {
      get => interstateRequestHistory ??= new();
      set => interstateRequestHistory = value;
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
    /// A value of Fips.
    /// </summary>
    [JsonPropertyName("fips")]
    public Fips Fips
    {
      get => fips ??= new();
      set => fips = value;
    }

    /// <summary>
    /// A value of CsePersonDesigPayee.
    /// </summary>
    [JsonPropertyName("csePersonDesigPayee")]
    public CsePersonDesigPayee CsePersonDesigPayee
    {
      get => csePersonDesigPayee ??= new();
      set => csePersonDesigPayee = value;
    }

    /// <summary>
    /// A value of ZdelInformationRequest.
    /// </summary>
    [JsonPropertyName("zdelInformationRequest")]
    public InformationRequest ZdelInformationRequest
    {
      get => zdelInformationRequest ??= new();
      set => zdelInformationRequest = value;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    public CaseRole Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of ZdelCaseUnit.
    /// </summary>
    [JsonPropertyName("zdelCaseUnit")]
    public CaseUnit ZdelCaseUnit
    {
      get => zdelCaseUnit ??= new();
      set => zdelCaseUnit = value;
    }

    private InterstateRequestHistory interstateRequestHistory;
    private CaseRole ap;
    private Fips fips;
    private CsePersonDesigPayee csePersonDesigPayee;
    private InformationRequest zdelInformationRequest;
    private CsePerson csePerson;
    private InterstateRequest interstateRequest;
    private Case1 case1;
    private CaseRole ar;
    private CaseUnit zdelCaseUnit;
  }
#endregion
}

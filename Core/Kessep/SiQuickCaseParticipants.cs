// Program: SI_QUICK_CASE_PARTICIPANTS, ID: 374540677, model: 746.
// Short name: SWE03110
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_QUICK_CASE_PARTICIPANTS.
/// </summary>
[Serializable]
public partial class SiQuickCaseParticipants: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_QUICK_CASE_PARTICIPANTS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiQuickCaseParticipants(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiQuickCaseParticipants.
  /// </summary>
  public SiQuickCaseParticipants(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // IMPORTANT
    // !!********************************************
    // *    IF IMPORT OR EXPORT VIEWS ARE MODIFIED THEN THE  *
    // *    DB2 STORED PROCEDURE MUST BE UPDATED TO MATCH!!  *
    // *******************************************************
    ExitState = "ACO_NN0000_ALL_OK";
    UseSiQuickGetCpHeader();

    if (IsExitState("CASE_NF"))
    {
      export.QuickErrorMessages.ErrorCode = "406";
      export.QuickErrorMessages.ErrorMessage = "Case Not Found";

      return;
    }

    // **********************************************************************
    // If family violence is found for any case participant on the case
    // then no data will be returned.  This includes all active and
    // inactive case participants.  This is true for open or closed cases.
    // **********************************************************************
    if (IsExitState("SC0000_DATA_NOT_DISPLAY_FOR_FV"))
    {
      export.QuickErrorMessages.ErrorCode = "407";
      export.QuickErrorMessages.ErrorMessage =
        "Disclosure prohibited on the case requested.";

      return;
    }

    // *********************************************************************************
    // Use KS case number and FIPS code of requesting state to retrieve
    // the requesting state case number that KS has on record to return
    // *********************************************************************************
    if (ReadCase())
    {
      export.QuickCaseParticipants.CaseStatus =
        entities.ExistingCase.Status ?? Spaces(1);
    }

    if (ReadInterstateRequest())
    {
      export.QuickCaseParticipants.RequestingStateCaseId =
        entities.ExistingInterstateRequest.OtherStateCaseId ?? Spaces(15);
    }

    export.QuickAp.Index = -1;
    export.QuickAr.Index = -1;
    export.QuickCh.Index = -1;

    foreach(var item in ReadCaseRoleCsePerson())
    {
      if (Equal(entities.ExistingCsePerson.Number, local.Previous.Number))
      {
        continue;
      }

      local.Previous.Number = entities.ExistingCsePerson.Number;

      if (IsEmpty(entities.ExistingCsePerson.FamilyViolenceIndicator))
      {
        local.Fv.Text1 = "N";
      }
      else
      {
        local.Fv.Text1 = "Y";
      }

      // **********************************************************
      // Use case role type to group persons information
      // **********************************************************
      if (Equal(entities.ExistingCaseRole.Type1, "AP"))
      {
        if (export.QuickAp.Index + 1 >= Export.QuickApGroup.Capacity)
        {
          continue;
        }

        ++export.QuickAp.Index;
        export.QuickAp.CheckSize();

        // Retrieve person's name, SSN, and DOB.
        if (ReadCsePersonDetail())
        {
          export.QuickAp.Update.Ap.LastName = entities.CsePersonDetail.LastName;
          export.QuickAp.Update.Ap.FirstName =
            entities.CsePersonDetail.FirstName;
          export.QuickAp.Update.Ap.MiddleInitial =
            entities.CsePersonDetail.MiddleInitial ?? Spaces(1);

          if (!IsEmpty(entities.CsePersonDetail.Ssn))
          {
            export.QuickAp.Update.Ap.Ssn = entities.CsePersonDetail.Ssn ?? Spaces
              (9);
          }

          if (Lt(local.Null1.Date, entities.CsePersonDetail.DateOfBirth))
          {
            export.QuickAp.Update.Ap.Dob =
              NumberToString(DateToInt(entities.CsePersonDetail.DateOfBirth), 8);
              
          }
        }
        else
        {
          // Person may not have been copied to CSE Person Detail table yet.
        }

        export.QuickAp.Update.Ap.Fvi = local.Fv.Text1;
      }
      else if (Equal(entities.ExistingCaseRole.Type1, "AR"))
      {
        if (export.QuickAr.Index + 1 >= Export.QuickArGroup.Capacity)
        {
          continue;
        }

        ++export.QuickAr.Index;
        export.QuickAr.CheckSize();

        export.QuickAr.Update.ArCsePerson.Type1 =
          entities.ExistingCsePerson.Type1;

        // *************************************************************
        // When AR is an Organization want to get the Organization name.
        // *************************************************************
        if (AsChar(entities.ExistingCsePerson.Type1) == 'O')
        {
          export.QuickAr.Update.ArCsePerson.OrganizationName =
            entities.ExistingCsePerson.OrganizationName;
        }
        else
        {
          // Retrieve person's name, SSN, and DOB.
          if (ReadCsePersonDetail())
          {
            export.QuickAr.Update.ArQuickPersonsWorkSet.LastName =
              entities.CsePersonDetail.LastName;
            export.QuickAr.Update.ArQuickPersonsWorkSet.FirstName =
              entities.CsePersonDetail.FirstName;
            export.QuickAr.Update.ArQuickPersonsWorkSet.MiddleInitial =
              entities.CsePersonDetail.MiddleInitial ?? Spaces(1);

            if (!IsEmpty(entities.CsePersonDetail.Ssn))
            {
              export.QuickAr.Update.ArQuickPersonsWorkSet.Ssn =
                entities.CsePersonDetail.Ssn ?? Spaces(9);
            }

            if (Lt(local.Null1.Date, entities.CsePersonDetail.DateOfBirth))
            {
              export.QuickAr.Update.ArQuickPersonsWorkSet.Dob =
                NumberToString(DateToInt(entities.CsePersonDetail.DateOfBirth),
                8);
            }
          }
          else
          {
            // Person may not have been copied to CSE Person Detail table yet.
          }

          export.QuickAr.Update.ArQuickPersonsWorkSet.Fvi = local.Fv.Text1;
        }
      }
      else if (Equal(entities.ExistingCaseRole.Type1, "CH"))
      {
        if (export.QuickCh.Index >= Export.QuickChGroup.Capacity)
        {
          continue;
        }

        ++export.QuickCh.Index;
        export.QuickCh.CheckSize();

        // Retrieve person's name, SSN, and DOB.
        if (ReadCsePersonDetail())
        {
          export.QuickCh.Update.ChQuickPersonsWorkSet.LastName =
            entities.CsePersonDetail.LastName;
          export.QuickCh.Update.ChQuickPersonsWorkSet.FirstName =
            entities.CsePersonDetail.FirstName;
          export.QuickCh.Update.ChQuickPersonsWorkSet.MiddleInitial =
            entities.CsePersonDetail.MiddleInitial ?? Spaces(1);

          if (!IsEmpty(entities.CsePersonDetail.Ssn))
          {
            export.QuickCh.Update.ChQuickPersonsWorkSet.Ssn =
              entities.CsePersonDetail.Ssn ?? Spaces(9);
          }

          if (Lt(local.Null1.Date, entities.CsePersonDetail.DateOfBirth))
          {
            export.QuickCh.Update.ChQuickPersonsWorkSet.Dob =
              NumberToString(DateToInt(entities.CsePersonDetail.DateOfBirth), 8);
              
          }
        }
        else
        {
          // Person may not have been copied to CSE Person Detail table yet.
        }

        if (AsChar(entities.ExistingCsePerson.BornOutOfWedlock) == 'Y')
        {
          export.QuickCh.Update.ChCsePerson.BornOutOfWedlock =
            entities.ExistingCsePerson.BornOutOfWedlock;
        }
        else
        {
          export.QuickCh.Update.ChCsePerson.BornOutOfWedlock = "N";
        }

        export.QuickCh.Update.ChQuickPersonsWorkSet.Fvi = local.Fv.Text1;
      }
    }
  }

  private void UseSiQuickGetCpHeader()
  {
    var useImport = new SiQuickGetCpHeader.Import();
    var useExport = new SiQuickGetCpHeader.Export();

    useImport.QuickInQuery.CaseId = import.QuickInQuery.CaseId;

    Call(SiQuickGetCpHeader.Execute, useImport, useExport);

    export.QuickCpHeader.Assign(useExport.QuickCpHeader);
    export.Case1.Number = useExport.Case1.Number;
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
        entities.ExistingCase.Status = db.GetNullableString(reader, 1);
        entities.ExistingCase.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson()
  {
    entities.ExistingCsePerson.Populated = false;
    entities.ExistingCaseRole.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", export.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingCsePerson.Type1 = db.GetString(reader, 6);
        entities.ExistingCsePerson.OrganizationName =
          db.GetNullableString(reader, 7);
        entities.ExistingCsePerson.FamilyViolenceIndicator =
          db.GetNullableString(reader, 8);
        entities.ExistingCsePerson.BornOutOfWedlock =
          db.GetNullableString(reader, 9);
        entities.ExistingCsePerson.Populated = true;
        entities.ExistingCaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.ExistingCaseRole.Type1);
        CheckValid<CsePerson>("Type1", entities.ExistingCsePerson.Type1);

        return true;
      });
  }

  private bool ReadCsePersonDetail()
  {
    entities.CsePersonDetail.Populated = false;

    return Read("ReadCsePersonDetail",
      (db, command) =>
      {
        db.
          SetString(command, "personNumber", entities.ExistingCsePerson.Number);
          
      },
      (db, reader) =>
      {
        entities.CsePersonDetail.PersonNumber = db.GetString(reader, 0);
        entities.CsePersonDetail.FirstName = db.GetString(reader, 1);
        entities.CsePersonDetail.LastName = db.GetString(reader, 2);
        entities.CsePersonDetail.MiddleInitial =
          db.GetNullableString(reader, 3);
        entities.CsePersonDetail.DateOfBirth = db.GetNullableDate(reader, 4);
        entities.CsePersonDetail.Ssn = db.GetNullableString(reader, 5);
        entities.CsePersonDetail.Populated = true;
      });
  }

  private bool ReadInterstateRequest()
  {
    entities.ExistingInterstateRequest.Populated = false;

    return Read("ReadInterstateRequest",
      (db, command) =>
      {
        db.SetNullableString(command, "casINumber", export.Case1.Number);
        db.SetInt32(command, "othrStateFipsCd", import.QuickInQuery.OsFips);
      },
      (db, reader) =>
      {
        entities.ExistingInterstateRequest.IntHGeneratedId =
          db.GetInt32(reader, 0);
        entities.ExistingInterstateRequest.OtherStateCaseId =
          db.GetNullableString(reader, 1);
        entities.ExistingInterstateRequest.OtherStateFips =
          db.GetInt32(reader, 2);
        entities.ExistingInterstateRequest.CasINumber =
          db.GetNullableString(reader, 3);
        entities.ExistingInterstateRequest.Populated = true;
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
    /// A value of QuickInQuery.
    /// </summary>
    [JsonPropertyName("quickInQuery")]
    public QuickInQuery QuickInQuery
    {
      get => quickInQuery ??= new();
      set => quickInQuery = value;
    }

    private QuickInQuery quickInQuery;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A QuickApGroup group.</summary>
    [Serializable]
    public class QuickApGroup
    {
      /// <summary>
      /// A value of Ap.
      /// </summary>
      [JsonPropertyName("ap")]
      public QuickPersonsWorkSet Ap
      {
        get => ap ??= new();
        set => ap = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private QuickPersonsWorkSet ap;
    }

    /// <summary>A QuickArGroup group.</summary>
    [Serializable]
    public class QuickArGroup
    {
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
      /// A value of ArQuickPersonsWorkSet.
      /// </summary>
      [JsonPropertyName("arQuickPersonsWorkSet")]
      public QuickPersonsWorkSet ArQuickPersonsWorkSet
      {
        get => arQuickPersonsWorkSet ??= new();
        set => arQuickPersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private CsePerson arCsePerson;
      private QuickPersonsWorkSet arQuickPersonsWorkSet;
    }

    /// <summary>A QuickChGroup group.</summary>
    [Serializable]
    public class QuickChGroup
    {
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
      /// A value of ChQuickPersonsWorkSet.
      /// </summary>
      [JsonPropertyName("chQuickPersonsWorkSet")]
      public QuickPersonsWorkSet ChQuickPersonsWorkSet
      {
        get => chQuickPersonsWorkSet ??= new();
        set => chQuickPersonsWorkSet = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 40;

      private CsePerson chCsePerson;
      private QuickPersonsWorkSet chQuickPersonsWorkSet;
    }

    /// <summary>
    /// Gets a value of QuickAp.
    /// </summary>
    [JsonIgnore]
    public Array<QuickApGroup> QuickAp => quickAp ??= new(
      QuickApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickAp for json serialization.
    /// </summary>
    [JsonPropertyName("quickAp")]
    [Computed]
    public IList<QuickApGroup> QuickAp_Json
    {
      get => quickAp;
      set => QuickAp.Assign(value);
    }

    /// <summary>
    /// Gets a value of QuickAr.
    /// </summary>
    [JsonIgnore]
    public Array<QuickArGroup> QuickAr => quickAr ??= new(
      QuickArGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickAr for json serialization.
    /// </summary>
    [JsonPropertyName("quickAr")]
    [Computed]
    public IList<QuickArGroup> QuickAr_Json
    {
      get => quickAr;
      set => QuickAr.Assign(value);
    }

    /// <summary>
    /// Gets a value of QuickCh.
    /// </summary>
    [JsonIgnore]
    public Array<QuickChGroup> QuickCh => quickCh ??= new(
      QuickChGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of QuickCh for json serialization.
    /// </summary>
    [JsonPropertyName("quickCh")]
    [Computed]
    public IList<QuickChGroup> QuickCh_Json
    {
      get => quickCh;
      set => QuickCh.Assign(value);
    }

    /// <summary>
    /// A value of QuickCaseParticipants.
    /// </summary>
    [JsonPropertyName("quickCaseParticipants")]
    public QuickCaseParticipants QuickCaseParticipants
    {
      get => quickCaseParticipants ??= new();
      set => quickCaseParticipants = value;
    }

    /// <summary>
    /// A value of QuickCpHeader.
    /// </summary>
    [JsonPropertyName("quickCpHeader")]
    public QuickCpHeader QuickCpHeader
    {
      get => quickCpHeader ??= new();
      set => quickCpHeader = value;
    }

    /// <summary>
    /// A value of QuickErrorMessages.
    /// </summary>
    [JsonPropertyName("quickErrorMessages")]
    public QuickErrorMessages QuickErrorMessages
    {
      get => quickErrorMessages ??= new();
      set => quickErrorMessages = value;
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

    private Array<QuickApGroup> quickAp;
    private Array<QuickArGroup> quickAr;
    private Array<QuickChGroup> quickCh;
    private QuickCaseParticipants quickCaseParticipants;
    private QuickCpHeader quickCpHeader;
    private QuickErrorMessages quickErrorMessages;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of Previous.
    /// </summary>
    [JsonPropertyName("previous")]
    public CsePerson Previous
    {
      get => previous ??= new();
      set => previous = value;
    }

    /// <summary>
    /// A value of Fv.
    /// </summary>
    [JsonPropertyName("fv")]
    public WorkArea Fv
    {
      get => fv ??= new();
      set => fv = value;
    }

    /// <summary>
    /// A value of Input.
    /// </summary>
    [JsonPropertyName("input")]
    public CsePersonsWorkSet Input
    {
      get => input ??= new();
      set => input = value;
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

    private DateWorkArea null1;
    private CsePerson previous;
    private WorkArea fv;
    private CsePersonsWorkSet input;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonDetail.
    /// </summary>
    [JsonPropertyName("csePersonDetail")]
    public CsePersonDetail CsePersonDetail
    {
      get => csePersonDetail ??= new();
      set => csePersonDetail = value;
    }

    /// <summary>
    /// A value of ExistingFips.
    /// </summary>
    [JsonPropertyName("existingFips")]
    public Fips ExistingFips
    {
      get => existingFips ??= new();
      set => existingFips = value;
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
    /// A value of ExistingCsePerson.
    /// </summary>
    [JsonPropertyName("existingCsePerson")]
    public CsePerson ExistingCsePerson
    {
      get => existingCsePerson ??= new();
      set => existingCsePerson = value;
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

    /// <summary>
    /// A value of ExistingCase.
    /// </summary>
    [JsonPropertyName("existingCase")]
    public Case1 ExistingCase
    {
      get => existingCase ??= new();
      set => existingCase = value;
    }

    private CsePersonDetail csePersonDetail;
    private Fips existingFips;
    private InterstateRequest existingInterstateRequest;
    private CsePerson existingCsePerson;
    private CaseRole existingCaseRole;
    private Case1 existingCase;
  }
#endregion
}

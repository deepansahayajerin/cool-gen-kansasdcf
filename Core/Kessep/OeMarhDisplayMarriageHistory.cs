// Program: OE_MARH_DISPLAY_MARRIAGE_HISTORY, ID: 371884874, model: 746.
// Short name: SWE00945
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_MARH_DISPLAY_MARRIAGE_HISTORY.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// This action block reads and populates marriage_history details.
/// </para>
/// </summary>
[Serializable]
public partial class OeMarhDisplayMarriageHistory: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_MARH_DISPLAY_MARRIAGE_HISTORY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeMarhDisplayMarriageHistory(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeMarhDisplayMarriageHistory.
  /// </summary>
  public OeMarhDisplayMarriageHistory(IContext context, Import import,
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
    // SYSTEM:		KESSEP
    // MODULE:
    // MODULE TYPE:
    // DESCRIPTION:
    // This action block populates marriage history views for display.
    // PROCESSING:
    // This action block is passed the key details of marriage history to be 
    // displayed. It reads and populates export view of the marriage history
    // record.
    // ACTION BLOCKS:
    // ENTITY TYPES USED:
    // 	CSE_PERSON		- R - -
    // 	MARRIAGE_HISTORY	- R - -
    // DATABSE FILES USED:
    // CREATED BY:		govindaraj.
    // DATE CREATED:		01-04-1995.
    // *********************************************
    // *********************************************
    // MAINTENANCE LOG
    // AUTHOR	DATE	CHG REQ#	DESCRIPTION
    // govind	01-04-95		Initial Coding
    // *********************************************
    export.PrimeCsePerson.Number = import.SelectedPrime.Number;

    if (ReadCsePerson1())
    {
      export.PrimeCsePerson.Number = entities.ExistingPrime.Number;
      UseCabGetClientDetails2();
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    local.MarriageHistoryFound.Flag = "N";

    if (AsChar(import.DispSpecificMhistRec.Flag) == 'Y')
    {
      if (ReadMarriageHistory1())
      {
        MoveMarriageHistory(entities.Existing, export.MarriageHistory);
        local.MarriageHistoryFound.Flag = "Y";
        export.UpdateTimestamps.LastUpdatedBy = entities.Existing.LastUpdatedBy;
        export.UpdateTimestamps.LastUpdatedTimestamp =
          entities.Existing.LastUpdatedTimestamp;
      }
    }
    else if (ReadMarriageHistory2())
    {
      local.MarriageHistoryFound.Flag = "Y";
      MoveMarriageHistory(entities.Existing, export.MarriageHistory);
      export.UpdateTimestamps.LastUpdatedBy = entities.Existing.LastUpdatedBy;
      export.UpdateTimestamps.LastUpdatedTimestamp =
        entities.Existing.LastUpdatedTimestamp;
    }

    if (AsChar(local.MarriageHistoryFound.Flag) == 'N')
    {
      ExitState = "OE0001_NO_MARRIAGE_HISTORY_AVL";
    }
    else
    {
      if (IsEmpty(export.MarriageHistory.MarriageCountry))
      {
        export.MarriageCountry.Description =
          Spaces(CodeValue.Description_MaxLength);
      }
      else
      {
        local.Code.CodeName = "COUNTRY CODE";
        local.CodeValue.Cdvalue = export.MarriageHistory.MarriageCountry ?? Spaces
          (10);
        UseCabGetCodeValueDescription2();
      }

      if (IsEmpty(export.MarriageHistory.DivorceCountry))
      {
        export.DivorceCountry.Description =
          Spaces(CodeValue.Description_MaxLength);
      }
      else
      {
        local.Code.CodeName = "COUNTRY CODE";
        local.CodeValue.Cdvalue = export.MarriageHistory.DivorceCountry ?? Spaces
          (10);
        UseCabGetCodeValueDescription1();
      }

      if (ReadCsePerson2())
      {
        export.PrimeCsePerson.Number = entities.ExistingPrime.Number;
        UseCabGetClientDetails2();
      }

      if (ReadCsePerson3())
      {
        export.SpouseCsePerson.Number = entities.ExistingSpouseCsePerson.Number;
        UseCabGetClientDetails1();
      }
      else if (ReadContact())
      {
        export.SpouseContact.Assign(entities.ExistingSpouseContact);
      }
    }
  }

  private static void MoveCode(Code source, Code target)
  {
    target.Id = source.Id;
    target.CodeName = source.CodeName;
  }

  private static void MoveCodeValue(CodeValue source, CodeValue target)
  {
    target.Cdvalue = source.Cdvalue;
    target.Description = source.Description;
  }

  private static void MoveMarriageHistory(MarriageHistory source,
    MarriageHistory target)
  {
    target.Identifier = source.Identifier;
    target.DivorceCity = source.DivorceCity;
    target.MarriageCertificateCity = source.MarriageCertificateCity;
    target.DivorceCourtOrderNumber = source.DivorceCourtOrderNumber;
    target.DivorcePetitionDate = source.DivorcePetitionDate;
    target.MarriageCertificateState = source.MarriageCertificateState;
    target.MarriageCountry = source.MarriageCountry;
    target.DivorcePendingInd = source.DivorcePendingInd;
    target.DivorceCounty = source.DivorceCounty;
    target.DivorceState = source.DivorceState;
    target.DivorceCountry = source.DivorceCountry;
    target.MarriageCertificateCounty = source.MarriageCertificateCounty;
    target.DivorceDate = source.DivorceDate;
    target.SeparationDate = source.SeparationDate;
    target.MarriageDate = source.MarriageDate;
  }

  private void UseCabGetClientDetails1()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingSpouseCsePerson.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.SpouseCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseCabGetClientDetails2()
  {
    var useImport = new CabGetClientDetails.Import();
    var useExport = new CabGetClientDetails.Export();

    useImport.CsePerson.Number = entities.ExistingPrime.Number;

    Call(CabGetClientDetails.Execute, useImport, useExport);

    export.PrimeCsePersonsWorkSet.FormattedName =
      useExport.CsePersonsWorkSet.FormattedName;
  }

  private void UseCabGetCodeValueDescription1()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    MoveCode(local.Code, useImport.Code);

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, export.DivorceCountry);
  }

  private void UseCabGetCodeValueDescription2()
  {
    var useImport = new CabGetCodeValueDescription.Import();
    var useExport = new CabGetCodeValueDescription.Export();

    useImport.CodeValue.Cdvalue = local.CodeValue.Cdvalue;
    MoveCode(local.Code, useImport.Code);

    Call(CabGetCodeValueDescription.Execute, useImport, useExport);

    MoveCodeValue(useExport.CodeValue, export.MarriageCountry);
  }

  private bool ReadContact()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.ExistingSpouseContact.Populated = false;

    return Read("ReadContact",
      (db, command) =>
      {
        db.SetInt32(
          command, "contactNumber",
          entities.Existing.ConINumber.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.Existing.CspINumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingSpouseContact.CspNumber = db.GetString(reader, 0);
        entities.ExistingSpouseContact.ContactNumber = db.GetInt32(reader, 1);
        entities.ExistingSpouseContact.NameLast =
          db.GetNullableString(reader, 2);
        entities.ExistingSpouseContact.NameFirst =
          db.GetNullableString(reader, 3);
        entities.ExistingSpouseContact.MiddleInitial =
          db.GetNullableString(reader, 4);
        entities.ExistingSpouseContact.Populated = true;
      });
  }

  private bool ReadCsePerson1()
  {
    entities.ExistingPrime.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "numb", import.SelectedPrime.Number);
      },
      (db, reader) =>
      {
        entities.ExistingPrime.Number = db.GetString(reader, 0);
        entities.ExistingPrime.Populated = true;
      });
  }

  private bool ReadCsePerson2()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.ExistingPrime.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Existing.CspRNumber);
      },
      (db, reader) =>
      {
        entities.ExistingPrime.Number = db.GetString(reader, 0);
        entities.ExistingPrime.Populated = true;
      });
  }

  private bool ReadCsePerson3()
  {
    System.Diagnostics.Debug.Assert(entities.Existing.Populated);
    entities.ExistingSpouseCsePerson.Populated = false;

    return Read("ReadCsePerson3",
      (db, command) =>
      {
        db.SetString(command, "numb", entities.Existing.CspNumber ?? "");
      },
      (db, reader) =>
      {
        entities.ExistingSpouseCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingSpouseCsePerson.Populated = true;
      });
  }

  private bool ReadMarriageHistory1()
  {
    entities.Existing.Populated = false;

    return Read("ReadMarriageHistory1",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
        db.SetInt32(command, "identifier", import.Selected.Identifier);
      },
      (db, reader) =>
      {
        entities.Existing.CspRNumber = db.GetString(reader, 0);
        entities.Existing.MarriageDate = db.GetNullableDate(reader, 1);
        entities.Existing.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.Existing.DivorcePetitionDate = db.GetNullableDate(reader, 3);
        entities.Existing.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.Existing.MarriageCountry = db.GetNullableString(reader, 5);
        entities.Existing.DivorcePendingInd = db.GetNullableString(reader, 6);
        entities.Existing.DivorceCounty = db.GetNullableString(reader, 7);
        entities.Existing.DivorceState = db.GetNullableString(reader, 8);
        entities.Existing.DivorceCountry = db.GetNullableString(reader, 9);
        entities.Existing.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.Existing.DivorceDate = db.GetNullableDate(reader, 11);
        entities.Existing.SeparationDate = db.GetNullableDate(reader, 12);
        entities.Existing.CreatedBy = db.GetString(reader, 13);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.Existing.LastUpdatedBy = db.GetString(reader, 15);
        entities.Existing.LastUpdatedTimestamp = db.GetDateTime(reader, 16);
        entities.Existing.CspNumber = db.GetNullableString(reader, 17);
        entities.Existing.CspINumber = db.GetNullableString(reader, 18);
        entities.Existing.ConINumber = db.GetNullableInt32(reader, 19);
        entities.Existing.DivorceCity = db.GetNullableString(reader, 20);
        entities.Existing.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.Existing.Identifier = db.GetInt32(reader, 22);
        entities.Existing.Populated = true;
      });
  }

  private bool ReadMarriageHistory2()
  {
    entities.Existing.Populated = false;

    return Read("ReadMarriageHistory2",
      (db, command) =>
      {
        db.SetString(command, "cspRNumber", entities.ExistingPrime.Number);
      },
      (db, reader) =>
      {
        entities.Existing.CspRNumber = db.GetString(reader, 0);
        entities.Existing.MarriageDate = db.GetNullableDate(reader, 1);
        entities.Existing.DivorceCourtOrderNumber =
          db.GetNullableString(reader, 2);
        entities.Existing.DivorcePetitionDate = db.GetNullableDate(reader, 3);
        entities.Existing.MarriageCertificateState =
          db.GetNullableString(reader, 4);
        entities.Existing.MarriageCountry = db.GetNullableString(reader, 5);
        entities.Existing.DivorcePendingInd = db.GetNullableString(reader, 6);
        entities.Existing.DivorceCounty = db.GetNullableString(reader, 7);
        entities.Existing.DivorceState = db.GetNullableString(reader, 8);
        entities.Existing.DivorceCountry = db.GetNullableString(reader, 9);
        entities.Existing.MarriageCertificateCounty =
          db.GetNullableString(reader, 10);
        entities.Existing.DivorceDate = db.GetNullableDate(reader, 11);
        entities.Existing.SeparationDate = db.GetNullableDate(reader, 12);
        entities.Existing.CreatedBy = db.GetString(reader, 13);
        entities.Existing.CreatedTimestamp = db.GetDateTime(reader, 14);
        entities.Existing.LastUpdatedBy = db.GetString(reader, 15);
        entities.Existing.LastUpdatedTimestamp = db.GetDateTime(reader, 16);
        entities.Existing.CspNumber = db.GetNullableString(reader, 17);
        entities.Existing.CspINumber = db.GetNullableString(reader, 18);
        entities.Existing.ConINumber = db.GetNullableInt32(reader, 19);
        entities.Existing.DivorceCity = db.GetNullableString(reader, 20);
        entities.Existing.MarriageCertificateCity =
          db.GetNullableString(reader, 21);
        entities.Existing.Identifier = db.GetInt32(reader, 22);
        entities.Existing.Populated = true;
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
    /// A value of DispSpecificMhistRec.
    /// </summary>
    [JsonPropertyName("dispSpecificMhistRec")]
    public Common DispSpecificMhistRec
    {
      get => dispSpecificMhistRec ??= new();
      set => dispSpecificMhistRec = value;
    }

    /// <summary>
    /// A value of Selected.
    /// </summary>
    [JsonPropertyName("selected")]
    public MarriageHistory Selected
    {
      get => selected ??= new();
      set => selected = value;
    }

    /// <summary>
    /// A value of SelectedPrime.
    /// </summary>
    [JsonPropertyName("selectedPrime")]
    public CsePerson SelectedPrime
    {
      get => selectedPrime ??= new();
      set => selectedPrime = value;
    }

    private Common dispSpecificMhistRec;
    private MarriageHistory selected;
    private CsePerson selectedPrime;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of DivorceCountry.
    /// </summary>
    [JsonPropertyName("divorceCountry")]
    public CodeValue DivorceCountry
    {
      get => divorceCountry ??= new();
      set => divorceCountry = value;
    }

    /// <summary>
    /// A value of MarriageCountry.
    /// </summary>
    [JsonPropertyName("marriageCountry")]
    public CodeValue MarriageCountry
    {
      get => marriageCountry ??= new();
      set => marriageCountry = value;
    }

    /// <summary>
    /// A value of UpdateTimestamps.
    /// </summary>
    [JsonPropertyName("updateTimestamps")]
    public MarriageHistory UpdateTimestamps
    {
      get => updateTimestamps ??= new();
      set => updateTimestamps = value;
    }

    /// <summary>
    /// A value of SpouseCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("spouseCsePersonsWorkSet")]
    public CsePersonsWorkSet SpouseCsePersonsWorkSet
    {
      get => spouseCsePersonsWorkSet ??= new();
      set => spouseCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of PrimeCsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("primeCsePersonsWorkSet")]
    public CsePersonsWorkSet PrimeCsePersonsWorkSet
    {
      get => primeCsePersonsWorkSet ??= new();
      set => primeCsePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of MarriageHistory.
    /// </summary>
    [JsonPropertyName("marriageHistory")]
    public MarriageHistory MarriageHistory
    {
      get => marriageHistory ??= new();
      set => marriageHistory = value;
    }

    /// <summary>
    /// A value of SpouseContact.
    /// </summary>
    [JsonPropertyName("spouseContact")]
    public Contact SpouseContact
    {
      get => spouseContact ??= new();
      set => spouseContact = value;
    }

    /// <summary>
    /// A value of SpouseCsePerson.
    /// </summary>
    [JsonPropertyName("spouseCsePerson")]
    public CsePerson SpouseCsePerson
    {
      get => spouseCsePerson ??= new();
      set => spouseCsePerson = value;
    }

    /// <summary>
    /// A value of PrimeCsePerson.
    /// </summary>
    [JsonPropertyName("primeCsePerson")]
    public CsePerson PrimeCsePerson
    {
      get => primeCsePerson ??= new();
      set => primeCsePerson = value;
    }

    private CodeValue divorceCountry;
    private CodeValue marriageCountry;
    private MarriageHistory updateTimestamps;
    private CsePersonsWorkSet spouseCsePersonsWorkSet;
    private CsePersonsWorkSet primeCsePersonsWorkSet;
    private MarriageHistory marriageHistory;
    private Contact spouseContact;
    private CsePerson spouseCsePerson;
    private CsePerson primeCsePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
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
    /// A value of MarriageHistoryFound.
    /// </summary>
    [JsonPropertyName("marriageHistoryFound")]
    public Common MarriageHistoryFound
    {
      get => marriageHistoryFound ??= new();
      set => marriageHistoryFound = value;
    }

    private CodeValue codeValue;
    private Code code;
    private Common marriageHistoryFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public MarriageHistory Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    /// <summary>
    /// A value of ExistingSpouseContact.
    /// </summary>
    [JsonPropertyName("existingSpouseContact")]
    public Contact ExistingSpouseContact
    {
      get => existingSpouseContact ??= new();
      set => existingSpouseContact = value;
    }

    /// <summary>
    /// A value of ExistingSpouseCsePerson.
    /// </summary>
    [JsonPropertyName("existingSpouseCsePerson")]
    public CsePerson ExistingSpouseCsePerson
    {
      get => existingSpouseCsePerson ??= new();
      set => existingSpouseCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingPrime.
    /// </summary>
    [JsonPropertyName("existingPrime")]
    public CsePerson ExistingPrime
    {
      get => existingPrime ??= new();
      set => existingPrime = value;
    }

    private MarriageHistory existing;
    private Contact existingSpouseContact;
    private CsePerson existingSpouseCsePerson;
    private CsePerson existingPrime;
  }
#endregion
}

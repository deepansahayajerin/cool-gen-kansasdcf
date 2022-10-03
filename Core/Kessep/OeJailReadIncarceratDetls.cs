// Program: OE_JAIL_READ_INCARCERAT_DETLS, ID: 371794508, model: 746.
// Short name: SWE00933
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: OE_JAIL_READ_INCARCERAT_DETLS.
/// </para>
/// <para>
/// RESP: OBLGESTB
/// WRITTEN BY:SID
/// </para>
/// </summary>
[Serializable]
public partial class OeJailReadIncarceratDetls: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_JAIL_READ_INCARCERAT_DETLS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeJailReadIncarceratDetls(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeJailReadIncarceratDetls.
  /// </summary>
  public OeJailReadIncarceratDetls(IContext context, Import import,
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
    // ---------------------------------------------
    // Date        Author         Reason
    // 02/22/95    Sid            Creation
    // 06/12/97    R Grey	   Modify join to separate reads for
    //                            incarceration and address
    // ---------------------------------------------
    // 04/19/00  W.Campbell       Placed READ EACH stmts
    //                            within IF stmts checking to insure
    //                            that an Entity View has been
    //                            populated before using it in
    //                            a CURRENT qualification.  This
    //                            was a bug which allowed this type
    //                            of READ to work in versions of
    //                            Cool Gen prior to v 5.1, but will not
    //                            work in v5.1.  This work was done on
    //                            WR# 00162 for PRWORA - Family
    //                            Violence.
    // ---------------------------------------------
    export.CsePerson.Number = import.CsePerson.Number;
    UseOeCabSetMnemonics();

    if (IsEmpty(import.CsePerson.Number))
    {
      ExitState = "ACO_NE0000_REQUIRED_FIELD_MISSIN";

      return;
    }

    if (ReadCsePerson())
    {
      export.CsePerson.Number = entities.CsePerson.Number;
    }
    else
    {
      ExitState = "CSE_PERSON_NF";

      return;
    }

    // ---------------------------------------------
    // Read the CURRENT Jail/Prison Incarceration
    // Record.
    // ---------------------------------------------
    if (ReadIncarceration1())
    {
      MoveIncarceration(entities.JailIncarceration, export.JailIncarceration);

      if (Equal(entities.JailIncarceration.EndDate, local.MaxDate.ExpirationDate))
        
      {
        export.JailIncarceration.EndDate = null;
      }
    }

    // ---------------------------------------------
    // 04/19/00  W.Campbell - Placed READ EACH stmts
    // within IF stmts checking to insure that an Entity
    // View has been populated before using it in a
    // CURRENT qualification.  This was a bug
    // which allowed this type of READ to work
    // in versions of Cool Gen prior to v 5.1, but
    // will notwork in v5.1.  This work was done on
    // WR# 00162 for PRWORA - Family Violence.
    // ---------------------------------------------
    if (entities.JailIncarceration.Populated)
    {
      if (ReadIncarcerationAddress1())
      {
        export.JailIncarcerationAddress.
          Assign(entities.JailIncarcerationAddress);
      }
    }

    // ---------------------------------------------
    // Read the CURRENT Probation/Parole incarceration Record.
    // ---------------------------------------------
    if (ReadIncarceration2())
    {
      export.ProbationIncarceration.Assign(entities.ProbationIncarceration);

      if (Equal(entities.ProbationIncarceration.EndDate,
        local.MaxDate.ExpirationDate))
      {
        export.ProbationIncarceration.EndDate = null;
      }
    }

    // ---------------------------------------------
    // 04/19/00  W.Campbell - Placed READ EACH stmts
    // within IF stmts checking to insure that an Entity
    // View has been populated before using it in a
    // CURRENT qualification.  This was a bug
    // which allowed this type of READ to work
    // in versions of Cool Gen prior to v 5.1, but
    // will notwork in v5.1.  This work was done on
    // WR# 00162 for PRWORA - Family Violence.
    // ---------------------------------------------
    if (entities.ProbationIncarceration.Populated)
    {
      if (ReadIncarcerationAddress2())
      {
        export.ProbationIncarcerationAddress.Assign(
          entities.ProbationIncarcerationAddress);
      }
    }

    if (!entities.JailIncarceration.Populated && !
      entities.ProbationIncarceration.Populated)
    {
      ExitState = "ACO_NI0000_NO_DATA_TO_DISPLAY";
    }
    else
    {
      ExitState = "ACO_NI0000_SUCCESSFUL_DISPLAY";
    }
  }

  private static void MoveIncarceration(Incarceration source,
    Incarceration target)
  {
    target.PhoneAreaCode = source.PhoneAreaCode;
    target.PhoneExt = source.PhoneExt;
    target.Identifier = source.Identifier;
    target.VerifiedUserId = source.VerifiedUserId;
    target.VerifiedDate = source.VerifiedDate;
    target.InmateNumber = source.InmateNumber;
    target.ParoleEligibilityDate = source.ParoleEligibilityDate;
    target.WorkReleaseInd = source.WorkReleaseInd;
    target.ConditionsForRelease = source.ConditionsForRelease;
    target.ParoleOfficersTitle = source.ParoleOfficersTitle;
    target.Phone = source.Phone;
    target.EndDate = source.EndDate;
    target.StartDate = source.StartDate;
    target.Type1 = source.Type1;
    target.InstitutionName = source.InstitutionName;
    target.ParoleOfficerLastName = source.ParoleOfficerLastName;
    target.ParoleOfficerFirstName = source.ParoleOfficerFirstName;
    target.ParoleOfficerMiddleInitial = source.ParoleOfficerMiddleInitial;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.Incarcerated = source.Incarcerated;
  }

  private void UseOeCabSetMnemonics()
  {
    var useImport = new OeCabSetMnemonics.Import();
    var useExport = new OeCabSetMnemonics.Export();

    Call(OeCabSetMnemonics.Execute, useImport, useExport);

    local.MaxDate.ExpirationDate = useExport.MaxDate.ExpirationDate;
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
      });
  }

  private bool ReadIncarceration1()
  {
    entities.JailIncarceration.Populated = false;

    return Read("ReadIncarceration1",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.JailIncarceration.CspNumber = db.GetString(reader, 0);
        entities.JailIncarceration.Identifier = db.GetInt32(reader, 1);
        entities.JailIncarceration.VerifiedUserId =
          db.GetNullableString(reader, 2);
        entities.JailIncarceration.VerifiedDate = db.GetNullableDate(reader, 3);
        entities.JailIncarceration.InmateNumber =
          db.GetNullableString(reader, 4);
        entities.JailIncarceration.ParoleEligibilityDate =
          db.GetNullableDate(reader, 5);
        entities.JailIncarceration.WorkReleaseInd =
          db.GetNullableString(reader, 6);
        entities.JailIncarceration.ConditionsForRelease =
          db.GetNullableString(reader, 7);
        entities.JailIncarceration.ParoleOfficersTitle =
          db.GetNullableString(reader, 8);
        entities.JailIncarceration.Phone = db.GetNullableInt32(reader, 9);
        entities.JailIncarceration.EndDate = db.GetNullableDate(reader, 10);
        entities.JailIncarceration.StartDate = db.GetNullableDate(reader, 11);
        entities.JailIncarceration.Type1 = db.GetNullableString(reader, 12);
        entities.JailIncarceration.InstitutionName =
          db.GetNullableString(reader, 13);
        entities.JailIncarceration.ParoleOfficerLastName =
          db.GetNullableString(reader, 14);
        entities.JailIncarceration.ParoleOfficerFirstName =
          db.GetNullableString(reader, 15);
        entities.JailIncarceration.ParoleOfficerMiddleInitial =
          db.GetNullableString(reader, 16);
        entities.JailIncarceration.LastUpdatedBy = db.GetString(reader, 17);
        entities.JailIncarceration.PhoneExt = db.GetNullableString(reader, 18);
        entities.JailIncarceration.PhoneAreaCode =
          db.GetNullableInt32(reader, 19);
        entities.JailIncarceration.Incarcerated =
          db.GetNullableString(reader, 20);
        entities.JailIncarceration.Populated = true;
      });
  }

  private bool ReadIncarceration2()
  {
    entities.ProbationIncarceration.Populated = false;

    return Read("ReadIncarceration2",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", entities.CsePerson.Number);
      },
      (db, reader) =>
      {
        entities.ProbationIncarceration.CspNumber = db.GetString(reader, 0);
        entities.ProbationIncarceration.Identifier = db.GetInt32(reader, 1);
        entities.ProbationIncarceration.VerifiedUserId =
          db.GetNullableString(reader, 2);
        entities.ProbationIncarceration.VerifiedDate =
          db.GetNullableDate(reader, 3);
        entities.ProbationIncarceration.InmateNumber =
          db.GetNullableString(reader, 4);
        entities.ProbationIncarceration.ParoleEligibilityDate =
          db.GetNullableDate(reader, 5);
        entities.ProbationIncarceration.WorkReleaseInd =
          db.GetNullableString(reader, 6);
        entities.ProbationIncarceration.ConditionsForRelease =
          db.GetNullableString(reader, 7);
        entities.ProbationIncarceration.ParoleOfficersTitle =
          db.GetNullableString(reader, 8);
        entities.ProbationIncarceration.Phone = db.GetNullableInt32(reader, 9);
        entities.ProbationIncarceration.EndDate =
          db.GetNullableDate(reader, 10);
        entities.ProbationIncarceration.StartDate =
          db.GetNullableDate(reader, 11);
        entities.ProbationIncarceration.Type1 =
          db.GetNullableString(reader, 12);
        entities.ProbationIncarceration.InstitutionName =
          db.GetNullableString(reader, 13);
        entities.ProbationIncarceration.ParoleOfficerLastName =
          db.GetNullableString(reader, 14);
        entities.ProbationIncarceration.ParoleOfficerFirstName =
          db.GetNullableString(reader, 15);
        entities.ProbationIncarceration.ParoleOfficerMiddleInitial =
          db.GetNullableString(reader, 16);
        entities.ProbationIncarceration.LastUpdatedBy =
          db.GetString(reader, 17);
        entities.ProbationIncarceration.PhoneExt =
          db.GetNullableString(reader, 18);
        entities.ProbationIncarceration.PhoneAreaCode =
          db.GetNullableInt32(reader, 19);
        entities.ProbationIncarceration.Incarcerated =
          db.GetNullableString(reader, 20);
        entities.ProbationIncarceration.Populated = true;
      });
  }

  private bool ReadIncarcerationAddress1()
  {
    System.Diagnostics.Debug.Assert(entities.JailIncarceration.Populated);
    entities.JailIncarcerationAddress.Populated = false;

    return Read("ReadIncarcerationAddress1",
      (db, command) =>
      {
        db.SetInt32(
          command, "incIdentifier", entities.JailIncarceration.Identifier);
        db.
          SetString(command, "cspNumber", entities.JailIncarceration.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.JailIncarcerationAddress.IncIdentifier =
          db.GetInt32(reader, 0);
        entities.JailIncarcerationAddress.CspNumber = db.GetString(reader, 1);
        entities.JailIncarcerationAddress.EffectiveDate = db.GetDate(reader, 2);
        entities.JailIncarcerationAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.JailIncarcerationAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.JailIncarcerationAddress.City =
          db.GetNullableString(reader, 5);
        entities.JailIncarcerationAddress.State =
          db.GetNullableString(reader, 6);
        entities.JailIncarcerationAddress.Province =
          db.GetNullableString(reader, 7);
        entities.JailIncarcerationAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.JailIncarcerationAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.JailIncarcerationAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.JailIncarcerationAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.JailIncarcerationAddress.Country =
          db.GetNullableString(reader, 12);
        entities.JailIncarcerationAddress.Populated = true;
      });
  }

  private bool ReadIncarcerationAddress2()
  {
    System.Diagnostics.Debug.Assert(entities.ProbationIncarceration.Populated);
    entities.ProbationIncarcerationAddress.Populated = false;

    return Read("ReadIncarcerationAddress2",
      (db, command) =>
      {
        db.SetInt32(
          command, "incIdentifier", entities.ProbationIncarceration.Identifier);
          
        db.SetString(
          command, "cspNumber", entities.ProbationIncarceration.CspNumber);
      },
      (db, reader) =>
      {
        entities.ProbationIncarcerationAddress.IncIdentifier =
          db.GetInt32(reader, 0);
        entities.ProbationIncarcerationAddress.CspNumber =
          db.GetString(reader, 1);
        entities.ProbationIncarcerationAddress.EffectiveDate =
          db.GetDate(reader, 2);
        entities.ProbationIncarcerationAddress.Street1 =
          db.GetNullableString(reader, 3);
        entities.ProbationIncarcerationAddress.Street2 =
          db.GetNullableString(reader, 4);
        entities.ProbationIncarcerationAddress.City =
          db.GetNullableString(reader, 5);
        entities.ProbationIncarcerationAddress.State =
          db.GetNullableString(reader, 6);
        entities.ProbationIncarcerationAddress.Province =
          db.GetNullableString(reader, 7);
        entities.ProbationIncarcerationAddress.PostalCode =
          db.GetNullableString(reader, 8);
        entities.ProbationIncarcerationAddress.ZipCode5 =
          db.GetNullableString(reader, 9);
        entities.ProbationIncarcerationAddress.ZipCode4 =
          db.GetNullableString(reader, 10);
        entities.ProbationIncarcerationAddress.Zip3 =
          db.GetNullableString(reader, 11);
        entities.ProbationIncarcerationAddress.Country =
          db.GetNullableString(reader, 12);
        entities.ProbationIncarcerationAddress.Populated = true;
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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
    }

    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ProbationIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("probationIncarcerationAddress")]
    public IncarcerationAddress ProbationIncarcerationAddress
    {
      get => probationIncarcerationAddress ??= new();
      set => probationIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of ProbationIncarceration.
    /// </summary>
    [JsonPropertyName("probationIncarceration")]
    public Incarceration ProbationIncarceration
    {
      get => probationIncarceration ??= new();
      set => probationIncarceration = value;
    }

    /// <summary>
    /// A value of JailIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("jailIncarcerationAddress")]
    public IncarcerationAddress JailIncarcerationAddress
    {
      get => jailIncarcerationAddress ??= new();
      set => jailIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of JailIncarceration.
    /// </summary>
    [JsonPropertyName("jailIncarceration")]
    public Incarceration JailIncarceration
    {
      get => jailIncarceration ??= new();
      set => jailIncarceration = value;
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

    private IncarcerationAddress probationIncarcerationAddress;
    private Incarceration probationIncarceration;
    private IncarcerationAddress jailIncarcerationAddress;
    private Incarceration jailIncarceration;
    private CsePerson csePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Count.
    /// </summary>
    [JsonPropertyName("count")]
    public Common Count
    {
      get => count ??= new();
      set => count = value;
    }

    /// <summary>
    /// A value of MaxDate.
    /// </summary>
    [JsonPropertyName("maxDate")]
    public Code MaxDate
    {
      get => maxDate ??= new();
      set => maxDate = value;
    }

    private Common count;
    private Code maxDate;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ProbationIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("probationIncarcerationAddress")]
    public IncarcerationAddress ProbationIncarcerationAddress
    {
      get => probationIncarcerationAddress ??= new();
      set => probationIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of ProbationIncarceration.
    /// </summary>
    [JsonPropertyName("probationIncarceration")]
    public Incarceration ProbationIncarceration
    {
      get => probationIncarceration ??= new();
      set => probationIncarceration = value;
    }

    /// <summary>
    /// A value of JailIncarcerationAddress.
    /// </summary>
    [JsonPropertyName("jailIncarcerationAddress")]
    public IncarcerationAddress JailIncarcerationAddress
    {
      get => jailIncarcerationAddress ??= new();
      set => jailIncarcerationAddress = value;
    }

    /// <summary>
    /// A value of JailIncarceration.
    /// </summary>
    [JsonPropertyName("jailIncarceration")]
    public Incarceration JailIncarceration
    {
      get => jailIncarceration ??= new();
      set => jailIncarceration = value;
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

    private IncarcerationAddress probationIncarcerationAddress;
    private Incarceration probationIncarceration;
    private IncarcerationAddress jailIncarcerationAddress;
    private Incarceration jailIncarceration;
    private CsePerson csePerson;
  }
#endregion
}

// Program: SI_RETRIEVE_AP_ID_COMPARE_INFO, ID: 372515751, model: 746.
// Short name: SWE01235
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
/// A program: SI_RETRIEVE_AP_ID_COMPARE_INFO.
/// </para>
/// <para>
/// RESP: SRVINIT
/// </para>
/// </summary>
[Serializable]
public partial class SiRetrieveApIdCompareInfo: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RETRIEVE_AP_ID_COMPARE_INFO program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRetrieveApIdCompareInfo(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRetrieveApIdCompareInfo.
  /// </summary>
  public SiRetrieveApIdCompareInfo(IContext context, Import import,
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
    //         M A I N T E N A N C E   L O G
    //  Date	 Developer   Description
    // 5-22-95  Ken Evans   Initial development
    // ---------------------------------------------
    // **************************************************************
    // 4/13/99  C. Ott   Added an action block to get the AP associated to the 
    // Interstate Case.
    // **************************************************************
    // **************************************************************
    // 8/21/99  C. Ott   Added a statement to get the AP Case Role associated to
    // the Interstate Case.
    // **************************************************************
    // **************************************************************
    // 2/01/00  C. Scroggins   Added READ EACH statements to handle cases where 
    // there is more than one AP on the case.
    // **************************************************************
    // *********************************************
    // This AB retrieves the CSE person data needed
    // for the comparison.
    // *********************************************
    export.Cse.Assign(import.Apid);
    local.ApCounter.Count = 0;

    if (!IsEmpty(import.Apid.Number))
    {
      if (ReadCsePerson2())
      {
        export.CsePerson.Assign(entities.CsePerson);
        export.Cse.Number = entities.CsePerson.Number;
      }
      else
      {
        ExitState = "CSE_PERSON_NF";

        return;
      }
    }
    else if (!IsEmpty(import.InterstateCase.KsCaseId))
    {
      // **************************************************************
      // 2/01/00  C. Scroggins   Added READ EACH statements to handle cases 
      // where there is more than one AP on the case.
      // **************************************************************
      foreach(var item in ReadCsePerson3())
      {
        ++local.ApCounter.Count;
      }

      if (local.ApCounter.Count > 1)
      {
        MoveCsePerson(local.Refresh, export.CsePerson);
        ExitState = "NUMBER_OF_APS_EXCEED_MAX";

        return;
      }

      // **************************************************************
      // 8/21/99  C. Ott   Added the following action block to get the AP Case 
      // Role associated to the Interstate Case.
      // **************************************************************
      if (ReadCsePerson1())
      {
        export.CsePerson.Assign(entities.CsePerson);
        export.Cse.Number = entities.CsePerson.Number;
      }
      else
      {
        ExitState = "CO0000_AP_CSE_PERSON_NF";

        return;
      }
    }
    else
    {
      // **************************************************************
      // 2/01/00  C. Scroggins   Added READ EACH statements to handle cases 
      // where there is more than one AP on the case.
      // **************************************************************
      foreach(var item in ReadCsePerson4())
      {
        ++local.ApCounter.Count;
      }

      if (local.ApCounter.Count > 1)
      {
        MoveCsePerson(local.Refresh, export.CsePerson);
        ExitState = "NUMBER_OF_APS_EXCEED_MAX";

        return;
      }
      else
      {
        export.CsePerson.Assign(entities.CsePerson);
        export.Cse.Number = entities.CsePerson.Number;
      }

      // **************************************************************
      // 4/13/99  C. Ott   Added the following action block to get the AP 
      // associated to the Interstate Case.
      // **************************************************************
      UseSiGetApForInterstateCase();

      if (IsExitState("CO0000_ABSENT_PARENT_NF"))
      {
        return;
      }
    }

    local.ErrOnAdabasUnava.Flag = "Y";
    UseCabReadAdabasPerson();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      return;
    }

    UseSiAltsBuildAliasAndSsn();

    for(local.Ap.Index = 0; local.Ap.Index < local.Ap.Count; ++local.Ap.Index)
    {
      if (!local.Ap.CheckSize())
      {
        break;
      }

      if (AsChar(local.Ssn.Flag) != 'Y')
      {
        MoveCsePersonsWorkSet(local.Ap.Item.GapCsePersonsWorkSet,
          export.Export1St);
        local.Ssn.Flag = "Y";
      }
      else
      {
        MoveCsePersonsWorkSet(local.Ap.Item.GapCsePersonsWorkSet,
          export.Export2Nd);

        return;
      }
    }

    local.Ap.CheckIndex();
  }

  private static void MoveAp(SiAltsBuildAliasAndSsn.Export.ApGroup source,
    Local.ApGroup target)
  {
    target.GapCommon.SelectChar = source.GapCommon.SelectChar;
    target.GapCsePersonsWorkSet.Assign(source.GapCsePersonsWorkSet);
    target.GapSsn3.Text3 = source.GapSsn3.Text3;
    target.GapSsn2.Text3 = source.GapSsn2.Text3;
    target.GapSsn4.Text4 = source.GapSsn4.Text4;
    target.GapKscares.Flag = source.GapKscares.Flag;
    target.GapKanpay.Flag = source.GapKanpay.Flag;
    target.GapCse.Flag = source.GapCse.Flag;
    target.GapAe.Flag = source.GapAe.Flag;
    target.GapDbOccurrence.Flag = source.GapDbOccurrence.Flag;
  }

  private static void MoveCsePerson(CsePerson source, CsePerson target)
  {
    target.Type1 = source.Type1;
    target.Number = source.Number;
    target.KscaresNumber = source.KscaresNumber;
    target.Occupation = source.Occupation;
    target.AeCaseNumber = source.AeCaseNumber;
    target.DateOfDeath = source.DateOfDeath;
    target.IllegalAlienIndicator = source.IllegalAlienIndicator;
    target.CurrentSpouseMi = source.CurrentSpouseMi;
    target.CurrentSpouseFirstName = source.CurrentSpouseFirstName;
    target.BirthPlaceState = source.BirthPlaceState;
    target.EmergencyPhone = source.EmergencyPhone;
    target.NameMiddle = source.NameMiddle;
    target.NameMaiden = source.NameMaiden;
    target.HomePhone = source.HomePhone;
    target.OtherNumber = source.OtherNumber;
    target.BirthPlaceCity = source.BirthPlaceCity;
    target.Weight = source.Weight;
    target.OtherIdInfo = source.OtherIdInfo;
    target.CurrentMaritalStatus = source.CurrentMaritalStatus;
    target.CurrentSpouseLastName = source.CurrentSpouseLastName;
    target.Race = source.Race;
    target.HeightFt = source.HeightFt;
    target.HeightIn = source.HeightIn;
    target.HairColor = source.HairColor;
    target.EyeColor = source.EyeColor;
    target.HomePhoneAreaCode = source.HomePhoneAreaCode;
    target.EmergencyAreaCode = source.EmergencyAreaCode;
    target.OtherAreaCode = source.OtherAreaCode;
    target.OtherPhoneType = source.OtherPhoneType;
    target.WorkPhoneAreaCode = source.WorkPhoneAreaCode;
    target.WorkPhone = source.WorkPhone;
    target.WorkPhoneExt = source.WorkPhoneExt;
  }

  private static void MoveCsePersonsWorkSet(CsePersonsWorkSet source,
    CsePersonsWorkSet target)
  {
    target.UniqueKey = source.UniqueKey;
    target.Ssn = source.Ssn;
  }

  private static void MoveInterstateCase(InterstateCase source,
    InterstateCase target)
  {
    target.TransSerialNumber = source.TransSerialNumber;
    target.TransactionDate = source.TransactionDate;
  }

  private void UseCabReadAdabasPerson()
  {
    var useImport = new CabReadAdabasPerson.Import();
    var useExport = new CabReadAdabasPerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnava.Flag;
    useImport.CsePersonsWorkSet.Number = export.Cse.Number;

    Call(CabReadAdabasPerson.Execute, useImport, useExport);

    export.Cse.Assign(useExport.CsePersonsWorkSet);
    export.OwnedByAe.Flag = useExport.Ae.Flag;
  }

  private void UseSiAltsBuildAliasAndSsn()
  {
    var useImport = new SiAltsBuildAliasAndSsn.Import();
    var useExport = new SiAltsBuildAliasAndSsn.Export();

    useImport.Ap1.Number = export.Cse.Number;

    Call(SiAltsBuildAliasAndSsn.Execute, useImport, useExport);

    useExport.Ap.CopyTo(local.Ap, MoveAp);
  }

  private void UseSiGetApForInterstateCase()
  {
    var useImport = new SiGetApForInterstateCase.Import();
    var useExport = new SiGetApForInterstateCase.Export();

    MoveInterstateCase(import.InterstateCase, useImport.InterstateCase);

    Call(SiGetApForInterstateCase.Execute, useImport, useExport);

    MoveCsePerson(useExport.CsePerson, export.CsePerson);
    export.Cse.Number = useExport.CsePersonsWorkSet.Number;
  }

  private bool ReadCsePerson1()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson1",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "startDate", date);
        db.SetNullableString(
          command, "ksCaseId", import.InterstateCase.KsCaseId ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 23);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 24);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 25);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 26);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 27);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 28);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 29);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 30);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 31);
        entities.CsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 32);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private bool ReadCsePerson2()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Apid.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 23);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 24);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 25);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 26);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 27);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 28);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 29);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 30);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 31);
        entities.CsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 32);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);
      });
  }

  private IEnumerable<bool> ReadCsePerson3()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson3",
      (db, command) =>
      {
        var date = Now().Date;

        db.SetNullableDate(command, "startDate", date);
        db.SetNullableString(
          command, "ksCaseId", import.InterstateCase.KsCaseId ?? "");
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 23);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 24);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 25);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 26);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 27);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 28);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 29);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 30);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 31);
        entities.CsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 32);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePerson4()
  {
    entities.CsePerson.Populated = false;

    return ReadEach("ReadCsePerson4",
      (db, command) =>
      {
        db.SetDate(
          command, "transactionDate",
          import.InterstateCase.TransactionDate.GetValueOrDefault());
        db.SetInt64(
          command, "transactionSerial",
          import.InterstateCase.TransSerialNumber);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Type1 = db.GetString(reader, 1);
        entities.CsePerson.Occupation = db.GetNullableString(reader, 2);
        entities.CsePerson.AeCaseNumber = db.GetNullableString(reader, 3);
        entities.CsePerson.DateOfDeath = db.GetNullableDate(reader, 4);
        entities.CsePerson.IllegalAlienIndicator =
          db.GetNullableString(reader, 5);
        entities.CsePerson.CurrentSpouseMi = db.GetNullableString(reader, 6);
        entities.CsePerson.CurrentSpouseFirstName =
          db.GetNullableString(reader, 7);
        entities.CsePerson.BirthPlaceState = db.GetNullableString(reader, 8);
        entities.CsePerson.EmergencyPhone = db.GetNullableInt32(reader, 9);
        entities.CsePerson.NameMiddle = db.GetNullableString(reader, 10);
        entities.CsePerson.NameMaiden = db.GetNullableString(reader, 11);
        entities.CsePerson.HomePhone = db.GetNullableInt32(reader, 12);
        entities.CsePerson.OtherNumber = db.GetNullableInt32(reader, 13);
        entities.CsePerson.BirthPlaceCity = db.GetNullableString(reader, 14);
        entities.CsePerson.CurrentMaritalStatus =
          db.GetNullableString(reader, 15);
        entities.CsePerson.CurrentSpouseLastName =
          db.GetNullableString(reader, 16);
        entities.CsePerson.Race = db.GetNullableString(reader, 17);
        entities.CsePerson.HairColor = db.GetNullableString(reader, 18);
        entities.CsePerson.EyeColor = db.GetNullableString(reader, 19);
        entities.CsePerson.Weight = db.GetNullableInt32(reader, 20);
        entities.CsePerson.HeightFt = db.GetNullableInt32(reader, 21);
        entities.CsePerson.HeightIn = db.GetNullableInt32(reader, 22);
        entities.CsePerson.KscaresNumber = db.GetNullableString(reader, 23);
        entities.CsePerson.OtherAreaCode = db.GetNullableInt32(reader, 24);
        entities.CsePerson.EmergencyAreaCode = db.GetNullableInt32(reader, 25);
        entities.CsePerson.HomePhoneAreaCode = db.GetNullableInt32(reader, 26);
        entities.CsePerson.WorkPhoneAreaCode = db.GetNullableInt32(reader, 27);
        entities.CsePerson.WorkPhone = db.GetNullableInt32(reader, 28);
        entities.CsePerson.WorkPhoneExt = db.GetNullableString(reader, 29);
        entities.CsePerson.OtherPhoneType = db.GetNullableString(reader, 30);
        entities.CsePerson.OtherIdInfo = db.GetNullableString(reader, 31);
        entities.CsePerson.TextMessageIndicator =
          db.GetNullableString(reader, 32);
        entities.CsePerson.Populated = true;
        CheckValid<CsePerson>("Type1", entities.CsePerson.Type1);

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
    /// A value of Apid.
    /// </summary>
    [JsonPropertyName("apid")]
    public CsePersonsWorkSet Apid
    {
      get => apid ??= new();
      set => apid = value;
    }

    /// <summary>
    /// A value of InterstateCase.
    /// </summary>
    [JsonPropertyName("interstateCase")]
    public InterstateCase InterstateCase
    {
      get => interstateCase ??= new();
      set => interstateCase = value;
    }

    private CsePersonsWorkSet apid;
    private InterstateCase interstateCase;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of Export2Nd.
    /// </summary>
    [JsonPropertyName("export2Nd")]
    public CsePersonsWorkSet Export2Nd
    {
      get => export2Nd ??= new();
      set => export2Nd = value;
    }

    /// <summary>
    /// A value of Export1St.
    /// </summary>
    [JsonPropertyName("export1St")]
    public CsePersonsWorkSet Export1St
    {
      get => export1St ??= new();
      set => export1St = value;
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
    /// A value of Cse.
    /// </summary>
    [JsonPropertyName("cse")]
    public CsePersonsWorkSet Cse
    {
      get => cse ??= new();
      set => cse = value;
    }

    /// <summary>
    /// A value of OwnedByAe.
    /// </summary>
    [JsonPropertyName("ownedByAe")]
    public Common OwnedByAe
    {
      get => ownedByAe ??= new();
      set => ownedByAe = value;
    }

    private CsePersonsWorkSet export2Nd;
    private CsePersonsWorkSet export1St;
    private CsePerson csePerson;
    private CsePersonsWorkSet cse;
    private Common ownedByAe;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>A ApGroup group.</summary>
    [Serializable]
    public class ApGroup
    {
      /// <summary>
      /// A value of GapCommon.
      /// </summary>
      [JsonPropertyName("gapCommon")]
      public Common GapCommon
      {
        get => gapCommon ??= new();
        set => gapCommon = value;
      }

      /// <summary>
      /// A value of GapCsePersonsWorkSet.
      /// </summary>
      [JsonPropertyName("gapCsePersonsWorkSet")]
      public CsePersonsWorkSet GapCsePersonsWorkSet
      {
        get => gapCsePersonsWorkSet ??= new();
        set => gapCsePersonsWorkSet = value;
      }

      /// <summary>
      /// A value of GapSsn3.
      /// </summary>
      [JsonPropertyName("gapSsn3")]
      public WorkArea GapSsn3
      {
        get => gapSsn3 ??= new();
        set => gapSsn3 = value;
      }

      /// <summary>
      /// A value of GapSsn2.
      /// </summary>
      [JsonPropertyName("gapSsn2")]
      public WorkArea GapSsn2
      {
        get => gapSsn2 ??= new();
        set => gapSsn2 = value;
      }

      /// <summary>
      /// A value of GapSsn4.
      /// </summary>
      [JsonPropertyName("gapSsn4")]
      public WorkArea GapSsn4
      {
        get => gapSsn4 ??= new();
        set => gapSsn4 = value;
      }

      /// <summary>
      /// A value of GapKscares.
      /// </summary>
      [JsonPropertyName("gapKscares")]
      public Common GapKscares
      {
        get => gapKscares ??= new();
        set => gapKscares = value;
      }

      /// <summary>
      /// A value of GapKanpay.
      /// </summary>
      [JsonPropertyName("gapKanpay")]
      public Common GapKanpay
      {
        get => gapKanpay ??= new();
        set => gapKanpay = value;
      }

      /// <summary>
      /// A value of GapCse.
      /// </summary>
      [JsonPropertyName("gapCse")]
      public Common GapCse
      {
        get => gapCse ??= new();
        set => gapCse = value;
      }

      /// <summary>
      /// A value of GapAe.
      /// </summary>
      [JsonPropertyName("gapAe")]
      public Common GapAe
      {
        get => gapAe ??= new();
        set => gapAe = value;
      }

      /// <summary>
      /// A value of GapDbOccurrence.
      /// </summary>
      [JsonPropertyName("gapDbOccurrence")]
      public Common GapDbOccurrence
      {
        get => gapDbOccurrence ??= new();
        set => gapDbOccurrence = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 6;

      private Common gapCommon;
      private CsePersonsWorkSet gapCsePersonsWorkSet;
      private WorkArea gapSsn3;
      private WorkArea gapSsn2;
      private WorkArea gapSsn4;
      private Common gapKscares;
      private Common gapKanpay;
      private Common gapCse;
      private Common gapAe;
      private Common gapDbOccurrence;
    }

    /// <summary>
    /// A value of Refresh.
    /// </summary>
    [JsonPropertyName("refresh")]
    public CsePerson Refresh
    {
      get => refresh ??= new();
      set => refresh = value;
    }

    /// <summary>
    /// A value of ApCounter.
    /// </summary>
    [JsonPropertyName("apCounter")]
    public Common ApCounter
    {
      get => apCounter ??= new();
      set => apCounter = value;
    }

    /// <summary>
    /// A value of Ssn.
    /// </summary>
    [JsonPropertyName("ssn")]
    public Common Ssn
    {
      get => ssn ??= new();
      set => ssn = value;
    }

    /// <summary>
    /// Gets a value of Ap.
    /// </summary>
    [JsonIgnore]
    public Array<ApGroup> Ap => ap ??= new(ApGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ap for json serialization.
    /// </summary>
    [JsonPropertyName("ap")]
    [Computed]
    public IList<ApGroup> Ap_Json
    {
      get => ap;
      set => Ap.Assign(value);
    }

    /// <summary>
    /// A value of ErrOnAdabasUnava.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnava")]
    public Common ErrOnAdabasUnava
    {
      get => errOnAdabasUnava ??= new();
      set => errOnAdabasUnava = value;
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

    private CsePerson refresh;
    private Common apCounter;
    private Common ssn;
    private Array<ApGroup> ap;
    private Common errOnAdabasUnava;
    private AbendData abendData;
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
    /// A value of InterstateRequest.
    /// </summary>
    [JsonPropertyName("interstateRequest")]
    public InterstateRequest InterstateRequest
    {
      get => interstateRequest ??= new();
      set => interstateRequest = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
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

    private InterstateRequestHistory interstateRequestHistory;
    private InterstateRequest interstateRequest;
    private CaseRole absentParent;
    private CaseRole caseRole;
    private Case1 case1;
    private CsePerson csePerson;
  }
#endregion
}

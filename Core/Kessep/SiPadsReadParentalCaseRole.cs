// Program: SI_PADS_READ_PARENTAL_CASE_ROLE, ID: 371758852, model: 746.
// Short name: SWE01232
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SI_PADS_READ_PARENTAL_CASE_ROLE.
/// </para>
/// <para>
/// RESP: SRVINIT
/// This process reads the case role details for either a mother or a father 
/// depending upon the case role passed to it.
/// </para>
/// </summary>
[Serializable]
public partial class SiPadsReadParentalCaseRole: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_PADS_READ_PARENTAL_CASE_ROLE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiPadsReadParentalCaseRole(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiPadsReadParentalCaseRole.
  /// </summary>
  public SiPadsReadParentalCaseRole(IContext context, Import import,
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
    // ------------------------------------------------------------
    //                    M A I N T E N A N C E    L O G
    // Date	  Developer		Description
    // 04-24-95  Helen Sharland - MTW	Init Dev
    // 09/26/96  G. Lofton		Add county code views
    // 04/30/97  JF.Caillouet		Change Current Date
    // 06/05/97  Sid Chowdhary		Cleanup and FIxes.
    // ------------------------------------------------------------
    // 01/14/99 W.Campbell            Some code disabled and
    //                                
    // some new code inserted in order
    //                                
    // to use new logic to obtain
    //                                
    // the CSE_PERSON_ADDRESS.
    // ------------------------------------------------------
    // 06/22/99 W.Campbell            Modified the properties
    //                                
    // of 2 READ statements to
    //                                
    // Select Only.
    // ---------------------------------------------------------
    UseCabSetMaximumDiscontinueDate();
    local.Current.Date = Now().Date;
    local.ErrOnAdabasUnavailable.Flag = "Y";

    // ---------------------------------------------------------
    // 06/22/99 W.Campbell - Modified the properties
    // of the following READ statement to Select Only.
    // ---------------------------------------------------------
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (AsChar(entities.Case1.Status) == 'O')
    {
      // ---------------------------------------------------------
      // 06/22/99 W.Campbell - Modified the properties
      // of the following READ statement to Select Only.
      // ---------------------------------------------------------
      if (ReadCaseRoleCsePerson2())
      {
        export.CaseRole.Assign(entities.CaseRole);

        if (Equal(entities.CaseRole.EndDate, local.Max.Date))
        {
          export.CaseRole.EndDate = null;
        }

        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePerson();

        if (IsExitState("ACO_NN0000_ALL_OK") || IsExitState
          ("PARENT_IS_AR_OR_AP") || IsExitState("NO_APS_ON_A_CASE"))
        {
        }
        else
        {
          return;
        }

        export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
      }
      else
      {
        return;
      }
    }
    else if (AsChar(entities.Case1.Status) == 'C')
    {
      if (ReadCaseRoleCsePerson1())
      {
        export.CaseRole.Assign(entities.CaseRole);

        if (Equal(entities.CaseRole.EndDate, local.Max.Date))
        {
          export.CaseRole.EndDate = null;
        }

        local.CsePersonsWorkSet.Number = entities.CsePerson.Number;
        UseSiReadCsePerson();
        export.CsePersonsWorkSet.Assign(local.CsePersonsWorkSet);
      }
    }

    // --------------------------------------------
    // Read Parent's current mailing address.
    // --------------------------------------------
    // ------------------------------------------------------
    // 01/14/99 W.Campbell - Following code disabled in order
    // to use new logic to obtain the CSE_PERSON_ADDRESS.
    // ------------------------------------------------------
    // ------------------------------------------------------
    // 01/14/99 W.Campbell - Following code inserted
    // to use new logic to obtain the CSE_PERSON_ADDRESS.
    // ------------------------------------------------------
    local.CsePerson.Number = export.CsePersonsWorkSet.Number;
    UseSiGetCsePersonMailingAddr();

    if (AsChar(export.CsePersonAddress.LocationType) == 'F')
    {
      // ------------------------------------------------------
      // If the returned address is 'F'oreign, then
      // must blank out the export (returned) address.
      // ------------------------------------------------------
      MoveCsePersonAddress(local.Blank, export.CsePersonAddress);
    }

    // ------------------------------------------------------
    // 01/14/99 W.Campbell - End of code inserted
    // to use new logic to obtain the CSE_PERSON_ADDRESS.
    // ------------------------------------------------------
  }

  private static void MoveCsePersonAddress(CsePersonAddress source,
    CsePersonAddress target)
  {
    target.LocationType = source.LocationType;
    target.LastUpdatedTimestamp = source.LastUpdatedTimestamp;
    target.LastUpdatedBy = source.LastUpdatedBy;
    target.CreatedTimestamp = source.CreatedTimestamp;
    target.CreatedBy = source.CreatedBy;
    target.SendDate = source.SendDate;
    target.Source = source.Source;
    target.Identifier = source.Identifier;
    target.Street1 = source.Street1;
    target.Street2 = source.Street2;
    target.City = source.City;
    target.Type1 = source.Type1;
    target.WorkerId = source.WorkerId;
    target.VerifiedDate = source.VerifiedDate;
    target.EndDate = source.EndDate;
    target.EndCode = source.EndCode;
    target.County = source.County;
    target.State = source.State;
    target.ZipCode = source.ZipCode;
    target.Zip4 = source.Zip4;
    target.Zip3 = source.Zip3;
  }

  private void UseCabSetMaximumDiscontinueDate()
  {
    var useImport = new CabSetMaximumDiscontinueDate.Import();
    var useExport = new CabSetMaximumDiscontinueDate.Export();

    Call(CabSetMaximumDiscontinueDate.Execute, useImport, useExport);

    local.Max.Date = useExport.DateWorkArea.Date;
  }

  private void UseSiGetCsePersonMailingAddr()
  {
    var useImport = new SiGetCsePersonMailingAddr.Import();
    var useExport = new SiGetCsePersonMailingAddr.Export();

    useImport.CsePerson.Number = local.CsePerson.Number;

    Call(SiGetCsePersonMailingAddr.Execute, useImport, useExport);

    MoveCsePersonAddress(useExport.CsePersonAddress, export.CsePersonAddress);
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.ErrOnAdabasUnavailable.Flag = local.ErrOnAdabasUnavailable.Flag;
    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
    export.AbendData.Assign(useExport.AbendData);
    export.Ae.Flag = useExport.Ae.Flag;
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
        entities.Case1.Number = db.GetString(reader, 0);
        entities.Case1.Status = db.GetNullableString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadCaseRoleCsePerson1()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "endDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.ConfirmedType = db.GetNullableString(reader, 9);
        entities.CaseRole.Note = db.GetNullableString(reader, 10);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
      });
  }

  private bool ReadCaseRoleCsePerson2()
  {
    entities.CsePerson.Populated = false;
    entities.CaseRole.Populated = false;

    return Read("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "type", import.CaseRole.Type1);
        db.SetNullableDate(
          command, "startDate", local.Current.Date.GetValueOrDefault());
        db.SetString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.CaseRole.CasNumber = db.GetString(reader, 0);
        entities.CaseRole.CspNumber = db.GetString(reader, 1);
        entities.CsePerson.Number = db.GetString(reader, 1);
        entities.CaseRole.Type1 = db.GetString(reader, 2);
        entities.CaseRole.Identifier = db.GetInt32(reader, 3);
        entities.CaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.CaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.CaseRole.OnSsInd = db.GetNullableString(reader, 6);
        entities.CaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 7);
        entities.CaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 8);
        entities.CaseRole.ConfirmedType = db.GetNullableString(reader, 9);
        entities.CaseRole.Note = db.GetNullableString(reader, 10);
        entities.CsePerson.Populated = true;
        entities.CaseRole.Populated = true;
        CheckValid<CaseRole>("Type1", entities.CaseRole.Type1);
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

    private CaseRole caseRole;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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
    /// A value of Ae.
    /// </summary>
    [JsonPropertyName("ae")]
    public Common Ae
    {
      get => ae ??= new();
      set => ae = value;
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
    /// A value of CaseRole.
    /// </summary>
    [JsonPropertyName("caseRole")]
    public CaseRole CaseRole
    {
      get => caseRole ??= new();
      set => caseRole = value;
    }

    private CsePersonAddress csePersonAddress;
    private AbendData abendData;
    private Common ae;
    private CsePersonsWorkSet csePersonsWorkSet;
    private CaseRole caseRole;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Blank.
    /// </summary>
    [JsonPropertyName("blank")]
    public CsePersonAddress Blank
    {
      get => blank ??= new();
      set => blank = value;
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
    /// A value of Current.
    /// </summary>
    [JsonPropertyName("current")]
    public DateWorkArea Current
    {
      get => current ??= new();
      set => current = value;
    }

    /// <summary>
    /// A value of ErrOnAdabasUnavailable.
    /// </summary>
    [JsonPropertyName("errOnAdabasUnavailable")]
    public Common ErrOnAdabasUnavailable
    {
      get => errOnAdabasUnavailable ??= new();
      set => errOnAdabasUnavailable = value;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    private CsePersonAddress blank;
    private CsePerson csePerson;
    private DateWorkArea current;
    private Common errOnAdabasUnavailable;
    private DateWorkArea max;
    private CsePersonsWorkSet csePersonsWorkSet;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of CsePersonAddress.
    /// </summary>
    [JsonPropertyName("csePersonAddress")]
    public CsePersonAddress CsePersonAddress
    {
      get => csePersonAddress ??= new();
      set => csePersonAddress = value;
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

    private CsePersonAddress csePersonAddress;
    private CsePerson csePerson;
    private CaseRole caseRole;
    private Case1 case1;
  }
#endregion
}

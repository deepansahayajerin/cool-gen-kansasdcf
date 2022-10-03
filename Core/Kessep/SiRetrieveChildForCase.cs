// Program: SI_RETRIEVE_CHILD_FOR_CASE, ID: 371736656, model: 746.
// Short name: SWE01237
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
/// A program: SI_RETRIEVE_CHILD_FOR_CASE.
/// </para>
/// <para>
/// RESP: SRVINIT		
/// This process reads a given case and then returns a child if only one exists.
/// If more than one exists, it will set an indicator.
/// </para>
/// </summary>
[Serializable]
public partial class SiRetrieveChildForCase: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RETRIEVE_CHILD_FOR_CASE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRetrieveChildForCase(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRetrieveChildForCase.
  /// </summary>
  public SiRetrieveChildForCase(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // -------------------------------------------------------------------
    // 		M A I N T E N A N C E   L O G
    // Date		Developer		Request #	Description
    // 04-23-96	G. LOFTON - MTW			0	Initial Dev
    // 06/09/97	Sid		Cleanup and Fixes.
    // 11/10/98  C Deghand             Added read each to populate person
    //                                 
    // number for an inactive child.
    // -------------------------------------------------------------------
    // 06/25/99 W.Campbell       Added an IF statement
    //                           so that the READ EACH for
    //                           inactive CH only occurrs
    //                           if there are no active CH.
    // -------------------------------------------------------------------
    // 11/23/99 M.Lachowicz      Changed logic of the CAB
    //                           to detect multiple children
    //                           on case.
    //                           PR #80438.
    // -------------------------------------------------------------------
    // ---------------------------------------------
    // Check to see how many active CH's are on a
    // case.  If only one, read that one else set an
    // indicator to make the procedure flow to Case
    // Composition.
    // ---------------------------------------------
    export.MultipleChildren.Flag = "N";
    local.Common.Count = 0;
    local.Current.Date = Now().Date;

    // 11/23/99 M.L Start
    export.ActiveChild.Flag = "N";

    foreach(var item in ReadCsePersonCaseRole())
    {
      if (!Lt(entities.ChildCaseRole.EndDate, local.Current.Date) && !
        Lt(local.Current.Date, entities.ChildCaseRole.StartDate))
      {
        export.ActiveChild.Flag = "Y";
      }

      ++local.Common.Count;

      if (local.Common.Count == 1)
      {
        local.Save.Number = entities.ChildCsePerson.Number;
        export.Child.Number = entities.ChildCsePerson.Number;
      }
      else if (!Equal(local.Save.Number, entities.ChildCsePerson.Number))
      {
        export.MultipleChildren.Flag = "Y";

        return;
      }
    }

    // 11/23/99 M.L End
  }

  private IEnumerable<bool> ReadCsePersonCaseRole()
  {
    entities.ChildCsePerson.Populated = false;
    entities.ChildCaseRole.Populated = false;

    return ReadEach("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ChildCsePerson.Number = db.GetString(reader, 0);
        entities.ChildCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ChildCsePerson.Type1 = db.GetString(reader, 1);
        entities.ChildCsePerson.OrganizationName =
          db.GetNullableString(reader, 2);
        entities.ChildCaseRole.CasNumber = db.GetString(reader, 3);
        entities.ChildCaseRole.Type1 = db.GetString(reader, 4);
        entities.ChildCaseRole.Identifier = db.GetInt32(reader, 5);
        entities.ChildCaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.ChildCaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.ChildCaseRole.OnSsInd = db.GetNullableString(reader, 8);
        entities.ChildCaseRole.HealthInsuranceIndicator =
          db.GetNullableString(reader, 9);
        entities.ChildCaseRole.MedicalSupportIndicator =
          db.GetNullableString(reader, 10);
        entities.ChildCaseRole.AbsenceReasonCode =
          db.GetNullableString(reader, 11);
        entities.ChildCaseRole.PriorMedicalSupport =
          db.GetNullableDecimal(reader, 12);
        entities.ChildCaseRole.ArWaivedInsurance =
          db.GetNullableString(reader, 13);
        entities.ChildCaseRole.DateOfEmancipation =
          db.GetNullableDate(reader, 14);
        entities.ChildCaseRole.FcAdoptionDisruptionInd =
          db.GetNullableString(reader, 15);
        entities.ChildCaseRole.FcApNotified = db.GetNullableString(reader, 16);
        entities.ChildCaseRole.FcCincInd = db.GetNullableString(reader, 17);
        entities.ChildCaseRole.FcCostOfCare = db.GetNullableDecimal(reader, 18);
        entities.ChildCaseRole.FcCostOfCareFreq =
          db.GetNullableString(reader, 19);
        entities.ChildCaseRole.FcCountyChildRemovedFrom =
          db.GetNullableString(reader, 20);
        entities.ChildCaseRole.FcDateOfInitialCustody =
          db.GetNullableDate(reader, 21);
        entities.ChildCaseRole.FcInHomeServiceInd =
          db.GetNullableString(reader, 22);
        entities.ChildCaseRole.FcIvECaseNumber =
          db.GetNullableString(reader, 23);
        entities.ChildCaseRole.FcJuvenileCourtOrder =
          db.GetNullableString(reader, 24);
        entities.ChildCaseRole.FcJuvenileOffenderInd =
          db.GetNullableString(reader, 25);
        entities.ChildCaseRole.FcLevelOfCare = db.GetNullableString(reader, 26);
        entities.ChildCaseRole.FcNextJuvenileCtDt =
          db.GetNullableDate(reader, 27);
        entities.ChildCaseRole.FcOrderEstBy = db.GetNullableString(reader, 28);
        entities.ChildCaseRole.FcOtherBenefitInd =
          db.GetNullableString(reader, 29);
        entities.ChildCaseRole.FcParentalRights =
          db.GetNullableString(reader, 30);
        entities.ChildCaseRole.FcPrevPayeeFirstName =
          db.GetNullableString(reader, 31);
        entities.ChildCaseRole.FcPrevPayeeMiddleInitial =
          db.GetNullableString(reader, 32);
        entities.ChildCaseRole.FcPlacementDate = db.GetNullableDate(reader, 33);
        entities.ChildCaseRole.FcPlacementName =
          db.GetNullableString(reader, 34);
        entities.ChildCaseRole.FcPlacementReason =
          db.GetNullableString(reader, 35);
        entities.ChildCaseRole.FcPreviousPa = db.GetNullableString(reader, 36);
        entities.ChildCaseRole.FcPreviousPayeeLastName =
          db.GetNullableString(reader, 37);
        entities.ChildCaseRole.FcSourceOfFunding =
          db.GetNullableString(reader, 38);
        entities.ChildCaseRole.FcSrsPayee = db.GetNullableString(reader, 39);
        entities.ChildCaseRole.FcSsa = db.GetNullableString(reader, 40);
        entities.ChildCaseRole.FcSsi = db.GetNullableString(reader, 41);
        entities.ChildCaseRole.FcVaInd = db.GetNullableString(reader, 42);
        entities.ChildCaseRole.FcWardsAccount =
          db.GetNullableString(reader, 43);
        entities.ChildCaseRole.FcZebInd = db.GetNullableString(reader, 44);
        entities.ChildCaseRole.Over18AndInSchool =
          db.GetNullableString(reader, 45);
        entities.ChildCaseRole.ResidesWithArIndicator =
          db.GetNullableString(reader, 46);
        entities.ChildCaseRole.SpecialtyArea = db.GetNullableString(reader, 47);
        entities.ChildCaseRole.LastUpdatedTimestamp =
          db.GetNullableDateTime(reader, 48);
        entities.ChildCaseRole.LastUpdatedBy = db.GetNullableString(reader, 49);
        entities.ChildCaseRole.CreatedTimestamp = db.GetDateTime(reader, 50);
        entities.ChildCaseRole.CreatedBy = db.GetString(reader, 51);
        entities.ChildCaseRole.RelToAr = db.GetNullableString(reader, 52);
        entities.ChildCaseRole.Note = db.GetNullableString(reader, 53);
        entities.ChildCsePerson.Populated = true;
        entities.ChildCaseRole.Populated = true;
        CheckValid<CsePerson>("Type1", entities.ChildCsePerson.Type1);
        CheckValid<CaseRole>("Type1", entities.ChildCaseRole.Type1);
        CheckValid<CaseRole>("ResidesWithArIndicator",
          entities.ChildCaseRole.ResidesWithArIndicator);
        CheckValid<CaseRole>("SpecialtyArea",
          entities.ChildCaseRole.SpecialtyArea);

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
    /// A value of CaseOpen.
    /// </summary>
    [JsonPropertyName("caseOpen")]
    public Common CaseOpen
    {
      get => caseOpen ??= new();
      set => caseOpen = value;
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

    private Common caseOpen;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ActiveChild.
    /// </summary>
    [JsonPropertyName("activeChild")]
    public Common ActiveChild
    {
      get => activeChild ??= new();
      set => activeChild = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePersonsWorkSet Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of MultipleChildren.
    /// </summary>
    [JsonPropertyName("multipleChildren")]
    public Common MultipleChildren
    {
      get => multipleChildren ??= new();
      set => multipleChildren = value;
    }

    private Common activeChild;
    private CsePersonsWorkSet child;
    private Common multipleChildren;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Save.
    /// </summary>
    [JsonPropertyName("save")]
    public CsePerson Save
    {
      get => save ??= new();
      set => save = value;
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
    /// A value of Common.
    /// </summary>
    [JsonPropertyName("common")]
    public Common Common
    {
      get => common ??= new();
      set => common = value;
    }

    private CsePerson save;
    private DateWorkArea current;
    private Common common;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ChildCaseRole.
    /// </summary>
    [JsonPropertyName("childCaseRole")]
    public CaseRole ChildCaseRole
    {
      get => childCaseRole ??= new();
      set => childCaseRole = value;
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

    private CsePerson childCsePerson;
    private CaseRole childCaseRole;
    private Case1 case1;
  }
#endregion
}

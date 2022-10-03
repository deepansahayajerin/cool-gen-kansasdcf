// Program: SI_RMAD_FOR_CH_WITH_DIFF_AR, ID: 373475627, model: 746.
// Short name: SWE02104
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_RMAD_FOR_CH_WITH_DIFF_AR.
/// </summary>
[Serializable]
public partial class SiRmadForChWithDiffAr: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_RMAD_FOR_CH_WITH_DIFF_AR program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiRmadForChWithDiffAr(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiRmadForChWithDiffAr.
  /// </summary>
  public SiRmadForChWithDiffAr(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ----------------------------------------------------------------------------------------------------
    // The purpose of this CAB is to perform a post mortem check to determine if
    // there are timeframes
    // when the children on the case are active on other cases with other ARs.  
    // This is a post
    // mortem edit because by the time this cab is called, RMAD has already done
    // the database
    // updates.  If this cab finds an error in the timeframes then RMAD will 
    // rollback those updates.
    // ----------------------------------------------------------------------------------------------------
    // ----------------------------------------------------------------------------------------------------
    //                                
    // M A I N T E N A N C E   L O G
    // ----------------------------------------------------------------------------------------------------
    // Date      Developer	Request #	Description
    // --------  ----------	---------	
    // ------------------------------------------------------------
    // 11/13/02  Marek Lachowicz		Initial Code
    // 02/06/03  GVandy	PR169107	Restructured, commented, and corrected logic 
    // which identifies
    // 					overlaps.
    // 02/06/03  GVandy	PR 168472	Modify to alleviate the following scenario:  
    // The case
    // 					contains 1 or more additional children who are on cases not
    // 					in common with the child listed in the header on RMAD.
    // 					If there is an overlap with the dates of one of these non
    // 					common cases the user cannot fix the dates because they
    // 					don't appear on the screen.  In this scenario we want to
    // 					allow the save to continue but give a message indicating that
    // 					the user should check the dates on other cases for each child.
    // 05/14/03  GVandy	PR174876	Modify edit for overlapping effective dates of 
    // children
    // 					on other cases.
    // ----------------------------------------------------------------------------------------------------
    // --  Check each child on the case to determine if they are active on a 
    // different case with a
    //     different AR during the same time frames they are active on this 
    // case.
    // --  Find each child on the case.
    foreach(var item in ReadCsePersonCaseRole())
    {
      // --  Find the AR(s) on this case which were active during the time the 
      // childs case role was
      //     active.  Please insure that you have a good understanding how the 
      // remaining logic is working
      //     before attempting to make any changes!
      foreach(var item1 in ReadCaseRoleCsePerson1())
      {
        // -- Determine the start date of the AR and Child relationship.  (The 
        // start date of the relationship
        //    is the larger of the two start dates.)
        if (Lt(entities.ExistingChCaseRole.StartDate,
          entities.ExistingArCaseRole.StartDate))
        {
          local.ArChildRelationship.StartDate =
            entities.ExistingArCaseRole.StartDate;
        }
        else
        {
          local.ArChildRelationship.StartDate =
            entities.ExistingChCaseRole.StartDate;
        }

        // -- Determine the end date of the AR and Child relationship.  (The end
        // date of the relationship
        //    is the smaller of the two end dates.)
        if (Lt(entities.ExistingArCaseRole.EndDate,
          entities.ExistingChCaseRole.EndDate))
        {
          local.ArChildRelationship.EndDate =
            entities.ExistingArCaseRole.EndDate;
        }
        else
        {
          local.ArChildRelationship.EndDate =
            entities.ExistingChCaseRole.EndDate;
        }

        // -- Find other cases where the child is active during the same 
        // timeframe.
        foreach(var item2 in ReadCaseCaseRoleCsePerson())
        {
          // -- Determine if there was a different AR on this other case that 
          // was also active during the timeframe.
          // Note that this AR role must be active during both the timeframe 
          // from the original case and
          // the timeframe for which the child is active on this particular 
          // case.
          foreach(var item3 in ReadCaseRoleCsePerson2())
          {
            // --  This AR overlaps with the AR on the original case.
            // -- 02/03/03 GVandy PR 168472  The import CSE_Person view contains
            // the childs person
            // number displayed in the header of the RMAD screen.  Check if that
            // child is on this particular
            // case.  If not, allow the save to continue but set a flag so that 
            // the 'successful update' message
            // will indicate that the user needs to check other cases for the 
            // other children on the case.
            // -- 05/14/03  GVandy PR174876 Insure that the person has a 'CH' 
            // role on the other case.
            if (ReadCaseRole())
            {
              // -- Move the existing child person number and case role 
              // identifier to the export views.
              // This information will be used by the PrAD to determine which 
              // row to highlight as being in error.
              export.ChCsePerson.Number = entities.ExistingChCsePerson.Number;
              export.ChCaseRole.Identifier =
                entities.ExistingChCaseRole.Identifier;
              ExitState = "CASE_EXIST_FOR_CH_AND_DIFF_AR";

              return;
            }

            export.CheckCasesForOtherKid.Flag = "Y";
          }
        }
      }
    }
  }

  private IEnumerable<bool> ReadCaseCaseRoleCsePerson()
  {
    entities.OtherChCaseRole.Populated = false;
    entities.Other.Populated = false;
    entities.OtherChCsePerson.Populated = false;

    return ReadEach("ReadCaseCaseRoleCsePerson",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          local.ArChildRelationship.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          local.ArChildRelationship.StartDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ExistingChCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.Other.Number = db.GetString(reader, 0);
        entities.OtherChCaseRole.CasNumber = db.GetString(reader, 0);
        entities.OtherChCaseRole.CspNumber = db.GetString(reader, 1);
        entities.OtherChCsePerson.Number = db.GetString(reader, 1);
        entities.OtherChCaseRole.Type1 = db.GetString(reader, 2);
        entities.OtherChCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.OtherChCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.OtherChCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.OtherChCaseRole.Populated = true;
        entities.Other.Populated = true;
        entities.OtherChCsePerson.Populated = true;

        return true;
      });
  }

  private bool ReadCaseRole()
  {
    entities.IsKidOnSameCaseCaseRole.Populated = false;

    return Read("ReadCaseRole",
      (db, command) =>
      {
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetString(command, "casNumber", entities.Other.Number);
      },
      (db, reader) =>
      {
        entities.IsKidOnSameCaseCaseRole.CasNumber = db.GetString(reader, 0);
        entities.IsKidOnSameCaseCaseRole.CspNumber = db.GetString(reader, 1);
        entities.IsKidOnSameCaseCaseRole.Type1 = db.GetString(reader, 2);
        entities.IsKidOnSameCaseCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.IsKidOnSameCaseCaseRole.Populated = true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson1()
  {
    entities.ExistingArCaseRole.Populated = false;
    entities.ExistingArCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson1",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
        db.SetNullableDate(
          command, "startDate",
          entities.ExistingChCaseRole.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate",
          entities.ExistingChCaseRole.StartDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.ExistingArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.ExistingArCsePerson.Number = db.GetString(reader, 1);
        entities.ExistingArCaseRole.Type1 = db.GetString(reader, 2);
        entities.ExistingArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.ExistingArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.ExistingArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.ExistingArCaseRole.Populated = true;
        entities.ExistingArCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCaseRoleCsePerson2()
  {
    entities.OtherArCaseRole.Populated = false;
    entities.OtherArCsePerson.Populated = false;

    return ReadEach("ReadCaseRoleCsePerson2",
      (db, command) =>
      {
        db.SetString(command, "casNumber", entities.Other.Number);
        db.SetNullableDate(
          command, "startDate1",
          local.ArChildRelationship.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate1",
          local.ArChildRelationship.StartDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "startDate2",
          entities.OtherChCaseRole.EndDate.GetValueOrDefault());
        db.SetNullableDate(
          command, "endDate2",
          entities.OtherChCaseRole.StartDate.GetValueOrDefault());
        db.SetString(command, "cspNumber", entities.ExistingArCsePerson.Number);
      },
      (db, reader) =>
      {
        entities.OtherArCaseRole.CasNumber = db.GetString(reader, 0);
        entities.OtherArCaseRole.CspNumber = db.GetString(reader, 1);
        entities.OtherArCsePerson.Number = db.GetString(reader, 1);
        entities.OtherArCaseRole.Type1 = db.GetString(reader, 2);
        entities.OtherArCaseRole.Identifier = db.GetInt32(reader, 3);
        entities.OtherArCaseRole.StartDate = db.GetNullableDate(reader, 4);
        entities.OtherArCaseRole.EndDate = db.GetNullableDate(reader, 5);
        entities.OtherArCaseRole.Populated = true;
        entities.OtherArCsePerson.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadCsePersonCaseRole()
  {
    entities.ExistingChCaseRole.Populated = false;
    entities.ExistingChCsePerson.Populated = false;

    return ReadEach("ReadCsePersonCaseRole",
      (db, command) =>
      {
        db.SetString(command, "casNumber", import.Case1.Number);
      },
      (db, reader) =>
      {
        entities.ExistingChCsePerson.Number = db.GetString(reader, 0);
        entities.ExistingChCaseRole.CspNumber = db.GetString(reader, 0);
        entities.ExistingChCsePerson.Type1 = db.GetString(reader, 1);
        entities.ExistingChCsePerson.PaternityEstablishedIndicator =
          db.GetNullableString(reader, 2);
        entities.ExistingChCaseRole.CasNumber = db.GetString(reader, 3);
        entities.ExistingChCaseRole.Type1 = db.GetString(reader, 4);
        entities.ExistingChCaseRole.Identifier = db.GetInt32(reader, 5);
        entities.ExistingChCaseRole.StartDate = db.GetNullableDate(reader, 6);
        entities.ExistingChCaseRole.EndDate = db.GetNullableDate(reader, 7);
        entities.ExistingChCaseRole.Populated = true;
        entities.ExistingChCsePerson.Populated = true;

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
    /// A value of CsePerson.
    /// </summary>
    [JsonPropertyName("csePerson")]
    public CsePerson CsePerson
    {
      get => csePerson ??= new();
      set => csePerson = value;
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

    private CsePerson csePerson;
    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ChCaseRole.
    /// </summary>
    [JsonPropertyName("chCaseRole")]
    public CaseRole ChCaseRole
    {
      get => chCaseRole ??= new();
      set => chCaseRole = value;
    }

    /// <summary>
    /// A value of CheckCasesForOtherKid.
    /// </summary>
    [JsonPropertyName("checkCasesForOtherKid")]
    public Common CheckCasesForOtherKid
    {
      get => checkCasesForOtherKid ??= new();
      set => checkCasesForOtherKid = value;
    }

    /// <summary>
    /// A value of ChCsePerson.
    /// </summary>
    [JsonPropertyName("chCsePerson")]
    public CsePerson ChCsePerson
    {
      get => chCsePerson ??= new();
      set => chCsePerson = value;
    }

    private CaseRole chCaseRole;
    private Common checkCasesForOtherKid;
    private CsePerson chCsePerson;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ArChildRelationship.
    /// </summary>
    [JsonPropertyName("arChildRelationship")]
    public CaseRole ArChildRelationship
    {
      get => arChildRelationship ??= new();
      set => arChildRelationship = value;
    }

    private CaseRole arChildRelationship;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of IsKidOnSameCaseCsePerson.
    /// </summary>
    [JsonPropertyName("isKidOnSameCaseCsePerson")]
    public CsePerson IsKidOnSameCaseCsePerson
    {
      get => isKidOnSameCaseCsePerson ??= new();
      set => isKidOnSameCaseCsePerson = value;
    }

    /// <summary>
    /// A value of IsKidOnSameCaseCaseRole.
    /// </summary>
    [JsonPropertyName("isKidOnSameCaseCaseRole")]
    public CaseRole IsKidOnSameCaseCaseRole
    {
      get => isKidOnSameCaseCaseRole ??= new();
      set => isKidOnSameCaseCaseRole = value;
    }

    /// <summary>
    /// A value of OtherArCaseRole.
    /// </summary>
    [JsonPropertyName("otherArCaseRole")]
    public CaseRole OtherArCaseRole
    {
      get => otherArCaseRole ??= new();
      set => otherArCaseRole = value;
    }

    /// <summary>
    /// A value of OtherChCaseRole.
    /// </summary>
    [JsonPropertyName("otherChCaseRole")]
    public CaseRole OtherChCaseRole
    {
      get => otherChCaseRole ??= new();
      set => otherChCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingArCaseRole.
    /// </summary>
    [JsonPropertyName("existingArCaseRole")]
    public CaseRole ExistingArCaseRole
    {
      get => existingArCaseRole ??= new();
      set => existingArCaseRole = value;
    }

    /// <summary>
    /// A value of ExistingChCaseRole.
    /// </summary>
    [JsonPropertyName("existingChCaseRole")]
    public CaseRole ExistingChCaseRole
    {
      get => existingChCaseRole ??= new();
      set => existingChCaseRole = value;
    }

    /// <summary>
    /// A value of Other.
    /// </summary>
    [JsonPropertyName("other")]
    public Case1 Other
    {
      get => other ??= new();
      set => other = value;
    }

    /// <summary>
    /// A value of OtherArCsePerson.
    /// </summary>
    [JsonPropertyName("otherArCsePerson")]
    public CsePerson OtherArCsePerson
    {
      get => otherArCsePerson ??= new();
      set => otherArCsePerson = value;
    }

    /// <summary>
    /// A value of OtherChCsePerson.
    /// </summary>
    [JsonPropertyName("otherChCsePerson")]
    public CsePerson OtherChCsePerson
    {
      get => otherChCsePerson ??= new();
      set => otherChCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingArCsePerson.
    /// </summary>
    [JsonPropertyName("existingArCsePerson")]
    public CsePerson ExistingArCsePerson
    {
      get => existingArCsePerson ??= new();
      set => existingArCsePerson = value;
    }

    /// <summary>
    /// A value of ExistingChCsePerson.
    /// </summary>
    [JsonPropertyName("existingChCsePerson")]
    public CsePerson ExistingChCsePerson
    {
      get => existingChCsePerson ??= new();
      set => existingChCsePerson = value;
    }

    /// <summary>
    /// A value of Existing.
    /// </summary>
    [JsonPropertyName("existing")]
    public Case1 Existing
    {
      get => existing ??= new();
      set => existing = value;
    }

    private CsePerson isKidOnSameCaseCsePerson;
    private CaseRole isKidOnSameCaseCaseRole;
    private CaseRole otherArCaseRole;
    private CaseRole otherChCaseRole;
    private CaseRole existingArCaseRole;
    private CaseRole existingChCaseRole;
    private Case1 other;
    private CsePerson otherArCsePerson;
    private CsePerson otherChCsePerson;
    private CsePerson existingArCsePerson;
    private CsePerson existingChCsePerson;
    private Case1 existing;
  }
#endregion
}

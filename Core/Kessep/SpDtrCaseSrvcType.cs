// Program: SP_DTR_CASE_SRVC_TYPE, ID: 371728366, model: 746.
// Short name: SWE02046
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// <para>
/// A program: SP_DTR_CASE_SRVC_TYPE.
/// </para>
/// <para>
/// This action block reads an import Case Number and returns to a holding area 
/// a Case Unit State which contains the Case &quot;service type.&quot; This 5
/// character State is then view matched into the SI_Create_Case_Unit action
/// block.
/// </para>
/// </summary>
[Serializable]
public partial class SpDtrCaseSrvcType: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_DTR_CASE_SRVC_TYPE program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpDtrCaseSrvcType(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpDtrCaseSrvcType.
  /// </summary>
  public SpDtrCaseSrvcType(IContext context, Import import, Export export):
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
    // Date	 Developer         Description
    // ??/??/?? ?????????         Initial Development
    // ------------------------------------------------------------
    // 02/26/99 W.Campbell        Changed literal for setting of
    //                            case_unit state to 'LENUU'
    //                            from 'PENUU', when expedited
    //                            paternity = 'Y'.
    // ------------------------------------------------------------
    // 06/22/99  M. Lachowicz          Change property of READ
    //                                 
    // (Select Only or Cursor Only)
    // ------------------------------------------------------------
    // 06/22/99 M.L
    //              Change property of the following READ to generate
    //              SELECT ONLY
    if (!ReadCase())
    {
      ExitState = "CASE_NF";

      return;
    }

    if (AsChar(entities.Case1.ExpeditedPaternityInd) == 'Y')
    {
      // -------------------------------------------------------
      // 02/26/99 W.Campbell - Changed literal for setting of
      // case_unit state to 'LENUU' from 'PENUU'.
      // -------------------------------------------------------
      export.CaseUnit.State = "LENUU";
    }
    else if (AsChar(entities.Case1.FullServiceWithMedInd) == 'Y')
    {
      export.CaseUnit.State = "LFNUU";
    }
    else if (AsChar(entities.Case1.FullServiceWithoutMedInd) == 'Y')
    {
      export.CaseUnit.State = "LFNUU";
    }
    else if (AsChar(entities.Case1.LocateInd) == 'Y')
    {
      export.CaseUnit.State = "LLNUU";
    }
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
        entities.Case1.Number = db.GetString(reader, 3);
        entities.Case1.Status = db.GetNullableString(reader, 4);
        entities.Case1.StatusDate = db.GetNullableDate(reader, 5);
        entities.Case1.ExpeditedPaternityInd = db.GetNullableString(reader, 6);
        entities.Case1.Populated = true;
      });
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
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

    private Case1 case1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of CaseUnit.
    /// </summary>
    [JsonPropertyName("caseUnit")]
    public CaseUnit CaseUnit
    {
      get => caseUnit ??= new();
      set => caseUnit = value;
    }

    private CaseUnit caseUnit;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private Case1 case1;
  }
#endregion
}

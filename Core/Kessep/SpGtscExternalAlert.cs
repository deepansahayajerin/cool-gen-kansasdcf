// Program: SP_GTSC_EXTERNAL_ALERT, ID: 371797162, model: 746.
// Short name: SWE01864
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SP_GTSC_EXTERNAL_ALERT.
/// </summary>
[Serializable]
public partial class SpGtscExternalAlert: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SP_GTSC_EXTERNAL_ALERT program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SpGtscExternalAlert(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SpGtscExternalAlert.
  /// </summary>
  public SpGtscExternalAlert(IContext context, Import import, Export export):
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
    // Change Log
    // Date		Ref#	Made By		Remarks
    // 01/15/97		Raju		PA check
    // 					SQL changed
    // 8/8/97 		CHG001	Siraj Konkader	Per Kim, AR not AP should exist on AE 
    // files.
    // 01/23/98	H00034011 R Grey - Alert 44 debug
    // 02/  /98	H00034011 R Grey - delete all and rewrite action
    // 		block, remove person validations.
    // --------------------------------------------
    if (ReadCase())
    {
      if (ReadPaReferral())
      {
        local.RecFound.Text1 = "";

        if (ReadPaReferralParticipant())
        {
          local.RecFound.Text1 = "Y";
        }

        if (AsChar(local.RecFound.Text1) != 'Y')
        {
          ExitState = "PA_REFERRRAL_PARTICIPANT_NF";

          return;
        }
      }
      else
      {
        ExitState = "SI0000_CASE_NOT_FROM_REFERRAL";

        return;
      }
    }
    else
    {
      ExitState = "CASE_NF";

      return;
    }

    // --------------------------------------------------------------------
    // ALERT CODE:  44
    // ALERT TEXT:  AP NAME         1-  7
    //              CHILD NAME      8- 14
    // --------------------------------------------------------------------
    export.InterfaceAlert.AlertCode = import.InterfaceAlert.AlertCode ?? "";
    export.InterfaceAlert.CsePersonNumber = import.Ar.Number;
    local.CsePersonsWorkSet.Number = import.Ap.Number;
    UseSiReadCsePerson();

    if (IsExitState("OE0000_SCHED_RESCHED_SUCCESSFUL") || IsExitState
      ("ZD_ACO_NI0000_SUCCESSFUL_UPD_2") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      local.Temp.Value = (local.Temp.Value ?? "") + Substring
        (local.CsePersonsWorkSet.LastName, CsePersonsWorkSet.LastName_MaxLength,
        1, 5);
      local.Temp.Value = (local.Temp.Value ?? "") + " " + Substring
        (local.CsePersonsWorkSet.FirstName,
        CsePersonsWorkSet.FirstName_MaxLength, 1, 1);
    }
    else
    {
      return;
    }

    local.CsePersonsWorkSet.Number = import.Child.Number;
    UseSiReadCsePerson();

    if (IsExitState("OE0000_SCHED_RESCHED_SUCCESSFUL") || IsExitState
      ("ZD_ACO_NI0000_SUCCESSFUL_UPD_2") || IsExitState
      ("ACO_NI0000_SUCCESSFUL_UPDATE"))
    {
      local.Temp.Value = (local.Temp.Value ?? "") + Substring
        (local.CsePersonsWorkSet.LastName, CsePersonsWorkSet.LastName_MaxLength,
        1, 5);
      local.Temp.Value = (local.Temp.Value ?? "") + " " + Substring
        (local.CsePersonsWorkSet.FirstName,
        CsePersonsWorkSet.FirstName_MaxLength, 1, 1);
    }
    else
    {
      return;
    }

    export.InterfaceAlert.NoteText = local.Temp.Value ?? "";
    UseSpCreateOutgoingExtAlert();
  }

  private void UseSiReadCsePerson()
  {
    var useImport = new SiReadCsePerson.Import();
    var useExport = new SiReadCsePerson.Export();

    useImport.CsePersonsWorkSet.Number = local.CsePersonsWorkSet.Number;

    Call(SiReadCsePerson.Execute, useImport, useExport);

    local.ReadCsePerson.Type1 = useExport.AbendData.Type1;
    local.CsePersonsWorkSet.Assign(useExport.CsePersonsWorkSet);
  }

  private void UseSpCreateOutgoingExtAlert()
  {
    var useImport = new SpCreateOutgoingExtAlert.Import();
    var useExport = new SpCreateOutgoingExtAlert.Export();

    useImport.KscParticipation.Flag = local.Kscares.Flag;
    useImport.InterfaceAlert.Assign(export.InterfaceAlert);

    Call(SpCreateOutgoingExtAlert.Execute, useImport, useExport);
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
        entities.Case1.PaReferralNumber = db.GetString(reader, 1);
        entities.Case1.Populated = true;
      });
  }

  private bool ReadPaReferral()
  {
    entities.PaReferral.Populated = false;

    return Read("ReadPaReferral",
      (db, command) =>
      {
        db.SetNullableString(command, "casNumber", entities.Case1.Number);
      },
      (db, reader) =>
      {
        entities.PaReferral.Number = db.GetString(reader, 0);
        entities.PaReferral.AssignDeactivateInd =
          db.GetNullableString(reader, 1);
        entities.PaReferral.CaseNumber = db.GetNullableString(reader, 2);
        entities.PaReferral.Type1 = db.GetString(reader, 3);
        entities.PaReferral.CreatedTimestamp = db.GetDateTime(reader, 4);
        entities.PaReferral.CasNumber = db.GetNullableString(reader, 5);
        entities.PaReferral.Populated = true;
      });
  }

  private bool ReadPaReferralParticipant()
  {
    entities.PaReferralParticipant.Populated = false;

    return Read("ReadPaReferralParticipant",
      (db, command) =>
      {
        db.SetDateTime(
          command, "pafTstamp",
          entities.PaReferral.CreatedTimestamp.GetValueOrDefault());
        db.SetString(command, "preNumber", entities.PaReferral.Number);
        db.SetString(command, "pafType", entities.PaReferral.Type1);
        db.SetNullableString(command, "personNum", import.Child.Number);
      },
      (db, reader) =>
      {
        entities.PaReferralParticipant.Identifier = db.GetInt32(reader, 0);
        entities.PaReferralParticipant.PersonNumber =
          db.GetNullableString(reader, 1);
        entities.PaReferralParticipant.PreNumber = db.GetString(reader, 2);
        entities.PaReferralParticipant.PafType = db.GetString(reader, 3);
        entities.PaReferralParticipant.PafTstamp = db.GetDateTime(reader, 4);
        entities.PaReferralParticipant.Role = db.GetNullableString(reader, 5);
        entities.PaReferralParticipant.Populated = true;
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
    /// A value of Ar.
    /// </summary>
    [JsonPropertyName("ar")]
    public CsePerson Ar
    {
      get => ar ??= new();
      set => ar = value;
    }

    /// <summary>
    /// A value of Ap.
    /// </summary>
    [JsonPropertyName("ap")]
    public CsePerson Ap
    {
      get => ap ??= new();
      set => ap = value;
    }

    /// <summary>
    /// A value of Child.
    /// </summary>
    [JsonPropertyName("child")]
    public CsePerson Child
    {
      get => child ??= new();
      set => child = value;
    }

    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private Case1 case1;
    private CsePerson ar;
    private CsePerson ap;
    private CsePerson child;
    private InterfaceAlert interfaceAlert;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of InterfaceAlert.
    /// </summary>
    [JsonPropertyName("interfaceAlert")]
    public InterfaceAlert InterfaceAlert
    {
      get => interfaceAlert ??= new();
      set => interfaceAlert = value;
    }

    private InterfaceAlert interfaceAlert;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of Kscares.
    /// </summary>
    [JsonPropertyName("kscares")]
    public Common Kscares
    {
      get => kscares ??= new();
      set => kscares = value;
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
    /// A value of ReadCsePerson.
    /// </summary>
    [JsonPropertyName("readCsePerson")]
    public AbendData ReadCsePerson
    {
      get => readCsePerson ??= new();
      set => readCsePerson = value;
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
    /// A value of Temp.
    /// </summary>
    [JsonPropertyName("temp")]
    public FieldValue Temp
    {
      get => temp ??= new();
      set => temp = value;
    }

    /// <summary>
    /// A value of LastPosition.
    /// </summary>
    [JsonPropertyName("lastPosition")]
    public Common LastPosition
    {
      get => lastPosition ??= new();
      set => lastPosition = value;
    }

    /// <summary>
    /// A value of Final.
    /// </summary>
    [JsonPropertyName("final")]
    public FieldValue Final
    {
      get => final ??= new();
      set => final = value;
    }

    /// <summary>
    /// A value of RecFound.
    /// </summary>
    [JsonPropertyName("recFound")]
    public WorkArea RecFound
    {
      get => recFound ??= new();
      set => recFound = value;
    }

    private Common kscares;
    private Common ae;
    private AbendData readCsePerson;
    private CsePersonsWorkSet csePersonsWorkSet;
    private FieldValue temp;
    private Common lastPosition;
    private FieldValue final;
    private WorkArea recFound;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of PaReferralParticipant.
    /// </summary>
    [JsonPropertyName("paReferralParticipant")]
    public PaReferralParticipant PaReferralParticipant
    {
      get => paReferralParticipant ??= new();
      set => paReferralParticipant = value;
    }

    /// <summary>
    /// A value of PaReferral.
    /// </summary>
    [JsonPropertyName("paReferral")]
    public PaReferral PaReferral
    {
      get => paReferral ??= new();
      set => paReferral = value;
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

    private PaReferralParticipant paReferralParticipant;
    private PaReferral paReferral;
    private Case1 case1;
  }
#endregion
}

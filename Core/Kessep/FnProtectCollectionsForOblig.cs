// Program: FN_PROTECT_COLLECTIONS_FOR_OBLIG, ID: 373381677, model: 746.
// Short name: SWE02095
using System;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_PROTECT_COLLECTIONS_FOR_OBLIG.
/// </summary>
[Serializable]
public partial class FnProtectCollectionsForOblig: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_PROTECT_COLLECTIONS_FOR_OBLIG program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnProtectCollectionsForOblig(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnProtectCollectionsForOblig.
  /// </summary>
  public FnProtectCollectionsForOblig(IContext context, Import import,
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
    if (!import.Persistent.Populated)
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";

      return;
    }

    // IS LOCKED expression is used.
    // Entity is considered to be locked during the call.
    if (!true)
    {
      ExitState = "FN0000_OBLIGATION_NF_RB";

      return;
    }

    if (AsChar(import.CreateObCollProtHist.Flag) == 'Y')
    {
      local.CreateObCollProtHist.Flag = "Y";
    }
    else
    {
      local.CreateObCollProtHist.Flag = "N";
    }

    export.ObCollProtHistCreated.Flag = "N";
    export.CollsFndToProtect.Flag = "N";
    local.CollProtAction.Text1 = "A";

    if (AsChar(import.Persistent.PrimarySecondaryCode) == 'S')
    {
      ExitState = "FN0000_CANT_PROT_COLLS_ON_SEC_OB";

      return;
    }

    // : Support the ability to protect a range of Collection Dates.
    //   If no dates are passed in, use the first and last Collection Date for 
    // the Obligation.
    MoveObligCollProtectionHist(import.ObligCollProtectionHist,
      local.ObligCollProtectionHist);

    if (Equal(local.ObligCollProtectionHist.CvrdCollStrtDt,
      local.NullDateWorkArea.Date))
    {
      if (IsEmpty(import.ObligCollProtectionHist.ProtectionLevel))
      {
        ReadCollection8();
      }
      else
      {
        ReadCollection6();
      }

      if (Equal(local.ObligCollProtectionHist.CvrdCollStrtDt,
        local.NullDateWorkArea.Date))
      {
        return;
      }
    }
    else
    {
      if (IsEmpty(import.ObligCollProtectionHist.ProtectionLevel))
      {
        ReadCollection7();
      }
      else
      {
        ReadCollection5();
      }

      if (Equal(local.ObligCollProtectionHist.CvrdCollStrtDt,
        local.NullDateWorkArea.Date))
      {
        return;
      }
    }

    if (Equal(local.ObligCollProtectionHist.CvrdCollEndDt,
      local.NullDateWorkArea.Date))
    {
      if (IsEmpty(import.ObligCollProtectionHist.ProtectionLevel))
      {
        ReadCollection4();
      }
      else
      {
        ReadCollection2();
      }

      if (Equal(local.ObligCollProtectionHist.CvrdCollEndDt,
        local.NullDateWorkArea.Date))
      {
        return;
      }
    }
    else
    {
      if (IsEmpty(import.ObligCollProtectionHist.ProtectionLevel))
      {
        ReadCollection3();
      }
      else
      {
        ReadCollection1();
      }

      if (Equal(local.ObligCollProtectionHist.CvrdCollEndDt,
        local.NullDateWorkArea.Date))
      {
        return;
      }
    }

    if (Lt(local.ObligCollProtectionHist.CvrdCollEndDt,
      local.ObligCollProtectionHist.CvrdCollStrtDt))
    {
      return;
    }

    if (IsEmpty(local.ObligCollProtectionHist.ReasonText))
    {
      local.ObligCollProtectionHist.ReasonText =
        "COLLECTION PROTECTION HAS BEEN APPLIED TO COLLECTIONS WITHIN THE SPECIFIED DATE RANGE.";
        
    }

    UseFnProtectCollectionsForObli3();

    if (AsChar(export.ObCollProtHistCreated.Flag) == 'Y')
    {
      if (!ReadCsePerson())
      {
        ExitState = "FN0000_OBLIGOR_NF_RB";

        return;
      }

      if (ReadLegalAction())
      {
        local.LegalAction.StandardNumber =
          entities.ExistingLegalAction.StandardNumber;
      }
      else
      {
        local.LegalAction.StandardNumber = local.NullLegalAction.StandardNumber;
      }

      UseFnPrcCollProtHistAndAlrts();
    }

    if (IsEmpty(import.Persistent.PrimarySecondaryCode))
    {
      return;
    }

    if (!ReadObligation2())
    {
      if (!ReadObligation1())
      {
        ExitState = "FN0000_OBLIG_RLN_NF_RB";

        return;
      }
    }

    UseFnProtectCollectionsForObli1();

    if (AsChar(export.ObCollProtHistCreated.Flag) == 'Y')
    {
      if (!ReadCsePerson())
      {
        ExitState = "FN0000_OBLIGOR_NF_RB";

        return;
      }

      if (ReadLegalAction())
      {
        local.LegalAction.StandardNumber =
          entities.ExistingLegalAction.StandardNumber;
      }
      else
      {
        local.LegalAction.StandardNumber = local.NullLegalAction.StandardNumber;
      }

      UseFnPrcCollProtHistAndAlrts();
    }

    if (AsChar(export.CollsFndToProtect.Flag) == 'N')
    {
      export.CollsFndToProtect.Flag = local.CollsFndToProtect.Flag;
    }

    if (AsChar(export.ObCollProtHistCreated.Flag) == 'N')
    {
      export.ObCollProtHistCreated.Flag = local.ObCollProtHistCreated.Flag;
    }
  }

  private static void MoveObligCollProtectionHist(
    ObligCollProtectionHist source, ObligCollProtectionHist target)
  {
    target.CvrdCollStrtDt = source.CvrdCollStrtDt;
    target.CvrdCollEndDt = source.CvrdCollEndDt;
    target.ProtectionLevel = source.ProtectionLevel;
    target.ReasonText = source.ReasonText;
  }

  private void UseFnPrcCollProtHistAndAlrts()
  {
    var useImport = new FnPrcCollProtHistAndAlrts.Import();
    var useExport = new FnPrcCollProtHistAndAlrts.Export();

    useImport.Obligor.Number = entities.ExistingObligor1.Number;
    useImport.Obligation.SystemGeneratedIdentifier =
      import.Persistent.SystemGeneratedIdentifier;
    useImport.CollProtAction.Text1 = local.CollProtAction.Text1;
    useImport.ObligCollProtectionHist.Assign(local.ObligCollProtectionHist);
    useImport.LegalAction.StandardNumber = local.LegalAction.StandardNumber;

    Call(FnPrcCollProtHistAndAlrts.Execute, useImport, useExport);
  }

  private void UseFnProtectCollectionsForObli1()
  {
    var useImport = new FnProtectCollectionsForObli2.Import();
    var useExport = new FnProtectCollectionsForObli2.Export();

    useImport.Persistent.Assign(entities.ExistingRelated);
    useImport.ObligCollProtectionHist.Assign(local.ObligCollProtectionHist);
    useImport.CreateObCollProtHist.Flag = local.CreateObCollProtHist.Flag;

    Call(FnProtectCollectionsForObli2.Execute, useImport, useExport);

    entities.ExistingRelated.Assign(useImport.Persistent);
    local.CollsFndToProtect.Flag = useExport.CollsFndToProtect.Flag;
    local.ObCollProtHistCreated.Flag = useExport.ObCollProtHistCreated.Flag;
  }

  private void UseFnProtectCollectionsForObli3()
  {
    var useImport = new FnProtectCollectionsForObli2.Import();
    var useExport = new FnProtectCollectionsForObli2.Export();

    useImport.Persistent.Assign(import.Persistent);
    useImport.ObligCollProtectionHist.Assign(local.ObligCollProtectionHist);
    useImport.CreateObCollProtHist.Flag = local.CreateObCollProtHist.Flag;

    Call(FnProtectCollectionsForObli2.Execute, useImport, useExport);

    import.Persistent.Assign(useImport.Persistent);
    export.CollsFndToProtect.Flag = useExport.CollsFndToProtect.Flag;
    export.ObCollProtHistCreated.Flag = useExport.ObCollProtHistCreated.Flag;
  }

  private bool ReadCollection1()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    local.ObligCollProtectionHist.Populated = false;

    return Read("ReadCollection1",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetString(
          command, "appliedToCd",
          import.ObligCollProtectionHist.ProtectionLevel);
        db.SetDate(
          command, "collDt",
          local.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 0);
        local.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadCollection2()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    local.ObligCollProtectionHist.Populated = false;

    return Read("ReadCollection2",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetString(
          command, "appliedToCd",
          import.ObligCollProtectionHist.ProtectionLevel);
      },
      (db, reader) =>
      {
        local.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 0);
        local.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadCollection3()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    local.ObligCollProtectionHist.Populated = false;

    return Read("ReadCollection3",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetDate(
          command, "collDt",
          local.ObligCollProtectionHist.CvrdCollEndDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 0);
        local.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadCollection4()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    local.ObligCollProtectionHist.Populated = false;

    return Read("ReadCollection4",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
      },
      (db, reader) =>
      {
        local.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 0);
        local.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadCollection5()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    local.ObligCollProtectionHist.Populated = false;

    return Read("ReadCollection5",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetString(
          command, "appliedToCd",
          import.ObligCollProtectionHist.ProtectionLevel);
        db.SetDate(
          command, "collDt",
          local.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        local.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadCollection6()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    local.ObligCollProtectionHist.Populated = false;

    return Read("ReadCollection6",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetString(
          command, "appliedToCd",
          import.ObligCollProtectionHist.ProtectionLevel);
      },
      (db, reader) =>
      {
        local.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        local.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadCollection7()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    local.ObligCollProtectionHist.Populated = false;

    return Read("ReadCollection7",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
        db.SetDate(
          command, "collDt",
          local.ObligCollProtectionHist.CvrdCollStrtDt.GetValueOrDefault());
      },
      (db, reader) =>
      {
        local.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        local.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadCollection8()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    local.ObligCollProtectionHist.Populated = false;

    return Read("ReadCollection8",
      (db, command) =>
      {
        db.SetInt32(command, "otyId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgId", import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
      },
      (db, reader) =>
      {
        local.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        local.ObligCollProtectionHist.Populated = true;
      });
  }

  private bool ReadCsePerson()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingObligor1.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.Persistent.CspNumber);
      },
      (db, reader) =>
      {
        entities.ExistingObligor1.Number = db.GetString(reader, 0);
        entities.ExistingObligor1.Populated = true;
      });
  }

  private bool ReadLegalAction()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingLegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(
          command, "legalActionId",
          import.Persistent.LgaId.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ExistingLegalAction.Identifier = db.GetInt32(reader, 0);
        entities.ExistingLegalAction.StandardNumber =
          db.GetNullableString(reader, 1);
        entities.ExistingLegalAction.Populated = true;
      });
  }

  private bool ReadObligation1()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingRelated.Populated = false;

    return Read("ReadObligation1",
      (db, command) =>
      {
        db.SetInt32(command, "otySecondId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaType", import.Persistent.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingRelated.CpaType = db.GetString(reader, 0);
        entities.ExistingRelated.CspNumber = db.GetString(reader, 1);
        entities.ExistingRelated.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingRelated.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingRelated.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.ExistingRelated.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ExistingRelated.LastUpdateTmst =
          db.GetNullableDateTime(reader, 6);
        entities.ExistingRelated.LastObligationEvent =
          db.GetNullableString(reader, 7);
        entities.ExistingRelated.Populated = true;
      });
  }

  private bool ReadObligation2()
  {
    System.Diagnostics.Debug.Assert(import.Persistent.Populated);
    entities.ExistingRelated.Populated = false;

    return Read("ReadObligation2",
      (db, command) =>
      {
        db.SetInt32(command, "otyFirstId", import.Persistent.DtyGeneratedId);
        db.SetInt32(
          command, "obgFGeneratedId",
          import.Persistent.SystemGeneratedIdentifier);
        db.SetString(command, "cspFNumber", import.Persistent.CspNumber);
        db.SetString(command, "cpaFType", import.Persistent.CpaType);
      },
      (db, reader) =>
      {
        entities.ExistingRelated.CpaType = db.GetString(reader, 0);
        entities.ExistingRelated.CspNumber = db.GetString(reader, 1);
        entities.ExistingRelated.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingRelated.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingRelated.PrimarySecondaryCode =
          db.GetNullableString(reader, 4);
        entities.ExistingRelated.LastUpdatedBy =
          db.GetNullableString(reader, 5);
        entities.ExistingRelated.LastUpdateTmst =
          db.GetNullableDateTime(reader, 6);
        entities.ExistingRelated.LastObligationEvent =
          db.GetNullableString(reader, 7);
        entities.ExistingRelated.Populated = true;
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
    /// A value of Persistent.
    /// </summary>
    [JsonPropertyName("persistent")]
    public Obligation Persistent
    {
      get => persistent ??= new();
      set => persistent = value;
    }

    /// <summary>
    /// A value of CreateObCollProtHist.
    /// </summary>
    [JsonPropertyName("createObCollProtHist")]
    public Common CreateObCollProtHist
    {
      get => createObCollProtHist ??= new();
      set => createObCollProtHist = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    private Obligation persistent;
    private Common createObCollProtHist;
    private ObligCollProtectionHist obligCollProtectionHist;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>
    /// A value of ObCollProtHistCreated.
    /// </summary>
    [JsonPropertyName("obCollProtHistCreated")]
    public Common ObCollProtHistCreated
    {
      get => obCollProtHistCreated ??= new();
      set => obCollProtHistCreated = value;
    }

    /// <summary>
    /// A value of CollsFndToProtect.
    /// </summary>
    [JsonPropertyName("collsFndToProtect")]
    public Common CollsFndToProtect
    {
      get => collsFndToProtect ??= new();
      set => collsFndToProtect = value;
    }

    private Common obCollProtHistCreated;
    private Common collsFndToProtect;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of CollProtAction.
    /// </summary>
    [JsonPropertyName("collProtAction")]
    public TextWorkArea CollProtAction
    {
      get => collProtAction ??= new();
      set => collProtAction = value;
    }

    /// <summary>
    /// A value of ObCollProtHistCreated.
    /// </summary>
    [JsonPropertyName("obCollProtHistCreated")]
    public Common ObCollProtHistCreated
    {
      get => obCollProtHistCreated ??= new();
      set => obCollProtHistCreated = value;
    }

    /// <summary>
    /// A value of CollsFndToProtect.
    /// </summary>
    [JsonPropertyName("collsFndToProtect")]
    public Common CollsFndToProtect
    {
      get => collsFndToProtect ??= new();
      set => collsFndToProtect = value;
    }

    /// <summary>
    /// A value of OverrideCollProtFnd.
    /// </summary>
    [JsonPropertyName("overrideCollProtFnd")]
    public Common OverrideCollProtFnd
    {
      get => overrideCollProtFnd ??= new();
      set => overrideCollProtFnd = value;
    }

    /// <summary>
    /// A value of CreateObCollProtHist.
    /// </summary>
    [JsonPropertyName("createObCollProtHist")]
    public Common CreateObCollProtHist
    {
      get => createObCollProtHist ??= new();
      set => createObCollProtHist = value;
    }

    /// <summary>
    /// A value of ObligCollProtectionHist.
    /// </summary>
    [JsonPropertyName("obligCollProtectionHist")]
    public ObligCollProtectionHist ObligCollProtectionHist
    {
      get => obligCollProtectionHist ??= new();
      set => obligCollProtectionHist = value;
    }

    /// <summary>
    /// A value of LegalAction.
    /// </summary>
    [JsonPropertyName("legalAction")]
    public LegalAction LegalAction
    {
      get => legalAction ??= new();
      set => legalAction = value;
    }

    /// <summary>
    /// A value of NullDateWorkArea.
    /// </summary>
    [JsonPropertyName("nullDateWorkArea")]
    public DateWorkArea NullDateWorkArea
    {
      get => nullDateWorkArea ??= new();
      set => nullDateWorkArea = value;
    }

    /// <summary>
    /// A value of NullLegalAction.
    /// </summary>
    [JsonPropertyName("nullLegalAction")]
    public LegalAction NullLegalAction
    {
      get => nullLegalAction ??= new();
      set => nullLegalAction = value;
    }

    private TextWorkArea collProtAction;
    private Common obCollProtHistCreated;
    private Common collsFndToProtect;
    private Common overrideCollProtFnd;
    private Common createObCollProtHist;
    private ObligCollProtectionHist obligCollProtectionHist;
    private LegalAction legalAction;
    private DateWorkArea nullDateWorkArea;
    private LegalAction nullLegalAction;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingObligor1.
    /// </summary>
    [JsonPropertyName("existingObligor1")]
    public CsePerson ExistingObligor1
    {
      get => existingObligor1 ??= new();
      set => existingObligor1 = value;
    }

    /// <summary>
    /// A value of ExistingObligor2.
    /// </summary>
    [JsonPropertyName("existingObligor2")]
    public CsePersonAccount ExistingObligor2
    {
      get => existingObligor2 ??= new();
      set => existingObligor2 = value;
    }

    /// <summary>
    /// A value of ExistingRelated.
    /// </summary>
    [JsonPropertyName("existingRelated")]
    public Obligation ExistingRelated
    {
      get => existingRelated ??= new();
      set => existingRelated = value;
    }

    /// <summary>
    /// A value of ExistingObligationRln.
    /// </summary>
    [JsonPropertyName("existingObligationRln")]
    public ObligationRln ExistingObligationRln
    {
      get => existingObligationRln ??= new();
      set => existingObligationRln = value;
    }

    /// <summary>
    /// A value of ExistingDebt.
    /// </summary>
    [JsonPropertyName("existingDebt")]
    public ObligationTransaction ExistingDebt
    {
      get => existingDebt ??= new();
      set => existingDebt = value;
    }

    /// <summary>
    /// A value of ExistingCollection.
    /// </summary>
    [JsonPropertyName("existingCollection")]
    public Collection ExistingCollection
    {
      get => existingCollection ??= new();
      set => existingCollection = value;
    }

    /// <summary>
    /// A value of ExistingLegalAction.
    /// </summary>
    [JsonPropertyName("existingLegalAction")]
    public LegalAction ExistingLegalAction
    {
      get => existingLegalAction ??= new();
      set => existingLegalAction = value;
    }

    /// <summary>
    /// A value of DelMe.
    /// </summary>
    [JsonPropertyName("delMe")]
    public ObligCollProtectionHist DelMe
    {
      get => delMe ??= new();
      set => delMe = value;
    }

    /// <summary>
    /// A value of DelMe2.
    /// </summary>
    [JsonPropertyName("delMe2")]
    public ObligCollProtectionHist DelMe2
    {
      get => delMe2 ??= new();
      set => delMe2 = value;
    }

    private CsePerson existingObligor1;
    private CsePersonAccount existingObligor2;
    private Obligation existingRelated;
    private ObligationRln existingObligationRln;
    private ObligationTransaction existingDebt;
    private Collection existingCollection;
    private LegalAction existingLegalAction;
    private ObligCollProtectionHist delMe;
    private ObligCollProtectionHist delMe2;
  }
#endregion
}

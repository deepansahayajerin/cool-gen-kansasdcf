// Program: FN_READ_COLLECTION_PROTECTION, ID: 373388439, model: 746.
// Short name: SWE00492
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: FN_READ_COLLECTION_PROTECTION.
/// </summary>
[Serializable]
public partial class FnReadCollectionProtection: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the FN_READ_COLLECTION_PROTECTION program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new FnReadCollectionProtection(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of FnReadCollectionProtection.
  /// </summary>
  public FnReadCollectionProtection(IContext context, Import import,
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
    // ----------------------------------------------------------------
    // Date     Developer Name   Request#  Description
    // 12/27/01  Mark Ashworth   WR10504   New Development
    // ----------------------------------------------------------------
    if (ReadObligation())
    {
      export.Ocp.Index = -1;

      if (AsChar(import.ShowActiveOnly.Flag) == 'Y')
      {
        foreach(var item in ReadObligCollProtectionHist1())
        {
          ++export.Ocp.Index;
          export.Ocp.CheckSize();

          export.Ocp.Update.ObligCollProtectionHist.Assign(
            entities.ObligCollProtectionHist);
        }
      }
      else
      {
        foreach(var item in ReadObligCollProtectionHist2())
        {
          ++export.Ocp.Index;
          export.Ocp.CheckSize();

          export.Ocp.Update.ObligCollProtectionHist.Assign(
            entities.ObligCollProtectionHist);
        }
      }
    }
    else
    {
      ExitState = "FN0000_OBLIGATION_NF";
    }
  }

  private IEnumerable<bool> ReadObligCollProtectionHist1()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return ReadEach("ReadObligCollProtectionHist1",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyIdentifier", entities.ExistingObligation.DtyGeneratedId);
          
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
        db.SetDate(
          command, "deactivationDate",
          local.Null1.DeactivationDate.GetValueOrDefault());
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedBy = db.GetString(reader, 3);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.ObligCollProtectionHist.ReasonText = db.GetString(reader, 5);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 6);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 7);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 8);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 9);
        entities.ObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 10);
        entities.ObligCollProtectionHist.Populated = true;

        return true;
      });
  }

  private IEnumerable<bool> ReadObligCollProtectionHist2()
  {
    System.Diagnostics.Debug.Assert(entities.ExistingObligation.Populated);
    entities.ObligCollProtectionHist.Populated = false;

    return ReadEach("ReadObligCollProtectionHist2",
      (db, command) =>
      {
        db.SetString(command, "cpaType", entities.ExistingObligation.CpaType);
        db.SetInt32(
          command, "obgIdentifier",
          entities.ExistingObligation.SystemGeneratedIdentifier);
        db.SetInt32(
          command, "otyIdentifier", entities.ExistingObligation.DtyGeneratedId);
          
        db.
          SetString(command, "cspNumber", entities.ExistingObligation.CspNumber);
          
      },
      (db, reader) =>
      {
        entities.ObligCollProtectionHist.CvrdCollStrtDt = db.GetDate(reader, 0);
        entities.ObligCollProtectionHist.CvrdCollEndDt = db.GetDate(reader, 1);
        entities.ObligCollProtectionHist.DeactivationDate =
          db.GetDate(reader, 2);
        entities.ObligCollProtectionHist.CreatedBy = db.GetString(reader, 3);
        entities.ObligCollProtectionHist.CreatedTmst =
          db.GetDateTime(reader, 4);
        entities.ObligCollProtectionHist.ReasonText = db.GetString(reader, 5);
        entities.ObligCollProtectionHist.CspNumber = db.GetString(reader, 6);
        entities.ObligCollProtectionHist.CpaType = db.GetString(reader, 7);
        entities.ObligCollProtectionHist.OtyIdentifier = db.GetInt32(reader, 8);
        entities.ObligCollProtectionHist.ObgIdentifier = db.GetInt32(reader, 9);
        entities.ObligCollProtectionHist.ProtectionLevel =
          db.GetString(reader, 10);
        entities.ObligCollProtectionHist.Populated = true;

        return true;
      });
  }

  private bool ReadObligation()
  {
    entities.ExistingObligation.Populated = false;

    return Read("ReadObligation",
      (db, command) =>
      {
        db.
          SetInt32(command, "obId", import.Obligation.SystemGeneratedIdentifier);
          
        db.SetString(command, "cspNumber", import.CsePerson.Number);
        db.SetInt32(
          command, "dtyGeneratedId",
          import.ObligationType.SystemGeneratedIdentifier);
      },
      (db, reader) =>
      {
        entities.ExistingObligation.CpaType = db.GetString(reader, 0);
        entities.ExistingObligation.CspNumber = db.GetString(reader, 1);
        entities.ExistingObligation.SystemGeneratedIdentifier =
          db.GetInt32(reader, 2);
        entities.ExistingObligation.DtyGeneratedId = db.GetInt32(reader, 3);
        entities.ExistingObligation.Populated = true;
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
    /// A value of ShowActiveOnly.
    /// </summary>
    [JsonPropertyName("showActiveOnly")]
    public Common ShowActiveOnly
    {
      get => showActiveOnly ??= new();
      set => showActiveOnly = value;
    }

    /// <summary>
    /// A value of Obligation.
    /// </summary>
    [JsonPropertyName("obligation")]
    public Obligation Obligation
    {
      get => obligation ??= new();
      set => obligation = value;
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
    /// A value of ObligationType.
    /// </summary>
    [JsonPropertyName("obligationType")]
    public ObligationType ObligationType
    {
      get => obligationType ??= new();
      set => obligationType = value;
    }

    private Common showActiveOnly;
    private Obligation obligation;
    private CsePerson csePerson;
    private ObligationType obligationType;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A OcpGroup group.</summary>
    [Serializable]
    public class OcpGroup
    {
      /// <summary>
      /// A value of Sel.
      /// </summary>
      [JsonPropertyName("sel")]
      public Common Sel
      {
        get => sel ??= new();
        set => sel = value;
      }

      /// <summary>
      /// A value of Prompt.
      /// </summary>
      [JsonPropertyName("prompt")]
      public Common Prompt
      {
        get => prompt ??= new();
        set => prompt = value;
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

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 75;

      private Common sel;
      private Common prompt;
      private ObligCollProtectionHist obligCollProtectionHist;
    }

    /// <summary>
    /// Gets a value of Ocp.
    /// </summary>
    [JsonIgnore]
    public Array<OcpGroup> Ocp => ocp ??= new(OcpGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ocp for json serialization.
    /// </summary>
    [JsonPropertyName("ocp")]
    [Computed]
    public IList<OcpGroup> Ocp_Json
    {
      get => ocp;
      set => Ocp.Assign(value);
    }

    private Array<OcpGroup> ocp;
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
    public ObligCollProtectionHist Null1
    {
      get => null1 ??= new();
      set => null1 = value;
    }

    private ObligCollProtectionHist null1;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
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
    /// A value of ExistingObligation.
    /// </summary>
    [JsonPropertyName("existingObligation")]
    public Obligation ExistingObligation
    {
      get => existingObligation ??= new();
      set => existingObligation = value;
    }

    /// <summary>
    /// A value of ExistingObligor.
    /// </summary>
    [JsonPropertyName("existingObligor")]
    public CsePersonAccount ExistingObligor
    {
      get => existingObligor ??= new();
      set => existingObligor = value;
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
    /// A value of ExistingObligationType.
    /// </summary>
    [JsonPropertyName("existingObligationType")]
    public ObligationType ExistingObligationType
    {
      get => existingObligationType ??= new();
      set => existingObligationType = value;
    }

    private ObligCollProtectionHist obligCollProtectionHist;
    private Obligation existingObligation;
    private CsePersonAccount existingObligor;
    private CsePerson existingCsePerson;
    private ObligationType existingObligationType;
  }
#endregion
}

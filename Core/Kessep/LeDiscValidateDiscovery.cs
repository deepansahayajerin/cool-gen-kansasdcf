// Program: LE_DISC_VALIDATE_DISCOVERY, ID: 372025879, model: 746.
// Short name: SWE00763
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
/// A program: LE_DISC_VALIDATE_DISCOVERY.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block validates Discovery details.
/// </para>
/// </summary>
[Serializable]
public partial class LeDiscValidateDiscovery: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_DISC_VALIDATE_DISCOVERY program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeDiscValidateDiscovery(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeDiscValidateDiscovery.
  /// </summary>
  public LeDiscValidateDiscovery(IContext context, Import import, Export export):
    
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
    // Date		Developer       Description
    // 11/04/98        R. Jean         Eliminated unnecessary reads
    // -------------------------------------------------------------------
    export.ErrorCodes.Index = -1;

    if (Equal(import.UserAction.Command, "DELETE"))
    {
      // ---------------------------------------------
      // No more validation needed
      // ---------------------------------------------
      return;
    }

    if (AsChar(import.Discovery.RequestedByCseInd) != 'Y' && AsChar
      (import.Discovery.RequestedByCseInd) != 'N')
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 9;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (IsEmpty(import.Discovery.LastName))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 5;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (IsEmpty(import.Discovery.FirstName))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 4;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (IsEmpty(import.Discovery.RespReqByLastName))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 10;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (IsEmpty(import.Discovery.RespReqByFirstName))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 11;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (!Lt(new DateTime(1, 1, 1), import.Discovery.RequestedDate))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 3;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (Lt(Now().Date, import.Discovery.RequestedDate))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 13;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (IsEmpty(import.Discovery.RequestDescription))
    {
      ++export.ErrorCodes.Index;
      export.ErrorCodes.CheckSize();

      export.ErrorCodes.Update.DetailErrorCode.Count = 12;
      export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

      return;
    }

    if (Lt(local.InitialisedToZeros.Date, import.Discovery.ResponseDate))
    {
      if (Lt(Now().Date, import.Discovery.ResponseDate) || Lt
        (import.Discovery.ResponseDate, import.Discovery.RequestedDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 6;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }
    }

    if (Lt(local.InitialisedToZeros.Date, import.Discovery.ResponseDate) || !
      IsEmpty(import.Discovery.ResponseDescription))
    {
      if (!Lt(local.InitialisedToZeros.Date, import.Discovery.ResponseDate))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 7;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;

        return;
      }

      if (IsEmpty(import.Discovery.ResponseDescription))
      {
        ++export.ErrorCodes.Index;
        export.ErrorCodes.CheckSize();

        export.ErrorCodes.Update.DetailErrorCode.Count = 8;
        export.LastErrorEntryNo.Count = export.ErrorCodes.Index + 1;
      }
    }
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
    /// A value of UserAction.
    /// </summary>
    [JsonPropertyName("userAction")]
    public Common UserAction
    {
      get => userAction ??= new();
      set => userAction = value;
    }

    /// <summary>
    /// A value of Discovery.
    /// </summary>
    [JsonPropertyName("discovery")]
    public Discovery Discovery
    {
      get => discovery ??= new();
      set => discovery = value;
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

    private Common userAction;
    private Discovery discovery;
    private LegalAction legalAction;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A ErrorCodesGroup group.</summary>
    [Serializable]
    public class ErrorCodesGroup
    {
      /// <summary>
      /// A value of DetailErrorCode.
      /// </summary>
      [JsonPropertyName("detailErrorCode")]
      public Common DetailErrorCode
      {
        get => detailErrorCode ??= new();
        set => detailErrorCode = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 100;

      private Common detailErrorCode;
    }

    /// <summary>
    /// A value of LastErrorEntryNo.
    /// </summary>
    [JsonPropertyName("lastErrorEntryNo")]
    public Common LastErrorEntryNo
    {
      get => lastErrorEntryNo ??= new();
      set => lastErrorEntryNo = value;
    }

    /// <summary>
    /// Gets a value of ErrorCodes.
    /// </summary>
    [JsonIgnore]
    public Array<ErrorCodesGroup> ErrorCodes => errorCodes ??= new(
      ErrorCodesGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of ErrorCodes for json serialization.
    /// </summary>
    [JsonPropertyName("errorCodes")]
    [Computed]
    public IList<ErrorCodesGroup> ErrorCodes_Json
    {
      get => errorCodes;
      set => ErrorCodes.Assign(value);
    }

    private Common lastErrorEntryNo;
    private Array<ErrorCodesGroup> errorCodes;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of InitialisedToZeros.
    /// </summary>
    [JsonPropertyName("initialisedToZeros")]
    public DateWorkArea InitialisedToZeros
    {
      get => initialisedToZeros ??= new();
      set => initialisedToZeros = value;
    }

    private DateWorkArea initialisedToZeros;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of ExistingDiscovery.
    /// </summary>
    [JsonPropertyName("existingDiscovery")]
    public Discovery ExistingDiscovery
    {
      get => existingDiscovery ??= new();
      set => existingDiscovery = value;
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

    private Discovery existingDiscovery;
    private LegalAction existingLegalAction;
  }
#endregion
}

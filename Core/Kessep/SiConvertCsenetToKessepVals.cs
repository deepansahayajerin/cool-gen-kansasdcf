// Program: SI_CONVERT_CSENET_TO_KESSEP_VALS, ID: 372621070, model: 746.
// Short name: SWE02563
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

using static Bphx.Cool.Functions;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: SI_CONVERT_CSENET_TO_KESSEP_VALS.
/// </summary>
[Serializable]
public partial class SiConvertCsenetToKessepVals: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the SI_CONVERT_CSENET_TO_KESSEP_VALS program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new SiConvertCsenetToKessepVals(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of SiConvertCsenetToKessepVals.
  /// </summary>
  public SiConvertCsenetToKessepVals(IContext context, Import import,
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
    // -----------------------------------------------------------------------
    //         M A I N T E N A N C E   L O G
    // -----------------------------------------------------------------------
    // Date		Developer	Request		Description
    // -----------------------------------------------------------------------
    // 4/15/1999	Carl Ott			Initial Dev.
    // 7/06/2000	C. Scroggins	95529
    // 11/17/2000	M Ramirez	106461		Set values according to
    // 						KESSEP Case Types
    // 						(Person_Program Codes)
    // 03/13/2001	M Ramirez	113335		CSI txns have too many
    // 						leading zeroes on the
    // 						KS Case Id
    // 04/27/2001	M Ramirez	117740		Some incoming ks case
    // 						numbers don't have leading
    // 						zeroes.
    // 10/03/2001	M Ashworth	WR20128		Add two new case types for
    //                                                 
    // CSENET S-Former Assistance
    //                                                 
    // and T- Never Assistance.
    //                                                 
    // Both to be converted to NAI
    // -----------------------------------------------------------------------
    export.InterstateCase.Assign(import.InterstateCase);
    export.InterstateApIdentification.Assign(import.InterstateApIdentification);
    export.InterstateApLocate.Assign(import.InterstateApLocate);

    // mjr
    // ---------------------------------------------
    // 03/13/2001
    // Extract CSE Case Number from ks_case_id
    // ----------------------------------------------------------
    if (!IsEmpty(export.InterstateCase.KsCaseId))
    {
      // mjr
      // ---------------------------------------------
      // 04/27/2001
      // Check for KPC in the first 3 characters.  If present, don't
      // change the ks_case_id
      // ----------------------------------------------------------
      if (Equal(export.InterstateCase.KsCaseId, 1, 3, "KPC"))
      {
        goto Test;
      }

      // mjr
      // -------------------------------------------------------
      // ks_case_id has 1 to 15 characters in it
      // Take all the characters, except for leading zeroes
      // and leading spaces
      // ----------------------------------------------------------
      local.Starting.Count = Verify(export.InterstateCase.KsCaseId, " 0");

      if (local.Starting.Count > 1)
      {
        local.Case1.Number =
          Substring(export.InterstateCase.KsCaseId, local.Starting.Count, 10);
      }
      else
      {
        local.Case1.Number = export.InterstateCase.KsCaseId ?? Spaces(10);
      }

      UseCabZeroFillNumber();

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        ExitState = "ACO_NN0000_ALL_OK";

        goto Test;
      }

      export.InterstateCase.KsCaseId = local.Case1.Number;
    }

Test:

    // **************************************************************
    // Convert Case Type codes from CSENet values to KESSEP values.
    // **************************************************************
    if (!Equal(export.InterstateCase.FunctionalTypeCode, "LO1"))
    {
      if (AsChar(export.InterstateCase.ActionCode) == 'R')
      {
        switch(TrimEnd(import.InterstateCase.CaseType))
        {
          case "A":
            export.InterstateCase.CaseType = "AFI";

            break;
          case "C":
            export.InterstateCase.CaseType = "FCI";

            break;
          case "F":
            export.InterstateCase.CaseType = "FCI";

            break;
          case "M":
            export.InterstateCase.CaseType = "MAI";

            break;
          case "N":
            export.InterstateCase.CaseType = "NAI";

            break;
          case "R":
            export.InterstateCase.CaseType = "AFI";

            break;
          case "S":
            export.InterstateCase.CaseType = "NAI";

            break;
          case "T":
            export.InterstateCase.CaseType = "NAI";

            break;
          case "V":
            break;
          default:
            break;
        }
      }
      else if (!Equal(export.InterstateCase.CaseType, "V"))
      {
        export.InterstateCase.CaseType = "";
      }
    }

    // **************************************************************
    // Convert Race codes from CSENet values to KESSEP values.
    // **************************************************************
    switch(TrimEnd(import.InterstateApIdentification.Race ?? ""))
    {
      case "I":
        export.InterstateApIdentification.Race = "AI";

        break;
      case "B":
        export.InterstateApIdentification.Race = "BL";

        break;
      case "S":
        export.InterstateApIdentification.Race = "HI";

        break;
      case "X":
        export.InterstateApIdentification.Race = "OT";

        break;
      case "A":
        export.InterstateApIdentification.Race = "SA";

        break;
      case "W":
        export.InterstateApIdentification.Race = "WH";

        break;
      default:
        export.InterstateApIdentification.Race = "UK";

        break;
    }

    // **************************************************************
    // Convert Gender codes from CSENet values to KESSEP values.
    // **************************************************************
    if (AsChar(import.InterstateApIdentification.Sex) == 'O')
    {
      export.InterstateApIdentification.Sex = "";
    }

    // ****************************************************************
    // KESSEP does not accept partial addresses.  If Confirmation Indicator is 
    // set for partial address, set to spaces.
    // ***************************************************************
    if (AsChar(import.InterstateApLocate.ResidentialAddressConfirmInd) == 'P')
    {
      export.InterstateApLocate.ResidentialAddressEffectvDate = null;
      export.InterstateApLocate.ResidentialAddressEndDate = null;
      export.InterstateApLocate.ResidentialAddressLine1 = "";
      export.InterstateApLocate.ResidentialAddressLine2 = "";
      export.InterstateApLocate.ResidentialCity = "";
      export.InterstateApLocate.ResidentialState = "";
      export.InterstateApLocate.ResidentialZipCode4 = "";
      export.InterstateApLocate.ResidentialZipCode5 = "";
    }

    if (AsChar(import.InterstateApLocate.MailingAddressConfirmedInd) == 'P')
    {
      export.InterstateApLocate.MailingAddressEffectiveDate = null;
      export.InterstateApLocate.MailingAddressEndDate = null;
      export.InterstateApLocate.MailingAddressLine1 = "";
      export.InterstateApLocate.MailingAddressLine2 = "";
      export.InterstateApLocate.MailingCity = "";
      export.InterstateApLocate.MailingState = "";
      export.InterstateApLocate.MailingZipCode4 = "";
      export.InterstateApLocate.MailingZipCode5 = "";
    }

    export.Participant.Index = 0;
    export.Participant.Clear();

    for(import.Participant.Index = 0; import.Participant.Index < import
      .Participant.Count; ++import.Participant.Index)
    {
      if (export.Participant.IsFull)
      {
        break;
      }

      export.Participant.Update.InterstateParticipant.Assign(
        import.Participant.Item.InterstateParticipant);

      // **************************************************************
      // Convert Race codes from CSENet values to KESSEP values.
      // **************************************************************
      switch(TrimEnd(import.Participant.Item.InterstateParticipant.Race ?? ""))
      {
        case "I":
          export.Participant.Update.InterstateParticipant.Race = "AI";

          break;
        case "B":
          export.Participant.Update.InterstateParticipant.Race = "BL";

          break;
        case "S":
          export.Participant.Update.InterstateParticipant.Race = "HI";

          break;
        case "X":
          export.Participant.Update.InterstateParticipant.Race = "OT";

          break;
        case "A":
          export.Participant.Update.InterstateParticipant.Race = "SA";

          break;
        case "W":
          export.Participant.Update.InterstateParticipant.Race = "WH";

          break;
        default:
          export.Participant.Update.InterstateParticipant.Race = "UK";

          break;
      }

      // **************************************************************
      // Convert Relationship from CSENet values to KESSEP values.
      // **************************************************************
      switch(TrimEnd(import.Participant.Item.InterstateParticipant.
        Relationship ?? ""))
      {
        case "A":
          export.Participant.Update.InterstateParticipant.Relationship = "AP";

          break;
        case "C":
          export.Participant.Update.InterstateParticipant.Relationship = "AR";

          break;
        case "D":
          export.Participant.Update.InterstateParticipant.Relationship = "CH";

          break;
        case "P":
          export.Participant.Update.InterstateParticipant.Relationship = "AP";

          break;
        case "S":
          export.Participant.Update.InterstateParticipant.Relationship = "";

          break;
        default:
          break;
      }

      // **************************************************************
      // Convert Dependent Relationship to CP from CSENet values to KESSEP 
      // values.
      // **************************************************************
      switch(TrimEnd(import.Participant.Item.InterstateParticipant.
        DependentRelationCp ?? ""))
      {
        case "A":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "OR";

          break;
        case "C":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "CH";

          break;
        case "F":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "FC";

          break;
        case "G":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "GC";

          break;
        case "E":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "NN";

          break;
        case "N":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "NR";

          break;
        case "O":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "OR";

          break;
        case "S":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "SB";

          break;
        case "U":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "CO";

          break;
        case "W":
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "OR";

          break;
        default:
          export.Participant.Update.InterstateParticipant.DependentRelationCp =
            "OR";

          break;
      }

      // **************************************************************
      // Convert Gender codes from CSENet values to KESSEP values.
      // **************************************************************
      if (AsChar(import.Participant.Item.InterstateParticipant.Sex) == 'O')
      {
        export.Participant.Update.InterstateParticipant.Sex = "";
      }

      // **************************************************************
      // Convert Participant Status codes from CSENet values to KESSEP values.
      // **************************************************************
      switch(AsChar(import.Participant.Item.InterstateParticipant.Status))
      {
        case 'C':
          export.Participant.Update.InterstateParticipant.Status = "I";

          break;
        case 'O':
          export.Participant.Update.InterstateParticipant.Status = "A";

          break;
        default:
          break;
      }

      export.Participant.Next();
    }

    export.Order.Index = 0;
    export.Order.Clear();

    for(import.Order.Index = 0; import.Order.Index < import.Order.Count; ++
      import.Order.Index)
    {
      if (export.Order.IsFull)
      {
        break;
      }

      export.Order.Update.InterstateSupportOrder.Assign(
        import.Order.Item.InterstateSupportOrder);

      if (Equal(import.Order.Item.InterstateSupportOrder.DebtType, "SS"))
      {
        export.Order.Update.InterstateSupportOrder.DebtType = "SP";
      }

      switch(TrimEnd(import.Order.Item.InterstateSupportOrder.PaymentFreq ?? ""))
        
      {
        case "B":
          export.Order.Update.InterstateSupportOrder.PaymentFreq = "BW";

          break;
        case "S":
          export.Order.Update.InterstateSupportOrder.PaymentFreq = "SM";

          break;
        default:
          break;
      }

      export.Order.Next();
    }

    export.Collection.Index = 0;
    export.Collection.Clear();

    for(import.Collection.Index = 0; import.Collection.Index < import
      .Collection.Count; ++import.Collection.Index)
    {
      if (export.Collection.IsFull)
      {
        break;
      }

      export.Collection.Update.InterstateCollection.Assign(
        import.Collection.Item.InterstateCollection);

      if (AsChar(import.Collection.Item.InterstateCollection.PaymentSource) == 'I'
        )
      {
        export.Collection.Update.InterstateCollection.PaymentSource = "F";
      }

      export.Collection.Next();
    }
  }

  private void UseCabZeroFillNumber()
  {
    var useImport = new CabZeroFillNumber.Import();
    var useExport = new CabZeroFillNumber.Export();

    useImport.Case1.Number = local.Case1.Number;

    Call(CabZeroFillNumber.Execute, useImport, useExport);

    local.Case1.Number = useImport.Case1.Number;
  }
#endregion

#region Parameters.
  protected readonly Import import;
  protected readonly Export export;
  protected readonly Local local = new();
#endregion

#region Structures
  /// <summary>
  /// This class defines import view.
  /// </summary>
  [Serializable]
  public class Import
  {
    /// <summary>A CollectionGroup group.</summary>
    [Serializable]
    public class CollectionGroup
    {
      /// <summary>
      /// A value of InterstateCollection.
      /// </summary>
      [JsonPropertyName("interstateCollection")]
      public InterstateCollection InterstateCollection
      {
        get => interstateCollection ??= new();
        set => interstateCollection = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateCollection interstateCollection;
    }

    /// <summary>A OrderGroup group.</summary>
    [Serializable]
    public class OrderGroup
    {
      /// <summary>
      /// A value of InterstateSupportOrder.
      /// </summary>
      [JsonPropertyName("interstateSupportOrder")]
      public InterstateSupportOrder InterstateSupportOrder
      {
        get => interstateSupportOrder ??= new();
        set => interstateSupportOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder interstateSupportOrder;
    }

    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of InterstateParticipant.
      /// </summary>
      [JsonPropertyName("interstateParticipant")]
      public InterstateParticipant InterstateParticipant
      {
        get => interstateParticipant ??= new();
        set => interstateParticipant = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateParticipant interstateParticipant;
    }

    /// <summary>
    /// Gets a value of Collection.
    /// </summary>
    [JsonIgnore]
    public Array<CollectionGroup> Collection => collection ??= new(
      CollectionGroup.Capacity);

    /// <summary>
    /// Gets a value of Collection for json serialization.
    /// </summary>
    [JsonPropertyName("collection")]
    [Computed]
    public IList<CollectionGroup> Collection_Json
    {
      get => collection;
      set => Collection.Assign(value);
    }

    /// <summary>
    /// Gets a value of Order.
    /// </summary>
    [JsonIgnore]
    public Array<OrderGroup> Order => order ??= new(OrderGroup.Capacity);

    /// <summary>
    /// Gets a value of Order for json serialization.
    /// </summary>
    [JsonPropertyName("order")]
    [Computed]
    public IList<OrderGroup> Order_Json
    {
      get => order;
      set => Order.Assign(value);
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

    /// <summary>
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
    }

    private Array<CollectionGroup> collection;
    private Array<OrderGroup> order;
    private InterstateCase interstateCase;
    private Array<ParticipantGroup> participant;
    private InterstateApIdentification interstateApIdentification;
    private InterstateApLocate interstateApLocate;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A CollectionGroup group.</summary>
    [Serializable]
    public class CollectionGroup
    {
      /// <summary>
      /// A value of InterstateCollection.
      /// </summary>
      [JsonPropertyName("interstateCollection")]
      public InterstateCollection InterstateCollection
      {
        get => interstateCollection ??= new();
        set => interstateCollection = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateCollection interstateCollection;
    }

    /// <summary>A OrderGroup group.</summary>
    [Serializable]
    public class OrderGroup
    {
      /// <summary>
      /// A value of InterstateSupportOrder.
      /// </summary>
      [JsonPropertyName("interstateSupportOrder")]
      public InterstateSupportOrder InterstateSupportOrder
      {
        get => interstateSupportOrder ??= new();
        set => interstateSupportOrder = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateSupportOrder interstateSupportOrder;
    }

    /// <summary>A ParticipantGroup group.</summary>
    [Serializable]
    public class ParticipantGroup
    {
      /// <summary>
      /// A value of InterstateParticipant.
      /// </summary>
      [JsonPropertyName("interstateParticipant")]
      public InterstateParticipant InterstateParticipant
      {
        get => interstateParticipant ??= new();
        set => interstateParticipant = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private InterstateParticipant interstateParticipant;
    }

    /// <summary>
    /// Gets a value of Collection.
    /// </summary>
    [JsonIgnore]
    public Array<CollectionGroup> Collection => collection ??= new(
      CollectionGroup.Capacity);

    /// <summary>
    /// Gets a value of Collection for json serialization.
    /// </summary>
    [JsonPropertyName("collection")]
    [Computed]
    public IList<CollectionGroup> Collection_Json
    {
      get => collection;
      set => Collection.Assign(value);
    }

    /// <summary>
    /// Gets a value of Order.
    /// </summary>
    [JsonIgnore]
    public Array<OrderGroup> Order => order ??= new(OrderGroup.Capacity);

    /// <summary>
    /// Gets a value of Order for json serialization.
    /// </summary>
    [JsonPropertyName("order")]
    [Computed]
    public IList<OrderGroup> Order_Json
    {
      get => order;
      set => Order.Assign(value);
    }

    /// <summary>
    /// A value of InterstateApLocate.
    /// </summary>
    [JsonPropertyName("interstateApLocate")]
    public InterstateApLocate InterstateApLocate
    {
      get => interstateApLocate ??= new();
      set => interstateApLocate = value;
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

    /// <summary>
    /// Gets a value of Participant.
    /// </summary>
    [JsonIgnore]
    public Array<ParticipantGroup> Participant => participant ??= new(
      ParticipantGroup.Capacity);

    /// <summary>
    /// Gets a value of Participant for json serialization.
    /// </summary>
    [JsonPropertyName("participant")]
    [Computed]
    public IList<ParticipantGroup> Participant_Json
    {
      get => participant;
      set => Participant.Assign(value);
    }

    /// <summary>
    /// A value of InterstateApIdentification.
    /// </summary>
    [JsonPropertyName("interstateApIdentification")]
    public InterstateApIdentification InterstateApIdentification
    {
      get => interstateApIdentification ??= new();
      set => interstateApIdentification = value;
    }

    private Array<CollectionGroup> collection;
    private Array<OrderGroup> order;
    private InterstateApLocate interstateApLocate;
    private InterstateCase interstateCase;
    private Array<ParticipantGroup> participant;
    private InterstateApIdentification interstateApIdentification;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public Common Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    private Case1 case1;
    private Common starting;
  }
#endregion
}

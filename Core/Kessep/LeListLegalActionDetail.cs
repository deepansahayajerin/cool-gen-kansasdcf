// Program: LE_LIST_LEGAL_ACTION_DETAIL, ID: 371993420, model: 746.
// Short name: SWE00800
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
/// A program: LE_LIST_LEGAL_ACTION_DETAIL.
/// </para>
/// <para>
/// RESP: LGLENFAC
/// This action block will List Legal Action Detail for the specified Legal 
/// Action.
/// </para>
/// </summary>
[Serializable]
public partial class LeListLegalActionDetail: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the LE_LIST_LEGAL_ACTION_DETAIL program.
  /// </summary>
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new LeListLegalActionDetail(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of LeListLegalActionDetail.
  /// </summary>
  public LeListLegalActionDetail(IContext context, Import import, Export export):
    
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ------------------------------------------------------------------------------------------------------------------------
    // DATE	  DEVELOPER	REQUEST #	DESCRIPTION
    // --------  ------------	-----------	
    // --------------------------------------------------------------------------------
    // 05/30/95  Dave Allen			Initial Code
    // 01/10/98  P. Sharp			Removed reads of legal action, fips, trib and 
    // fips_trip_address. Removed any unused views.
    // 02/17/98  P. Sharp			Added created by and updated by to the export and 
    // entity action views. Need to be able
    // 					to determine if the detail is a converted detail.
    // 02/07/05  GVandy	PR233007	Read and export the legal action attributes.
    // ------------------------------------------------------------------------------------------------------------------------
    ExitState = "ACO_NN0000_ALL_OK";

    if (ReadLegalAction())
    {
      import.Export1.Assign(entities.LegalAction);
    }
    else
    {
      ExitState = "LEGAL_ACTION_NF";

      return;
    }

    export.LegalActionDetail.Index = 0;
    export.LegalActionDetail.Clear();

    foreach(var item in ReadLegalActionDetail())
    {
      export.LegalActionDetail.Update.LegalActionDetail1.Assign(
        entities.LegalActionDetail1);

      if (!IsEmpty(entities.LegalActionDetail1.FreqPeriodCode))
      {
        local.ObligationPaymentSchedule.FrequencyCode =
          entities.LegalActionDetail1.FreqPeriodCode ?? Spaces(2);
        UseFnSetFrequencyTextField();
      }

      if (AsChar(entities.LegalActionDetail1.DetailType) == 'F')
      {
        if (ReadObligationType())
        {
          MoveObligationType(entities.ObligationType,
            export.LegalActionDetail.Update.Detail);
        }
        else
        {
          ExitState = "OBLIGATION_TYPE_NF";
          export.LegalActionDetail.Next();

          return;
        }
      }
      else
      {
        export.LegalActionDetail.Update.Detail.Code =
          entities.LegalActionDetail1.NonFinOblgType ?? Spaces(7);
      }

      export.LegalActionDetail.Next();
    }
  }

  private static void MoveObligationType(ObligationType source,
    ObligationType target)
  {
    target.SystemGeneratedIdentifier = source.SystemGeneratedIdentifier;
    target.Code = source.Code;
  }

  private void UseFnSetFrequencyTextField()
  {
    var useImport = new FnSetFrequencyTextField.Import();
    var useExport = new FnSetFrequencyTextField.Export();

    useImport.ObligationPaymentSchedule.FrequencyCode =
      local.ObligationPaymentSchedule.FrequencyCode;

    Call(FnSetFrequencyTextField.Execute, useImport, useExport);

    export.LegalActionDetail.Update.FrequencyWorkSet.FrequencyDescription =
      useExport.FrequencyWorkSet.FrequencyDescription;
  }

  private bool ReadLegalAction()
  {
    entities.LegalAction.Populated = false;

    return Read("ReadLegalAction",
      (db, command) =>
      {
        db.SetInt32(command, "legalActionId", import.Export1.Identifier);
      },
      (db, reader) =>
      {
        entities.LegalAction.Identifier = db.GetInt32(reader, 0);
        entities.LegalAction.Classification = db.GetString(reader, 1);
        entities.LegalAction.ActionTaken = db.GetString(reader, 2);
        entities.LegalAction.FiledDate = db.GetNullableDate(reader, 3);
        entities.LegalAction.CourtCaseNumber = db.GetNullableString(reader, 4);
        entities.LegalAction.PaymentLocation = db.GetNullableString(reader, 5);
        entities.LegalAction.StandardNumber = db.GetNullableString(reader, 6);
        entities.LegalAction.Populated = true;
      });
  }

  private IEnumerable<bool> ReadLegalActionDetail()
  {
    return ReadEach("ReadLegalActionDetail",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", entities.LegalAction.Identifier);
        db.SetInt32(command, "laDetailNo", import.Starting.Number);
      },
      (db, reader) =>
      {
        if (export.LegalActionDetail.IsFull)
        {
          return false;
        }

        entities.LegalActionDetail1.LgaIdentifier = db.GetInt32(reader, 0);
        entities.LegalActionDetail1.Number = db.GetInt32(reader, 1);
        entities.LegalActionDetail1.EndDate = db.GetNullableDate(reader, 2);
        entities.LegalActionDetail1.EffectiveDate = db.GetDate(reader, 3);
        entities.LegalActionDetail1.BondAmount =
          db.GetNullableDecimal(reader, 4);
        entities.LegalActionDetail1.CreatedBy = db.GetString(reader, 5);
        entities.LegalActionDetail1.LastUpdatedBy =
          db.GetNullableString(reader, 6);
        entities.LegalActionDetail1.ArrearsAmount =
          db.GetNullableDecimal(reader, 7);
        entities.LegalActionDetail1.CurrentAmount =
          db.GetNullableDecimal(reader, 8);
        entities.LegalActionDetail1.JudgementAmount =
          db.GetNullableDecimal(reader, 9);
        entities.LegalActionDetail1.Limit = db.GetNullableInt32(reader, 10);
        entities.LegalActionDetail1.NonFinOblgType =
          db.GetNullableString(reader, 11);
        entities.LegalActionDetail1.DetailType = db.GetString(reader, 12);
        entities.LegalActionDetail1.FreqPeriodCode =
          db.GetNullableString(reader, 13);
        entities.LegalActionDetail1.DayOfWeek = db.GetNullableInt32(reader, 14);
        entities.LegalActionDetail1.DayOfMonth1 =
          db.GetNullableInt32(reader, 15);
        entities.LegalActionDetail1.DayOfMonth2 =
          db.GetNullableInt32(reader, 16);
        entities.LegalActionDetail1.PeriodInd =
          db.GetNullableString(reader, 17);
        entities.LegalActionDetail1.Description = db.GetString(reader, 18);
        entities.LegalActionDetail1.Populated = true;
        CheckValid<LegalActionDetail>("DetailType",
          entities.LegalActionDetail1.DetailType);

        return true;
      });
  }

  private bool ReadObligationType()
  {
    entities.ObligationType.Populated = false;

    return Read("ReadObligationType",
      (db, command) =>
      {
        db.SetInt32(command, "lgaIdentifier", import.Export1.Identifier);
        db.SetInt32(command, "laDetailNo", entities.LegalActionDetail1.Number);
      },
      (db, reader) =>
      {
        entities.ObligationType.SystemGeneratedIdentifier =
          db.GetInt32(reader, 0);
        entities.ObligationType.Code = db.GetString(reader, 1);
        entities.ObligationType.Populated = true;
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
    /// A value of Starting.
    /// </summary>
    [JsonPropertyName("starting")]
    public LegalActionDetail Starting
    {
      get => starting ??= new();
      set => starting = value;
    }

    /// <summary>
    /// A value of Export1.
    /// </summary>
    [JsonPropertyName("export1")]
    public LegalAction Export1
    {
      get => export1 ??= new();
      set => export1 = value;
    }

    private LegalActionDetail starting;
    private LegalAction export1;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A LegalActionDetailGroup group.</summary>
    [Serializable]
    public class LegalActionDetailGroup
    {
      /// <summary>
      /// A value of FrequencyWorkSet.
      /// </summary>
      [JsonPropertyName("frequencyWorkSet")]
      public FrequencyWorkSet FrequencyWorkSet
      {
        get => frequencyWorkSet ??= new();
        set => frequencyWorkSet = value;
      }

      /// <summary>
      /// A value of Detail.
      /// </summary>
      [JsonPropertyName("detail")]
      public ObligationType Detail
      {
        get => detail ??= new();
        set => detail = value;
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

      /// <summary>
      /// A value of LegalActionDetail1.
      /// </summary>
      [JsonPropertyName("legalActionDetail1")]
      public LegalActionDetail LegalActionDetail1
      {
        get => legalActionDetail1 ??= new();
        set => legalActionDetail1 = value;
      }

      /// <summary>
      /// A value of PromptType.
      /// </summary>
      [JsonPropertyName("promptType")]
      public Common PromptType
      {
        get => promptType ??= new();
        set => promptType = value;
      }

      /// <summary>
      /// A value of PromptFreq.
      /// </summary>
      [JsonPropertyName("promptFreq")]
      public Common PromptFreq
      {
        get => promptFreq ??= new();
        set => promptFreq = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 20;

      private FrequencyWorkSet frequencyWorkSet;
      private ObligationType detail;
      private Common common;
      private LegalActionDetail legalActionDetail1;
      private Common promptType;
      private Common promptFreq;
    }

    /// <summary>
    /// Gets a value of LegalActionDetail.
    /// </summary>
    [JsonIgnore]
    public Array<LegalActionDetailGroup> LegalActionDetail =>
      legalActionDetail ??= new(LegalActionDetailGroup.Capacity);

    /// <summary>
    /// Gets a value of LegalActionDetail for json serialization.
    /// </summary>
    [JsonPropertyName("legalActionDetail")]
    [Computed]
    public IList<LegalActionDetailGroup> LegalActionDetail_Json
    {
      get => legalActionDetail;
      set => LegalActionDetail.Assign(value);
    }

    private Array<LegalActionDetailGroup> legalActionDetail;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of FrequencyWorkSet.
    /// </summary>
    [JsonPropertyName("frequencyWorkSet")]
    public FrequencyWorkSet FrequencyWorkSet
    {
      get => frequencyWorkSet ??= new();
      set => frequencyWorkSet = value;
    }

    /// <summary>
    /// A value of ObligationPaymentSchedule.
    /// </summary>
    [JsonPropertyName("obligationPaymentSchedule")]
    public ObligationPaymentSchedule ObligationPaymentSchedule
    {
      get => obligationPaymentSchedule ??= new();
      set => obligationPaymentSchedule = value;
    }

    private FrequencyWorkSet frequencyWorkSet;
    private ObligationPaymentSchedule obligationPaymentSchedule;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
  {
    /// <summary>
    /// A value of LaDetFinancial.
    /// </summary>
    [JsonPropertyName("laDetFinancial")]
    public LegalActionDetail LaDetFinancial
    {
      get => laDetFinancial ??= new();
      set => laDetFinancial = value;
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
    /// A value of LegalActionDetail1.
    /// </summary>
    [JsonPropertyName("legalActionDetail1")]
    public LegalActionDetail LegalActionDetail1
    {
      get => legalActionDetail1 ??= new();
      set => legalActionDetail1 = value;
    }

    private LegalActionDetail laDetFinancial;
    private ObligationType obligationType;
    private LegalAction legalAction;
    private LegalActionDetail legalActionDetail1;
  }
#endregion
}

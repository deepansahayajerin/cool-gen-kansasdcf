// Program: OE_FCR_SVES_T16_WEB, ID: 945076017, model: 746.
// Short name: SWCSV04P
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Bphx.Cool;
using Gov.Kansas.DCF.Cse.Entities;
using Gov.Kansas.DCF.Cse.Worksets;

namespace Gov.Kansas.DCF.Cse.Kessep;

/// <summary>
/// A program: OE_FCR_SVES_T16_WEB.
/// </summary>
[Serializable]
[ProcedureStep(ProcedureType.Server, ParticipateInTransaction = true)]
public partial class OeFcrSvesT16Web: Bphx.Cool.Action
{
  /// <summary>
  /// Executes the OE_FCR_SVES_T16_WEB program.
  /// </summary>
  [Entry]
  public static readonly Action<IContext, Import, Export> Execute =
    (c, i, e) => new OeFcrSvesT16Web(c, i, e).Run();

  /// <summary>
  /// Constructs an instance of OeFcrSvesT16Web.
  /// </summary>
  public OeFcrSvesT16Web(IContext context, Import import, Export export):
    base(context)
  {
    this.import = import;
    this.export = export;
  }

#region Implementation
  /// <summary>Executes action's logic.</summary>
  public void Run()
  {
    // ***********************************************************************
    // *    DATE       NAME             PR/SR #     DESCRIPTION OF THE CHANGE
    // * ----------  -----------------  ---------   -----------------------
    // * 07/08/11    LSS                CQ5577      Initial Coding.
    // *
    // * 12/13/13    LSS                CQ42137     Change security access to 
    // the web page back to the
    // *
    // 
    // original security to allow only the assigned
    // case worker
    // *
    // 
    // and their supervisor access to view FPLS
    // data per business
    // *
    // 
    // requirement dated 11/27/13 to be in
    // compliance with
    // *
    // 
    // the FPLS Security Agreement.
    // 03/29/2018  JLH     CQ61705     Remove edit that only allows case worker 
    // and supervisor to see SVES information.
    // *********************************************************
    // Fields needing formatted (dates, ssn, phone) are being exported as 
    // formatted work views.
    ExitState = "ACO_NN0000_ALL_OK";
    global.Command = "DISPLAY";
    export.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    export.FcrSvesGenInfo.LocateSourceResponseAgencyCo =
      import.FcrSvesGenInfo.LocateSourceResponseAgencyCo;

    if (!ReadCsePerson())
    {
      ExitState = "CSE_PERSON_NF";

      if (!IsExitState("ACO_NN0000_ALL_OK"))
      {
        UseEabExtractExitStateMessage();
        export.SvesErrorMsgs.Text80 = local.ExitStateWorkArea.Message;

        return;
      }
    }

    // Security code change per the following business rule change:
    // The original security feature to the FPLS screen allowed only the 
    // assigned case worker and their supervisor
    // the ability to view data via the FPLS screen.  Effective 9/13/2011, the 
    // additional layer of security is to
    // be removed, per central office decision.  Any profile deemed appropriate 
    // will have the ability to view FPLS data,
    // the user does not have to be assigned to the case any longer.
    // *The business rule for SVES is to have the same security as FPLS
    // The following is the profile authorization level of security that 
    // replaces the case assignment level of
    // security in the above commented code.   Per central office business 
    // decision 9/13/11.
    UseScCabTestSecurity();

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      export.SvesErrorMsgs.Text80 = local.ExitStateWorkArea.Message;

      return;
    }

    // END - CQ42137 Security access
    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeFcrSvesPersonRecord();
    }

    if (IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseOeFcrSvesT16();
    }

    if (!IsExitState("ACO_NN0000_ALL_OK"))
    {
      UseEabExtractExitStateMessage();
      export.SvesErrorMsgs.Text80 = local.ExitStateWorkArea.Message;
    }
  }

  private static void MoveFcrSvesGenInfo(FcrSvesGenInfo source,
    FcrSvesGenInfo target)
  {
    target.LocateSourceResponseAgencyCo = source.LocateSourceResponseAgencyCo;
    target.TransmitterStateTerritoryCode = source.TransmitterStateTerritoryCode;
    target.SubmittedFirstName = source.SubmittedFirstName;
    target.SubmittedMiddleName = source.SubmittedMiddleName;
    target.SubmittedLastName = source.SubmittedLastName;
  }

  private static void MovePhist(OeFcrSvesT16.Export.PhistGroup source,
    Export.PhistGroup target)
  {
    target.GphistType.PhistPaymentPayFlag1 =
      source.GphistType.PhistPaymentPayFlag1;
    target.GphistAmount.SsiMonthlyAssistanceAmount1 =
      source.GphistAmount.SsiMonthlyAssistanceAmount1;
    target.FormattedPhPmtDate.Text10 = source.FormattedPhPmtDate.Text10;
  }

  private static void MoveSvesType(OeFcrSvesPersonRecord.Export.
    SvesTypeGroup source, Export.SvesTypeGroup target)
  {
    target.Gagency.LocateSourceResponseAgencyCo =
      source.Gtype.LocateSourceResponseAgencyCo;
  }

  private static void MoveUi(OeFcrSvesT16.Export.UiGroup source,
    Export.UiGroup target)
  {
    target.GuiType.UnearnedIncomeTypeCode1 =
      source.GuiType.UnearnedIncomeTypeCode1;
    target.GuiVerifCd.UnearnedIncomeVerifiCd1 =
      source.GuiVerifCode.UnearnedIncomeVerifiCd1;
    target.FormattedUiStartDate.Text10 = source.FormattedUiStartDate.Text10;
    target.FormattedUiEndDate.Text10 = source.FormattedUiEndDate.Text10;
  }

  private void UseEabExtractExitStateMessage()
  {
    var useImport = new EabExtractExitStateMessage.Import();
    var useExport = new EabExtractExitStateMessage.Export();

    useExport.ExitStateWorkArea.Message = local.ExitStateWorkArea.Message;

    Call(EabExtractExitStateMessage.Execute, useImport, useExport);

    local.ExitStateWorkArea.Message = useExport.ExitStateWorkArea.Message;
  }

  private void UseOeFcrSvesPersonRecord()
  {
    var useImport = new OeFcrSvesPersonRecord.Import();
    var useExport = new OeFcrSvesPersonRecord.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.FcrSvesGenInfo.LocateSourceResponseAgencyCo =
      import.FcrSvesGenInfo.LocateSourceResponseAgencyCo;

    Call(OeFcrSvesPersonRecord.Execute, useImport, useExport);

    export.FormattedReqDate.Text10 = useExport.FormattedReqDate.Text10;
    export.FormattedRespDate.Text10 = useExport.FormattedRespRecDate.Text10;
    export.SvesErrorMsgs.Text80 = useExport.SvesErrorMsgs.Text80;
    useExport.SvesType.CopyTo(export.SvesType, MoveSvesType);
    MoveFcrSvesGenInfo(useExport.FcrSvesGenInfo, export.FcrSvesGenInfo);
  }

  private void UseOeFcrSvesT16()
  {
    var useImport = new OeFcrSvesT16.Import();
    var useExport = new OeFcrSvesT16.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;
    useImport.FcrSvesGenInfo.LocateSourceResponseAgencyCo =
      import.FcrSvesGenInfo.LocateSourceResponseAgencyCo;

    Call(OeFcrSvesT16.Execute, useImport, useExport);

    export.FormattedPhone.Text12 = useExport.FormattedPhone.Text12;
    export.FormattedStartDate.Text10 = useExport.FormattedStartDate.Text10;
    export.FormattedEndDate.Text10 = useExport.FormattedEndDate.Text10;
    useExport.Phist.CopyTo(export.Phist, MovePhist);
    useExport.Ui.CopyTo(export.Ui, MoveUi);
    export.FormattedPayStatDate.Text10 = useExport.FormattedPayStatDate.Text10;
    export.FormattedAppealDate.Text10 = useExport.FormattedAppealDate.Text10;
    export.FormattedRedeterDate.Text10 = useExport.FormattedRedeterDate.Text10;
    export.FormattedDenialDate.Text10 = useExport.FormattedDenialDate.Text10;
    export.FormattedEligDate.Text10 = useExport.FormattedEligDate.Text10;
    export.FormattedEstabDate.Text10 = useExport.FormattedEstabDate.Text10;
    export.SvesErrorMsgs.Text80 = useExport.SvesErrorMsgs.Text80;
    export.FcrSvesGenInfo.ParticipantType =
      useExport.FcrSvesGenInfo.ParticipantType;
    export.FcrSvesAddress.Assign(useExport.FcrSvesAddress);
    export.FcrSvesTitleXvi.Assign(useExport.FcrSvesTitleXvi);
  }

  private void UseScCabTestSecurity()
  {
    var useImport = new ScCabTestSecurity.Import();
    var useExport = new ScCabTestSecurity.Export();

    useImport.CsePersonsWorkSet.Number = import.CsePersonsWorkSet.Number;

    Call(ScCabTestSecurity.Execute, useImport, useExport);
  }

  private bool ReadCsePerson()
  {
    entities.CsePerson.Populated = false;

    return Read("ReadCsePerson",
      (db, command) =>
      {
        db.SetString(command, "numb", import.CsePersonsWorkSet.Number);
      },
      (db, reader) =>
      {
        entities.CsePerson.Number = db.GetString(reader, 0);
        entities.CsePerson.Populated = true;
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
    /// A value of CsePersonsWorkSet.
    /// </summary>
    [JsonPropertyName("csePersonsWorkSet")]
    public CsePersonsWorkSet CsePersonsWorkSet
    {
      get => csePersonsWorkSet ??= new();
      set => csePersonsWorkSet = value;
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private FcrSvesGenInfo fcrSvesGenInfo;
  }

  /// <summary>
  /// This class defines export view.
  /// </summary>
  [Serializable]
  public class Export
  {
    /// <summary>A PhistGroup group.</summary>
    [Serializable]
    public class PhistGroup
    {
      /// <summary>
      /// A value of GphistType.
      /// </summary>
      [JsonPropertyName("gphistType")]
      public FcrSvesTitleXvi GphistType
      {
        get => gphistType ??= new();
        set => gphistType = value;
      }

      /// <summary>
      /// A value of GphistAmount.
      /// </summary>
      [JsonPropertyName("gphistAmount")]
      public FcrSvesTitleXvi GphistAmount
      {
        get => gphistAmount ??= new();
        set => gphistAmount = value;
      }

      /// <summary>
      /// A value of FormattedPhPmtDate.
      /// </summary>
      [JsonPropertyName("formattedPhPmtDate")]
      public WorkArea FormattedPhPmtDate
      {
        get => formattedPhPmtDate ??= new();
        set => formattedPhPmtDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 8;

      private FcrSvesTitleXvi gphistType;
      private FcrSvesTitleXvi gphistAmount;
      private WorkArea formattedPhPmtDate;
    }

    /// <summary>A UiGroup group.</summary>
    [Serializable]
    public class UiGroup
    {
      /// <summary>
      /// A value of GuiType.
      /// </summary>
      [JsonPropertyName("guiType")]
      public FcrSvesTitleXvi GuiType
      {
        get => guiType ??= new();
        set => guiType = value;
      }

      /// <summary>
      /// A value of GuiVerifCd.
      /// </summary>
      [JsonPropertyName("guiVerifCd")]
      public FcrSvesTitleXvi GuiVerifCd
      {
        get => guiVerifCd ??= new();
        set => guiVerifCd = value;
      }

      /// <summary>
      /// A value of FormattedUiStartDate.
      /// </summary>
      [JsonPropertyName("formattedUiStartDate")]
      public WorkArea FormattedUiStartDate
      {
        get => formattedUiStartDate ??= new();
        set => formattedUiStartDate = value;
      }

      /// <summary>
      /// A value of FormattedUiEndDate.
      /// </summary>
      [JsonPropertyName("formattedUiEndDate")]
      public WorkArea FormattedUiEndDate
      {
        get => formattedUiEndDate ??= new();
        set => formattedUiEndDate = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 9;

      private FcrSvesTitleXvi guiType;
      private FcrSvesTitleXvi guiVerifCd;
      private WorkArea formattedUiStartDate;
      private WorkArea formattedUiEndDate;
    }

    /// <summary>A SvesTypeGroup group.</summary>
    [Serializable]
    public class SvesTypeGroup
    {
      /// <summary>
      /// A value of Gagency.
      /// </summary>
      [JsonPropertyName("gagency")]
      public FcrSvesGenInfo Gagency
      {
        get => gagency ??= new();
        set => gagency = value;
      }

      /// <summary>A collection capacity.</summary>
      public const int Capacity = 4;

      private FcrSvesGenInfo gagency;
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
    /// A value of FormattedReqDate.
    /// </summary>
    [JsonPropertyName("formattedReqDate")]
    public WorkArea FormattedReqDate
    {
      get => formattedReqDate ??= new();
      set => formattedReqDate = value;
    }

    /// <summary>
    /// A value of FormattedRespDate.
    /// </summary>
    [JsonPropertyName("formattedRespDate")]
    public WorkArea FormattedRespDate
    {
      get => formattedRespDate ??= new();
      set => formattedRespDate = value;
    }

    /// <summary>
    /// A value of FormattedPhone.
    /// </summary>
    [JsonPropertyName("formattedPhone")]
    public TextWorkArea FormattedPhone
    {
      get => formattedPhone ??= new();
      set => formattedPhone = value;
    }

    /// <summary>
    /// A value of FormattedDate.
    /// </summary>
    [JsonPropertyName("formattedDate")]
    public WorkArea FormattedDate
    {
      get => formattedDate ??= new();
      set => formattedDate = value;
    }

    /// <summary>
    /// A value of FormattedStartDate.
    /// </summary>
    [JsonPropertyName("formattedStartDate")]
    public WorkArea FormattedStartDate
    {
      get => formattedStartDate ??= new();
      set => formattedStartDate = value;
    }

    /// <summary>
    /// A value of FormattedEndDate.
    /// </summary>
    [JsonPropertyName("formattedEndDate")]
    public WorkArea FormattedEndDate
    {
      get => formattedEndDate ??= new();
      set => formattedEndDate = value;
    }

    /// <summary>
    /// Gets a value of Phist.
    /// </summary>
    [JsonIgnore]
    public Array<PhistGroup> Phist => phist ??= new(PhistGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Phist for json serialization.
    /// </summary>
    [JsonPropertyName("phist")]
    [Computed]
    public IList<PhistGroup> Phist_Json
    {
      get => phist;
      set => Phist.Assign(value);
    }

    /// <summary>
    /// Gets a value of Ui.
    /// </summary>
    [JsonIgnore]
    public Array<UiGroup> Ui => ui ??= new(UiGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of Ui for json serialization.
    /// </summary>
    [JsonPropertyName("ui")]
    [Computed]
    public IList<UiGroup> Ui_Json
    {
      get => ui;
      set => Ui.Assign(value);
    }

    /// <summary>
    /// A value of FormattedPayStatDate.
    /// </summary>
    [JsonPropertyName("formattedPayStatDate")]
    public WorkArea FormattedPayStatDate
    {
      get => formattedPayStatDate ??= new();
      set => formattedPayStatDate = value;
    }

    /// <summary>
    /// A value of FormattedAppealDate.
    /// </summary>
    [JsonPropertyName("formattedAppealDate")]
    public WorkArea FormattedAppealDate
    {
      get => formattedAppealDate ??= new();
      set => formattedAppealDate = value;
    }

    /// <summary>
    /// A value of FormattedRedeterDate.
    /// </summary>
    [JsonPropertyName("formattedRedeterDate")]
    public WorkArea FormattedRedeterDate
    {
      get => formattedRedeterDate ??= new();
      set => formattedRedeterDate = value;
    }

    /// <summary>
    /// A value of FormattedDenialDate.
    /// </summary>
    [JsonPropertyName("formattedDenialDate")]
    public WorkArea FormattedDenialDate
    {
      get => formattedDenialDate ??= new();
      set => formattedDenialDate = value;
    }

    /// <summary>
    /// A value of FormattedEligDate.
    /// </summary>
    [JsonPropertyName("formattedEligDate")]
    public WorkArea FormattedEligDate
    {
      get => formattedEligDate ??= new();
      set => formattedEligDate = value;
    }

    /// <summary>
    /// A value of FormattedEstabDate.
    /// </summary>
    [JsonPropertyName("formattedEstabDate")]
    public WorkArea FormattedEstabDate
    {
      get => formattedEstabDate ??= new();
      set => formattedEstabDate = value;
    }

    /// <summary>
    /// A value of SvesErrorMsgs.
    /// </summary>
    [JsonPropertyName("svesErrorMsgs")]
    public WorkArea SvesErrorMsgs
    {
      get => svesErrorMsgs ??= new();
      set => svesErrorMsgs = value;
    }

    /// <summary>
    /// Gets a value of SvesType.
    /// </summary>
    [JsonIgnore]
    public Array<SvesTypeGroup> SvesType => svesType ??= new(
      SvesTypeGroup.Capacity, 0);

    /// <summary>
    /// Gets a value of SvesType for json serialization.
    /// </summary>
    [JsonPropertyName("svesType")]
    [Computed]
    public IList<SvesTypeGroup> SvesType_Json
    {
      get => svesType;
      set => SvesType.Assign(value);
    }

    /// <summary>
    /// A value of FcrSvesGenInfo.
    /// </summary>
    [JsonPropertyName("fcrSvesGenInfo")]
    public FcrSvesGenInfo FcrSvesGenInfo
    {
      get => fcrSvesGenInfo ??= new();
      set => fcrSvesGenInfo = value;
    }

    /// <summary>
    /// A value of FcrSvesAddress.
    /// </summary>
    [JsonPropertyName("fcrSvesAddress")]
    public FcrSvesAddress FcrSvesAddress
    {
      get => fcrSvesAddress ??= new();
      set => fcrSvesAddress = value;
    }

    /// <summary>
    /// A value of FcrSvesTitleXvi.
    /// </summary>
    [JsonPropertyName("fcrSvesTitleXvi")]
    public FcrSvesTitleXvi FcrSvesTitleXvi
    {
      get => fcrSvesTitleXvi ??= new();
      set => fcrSvesTitleXvi = value;
    }

    private CsePersonsWorkSet csePersonsWorkSet;
    private WorkArea formattedReqDate;
    private WorkArea formattedRespDate;
    private TextWorkArea formattedPhone;
    private WorkArea formattedDate;
    private WorkArea formattedStartDate;
    private WorkArea formattedEndDate;
    private Array<PhistGroup> phist;
    private Array<UiGroup> ui;
    private WorkArea formattedPayStatDate;
    private WorkArea formattedAppealDate;
    private WorkArea formattedRedeterDate;
    private WorkArea formattedDenialDate;
    private WorkArea formattedEligDate;
    private WorkArea formattedEstabDate;
    private WorkArea svesErrorMsgs;
    private Array<SvesTypeGroup> svesType;
    private FcrSvesGenInfo fcrSvesGenInfo;
    private FcrSvesAddress fcrSvesAddress;
    private FcrSvesTitleXvi fcrSvesTitleXvi;
  }

  /// <summary>
  /// This class defines local view.
  /// </summary>
  [Serializable]
  public class Local
  {
    /// <summary>
    /// A value of ExitStateWorkArea.
    /// </summary>
    [JsonPropertyName("exitStateWorkArea")]
    public ExitStateWorkArea ExitStateWorkArea
    {
      get => exitStateWorkArea ??= new();
      set => exitStateWorkArea = value;
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
    /// A value of OnTheCase.
    /// </summary>
    [JsonPropertyName("onTheCase")]
    public Common OnTheCase
    {
      get => onTheCase ??= new();
      set => onTheCase = value;
    }

    /// <summary>
    /// A value of Supervisor.
    /// </summary>
    [JsonPropertyName("supervisor")]
    public Common Supervisor
    {
      get => supervisor ??= new();
      set => supervisor = value;
    }

    private ExitStateWorkArea exitStateWorkArea;
    private CsePerson csePerson;
    private Common onTheCase;
    private Common supervisor;
  }

  /// <summary>
  /// This class defines entity view.
  /// </summary>
  [Serializable]
  public class Entities
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

    private CsePerson csePerson;
  }
#endregion
}
